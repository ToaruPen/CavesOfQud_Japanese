# UI タスクボード

Options / EmbarkModules / Menus など UI 系 XML の未訳タスクを管理します。  
`python3 scripts/diff_localization.py --missing-only` の出力で UI ファイルが挙がったら、このファイルに反映してください。

## 未訳 / 対応中
- [ ] `Options.jp.xml` : Legacy UI → Misc Prompts のプレースホルダー文を最終レビューする。
- [ ] `EmbarkModules.jp.xml` : Mutations / Cybernetics 内の各エントリ（個別説明）は今後の ObjectBlueprints タスクにリンクすること。
- [x] `Genotypes.jp.xml` / `Subtypes.jp.xml` : キャラ作成画面のジェノタイプ + カースト／天職の名称・説明を日本語化（DisplayName 付与、stat 文言訳出）。
- [ ] `Manual.jp.xml` : クイックスタート／能力値／スキル／突然変異／アーティファクト／トニックを追加済み。残りのヘルプトピックも同じ方針で翻訳する。

## 完了見込み
- [ ] UI 同期後に `python3 scripts/sync_mod.py` を実行 → ゲーム内で UI が崩れていないか確認。

## メモ
- UI の個別要素（ジェノタイプの説明など）は ObjectBlueprints 側を参照。ここではウィンドウや説明枠のみ取り扱う。
