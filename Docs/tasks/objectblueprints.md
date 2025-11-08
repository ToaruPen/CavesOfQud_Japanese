# ObjectBlueprints / タスクボード

ObjectBlueprints 配下の翻訳状況を管理します。`references/Base/ObjectBlueprints/*.xml` をベースに、`Mods/QudJP/Localization/ObjectBlueprints/*.jp.xml` を `Load="Merge"` 形式で作成してください。

## 進行サマリ
- [x] `ObjectBlueprints/Items.jp.xml` … サイバネティクス装備と信用楔を訳出済み。**通常アイテム（武器、防具、設置物など）は未訳**なので diff を見ながら追加する。
- [x] `ObjectBlueprints/RootObjects.jp.xml` … CosmeticObject の DisplayName を `[オブジェクト]` に差し替え。
- [ ] `ObjectBlueprints/ObjectBlueprints/*.jp.xml` … `references/Base/ObjectBlueprints/ObjectBlueprints/` 以下のメタ定義はすべて `file-missing`。

## 未訳ファイル
- [ ] `ObjectBlueprints/Creatures.jp.xml`
- [ ] `ObjectBlueprints/Data.jp.xml`
- [ ] `ObjectBlueprints/Foods.jp.xml`
- [ ] `ObjectBlueprints/Furniture.jp.xml`
- [ ] `ObjectBlueprints/HiddenObjects.jp.xml`
- [ ] `ObjectBlueprints/PhysicalPhenomena.jp.xml`
- [ ] `ObjectBlueprints/Staging.jp.xml`
- [ ] `ObjectBlueprints/TutorialStaging.jp.xml`
- [ ] `ObjectBlueprints/Walls.jp.xml`
- [ ] `ObjectBlueprints/Widgets.jp.xml`
- [ ] `ObjectBlueprints/WorldTerrain.jp.xml`
- [ ] `ObjectBlueprints/ZoneTerrain.jp.xml`

同じ並びで `ObjectBlueprints/ObjectBlueprints/*.xml` もすべて未作成なので、上記と並行して対応する。

## 進め方メモ
1. `scripts/diff_localization.ps1 -MissingOnly` で `file-missing` / `object-missing` を洗い出す。`Docs/backlog/latest.json` にも同じ情報を保存しておく。
2. 1 ファイルずつ `references/Base/ObjectBlueprints/<ファイル名>.xml` をコピーし、DisplayName / Description / BehaviorDescription など文字列のみを翻訳。数値・タグ構造は変更しない。
3. `Load="Merge" Replace="true"` 方針を維持し、`<object Name>` はベースと全く同じにする。
4. 翻訳後はこのタスクボードと `Docs/translation_status.md` を更新し、`scripts/check_encoding.ps1 -FailOnIssues` でモジバケを検知。
5. 必要に応じて `Docs/glossary.csv` に用語を追記し、UI 表示は `Docs/test_plan.md` のシナリオで検証する。
