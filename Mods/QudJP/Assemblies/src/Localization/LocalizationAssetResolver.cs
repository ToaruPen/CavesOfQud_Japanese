using System;
using System.Collections.Generic;
using System.IO;
using XRL;

namespace QudJP.Localization
{
    /// <summary>
    /// Maps XmlDataHelper / ModManager requests to the jp-localized assets that live under Mods/QudJP/Localization.
    /// Also hides legacy attributes (Load/Replace) from XmlDataHelper's sanity checks so Player.log stays clean.
    /// </summary>
    internal static class LocalizationAssetResolver
    {
        private const string ModId = "QudJP";
        private const string LocalizationFolderName = "Localization";
        private const string LocalizationSuffix = ".jp";
        private const string Utf8EncodingName = "utf-8";
        internal const string Utf8PassthroughEncoding = "utf-8-qudjp";

        private static readonly Dictionary<string, string?> OverrideCache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> LegacyAttributeNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "Load",
            "Replace",
        };

        private static ModInfo? _cachedModInfo;

        public static bool TryInjectOverride(string fileName, Action<string, ModInfo> fileAction, bool recursive)
        {
            if (recursive || fileAction == null || string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            var modInfo = GetLocalizationMod();
            if (modInfo == null || !modInfo.Active)
            {
                return false;
            }

            if (!TryGetOverridePath(modInfo, fileName, out var overridePath))
            {
                return false;
            }

            try
            {
                fileAction(overridePath, modInfo);
                return true;
            }
            catch (Exception ex)
            {
                modInfo.Error(DataManager.SanitizePathForDisplay($"{modInfo.Path}/{fileName}: {ex}"));
                return false;
            }
        }

        public static void IgnoreLegacyAttributes(XmlDataHelper reader)
        {
            if (reader == null || reader.AttributeCount == 0)
            {
                return;
            }

            if (!IsLocalizationXml(reader))
            {
                return;
            }

            EnsureUtf8Passthrough(reader);

            foreach (var attribute in LegacyAttributeNames)
            {
                reader.GetAttribute(attribute);
            }
        }

        private static bool TryGetOverridePath(ModInfo modInfo, string fileName, out string path)
        {
            if (OverrideCache.TryGetValue(fileName, out var cached))
            {
                if (!string.IsNullOrEmpty(cached))
                {
                    path = cached!;
                    return true;
                }

                path = string.Empty;
                return false;
            }

            var candidate = BuildOverridePath(modInfo.Path, fileName);
            if (!string.IsNullOrEmpty(candidate) && File.Exists(candidate))
            {
                OverrideCache[fileName] = candidate;
                path = candidate!;
                return true;
            }

            OverrideCache[fileName] = null;
            path = string.Empty;
            return false;
        }

        private static string? BuildOverridePath(string modRoot, string fileName)
        {
            if (string.IsNullOrEmpty(modRoot))
            {
                return null;
            }

            var normalized = fileName.Replace('\\', '/').TrimStart('/');
            if (string.IsNullOrEmpty(normalized))
            {
                return null;
            }

            var extension = Path.GetExtension(normalized);
            if (!".xml".Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (normalized.EndsWith(".jp.xml", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var directory = Path.GetDirectoryName(normalized.Replace('/', Path.DirectorySeparatorChar));
            var baseName = Path.GetFileNameWithoutExtension(normalized);
            if (string.IsNullOrEmpty(baseName))
            {
                return null;
            }

            var localizedFileName = baseName + LocalizationSuffix + extension;
            var localizedRelative = string.IsNullOrEmpty(directory)
                ? localizedFileName
                : Path.Combine(directory, localizedFileName);

            return Path.Combine(modRoot, LocalizationFolderName, localizedRelative);
        }

        internal static bool IsLocalizationXml(XmlDataHelper reader)
        {
            var info = reader.modInfo;
            if (info == null || !string.Equals(info.ID, ModId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var uri = reader.BaseURI;
            if (string.IsNullOrEmpty(uri))
            {
                return false;
            }

            var normalized = uri.Replace('\\', '/');
            if (normalized.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring("file://".Length);
            }

            return normalized.IndexOf("/localization/", StringComparison.OrdinalIgnoreCase) >= 0
                || normalized.EndsWith(".jp.xml", StringComparison.OrdinalIgnoreCase);
        }

        public static void EnsureUtf8Passthrough(XmlDataHelper reader)
        {
            if (reader == null)
            {
                return;
            }

            if (!IsLocalizationXml(reader))
            {
                return;
            }

            if (string.Equals(reader.StringEncoding, Utf8EncodingName, StringComparison.OrdinalIgnoreCase))
            {
                reader.StringEncoding = Utf8PassthroughEncoding;
            }
        }

        private static ModInfo? GetLocalizationMod()
        {
            if (_cachedModInfo != null)
            {
                return _cachedModInfo;
            }

            if (ModManager.ModMap.TryGetValue(ModId, out var info))
            {
                _cachedModInfo = info;
                return info;
            }

            foreach (var mod in ModManager.Mods)
            {
                if (string.Equals(mod.ID, ModId, StringComparison.OrdinalIgnoreCase))
                {
                    _cachedModInfo = mod;
                    return mod;
                }
            }

            return null;
        }
    }
}
