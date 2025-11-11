# Conversation / Dialogue パイプライン仕様 v1

> **画面 / 部位:** 会話ウィンドウ（NPC 対話、ショップ提示、クエスト分岐など）  
> **出力:** Console（`ConversationUI.RenderClassic`） / Unity Popup（`Popup.ShowConversation` → `Qud.UI.PopupMessage`）

## 概要

- 会話は `XRL.World.Conversations.Conversation`（XML ブループリント）を解析して `Node`（発話）と `Choice`（選択肢）を生成する。各 `IConversationElement` が `GetDisplayText` でテキストを組み立て、`HistoricStringExpander` / `GameText.VariableReplace` を通じてトークンを展開する。
- **コンソール表示**（クラシック UI）では `ConversationUI.RenderClassic` が `ScreenBuffer` 上に `RenderableLines` / `RenderableSelection` を描画し、`Markup.Transform` 済みの文字列を CP437 幅で整形。
- **Modern UI** では `ConversationUI.Render` → `Popup.ShowConversation` → `Popup.WaitNewPopupMessage` → `PopupMessage.ShowPopup` というポップアップ経路に乗り、本文と選択肢を `UITextSkin.SetText`（TMP RichText）で描画。
- 会話中に表示されるトレード選択 (`[begin trade]`) は `Conversation.Parts.Trade` が `Choice` を拡張し、`PopupMessage` のボタンへ `Trade.ShowScreen()` を接続する。

## 主なクラス / メソッド

| フェーズ | クラス / メソッド | 役割 |
| --- | --- | --- |
| データロード | `Conversation`, `ConversationXMLBlueprint` | XML (`ConversationLoader`) から `Node`, `Choice`, `ConversationText` を生成。 |
| テキスト生成 | `IConversationElement.GetDisplayText` | `GetText()` → `ConversationText.Text` を選び、`GameText.VariableReplace` / `HistoricStringExpander` で変数展開。 |
| 会話制御 | `ConversationUI.HaveConversation` | `StartNode` から `CurrentNode` を巡回、`Choice.Enter()` → `Node` 遷移を繰り返す。 |
| Console 描画 | `ConversationUI.RenderClassic` | `StringFormat.ClipTextToArray` で本文 / 選択肢を折り返し、`ScreenBuffer.Write` で二列表示（`"> 1) text"`）。スクロール・ショートカット（1-9/A-Z）を実装。 |
| Unity 描画 | `ConversationUI.Render` → `Popup.ShowConversation` | `PopupMessage.ShowPopup` がタイトル（`GetTitle()`）、本文 (`CurrentNode.GetDisplayText`)、選択肢リスト (`Choice.GetDisplayText`) を TMP で描画。 |
| Trade フック | `XRL.World.Conversations.Parts.Trade` | `Choice` に `{{g|[begin trade]}}` タグを付与し、選択時に `TradeUI.ShowTradeScreen` を呼ぶ。 |

## データフロー

1. `ConversationUI.HaveConversation("NPCConversationID", speaker)` が呼ばれると、`Conversation` インスタンスが `CurrentNode = GetStart()` をセットし、`Choice` リストを準備。
2. `CurrentNode.Prepare()` → 各 `Choice` が `GetDisplayText(WithColor:true)` を返す。この時点で `HistoricStringExpander` / Grammar / `%object%` トークンが展開される。
3. **Console**: `RenderableLines` で本文 (`Node`) を幅 `Width`（76列）にクリップ → `RenderableSelection` で `Choice` ごとに `StringFormat.ClipTextToArray` → `ScreenBuffer` に書き込み。`{{color|}}` を保持したまま `(Index == SelectedChoice ? "{{Y|>}}" : "  ")` を付加。
4. **Modern**: `Popup.ShowConversation` が `Title`（`Speaker.DisplayName`）、`Icon`（`Speaker.RenderForUI`）、本文/選択肢を `WaitNewPopupMessage` に渡す。`PopupMessage.ShowPopup` は `UITextSkin` で RichText 化し、ボタン（`Choice`）を `QudMenuItem` として描画。
5. ユーザー入力で `ConversationUI.Select(index)` → `Choice.Enter()` → `Node.Leave()` → `Conversation.GetTargetNode` → 次ノードへ遷移。`Render()` で再描画。
6. `Trade`, `Conversation Parts` が `GetChoiceTagEvent`, `EnterElementEvent` 等をフックし、会話中にサブ UI (Trade, Vendor Actions) を起動。

