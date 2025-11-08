# タスクボード運用ルール

カテゴリごとに 1 ファイルで未訳タスクを管理し、完了した単位で `archive/` サブフォルダーへ移動します。ファイル数が肥大化しないよう、常時メンテナンスするのは以下の 4 ファイルのみです。

| ファイル | 管理対象 | 自動レポート連携の目安 |
| --- | --- | --- |
| `Docs/tasks/ui.md` | Options / EmbarkModules など UI 系 XML | `scripts/diff_localization.ps1 -MissingOnly` で UI に関係するファイルが出たら追記 |
| `Docs/tasks/conversations.md` | 会話データ（Argyve 以外、NPC 全般） | `references/Base/Conversations` の差分確認ごと |
| `Docs/tasks/books.md` | Books / Corpus 系の長文 | 書籍テンプレ更新時 |
| `Docs/tasks/objectblueprints.md` | Items / Creatures / Mutations など ObjectBlueprints 一式 | Harmony 連携タスクもここに集約 |

### 更新手順
1. 翻訳対象を着手する際、該当カテゴリファイルの「未訳/対応中」セクションにチェックボックス付きで追記します。
2. 作業が完了したら `Docs/tasks/archive/` 以下に `YYYY-MM-ui.md` のような名前で切り出して保存し、元ファイルからは該当項目を削除します。
3. `Docs/translation_status.md` にはカテゴリ単位のステータスだけを残し、個別詳細は各タスクファイルを参照する形にします。

### 注意事項
- 自動レポート（`scripts/diff_localization.ps1 -MissingOnly -JsonPath Docs/backlog/latest.json` 等）がある場合は、その結果をもとに「未訳ファイル一覧」を上書きする。
- アーカイブ済みファイルは参照しない。必要になったら `archive/` から戻す。
- ファイルを増やしたくなった場合でも、まずは既存 4 カテゴリのどれに入るかを検討し、どうしても分割が必要ならチームで合意を取る。
