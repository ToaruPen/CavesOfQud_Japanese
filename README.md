# Caves of Qud Japanese Localization

## プロジェクト概要
Caves of Qud を日本語で快適に遊べるようにするためのローカライズ Mod です。  
会話や書籍・UI テキストを自然な日本語に翻訳しつつ、Harmony ベースの補助 DLL と CJK 対応フォントを同梱して、ゲーム内のあらゆるレイヤーで文字化けなく表示できることを目標にしています。

## 現在の進行方針
- Mod ルート `Mods/QudJP` に manifest / README / Harmony DLL / フォント / Localization XML を集約。
- `Docs/` にログ監視、フォント生成、翻訳フロー、テスト計画、用語集などのナレッジを記載。
- `scripts/` には vanilla ファイルの抽出や差分チェック用 PowerShell ツールを配置。
- `references/Base/` でゲーム本体のベースデータを管理し、翻訳 XML との差分を追いやすくする。

## 直近 TODO
1. Harmony プロジェクト雛形を `Mods/QudJP/Assemblies` に作成し、フォント差し替え PoC を実施。
2. Noto 系フォントのサブセット化と TMP Font Asset を生成して `Fonts/` に追加。
3. Conversations / Books / Commands など主要 XML の `Load="Merge"` テンプレートを作成。
4. 翻訳ワークフローを Docs/translation_process.md に沿って回し始め、ログ監視で問題を検出。
5. README / changelog / workshop 用アセットを整備し、配布形態（ZIP + Workshop）を確立。

貢献や質問は Issue / Discussion / PR で歓迎します。フォントライセンスなどの注意点は `Docs` ディレクトリを参照してください。
