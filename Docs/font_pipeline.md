# フォント生成パイプライン

ゲーム内の全 UI で日本語を破綻なく表示するために、CJK フォントをサブセット化し、TextMeshPro／uGUI 両対応のフォントを動的に生成する手順をまとめる。

## ゴール
- SIL OFL ライセンスの日本語フォントを最小限のグリフでサブセット化し、Mod 配布サイズを抑える。
- `Mods/QudJP/Fonts` にサブセット化した OTF を配置し、Harmony から `TMP_FontAsset.CreateFontAsset` でランタイム生成する。
- Harmony パッチで `TextMeshProUGUI` / `TextMeshPro` / `TMP_InputField` / `UnityEngine.UI.Text` にフォントとレイアウト設定を適用する。

## 推奨フォント
- [Noto Sans CJK JP](https://github.com/notofonts/noto-cjk)（Google / Adobe, SIL OFL 1.1）
- Source Han Sans JP（上記と同系フォント）

どちらも再配布可能だが、README / manifest / Workshop ページ等にライセンス表記を忘れないこと。

## 文字セット定義
`Docs/glyphset.txt` に必要な Unicode Range を列挙する。漢字 / ひらがな / カタカナ / 記号 / ルーンなど、ゲーム上で追加で必要になったブロックはここへ追記する。

## フォントのサブセット化
1. 依存インストール  
   ```powershell
   py -m pip install --user fonttools brotli
   ```
2. フォント（例: `NotoSansCJKjp-Regular.otf`）を `Mods/QudJP/Fonts` に配置する。
3. サブセット化コマンド（Regular/Bold をそれぞれ実行）  
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
4. 生成された OTF が 1 本あたり 12MB 前後。Mod 配布には `*-Subset.otf` のみを含める。

## TMP Font Asset の扱い
- Unity エディタで `.asset` を事前生成するのではなく、`FontManager` が `TMP_FontAsset.CreateFontAsset(path, …)` を呼び出してサブセット OTF から SDF をランタイム生成する。
- Atlas 設定（4096x4096, GlyphRenderMode=SDFAA, Padding=6）や `fontWeightTable` の差し替えは `FontManager` 側で自動化済み。
- 既存のフォントを残したままでも、`TMP_Settings.defaultFontAsset` と `fallbackFontAssets` に Noto を登録することで Missing Glyph を防げる。
- UnityEditor が必要になるケースは「マテリアル調整やベイク結果を確認したいとき」のみ。

## Harmony 実装メモ
- `FontManager.TryLoadFonts` が Mod 起動時に Regular/Bold を読み込み、`TMP_Settings` と `UnityEngine.UI.Text` (legacy) 向けフォントを初期化する。
- `TextMeshProPatches` で `TMP_Text.Awake/OnEnable` と `TMP_InputField.OnEnable` をフックし、フォント / 行間 / 禁則パラメータを一括適用。
- `UnityUITextPatch` で `UnityEngine.UI.Text.OnEnable` をフックし、レガシー UI の `Font` も差し替える。
- 既存フォントを強制的に上書きしたくない場合は `LiberationSans` など既知のバニラフォントだけを置換対象にしている。

## QA チェックリスト
- メインメニュー、ステータスパネル、ログ、ツールチップ、会話ウィンドウで折り返し／行間が崩れていないか確認。
- `Player.log` に `Missing glyph in font asset` が出ていないか監視。出た場合は `Docs/glyphset.txt` にコードポイントを追記し、再サブセット化する。
- Bold / Italic / Outline などスタイル付きテキストが期待通りに描画されるか確認。
- 長文入力フィールド（検索やノート）で `TMP_InputField` のカーソル／折り返しが崩れないか確認。
