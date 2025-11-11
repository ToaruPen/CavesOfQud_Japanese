# Help / Options / Keybinds パイプライン v1

> **対象 UI**
> - `ModernHelp` (`Qud.UI.HelpScreen`)
> - `ModernOptionsMenu` (`Qud.UI.OptionsScreen`)
> - `Keybinds` (`Qud.UI.KeybindsScreen`)
> - 旧来のコンソール版 (`XRL.Help.XRLManual`, `XRL.UI.OptionsUI`, `XRL.UI.KeyMappingUI`)
>
> **主要ソース** `XRL.Help.XRLManual`, `Qud.UI.HelpRow`, `XRL.UI.Options`, `Qud.UI.OptionsRow` + `Options*Control`, `CommandBindingManager`, `Qud.UI.KeybindRow`
>
> **関連** `Docs/pipelines/characterbuild.md`（Embark overlay / `MenuOption` 共通部品）

---

## 1. 共通構成（Embark overlay + Framework UI）
- 3 画面とも `EmbarkBuilderModuleBackButton`/`FrameworkScroller`/`FrameworkSearchInput` 等、Embark Builder 系プレハブの再利用で構築されている。`Back`/`Next` ボタンやホットキーガイド (`HorizontalMenuScroller`) の文言をまとめて差し替えるなら `EmbarkBuilderOverlayWindow.BackMenuOption` / `NextMenuOption` / `MenuOption.getMenuText` を翻訳するのが最小コスト。
- `MenuOption.Description` に直接英語が埋め込まれているケース（Help の “Toggle Visibility”、Options の “Collapse All” など）は、各 `UpdateMenuBars` で `MenuOption` を生成する瞬間に差し替える。
- `FrameworkSearchInput` は `<search>` プレースホルダーと `PopupTitle = "Enter search text"` を内部で持つ。`SearchInput.context.inputText` は `CmdFilter` で入力されるため、翻訳後も `FuzzySharp.Process.ExtractTop` にヒットするよう `SearchWords` をローカライズ済み文字列で再構築する必要がある。
- 左側カテゴリー (`LeftSideCategory.setData`) は `HelpDataRow.CategoryId` / `OptionsCategoryRow.CategoryId` / `KeybindCategoryRow.CategoryDescription` をそのまま `{{C|...}}` に流し込む。キー（topic ID や Option.Category）と表示名（多言語ラベル）を分けて管理する辞書を用意する。

---

## 2. Help Screen（ModernHelp / XRL Manual）

### データソース
1. `XRL.Help.XRLManual` が `DataManager.YieldXMLStreamsWithRoot("help")` で `<topic name="...">` を列挙し、`Manual.Pages` / `Manual.Page` に `XRLManualPage` を溜め込む。`name` 属性がカテゴリー ID、本文は `[[ ]]` で旧 UI の強調タグを表す。
2. 旧 UI (`XRLManual.RenderPage`) は `ScreenBuffer.Write` で `Pages[topic].Lines` を 80x25 へ直描画。ここを翻訳するとコンソールとモダン UI 両方に効く。

### Modern UI フロー
1. `HelpScreen.HelpMenu()` が `The.Manual.Pages` を走査して `HelpDataRow` リストを作成 (`Description=CategoryId`, `HelpText=page.GetData()`).
2. `helpScroller.BeforeShow` で各 `HelpRow` に `HelpDataRow` が渡される。
3. `HelpRow.setData`:
   - `categoryDescription.text = "{{C|" + Description.ToUpper() + "}}"` → 日本語だと大文字化が破綻するため、Prefix/Postfix を含めて `SetText` するハーモニーパッチ側で独自処理する。
   - `description.text = helpDataRow.HelpText`。`~CmdName` プレースホルダーを `ControlManager.getCommandInputFormatted` で置換 (`keysByLength` 降順ループ) するので、翻訳後も `~` プレースホルダーを保持する。
   - 折り畳み状態に応じて `[+]` / `[-]` を `categoryExpander` へ流し込む。
