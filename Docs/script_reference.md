# スクリプト リファレンス

翻訳フローで繰り返し使う補助スクリプトをカテゴリ別にまとめます。ここに記載のコマンドはすべてリポジトリの `scripts/` 配下にあります。

## 1. 文字コード / ロケール

### 1.1 `check_encoding.py`
- **目的**: `.md` / `.xml` / `.txt` / `.csv` などを走査し、`繧` や制御文字といったモジバケ候補を検出する。
- **代表コマンド**: `py -3 scripts/check_encoding.py --fail-on-issues`
  - `--path` / `--extension` / `--ignore` で対象を絞り込める。CI や pre-commit で利用する場合は `--fail-on-issues` を追加して異常時に終了コード 1 を返す。
- **関連ツール**: PowerShell 端末を UTF-8 へ固定するには `scripts/ensure_utf8.ps1`（単発実行）と `scripts/install_utf8_profile.ps1`（プロファイル登録）を使用する。

## 2. ベースデータ抽出

### 2.1 `extract_base.py`
- **目的**: ゲーム本体の `StreamingAssets/Base` を `references/Base` へコピーし、最新の XML / TXT 原文を取得する。
- **代表コマンド**: `py -3 scripts/extract_base.py --game-path "C:\Program Files (x86)\Steam\steamapps\common\Caves of Qud"`
  - `--output-path` を指定すれば別フォルダへ書き出し可能。
- 実行後は `git status` で差分を確認し、翻訳XMLの更新が必要か判断する。

## 3. 差分・欠落チェック

### 3.1 `diff_localization.py`
- **目的**: `references/Base` と `Mods/QudJP/Localization` の XML/TXT を比較し、未翻訳 (`object-missing` / `file-missing`) を抽出する。
- **代表コマンド**: `py -3 scripts/diff_localization.py --missing-only`
  - `--base Conversations.xml` のように個別ファイルへ絞り込める。`--json-path Docs/backlog/latest.json` を付けるとレポートを JSON として保存できる。
- 翻訳完了チェック、PR レビュー、リリース前確認の各タイミングで必ず実行する。

## 4. ObjectBlueprints 用ユーティリティ

LLM や大量抽出を伴う作業では以下の 3 つをセットで利用する。詳細なワークフローは `Docs/tasks/objectblueprints_llm.md` を参照。

| スクリプト | 役割 | 主なオプション |
| ---------- | ---- | -------------- |
| `objectblueprint_extract.py` | Base XML と JP XML を比較し、未翻訳の `<object Name>` を JSON へ抽出。 | `--base-file / --localized-file`（明示パス）、`--object`（対象限定）、`--max-objects`、`--include-present` |
| `objectblueprint_insert.py` | 抽出 JSON＋LLM の訳文 JSON から JP XML へ差分を挿入。 | `--payload`（extract の結果）、`--translations`（LLM 応答）、`--target`（書き込み先）、`--dry-run` |
| `objectblueprint_repair.py` | 途中で壊れた JP XML を、Base XML を参照しながら元の構造に復元。 | `--localized`（修復対象）、`--base`（参照するベース）、`--output`（別ファイル出力） |

## 5. Mod 同期

### 5.1 `sync_mod.py`
- **目的**: リポジトリ内 `Mods/QudJP` を実機の Mods フォルダへコピーし、ゲーム内確認できるようにする。
- **代表コマンド**: `py -3 scripts/sync_mod.py --dry-run`（内容確認）、`py -3 scripts/sync_mod.py`（実コピー）
  - `--target` で出力先を明示したり、`--exclude-fonts` で Fonts ディレクトリを省略できる。
- 実コピー時はゲームを再起動し、`Player.log` にエラーが出ていないか `Docs/log_watching.md` の手順で確認する。

---

このドキュメントにないスクリプトを使用する場合は `scripts/<name>.py --help` でオプションを確認し、必要に応じて本ファイルへ追記してください。
