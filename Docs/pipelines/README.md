# パイプライン調査フローと進捗

今後の翻訳フック設計では **ILSpy から段階的に該当パスを読み込み、確認できた範囲だけドキュメント化する** 方針を徹底します。網羅確認が取れていない箇所を「不要」と判断せず、見送る場合は必ずユーザーへ相談します。

## 調査手順
1. **対象の UI / サブシステムを選定**（例: Popup → Tooltip → MessageLog → Journal/History …）。
2. ILSpy ディレクトリ（`./.ilspy/Assembly-CSharp`, `Unity.*`, `TextCore` など）で関連クラス / メソッドを検索し、呼び出し関係を把握。
3. 参照し終えた範囲をここに記録し、`Docs/pipelines/<name>.md` を更新。
4. 次に読む予定のサブシステムを明示し、読了後に「完了」へ移す。
5. 本当に除外すると決める場合は **必ずユーザーに確認** し、その旨をドキュメントに記載。

## 現在の読了状況（2025-11-11）
| サブシステム | 参照した ILSpy ソース | 状態 | 備考 |
| --- | --- | --- | --- |
| Popup | `XRL.UI.Popup`, `Qud.UI.PopupMessage`, `ConsoleLib.Console` (ScreenBuffer / TextBlock) | ✅ 完了 | Markup → Clip → ScreenBuffer/TMP まで把握済み。`Docs/pipelines/popup.md` 参照。 |
| Tooltip | `XRL.UI.Look`, `ModelShark.TooltipTrigger`, `Sidebar.FormatToRTF`, `RTF.FormatToRTF` | ✅ 完了 | Look/Inventory hover 経路を確認済み。`Docs/pipelines/tooltip.md` 参照。 |
| MessageLog | `XRL.Messages.MessageQueue`, `XRL.UI.Sidebar`, `Qud.UI.MessageLogWindow` | ✅ 完了 | Console/Unity 双方の描画パスを整理済み。 |
| Journal / History | `XRL.UI.JournalScreen`, `XRL.UI.BookUI`, `HistoryKit.HistoricStringExpander` | ✅ 完了 | StringFormat.ClipTextToArray (幅75) と BookUI の自動段組を調査し、`Docs/pipelines/journal.md` に反映。 |
| Inventory / Equipment | `XRL.UI.InventoryScreen`, `Qud.UI.InventoryAndEquipmentStatusScreen`, `InventoryLine`, `UITextSkin` | ✅ 完了 | Console/Unity 両 UI のリスト生成・重量表示・TMP 変換を調査済み。`Docs/pipelines/inventory.md` を参照。 |
| Factions / Reputation | `XRL.UI.FactionsScreen`, `Qud.UI.FactionsStatusScreen`, `FactionsLine` | ✅ 完了 | Console/Unity 両レピュテーション画面の整形・詳細テキスト生成を調査済み。`Docs/pipelines/factions.md` を参照。 |
| Trade / Shop | `XRL.UI.TradeUI`, `Qud.UI.TradeScreen`, `TradeLine` | ✅ 完了 | Console/Modern のバーター画面、価格計算、Vendor Actions を調査し `Docs/pipelines/trade.md` に反映。 |
| CharacterBuild / Embark | `XRL.CharacterBuilds.UI.EmbarkBuilderOverlayWindow`, `Qud*ModuleWindow`, `ChoiceWithColorIcon` | ✅ 完了 | Embark Builder のモジュール／データソース／UITextSkin 処理を調査済み。`Docs/pipelines/characterbuild.md` を参照。 |
| Skills & Powers Status | `Qud.UI.SkillsAndPowersStatusScreen`, `Qud.UI.SkillsAndPowersLine`, `XRL.UI.SPNode`, `XRL.UI.SkillsAndPowersScreen` | ✅ 完了 | Modern Skills/Power ツリー（検索・詳細パネル含む）を調査し `Docs/pipelines/skillsandpowers.md` を追加。 |
| Tinkering / Cybernetics | `Qud.UI.TinkeringStatusScreen`, `TinkeringLine*`, `CyberneticsTerminalScreen`, `TerminalScreen` | ✅ 完了 | Build/Mod UI + bit一覧、Cybernetics 端末のテキスト生成と入力フローを整理。`Docs/pipelines/tinkering.md` を参照。 |
| Ability Bar / Manager | `Qud.UI.AbilityBar`, `AbilityBarButton`, `Qud.UI.AbilityManagerScreen`, `AbilityManagerLine` | ✅ 完了 | HUD ホットバーと AbilityManager の検索/並び替え/再バインド処理を調査し `Docs/pipelines/abilities.md` を追加。 |
| Game Summary / High Scores / Save | `Qud.UI.GameSummaryScreen`, `HighScoresScreen`, `HighScoresRow`, `SaveManagement`, `SaveManagementRow` | ✅ 完了 | エンディング画面・ハイスコア一覧・続きから UI のパイプラインを整理し `Docs/pipelines/gameend.md` に記載。 |
| Help / Options / Keybinds | `Qud.UI.HelpScreen`, `Qud.UI.OptionsScreen`, `Qud.UI.KeybindsScreen`, `XRL.Help.XRLManual`, `XRL.UI.OptionsUI`, `XRL.UI.KeyMappingUI`, `CommandBindingManager` | ✅ 完了 | Embark overlay 系 UI（ホットキー／検索バー／カテゴリスコープ）とコンソール UI を横断して調査し、`Docs/pipelines/help_options_keybinds.md` にデータ層＋UI層のフック案を追加。 |
| WorldGeneration / Creation Progress | `Qud.UI.WorldGenerationScreen`, `XRL.UI.WorldCreationProgress`, `XRL.CharacterBuilds.Qud.QudGameBootModule`, `XRL.World.WorldFactory` | ✅ 完了 | 世界生成ログとグラフ（コンソール／Modern 共有）を `Docs/pipelines/worldgeneration.md` に整理。`NextStep`/`StepProgress` フックで双方を同時に翻訳する計画を追記。 |
| Quests / Book (Modern) | `Qud.UI.QuestsStatusScreen`, `Qud.UI.QuestsLine`, `MapScrollerController`, `Qud.UI.BookScreen` | ✅ 完了 | クエストタブ + BookScreen（モダン書籍ビュー）のリスト生成・検索・ミニマップを整理。`Docs/pipelines/quests.md` を参照。 |
| 会話分岐 / Dialogue | `XRL.UI.ConversationUI`, `Popup.ShowConversation`, `XRL.World.Conversations.*` | ✅ 完了 | Console/Modern 双方の会話表示、Choice/Node テキスト生成を調査。`Docs/pipelines/conversation.md` を参照。 |

## 次アクション
1. **残りの高優先 UI の洗い出し**
   - これまでの調査で抜けている UI（例: Build Summary、特別な Overlay など）があれば列挙し、必要に応じて追加調査。
2. **翻訳フックの実装準備**
   - `pipelines.csv` をもとにフック候補を優先順位付けし、Harmony などでの実装計画をまとめる。
3. その他サブシステムを除外する場合は、必ずユーザーへ確認してから記載。