4. `LeftSideCategory` は `CategoryId` を `{{C|}}` で描画。ここも翻訳済みラベルを `HelpDataRow.Description` に保持しておけば共通化できる。
5. `hotkeyBar` は `MenuOption { Description = "navigate" }`, `{ Description = "Toggle Visibility" }` の 2 件のみ。`ControlManager.getCommandInputDescription` によって `[Esc] Back` 表示が追加される。

### フック候補
- **データ読込**: `XRL.Help.XRLManual` (`LoadTopic`) で `<topic name>` を辞書キーとして保存するタイミングで `ManualTopic.<id>.Title` / `.Body` を翻訳し `XRLManualPage.Data` を差し替える。これで旧 UI / 新 UI 双方が同じ訳を参照する。
- **UI レイヤ**: `Qud.UI.HelpRow.setData` Prefix/Postfix で `Description`、`HelpText` をローカライズ済みストリングに置換、`keysByLength` の置換前に `~Command` トークンを付与し直す。`Description.ToUpper()` で壊れる場合は、Harmony で `ToUpper` 呼び出しを差し替えて `TranslationUtility.ToUINarrowCaps(string, locale)` のような関数に逃がすとよい。
- **Hotkey 表示**: `HelpScreen.UpdateMenuBars` 内の `MenuOption.Description` を辞書化 (`LocalizedOverlayText.Navigate`, `LocalizedOverlayText.ToggleVisibility`)。

### 注意点
- `helpScroller` は `ScrollOnSelection = ShouldScrollToSelection` で仮想化しているため、長文トピックでも `ScrollViewCalcs` がスクロールを制御する。フック内での例外は `UI thread` を停止させるので try/finally でラップ。
- `categoryExpanded` は `Dictionary<string,bool>`。キーは topic ID なので、翻訳後も ID 文字列は変更しないこと。
- `XRLManualPage.GetData(StripBrackets=true)` が `[[` を除去する。必要なら `<rtf>` 変換済み文字列を返す翻訳ユーティリティを挟む。

### チェックリスト
1. 旧 UI (`Shift+?` 相当) と ModernHelp の双方で同じ用語/レイアウトになるか。
2. `~` 置換（特に `~Highlight` → Alt）と日本語テキストが競合しないか。
3. 折り畳み/展開 (`CategorySelect`) が翻訳済みタイトルでも正常に動作するか。

---

## 3. Options Menu（ModernOptionsMenu / Options）

### データロード
- `Options.LoadOptionNode(XmlDataHelper)` が `<Option>` を1件ずつ `GameOption` オブジェクトに展開。ここで `DisplayText` / `Category` / `SearchKeywords` / `DisplayValues` / `<helptext>` などをすべて揃える。**この時点で翻訳すればコンソール (`OptionsUI`) とモダン UI の両方が恩恵を受ける**。
- `Option.Values="foo|Foo Display"` のような書式で英語 UI 表示が定義されている。`DisplayValues` にだけ訳文を詰めることもできる。

### Modern UI (`Qud.UI.OptionsScreen`)
1. `OptionsMenu()` が `Options.OptionsByCategory` を走査。カテゴリの先頭に `OptionsCategoryRow`、続けて実データ (`OptionsCheckboxRow`/`OptionsSliderRow`/`OptionsComboBoxRow`/`OptionsMenuButtonRow` 等) を挿入。
2. `FilterItems()` は `FuzzySharp.Process.ExtractTop` を使用。`SearchWords`（`Category + DisplayText + SearchKeywords`）を `.ToLower()` して比較するため、日本語検索を成立させるには翻訳済み文字列を格納しつつ `ToLowerInvariant` 前に `CultureInfo` を注入する。
3. `OptionsRow.setData` が該当タイプのプレハブ（`OptionsCheckboxControl`, `OptionsSliderControl`, `OptionsComboBoxControl`, `OptionsButtonControl` 等）を有効化し `setData` を転送。
4. 各 `*Control.Render` は下記の通り文字列を `UITextSkin` へ流す。

