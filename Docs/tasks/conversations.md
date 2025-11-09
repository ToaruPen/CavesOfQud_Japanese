# 会話タスクボード

`Conversations.jp.xml`（NPC 会話）と関連トリガー（ログ参照、Argyve 以外の導線など）を管理します。

## 未訳 / 対応中
- [x] Argyve 以降の Joppa NPC 会話ブロック（`StartHasFetch3` 以降）を翻訳する。→ Weirdwire / Canticle / post-Canticle 応答まで訳出済み。
- [x] BaseConversation: Water Ritual / Trade の共通選択肢と応答を翻訳し、すべての NPC で儀式の文言が日本語化されるようにする。
- [x] BaseSlynthMayor: Slynth 移住クエスト向けの共通会話（要請～到着/定住報告）と選択肢を翻訳する。
- [ ] Barathrumites クエストラインのベース差分を `references/Base/Conversations.xml` から抽出。

## レビュー待ち
- [ ] 追加した会話ノードが `Docs/log_watching.md` の手順で Missing Node を出さないか確認。

## メモ
- 差分確認時は `python3 scripts/diff_localization.py --missing-only --json-path Docs/backlog/conversations.json` を利用する（必要に応じて再生成）。
