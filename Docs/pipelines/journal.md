# Journal / History パイプライン仕様 v1

> **画面 / 部位:** Journal タブ群（Locations / Chronology / Sultan Histories など）と BookUI（書籍閲覧）。  
> **出力:** Console（`ScreenBuffer` ベース UI）

## 概要

- `JournalScreen` は `JournalAPI` が保持する `IBaseJournalEntry` 群を取得し、`StringFormat.ClipTextToArray` で幅 75 文字に整形して `ScreenBuffer` へ描画する。
- `BookUI` は XML (`references/Base/Books.xml` 他) を `BookInfo` に読み込み、`StringFormat.ClipTextToArray` / `TextBlock` 相当のオートフォーマットでページを生成して読書ビューを描画する。
- `HistoricStringExpander` / `HistoryAPI` によるダイナミック文生成は Journal のエントリー本文に直接反映されるため、翻訳フックは「文字列展開」→「Journal 描画」の間で行う。

## 主なクラス / メソッド

| フェーズ | クラス | メソッド / 備考 |
| --- | --- | --- |
| 生成 (Journal) | `JournalScreen` | `UpdateEntries` が `JournalAPI` 経由で raw entries (`JournalAccomplishment`, `JournalMapNote`, etc.) を取得し `entry = item.GetDisplayText()` を保持。 |
| 整形 (Journal) | `JournalScreen` | `StringFormat.ClipTextToArray(..., maxWidth=75, KeepNewlines=true)` で `displayLines` を構築。カテゴリーや tradable フラグに応じて `{{G|$}}`, `@`, `[X]` などのマーカーを追加。 |
| 描画 (Journal) | `JournalScreen.Show` | `ScreenBuffer` に枠・タブ・本文を `displayLines` から描画、スクロールバーも ASCII で描画。 |
| 生成 (Books) | `BookUI.HandleBookNode` | XML `<book><page>` を Markup.Transform→`BookInfo.Texts` に蓄積。`IncludeCorpusData` 時は `BookCorpus` にも書き出し。 |
| 整形 (Books) | `BookUI.AutoformatPages` | `StringFormat.ClipTextToArray(GameText.VariableReplace(Text), width=80-左右マージン)` → `BookPage` を組み立て。 |
| 描画 (Books) | `BookUI.RenderPage` | `ScreenBuffer` に本文行（`bookPage.Lines`）を描画、`ControlManager` 由来のナビゲーションヒントも追加。 |
| 文字列展開 | `HistoryKit.HistoricStringExpander` | Sultan/Village ヒストリーやスパイス文字列で `<entity.prop>` などを解決し、`Grammar` / `HistoricSpice` を通して英語文を構築。 |

## データフロー（Journal）

1. `JournalScreen.UpdateEntries(selectedTab, GO)` が `GetRawEntriesFor(tab, category)` を呼び出し、`JournalAPI` のコレクションから `IBaseJournalEntry` を列挙。
2. 各 entry で `entry.entry = baseEntry.GetDisplayText()` を取得。Chronology/Locations では `@` / `$` / `[X]` / `[ ]` などの接頭辞を付与。
3. `StringFormat.ClipTextToArray`（幅 75, `KeepNewlines=true`）で行ごとの `list` を得て `displayLines` に追加。行頭には必要に応じ `{{G|$}}` などの Markup が残る。
4. `JournalScreen.Show` が `ScreenBuffer.SingleBox` で枠を描画後、`displayLines[currentTopLine ...]` を `ScreenBuffer.Write` で出力。スクロールバーも `177` 文字で描画。
5. 输入 (`Keyboard.getvk`) に応じて `currentTopLine` / `cursorPosition` を更新、`Popup.AskString` で加筆/削除を行う場合も Popup パイプラインへ渡る。

## データフロー（BookUI）

1. `BookUI.InitBooks()` が XML を読み込み、各 `BookInfo` に `Texts` を蓄積 (`Markup.Transform` + `[[header]]` 除去)。
2. `BookUI.AutoformatPages` が `GameText.VariableReplace` 後に `StringFormat.ClipTextToArray` で折り返し、`BookPage` (タイトル, Lines, マージン) を生成。
3. `BookUI.RenderPage` が `ScreenBuffer.Write` で本文を描画。`ControlManager` から得たキー名をフッタに表示。スクロール必要時は `177/219` でスクロールバー描画。

## 整形規則

