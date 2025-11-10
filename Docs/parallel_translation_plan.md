# 並列翻訳ワークフロープラン

複数の Codex チャット（または作業者）を同時に走らせて翻訳を進める際の共通指針をまとめる。  
目的は「誰がどのワークストリームを担当しても、必要なファイル・コマンド・報告先が即座に分かる状態」を維持すること。

## 参照すべき既存ドキュメント

- `Docs/translation_process.md` – 全体フローと diff/backlog 更新ルール
- `Docs/translation_status.md` – カテゴリ別進捗テーブル
- `Docs/tasks/*.md` – カテゴリごとのタスクボード（チェックボックス運用）
- `Docs/localization_targets.md` – ローダー仕様と対象一覧
- `Docs/log_watching.md` – ローダーエラー確認手順

## 共通ルール

1. **開始前同期**  
   - `py -3 scripts/diff_localization.py --missing-only --json-path Docs/backlog/latest.json` を実行して最新の未訳リストを更新・共有する。
   - 担当チャットには「ワークストリーム名」「対象ファイル（例：Items.xml 1〜150）」を必ず記載する。
2. **成果の記録**  
   - 各バッチ完了後に `Docs/tasks/<category>.md` と `Docs/translation_status.md` を更新し、必要なら backlog JSON も再出力する。
   - 変更内容は `git status` で確認し、余計なファイルを含めない。
3. **検証サイクル**  
   - 作業ごとに `py -3 scripts/diff_localization.py --missing-only` を再実行して漏れをチェック。
   - ローダー／ゲーム側で問題が起きた場合は `Docs/log_watching.md` に従って `Player.log` を確認する。

## ワークストリーム一覧

### ワークストリームA – ObjectBlueprints/Items

- **目的**: `ObjectBlueprints/Items.xml` に残っている 6,400 以上の `object-missing` をカテゴリ単位で潰す。
- **手順**  
  1. 例: `python3 scripts/objectblueprint_extract.py --base-file references/Base/ObjectBlueprints/Items.xml --max-objects 150 --output work/items_chunk_##.json`  
     （カテゴリ区切りで `--object` を併用しても良い）
  2. `pending_strings` を翻訳（LLM でも手動でも可）。  
  3. `python3 scripts/objectblueprint_insert.py --payload work/items_chunk_##.json --translations work/items_chunk_##_translated.json`
  4. `py -3 scripts/diff_localization.py --missing-only --json-path Docs/backlog/latest.json` で欠落を確認。
  5. `Docs/tasks/objectblueprints.md` の該当カテゴリにチェックとメモを追加。

### ワークストリームB – Conversations（Barathrumites ほか）

- **対象ノード**: `Conversations.xml` 内の `GritGateHandler` / `QuestHandler` / `KithAndKin*` / `AngorNegotiation` など 58 件（`Docs/backlog/latest.json` で抽出済み）。
- **手順**  
  1. `py -3 scripts/diff_localization.py --missing-only --json-path Docs/backlog/conversations.json --base Conversations.xml` などで対象だけのリストを作成。
  2. 会話チェーン単位で翻訳して `Mods/QudJP/Localization/Conversations.jp.xml` へ追記。
  3. `Docs/tasks/conversations.md` の該当チェックボックスを更新し、必要ならメモ欄で担当者を記録。

### ワークストリームC – Corpus / Books

- **対象**: `Machinery-of-the-Universe` / `Meteorology-Weather-Explained` / `Thought-Forms` を含む `Corpus/*.txt`。
- **手順**  
  1. `references/Base/Corpus/<file>.txt` を読み、`Mods/QudJP/Localization/Corpus/<file>.jp.txt` を新規作成。
  2. 逐語訳＋語調調整（`Docs/glossary.csv` を参照）後、`python scripts/check_encoding.py --fail-on-issues` で UTF-8/制御文字を検査。
  3. `Docs/tasks/books.md` と `Docs/translation_status.md` を更新。

### ワークストリームD – その他 ObjectBlueprints & EmbarkModules

- **対象**: `Creatures/Data/Foods/Furniture/HiddenObjects/PhysicalPhenomena/Staging/Walls/Widgets/WorldTerrain/ZoneTerrain` など `Docs/tasks/objectblueprints.md:11` の未チェック項目＋ `EmbarkModules.jp.xml.disabled`。
- **手順（Blueprint 系）**: Items と同じ extractor→翻訳→inserter の流れ。1 ファイルごとに payload を作り、小さな単位でコミットする。
- **手順（EmbarkModules）**: `.disabled` を外す前に内容を確認し、UI 文言の整合性を `Docs/localization_targets.md` と突き合わせた上で `py -3 scripts/diff_localization.py --missing-only` で完了を検証。

## チャット／担当の割り当て方法

1. 新しいチャットを開いたら、冒頭で「ワークストリームA：Items（カテゴリ=Base objects 〜 Melee Weapons）」のように担当範囲を明記する。
2. 別チャットに同じ範囲を割り当てない。必要なら `Docs/tasks/<category>.md` に担当者名をメモして調整する。
3. バッチ完了を報告するときは「処理した payload / 追加したファイル / 実行コマンド / 残タスク」の形で要約する。

## バッチ完了チェックリスト

1. 変更ファイルの diff を確認（`git diff`）。
2. `py -3 scripts/diff_localization.py --missing-only` を再実行し、担当範囲に未訳が残っていないか確認。
3. `Docs/tasks/*.md`・`Docs/translation_status.md` を更新。必要に応じて `Docs/backlog/<name>.json` を再生成。
4. `python scripts/check_encoding.py --fail-on-issues`（新規ファイルや大量の文字列を触った場合）。
5. ローダーに関わる変更なら `Docs/log_watching.md` を参照して `Player.log` を確認。

この文書は随時アップデートして良い。新しいワークストリームを追加したり、既存フローに変更が入った場合はここに追記し、以降は各チャットでこのファイルを参照するよう周知すること。

