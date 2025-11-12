# フォント生成パイプライン

ゲーム内の全 UI で日本語（CJK）を正しく描画するために、フォント資産をサブセット化し、TextMeshPro / uGUI の両方へ適切に適用する手順をまとめる。

> **Contract (2025-11)**  
> - `FontManager` と `UITextSkin` が `TMP_Text` にフォントを適用する。`TMP_Text.SetText` のグローバルパッチは使わない。  
> - Sprite Asset / フォントフォールバックを `TMP_Settings` レベルで登録し、`<sprite>` と PUA グリフ（U+E000–F8FF）を欠けさせない。  
> - `Docs/font_pipeline.md` を含む全ドキュメントは UTF-8 (BOM 無し) で保存する。

## ゴール
- SIL OFL 1.1 のライセンスで再配布できる CJK フォント（例: Noto Sans CJK JP）をサブセット化し、Mod サイズを抑えつつ必要な文字をすべて含める。
- Harmony パッチで `TextMeshProUGUI` / `TextMeshPro` / `TMP_InputField` / `UnityEngine.UI.Text` にフォントとレイアウト設定をまとめて適用する。
- `<sprite>` タグと PUA グリフのどちらもフォント/スプライト資産に含め、ツール tip や HUD で Missing Character を出さない。

## サブセット定義
`Docs/glyphset.txt` に必要な Unicode Range を列挙する（ひらがな、カタカナ、漢字、記号、ルーン、PUA など）。ゲーム内で新しいコードポイントが必要になったらこのファイルへ追記する。

## サブセット化手順
1. 依存のインストール
   ```powershell
   py -m pip install --user fonttools brotli
   ```
2. 元フォント（例: `NotoSansCJKjp-Regular.otf`, `NotoSansCJKjp-Bold.otf`）を `Mods/QudJP/Fonts` へ配置。
3. サブセット化コマンド
   ```powershell
   py -m fontTools.subset Mods/QudJP/Fonts/NotoSansCJKjp-Regular.otf `
     --unicodes-file=Docs/glyphset.txt `
     --layout-features=* `
     --output-file=Mods/QudJP/Fonts/NotoSansCJKjp-Regular-Subset.otf

   py -m fontTools.subset Mods/QudJP/Fonts/NotoSansCJKjp-Bold.otf `
     --unicodes-file=Docs/glyphset.txt `
     --layout-features=* `
     --output-file=Mods/QudJP/Fonts/NotoSansCJKjp-Bold-Subset.otf
   ```
4. 生成された `*-Subset.otf` のみを配布し、元フォントはコミットしない（ライセンス条項を README / manifest / Workshop ページへ明記する）。

## FontManager の役割
1. **フォント読み込み** (`FontManager.TryLoadFonts`)
   - `TMP_FontAsset.CreateFontAsset` で Regular / Bold を生成し、`PrimaryFont`, `BoldFont` として保持。
   - `fontWeightTable` の差し替え、マテリアル共有、`extraPadding` など TMP の推奨設定を適用。
2. **TMP グローバル設定**
   - `TMP_Settings.defaultFontAsset` を Primary に差し替え、`TMP_Settings.fallbackFontAssets` の先頭へ Primary/Bold を挿入。
   - 既存の `TMP_FontAsset` へ `EnsureFallbackChain` を走らせ、`Resources.FindObjectsOfTypeAll<TMP_FontAsset>()` で再帰的にフォールバックを構成する。
3. **レガシー UI**
   - `UnityEngine.UI.Text` の `Font` を Primary の `sourceFontFile` へ差し替え、`TextMeshPro` を導入できていない UI も表示崩れを防ぐ。

## Sprite Asset 登録
- `RegisterSpriteAssets` が `TMP_Settings.defaultSpriteAsset` をチェックし、Qud の `TMP_SpriteAsset`（Stat アイコンや AV/DV が含まれるもの）を優先的に選択する。
- `_attachedSpriteAsset` と `_previousSpriteAsset` を保持して二重登録を防ぎつつ、`TMP_Settings.defaultSpriteAsset` を上書き。
- `EnsureSpriteFallbacks` で全 `TMP_SpriteAsset` の `fallbackSpriteAssets` リストにメイン Sprite Asset を挿入する。これにより `<sprite spriteAsset=>` を指定していなくても描画できる。

## QA チェック
1. メインメニュー / ステータスパネル / ログ / Tooltip / 会話ウィンドウで折り返し・行間が崩れていないか確認する。
2. `Player.log` に `Missing glyph in font asset` や `Sprite index out of range` が出ていないか `scripts/log_watch` で監視する。発生したら `Docs/glyphset.txt` にコードポイントを追加して再サブセット化。
3. Bold / Italic / Outline などのスタイル付きテキストが期待どおり描画されるか確認する。
4. `TMP_InputField`（検索バーや名前入力欄）でカーソル位置と折り返しが崩れていないか動作チェックする。

## よくある落とし穴
- フォントを CP932 など別のエンコーディングで上書きするとフォント名が文字化けする。`Set-Content -Encoding UTF8` を使い、`scripts/check_encoding.py` で検知する。
- Sprite Asset を差し替えずに `<sprite name=\"AV\"/>` を翻訳だけで残すと、TMP が描画できずに空文字になる。必ず `RegisterSpriteAssets` を通じて default/fallback を更新する。
- `TMP_Settings` の `defaultFontAsset` を変えただけでは既存オブジェクトに適用されない。`EnsureFallbackOnAllFontAssets` と Harmony パッチで `TMP_Text.Awake/OnEnable` をフックしておく。
