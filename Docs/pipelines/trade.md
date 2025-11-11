# Trade / Barter パイプライン仕様 v1

> **画面 / 部位:** 取引（プレイヤーと NPC の barter／コンテナ transfer）  
> **出力:** Console（`TradeUI`） / Unity（`TradeScreen` + `TradeLine`）

## 概要

- **クラシック Trade**: `XRL.UI.TradeUI.ShowTradeScreen` が `Objects[2]`（プレイヤー側 / トレーダー側）の `TradeEntry` 配列を構築し、`ScreenBuffer` で 80×25 の左右 2 ペインを描画する。価格や重量は `string.Format`（`{0:0.###}` や `{$"{num,5}"}`）を使って手動整形。
- **Modern Trade**: `Qud.UI.TradeScreen` が `TradeLineData`（カテゴリ＆アイテム行）を `FrameworkScroller` に渡し、`TradeLine` の `UITextSkin.SetText` で TMP RichText を描画。検索／ソート／ドラッグ＆ドロップも Unity 側で処理する。
- 共通で価格計算を `TradeUI.GetValue` / `TradeUI.FormatPrice` に委ね、`Performance`（交渉結果）や `costMultiple`（容器 transfer など）を加味する。

## 主なクラス / メソッド

| フェーズ | Console | Unity |
| --- | --- | --- |
| 生成 | `TradeUI.GetObjects`, `TradeEntry(Category|GameObject)` | `TradeScreen.ClearAndSetupTradeUI`, `TradeLineData.set` |
| 整形 (一覧) | `TradeUI.ShowTradeScreen`: `ScreenBuffer.Write` でカテゴリ行・アイテム行を直接組み立て、右端に `{{K|weight#}}` や `{{C|$value}}` を描画 | `TradeLine.setData`: `categoryText.SetText("[+/-] Category")`, `text.SetText(go.DisplayName)`, `rightFloatText.SetText("[$xx.xx]")`, `check.SetText`（選択数） |
| 価格/重量表示 | `UpdateTotals` → `sReadout = " {{C|123}} drams <-> {{C|45}} drams …"`、重量 `{{K|current/max lbs.}}` | `TradeScreen.UpdateTotals`: `Totals` / `Weight` を再計算し、`detailsRightText` や画面下部の合計表示 (`UITextSkin.SetText`) に反映 |
| 操作ボタン | 下部の `[ESC Exit] [1-0 Pick] [Enter Offer] …` を文字列連結 | `MenuOption` (`OFFER_TRADE`, `SET_FILTER` etc.) をボタンバーに表示（`UITextSkin`） |
| 詳細表示 | 下部 2 行: アイテム Renderable ＋ `transformed description` | `detailsLeftText.SetText(go.DisplayNameSingle)`, `detailsRightText.SetText("{{K|5#}} {{Y|$}}…")` when highlight |

## データフロー

### Console (`TradeUI`)
1. `GetObjects(Trader, Objects[0])` / `GetObjects(The.Player, Objects[1])` がカテゴリ挿入を含む `TradeEntry` リストを生成。
2. `NumberSelected[side][row]` で選択数を保持、`UpdateTotals` が `Totals` / `Weight` / `sReadout` を再計算 (`Totals[i] += ItemValueEach * Performance` など)。
3. `ShowTradeScreen` の描画ループで:
   - 左/右ペインを `ScreenBuffer.WriteAt` で描画（カテゴリ→アイテム→選択数→価格→重量）。
   - 下部の合計表示 (`{{C|{Totals[0]:0.###}}}`) や重量 (`{{K|current/max lbs.}}`) を出力。
   - `keys = Keyboard.getvk(...)` で入力処理 (`CmdTradeOffer`, `CmdVendorActions`, `Ctrl+F` など)。
4. 取引成立時は `TradeUI.DoTrade`（別メソッド）に遷移し、ログメッセージなどを出す。

### Modern (`TradeScreen`)
1. `ClearAndSetupTradeUI` → `listItems[side]` を `TradeLineData` で構築。各カテゴリごとに collapsible 行を挿入。
2. `TradeLine` バインド時に:
   - カテゴリ: `[+/-] CategoryName`、`check` 非表示。
   - アイテム: `text = go.DisplayName`, `check = numberSelected`, `rightFloatText = "[$xx.xx]"`（通貨は `{{W|}}` 付き）。
