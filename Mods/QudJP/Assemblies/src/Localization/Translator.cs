using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;
using UnityEngine;
using QudJP.Diagnostics;

namespace QudJP.Localization
{
    /// <summary>
    /// JSON 辞書を読み込み、UI 表示直前に日本語訳へ置換するサービス。
    /// 言語単位でロードし、ファイル変更を監視してホットリロードする。
    /// </summary>
    public sealed class Translator : IDisposable
    {
        public static Translator Instance { get; } = new Translator();

        private readonly object _gate = new();
        private readonly DataContractJsonSerializer _serializer =
            new(typeof(TranslationFile), new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = true,
            });

        private TranslationSnapshot _snapshot = TranslationSnapshot.Empty;
        private FileSystemWatcher? _watcher;
        private Timer? _reloadTimer;
        private bool _initialized;
        private string _language = "ja";

        private string DictionariesDirectory =>
            Path.Combine(ModPathResolver.ResolveModPath(), "Localization", "Dictionaries");

        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            Directory.CreateDirectory(DictionariesDirectory);
            LoadDictionaries();
            TryStartWatcher();
        }

        public void Dispose()
        {
            lock (_gate)
            {
                _watcher?.Dispose();
                _watcher = null;
                _reloadTimer?.Dispose();
                _reloadTimer = null;
            }
        }

        public void SetLanguage(string language)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                throw new ArgumentException("Language must be specified.", nameof(language));
            }

            if (string.Equals(_language, language, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _language = language.Trim();
            LoadDictionaries();
        }

        public string Apply(string? text, string? contextId = null)
        {
            var original = text ?? string.Empty;
            if (original.Length == 0)
            {
                return string.Empty;
            }

            var snapshot = _snapshot;
            var normalized = NormalizeKey(original);

            var eid = UIContext.Current;
            var hit = false;
            var result = original;

            if (TryTranslate(normalized, contextId, snapshot, out var translated))
            {
                result = translated;
                hit = true;
            }
            var delimiter = DetectDelimiter(original);
            if (!string.IsNullOrEmpty(delimiter))
            {
                var segmented = TranslateSegments(original, delimiter!, contextId, snapshot);
                if (segmented != null)
                {
                    result = segmented;
                    hit = true;
                }
            }

            var fallback = TryTranslateLabelFallback(original, contextId, snapshot);
            if (!hit && fallback != null)
            {
                result = fallback;
                hit = true;
            }

            result = string.IsNullOrEmpty(result) ? original : result;

            if (JpLog.Enabled)
            {
                var ctx = contextId ?? "-";
                JpLog.Info(eid, "TR", hit ? "HIT" : "MISS", $"ctx={ctx} keyLen={normalized.Length} outLen={result.Length}");
            }

            return result;
        }

        private static bool TryTranslate(string normalized, string? contextId, TranslationSnapshot snapshot, out string translated)
        {
            translated = default!;

            if (!string.IsNullOrEmpty(contextId))
            {
                var contextKey = contextId!;
                if (snapshot.Contextual.TryGetValue(contextKey, out var contextMap) &&
                    contextMap.TryGetValue(normalized, out var contextValue) &&
                    !string.IsNullOrEmpty(contextValue))
                {
                    translated = contextValue;
                    return true;
                }
            }

            if (snapshot.Global.TryGetValue(normalized, out var value) &&
                !string.IsNullOrEmpty(value))
            {
                translated = value;
                return true;
            }

            return false;
        }

        private static string? TryTranslateLabelFallback(string original, string? contextId, TranslationSnapshot snapshot)
        {
            var colonIndex = original.IndexOf(':');
            if (colonIndex <= 0)
            {
                return null;
            }

            var label = original.Substring(0, colonIndex);
            var normalizedLabel = NormalizeKey(label);

            if (TryTranslate(normalizedLabel, contextId, snapshot, out var translatedLabel))
            {
                return translatedLabel + original.Substring(colonIndex);
            }

            return null;
        }

        private void LoadDictionaries()
        {
            try
            {
                var directory = DictionariesDirectory;
                if (!Directory.Exists(directory))
                {
                    _snapshot = TranslationSnapshot.Empty;
                    Debug.LogWarning($"[QudJP] Dictionary directory not found: {directory}");
                    return;
                }

                var files = Directory.GetFiles(directory, "*.json", SearchOption.AllDirectories);
                var global = new Dictionary<string, string>(StringComparer.Ordinal);
                var contextual = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
                var loaded = 0;

                foreach (var file in files)
                {
                    var document = Deserialize(file);
                    if (document == null)
                    {
                        continue;
                    }

                    var lang = document.Meta?.Language ?? _language;
                    if (!LanguageMatches(lang))
                    {
                        continue;
                    }

                    if (document.Entries == null || document.Entries.Count == 0)
                    {
                        continue;
                    }

                    loaded++;
                    foreach (var entry in document.Entries)
                    {
                        var keyRaw = entry?.Key;
                        var valueRaw = entry?.Text;
                        if (string.IsNullOrEmpty(keyRaw) || string.IsNullOrEmpty(valueRaw))
                        {
                            continue;
                        }

                        var key = NormalizeKey(keyRaw!);
                        var value = valueRaw!;
                        var contextHint = entry?.Context;

                        if (!string.IsNullOrEmpty(contextHint))
                        {
                            var contextKey = contextHint!;
                            if (!contextual.TryGetValue(contextKey, out var map))
                            {
                                map = new Dictionary<string, string>(StringComparer.Ordinal);
                                contextual[contextKey] = map;
                            }

                            map[key] = value;
                        }
                        else
                        {
                            global[key] = value;
                        }
                    }
                }

                _snapshot = new TranslationSnapshot(global, contextual);
                Debug.Log($"[QudJP] Translator loaded {_snapshot.EntryCount} entries from {loaded} dictionaries (lang='{_language}').");
                try
                {
                    var sample = Apply("Continue", "QudMenuItem");
                    Debug.Log($"[QudJP][Diag] Sample translation: Continue -> '{sample}'");
                }
                catch { }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[QudJP] Failed to load dictionaries: {ex}");
            }
        }

        private TranslationFile? Deserialize(string path)
        {
            // Robust UTF-8 parse path first (avoids any intermediate codepage surprises),
            // then fall back to raw BOM-handling stream if needed.
            try
            {
                var json = File.ReadAllText(path, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
                using var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
                return _serializer.ReadObject(ms) as TranslationFile;
            }
            catch (Exception ex)
            {
                // Last resort - original DataContract path reading raw bytes (with BOM handling)
                try
                {
                    using var file = File.OpenRead(path);
                    using var ms = new MemoryStream();
                    int b1 = file.ReadByte();
                    int b2 = file.ReadByte();
                    int b3 = file.ReadByte();
                    bool hasUtf8Bom = b1 == 0xEF && b2 == 0xBB && b3 == 0xBF;
                    if (!hasUtf8Bom)
                    {
                        if (b1 != -1) ms.WriteByte((byte)b1);
                        if (b2 != -1) ms.WriteByte((byte)b2);
                        if (b3 != -1) ms.WriteByte((byte)b3);
                    }
                    file.CopyTo(ms);
                    ms.Position = 0;
                    return _serializer.ReadObject(ms) as TranslationFile;
                }
                catch (Exception ex2)
                {
                    Debug.LogWarning($"[QudJP] Failed to parse dictionary '{path}': {ex.Message}; fallback: {ex2.Message}");
                    return null;
                }
            }
        }

        private void TryStartWatcher()
        {
            try
            {
                var directory = DictionariesDirectory;
                if (!Directory.Exists(directory))
                {
                    return;
                }

                var watcher = new FileSystemWatcher(directory, "*.json")
                {
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
                };

                watcher.Changed += OnDictionaryChanged;
                watcher.Created += OnDictionaryChanged;
                watcher.Deleted += OnDictionaryChanged;
                watcher.Renamed += OnDictionaryChanged;
                watcher.EnableRaisingEvents = true;

                lock (_gate)
                {
                    _watcher?.Dispose();
                    _watcher = watcher;
                }

                Debug.Log("[QudJP] Translator watcher started.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[QudJP] Failed to start dictionary watcher: {ex.Message}");
            }
        }

        private void OnDictionaryChanged(object sender, FileSystemEventArgs e)
        {
            ScheduleReload();
        }

        private void ScheduleReload()
        {
            lock (_gate)
            {
                _reloadTimer ??= new Timer(_ => ReloadFromWatcher(), null, Timeout.Infinite, Timeout.Infinite);
                _reloadTimer.Change(TimeSpan.FromMilliseconds(500), Timeout.InfiniteTimeSpan);
            }
        }

        private void ReloadFromWatcher()
        {
            try
            {
                LoadDictionaries();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[QudJP] Dictionary reload skipped: {ex.Message}");
            }
        }

        private bool LanguageMatches(string language)
        {
            return string.Equals(language, _language, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeKey(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Replace("\r\n", "\n").Replace("\r", "\n");
        }

        private static string? DetectDelimiter(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (value.IndexOf('%') >= 0)
            {
                return "%";
            }

            return null;
        }

        private static string? TranslateSegments(string original, string delimiter, string? contextId, TranslationSnapshot snapshot)
        {
            var parts = original.Split(new[] { delimiter }, StringSplitOptions.None);
            var changed = false;
            for (int i = 0; i < parts.Length; i++)
            {
                var normalized = NormalizeKey(parts[i]);
                if (TryTranslate(normalized, contextId, snapshot, out var translated))
                {
                    parts[i] = translated;
                    changed = true;
                    continue;
                }

                var trimmed = normalized.Trim();
                if (trimmed.Length > 0 && TryTranslate(trimmed, contextId, snapshot, out translated))
                {
                    parts[i] = translated;
                    changed = true;
                }
            }

            if (!changed)
            {
                return null;
            }

            return string.Join(delimiter, parts);
        }

        [DataContract]
        private sealed class TranslationFile
        {
            [DataMember(Name = "meta")]
            public TranslationMeta? Meta { get; set; }

            [DataMember(Name = "rules")]
            public TranslationRules? Rules { get; set; }

            [DataMember(Name = "entries")]
            public List<TranslationEntry>? Entries { get; set; }
        }

        [DataContract]
        private sealed class TranslationMeta
        {
            [DataMember(Name = "lang")]
            public string? Language { get; set; }

            [DataMember(Name = "id")]
            public string? Id { get; set; }

            [DataMember(Name = "version")]
            public string? Version { get; set; }
        }

        [DataContract]
        private sealed class TranslationRules
        {
            [DataMember(Name = "protectColorTags")]
            public bool ProtectColorTags { get; set; }

            [DataMember(Name = "protectHtmlEntities")]
            public bool ProtectHtmlEntities { get; set; }
        }

        [DataContract]
        private sealed class TranslationEntry
        {
            [DataMember(Name = "key")]
            public string? Key { get; set; }

            [DataMember(Name = "text")]
            public string? Text { get; set; }

            [DataMember(Name = "context")]
            public string? Context { get; set; }
        }

        private readonly struct TranslationSnapshot
        {
            public static TranslationSnapshot Empty { get; } = new(
                new Dictionary<string, string>(StringComparer.Ordinal),
                new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal));

            public TranslationSnapshot(
                Dictionary<string, string> global,
                Dictionary<string, Dictionary<string, string>> contextual)
            {
                Global = global;
                Contextual = contextual;
            }

            public Dictionary<string, string> Global { get; }

            public Dictionary<string, Dictionary<string, string>> Contextual { get; }

            public int EntryCount
            {
                get
                {
                    var total = Global.Count;
                    foreach (var map in Contextual.Values)
                    {
                        total += map.Count;
                    }

                    return total;
                }
            }
        }
    }
}
