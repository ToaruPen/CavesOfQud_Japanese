# Quests / Book モダン UI パイプライン v1

> **対象** StatusScreens の `Quests` タブと、Unity ベースの `BookScreen`（Modern Journal/書籍リーダー）  
> **参照 ILSpy** `Qud.UI.QuestsStatusScreen`, `Qud.UI.QuestsLine`, `Qud.UI.QuestsLineData`, `XRL.UI.QuestLog`, `MapScrollerController`, `MapScrollerPinItem`, `Qud.UI.BookScreen`, `Qud.UI.BookLine`, `Qud.UI.BookLineData`, `XRL.UI.BookUI`, `XRL.World.Parts.MarkovBook`

## Quests Status Screen

### 概要
- `QuestsStatusScreen` は `QuestsAPI.allQuests()` でアクティブなクエストを集め、`FrameworkScroller` に `QuestsLineData` を流し込む。`CollapsedEntries` で折り畳み状態（「[-]」「[+]」）を保持し、`HandleSelectItem`/`HandleV±` でトグル。
- 行テキストは `QuestsLine.setData` が担当。タイトル/発注者/本文の 3 本の `UITextSkin` を使い、本文は `QuestLog.GetLinesForQuest` が生成するマークアップ済み文字列（`{{white|ﾃｹ step}}` など）を結合している。
- 検索バーは `BaseStatusScreen.filterText` を利用し、`FuzzySharp.Process.ExtractTop` で最大 50 件までヒットさせる。`QuestsLineData.searchText` は `Quest.DisplayName`、依頼主、各 `QuestStep.Name`/`Text` を小文字化してキャッシュする。
- 右側のミニワールドマップは `MapScrollerController`。`QuestsStatusScreen.UpdateViewFromData` でロケーションごとに `MapPinData` を作り、`MapScrollerPinItem.SetData` → `UITextSkin.SetText` でタイトル/詳細を描画する。同時に `mapController.SetHighlights` でハイライト座標も推定。

### 主な処理 / メソッド

| コンポーネント | メソッド | 役割 / メモ |
| --- | --- | --- |
| データ収集 | `QuestsStatusScreen.UpdateViewFromData` | `QuestsAPI.allQuests()` → `PooledFrameworkDataElement<QuestsLineData>.next()` → 検索結果でフィルタ。`searcher.searchText`（小文字）と `Process.ExtractTop` を共有。 |
| 折り畳み保存 | `QuestsStatusScreen.CollapsedEntries` | `HashSet<string>` で Quest ID を保持。`HandleSelectItem` / `HandleV±` / `QuestsLine.XAxis` から出し入れされる。 |
| 行表示 | `QuestsLine.setData` | タイトル = `[+/-] + Quest.DisplayName`（ColorUtility.StripFormatting）、`giverText` = `QuestGiverName / QuestGiverLocationName`。本文は `QuestLog.GetLinesForQuest(..., ClipWidth=70|45)` を連結し `bodyText.SetText`. |
| 本文生成 | `QuestLog.GetLinesForQuest` | `QuestStep` ごとに `StringFormat.ClipTextToArray`（幅=ClipWidth-3）で整形、`{{white|ﾃｹ ...}}`, `{{red|X}}` などのグリフ/色タグを差し込む。Console/Unity 両方が同メソッドを共有。 |
| マップ連動 | `MapScrollerController.SetPins` / `MapScrollerPinItem.SetData` | `Quest.QuestGiverLocationZoneID` から `Location2D` を逆引き→ `MapPinData.title = "{{W|地名}}"`, `details = "{{B|quest:}} <display name>"`. TMP に渡す直前で翻訳する必要がある。 |

### データフロー
1. `ShowScreen` で `filterBar` と `controller` のリスナーを設定し、`UpdateViewFromData()` を即時実行。
2. `UpdateViewFromData()`  
   1. 既存 `searchResult` を `free()` → クリア。  
   2. `QuestsAPI.allQuests().Where(!Finished)` から `QuestsLineData.set(quest, expanded)` を生成。  
   3. 検索文字列が空なら全件を `searchResult` へ。検索中は Fuzzy 検索で 50 件抽出。  
   4. MapPin 用にロケーションごと `MapPinData` を作り、ピン詳細文（`quest:` 行）を構築。  
   5. `controller.BeforeShow(searchResult)` → `FrameworkScroller` が各 `QuestsLine` に `setData` を呼ぶ。  
   6. `mapController.SetHighlights` / `SetPins` でミニマップ更新、`mapTarget` は非表示。