3. `HandleHighlightObject` でサイドバー `detailsLeft/Right` を更新（`DisplayNameSingle`, `"{{K|weight#}} {{Y|$}}…"`)。
4. `UpdateTotals` でドラッグ表示や下部の合計をリフレッシュ、`dragIndicatorText` に選択数を表示。
5. `MenuOption` バー（Offer, Add/Remove, Toggle Sort, Filter, Vendor Actions）も `UITextSkin` で表示。

## 整形規則

- Console:
  - 固定幅（40 列×2）。訳文が長いと右列や重量列に重なりやすいので、カテゴリ名・操作案内は短めに要調整。
  - 価格表示：`{$"{Totals[0]:0.###}"}` の `0.###` フォーマットは桁指定。`FormatPrice` も `"{value:0.00}"` を返す。翻訳でフォーマット文字列を変えない。
  - 重量: `"{{K|{current}/{max} lbs.}}"` の `lbs.` を翻訳する場合は `LengthExceptFormatting` 依存に注意。
- Unity:
  - `UITextSkin` が `ToRTFCached(blockWrap=72)` を通す。 `rightFloatText` に `{{W|}}` を埋め込むなど Markup 前提。
  - `detailsRightText` では `{{K|weight#}}` + `{{Y|$}}` + `{{C|price}}` の複合 Markup。文字列構造を崩すと色がズレる。
  - カテゴリ `[+]/[-]` は UI ロジックと連動。`categoryText` の書式（`"[+]" + name`）を保つ。

## 同期性

- 両 UI ともゲームスレッドでリスト構築と数値計算を行う。Console 版は `Keyboard.getvk` ループ、Modern 版は `FrameworkScroller` で UI スレッドに反映。
- `APIDispatch.RunAndWaitAsync` を通じて `TradeUI` の同期版ロジックを呼び出す箇所がある（Vendor Actions 等）。翻訳フックは基本的にゲームスレッド側で問題なし。

## 置換安全点（推奨フック）

- `GameObject.DisplayName` / `DisplayNameSingle` → 取引リストに表示されるテキスト全般。
- `TradeUI.FormatPrice`, `TradeUI.UpdateTotals`  
  - ContextID: `XRL.UI.TradeUI.Totals.Readout`, `XRL.UI.TradeUI.TotalWeight`.  
  - 合計表示を翻訳する場合、ここで書き換える。
- `Qud.UI.TradeLine.setData`  
  - ContextID: `Qud.UI.TradeLine.CategoryText`, `Qud.UI.TradeLine.RightFloat`.  
  - Modern UI 固有の語順を変更するならこのメソッドに Harmony で介入。
- `Qud.UI.TradeScreen.HandleHighlightObject`  
  - ContextID: `Qud.UI.TradeScreen.DetailsRight`.  
  - サイドバー詳細を翻訳・再整形する際に使用。
- `TradeUI.ShowVendorActions` / `TradeUI.DoVendor*`  
  - ベンダーアクションのダイアログやログも同時に翻訳する場合はこの周辺を参照。

## リスク

- Console 版は `ScreenBuffer` の位置指定に依存するため、翻訳で文字数が増えると左右ペインの境界（x=39/40）を越えて崩れる。特に `sReadout` や `text4`（説明ラベル）は `LengthExceptFormatting` 前提で右寄せしている。
- Modern 版の `rightFloatText` 色 (RGB) を直接設定しているため、`SetText` に `<color>` を追加すると意図と異なる色になる可能性あり。必要なら `UITextSkin.color` を設定する。
- `FuzzySharp` 検索 (`searchText`) は英語 DisplayName を lower-case で格納。翻訳したテキストを検索対象にしたい場合は `TradeEntry` or `TradeLineData` で再生成する必要がある。
- 取引ロジックが `GameObject.Count` / `Weight` を前提にしているので、翻訳が `#` 記法を外すと重量表示 (`{{K|5#}}`) が崩れる。

## テスト手順

1. **Console Trade** (`Options.ModernUI=false`): 任意の商人と交渉し、左右ペインのカテゴリ・アイテム表示、合計表示、重量表示、入力プロンプトが翻訳崩れなく表示されるか確認。
2. **Modern Trade**: `ModernTrade` UI でカテゴリ折り畳み、検索、ドラッグ操作、Offer/Haggle を実行し、`TradeLine` のテキスト・サイドバー詳細・メニューバー文言を確認。
3. ベンダーアクション（Repair/Recharge/Read 等）をトリガーしてポップアップ文言をチェック。
4. `costMultiple=0`（隊員との transfer）や `TradeScreenMode.Container` を試し、`"transfer"` などモード依存テキストが翻訳されているか確認。
