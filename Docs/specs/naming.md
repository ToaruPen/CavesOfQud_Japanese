# 命名・参照規約 v1

翻訳差し込みの議論を始めるための共通ルールを先に固定しておく。

## コンテキスト ID

- 形式: `<Namespace.Class.Method>.<Role>`
- `<Namespace.Class.Method>` は **ILSpy のフル修飾名**に合わせる。拡張メソッドは定義側の型名を使い、オーバーロードは必要に応じて `Method@2` のように明示する。
- `<Role>` は **UI の役割**を示す。例: `PopupMessage.ShowPopup.Message`、`MessageQueue.AddPlayerMessage.LogLine`。
- スレッド境界が変わる場合は `.GameQueue` / `.UIThread` などを Role に付記して曖昧さを避ける。

## 推奨ロール

| Role | 用途 / 例 |
| --- | --- |
| `Title` | ポップアップやツールチップの第一見出し |
| `Message` | 汎用ポップアップ本文 |
| `Body` | 長文テキスト全体（ログ / 日誌など） |
| `SubHeader` | Tooltip のセクション見出し |
| `Footer` | 確認ダイアログの補足 / ヒント |
| `ButtonLabel` | `Yes / No` などの UI ボタン |
| `LogLine` | MessageLog・戦闘ログの 1 行 |
| `EntryLine` | ジャーナルの項目 |
| `WeightLabel` | インベントリの重量やコスト欄 |
| `Hint` | 操作チュートリアルや暗黙説明 |
| `TooltipField` | Tooltip 内の個別フィールド |

## 辞書キー正規化

1. 前後の空白は trim、内部の連続空白は 1 つに揃える。ただし `\n` / 制御記号は保持。
2. 色タグ / Markup（例: `{{K|text}}`）は元の位置で保持。翻訳結果ではタグを入れ替えず、別セグメントに分けない。
3. `%` や `{{value}}` といった動的トークンは **セグメントの境界として利用**し、必要なら `%` 単位で分割翻訳。
4. 数値・単位は `{0} {{R|dram}}` のようにプレースホルダ化して語順差異を吸収する。
5. RTF / TMP RichText の `<>` は XML エスケープ不要な状態で保存する。

## 辞書エントリ作成時のログ

- `Translator/JpLog` へは `ContextID|Key|Status` 形式で出力する（JpLog.cs 参照）。
- ヒット／ミス率の分析をしやすくするため、ContextID と Role のペアが最小単位。
- 非同期 UI の場合は `ContextID` に `.AsyncPending` や `.GameQueue` を付け、ログ上でキューを追えるようにする。

## 参照位置メモ

- `Docs/pipelines/*.md` : 各パイプライン仕様書で ContextID と Role を必須項目にする。
- `Docs/targets/*.md` : コンテキスト ID ごとのカバレッジ表をここへ集約。
- `Docs/specs/formatting.md` : Markup / RTF / ClipText の整形前提をまとめ、辞書キー整形の根拠とする。

上記は v1 の基盤。追加ロールや例外が出た場合は `docs/specs/naming.md` を更新し、参照箇所もリンクする。
