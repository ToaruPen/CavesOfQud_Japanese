# 翻訳進捗表

| ファイル / カテゴリ | 状態 | 備考 |
| --- | --- | --- |
| Conversations | 進行中 | `Conversations.jp.xml` で主要 NPC（Argyve など）の会話を反映済み。未訳イベントは diff レポートで随時確認。 |
| Books | 進行中 | `Books.jp.xml` を作成済み。`Corpus/` 本編は未訳なので、Docs/tasks/books.md のタスクを参照。 |
| Commands | 進行中 | `Commands.jp.xml` を `Replace="true"` 方針で更新。UI 表記との整合レビュー中。 |
| EmbarkModules | 進行中 | `EmbarkModules.jp.xml` で Genotype / Subtype / Cybernetics 等を訳出。Subtype UI など Replace ブロックの再確認が必要。 |
| Genotypes / Subtypes | 進行中 | `Genotypes.jp.xml` / `Subtypes.jp.xml` で名称訳は完了。選択画面の幅調整と追加派生に注意。 |
| Mutations | 進行中 | `Mutations.jp.xml` に Morphotypes / Physical / PhysicalDefects / Mental / MentalDefects を実装済み。残りカテゴリは未着手。 |
| Options | 進行中 | `Options.jp.xml` で全カテゴリの DisplayText を訳出済み。細部 UI テスト待ち。 |
| ObjectBlueprints | 進行中 | `ObjectBlueprints/Items.jp.xml` でサイバネ装備 + 信用楔を訳出、`RootObjects.jp.xml` で CosmeticObject を差し替え。その他ファイルは `file-missing`。`Items.jp.xml` も一般アイテムは未訳。 |
| Corpus (Lore) | 未着手 | `Corpus/*.jp.txt` は未作成。Docs/tasks/books.md のチェックボックスに従って対応。 |
| Grammar / Population / Harmony | 未着手 | Harmony フック側で翻訳テーブルを組み込む予定。 |

状態ラベルの目安: `未着手` / `進行中` / `レビュー中` / `完了`。