| Control | 表示内容 | 補足 |
| --- | --- | --- |
| `OptionsCheckboxControl.Render` | `[■] Title` or `[ ] Title`（選択中は `{{W|}}` 強調） | `Options.SetOption` へ Yes/No を書き戻す。`MenuOption Description = "Toggle Option"` も翻訳対象。 |
| `OptionsSliderControl.Render` | `text.SetText(data.Title)` + 数値ラベル + `Slider` | `NavigationContext` に `Change Value / Save / Cancel` を英語で持つ。 |
| `OptionsComboBoxControl.Render` | ラベル `text.SetText(data.Title)`、下部リスト `MenuOption { Description = "{{c|Display}}" }` | 選択肢ごとにリストを生成するので `MenuOption` もローカライズ。 |
| `OptionsButtonControl.Render` | `text.SetText(data.Title)` | `OptionsMenuButtonRow.OnClick`（例: “Open keybindings”）の後に `Show()` を呼び直す。 |

5. `TooltipTrigger.SetText("BodyText", RTF.FormatToRTF(data.HelpText))` により、長文ヘルプが `Tooltip` パイプラインへ流れる。
6. `defaultMenuOptions` = `[navigate]`, `[Collapse All]`, `[Expand All]`, `[Select]`, `[Help]`。英語がリスト内にハードコードされている。
7. 検索バー (`FrameworkSearchInput`) と “Show Advanced Options” トグルは `topHorizNav` でグルーピングされ、`CmdFilter` でフォーカスを奪う。

### コンソール版 (`OptionsUI`)
- `ConsoleTreeNode<OptionNode>` を使った 80x25 レイアウト。`OptionNode.Option.DisplayText` / `Category` / `Options.GetOption()` をそのまま描画。`ScreenBuffer` へ `[-] <Category>`、`[ ] <Option Display>` などを出力する。翻訳は `GameOption` の段階で同じ値を共有するのがベター。

### フック候補
- **データ**: `Options.LoadOptionNode` Prefix で `(option.ID, fieldName)` を `Translator.Lookup("Options.<id>.Display")` のようなコンテキスト ID へマップし、`DisplayText` / `DisplayValues[]` / `HelpText` / `Category` / `SearchKeywords` を差し替える。
- **UI**: `OptionsScreen.UpdateMenuBars` と各 `MenuOption` 生成箇所で `Description` を辞書化。`OptionsCheckboxControl.Render` では `[■]` や `[ ]` のグリフを残したままラベルのみを翻訳する必要がある。
- **検索**: `OptionsScreen.FilterItems` で `searcher.SearchWords` を `CultureInfo.InvariantCulture.TextInfo.ToLower` ではなく Unicode casefold を噛ませる。翻訳辞書が `Hiragana/Katakana` を混合する場合は `KanaConverter` を導入する案もある。
- **Tooltip**: `TooltipTrigger.SetText` に渡す時点で RT フラグメントを持つので、翻訳関数の出力も `Markup` ベース（`{{C|}}` 等）に合わせる。

### 注意点
- `advancedOptionsCheck` は `OptionShowAdvancedOptions` を `OptionsMenu()` で抜き出し別枠に表示している。ここを翻訳しないとテキストだけ英語になる。
- `OptionsMenuButtonRow` で `OnClick` を await した直後に `Show()` を呼ぶため、パッチの例外があるとメニューが閉じなくなる。`HandleSelect` 内で try/catch を入れる。
- `OptionsUI` は `Keyboard.MouseEvent` 由来の文字列（`"Command:CmdHelp"` 等）をローグライクに渡すため、`Popup` パイプラインの翻訳とも連動する。

### チェックリスト
1. Modern / Console 両 UI のカテゴリ名・オプション名・ヘルプが一致するか。
2. 検索バーに日本語を入力してヒットするか (`FuzzySharp` によるスコアリング含む)。
3. トグル／スライダー／ボタン／ドロップダウン操作が翻訳後も問題なく適用できるか。
4. “Show Advanced Options” を翻訳した状態でオン/オフしてもクラッシュしないか。

