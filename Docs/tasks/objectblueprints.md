# ObjectBlueprints / タスクボード

ObjectBlueprints 系 XML の翻訳タスクを管理します。`references/Base/ObjectBlueprints/*.xml` をベースに、`Mods/QudJP/Localization/ObjectBlueprints/*.jp.xml` を **Load="Merge"** で差し込む想定です。

## 進行状況
- [x] `ObjectBlueprints/Items.jp.xml`：Batch15～18 を `work/items_missing_batch*.json`／`_translated_batch*.json` から反映し、EatenAccomplishment～Grit Gate Grid Key までの DisplayName / Description を日本語化。
- [x] 2025-11-12 AM：Items の複合語（Laser Rifle / Force Modulator など）の中黒位置を調整し、{{C|…}} テンプレの波ダッシュを正規化。
- [x] 2025-11-12 PM：`py -3 scripts/diff_localization.py --missing-only` で BaseTierBack/Arm/Floating Tier7-8／各種 Templates (Animal/Humanoid/Arthropod/Fish)／DataDisk／Soup／BrainBrine／SunSlag Phial／StorageTank／Gourd／Entropy Cyst／Magnetic Bottle／Fungal Infection／Security Card の欠落を解消したことを確認。
- [x] `ObjectBlueprints/RootObjects.jp.xml`：CosmeticObject 系の DisplayName を `[Object]` → `[オブジェクト]` に置換。
- [x] `ObjectBlueprints/ObjectBlueprints/*.jp.xml`：ベース側に該当ディレクトリが存在しないため対象外（diff_missing = 0 を確認済み）。

## カテゴリ別チェック
- [x] `ObjectBlueprints/PhysicalPhenomena.jp.xml`（2025-11-12 diff_missing = 0）
- [x] `ObjectBlueprints/Staging.jp.xml`（2025-11-12 diff_missing = 0）
- [x] `ObjectBlueprints/TutorialStaging.jp.xml`
- [x] `ObjectBlueprints/Walls.jp.xml`（2025-11-12 diff_missing = 0）
- [x] `ObjectBlueprints/Widgets.jp.xml`（2025-11-12 diff_missing = 0）
- [x] `ObjectBlueprints/WorldTerrain.jp.xml`（2025-11-12 diff_missing = 0）
- [x] `ObjectBlueprints/ZoneTerrain.jp.xml`（2025-11-12 diff_missing = 0）

ベースに存在しない `ObjectBlueprints/ObjectBlueprints/*.xml` は diff 対象外のため、新規作成は不要。

## 手順メモ
1. `python3 scripts/diff_localization.py --missing-only` で `file-missing` / `object-missing` を洗い出し、`Docs/backlog/latest.json` に保存。
2. `references/Base/ObjectBlueprints/<ファイル名>.xml` を参照し、必要な `<object>` の DisplayName / Description / BehaviorDescription をコピーして翻訳。ラベルやタグは変更しない。
3. `Load="Merge"` + `Replace="true"` を維持しつつ、`<object Name>` はベースと同じ ID を使用する。
4. 作業後は `Docs/translation_status.md` を更新し、`python3 scripts/check_encoding.py --fail-on-issues` でエンコーディングを確認。
5. 用語の揺れは `Docs/glossary.csv` で統一し、UI 影響がある場合は `Docs/test_plan.md` のシナリオで確認。
6. **派生ステータス（例: FAV / DV / HP / Wait）等は調整後に必ずゲーム内で再確認し、必要に応じて Framework 側パッチのみを更新する。**
