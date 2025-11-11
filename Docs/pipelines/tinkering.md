# Tinkering / Cybernetics パイプライン v1

> **対象 UI**  
> - StatusScreens の `Tinkering` タブ（Build/Mod 切り替え、ビット残量、詳細ペイン）  
> - Modern Cybernetics/Cecsica 端末（`CyberneticsTerminalScreen`／`GenericTerminal` 系）
>
> **参照 ILSpy** `Qud.UI.TinkeringStatusScreen`, `TinkeringLine`, `TinkeringLineData`, `TinkeringBitsLine`, `TinkeringBitsLineData`, `TinkeringDetailsLine`, `Qud.UI.CyberneticsTerminalScreen`, `CyberneticsTerminalRow`, `CyberneticsTerminalLineData`, `XRL.UI.TinkeringScreen`, `XRL.UI.CyberneticsTerminal`, `XRL.UI.TerminalScreen`, `XRL.UI.GenericTerminalScreen`

---

## 1. Tinkering Status Screen

### 概要
- Modern Tinkering は `TinkeringStatusScreen` が `Build`/`Mod` の 2 カテゴリを持つタブを構築し、`FrameworkScroller` で `TinkeringLine` を流す。ビット残量は別 `FrameworkScroller` (`bitsController`) で `TinkeringBitsLine` を表示。
- データソースは `TinkerData.KnownRecipes`（全スキーマ）を `UpdateTinkeringData()` で仕分け。Build は `data.uiCategory`（ブループリント側のカテゴリ文字列）で折り畳み、Mod は「適用可能なアイテム」と紐付けた結果をカテゴリ風に並べる。
- 検索は `BaseStatusScreen.filterText` を `OnSearchTextChange` で受けて `searcher._sortString` に lower-case を格納。`FuzzySharp.Process.ExtractTop` で 50件までを抽出し `listItems` に絞る。  
  さらに `FilterBar` のカテゴリトグル（`filterBar.enabledCategories`) と併用されるため、訳語でも `uiCategory` が一致するように注意。
- 詳細ペインは `TinkeringDetailsLine` が `tinkeringLineData.data` と `modObject` からアイコン・説明・ビットコスト・素材リストを描画する。`ActiveCost` を更新するとビット一覧側のハイライトも再計算される。

### 主なメソッド

| 役割 | メソッド / クラス | 中身 |
| --- | --- | --- |
| レシピ仕分け | `TinkeringStatusScreen.UpdateTinkeringData` | `TinkerData.KnownRecipes` を `BuildRecipes` と `ModRecipes` に分ける。 |
| View 更新 | `TinkeringStatusScreen.UpdateViewFromData` | Build: `uiCategory` ごとに `objectCategories[category]` を構築し、カテゴリ行 (`category=true`) と子行を `listItems` へ。Mod: 各 `TinkeringLineData.applicableObjects` をビルドし、`modObject` と紐付け。いずれも Fuzzy + FilterBar で絞り込む。 |
| 行描画 | `TinkeringLine.setData` | カテゴリ行→ `[+]/[-] CategoryName [count]`。レシピ行→ `"    <DisplayName> [<BitCost>]"` または `"    <modObject.DisplayName> [...]"`, 模型対象が無ければ `<no applicable items>`。 |
| 詳細パネル | `TinkeringDetailsLine.setData` | Build: サンプル Renderable、`data.UnclippedDescription`、`BitCost`, `Ingredient` (`{{G|ﾃｻ}}` or `{{R|X}}`) などを `UITextSkin` に流す。Mod: `modObject` アイコン + `Description.GetShortDescription`, `ItemModding.GetModificationDescription` を `{{rules|...}}` で表示。 |
| ビット一覧 | `TinkeringStatusScreen.UpdateBitlocker` + `TinkeringBitsLine.setData` | `BitType.BitOrder` を回して `{{Color|Glyph Description}}` 文字列を生成、所持数と `ActiveCost` を比較して `{{K|}}/{{G|}}/{{R|}}` を切り替え。 |
| コンソール版 | `XRL.UI.TinkeringScreen` | Classic UI では `StringFormat.ClipLine` で 80×25 に描画。Modern と同じ `TinkerData.DisplayName`・`BitCost.ToString()` が使われる。 |

### データフロー（Build モード）
1. `ShowScreen` 呼び出し直後に `UpdateViewFromData()` 実行。`BuildRecipes` から `TinkeringLineData` を生成し `filterBarCategories` を初期化。
2. `FilterBar`/検索条件 → `enumerable`（表示対象）を決定。
3. `objectCategories`（カテゴリ名 → レシピ一覧）を組み、`usedCategories` をソート。
4. カテゴリごとに `categoryCollapsed` フラグを参照して `[+]/[-]` 行を作成、展開時のみ子要素を追加。
5. `controller.BeforeShow(listItems)` → `TinkeringLine.setData` が `UITextSkin.SetText` を呼ぶ。
6. 行ハイライト時 (`HandleHighlightObject`) に `ActiveCost` を更新し `UpdateBitlocker()` を再描画、`detailsLine.setData` を呼んで詳細パネル更新。

