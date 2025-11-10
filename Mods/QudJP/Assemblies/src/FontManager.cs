using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace QudJP
{
    /// <summary>
    /// サブセット化した OTF から TextMeshPro / uGUI 両対応のフォントを動的生成し、UI へ適用するマネージャ。
    /// </summary>
    public sealed class FontManager : IDisposable
    {
        public static FontManager Instance { get; } = new FontManager();

        private const string RegularFontFile = "NotoSansCJKjp-Regular-Subset.otf";
        private const string BoldFontFile = "NotoSansCJKjp-Bold-Subset.otf";

        private static readonly HashSet<string> VanillaFontHints = new(StringComparer.OrdinalIgnoreCase)
        {
            "LiberationSans",
            "Liberation Sans",
            "LiberationSans SDF",
            "Liberation Sans SDF"
        };

        private static readonly FieldInfo? FontWeightTableField =
            AccessTools.Field(typeof(TMP_FontAsset), "m_FontWeightTable");

        private readonly List<UnityEngine.Object> _loadedAssets = new();
        private bool _loaded;

        public TMP_FontAsset? PrimaryFont { get; private set; }
        public TMP_FontAsset? BoldFont { get; private set; }
        public Font? LegacyFont => PrimaryFont?.sourceFontFile;

        public void TryLoadFonts()
        {
            if (_loaded)
            {
                return;
            }

            try
            {
                var fontsDir = Path.Combine(ModPathResolver.ResolveModPath(), "Fonts");
                PrimaryFont = LoadFontAsset(fontsDir, RegularFontFile);
                if (PrimaryFont == null)
                {
                    Debug.LogWarning("[QudJP] 日本語フォント (Regular) が見つからないため差し替えをスキップします。");
                    return;
                }

                BoldFont = LoadFontAsset(fontsDir, BoldFontFile) ?? PrimaryFont;

                ConfigureFontWeights();
                RegisterTmpSettings();

                _loaded = true;
                Debug.Log($"[QudJP] TMP フォントを初期化しました (Regular={PrimaryFont.name}, Bold={BoldFont.name}).");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[QudJP] フォント初期化に失敗しました: {ex}");
                Dispose();
            }
        }

        public void ApplyToText(TMP_Text? text, bool forceReplace = false)
        {
            if (!_loaded || text == null || PrimaryFont == null)
            {
                return;
            }

            if (forceReplace)
            {
                text.font = PrimaryFont;
                text.fontSharedMaterial = PrimaryFont.material;
            }
            else
            {
                if (text.font == null)
                {
                    text.font = PrimaryFont;
                    text.fontSharedMaterial = PrimaryFont.material;
                }

                EnsureFallbackChain(text.font);
            }

            text.extraPadding = true;
            text.wordWrappingRatios = Mathf.Clamp(text.wordWrappingRatios, 0.35f, 1f);
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Truncate;
        }

        public void ApplyToInputField(TMP_InputField? inputField)
        {
            if (inputField == null)
            {
                return;
            }

            ApplyToText(inputField.textComponent);
            if (inputField.placeholder is TMP_Text placeholder)
            {
                ApplyToText(placeholder);
            }
        }

        public void ApplyToLegacyText(Text? text)
        {
            if (!_loaded || text == null)
            {
                return;
            }

            var legacyFont = LegacyFont;
            if (legacyFont == null)
            {
                return;
            }

            // Avoid perturbing layout for ASCII-only labels; keep vanilla fonts/metrics.
            var s = text.text;
            bool needsJP = false;
            if (!string.IsNullOrEmpty(s))
            {
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] > 0x7F)
                    {
                        needsJP = true;
                        break;
                    }
                }
            }
            if (!needsJP)
            {
                return;
            }

            if (ShouldReplaceLegacyFont(text.font))
            {
                text.font = legacyFont;
                text.material = legacyFont.material;
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
            }
        }

        public void Dispose()
        {
            TMP_Settings.defaultFontAsset = null;
            var fallback = TMP_Settings.fallbackFontAssets;
            fallback?.RemoveAll(asset => asset != null && (asset == PrimaryFont || asset == BoldFont));
            TMP_Settings.fallbackFontAssets = fallback;

            foreach (var asset in _loadedAssets)
            {
                if (asset != null)
                {
                    Resources.UnloadAsset(asset);
                }
            }

            _loadedAssets.Clear();
            PrimaryFont = null;
            BoldFont = null;
            _loaded = false;
        }

        private TMP_FontAsset? LoadFontAsset(string fontsDir, string fileName)
        {
            var path = Path.Combine(fontsDir, fileName);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[QudJP] フォントファイルが見つかりません: {fileName}");
                return null;
            }

            try
            {
                var fontAsset = TMP_FontAsset.CreateFontAsset(path, 0, 96, 6, GlyphRenderMode.SDFAA, 4096, 4096);
                fontAsset.name = Path.GetFileNameWithoutExtension(fileName);
                fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                fontAsset.isMultiAtlasTexturesEnabled = true;
                fontAsset.fallbackFontAssetTable?.Clear();

                if (fontAsset.sourceFontFile != null)
                {
                    _loadedAssets.Add(fontAsset.sourceFontFile);
                }

                _loadedAssets.Add(fontAsset);
                return fontAsset;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[QudJP] {fileName} から TMP Font Asset を生成できませんでした: {ex}");
                return null;
            }
        }

        private void ConfigureFontWeights()
        {
            if (PrimaryFont == null)
            {
                return;
            }

            var table = EnsureFontWeightTable(PrimaryFont);

            var boldAsset = BoldFont ?? PrimaryFont;

            void Assign(FontWeight weight, TMP_FontAsset asset)
            {
                var index = (int)weight;
                if (index >= table.Length)
                {
                    Array.Resize(ref table, index + 1);
                }

                var pair = table[index];
                pair.regularTypeface = asset;
                pair.italicTypeface = asset;
                table[index] = pair;
            }

            Assign(FontWeight.Thin, PrimaryFont);
            Assign(FontWeight.Light, PrimaryFont);
            Assign(FontWeight.Regular, PrimaryFont);
            Assign(FontWeight.Medium, PrimaryFont);
            Assign(FontWeight.SemiBold, boldAsset);
            Assign(FontWeight.Bold, boldAsset);

        }

        private TMP_FontWeightPair[] EnsureFontWeightTable(TMP_FontAsset asset)
        {
            var table = asset.fontWeightTable;
            if (table == null || table.Length == 0)
            {
                table = new TMP_FontWeightPair[10];
                FontWeightTableField?.SetValue(asset, table);
            }

            return table;
        }

        private void RegisterTmpSettings()
        {
            if (PrimaryFont == null)
            {
                return;
            }

            var fallback = TMP_Settings.fallbackFontAssets ?? new List<TMP_FontAsset>();
            fallback.RemoveAll(asset => asset == null || asset == PrimaryFont || asset == BoldFont);
            fallback.Insert(0, PrimaryFont);
            if (BoldFont != null && BoldFont != PrimaryFont)
            {
                fallback.Insert(1, BoldFont);
            }

            TMP_Settings.fallbackFontAssets = fallback;

            if (TMP_Settings.defaultFontAsset == null || TMP_Settings.defaultFontAsset == PrimaryFont)
            {
                TMP_Settings.defaultFontAsset = PrimaryFont;
            }
            else
            {
                EnsureFallbackChain(TMP_Settings.defaultFontAsset);
            }

            EnsureFallbackOnAllFontAssets();
        }

        private void EnsureFallbackChain(TMP_FontAsset? font)
        {
            if (font == null || PrimaryFont == null)
            {
                return;
            }

            var fallback = font.fallbackFontAssetTable ??= new List<TMP_FontAsset>();
            if (font != PrimaryFont && !fallback.Contains(PrimaryFont))
            {
                fallback.Insert(0, PrimaryFont);
            }

            if (BoldFont != null && BoldFont != PrimaryFont && !fallback.Contains(BoldFont))
            {
                fallback.Add(BoldFont);
            }
        }

        private void EnsureFallbackOnAllFontAssets()
        {
            if (PrimaryFont == null)
            {
                return;
            }

            var assets = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            foreach (var asset in assets)
            {
                EnsureFallbackChain(asset);
            }
        }

        private static bool ShouldReplaceLegacyFont(Font? font)
        {
            if (font == null)
            {
                return true;
            }

            foreach (var hint in VanillaFontHints)
            {
                if (font.name.IndexOf(hint, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
