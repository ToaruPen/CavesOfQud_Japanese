# <Pipeline 名称> パイプライン仕様 (雛形)

> **画面 / コンテキスト:** 例) ポップアップ (Console), Tooltip (Unity)

## 概要

- 出力先 (Console / Unity)
- 生成トリガ (例: `PopupMessage.ShowPopup`)
- 対応する ContextID / Role

## 主なクラス / メソッド

| フェーズ | クラス | メソッド / 備考 |
| --- | --- | --- |
| 生成 |  |  |
| 整形 |  |  |
| 描画 |  |  |

## データフロー

1. **生成:** 例 `Popup.Show` が Markup 付きテキストを受け取る。
2. **整形:** `Markup.Transform` → `RTF.FormatToRTF` → `StringFormat.ClipTextToArray`
3. **描画:** `ScreenBuffer.Write` / `TMP_Text.SetText`

## 整形規則

- 色タグ / Markup
- 折り返し幅 / ClipTextToArray のパラメータ
- インデント / 余白

## 同期性

- 呼び出しスレッド (sync / gameQueue / uiQueue)
- ハーモニーパッチの実行位置

## 置換安全点

- 推奨フック (Prefix / Postfix / Transpiler)
- 理由 (整形前 / clip 前に差し込めるか)

## 例文 / トークン

- テキスト例
- Grammar / HistoricStringExpander の展開ポイント

## リスク

- 二重 Transform
- 表示崩れ
- 検索性 (キーが動的に変化する箇所 等)

## テスト手順

- 操作手順
- 想定出力
- 崩れ検査ポイント