## 整形規則

- `IConversationElement.GetDisplayText` が返す文字列は `{{color|}}` を含む Markup。Console 版では `SB.WriteAt` にそのまま渡し、Unity 版では `PopupMessage` が `Markup.Transform` → `UITextSkin.SetText` で TMP RichText に変換。
- 選択肢番号は `DisplayChar` (`1-9`, `A-Z`)、`RenderableSelection.Render` が `{{W|#}})` 形式で描画。Modern 版では `PopupMessage` が `[space]` ボタンに相当する `QudMenuItem` を生成。
- `Choice.GetTag()` が `{{K|[End]}}` や `{{g|[begin trade]}}` を追加する。翻訳時は `Choice` の `GetTag` を置き換えると `Trade.Visible` など他システムにも影響するので、辞書対応で同じマーカーを維持。
- `ConversationText` / `ConversationXMLBlueprint` の `Text` 要素は `|` で Markup を付けたり `~` でランダム分岐（`"line1~line2"`）を指定できる。翻訳では `~` 区切りを壊さない。

## 同期性

- `ConversationUI.InternalConversation` はゲームスレッドで実行。Console 版は `Keyboard.getvk` ループ、Modern 版は `PopupMessage`（uiQueue）に処理を委譲し、結果が戻るまで待機。
- 翻訳フック（`ConversationText.Text`, `Choice.Texts`）は `CurrentNode.Prepare` などゲームスレッド側で挿入する。`PopupMessage` に直接手を入れる必要はない。

## 置換安全点（推奨フック）

- `ConversationXMLBlueprint` → `ConversationText.Text` 生成時  
  - XML から読み込む段階で翻訳済みテキストに差し替え（`ConversationLoader` or `ConversationText`）。
- `IConversationElement.GetDisplayText` / `ConversationText.GetText`  
  - ContextID: `XRL.World.Conversations.Node.Text`, `XRL.World.Conversations.Choice.Text`.  
  - Grammar 展開後の文章を最終的に翻訳する場合に使用。ただし `Choice` の `GetTag` なども同時に確認。
- `Popup.ShowConversation`（Modern UI 最終段）  
  - ContextID: `XRL.UI.Popup.ShowConversation.Body`, `...Choice`.  
  - Console/Unity で別訳を出したい場合に fallback として利用可能。
- `ConversationParts`（Trade, Vendor Actions など）  
  - `GetChoiceTagEvent`, `EnterElementEvent` をハーモニーして `[begin trade]` 等のタグ文字列を翻訳。

## リスク

- Console 版は `ScreenBuffer` 幅が限定的（76列）。訳文が長いとスクロールが頻発し、`<more...>` 表示が増える。特に多段階の NPC 説明は Markup も含むため、幅調整が難しい。
- Modern 版は `PopupMessage` のボタン幅が固定なので、選択肢を長文化すると折り返しが発生し、 `[space]` ボタンと重なる場合がある。
- `HistoricStringExpander` / `Grammar` トークンは会話以外にも利用されるため、翻訳でテンプレを壊すと別 UI（NPC チップ、ツールチップ等）に影響する。
- `Choice` の `Target` や `ID` を翻訳してはいけない（遷移ロジックが壊れる）。必ず `Texts` / `Description` など表示フィールドのみに留める。

## テスト手順

1. NPC との代表的な会話（売買、クエスト、派閥会話、HistoricStringExpander を含む会話）を行い、**Console と Modern** 両方で本文・選択肢が翻訳されているか確認。
2. `Trade` 選択肢 (`[begin trade]`) や `End` 選択肢 (`[End]`) のタグが翻訳後も表示され、機能が維持されているかテスト。
3. `HistoricStringExpander` を多用する会話（例: 村長の挨拶）で `%subject%`, `<spice.*>` などのトークンが正しく展開され、色タグが崩れていないか確認。
4. `Popup.ShowConversation` 経路で `PopupMessage` によるポップアップが開かれる場合（Modern UI）に `Translator/JpLog` の ContextID がヒットしているか監視し、未翻訳の Choice / Node を洗い出す。
