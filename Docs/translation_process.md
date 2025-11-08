# 翻訳プロセス

## 0. 文字コード設定（重要）
- リポジトリ直下の `.editorconfig` を守り、すべてのテキストは UTF-8 (BOM 無し) / LF で保存する。エディタで別のコードページが選ばれていないかを毎回確認する。
- PowerShell 5.x では `chcp 65001 > $null; [Console]::OutputEncoding = [System.Text.Encoding]::UTF8` を起動直後に実行し、`Get-Content` / `Set-Content` も `-Encoding utf8` を常用する。これを行わないと CP932 で読み書きして文字化けが再注入される。
- ファイルをコミットする前に `scripts/check_encoding.ps1 -FailOnIssues` を実行し、`繧` / `縺` など典型的なモジバケシーケンスが混入していないか自動チェックする。CI や pre-commit に組み込む場合も同コマンドを利用する。

## 1. ベースデータの取得
1. Caves of Qud を最新バージョンに更新。
2. Windows の場合は `scripts/extract_base.ps1 -GamePath "C:\Program Files (x86)\Steam\steamapps\common\Caves of Qud"`、macOS の場合は  
   `pwsh ./scripts/extract_base.ps1 -GamePath "$HOME/Library/Application Support/Steam/steamapps/common/Caves of Qud"` を実行し、`references/Base` に Conversations / Books / ObjectBlueprints などの最新コピーを取得。  
3. ゲームのアップデートやリリース前チェック時にも再取得して差分を確認する。

## 2. ローカライズ テンプレート作成
- `Mods/QudJP/Localization/*.jp.xml` を `Load="Merge"` 形式で作成し、元ファイルから必要な `<object Name>` や `<command ID>` を丸ごとコピーする。
- Blueprint 名や GUID は絶対に変更しない。`Replace="true"` を付ける場合は、元データの構造を完全に再現する。
- 改行・制御コード（`%t`, `^G`, `&object&` など）は必ず残す。

## 3. 翻訳フロー
1. 共通用語は `Docs/glossary.csv` に追記し、表記ゆれを防ぐ。
2. ファイル単位（会話 / 書籍 / UI など）で「翻訳 → ゲーム内確認 → ログチェック」を 1 サイクルとする。
3. CAT ツールを使う場合は UTF-8 / LF を維持したまま XML / TXT に戻す。
4. 進捗は `Docs/translation_status.md`（カテゴリ単位）と `Docs/tasks/*.md`（カテゴリ別タスクボード）に反映する。  
   - 細粒度タスクは各タスクボードにチェックボックス付きで追記し、完了後は `Docs/tasks/archive/` へ移す。
5. `scripts/diff_localization.ps1 -MissingOnly -JsonPath Docs/backlog/latest.json` を適宜実行し、未訳リストを自動更新する。

## 4. Mod 実体への反映
- 作業ブランチ内の `Mods/QudJP` を真実のソースとし、ゲームが参照する Mod 実体（`%USERPROFILE%\AppData\LocalLow\Freehold Games\CavesOfQud\Mods\QudJP`）へは必要なタイミングでのみ同期する。
- `scripts/sync_mod.ps1` を実行し、翻訳をゲームに適用したい時だけ `/MIR` コピーを行う。`-WhatIf` でドライラン、`-ExcludeFonts` で Fonts フォルダを除外できる。
- 同期後にテストする場合はゲームを再起動し、`Player.log` を確認する。

## 5. 差分・レビュー
- `scripts/diff_localization.ps1 -MissingOnly` で未翻訳ファイルや `<object Name>` の欠落を把握。必要に応じて `-JsonPath` でレポートを保存する。
- Pull Request には変更ファイル、スクリーンショット（必要な場合）、`Player.log` を添付し、レビュアーが再現できるようにする。
- 長文（書籍・詩など）はダブルチェックを推奨。

## 6. 自動生成テキスト
- Grammar / Population / Combat Log などコード生成系テキストは Harmony 側でハンドラを追加し、翻訳テーブル経由で日本語化する。
- 名詞・動詞活用など複雑な箇所はヘルパークラスに切り出して再利用性を高める。

## 7. リリース前チェック
1. `scripts/diff_localization.ps1` で未訳が残っていないか確認。
2. `Docs/test_plan.md` のシナリオを実施し、UI 崩れや Missing Glyph が無いかを `Docs/log_watching.md` の手順で検証。
3. `Mods/QudJP` フォルダを整理（不要ファイル削除）し、`README` / `CHANGELOG` / Workshop テキストを更新。
4. 配布時は `Mods/QudJP` フォルダのみをまとめ、`references` や `Docs` は含めない。