### モダイモード（Mod）
- `dataList.AddRange(ModRecipes.Select(...))` し、プレイヤーの所持品/装備に対して `ItemModding.ModKey(obj)` をキー化。該当するレシピに `applicableObjects` を追加。`filterBar` には `ModKey` 内のタグ（`"weapon"`, `"gun"`, `"[TechTier]"` 等）が並ぶ。
- リスト表示は「レシピ名カテゴリ」→展開で個別の対象アイテムという 2段構造。`TinkeringLine.text` は `"    <ItemName> [BitCost]"` で costString は `TinkeringLineData.cost` が初期化する。
- `startupModItem` が指定された状態（アイテム検査→Tinkerへ遷移など）では、そのアイテムに適用できるレシピだけを残す特別経路がある。

### 翻訳フック候補
- `TinkerData.DisplayName`, `.UnclippedDescription`, `.UICategory`  
  - ゲームデータ側を翻訳しておくと Console/Modern 両 UI で統一される。特に `UICategory` は FilterBar のキーにもなるため、辞書化しても英語キーを保持するか、`FilterBar` へ渡す表示名だけ差し替える。
- `Qud.UI.TinkeringLine.setData` (`Qud.UI.TinkeringLine.Text`, `.CategoryText`)  
  - Build/Mod 両行のラベル (`Starting cost`, `<no applicable items>` 等)・ビット表記・カテゴリ行 `[+]/[-]` の日本語化を行う最前線。
- `Qud.UI.TinkeringDetailsLine.setData` (`DescriptionText`, `ModBitCostText`, `ModDescriptionText`, `RequirementsHeaderText`)  
  - ここで `Ingredient` ラベルや `Bit Cost` ヘッダを訳し、`ItemModding.GetModificationDescription` の結果に追加フォーマットを掛ける。
- `Qud.UI.TinkeringBitsLine.setData` (`Text`, `AmountText`) / `TinkeringStatusScreen.UpdateBitlocker`  
  - ビット説明（`{{y|K  Small data disk}}` のような固定文字列）は `UpdateBitlocker` で構築しているので、この箇所をフックすると一覧・詳細の両方で同じ訳を使える。
- Console 互換性を考えるなら `XRL.UI.TinkeringScreen` の描画文字列（`"You don't have any schematics."` など）も合わせて置換しておく。

### 注意点・罠
- `TinkeringLineData.sortString` は DisplayName から色コードを削除して lower-case 化したものをキャッシュしている。翻訳後に検索／アルファソートを成立させたい場合は `set()` 内でキャッシュを破棄するか、`DisplayName` 側で恒久的に翻訳する必要がある。
- `filterBar.enabledCategories` は `uiCategory` 文字列を直接比較するため、英語→日本語に変える際は表示と内部キーを分ける（e.g. `Dictionary<string englishKey, string localizedLabel>` を管理）か、`FilterBar.SetCategories` 呼び出し前にラベルだけ差し替える。
- Mod モードのカテゴリ名は `data.DisplayName`（レシピ名）をそのまま使用するため、プレーンな訳語にすると `categoryCollapsed` のキーも変わる。既存の折り畳み状態を維持したい場合は英語キーを保持しつつ表示だけ置換する。
- `TinkeringBitsLine` は `ActiveCost` 参照により色替えを行う。翻訳フックで例外が出ると `ActiveCost` が null のままになり表示が壊れるので、null チェック必須。

### テスト観点
1. Build/Mod 両モードで検索＆カテゴリフィルターが期待通りヒットするか（英日混在でも Fuzzy 検索が働くか）。
2. Mod レシピに対して `modDescriptionText` の `{{rules|...}}` マークアップが崩れないか。
3. ビット残量が 0 → 必要数より少ない場合、`amountText` が赤く変わるか。
4. レシピ詳細の「Ingredient」列で複数素材が存在する場合でも `-or-` 区切りと色分けが維持されるか。

---

## 2. Cybernetics / Generic Terminal

