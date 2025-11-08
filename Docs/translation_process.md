# 翻訳プロセス

## 1. ベースデータの取得
1. `scripts/extract_base.ps1 -GamePath "C:\Program Files (x86)\Steam\steamapps\common\Caves of Qud"` を実行し、`references/Base` に最新の `Conversations.xml` などをコピー。
2. ゲームアップデート後は必ず再実行し、コミット前に差分を確認。

## 2. 抽出テンプレート作成
- `Mods/QudJP/Localization/*.jp.xml` を `Load="Merge"` で作成し、原文から必要な要素のみをコピー。
- GUID や blueprint 名は絶対に変更しない。
- 改行やプレースホルダー (`&object&`, `%t`, `^G`) はそのまま残す。

## 3. 翻訳フロー
1. `Docs/glossary.csv` に用語・固有名詞・人称の訳語ルールを追記。
2. 章やカテゴリ単位でローカライズ → ゲーム内チェック → ログ確認を 1 サイクルとする。
3. CAT ツールを使う場合は CSV/TSV エクスポートを利用し、終了後 XML に戻す際にエンコード(UTF-8)を維持。

## 4. レビュー
- PR では翻訳ファイル + 該当スクリーンショット + Player.log を添付。
- 長文 (書籍/詩) は誤字脱字防止のためレビュー担当を割り当てる。
- ゲーム内表示確認を済ませたら `Docs/translation_status.md` に済マークを付ける。

## 5. 自動生成テキスト
- Harmony パッチで英語文を生成している箇所はリソースファイル化し、JP 文字列テーブル経由で管理する。
- 名詞／動詞の活用や助詞の選択は専用ヘルパーに切り出し、翻訳者が文字列を直接編集しなくても流暢な文章になるようにする。

## 6. リリース前チェック
- `scripts/diff_localization.ps1` でベースとの差分と未翻訳ノードを確認。
- スモークテスト (Docs/test_plan.md) を一通り実施し、UI 崩れやログエラーが無ければ changelog を更新してタグ付け。
- Zip 化する場合は `Mods/QudJP` 直下のみを含め、`references` や `Docs` は含めない。
