# Tooltip パイプライン v1

> **対象システム**: `Look.ShowItemTooltipAsync` が生成するハイライト/ポップアップ Tooltip（マウスホバー、`l`ook UI）  
> **Contract (2025-11)**: 翻訳は `gameQueue` 上で完了させ、UI スレッドでは `ModelShark.TooltipTrigger.SetText` が最終文字列を `TMP_Text` へ流し込むだけにする。RichText は `Sidebar.FormatToRTF` / `RTF.FormatToRTF` の Allowlist を越えないタグのみ許可する。  
> **Encoding**: 本ファイルを含む `Docs/pipelines` は **UTF-8 (BOM 無し)** 固定。`scripts/check_encoding.py` のレポートを PR に添付して再発を防止する。

## 目的
- Tooltip の生成段階（データ収集 → 本文構築 → RTF 化 → TMP 表示）を分解し、翻訳 Hook を入れる安全な手順を共有する。
- `<sprite …>` や PUA グリフ（U+E000–F8FF）を一切壊さず、テキスト本体だけを翻訳するしくみを文書化する。
- gameQueue / uiQueue の境界を明示し、非同期間でメモリ再利用が起きても RichText が崩れないようにする。

## パイプライン概要

| ステージ | 実装 | 役割 |
| --- | --- | --- |
| データ収集 | `XRL.UI.Look.GenerateTooltipInformation` | `TooltipInformation` を組み立てる。DisplayName / SubHeader / WoundLevel / LongDescription / Icon がここで決まる。`gameQueue` 内で実行。 |
| 本文生成 | `XRL.UI.Look.GenerateTooltipContent` | `StringBuilder` で DisplayName → 改行 → LongDescription → 改行 → SubHeader → WoundLevel を結合し、`Markup.Transform` で色コードを解決する。 |
| RTF 変換 | `Sidebar.FormatToRTF`（長文） / `RTF.FormatToRTF`（短文） | 60 桁幅で折り返し、Unity RichText 安全タグだけを残した RTF 互換表現へ変換。 |
| UI 表示 | `ModelShark.TooltipTrigger.SetText` | `TooltipTrigger.SetText("BodyText", …)` が `TMP_Text` へ値を渡し、`ShowManually()` が描画をトリガーする。 |

## トークナイズ翻訳（TooltipTokenizedTranslator）

### 保護対象
- `<sprite …>` タグ（属性は name / index / spriteAsset など未知の値も温存）
- Unity RichText タグ `<b><i><u><s><mark><color><size><font><align><alpha><cspace><mspace><indent><line-height><lowercase><uppercase><smallcaps><sub><sup><voffset><link><nobr><br>`
- Qud Markup `{{…}}`（`{{C|text}}` 等）
- PUA グリフ（U+E000–U+F8FF）：Missing character 0xE*** を避けるため 1 文字ごと丸ごと保護する

### 処理手順
1. **保護**  
   `TooltipTokenizedTranslator` が上記パターンを `⟦S0⟧` / `⟦R1⟧` / `⟦Q2⟧` / `⟦P3⟧` のような一意トークンへ置き換える。置換は右→左で行い、文字位置がずれないようにする。
2. **翻訳**  
   トークンで区切られたテキスト走査部のみを `Translator.Instance.Apply(contextId)` に渡す。空白・改行だけの run はそのまま残し、辞書 miss の場合も原文を維持する。Tooltip の ContextID は `ModelShark.Tooltip.<StyleName>.<Field>` 形式（例: `ModelShark.Tooltip.LookLong.BodyText`）。
3. **復元**  
   左→右でトークンを元の文字列に戻す。`TooltipRichTextSanitizer` が最後に `<color>` の正規化と空タグの除去を実施し、TMP 側へ安全なタグのみ渡す。

### ハーモニーフック
- `ModelShark.TooltipTrigger.SetText(string fieldName, string text)` Prefix  
  - `TooltipIconPartitioner` が先頭/末尾の `<sprite>` / PUA を前後プレフィックスとして切り離し、中央ラベルのみ `TooltipFieldLocalizer.Process` に渡す。  
  - `TooltipTokenizedTranslator` が内部でトークナイズ翻訳を実行。`TooltipFieldLocalizer.IsSubHeaderField` が空文字だった場合は `TooltipSubHeaderBuilder` でサブヘッダを再生成。
- `ModelShark.TooltipTrigger.SetText(string text)` Prefix  
  - 表示対象が 1 フィールドのみの場合でも同じ処理を適用する。  
- Postfix で `TooltipTrigger.parameterizedTextFields` を補完し、`TooltipManager.textFieldDelimiter` を使った占位文字を自動復元する。

## Sprite / PUA フォント
- `FontManager.TryLoadFonts` が起動時に Noto Sans CJK フォントを `TMP_FontAsset.CreateFontAsset` から生成し、`TMP_Settings` へ登録する。
- `RegisterSpriteAssets` は Qud の `TMP_SpriteAsset` を検出して `TMP_Settings.defaultSpriteAsset` へ割り当て、全 `TMP_SpriteAsset.fallbackSpriteAssets` にも挿入する。これにより `<sprite name="AV"/>` や `<sprite index=123>` が必ず描画される。
- PUA グリフに対しては Primary/Bold FontAsset の `fallbackFontAssetTable` を強制的に更新し、`Resources.FindObjectsOfTypeAll<TMP_FontAsset>()` を走査してチェーンを共有させる。

## QA チェックリスト
1. `Player.log` に `Missing character 0xE...` / `Sprite 'DV' not found` が出ていないか確認。監視には `tmp/player_log_watch.txt` を tail し、キーワード一致で検出する。
2. 代表ケース（ShortStatLine/WoundLevel/PUA 先頭/既に RTF 済みの文字列）を `JpLog` 付きで確認し、ContextID が `ModelShark.Tooltip.<Style>.<Field>` に揃っているか検証する。
3. Hover と キーボード Look で `<sprite>` が付くアイコン（例: AV/DV、攻撃力、傷レベル）が全て残っているか、和訳後の余白が崩れていないかを目視チェックする。
4. `TooltipTokenizedTranslator` のトークン `⟦S0⟧` などが UI に流出していないことを `Player.log` / 画面両方で確認する。

## ログ監視
- `ModelShark.TooltipTrigger.SetText.TooltipField` の JpLog ヒット/ミスをプレフィックスごとに出力し、辞書に無い文字列を洗い出す。
- `Player.log` から以下を自動検知し、CI で失敗させる:
  - `Missing character 0xE`（PUA グリフの欠落）
  - `Sprite index out of range` / `Missing sprite`（Sprite Asset 未登録）
  - `Rich Text Tag not supported`（Allowlist 外のタグが混入している場合）

## まとめ
- Tooltip の翻訳は **gameQueue でテキストを確定 → uiQueue でタグサニタイズのみ実施** の原則で統一する。
- `<sprite>` / PUA / `{{…}}` / Unity RichText を保護したまま翻訳するため、`TooltipTokenizedTranslator` のトークン化手順を崩さないこと。
- ドキュメントと実装は常に UTF-8 (BOM 無し) で保存し、`scripts/check_encoding.py` によるチェック結果を PR に添付する。
