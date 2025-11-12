# パイプライン資料の書き方

ローカライズ Hook の設計では **ILSpy などでパスを段階的に調査し、確認できた範囲だけをドキュメント化する** 方針を徹底する。未調査部分を「不要」と断定しない。保留する場合は必ずチームへ共有し、`TODO` や Issue で追跡する。

## エンコーディングポリシー
- `Docs/pipelines` 配下は **UTF-8 (BOM 無し)** で保存する。PowerShell / メモ帳 / Excel など CP932 前提のアプリで上書きしない。
- 文字化けを見付けた場合は該当ファイルを UTF-8 で再作成し、PR では `scripts/check_encoding.py` のログを貼って再発防止策を明示する。
- 各ファイルの冒頭に Encoding 注意書きを重ねて書く必要はないが、重要な節の導入で再度触れるのは歓迎。

## 調査手順
1. **対象の UI / サブシステムを選定**（例: Popup → Tooltip → MessageLog → Journal / History …）。
2. ILSpy データ（`./.ilspy/Assembly-CSharp`, `Unity.*`, `TextMeshPro` など）で関連クラス/メソッドを検索し、呼び出し関係を把握する。
3. 確認できた範囲を `Docs/pipelines/<name>.md` に追記し、ContextID / Threading / Hook 点 / QA などを整理する。
4. `Docs/pipelines.csv` に同じ情報（サブシステム、クラス、メソッド、ContextID、注意点）を必ず追加する。差分が出たら csv を最新化してから PR を送る。
5. 調査中にブロッカーを見付けたら **速やかに相談** し、ドキュメント上にも「未解決」「確認中」と明記する。

## 現在のカバレッジ（一部抜粋）
| Subsystem | 主要ソース | ドキュメント |
| --- | --- | --- |
| Popup | `XRL.UI.Popup`, `Qud.UI.PopupMessage`, `ConsoleLib.Console` | `Docs/pipelines/popup.md` |
| Tooltip | `XRL.UI.Look`, `ModelShark.TooltipTrigger`, `Sidebar.FormatToRTF` | `Docs/pipelines/tooltip.md` |
| Inventory / Equipment | `XRL.UI.InventoryScreen`, `Qud.UI.InventoryAndEquipmentStatusScreen`, `UITextSkin` | `Docs/pipelines/inventory.md` |
| World Generation | `Qud.UI.WorldGenerationScreen`, `XRL.UI.WorldCreationProgress` | `Docs/pipelines/worldgeneration.md` |
| Message Log | `XRL.Messages.MessageQueue`, `Qud.UI.MessageLogWindow` | `Docs/pipelines/messagelog.md` |
| Conversation | `XRL.UI.ConversationUI`, `Popup.ShowConversation` | `Docs/pipelines/conversation.md` |
| Trade | `XRL.UI.TradeUI`, `Qud.UI.TradeScreen` | `Docs/pipelines/trade.md` |
| Character Build | `Qud.*ModuleWindow`, `ChoiceWithColorIcon` | `Docs/pipelines/characterbuild.md` |
| Skills & Powers | `Qud.UI.SkillsAndPowersStatusScreen`, `Qud.UI.SkillsAndPowersLine` | `Docs/pipelines/skillsandpowers.md` |
| Help / Options / Keybinds | `Qud.UI.*Screen`, `CommandBindingManager` | `Docs/pipelines/help_options_keybinds.md` |

※ すべての行が最新状態とは限らない。更新したら必ず該当 Markdown / `pipelines.csv` を同時に修正すること。

## 直近の優先度
1. **未文書化の UI を分担して洗い出す**  
   - 例: Game Summary / High Scores / Save 管理 / 端末 UI など。担当者と期日を `Docs/pipelines/backlog.md` に追記。
2. **Hook 契約の整合性チェック**  
   - `pipelines.csv` と各 Markdown の記述が一致しているかを確認し、Harmony パッチの実装と差異があれば修正。
3. **ログ監視・QA の自動化**  
   - `tmp/player_log_watch.txt` に追加したキーワードを文書化し、`Docs/log_watching.md` で共有する。

## 書き方のヒント
- 「どのスレッドで実行されるか」「翻訳をどこで完了させるか」「TMP/TextBlock などの整形関数は何か」を最低限まとめる。
- Hook を貼るメソッドには ContextID を割り当て、`Translator` のキーと一致させる。
- QA セクションに代表的なケース（成功例 / 落とし穴）を載せる。実測ログやスクリーンショットがあればリンクを張る。
- 文章は簡潔に。必要なら表や番号付きリストを活用し、将来のメンテナが読み返してすぐ分かる形にする。