- Journal:
  - `maxWidth=75`（左右マージン込み）で Clip。空行を 1 行挿入してエントリー間の余白にする。
  - `KeepNewlines=true` のため、エントリー内の `\n` はそのまま `displayLines` に反映。
  - 先頭マーカー例: `@`（プレイヤー手動メモ）、`{{G|$}}`（tradable）、`{{K|?}}`/`{{G|?}}`（マップ訪問状況）、`[{{G|X}}]`（トグル）。
- BookUI:
  - `StringFormat.ClipTextToArray` を `GameText.VariableReplace` 後に適用。マージンはオプション (`Margins="Top,Right,Bottom,Left"`)、既定 2。
  - `BookPage.Title` は上部 `[ {{Y|Title}} ]` で `ScreenBuffer` に描画。本文は CP437 幅で 80 列に合わせる。

## 同期性

- すべて **ゲームスレッド (sync)** で動作。`JournalScreen.Show` は `GameManager.Instance.PushGameView("Journal")` で専用ビューを開き、`Keyboard.getvk` をポーリング。
- 翻訳フックは `JournalScreen.UpdateEntries`（ゲームスレッド）や `BookUI.HandlePageNode`/`AutoformatPages`（キャッシュ初期化時）に差し込むことになるため、UI キューとの同期は不要。

## 置換安全点（推奨フック）

- `Harmony Prefix: IBaseJournalEntry.GetDisplayText`（各派生クラス）  
  - ContextID 例: `XRL.UI.JournalEntry.Accomplishment.EntryLine`.  
  - 元テキストを翻訳した上で `JournalScreen` へ渡す。`JournalScreen` では同じ `entry` が `StringFormat.ClipTextToArray` に流れる。
- `Harmony Prefix: XRL.UI.JournalScreen.UpdateEntries`  
  - ContextID: `XRL.UI.JournalScreen.UpdateEntries.EntryLine`.  
  - `journalEntry.entry` を置き換えたり、カテゴリ表示行（`displayLines.Add(...)`）を翻訳する。
- `Harmony Prefix: XRL.UI.BookUI.HandlePageNode` / `AutoformatPages`  
  - ContextID: `XRL.UI.BookUI.HandlePageNode.PageText`.  
  - XML から読み込んだページ文字列を翻訳してから `BookInfo.Texts` に格納すれば、後続の Clip を再利用できる。
- `Harmony Prefix: HistoryKit.HistoricStringExpander.ExpandQuery`  
  - ContextID: `HistoryKit.HistoricStringExpander.ExpandQuery.Token`.  
  - Sultan/Village ログがここで生成されるため、テンプレートレベルで翻訳すると Journal 以外の用途（会話等）にも反映される。適用範囲が広い点に注意。

## 例文 / トークン

- Chronology: `"{{G|$}} Discovered {{C|[place]}}"`, `"@ Brought back 8 dram of water."`
- Sultan history: `"During the reign of Pall the Flute, {{...}}."`（`HistoricStringExpander` で `<entity$leader.name>` などが展開）
- Book page: `"&y^kThe Secrets of the Sightless Way\n\n{{K|...}}"` – `[[Title]]` のようなヘッダは読み込み時に除去。

## リスク

- `StringFormat.ClipTextToArray` の幅 75 に合わせず長文化するとスクロール量が増え、カーソル位置がずれる。特に `[{{G|X}}]` のようなマーカー込みで幅を計算する必要がある。
- Journal のカテゴリー行（例: `"{{K|You have no map notes.}}"`）は `selectedCategory==null` の場合のみ表示されるため、翻訳が空になると UI が無反応に見える。
- BookUI で `[[header]]` を翻訳テキストに残すと読書ビューでも `[[` `]]` が表示される。`HandlePageNode` でヘッダ削除 → 翻訳の順序に注意。
- `HistoricStringExpander` を不用意に変えると会話やツルバグ生成にも影響するため、文脈毎に ContextID を分ける必要がある。

## テスト手順

1. ゲーム内で Journal（`Ctrl+J` / メニュー）を開き、各タブ（Locations / Chronology / Sultan Histories / Villages / General / Recipes）を確認。  
   - カテゴリ一覧 → エントリー本文が翻訳され、スクロール／カーソルが正しく機能するかをチェック。
2. `JournalScreen.HandleInsert/Delete` で手動エントリーを追加／削除し、日本語入力 → `StringFormat.ClipTextToArray` 後の折り返しを確認。
3. 書籍を読む（例: 図書館や `book` アイテム使用）。`BookUI.RenderPage` の本文・タイトル・フッタが翻訳済みで崩れないか確認。
4. `Translator/JpLog` に `ContextID=JournalScreen.UpdateEntries.EntryLine` 等のログを追加し、ヒット状況を観測。
