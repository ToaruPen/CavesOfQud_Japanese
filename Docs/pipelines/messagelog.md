# Message Log パイプライン仕様 v1

> **画面 / 部位:** 画面左 / 右サイドバーの戦闘ログ + Modern UI の MessageLog ウィンドウ  
> **出力:** Console（`Sidebar` 経由） / Unity（`MessageLogWindow` → TMP）

## 概要

- すべてのログ行は `MessageQueue.AddPlayerMessage` / `Add` を経由し、`Markup.Transform` 済みの文字列として `Player.Messages` に蓄積される。
- Console サイドバーは `Sidebar` が 1 フレームに 1 回 `MessageQueue.GetLines(0,12)` を呼び、`Text.DrawBottomToTop` + `ScreenBuffer` で描画。
- Unity MessageLog は `XRLCore.RegisterNewMessageLogEntryCallback` が `Qud.UI.MessageLogWindow.AddMessage` を呼び、`RTF.FormatToRTF`→TMP へ流す。
- ContextID 例: `XRL.Messages.MessageQueue.AddPlayerMessage.LogLine`, `Qud.UI.MessageLogWindow.AddMessage.LogLine`.

## 主なクラス / メソッド

| フェーズ | クラス | メソッド / 備考 |
| --- | --- | --- |
| 生成 | `XRL.Messages.MessageQueue` | `AddPlayerMessage`, `Add`。`Markup.Transform` で色タグを統一。 |
| 蓄積 | `XRL.Messages.MessageQueue` | `Messages` List / `Cache_0_12`（直近 12 行のキャッシュ）。 |
| 整形(コンソール) | `XRL.UI.Sidebar` | `Text.DrawBottomToTop` が `ScreenBuffer` に逆順で書き出す。 |
| 整形(Unity) | `Qud.UI.MessageLogWindow` | `AddMessage` → `_AddMessage` → `RTF.FormatToRTF`. |
| 描画(Unity) | `Qud.UI.MessageLogPooledScrollRect` | `Add` で `MessageLogElement` (TMP) に文字列をセット。 |

## データフロー

1. 各種システムが `MessageQueue.AddPlayerMessage("{{Y|You hit the snapjaw}}")` を呼ぶ（Thread: `sync` / game thread）。
2. `AddPlayerMessage` が `ColorUtility.CapitalizeExceptFormatting`（任意）→ `Markup.Transform` を通して `Messages` List に追加、`XRLCore.CallNewMessageLogEntryCallbacks` を発火。
3. **Console:** `Sidebar` が描画タイミングで `MessageQueue.GetLines(0,12)` を呼び、結果の `StringBuilder` を `Text.DrawBottomToTop` に渡して `ScreenBuffer` へ描画。
4. **Unity:** `MessageLogWindow.AddMessage` が `uiQueue` で `:: ` prefix を付与し、`RTF.FormatToRTF` で TMP 用 RichText に変換 → `messageLog.Add`.
5. ユーザーが `MessageLog` ウィンドウを開いたときも、同じ `messageLog` コンポーネントがスクロールする。

## 整形規則

- Console:
  - 1 行は `">^k &y"+Message` として `Text.DrawBottomToTop` に渡る。  
  - 画面に表示されるのは常に 12 行。長い行は `ConsoleLib` 内で CP437 ベースの幅換算を受け、`...` へクリップ。
- Unity:
  - `MessageLogWindow.AddMessage` は `:: ` を頭に付けて `RTF.FormatToRTF`。  
  - `RTF.FormatToRTF` は色タグ (`{{Y|...}}`) を TMP RichText に変換し、必要に応じて `<line-height>` も付ける。
- 両者とも翻訳時に `{{` / `}}` を壊さないこと。半角スペースの有無だけでキーが変わるため、辞書登録時に `Trim+CollapseWhitespace` を適用する。

## 同期性

- `MessageQueue.Add*` はゲームスレッド上で呼ばれる。ここで重い I/O を行うとターン進行が止まる。
- `MessageLogWindow.AddMessage` は `GameManager.Instance?.uiQueue?.queueTask` で `uiQueue` に乗る。翻訳辞書から読み出す場合はスレッドセーフに。
- `Sidebar` は描画中に `MessageQueue.GetLines` を呼ぶので、ここで翻訳をしようとすると毎フレーム再計算になる。**必ず `AddPlayerMessage` 側で翻訳を済ませる。**

## 置換安全点（推奨フック）

- `Harmony Prefix: XRL.Messages.MessageQueue.AddPlayerMessage(string message, ...)`  
  - ContextID: `XRL.Messages.MessageQueue.AddPlayerMessage.LogLine`.  
  - `Markup.Transform` 前に原文を翻訳して `ref message` を挿げ替える。ログ行の一次情報はほぼすべてここを通る。
- `Harmony Prefix: XRL.Messages.MessageQueue.Add(string message)`  
  - コンソールコマンドやデバッグ出力（`!clear` など）向け。ContextID `XRL.Messages.MessageQueue.Add.LogLine`.
- `Harmony Postfix: Qud.UI.MessageLogWindow.AddMessage`  
  - Unity 表示でのみ差し替えたい場合の最終手段。ここで文章を差し替えると `Sidebar` 側は英語のままになるため、原則使用しない。

## 例文 / トークン

- `"{{G|You hit the watervine for {{R|5}} damage.}}"` – dmg 値のみ動的。`ContextID=MessageQueue.AddPlayerMessage.LogLine` + 部分テンプレ `%d` などで辞書キーを安定化。  
- `"{{R|You take 2 bleeding damage.}}"` – 状態異常ログ。  
- `"{{Y|You begin autoexploring.}}"` – Verb の活用を保ったまま翻訳する必要があるため、`Grammar` エントリから呼び出されるケースは `HistoricStringExpander` 由来と紐づける。

## リスク

- `Sidebar` 側で翻訳を挿入するとフレーム毎に辞書検索が走り、ターン進行や FPS が低下する。
- `MessageQueue.Messages` は 2000 件以上になると古いものから 100 件ずつ削除される。ContextID をログ ID と紐付けたい場合は `Translator/JpLog` で即時ログ化する。
- `RTF.FormatToRTF` 後に文字列を加工すると、リッチテキストの波括弧やエスケープが壊れやすい。

## テスト手順

1. 短時間で大量のログを出す（例: `.` でターン経過, `Ctrl+Space` で休息, 戦闘でダメージを発生）。  
   - Console: サイドバーに翻訳済みの 12 行が正しい順で表示されるか確認。  
   - Unity: `Cmd+Shift+L`（Message Log ボタン）でウィンドウを開き、日本語表示とスクロール動作を確認。
2. `Translator/JpLog` に `ContextID=MessageQueue.AddPlayerMessage.LogLine` のヒット/ミスが集計されているかを確認。
3. `Player.log` で `MessageLogWindow` 関連の例外 (`Missing glyph`, `RTF`) が出ていないか監視。