3. `QuestsLine.setData()` が `UITextSkin.SetText` を呼び出し、ユーザー入力で `context` を通じて再び `QuestsStatusScreen.UpdateViewFromData()` が走る。

### 翻訳フック候補
- `Qud.UI.QuestsLine.setData`  
  - ContextID 目安: `Qud.UI.QuestsLine.TitleText`, `.GiverText`, `.BodyText`.  
  - Quest タイトル・依頼主・本文を TMP に渡す直前。`QuestLog.GetLinesForQuest` の色タグ（`{{white|ﾃｹ}}` 等）を温存しつつ翻訳する。
- `XRL.UI.QuestLog.GetLinesForQuest`  
  - Console 版 QuestLog でも使用される共通メソッド。ここで `QuestStep.Name/Text` を翻訳し、`ClipTextToArray` へ渡す文字列長を管理すれば Classic/Modern どちらも揃う。
- `Qud.UI.QuestsStatusScreen.UpdateViewFromData` / `MapScrollerPinItem.SetData`  
  - ピンタイトルと詳細（`quest:` ラベル、依頼主名、地名）をまとめて差し替える。ハイフライト対象が 0 件のときに「You have no active quests.」メッセージを翻訳する場合もここ。

### 注意点 / バッドノウハウ
- `QuestsLineData.searchText` は一度生成されると `_searchText` にキャッシュされる。翻訳後の語で検索させるにはキャッシュ無効化、または `Quest`/`QuestStep` 側を恒常的に翻訳する必要がある。
- `QuestLog.GetLinesForQuest` は `StringFormat.ClipTextToArray` を複数回呼ぶ。翻訳で行幅が変わると `ClipWidth-3` を超えて不自然な改行が発生するため、要テスト。
- `MapScrollerController` は `SetPins` 呼び出しのたびに古いピンをプールへ戻す。翻訳フックで例外を出すと GameObject プールが枯渇するので、例外を飲むかエラーハンドリングを追加する。
- `QuestsLine` は `Media.sizeClass` によって本文クリップ幅を 70/45 に切り替える。翻訳済みテキストでも同じ幅で収まるか確認する。

### 確認ポイント
1. 検索バーに日本語を入れても `Process.ExtractTop` がヒットするか（`searchText` を翻訳済文字列で構築しているか）。
2. `QuestLog.GetLinesForQuest` のマークアップ（`{{red|X}}`, `{{green|ﾃｻ}}` など）が TMP で崩れないか。
3. 依頼主/ロケーション名が未定義（`<unknown>`）のケースで翻訳が二重括弧にならないか。
4. ミニマップの `MapPinData.details` が複数行になる場合でも、行頭 `quest:` ラベルが揃っているか。

## BookScreen（モダン書籍ビュー）

### 概要
- `BookScreen` は `MarkovBook`（動的生成）または `BookUI.Books[BookID]`（静的 XML）からページ文字列を取得し、左右 2 枚の `FrameworkScroller` に `BookLineData` を投げる。Modern UI の書籍/日誌表示はすべてここを通る。
- `RenderCurrentPage()` が `MarkovBook.Pages[CurrentPage].RenderForModernUI` もしくは `BookInfo.Pages[CurrentPage].RenderForModernUI` を返し、`UpdateViewFromData()` で単一行の `BookLineData` にラップして `pageControllers[0].BeforeShow(...)` する。ページ番号表示やホットキーラベル (`hotkeyText`, `leftPageNumber`, `rightPageNumber`) もこのタイミングで更新。
- `BookLine.setData` は単に `BookLineData.text` を `UITextSkin.SetText` へ渡す。したがって翻訳の主戦場は `RenderCurrentPage()` で作られるテキスト、または `BookUI` 側の段組処理。

### 主な処理 / メソッド

