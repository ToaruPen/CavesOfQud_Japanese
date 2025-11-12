# Inventory / Equipment パイプライン仕槁Ev1

> **画面 / 部佁E** インベントリ�E�裁E��管琁E 
> **出劁E** Console�E�EInventoryScreen`�E�E/ Unity�E�EInventoryAndEquipmentStatusScreen`�E�E
> **Contract (2025.11)** DisplayName/InventoryLine �Ŗ|����m�肳���AUITextSkin/TMP_Text �� Prefix �ł͍Ė|�󂵂Ȃ��BTransform/RTF/Clip ���O�̌o�H�������t�b�N�ΏۂƂ���B
## 概要E
- **クラシチE�� UI**�E�EOptions.ModernUI=false` また�E `ModernCharacterSheet=false`�E�では `XRL.UI.InventoryScreen` ぁE`ScreenBuffer` 上にカチE��リ一覧・アイチE��行�E重量を描画する、E- **Modern UI** では `Qud.UI.InventoryAndEquipmentStatusScreen` ぁEStatusScreens 冁E�Eタブとして表示され、`InventoryLine` / `EquipmentLine` の `UITextSkin` を経由して TextMeshPro にリチE��チE��ストを流し込む、E- 双方とめE`GameObject.DisplayName`�E�Earkup を含む�E�を基礎にしており、翻訳は DisplayName / カチE��リ吁E/ UI ラベルに挿入する、E
## 主なクラス / メソチE��

| フェーズ | クラシチE�� (Console) | Modern UI (Unity) |
| --- | --- | --- |
| 生�E | `InventoryScreen.RebuildLists` ぁE`GameObject.Inventory` の冁E��をカチE��リ別に構篁E(`CategoryMap`, `CategorySelectionList`) | `InventoryAndEquipmentStatusScreen.UpdateViewFromData` ぁE`InventoryLineData` のリストを生�E (`GO.Inventory.Objects`, `filterBar`, `categoryWeight` 筁E |
| 整形 | `ScreenBuffer.Write` へ渡す直前に `StringBuilder` で `{{K|[Category]}}`, `key)` 等�E Markup を絁E��立て。重量�E `{{Y|}}` 付き | `InventoryLine.setData` ぁE`UITextSkin.SetText` で `categoryLabel`, `itemWeightText`, `text` を更新。`UITextSkin` 冁E��で `ToRTFCached` ↁETMP RichText |
| 描画 | `ScreenBuffer.SingleBox` + `ScreenBuffer.Write`; 行�E左右に `ColorUtility.LengthExceptFormatting` で位置合わぁE| `FrameworkScroller.BeforeShow` ぁE`InventoryLine` プ�Eルを回し、`UITextSkin`�E�EMP�E�へ適用、Equipment パネルは `EquipmentLineData` 経由 |
| 付帯惁E�� | `InventoryScreen.Show` が「Total weight」「items hidden by filter」等を `StringBuilder` で生�E | `InventoryAndEquipmentStatusScreen.weightText` / `priceText` ぁE`{{C|...}}` など Markup 付き斁E���EめE`UITextSkin` に渡し、TMP で表示 |

## チE�Eタフロー

### Console (`InventoryScreen`)
1. `RebuildLists(GO)` ぁE`inventory.GetObjectsDirect()` を走査し、`CategoryMap` / `SelectionList` を構築（フィルタと `GameObject.GetInventoryCategory()` で刁E��）、E2. カチE��リ行�E `CategorySelectionList.Add(hotkey, new CategorySelectionListEntry(...))` の形で保持。各カチE��リに `Weight` / `Items` を集計、E3. `Show` ループ�Eで `Buffer.Write(" > key) ...")` としてカチE��リ�E�アイチE��行を描画。`gameObject.DisplayName` は Markup を含んだまま `ScreenBuffer` へ渡る、E4. 右端の重量列�E `StringBuilder.Append(" {{K|12#}}")` 等で作�E。合計重量�E `Buffer.Goto(79 - LengthExceptFormatting(...))` で右寁E��、E5. ユーザー操作！Erop/Eat/Filterなど�E��E `Keyboard.getvk` で処琁E��選択された行に応じて `InventoryActionEvent.Check` を呼び、忁E��に応じ `ResetNameCache`、E
### Modern UI (`InventoryAndEquipmentStatusScreen`)
1. `UpdateViewFromData` で `GO.Inventory.Objects` を�E挙し、`InventoryLineData` プ�Eルを取征EↁEカチE��リ別に `objectCategories` へ格納。`filterBar` / `SearchMode` で絞り込み、E2. カチE��リ行�E `InventoryLineData.set(category: true, ...)` で `categoryName`, `categoryWeight`, `categoryAmount` を保持。アイチE��行�E `displayName = go.DisplayName` めElazily 取得、E3. `inventoryController.BeforeShow(listItems)` がスクロール UI に行データをバインド。各 `InventoryLine` ぁE`setData` 冁E�� `UITextSkin.SetText` を呼んで TMP RichText 化、E4. 裁E��欁E(`equipmentPaperdollController`, `equipmentListController`) も同様に `EquipmentLineData` ↁE`UITextSkin`。ドラチE�� & ドロチE�EめE�EチE��キーめE`InventoryLine` に雁E��E��E5. 表示用ラベル: `priceText.SetText("{{B|$NN}}")`, `weightText.SetText("{{C|carried{{K|/max}} lbs.}}")`。`UITextSkin` ぁE`ToRTFCached`�E�E `RTF.FormatToRTF` + キャチE��ュ�E�で TMP 互換表現に変換、E
## 整形規則

- Console:
  - 幁E80 ÁE25 前提。カチE��リ一覧は画面左から `"> key) [+] {{K|[Name, N items]}}"` 形式。`ColorUtility.LengthExceptFormatting` を使って右端の重量列を合わせる、E  - アイチE��行�E終端は `Buffer.Write(stringBuilder8)` で `{{K|12#}}`�E�重釁E+ 単位）を `80 - length` の位置に描画、E  - 合計重量行も `{{Y|carried}} {{y|/}} {{currentMaxWeight}} lbs.` として Markup 付与。折り返しは `ScreenBuffer` 側で行わなぁE��め、翻訳は 1 行で収める、E- Unity:
  - `UITextSkin` ぁE`text.ToRTFCached(blockWrap)` で `<color=#RRGGBBAA>` 等�E RichText へ変換。`useBlockWrap=true` なめE`TextBlock` 相当�E幁E��限（既宁E72�E�を適用、E  - カチE��リ重量チE��スト�E `categoryWeightText.SetText($"|{amount} items|{weight} lbs.|")` のように `|` 記号を区刁E��に使ぁE��スキン冁E��で等幁E��に表示される、E  - アイチE��重量は `"[123 lbs.]"` の固定パターン。翻訳で単位位置を変える場合�E `[]` の括弧めE��値部刁E��保持する、E
## 同期性

- どちらも **ゲームスレチE�� (sync)** で実行。`InventoryScreen.Show` は `GameManager.Instance.PushGameView("Inventory")` でループし、`Keyboard.getvk` を直読みする、E- Modern UI めE`StatusScreensScreen`�E�Enity�E��Eで動くが、`UpdateViewFromData` はゲームスレチE��でリストを構築し、その征E`UITextSkin` ぁE`Apply()` するときに UI スレチE��へ反映される。翻訳フックはゲームスレチE��側�E�EInventoryLineData.displayName` 生�E時など�E�で差し込むのが安�E、E
## 置換安�E点�E�推奨フック�E�E
- `GameObject.DisplayName` / `InventoryLineData.displayName`  
  - ContextID: `XRL.World.GameObject.DisplayName.Inventory`.  
  - DisplayName は両 UI が�E有する�Eで、ここで翻訳すれば Console/Unity 一括で反映、E- `XRL.UI.InventoryScreen.RebuildLists`�E�カチE��リラベル, フィルタメチE��ージ�E�E 
  - ContextID: `XRL.UI.InventoryScreen.Category.Name`, `InventoryScreen.Show.TotalWeight`, など、E 
  - `StringBuilder` で直接英語文を作ってぁE��箁E��を置き換える、E- `Qud.UI.InventoryAndEquipmentStatusScreen.UpdateViewFromData`  
  - ContextID: `Qud.UI.InventoryAndEquipmentStatusScreen.CategoryLabel`, `.WeightText`, `.PriceText`.  
  - `SetText` に渡すフォーマット文字�Eをここで翻訳する。`{{` `}}` / `[]` / `|` は UI ロジチE��が前提にしてぁE��ため崩さなぁE��E- `Qud.UI.InventoryLine.setData`  
  - ContextID: `Qud.UI.InventoryLine.CategoryToggle`, `.ItemWeightLabel`.  
  - カチE��リ `[+]` / `[-]` など UI チE��ストを変更したぁE��合に利用、E
## 例文 / ト�Eクン

- カチE��リ: `"> a) [+] {{K|[{{Y|Weapons}}, 5 items]}}"`  
- アイチE��衁E `"   b) dagger"` + `" {{K|2#}}"`�E�右端�E�E 
- Total weight: `"Total weight: {{Y|125}} {{y|/}} 250 lbs."`  
- Modern UI weight: `weightText = "{{C|135{{K|/200}} lbs. }}"`  
- Modern UI price: `priceText = "{{B|$45}}"`  
- カチE��リ重量: `categoryWeightText = "|3 items|24 lbs.|"`

## リスク

- Console 版�Eハ�EドコーチE��ングされた位置合わせに依存するため、訳斁E��長ぁE��右端の重量列が潰れる。特にカチE��リ行！EBuffer.Goto(79 - length, y)`�E��E 80 列制限がシビア、E- Modern UI のカチE��リラベルは `hotkey)` と `[-]` / `[+]` を含んでぁE��。翻訳で頁E��を変えるとキーヒント解析！Edictionary.Add(key, num)`) とズレる�Eで注意、E- `UITextSkin` の `blockWrap` は既宁E72。長斁E��差し込むと TMP で自動折り返しされるが、`HotkeySpread` の位置めE��イコンとの横幁E��算が崩れる可能性がある、E- 価格�E�重量ラベルは `{{...}}` を褁E��ネストしてぁE��。翻訳時に波括弧を崩すと `ToRTFCached` のキャチE��ュキーが変わり、RichText が壊れる、E
## チE��ト手頁E
1. **クラシチE�� UI**: `Options.ModernUI=false` でゲームを起動し、`i`�E�Enventory�E�を開く。カチE��リ展開/折り畳み、E��量表示、フィルタ (`Ctrl+F`) を操作して斁E��とレイアウトを確認、E2. **Modern UI**: `Options.ModernUI=true` でキャラクター画面 (`Esc` ↁECharacter / Equipment) を開き、StatusScreens 冁E�E「Equipment」タブをチェチE��。カチE��リ collapsible、検索、トグル�E�Eaperdoll/List�E�を操作しながら翻訳が崩れなぁE��確認、E3. `priceText` / `weightText` / `categoryWeightText` ぁETMP RichText エラーを�EしてぁE��ぁE�� `Player.log` を確認、E4. `Translator/JpLog` に ContextID を追加し、カチE��リ名�E重量ラベルのヒット状況を収集して抜け漏れを検�E、E
## Context �o��

- `Qud.UI.InventoryAndEquipmentStatusScreen.WeightText` / `.PriceText` �Ńw�b�_���x���� `{carried}`, `{capacity}`, `{value}` �g�[�N���t���̂܂܎����Ǘ��B�|���ɒl�𖄂ߍ���� UITextSkin �ɓn���B
- `Qud.UI.InventoryLine.CategoryWeightText`�i����� `.WeightOnly`�j/ `Qud.UI.InventoryLine.ItemWeightLabel` �̓J�e�S���s�E�A�C�e���s�̏d�ʃo�b�W�p ContextID�B`{items}` �� `{weight}` �̃g�[�N����ێ�����B

- HookGuard: `InventoryParamMapCache` + `TranslationContextGuards` �� `InventoryLine.setData` ����擾���� `categoryName` / `displayName` / `weight` �� EID ���ƂɃL���b�V�����AuiQueue ���� `TMP_Text` / `UITextSkin` �ł͈�v�m�F��� Translator ���X�L�b�v���� DisplayName 2 �x�ڂ̖|��� Player.log �� `MISS` �x����}������B

## HookGuard / ParamMap
- Modern UI ??? `UiEntryInstrumentationPatch` ?? `InventoryLine` ? `UITextSkin` ? `UIContext` EID ??A `InventoryParamMapCache` ?? DisplayName/CategoryWeight/ItemWeight ? `ToRTFCached` ??????????????
- `TMP_Text.set_text` Prefix ???v???[?X?_?[?? (`TooltipParamMapCache` ?????) ?????? `TranslationContextGuards` ?? `TMP.InventoryLine.*` ?? Skip ????????uiQueue ??????? Hotkey/CategoryExpand ? `MISS` ???????