---

## 4. Keybinds Screen（ModernKeybinds / KeyMappingUI）

### データロード
- `CommandBindingManager.HandleCommandNode` が `Commands.xml` を読み、`GameCommand.DisplayText` / `Category` / `Layer` を設定。翻訳はここで行っておけば `KeybindsScreen` / `KeyMappingUI` / `ControlManager.getCommandInputDescription` が一貫する。
- `CategoriesInOrder` の順序は英語キーで決まるため、翻訳ラベルは別に保持する必要がある。

### Modern UI (`Qud.UI.KeybindsScreen`)
1. `KeybindsMenu()`：`CommandBindingManager.CategoriesInOrder` をループし、`KeybindCategoryRow` + `KeybindDataRow`（最大 4 つのバインド列）を `menuItems` に積む。
2. `FilterItems()`：`KeybindDataRow.SearchWords = "<category> <DisplayText>"`。`Process.ExtractTop`（最大 50）でファジー検索→カテゴリ単位で `filteredItems` を構成。
3. `keybindsScroller.BeforeShow` → `KeybindRow.setData`:
   - カテゴリ行 (`KeybindCategoryRow`) は大文字＋ `categoryExpander` `[+]/[-]`。
   - コマンド行 (`KeybindDataRow`) は `description.text = "{{C|"+DisplayText+"}}"`、各 `KeybindBox.boxText = "{{w|bind}}"` or `{{K|None}}`。
   - `KeybindBox` は `editMode`（再バインド時）で `"{{R|press key...}}"` を表示。
4. `hotkeyBar` のメニュー: `[navigate]`, `[select]`, `[remove keybind]`, `[back?]`, `[restore defaults]`。英語固定。
5. `searchInput`（`CmdFilter`）、`inputTypeContext`（キーボード/ゲームパッドの切り替え）、`backButton` などが `NavigationContext` で束ねられる。
6. `SelectInputType()` は `Popup.PickOptionAsync("Select Controller", ..., ControlTypeDisplayName.Values)` を表示。`ControlTypeDisplayName` 辞書自体が `"Keyboard && Mouse"`, `"Gamepad"` を保持。
7. Rebind フロー (`HandleRebind`):
   - `Popup.ShowAsync` / `Popup.ShowYesNoAsync` 系メッセージ（英語）が多数。`Popup` パイプラインのコンテキスト ID と合わせて翻訳する。
   - `bChangesMade` フラグで `Exit()` 時に "Would you like to save your changes?" を表示。

### コンソール版 (`KeyMappingUI`)
- `ConsoleTreeNode<KeyNode>` で 80x25 表示。`[-] Category` / `"{{c|Bind}}"` などを直接 `ScreenBuffer` へ書く。翻訳は `GameCommand.DisplayText` と `CommandBindingManager.Categories` を共通化することで再利用可能。

### フック候補
- **データ**: `CommandBindingManager.HandleCommandNode` で `DisplayText`, `Category`, `Display` を翻訳し、カテゴリ → 表示名マップを `LocalizedCommands.GetCategoryLabel(string id)` のように保持。
- **UI**:
  - `KeybindRow.setData`：カテゴリ見出しの `.ToUpper()` を差し替え、日本語文字列を保持。
  - `KeybindRow.box*.boxText` に書き込む “None” を辞書化。
  - `KeybindsScreen.UpdateMenuBars` / `SelectInputType` / `Exit` / `HandleMenuOption` の `MenuOption.Description` や `Popup` 引数を翻訳。
  - `KeybindsScreen.inputTypeText.SetText("{{C|Configuring Controller:}} ...")` を直接差し替える。
- **検索**: `KeybindDataRow.SearchWords` を翻訳済みテキストで再構築し、`Process.ExtractTop` の `scorer` に `CultureAwareLower()` を挟んでヒット率を確保。
- **ログ**: Rebind 失敗時の `MetricsManager.LogKeybinding` などは英語のままで良いが、`Popup` メッセージが翻訳済みかどうかを `JpLog` で観測する。

