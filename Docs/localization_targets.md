# ローカライズ対象ファイル一覧

このドキュメントは「どのテキストがどのファイルに格納され、どこへ挿入するのか」をカテゴリ別にまとめた対応表です。  
翻訳を進める前に、ここで対象とするソース（`references/Base`）と出力先（`Mods/QudJP/Localization` や派生フォルダ）を確認してください。

## 1. 基本 XML カテゴリ

| カテゴリ | ベースデータ (`references/Base`) | ローカライズ先 (`Mods/QudJP/Localization`) | 備考 / ステータス |
| --- | --- | --- | --- |
| 会話 / NPC | `Conversations.xml` | `Conversations.jp.xml` | Joppa 以外も含む全 NPC の台詞。Water Ritual や Slynth などの共有ノードもここを差し替える。進行中。 |
| 書籍 (Books) | `Books.xml` | `Books.jp.xml` | 書籍メタデータと短文。長文は Corpus セクションを参照。未訳多数。 |
| コマンド / Tinkering UI | `Commands.xml` | `Commands.jp.xml` | 行動ログやスキルコマンド、クラフト UI など。 |
| オプション / メニュー | `Options.xml` | `Options.jp.xml` | タイトルメニュー・設定画面・プロンプト。**Load="Merge" のまま**テキストだけ更新。 |
| Embark Modules (UI) | `EmbarkModules.xml` | `EmbarkModules.jp.xml` | キャラ作成ウィンドウ。`<module>` / `<window>` は **Load="Replace"** 指定で丸ごと翻訳。 |
| Genotypes | *ゲーム側 XML（XmlDataHelper 管理）* | `Genotypes.jp.xml` | キャラ作成ジェノタイプの名前と説明。XmlDataHelper が `Replace="true"` を解釈しないので、要素側に `Load="Replace"` を設定し構造を完全一致させる。 |
| Subtypes | *同上* | `Subtypes.jp.xml` | カースト / 天職の説明。ルールは Genotypes と同じ。 |
| Mutations | `Mutations.xml` | `Mutations.jp.xml` | 突然変異カテゴリ。XmlDataHelper 対応ファイルなので `<category>` などを `Load="Replace"` にして要素構造をコピーする。 |
| Manual | `Manual.xml` | `Manual.jp.xml` | ゲーム内マニュアルの XML 側スタブ。実際の本文は Harmony 用の `Docs/manual/ManualPatch.jp.manual`（別配布ソース）を参照。 |

## 2. ObjectBlueprints

すべて `references/Base/ObjectBlueprints/*.xml` をモジュールごとに `Mods/QudJP/Localization/ObjectBlueprints/*.jp.xml` へ `Load="Merge"` で差し込む。  
現在リポジトリに存在する／未作成のファイルは以下のとおり。

| サブカテゴリ | ベースファイル | ローカライズ先 | 状態 |
| --- | --- | --- | --- |
| Items | `ObjectBlueprints/Items.xml` | `Localization/ObjectBlueprints/Items.jp.xml` | 既存。カテゴリ単位で順次翻訳中。 |
| RootObjects | `ObjectBlueprints/RootObjects.xml` | `Localization/ObjectBlueprints/RootObjects.jp.xml` | 既存。CosmeticObject などの DisplayName を上書き済み。 |
| Creatures | `ObjectBlueprints/Creatures.xml` | *未作成*（`Localization/ObjectBlueprints/Creatures.jp.xml` を追加予定） | 未訳。 |
| Data | `ObjectBlueprints/Data.xml` | *未作成* | 未訳。 |
| Foods | `ObjectBlueprints/Foods.xml` | *未作成* | 未訳。 |
| Furniture | `ObjectBlueprints/Furniture.xml` | *未作成* | 未訳。 |
| HiddenObjects | `ObjectBlueprints/HiddenObjects.xml` | *未作成* | 未訳。 |
| PhysicalPhenomena | `ObjectBlueprints/PhysicalPhenomena.xml` | *未作成* | 未訳。 |
| Staging / TutorialStaging | `ObjectBlueprints/Staging.xml` / `TutorialStaging.xml` | *未作成* | 未訳。 |
| Walls | `ObjectBlueprints/Walls.xml` | *未作成* | 未訳。 |
| Widgets | `ObjectBlueprints/Widgets.xml` | *未作成* | 未訳。 |
| WorldTerrain | `ObjectBlueprints/WorldTerrain.xml` | *未作成* | 未訳。 |
| ZoneTerrain | `ObjectBlueprints/ZoneTerrain.xml` | *未作成* | 未訳。 |

> **LLM パイプライン:** `scripts/objectblueprint_extract.py` / `objectblueprint_insert.py` を利用する場合は、`Docs/tasks/objectblueprints_llm.md` を必ず参照して抽出→翻訳→再挿入の契約を守ること。

## 3. Corpus / 書籍テキスト

| カテゴリ | ベース (`references/Base/Corpus/…`) | ローカライズ先 | 備考 |
| --- | --- | --- | --- |
| 長文書籍 / 詩 / ロア | `Corpus/*.txt` | `Mods/QudJP/Localization/Corpus/*.jp.txt`（フォルダーを新規作成） | まだリポジトリに日本語ファイルはないので、翻訳ごとに `.jp.txt` を追加する。`Docs/tasks/books.md` のタスクリスト参照。 |

## 4. その他

- **Commands 以外のログメッセージ** … 多くは `Commands.xml` で扱うが、Harmony 側で生成するログについては C# プロジェクト (`Mods/QudJP/Assemblies`) にも翻訳文字列が存在する。対象コードを編集する場合は C# 側のリソースも確認すること。
- **Harmony / ManualPatch** … `Docs/manual/ManualPatch.jp.manual`（Harmony パッチ経由）に実体があり、`Localization/Manual.jp.xml` には空スタブを残す。XML 側に本文を戻すと重複登録になるため注意。
- **実機同期** … すべての翻訳ファイルは `Mods/QudJP` がソース。`python3 scripts/sync_mod.py` で `%USERPROFILE%\AppData\LocalLow\Freehold Games\CavesOfQud\Mods\QudJP` へミラーするまでゲーム側には反映されない。

## 5. 参照リンク

- タスクボード: `Docs/tasks/*.md`
- 進捗サマリ: `Docs/translation_status.md`
- フォント・UI 方針: `Docs/font_pipeline.md`
- エンコード / ログ監視: `Docs/utf8_safety.md`, `Docs/log_watching.md`

この表にないカテゴリを作業したくなった場合は、まず `references/Base` でソースを確認し、ここに追記してからチームへ共有してください。
