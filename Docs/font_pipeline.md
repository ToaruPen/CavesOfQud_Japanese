# フォント資産パイプライン

## 目的
日本語（CJK）フォントをサブセット化し、Unity(TextMeshPro) 互換のフォントアセットを生成して Mod に同梱する。

## 推奨フォント
- Noto Sans CJK JP
- 源ノ角ゴシック (Source Han Sans JP)

上記はいずれも SIL OFL 1.1 なので再配布可。ただし README / manifest / workshop ページにライセンス表記を記載すること。

## サブセット化
1. `pip install fonttools brotli`
2. 対応文字セットを `Docs/glyphset.txt` に列挙（基本漢字 + カタカナ + ひらがな + 記号 + ルーン等必要に応じて追加）。
3. コマンド例:
   ```bash
   pyftsubset "NotoSansCJKjp-Regular.otf" ^
     --unicodes-file=Docs/glyphset.txt ^
     --layout-features='*' ^
     --output-file Mods/QudJP/Fonts/NotoSansCJKjp-Subset.otf ^
     --flavor=otf --with-zopfli
   ```
4. 可能であればウェイト別（Regular/Bold）を用意。サイズ上限は 50MB 未満を目標。

## TMP Font Asset 作成
1. Unity エディタ (ゲーム本体と同じ Unity バージョン) で空のプロジェクトを作る。
2. TextMeshPro を導入し、Font Asset Creator で以下の設定を推奨:
   - Atlas Resolution: 4096x4096
   - Render Mode: SDF16
   - Padding: 5
   - Character Set: Custom（glyphset.txt を入力）
3. 生成した `.asset` と `.mat` を `Mods/QudJP/Fonts` にコピーし、ファイル名と GUID を記録 (後で Harmony からロード)。

## Harmony 実装の勘所
- 起動時に Font Asset を `Resources.Load` ではなく `AssetBundle.LoadFromFile` で読む方法もあるが、Mod ディレクトリから `UnityModManagerNet.UnityModManager.FindMod` 等で直接参照する簡易実装から始める。
- `TextMeshProUGUI` / `TextMeshPro` / `TMP_InputField` / `TextMeshProFallbackSettings` を網羅的に差し替え、`lineSpacing`, `wordWrappingRatios`, `enableKerning` を適宜調整。
- 漢字の禁則処理は Unity 標準では弱いので、最低限 `textWrappingMode = Truncated` の箇所を `PreferredWidth` 評価に置き換える。

## QA チェック
- メインメニュー、ログ、ツールチップ、インベントリ、ジャーナル、会話ウィンドウで折り返しと行間を確認。
- `Player.log` で Missing Glyph ログが無いかチェック。
- Atlas に含まれない漢字が見つかったら glyphset に追加して再生成する。
