# 翻訳フック実装計画 v3

Docs/pipelines/*.md と pipelines.csv を基盤に、Harmony フックをどこへ・どの粒度で入れるかを整理した計画書。すべてをゲーム実装と一対一で縛る必要はなく、**揺れやすい要所だけをコード契約でロックし、その他は仕様ドリブン（ハブ優先）で進める**方針を徹底する。

---

## 0. ゴール / 成功基準
- 各パイプラインで **生産 → 整形 → 描画** のどこにフックを置くかを明示し、Transform / RTF / Clip の直前で翻訳を確定。
- ContextID 規約に沿って辞書キーを決め、`Translator` + `JpLog` でヒット/Miss を常時観測。
- Popup / Tooltip / Log / Journal / Inventory など主要 UI が破綻していないことを QA とゴールデンスナップショットで確認。

---

## 1. 契約レベル（強く縛るべき要所）
以下はソース更新で最も壊れやすいため、コード契約を張る。契約 = 署名監視・順序アサート・HookGuard・テストスナップショット等を指す。

| 区分 | 具体例 / 対応 | 目的 |
| --- | --- | --- |
| Transform/RTF/Clip 直前 | `MessageQueue.AddPlayerMessage`, `Popup.ShowBlock`, `Look.GenerateTooltipInformation` | ここで翻訳を確定させる。Harmony ターゲットのメタデータトークンを `hooks.lock.json` へ書き出し、起動時に照合。差異があれば警告＋該当フックを自動無効化。 |
| 非同期境界 (gameQueue→uiQueue) | `Look.GenerateTooltipContent` → `TooltipTrigger.SetText` | gameQueue 側で完成させた文字列だけを UI へ渡す契約を設置。HookGuard（`[ThreadStatic] HashSet<int>`）で葉側の二重翻訳を抑止。 |
| 長文ブロック | `BookInfo.*`, `JournalScreen.UpdateEntries`, `WorldCreationProgress.*` | 折り返し幅・段落構造を守るため、データ層で翻訳して UI 層では再整形しない。ゴールデンスナップショットを保存し、レイアウト差分を CI で比較。 |
| 共通名称 (DisplayName) | `GameObject.GetDisplayName`, `InventoryLineData.displayName` | DisplayName を唯一のハブにし、UI の setData ではラベル・数値だけを編集。JpLog で MISS が出たらここを優先的に埋める。 |

上記以外（単純な `…setData`, `UITextSkin.SetText` など）は仕様書＋テストで十分管理する。必要なら HookGuard を併用しつつ段階的に撤去する。

### 自動チェック
1. **メタデータ監視**: Harmony ターゲットのシグネチャを lockfile に書き出し、起動時に実アセンブリと比較。ズレたら鳴らす。
2. **順序アサート**: デバッグビルドでは Transform 前後にフラグを置き、「Translate→Transform→RTF→Clip」の順序が守られているかを軽量トレースで検証。
3. **HookGuard**: `[ThreadStatic] HashSet<int>` で同フレーム・同原文への多重 Prefix を禁止。gameQueue/ uiQueue 双方で利用。
4. **ゴールデンスナップショット**: Popup/Tooltip/Log/Journal/Inventory の原文→訳文→描画直前テキストをテストモードで書き出し、差分を自動比較。

### 1.6 解析ディレクトリ
ゲーム本体（Caves of Qud）の実装を照合する際は、次の ILSpy 展開ディレクトリを参照する。

- `..\CavesOfQud_Japanese.ilspy_extracted\Assembly-CSharp\` … `XRL.*`, `Qud.UI.*`, `ModelShark.*` など主要アセンブリ
- `..\CavesOfQud_Japanese.ilspy_extracted\Unity.TextMeshPro\` / `UnityEngine.UI\` … TMP/Unity UI 周辺

※リポジトリ直下から一階層上に作られているため、`Docs/pipelines/*.md` 等でソース参照が必要な場合は上記パスを利用する。

---

## 2. 優先フック一覧（Contracts + Spec）
| 優先度 | サブシステム | 推奨フック / ContextID | 技術メモ |
| --- | --- | --- | --- |
| ☁☁☁ | Popup (Console/Unity) | `XRL.UI.Popup.ShowBlock.Message`, `Qud.UI.PopupMessage.ShowPopup.Message` | Console/Modern を同じハブで翻訳。Transform 前 Prefix。 |
| ☁☁☁ | Tooltip (Look / TooltipTrigger) | `XRL.UI.Look.GenerateTooltipInformation.DisplayName`, `Look.GenerateTooltipContent.Body`（gameQueue） | 非同期境界を守る。UI 側では再翻訳しない。 |
| ☁☁☁ | MessageLog | `XRL.Messages.MessageQueue.AddPlayerMessage.LogLine` | Console/Modern 共通。Transform 前で確定。 |
| ☁☁☁ | Journal / Book | `XRL.UI.BookUI.HandlePageNode.PageText`, `JournalScreen.UpdateEntries.EntryLine` | 折り返し幅 75/80 をデータ層で維持。 |
| ☁☁☁ | WorldGeneration | `XRL.UI.WorldCreationProgress.NextStep/StepProgress`, `Qud.UI.WorldGenerationScreen._AddMessage` | Console/Modern のロード文言を一括管理。 |
| ☁☁☁ | Inventory / Equipment | `XRL.World.GameObject.GetDisplayName`, `InventoryLineData.set`（カテゴリ名・重量のみ） | DisplayName をハブにし、UI ではラベル置換だけ行う。 |
| ☁☁ | Help / Options / Keybinds | `Options.LoadOptionNode.DisplayText`, `Qud.UI.OptionsRow.setData` など | テキスト層で確定、UI ではホットキー描画のみ。 |
| ☁☁ | Conversation | `IConversationElement.GetDisplayText`, `Popup.ShowConversation.Body` | 選択肢・本文をデータ層で確定。 |
| ☁☁ | Trade / Factions / Status | 各 `…LineData` ハブ | ハブ優先で setData を単純化。 |

（詳細なパイプライン／ContextID は Docs/pipelines/*.md を参照）

---

## 3. フェーズ別ロードマップ
1. **Phase A**: Popup / Tooltip / MessageLog
2. **Phase B**: Journal / Inventory / Conversation
3. **Phase C**: Trade / Factions / CharacterBuild / Abilities
4. **Phase D**: Help / Options / Keybinds / WorldGeneration
5. **Freeze**: backlog と JpLog を確認し、未対応 Context を棚卸してリリース候補を作成。

各フェーズ完了時に pipelines.csv / backlog / JpLog を更新し、ゴールデンスナップショットを取り直す。

---

## 4. テスト計画
- **自動**: `QudJP.Tests`（NormalizeKey, Translator, HookGuard, Contract 監視） + ゴールデンスナップショット比較。
- **手動**: Popup, Tooltip, MessageLog, Journal, Inventory, Conversation, WorldGeneration など代表画面をチェックリスト化。
- `JpLog` を定期確認し、MISS が多い Context から辞書補完 → 再テストのループを回す。

---

## 5. リスクと対策
- Console/Unity 差異 → ハブで確定＋HookGuard。
- Transform 後に再翻訳 → 契約で禁止（順序アサート + lockfile）。
- 長文崩れ → データ層で確定し、ゴールデンスナップショットで検知。
- 性能悪化 → Translator キャッシュ + HookGuard で多重呼び出しを抑制。

---

## 6. 運用メモ
- 新フックを実装したら必ず pipelines.csv に ContextID と注意点を追記。
- `Docs/pipelines/*.md` のパイプライン図を最新化し、作業者が“どこで翻訳を確定すべきか”を一目で把握できるようにする。
- Backlog と JpLog を週次で見直し、揺れやすい要所（契約対象）から優先的に処理する。
