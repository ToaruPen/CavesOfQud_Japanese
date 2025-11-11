# 整形仕様 (Markup / RTF / 折り返し) v1

この文書は、Caves of Qud のテキスト整形ルールを翻訳フック視点でまとめたもの。  
`Markup` → `ClipTextToArray` → `TextBlock` / `RTF` / `TMP_Text` のどこで処理するかを明示し、表示崩れや二重整形を避ける。

## 1. Markup / Console カラーコード

| 記法 | 役割 | 注意点 |
| --- | --- | --- |
| `{{K|text}}` | Markup: `Markup.Transform` 後に `&k` や `^k` に展開。 | ネスト不可。翻訳前に最小単位へ分割して辞書キー化する。 |
| `&k` / `^k` | 前景 / 背景カラーを 1 文字コードで指定。 | `Sidebar.FormatToRTF` が `<color=#RRGGBBAA>` に変換。`&&` / `^^` はエスケープ。 |
| `{{something}}` | Grammar / HistoricStringExpander で置換される。 | 翻訳時は `{{` `}}` をそのまま保持し、語順のみ変更。 |
| `&y^K` など | `StringFormat.ClipTextToArray` が **最後のカラー状態を次行に引き継ぐ** (`KeepColorsAcrossNewlines=true`)。 | 途中でタグを閉じると、次行の先頭に `&?` `^?` が足されなくなる。 |

### Markup.Transform の挙動

- `StringFormat.ClipTextToArray` / `Popup.RenderBlock` / `Sidebar.FormatToRTF` など、**多数のメソッドが内部で自動呼び出し**する。
- 翻訳フックは原則として `Markup.Transform` を呼ぶ前に差し込む。既に整形済みの文字列に再適用すると `{{` が `{{{` になる等の副作用が出る。
- `EscapeNonMarkupFormatting=true` の場合（`Popup.WaitNewPopupMessage` など）、`&` `^` が `&&` `^^` に変換されるので翻訳もそれに合わせる。

## 2. ClipTextToArray / TextBlock

`XRL.UI.StringFormat.ClipTextToArray(string input, int maxWidth, ...)`

- 単語単位で折り返し。`nextWordLength + currentLine > maxWidth` になると `list.Add(line)` → `num = nextWordLength + 4` (空白ぶん)。
- `KeepNewlines=true` の場合は原文の改行を優先。  
  例: `Popup.PopupTextBuilder` は `RespectNewlines=true` で呼ぶので、翻訳で改行数を変えるとボックス高さが変動する。
- `KeepColorsAcrossNewlines=true` はデフォルト。行頭に `&x` `^y` を補完してカラーを継承する。
- `TransformMarkup` フラグが `true` の場合、`list.Count > 1` かつ Markup が含まれると **行ごとに `Markup.Transform`** を再実行する。
- `TextBlock` (`ConsoleLib.Console.TextBlock`) も内部で `ClipTextToArray` を使用し、`width` と `maxLines` を指定できる（例: `Popup.RenderBlock` で width 78）。

### 桁合わせ / 数値・単位

- CP437 基準で 1 文字幅。全角は 2 幅なので、翻訳で全角数字を使うと折り返し位置がズレる。
- 単位（dram、str、°）は `{{R|dram}}` などの Markup で色付けされることが多い。  
  → **翻訳では単位をプレースホルダ化** (`{0} dram` → `{0} ドラム`) し、`Grammar` 側の複数形判定は英語と同じ位置に残す。

## 3. RTF / TMP RichText

| API | 役割 | 備考 |
| --- | --- | --- |
| `Sidebar.FormatToRTF(string, opacity)` | Markup (`&`/`^`) を `<color=#RRGGBBAA>` に変換。CP437 → Unicode マッピングを行う。 | `Opacity` には `FF`(不透明) が多用される。 |
| `RTF.FormatToRTF(string s, string opacity = "FF", int blockWrap = -1)` | 必要に応じて `TextBlock` で折り返してから `Sidebar.FormatToRTF` を呼ぶ。 | `blockWrap=60` 等の呼び出し元依存パラメータを `pipelines.csv` に併記。 |
| `TMP_Text.SetText` / `UITextSkin.SetText` | 最終描画。TMP の RichText 仕様に従う。 | `<align>`, `<line-height>` などのタグを追加する場合は `TMP` 仕様準拠で。 |

