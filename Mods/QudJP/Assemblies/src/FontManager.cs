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
        private readonly List<TMP_FontAsset> _symbolFallbacks = new();
        private bool _loaded;
        private TMP_FontAsset? _previousDefaultFont;
        private TMP_SpriteAsset? _attachedSpriteAsset;
        private TMP_SpriteAsset? _previousSpriteAsset;

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

            var needsReplace = forceReplace || text.font == null || text.font == _previousDefaultFont || !ReferenceEquals(text.font, PrimaryFont);
            if (needsReplace)
            {
                text.font = PrimaryFont;
                text.fontSharedMaterial = PrimaryFont.material;
            }

            EnsureFallbackChain(text.font);

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
            TMP_Settings.defaultFontAsset = _previousDefaultFont;
            var fallback = TMP_Settings.fallbackFontAssets;
            fallback?.RemoveAll(asset => asset != null && (asset == PrimaryFont || asset == BoldFont));
            TMP_Settings.fallbackFontAssets = fallback;

            if (_attachedSpriteAsset != null && TMP_Settings.defaultSpriteAsset == _attachedSpriteAsset)
            {
                TMP_Settings.defaultSpriteAsset = _previousSpriteAsset;
            }

            _symbolFallbacks.Clear();
            _previousDefaultFont = null;
            _attachedSpriteAsset = null;
            _previousSpriteAsset = null;

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

            CaptureSymbolFallback(TMP_Settings.defaultFontAsset);

            var fallback = TMP_Settings.fallbackFontAssets ?? new List<TMP_FontAsset>();
            fallback.RemoveAll(asset => asset == null);
            InsertFallbackFont(fallback, PrimaryFont, preferFront: true);
            if (BoldFont != null)
            {
                InsertFallbackFont(fallback, BoldFont, preferFront: false);
            }

            foreach (var symbol in _symbolFallbacks)
            {
                InsertFallbackFont(fallback, symbol, preferFront: false);
            }

            TMP_Settings.fallbackFontAssets = fallback;

            if (TMP_Settings.defaultFontAsset == null || TMP_Settings.defaultFontAsset == PrimaryFont)
            {
                TMP_Settings.defaultFontAsset = PrimaryFont;
            }
            else
            {
                if (_previousDefaultFont == null && TMP_Settings.defaultFontAsset != PrimaryFont)
                {
                    _previousDefaultFont = TMP_Settings.defaultFontAsset;
                }

                EnsureFallbackChain(TMP_Settings.defaultFontAsset);
            }

            EnsureFallbackOnAllFontAssets();
            RegisterSpriteAssets();
        }

        private void EnsureFallbackChain(TMP_FontAsset? font)
        {
            if (font == null || PrimaryFont == null)
            {
                return;
            }

            var fallback = font.fallbackFontAssetTable ??= new List<TMP_FontAsset>();
            InsertFallbackFont(fallback, PrimaryFont, preferFront: true, owner: font);
            if (BoldFont != null)
            {
                InsertFallbackFont(fallback, BoldFont, preferFront: false, owner: font);
            }

            foreach (var symbol in _symbolFallbacks)
            {
                InsertFallbackFont(fallback, symbol, preferFront: false, owner: font);
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

        private void CaptureSymbolFallback(TMP_FontAsset? candidate)
        {
            _symbolFallbacks.Clear();
            if (candidate == null || candidate == PrimaryFont || candidate == BoldFont)
            {
                return;
            }

            _symbolFallbacks.Add(candidate);
        }

        private static void InsertFallbackFont(List<TMP_FontAsset> list, TMP_FontAsset? font, bool preferFront, TMP_FontAsset? owner = null)
        {
            if (font == null)
            {
                return;
            }

            if (owner != null && ReferenceEquals(owner, font))
            {
                return;
            }

            list.RemoveAll(asset => asset == null || ReferenceEquals(asset, font));
            if (preferFront)
            {
                list.Insert(0, font);
            }
            else
            {
                list.Add(font);
            }
        }

        private void RegisterSpriteAssets()
        {
            var spriteAsset = ResolveSpriteAsset();
            if (spriteAsset == null)
            {
                Debug.LogWarning("[QudJP] TMP sprite asset が見つからないため、<sprite> を描画できない場合があります。");
                return;
            }

            if (_attachedSpriteAsset != spriteAsset)
            {
                if (_previousSpriteAsset == null)
                {
                    _previousSpriteAsset = TMP_Settings.defaultSpriteAsset;
                }

                TMP_Settings.defaultSpriteAsset = spriteAsset;
                _attachedSpriteAsset = spriteAsset;
            }

            EnsureSpriteFallbacks(spriteAsset);
        }

        private void EnsureSpriteFallbacks(TMP_SpriteAsset spriteAsset)
        {
            var assets = Resources.FindObjectsOfTypeAll<TMP_SpriteAsset>();
            foreach (var candidate in assets)
            {
                if (candidate == null || candidate == spriteAsset)
                {
                    continue;
                }

                var fallback = candidate.fallbackSpriteAssets ??= new List<TMP_SpriteAsset>();
                if (!fallback.Contains(spriteAsset))
                {
                    fallback.Insert(0, spriteAsset);
                }
            }
        }


        private TMP_SpriteAsset? ResolveSpriteAsset()
        {
            if (IsValidSpriteAsset(TMP_Settings.defaultSpriteAsset))
            {
                return TMP_Settings.defaultSpriteAsset;
            }

            TMP_SpriteAsset? preferred = null;
            var assets = Resources.FindObjectsOfTypeAll<TMP_SpriteAsset>();
            foreach (var asset in assets)
            {
                if (!IsValidSpriteAsset(asset))
                {
                    continue;
                }

                if (LooksLikeQudSpriteAsset(asset))
                {
                    return asset;
                }

                if (preferred == null || CountSpriteCharacters(asset) > CountSpriteCharacters(preferred))
                {
                    preferred = asset;
                }
            }

            return preferred;
        }

        private static bool IsValidSpriteAsset(TMP_SpriteAsset? asset) =>
            asset != null &&
            asset.spriteCharacterTable != null &&
            asset.spriteCharacterTable.Count > 0;

        private static int CountSpriteCharacters(TMP_SpriteAsset? asset) =>
            asset?.spriteCharacterTable?.Count ?? 0;

        private static bool LooksLikeQudSpriteAsset(TMP_SpriteAsset asset)
        {
            if (!IsValidSpriteAsset(asset))
            {
                return false;
            }

            var name = asset.name ?? string.Empty;
            if (name.IndexOf("Qud", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("Icon", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("Stat", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            foreach (var sprite in asset.spriteCharacterTable)
            {
                var spriteName = sprite?.name;
                if (string.IsNullOrEmpty(spriteName))
                {
                    continue;
                }

                if (spriteName.Equals("AV", StringComparison.OrdinalIgnoreCase) ||
                    spriteName.Equals("DV", StringComparison.OrdinalIgnoreCase) ||
                    spriteName.IndexOf("Wound", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    spriteName.IndexOf("Stat", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
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