### 概要
- Modern 端末 UI は `CyberneticsTerminalScreen` が `FrameworkScroller` に `CyberneticsTerminalLineData` を流し、1 行目に本文 (`RenderedTextForModernUI`) を、以降に選択肢を表示する。`CyberneticsTerminalRow` にはタイプライタ風カーソル演出があり、`HandleTextComplete` が呼ばれると `TerminalScreen.TextComplete()` を叩いて次の画面に進む。
- 元データは console 時代の `XRL.UI.CyberneticsTerminal` / `TerminalScreen` / `GenericTerminal`.  
  - `TerminalScreen.MainText` と `Options` は `StringFormat.ClipText`（幅 67, KeepNewlines）でクリップされた文字列を `RenderedText` にまとめる。  
  - Hack 状態 (`HackActive`) 時には `TextFilters.Leet` を通したり、`HackOption` を `{{R|CTRL-ENTER ...}}` でラップする処理が入る。
- Modern UI でも最終的な文字列はこれら `TerminalScreen`/`GenericTerminalScreen` が用意した `RenderedTextForModernUI` / `Options` をそのまま `UITextSkin.SetText` に渡している。よって翻訳はクラシックと同じ場所で処理するのが最も安全。

### 主なメソッド

| 役割 | メソッド / クラス | 中身 |
| --- | --- | --- |
| 端末データ収集 | `XRL.UI.CyberneticsTerminal.CurrentScreen` setter | 現在画面をセット → インベントリからインプラントやクレジットウェッジを収集し、ライセンス数・残数を計算。`TerminalScreen.Update()` を即時実行。 |
| 文字列レンダリング | `TerminalScreen.Update`, `GenericTerminalScreen.Update` | `StringFormat.ClipText(MainText, 67)` で本文を整形し、選択肢を `A. ...` / `{{R|CTRL-ENTER...}}` 形式で結合して `RenderedText` を作成。 |
| Modern UI への橋渡し | `CyberneticsTerminalScreen.GetMenuItems` | 先頭要素に本文 (`RenderedTextForModernUI`) を挿入し、その後 `Options` を `CyberneticsTerminalLineData` として列挙。 |
| 行描画 | `CyberneticsTerminalRow.Update` | `data.Text` の内容を `_` カーソル付きで1文字ずつ描画し、選択された行には背景を点灯。 |
| 選択処理 | `CyberneticsTerminalScreen.HandleSelect` | `OptionID >= 0` の行で `cyberneticsTerminal.CurrentScreen.Activate()` / `genericTerminal.currentScreen.Activate()` を呼び、再び `BeforeRender` → `Show()` でリストを作り直す。 |

### 翻訳フック候補
- `XRL.UI.TerminalScreen.Update` / `GenericTerminalScreen.Update`  
  - Console/Modern 共通の本文・選択肢を生成する箇所。ここで `MainText`, `Options[i]` を翻訳すれば 2 系統の UI 両方に反映される。`StringFormat.ClipText` 前に翻訳することで行幅計算を正しく行える。
- `XRL.UI.CyberneticsTerminalScreen.GetMenuItems` / `CyberneticsTerminalLineData`  
  - 追加のステータス行や脚注を挿入したい場合はここで `FrameworkDataElement` を増やす。翻訳専用なら上記 TerminalScreen 側の方が望ましい。
- 端末ごとの `TerminalScreen.MainText` ソース（例: `GritGateTerminalScreen*`, `CyberneticsScreen*`)  
  - `MainText` や `Options` は各派生クラスの `OnUpdate()` で組み立てるため、文面そのものを置換したい場合は派生クラス／データテーブル側をパッチする。

### 注意点
- `CyberneticsTerminalRow` のタイプライタ効果は `Text` プロパティを逐次切り詰めるため、`Markup` タグを途中で切ると不正なタグ列になる。`RenderedTextForModernUI` は `StringFormat.ClipText` 済みでタグ整合が保証されているので、翻訳時も `Markup.Transform` に準拠したタグを維持する。
- `Options` の先頭に自動的に `A. ` `B. ` が付く（HackOption では `{{R|CTRL-ENTER ...}}`）ため、選択肢本文は文章だけを翻訳する。接頭辞を変えたい場合は `TerminalScreen.Update` 内で処理。
- ハック状態（`Terminal.HackActive`）では `TextFilters.Leet` で変換される。翻訳後の文字列に Leet 変換を適用する場合、未翻訳の英単語が無いと挙動がわかりづらくなるので QA 必須。

### テスト観点
1. すべての Cybernetics メニュー（Install/Remove/Hackなど）で本文と選択肢がクリップ崩れせず表示されるか。
2. タイプライタ演出中にボタン入力（Skip）を行っても `_` カーソルが残らないか。
3. HackOption の `CTRL-ENTER` 表記や `Leet` 化された文字列が翻訳後も読めるか。
4. GenericTerminal（例えばコンカラークイズ端末など）でも同じ翻訳が適用され、`BACK_BUTTON` のホットキーが壊れないか。
