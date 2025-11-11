# Popup パイプライン仕様 v1

> **画面 / 部位:** 即時ポップアップ（警告 / ダイアログ）。  
> **出力:** Console（クラシック UI） / Unity（Modern UI）

## 概要

- `Popup.Show*` 系 API が呼ばれるたびに **コンソール描画**と**Unity Window (`PopupMessage`)** のどちらかを選択する。
- 文字列は Markup (`{{K|...}}`) を保持したまま `Markup.Transform` → `StringFormat.ClipTextToArray` → `ScreenBuffer.Write`（Console）もしくは `UITextSkin.SetText`（Unity）に渡される。
- 推奨 ContextID 例: `XRL.UI.Popup.ShowBlock.Message`, `Qud.UI.PopupMessage.ShowPopup.Title`, `Qud.UI.PopupMessage.ShowPopup.ButtonLabel`.

## 主なクラス / メソッド

| フェーズ | クラス | メソッド / 備考 |
| --- | --- | --- |
| 生成 | `XRL.UI.Popup` | `Show`, `ShowAsync`, `ShowBlock`, `ShowSpace` などが全文を受け取る。 |
| フォールバック判定 | `XRL.UI.UIManager` | `UseNewPopups` フラグで Console/Unity の枝を決定。 |
| 整形 (Console) | `XRL.UI.Popup` | `ShowBlock` → `Markup.Transform` → `TextBlock`（内部で `StringFormat.ClipTextToArray` 幅 78）。 |
| 描画 (Console) | `ConsoleLib.Console.ScreenBuffer` | `RenderBlock` がボックス／スクロール表示、`ScreenBuffer.Write` で出力。 |
| 整形 (Unity) | `XRL.UI.Popup` | `WaitNewPopupMessage` が `uiQueue` へデリゲートを送る。 |
| 描画 (Unity) | `Qud.UI.PopupMessage` | `ShowPopup` → `Message.SetText`（TMP RichText）、`controller.UpdateElements` でボタン更新。 |

## データフロー

1. ゲームロジックが `Popup.Show("You are famished!")` を呼ぶ。
2. `Show` (sync) で `Markup.Transform` / `ColorUtility.CapitalizeExceptFormatting` を適用。
3. `UIManager.UseNewPopups` が `false` なら `ShowBlock` → `RenderBlock` → `TextBlock(..., width=78)` → `ScreenBuffer`。
4. `UIManager.UseNewPopups` が `true` なら `WaitNewPopupMessage` が `uiQueue` に `PopupMessage.ShowPopup` をスケジュール。
5. `ShowPopup` が `Message.SetText("{{y|"+message+"}}")` とタイトル / コンテキスト画像 / ボタンを組み立て、`UITextSkin` → TMP へレンダリング。

## 整形規則

- Console: `TextBlock` が `StringFormat.ClipTextToArray`（幅 72 + Padding）→ 行数に応じスクロール (`<up/down for more...>` 表示)。
- Unity: `Message.SetText` は TMP RichText を受け付ける。`PopupMessage.Message` の `LayoutElement.minWidth` は本文幅に合わせて更新される。
- `WaitNewPopupMessage` で `EscapeNonMarkupFormatting=true` の場合、`&` `^` を `&&` `^^` に逃がす。翻訳文字列でこれら記号を追加する際は注意。
- ボタン行 (`QudMenuItem.text`) も Markup (`{{W|[y]}} {{y|Yes}}`) を保持したまま `controller.UpdateButtonLayout` が再計測する。

## 同期性

- `Show*` はゲームスレッド（`sync`）で実行。Console 分岐はそのまま描画。
- Unity 分岐は `WaitNewPopupMessage` → `GameManager.Instance.uiQueue` → `PopupMessage.ShowPopup`。  
  翻訳を挿入する際は **gameQueue から uiQueue へ手動で渡さない**（WaitNewPopupMessage がすでに橋渡し済み）。
- ボタン押下や入力フィールド完了は `uiQueue` から `TaskCompletionSource` でゲームスレッドに結果を返す。

## 置換安全点（推奨フック）

- `Harmony Prefix: XRL.UI.Popup.ShowBlock`  
  - ContextID: `XRL.UI.Popup.ShowBlock.Message` / `.Title`.  
  - 理由: `Markup.Transform` 直前なので Markup を維持したまま翻訳可。Console/Unity 共通でログ登録文字列もここで確定する。
- `Harmony Prefix: XRL.UI.Popup.WaitNewPopupMessage`  
  - ContextID: `XRL.UI.Popup.WaitNewPopupMessage.Message`.  
  - 理由: Unity 専用ブランチ。`EscapeNonMarkupFormatting` の処理前に翻訳すれば `PopupMessage` 以降に副作用が出ない。
- `Harmony Prefix: Qud.UI.PopupMessage.ShowPopup`（最終防衛）  
  - `UITextSkin` に渡す前に `Message` / `Title` / `buttons[i].text` を書き換えられる。  
  - ただし `ShowPopup` は `uiQueue` 専用のため、辞書アクセスに `ConcurrentDictionary` などを使う。

## 例文 / トークン

- `"{{R|You are bleeding!}}"` – 色タグは維持。  
- `"{{Y|%subject}} {{r|stares blankly.}}"` – Grammar 展開後の結果が渡るので `%subject` など動的トークンは原文側で正規化してキー化する。
- `PopupMessage.SingleButton[0].text = "{{W|[space]}} {{y|Continue}}"` – ボタン用 ContextID `Qud.UI.PopupMessage.ShowPopup.ButtonLabel`.

## リスク

- `Markup.Transform` 後の文字列にさらに色タグを入れると **二重 Transform** になり `{{` が崩れる。
- Console 分岐では `StringFormat.ClipTextToArray` の幅予測が厳しい。全角 2 文字扱いのため、翻訳で長文化するとスクロール頻度が上がる。
- Unity 分岐で `Message.SetText` に改行を大量に入れると `LayoutElement.minWidth` の再計算が暴れ、表示揺れが起きる。

## テスト手順

1. **Popup**: ゲーム内で `Ctrl+Space`（チュートリアルメッセージ）や `Esc`（確認ダイアログ）を発生させる。  
   - Console モード: `options.json` で `ModernUI=false` にしてから `ShowBlock` が使われるケースを確認。  
   - Unity モード: `ModernUI=true` で `PopupMessage` が正しく翻訳されているか、ボタン配置崩れがないかを確認。
2. `Translator/JpLog` で `ContextID=XRL.UI.Popup.ShowBlock.Message` のヒット率を監視する（ヒットしない場合はキー正規化を見直す）。
3. `Player.log` に `Missing glyph in font asset` / `TMPro` エラーが出ていないかをチェック。
