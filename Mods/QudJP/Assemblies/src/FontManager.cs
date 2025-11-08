using System;
using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace QudJP
{
    /// <summary>
    /// フォントアセットのロードと破棄を管理する簡易クラス。
    /// 現時点ではプレースホルダー。後で実際の TMP Font Asset 読み込みを実装する。
    /// </summary>
    public sealed class FontManager : IDisposable
    {
        public static FontManager Instance { get; } = new FontManager();

        private bool _loaded;
        private readonly List<TMP_FontAsset> _loadedAssets = new List<TMP_FontAsset>();

        public TMP_FontAsset? PrimaryFont { get; private set; }

        public void TryLoadFonts()
        {
            if (_loaded)
            {
                return;
            }

            var modDir = ModPathResolver.ResolveModPath();
            var fontPath = Path.Combine(modDir, "Fonts", "QudJP-Regular SDF.asset");
            if (!File.Exists(fontPath))
            {
                Debug.LogWarning("[QudJP] TMP Font Asset が見つかりません。Fonts ディレクトリを確認してください。");
                return;
            }

            // TODO: AssetBundle 化された TMP フォントを読む処理を実装
            _loaded = true;
        }

        public void Dispose()
        {
            foreach (var asset in _loadedAssets)
            {
                if (asset != null)
                {
                    Resources.UnloadAsset(asset);
                }
            }

            _loadedAssets.Clear();
            PrimaryFont = null;
            _loaded = false;
        }
    }
}
