# Inventory / Equipment パイプライン v1

> **対象 UI**: インベントリ / 装備画面（Classic = `XRL.UI.InventoryScreen`, Modern = `Qud.UI.InventoryAndEquipmentStatusScreen`）  
> **主要アセンブリ**: `Assembly-CSharp.dll`（GameObject/Inventory APIs, `InventoryLine`, `EquipmentLine`）, `Unity.TextMeshPro.dll`（`TMP_Text`, `UITextSkin`）  
> **Contract (2025-11)**: DisplayName / カテゴリ / ラベルは Classic では `ScreenBuffer`、Modern では `UITextSkin` を経由して翻訳する。TMP へ直書きせず、`ToRTFCached` → `RTF.FormatToRTF` のキャッシュ機構を壊さない。  
> **Encoding**: `Docs/pipelines` 配下は UTF-8 (BOM 無し) 固定。

## サマリー
- Classic UI は 80×25 の `ScreenBuffer` に `StringBuilder` で直接描画する。ホットキーや重量列は桁が固定なので、翻訳済みテキストの長さが 80 桁を越えないようにする。
- Modern UI は `InventoryLine` / `EquipmentLine` の `UITextSkin.SetText(ToRTFCached)` で TMP RichText に変換する。カテゴリごとに `InventoryLineData` が生成され、uiQueue に渡される。
- 共通で `GameObject.DisplayName` を基点とするため、DisplayName を直接書き換えると他システムへ副作用が広がる。可能な限り UI 層（InventoryLine/EquipmentLine）で Hook する。

## Classic（XRL.UI.InventoryScreen）
1. `RebuildLists(GameObject go)`  
   - `go.Inventory.GetObjectsDirect()` を列挙し、`CategoryMap` / `CategorySelectionList` へホットキー/カテゴリ名/重量合計を格納。
2. `Show()`  
   - カテゴリ行: `"> a) [+] {{K|[Weapons, 3 items]}}"` のように `Markup` を挿入し、`ScreenBuffer.Write` で 80 桁の左端に揃える。  
   - アイテム行: `StringBuilder` で DisplayName を描画し、`ColorUtility.LengthExceptFormatting` で見かけの長さを計算して右端（79 列）に重量 `{{K|12#}}` を配置。
3. 入力処理 (`Keyboard.getvk`) → `InventoryActionEvent.Check` → 選択行に応じたドロップ/装備などを実行。

**翻訳ポイント**
- `InventoryScreen.RebuildLists` Prefix で `CategorySelectionListEntry.DisplayName` を翻訳（Context: `XRL.UI.InventoryScreen.CategoryLine`）。
- `InventoryScreen.Show` Prefix で `StringBuilder` に追加する定型文（"Total weight", "items hidden by filter" など）を `SafeStringTranslator` で置換。
- Classic では 1 行 80 桁なので、訳語が長い場合は省略形を決め、`ColorUtility.LengthExceptFormatting` の結果が 79 以下に収まるよう調整する。

## Modern（Qud.UI.InventoryAndEquipmentStatusScreen）
1. `UpdateViewFromData()`  
   - `GO.Inventory.Objects` から `InventoryLineData` を生成し、カテゴリ順に `FrameworkScroller` へ渡す。検索バーやフィルタ状態もここで反映。
2. `InventoryLine.setData()`  
   - `UITextSkin.SetText` を複数回呼び出し、`categoryLabel`, `itemWeightText`, `itemText` を RTF へ変換（`UITextSkin.ToRTFCached(useBlockWrap:true)`）。
3. 装備パネル (`equipmentPaperdollController`, `equipmentListController`) も `EquipmentLineData` と `UITextSkin` で同じ処理を行う。

**翻訳ポイント**
- `InventoryAndEquipmentStatusScreen.UpdateViewFromData` でカテゴリタイトル・重量ラベルを置換（Context: `Qud.UI.InventoryLine.CategoryLabel`, `Qud.UI.InventoryLine.WeightLabel` など）。
- `InventoryLine.setData` Prefix で `UITextSkin.SetText` に渡す直前に翻訳を挟み、`UIContext` のうちどの `UITextSkin` へ入るか `tooltipStyle` と同様にログへ出す。
- `UITextSkin` のキャッシュキーは「原文 + 付与オプション」で構成されるため、翻訳後に `<color>` を追加しない。どうしても色を変えたい場合は Markup 側（`{{C|...}}`）で先に処理する。

## テキスト整形ルール
- Classic:  
  - 幅 80 固定。カテゴリ行は `"> a) "` から始まり、ラベルは `[Name, N items]` の形で `[]` を維持する。  
  - 重量列は `Buffer.Goto(79 - LengthExceptFormatting(value), y)` で右寄せするため、`{{ }}` のネストを増やさない。  
  - 合計重量は `"carried {{K|/}} max lbs."` のように `{{ }}` 分割が決め打ちされている。訳語は `{}` を増やさずに言い換える。
- Modern:  
  - `UITextSkin.ToRTFCached(useBlockWrap:true)` が TextBlock 互換の折り返し（既定 72 桁）を行う。`|` 区切り（例: `categoryWeightText`）はそのまま保持して幅を揃える。  
  - 重量の単位表記 `[12 lbs.]` は角括弧+単位を含むテンプレート。単位を変更する場合は括弧も含めて翻訳し、一括で `ToRTFCached` へ渡す。

## QA
1. Classic/Modern 両方でカテゴリ・アイテム・重量列が桁からはみ出していないかを目視確認。特に日本語化で長くなる「装備」「未分類」などに注意。
2. `Player.log` の `InventoryLine` / `EquipmentLine` で `Missing glyph` や RichText エラーが発生していないか `scripts/log_watch` の結果を確認。
3. Modern UI で `InventoryLine` を大量にスクロールしても `UITextSkin` のキャッシュが再利用され、GC アロケーションが増えていないか Profiler を確認。
4. `pipelines.csv` の `Inventory` 行を更新し、ContextID と Hook 位置が最新になっているかダブルチェックする。