### 注意点
- `VisibleWindowScroller` は仮想リスト。`keybindsScroller.GetPrefabForIndex` で取った `KeybindRow` を保持し続けると GC を阻害するため、翻訳辞書は ID ベースで参照する。
- `WorldGamepad` 未接続時は `"{{c|<no controller detected>}}` が埋め込まれる（`inputTypeText`）。ここも翻訳する。
- `CommandBindingManager` はゲーム起動時に一度だけロードされる。Mod 側で再読み込みすると `Unity InputSystem` の `ActionMap` がリセットされる点に注意。

### チェックリスト
1. キーボード/ゲームパッド双方で `Bind1-4` の表示と再バインドガイドが翻訳されているか。
2. 検索（`CmdFilter`）に日本語を入れてヒットするか。
3. `Remove Binding` / `Restore Defaults` の `Popup` 文言が翻訳されているか。
4. 旧 UI (`KeyMappingUI`) で同じ名称が表示されるか。

---

## 5. 次アクション / 実装メモ
1. `pipelines.csv` へ以下の ContextID を追加:
   - `Help` : `XRL.Help.XRLManual.RenderPage.TopicLine`, `Qud.UI.HelpRow.Description`.
   - `Options` : `XRL.UI.OptionsUI.Show.TreeLine`, `Qud.UI.OptionsRow.ControlText`.
   - `Keybinds` : `XRL.UI.KeyMappingUI.Show.TreeLine`, `Qud.UI.KeybindRow.BindBoxes`.
2. `Docs/pipelines/hook_plan.md` の優先度表へ `Help / Options / Keybinds` を追加し、Harmony パッチ案（データ層 & UI層）を紐付け。
3. 翻訳辞書キーの指針: `ContextID = "<Subsystem>/<Entity>/<Field>"` 形式（例: `Options/OptionShowAdvancedOptions/Title`）。`SearchWords` など複合フィールドは `"{Title} {Keywords}"` を再生成する。
4. `JpLog` に `NavigationController` 系 UI から収集した文字列を記録し、翻訳漏れを判定する。

## 6. ������ (2025-11-11)
- `QudJP.Patches.HelpRowLocalizationPatch`
  - `Help.Topic.<id>.Title/Body` �� UI �w�Ŗ|�󂵁A`~Command` �u��������O�Ɋm��B`LeftSideCategory` �� Postfix �ŃJ�e�S�����[�����������x�����g�p�B
- `QudJP.Patches.OptionsLocalizationPatch`
  - `Options.LoadOptionNode` Postfix �� `Options.Option.<id>.Title/Help/Value.*` �����O�|�󂵁A`DisplayValues` �� `Values` ���Q�Ƃ��Ă���ꍇ�� clone ���� UI �\���̂ݍ����ւ��B
  - `OptionsScreen.FilterItems` Prefix �� `SearchWords` �ɓ��{��J�e�S��/�^�C�g����ǉ��A`OptionsCategoryControl.Render` Postfix �� `.ToUpper()` �ɂ��j�����㏑���B
- `QudJP.Patches.KeybindsLocalizationPatch`
  - `CommandBindingManager.HandleCommandNode` Postfix �� `CommandBinding.<id>.Display` �� `CommandBinding.Category.<id>.Label` ���L���b�V���B
  - `KeybindRow.setData` �ŃJ�e�S�����o���E`{{K|None}}` �v���[�X�z���_�[�������ւ��A`KeybindsScreen.QueryKeybinds` Postfix �Ō�����E���̓f�o�C�X���x���E�o�i�[�������X�V�B
  - `FrameworkSearchInput` Update Postfix �� `<search>` �v���[�X�z���_�[�ƃ|�b�v�A�b�v�^�C�g�� (`FrameworkSearchInput.*`) �𓝈�B
- `MenuOptionLegendLocalizer` �� literal fallback �� `Collapse All`/`Expand All`/`Toggle Option`/`Toggle Visibility`/`remove keybind` ����ǉ����AHotkey Bar �ł��������f�B
