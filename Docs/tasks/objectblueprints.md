# ObjectBlueprints / タスクボード

ObjectBlueprints 配下の翻訳状況を管理します。`references/Base/ObjectBlueprints/*.xml` をベースに、`Mods/QudJP/Localization/ObjectBlueprints/*.jp.xml` を `Load="Merge"` 形式で作成してください。

## 進行サマリ
- [x] `ObjectBlueprints/Items.jp.xml` … サイバネティクス装備・信用楔・書籍カテゴリ（Book / 料理本 / 血染めの羊皮紙 / 方眼紙 / ソネット）・エナジーセル群（Chem / Solar / Nuclear / 反物質 / 燃焼 / 熱電 など）・基礎ライトソース（たいまつ / 光輝球 / 医療品）・スクラップ素材（一般 / 医療系）・トニック～ツール／リコイラー（基本工具、携帯壁、リコイラー各種など）・スプレー／試薬／宝飾品／水袋（脱塩ペレット、ポリゲル、宝石・フィギュリン各種、水袋・水筒・小瓶まで）に加えて、近接武器カテゴリの棍棒系（club / mace / war hammer / artifact rod / baton）、短剣系（dagger / kris / kukri / gaslight short blade）、斧＆鞭（battle axe / halberd / vinereaper / whip）、長剣系（long sword / great sword / vibro blade / ceremonial khopesh）および固有近接武器（hindren axes / Difucila / Fist of the Ape God / La Jeunesse など）を訳出済み。**ライフルカテゴリ（鉛スラッグ弾／Issachar rifle～Phase Cannon までの本体＋エネルギー兵器）も翻訳済み。弓矢カテゴリ・重火器・ピストル派生の武器もバッチ投入済みで、今回ショットガン（BaseShotgunProjectile～Combat Shotgun）を追加。残りの武器カテゴリ（長柄武器／ユニーク飛び道具／投擲武器など）および防具・設置物は未訳なので diff を見ながら追加する。
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
- [x] `ObjectBlueprints/TutorialStaging.jp.xml`
- [ ] `ObjectBlueprints/Walls.jp.xml`
- [ ] `ObjectBlueprints/Widgets.jp.xml`
- [ ] `ObjectBlueprints/WorldTerrain.jp.xml`
- [ ] `ObjectBlueprints/ZoneTerrain.jp.xml`

同じ並びで `ObjectBlueprints/ObjectBlueprints/*.xml` もすべて未作成なので、上記と並行して対応する。

## 進め方メモ
1. `python3 scripts/diff_localization.py --missing-only` で `file-missing` / `object-missing` を洗い出す。`Docs/backlog/latest.json` にも同じ情報を保存しておく。
2. 1 ファイルずつ `references/Base/ObjectBlueprints/<ファイル名>.xml` をコピーし、DisplayName / Description / BehaviorDescription など文字列のみを翻訳。数値・タグ構造は変更しない。
3. `Load="Merge" Replace="true"` 方針を維持し、`<object Name>` はベースと全く同じにする。
4. 翻訳後はこのタスクボードと `Docs/translation_status.md` を更新し、`python3 scripts/check_encoding.py --fail-on-issues` でモジバケを検知。
5. 必要に応じて `Docs/glossary.csv` に用語を追記し、UI 表示は `Docs/test_plan.md` のシナリオで検証する。
6. **汎用ステータスや操作コマンド（例：AV / DV / HP / Wait など）は原文の英語表記を維持し、説明文やフレーバーのみ日本語化する。**