| コンポーネント | メソッド | 役割 / メモ |
| --- | --- | --- |
| Page 準備 | `BookScreen.BeforeShow` | ナビゲーション文脈をセットし、既存 `BookLineData` をプールに返却。`UpdateMenuBars` → `UpdateViewFromData` を呼ぶ。 |
| ページレンダリング | `BookScreen.RenderCurrentPage` | `Book` があれば `Book.Pages[CurrentPage].RenderForModernUI`、無ければ `BookUI.Books[BookID].Pages[...]`。プレーン文字列（TMP マークアップ付き）を返すのみ。 |
| 行設定 | `BookLine.setData` | `BookLineData.text` を `UITextSkin` に流しこむ。1 ページ=1 行なので、ここで追加の組版・翻訳を行うなら丸ごと差し替える。 |
| 書籍データ | `XRL.UI.BookUI.HandleBookNode / HandlePageNode` | XML `books.xml` を読み、`Markup.Transform` → `GameText.VariableReplace` → `StringFormat.ClipTextToArray` をかけて `BookInfo.Texts` を生成。Console UI と Modern UI の両方が同じデータを使う。 |
| 動的書籍 | `XRL.World.Parts.MarkovBook.GenerateFormattedPage` | Markov 連鎖で文章を生成し、`RenderForModernUI` に文字列をキャッシュ。ここで翻訳しておけば Popup/BookScreen/Journal すべてに反映される。 |

### データフロー
1. `BookScreen.showScreen(MarkovBook book, ...)` が呼ばれると `BeforeShow()` → `UpdateViewFromData()` を実行し、`pageControllers[0].BeforeShow([...])` に現ページ文字列を預ける。
2. `FrameworkScroller` が `BookLine` を生成して `setData` を呼び、`UITextSkin.SetText` (TMP) が描画。
3. ページ送り (`HandleMenuOption`, `NEXT_PAGE/PREV_PAGE`) のたびに `CurrentPage` を更新し `UpdateViewFromData()` を再実行。
4. 書籍の元データは `BookUI.Books` または `MarkovBook.Pages` にあり、いずれも `RenderForModernUI` に既にマークアップ済テキストが入っている。

### 翻訳フック候補
- `Qud.UI.BookScreen.RenderCurrentPage` / `UpdateViewFromData`  
  - ContextID: `Qud.UI.BookScreen.PageText`, `Qud.UI.BookScreen.TitleText`, `Qud.UI.BookScreen.HotkeyText`.  
  - ページ文字列を UI へ渡す直前で翻訳し、必要であればページ幅に合わせて改行を挿入する。
- `Qud.UI.BookLine.setData`  
  - ContextID: `Qud.UI.BookLine.Text`.  
  - 最後の安全弁。`BookLineData.text` を丸ごと置き換えたり、マークアップを追加したい場合に利用できる。
- `XRL.UI.BookUI.HandlePageNode` / `MarkovBook.GenerateFormattedPage`  
  - 書籍本文をソース段階で翻訳しておけば、Console/Modern 両方の UI で整合が取れる。`Markup.Transform` 直後 or `RenderForModernUI` をキャッシュする時点が最も副作用が少ない。

### 注意点 / 確認事項
- `BookLine` は 1 行=1 ページでスクロールしない構造のため、改行を増やすとページ送りが崩れる。翻訳で行数が増える場合は `BookInfo` 側で段組し直す。
- `BookScreen` は `MenuOption` でページ送り音(`Sounds/Interact/sfx_interact_book_read`)を鳴らす。フックで例外を出すと BGM/SE が鳴りっぱなしになることがあるので注意。
- `BookUI.HandlePageNode` は `Markup.Transform` の後に `text.Replace("[[", "").Replace("]]", "")` を実行する。翻訳で `[[...]]` を使うと消されるので挿入しない。
- Markov 書籍は `MarkovChain.GenerateTitle` などでタイトルを作る。タイトルだけ別途翻訳する場合はゲーム内で `SetContents` の後にフックするか、`MarkovBook.Title` を上書きする。

### テスト観点
1. 2 カラム書籍（左/右ページ）の跨ぎ位置で文が不自然に切れないか。
2. 検索欄 (`OnSearchTextChange`) に日本語を打った際に期待通りのページへジャンプできるか。
3. `FREE_TEXT` のような動的ページが多い書籍でも `RenderForModernUI` のキャッシュが翻訳後に再生成されるか。必要に応じてキャッシュ破棄を挿入する。
