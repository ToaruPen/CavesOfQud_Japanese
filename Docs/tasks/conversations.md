# 会話タスクボード

`Conversations.jp.xml`（NPC 会話）と関連トリガー（ログ参照、Argyve 以外の導線など）を管理します。

## 未訳 / 対応中
- [x] Argyve 以降の Joppa NPC 会話ブロック（`StartHasFetch3` 以降）を翻訳する。→ Weirdwire / Canticle / post-Canticle 応答まで訳出済み。
- [ ] Barathrumites クエストラインのベース差分を `references/Base/Conversations.xml` から抽出。

## レビュー待ち
- [ ] 追加した会話ノードが `Docs/log_watching.md` の手順で Missing Node を出さないか確認。

## メモ
- 差分確認時は `scripts/diff_localization.ps1 -MissingOnly -JsonPath Docs/backlog/conversations.json` を利用する（必要に応じて再生成）。
