# UI タスクボード

Options / EmbarkModules / Menus など UI 系 XML の未訳タスクを管理します。
python3 scripts/diff_localization.py --missing-only の出力で UI ファイルが挙がったら、このファイルに反映してください。

## 未訳 / 対応中
- [ ] Options.jp.xml: Legacy UI と Misc Prompts のプレースホルダーを最終レビューする。
- [ ] EmbarkModules.jp.xml: Mutations / Cybernetics の個別エントリ（説明）を今後の ObjectBlueprints タスクにリンクする。
- [x] Genotypes.jp.xml / Subtypes.jp.xml: キャラ作成画面のジェノタイプ + カースト／天職の名称・説明を日本語化し、DisplayName とステータス訳を付与する。
- [ ] Manual.jp.xml: クイックスタート／能力値／耐性・温度／スキル／突然変異／アーティファクト／トニック／武器と防具／戦闘／セーヴィングスロー／アクションコストを追加済み。残るトピックも同じ方針で翻訳する。

## 完了／確認待ち
- [ ] UI 同期後に python3 scripts/sync_mod.py を実行し、ゲーム内で UI が崩れていないか確認する。

## メモ
- UI の個別要素（例: ジェノタイプ説明など）は ObjectBlueprints 側を参照。ここではウィンドウや説明枠のみ取り扱う。
- Options.jp.xml はファイル丸ごと置き換え未サポートのため、Load="Merge" のまま DisplayText 等の値だけ更新する。Replace="true" を付けると Player.log に Unused attribute 警告が出る。
- Genotypes.jp.xml / Subtypes.jp.xml / Mutations.jp.xml / EmbarkModules.jp.xml は `<genotype>` / `<class>` / `<category>` / `<module>` / `<window>` を **Load="Replace"** にし、不要な `Replace="true"` を使わない。XmlDataHelper が `Replace` 属性を解釈しないため、将来の警告を防げる。
- Docs/manual/ManualPatch.jp.manual（旧 Localization/Manual.jp.xml）は Harmony パッチ専用のデータソース。ゲームの XML ローダーに渡すと Quickstart などが二重登録されてクラッシュするので、Localization/Manual.jp.xml には空スタブを残す。