### TextMeshPro のフォント / フォールバック

- `Mods/QudJP/Fonts` 配下の Font Asset を `FontManager` が差し替えている。  
  - プライマリ: `NotoSansCJK` サブセット（`Docs/font_pipeline.md` 参照）。  
  - フォールバック: Vanilla Asset (`CavesOfQud SDF`) を残し、未翻訳部分（英数字）の幅を保つ。
- TMP の RichText タグは `<color>`, `<size>`, `<align>`, `<sprite>` が許可されている。**`<font>` は使えない**ので色変更は Markup→`<color>` 経由で行う。
- Missing glyph が出た場合は `Player.log` に `Missing glyph in font asset` が出力される。`Docs/glyphset.txt` → フォント再サブセット化。

## 4. 折り返し・禁則

- Console 側は **単語禁則**のみ（英語基準）。日本語では禁則処理が存在しないため、句読点が行頭にくる。  
  → 対策: 原文トークンを `「` `」` と併せて 1 セグメントにし、翻訳で擬似的に改行を調整する。
- Unity 側（TMP）は `WordWrapping` がオン。全角は 2 幅扱いで分割されるため、必要なら `<noparse>` を利用して改行抑制（ただし `<noparse>` は TMP 3.0+ 限定）。
- `RTF.BlockWrap` を使う場面では `maxLines=5000` が指定されることが多いので、実質的に高さは TMP 側で決まる。

## 5. 複数形 / 冠詞

- `Grammar` クラスが `Grammar.Pluralize`, `Grammar.A` 等で処理する。翻訳で語順が変わる場合は **テンプレ化**して辞書キーに `%item%` などを埋め込む。
- `HistoricStringExpander` や `StringEvaluator` から渡る文字列は、最終パイプライン（Popup/Log）でまとめて `Markup.Transform`。  
  → 翻訳キーは `ContextID + Role` で束ね、複数形かどうかを `Notes` に記載する。

## 6. 参考: よく使う整形 API

| API | 主な呼び出し元 | 推奨フック | 備考 |
| --- | --- | --- | --- |
| `Markup.Transform(string)` | Popup, MessageLog, Tooltip ほぼ全域 | 可能なら Transform **前** | `list<string>` に対してもオーバーロードあり。 |
| `StringFormat.ClipTextToArray` | Popup, Journal, QuestLog | `MaxWidth` を `ContextID` に書き残す | CP437 幅。`KeepColorsAcrossNewlines` デフォルト true。 |
| `TextBlock` | Popup, BookUI, RTF.BlockWrap | 同上 | `maxLines` で高さ制限。 |
| `Sidebar.FormatToRTF` | Tooltip, MessageLog, PopupMessage | 済 | Markup を TMP RichText に変換。 |
| `RTF.FormatToRTF` | Tooltip, MessageLogWindow | 変換後は安全に弄れない | `blockWrap` 指定時は TextBlock → TMP。 |

## 7. テスト / 検証手順

1. **Console** での折り返し確認  
   - `Options.ModernUI=false` にしてポップアップ / ログを確認。スクロール表示 (`<up/down for more...>`) が想定通りかを見る。
2. **Unity / TMP**  
   - `Player.log` を監視し `Missing glyph` や `RichText` 例外をチェック。  
   - `Translator/JpLog` の ContextID 別ヒット率を集計して幅崩れと紐付ける。
3. **幅測定**  
   - `DumpPopup.exe`（リポート root） を利用して `StringFormat.ClipTextToArray` の結果をオフラインで再現 → 長文化した訳文の折り返しを検証。

## 8. 今後の TODO

- TMP 側の `<link>` / `<sprite>` を使う箇所を catalog 化して `docs/pipelines/*.md` から参照できるようにする。
- `ClipTextToArray` に禁則処理を追加する場合の擬似実装案を `Docs/specs/formatting.md` に追記。
