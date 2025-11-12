# 翻訳プロセス

> スクリプトの使い方は `Docs/script_reference.md` に切り出しました。ここでは翻訳フローの考え方とスクリプト以外の注意点をまとめています。

## 0. 文字コード設定（重要）
- リポジトリ直下の `.editorconfig` を守り、すべてのテキストは UTF-8 (BOM 無し) / LF で保存する。エディタで別のコードページが選ばれていないかを毎回確認する。
- PowerShell 5.x では `chcp 65001 > $null; [Console]::OutputEncoding = [System.Text.Encoding]::UTF8` を起動直後に実行し、`Get-Content` / `Set-Content` も `-Encoding utf8` を常用する。これを行わないと CP932 で読み書きして文字化けが再注入される。
- ファイルをコミットする前に `scripts/check_encoding.py` で文字化けを検出し（詳細は `Docs/script_reference.md`）、`繧` / `縺` など典型的なモジバケシーケンスが混入していないか自動チェックする。CI や pre-commit に組み込む場合も同スクリプトを利用する。

## 1. ベースデータの取得
1. Caves of Qud を最新バージョンに更新。
2. `scripts/extract_base.py`（参照: `Docs/script_reference.md`）を実行し、`references/Base` に Conversations / Books / ObjectBlueprints などの最新コピーを取得。  
- Genotypes / Subtypes / Mutations / EmbarkModules など XmlDataHelper が処理する XML では要素側に `Load="Replace"` を指定して丸ごと置き換える。これらは `Replace="true"` を解釈しないため、旧テンプレを流用すると Player.log に `Unused attribute "Replace"` が出る。
3. ゲームのアップデートやリリース前チェック時にも再取得して差分を確認する。

## 2. ローカライズ テンプレート作成
- `Mods/QudJP/Localization/*.jp.xml` を `Load="Merge"` 形式で作成し、元ファイルから必要な `<object Name>` や `<command ID>` を丸ごとコピーする。
- Blueprint 名や GUID は絶対に変更しない。`Replace="true"` を付ける場合は、元データの構造を完全に再現する。
- 改行・制御コード（`%t`, `^G`, `&object&` など）は必ず残す。

## 3. 翻訳フロー
1. 共通用語は `Docs/glossary.csv` に追記し、表記ゆれを防ぐ。派閥・地名など短縮形が必要な語は `Short` 列も必ず埋め、UI 表記やログと揃える。
2. 機械名や装備名など英語複合語をカタカナ化する場合は、基本的に閉じた複合語として `・` を挟まずに書く（例: `Chain Mail` → `チェインメイル`、`Gas Mask` → `ガスマスク`、`Sniper Rifle` → `スナイパーライフル`）。`Chrome` など固有名詞や地名を強調したい場合、あるいは `・` を残さないと読みづらくなる場合のみ中黒を使う（例: `Chrome Pyramid` → `クローム・ピラミッド`、`Bethesda Susa Recoiler` → `ベセスダ・スーサ・リコイラー`）。

### 固有名の表記ルール
- Barathrum the Old は必ず「バラサラム（老）」、派閥は「バラサラム派」で統一する。派閥構成員は「バラサラム派の◯◯」「=factionaddress:Barathrumites=」の場合は「バラサラム派」。
- Sultan croc 系は「スルタン・クロコダイル」、死体も「スルタン・クロコダイルの死体」。
- Q Girl は UI・書籍・会話すべてで「Qガール」。色タグ付き表記でも同じ。
- Bethesda Susa は「ベセスダ・スーサ」。色タグやリコイラー名も同表記に揃える。
- これらは `Docs/glossary.csv` に登録済みなので、新規テキストでは必ず参照する。
3. ファイル単位（会話 / 書籍 / UI など）で「翻訳 → ゲーム内確認 → ログチェック」を 1 サイクルとする。
4. CAT ツールを使う場合は UTF-8 / LF を維持したまま XML / TXT に戻す。
5. 進捗は `Docs/translation_status.md`（カテゴリ単位）と `Docs/tasks/*.md`（カテゴリ別タスクボード）に反映する。  
   - 細粒度タスクは各タスクボードにチェックボックス付きで追記し、完了後は `Docs/tasks/archive/` へ移す。
6. `scripts/diff_localization.py` で未訳リストを適宜更新する（使い方は `Docs/script_reference.md` を参照）。

## 4. Mod 実体への反映
- 作業ブランチ内の `Mods/QudJP` を真実のソースとし、ゲームが参照する Mod 実体へは必要なタイミングでのみ同期する。  
  - Windows: `%USERPROFILE%\AppData\LocalLow\Freehold Games\CavesOfQud\Mods\QudJP`  
  - macOS (Steam 版): `~/Library/Application Support/Steam/steamapps/common/Caves of Qud/CoQ.app/Contents/Resources/Data/StreamingAssets/Mods/QudJP`
- `scripts/sync_mod.py` を実行し、翻訳をゲームに適用したい時だけミラーリングを行う（`Docs/script_reference.md` に詳細あり）。`--dry-run` や `--exclude-fonts` の各オプションを状況に応じて使い分ける。
- 同期後にテストする場合はゲームを再起動し、`Player.log` を確認する。

## 5. 差分・レビュー
- `scripts/diff_localization.py --missing-only` で未翻訳ファイルや `<object Name>` の欠落を把握。必要に応じて `--json-path` でレポートを保存する（詳細は `Docs/script_reference.md`）。
- Pull Request には変更ファイル、スクリーンショット（必要な場合）、`Player.log` を添付し、レビュアーが再現できるようにする。
- 長文（書籍・詩など）はダブルチェックを推奨。

## 6. 自動生成テキスト
- Grammar / Population / Combat Log などコード生成系テキストは Harmony 側でハンドラを追加し、翻訳テーブル経由で日本語化する。
- 名詞・動詞活用など複雑な箇所はヘルパークラスに切り出して再利用性を高める。

## 7. リリース前チェック
1. `scripts/diff_localization.py` で未訳が残っていないか確認。
2. `Docs/test_plan.md` のシナリオを実施し、UI 崩れや Missing Glyph が無いかを `Docs/log_watching.md` の手順で検証。
3. `Mods/QudJP` フォルダを整理（不要ファイル削除）し、`README` / `CHANGELOG` / Workshop テキストを更新。
4. 配布時は `Mods/QudJP` フォルダのみをまとめ、`references` や `Docs` は含めない。

## 8. ログファイルの保管場所
- Windows  
  - `build_log.txt` : `%USERPROFILE%\AppData\LocalLow\Freehold Games\CavesOfQud\build_log.txt`  
  - `Player.log` : `%USERPROFILE%\AppData\LocalLow\Freehold Games\CavesOfQud\Player.log`
- macOS (Steam 版)  
  - `build_log.txt` : `~/Library/Application Support/Freehold Games/CavesOfQud/build_log.txt`  
  - `Player.log` : `~/Library/Logs/Freehold Games/CavesOfQud/Player.log`

詳細な監視手順は `Docs/log_watching.md` を参照。
