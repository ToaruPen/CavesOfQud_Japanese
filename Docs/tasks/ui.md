# UI タスクボード

Options / EmbarkModules / Menus など UI 系 XML の未訳タスクを管理します。  
`python3 scripts/diff_localization.py --missing-only` の出力で UI ファイルが挙がったら、このファイルに反映してください。

## 未訳 / 対応中
- [ ] `Options.jp.xml` : Legacy UI → Misc Prompts のプレースホルダー文を最終レビューする。
- [ ] `EmbarkModules.jp.xml` : Mutations / Cybernetics 内の各エントリ（個別説明）は今後の ObjectBlueprints タスクにリンクすること。
- [x] `Genotypes.jp.xml` / `Subtypes.jp.xml` : キャラ作成画面のジェノタイプ + カースト／天職の名称・説明を日本語化（DisplayName 付与、stat 文言訳出）。
- [ ] `Manual.jp.xml` : クイックスタート／能力値（耐性・温度まで拡張）／スキル／突然変異／アーティファクト／トニック／武器と防具／戦闘／セーヴィングスロー／アクションコストを追加済み。残るトピックも同じ方針で翻訳する。

## 完了見込み
- [ ] UI 同期後に `python3 scripts/sync_mod.py` を実行 → ゲーム内で UI が崩れていないか確認。

## メモ
- UI の個別要素（ジェノタイプの説明など）は ObjectBlueprints 側を参照。ここではウィンドウや説明枠のみ取り扱う。
- `Options.jp.xml` はファイル丸ごと置き換えのみサポートされているようで、`Load="Merge"` や `Replace="true"` を付けると Player.log に `Unused attribute` 警告が出る。ベースと同じ構造を保ったまま DisplayText などの値だけを更新する。
- `Docs/ManualPatch.jp.xml`（旧 `Localization/Manual.jp.xml`）は Harmony パッチ専用のデータソース。ゲームの XML ローダーに渡すと `Quickstart` などが二重登録されてクラッシュするので、`Localization/Manual.jp.xml` には空スタブを置いたままにすること。
