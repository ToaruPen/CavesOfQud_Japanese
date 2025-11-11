# Character Build / Embark パイプライン仕様 v1

> **画面 / 部位:** 現行の Embark Builder（Genotype / Subtype / Mutations / Attributes / Gamemode などのモジュール群）  
> **出力:** Unity (`EmbarkBuilderOverlayWindow` + 各種 `Qud*ModuleWindow`)

## 概要

- キャラクターメイキングは **Embark Builder**（`XRL.CharacterBuilds`）のモジュールを順番に進める構成。各モジュールは `EmbarkBuilderModuleWindowPrefabBase<TModule, TScroller>` を継承し、`FrameworkScroller` を用いた UI プレハブにデータを流し込む。
- 代表的な表示形式:
  - `HorizontalScroller` + `ChoiceWithColorIcon` （Genotype / Gamemode / Starting Location 等）
  - `CategoryMenusScroller` + `CategoryMenuData` + `PrefixMenuOption`（Mutations / Cybernetics など複数カテゴリ＋詳細パネルのモジュール）
  - 専用 UI（Attributes スライダー、Build Summary リスト 等）
- 文字列ソースは `GenotypeEntry`, `SubtypeEntry`, `MutationEntry`, `ChoiceWithColorIcon.Description`, `PrefixMenuOption.Description` など、**ゲームデータ側の `*Entry` クラス**が持つ `DisplayName` や `GetFlatChargenInfo` の結果。
- すべてのテキストは最終的に `UITextSkin.SetText` を経由し、`ToRTFCached` ⇒ TMP RichText へ変換される。

## 主なクラス / メソッド

| レイヤー | クラス / ファイル | 役割 |
| --- | --- | --- |
| オーバーレイ | `EmbarkBuilderOverlayWindow` (`XRL.CharacterBuilds.UI/EmbarkBuilderOverlayWindow.cs`) | Back/Next ボタン、パンくず (`breadcrumbs`)、メニューバー (`HorizontalMenuScroller`)、Legend を描画。 |
| ビルダー | `EmbarkBuilder` (`XRL.CharacterBuilds/EmbarkBuilder.cs`) | モジュールの進行管理、`builder.advance/back()`、`handleUIEvent` でモジュール間のデータ連携。 |
| モジュール共通 | `EmbarkBuilderModuleWindowPrefabBase<TModule, TScroller>` | Unity プレハブ (`HorizontalScroller`, `CategoryMenusScroller` 等) に `FrameworkDataElement` を渡す。 |
| 選択肢 UI | `ChoiceWithColorIcon` (`XRL.UI.Framework/ChoiceWithColorIcon.cs`) | `Title`, `Description`, `IconPath`, `Chosen` predicate。`HorizontalScroller` 内で使用。 |
| カテゴリ UI | `CategoryMenuData` / `PrefixMenuOption` (`XRL.UI.Framework`) | Mutations/Cybernetics など階層メニューの行。`Prefix`（`[ ]` など）＋ `Description`（行ラベル）＋ `LongDescription`（右側詳細）。 |
| モジュール例 | `QudGenotypeModuleWindow.cs`, `QudSubtypeModuleWindow.cs`, `QudMutationsModuleWindow.cs`, `QudAttributesModuleWindow.cs` | 各モジュール固有の `GetSelections/UpdateControls` を実装し、`GenotypeEntry` / `SubtypeEntry` / `MutationEntry` などから表示テキストを組み立て。 |
| データソース | `GenotypeEntry`, `SubtypeEntry`, `MutationEntry`, `MutationCategory`, `QudMutationsModuleDataRow` など | `DisplayName`, `GetFlatChargenInfo`, `ExtraInfo`, `Description`, `GetDescription()` などの原文を保持。 |

## データフロー（例: Genotype → Subtype → Mutations）

1. ユーザーが該当モジュール画面に入ると、`EmbarkBuilder` が `EmbarkBuilderModuleWindowDescriptor` を生成し、ウィンドウ (`QudGenotypeModuleWindow.BeforeShow`) へ渡す。
2. モジュールは `GetSelections()` で `ChoiceWithColorIcon` の列挙を返す。各項目の `Title`/`Description` には `GenotypeEntry.DisplayName` と `GetFlatChargenInfo()`（`{{c|ﾃｹ}}` 付き箇条書き）を使用。
3. `HorizontalScroller.BeforeShow` がデータをプレハブに流し、選択時は `descriptionText.SetText(selection.Description)` で詳細を更新。
4. `EmbarkBuilderOverlayWindow.UpdateBreadcrumbs` が選択済み項目を `UIBreadcrumb.Title` に表示。ここでも `UITextSkin.SetText` が呼ばれる。
5. Mutations モジュールでは `CategoryMenusScroller` がカテゴリ (`CategoryMenuData.Title`) と各 `PrefixMenuOption` をレンダリング。`PrefixMenuOption.Prefix`（`[ ]` / `[笆]` 等）や `Description`（突然変異名）、`LongDescription`（`MutationEntry` 説明）を `UITextSkin` に差し込む。
6. Attributes モジュールでは `AttributeSelectionControl` が `AttributeDataElement.DisplayName` や補足テキストを `UITextSkin` で描画。
7. 全モジュール共通で `builder.advance()` を呼び出すと、`EmbarkBuilderOverlayWindow` の Next/Back ボタン表記や Legend (`legendBar`) が更新される。

## 整形規則

- `ChoiceWithColorIcon.Description` / `CategoryMenuData.menuOptions[].Description` などの文字列は **Markup (`{{color|}}`)** を含んだまま `UITextSkin` に渡す → `ToRTFCached` で TMP RichText へ変換される。
- 箇条書きは `GenotypeEntry.GetChargenInfo` などで `{{c|ﾃｹ}}` をプレフィックスに付与。翻訳で記号を変更する場合は `GetChargenInfo` をフックして整合性を取る。
- Mutations メニューは `PrefixMenuOption.Prefix` に `[ ]` / `[笆]` をセットし、`PrefixMenuOption.Description` に `MutationEntry.DisplayName` をセット。`LongDescription` に `MutationEntry` or `BaseMutation.GetDescription()` の長文を配置し、右ペイン `selectedDescriptionText` が `SetText` する。
- `UITextSkin` の `blockWrap` 既定値は 72（`useBlockWrap=true`）。モジュールによっては `descriptionText` の `preferredHeight` を `CalculateTallestDescription` で再計算するため、極端に長い説明はスクロールよりも高さ調整で対応。
- Icon 表示（`ChoiceWithColorIcon.IconPath`, `UIBreadcrumb.IconPath`）は `Renderable` の Tile 名を使用。翻訳には関係しないが、`IconDetailColor` など ColorMap 文字が `ConsoleLib` のカラー辞書に依存する。

## 同期性

- Embark Builder UI は Unity (`NavigationController`) 上で動き、**すべて uiQueue** に近い文脈で実行される。とはいえ `ChoiceWithColorIcon` などのデータ作成はゲームスレッドで行われるため、翻訳フックは `GetSelections` や `GetChargenInfo` などデータ構築時に挟むのが安全。
- `builder.handleUIEvent` はモジュール間データ共有のフックポイント（例: Mutations モジュールが `QudAttributesModuleWindow` に基礎 MP を問い合わせる）。テキスト整形には直接関与しない。

## 置換安全点（推奨フック）

- `GenotypeEntry.DisplayName`, `GenotypeEntry.GetChargenInfo`, `SubtypeEntry.GetFlatChargenInfo`, `MutationEntry.DisplayName`, `MutationEntry.GetDescription`  
  - ContextID 例: `XRL.GenotypeEntry.GetChargenInfo.Line`, `XRL.SubtypeEntry.GetFlatChargenInfo.Line`.
  - これらを翻訳すると、Genotype/Subtype/Mutations の選択肢・詳細・パンくずに一括反映。
- `ChoiceWithColorIcon.Title` / `.Description` を生成する各モジュール (`QudGenotypeModuleWindow.GetSelections`, `QudSubtypeModule.GetSelections` など)  
  - ContextID: `XRL.CharacterBuilds.Qud.QudGenotypeModuleWindow.Selection`, etc.  
  - UI 固有の追加テキスト（例: `"Choose Genotype"`, `"Mutations Available"`）を差し替えたい場合はここで翻訳。
- `PrefixMenuOption.Prefix/Description/LongDescription` を設定する箇所 (`QudMutationsModuleWindow.UpdateControls`, `QudCyberneticsModuleWindow.UpdateControls`)  
  - ContextID: `XRL.CharacterBuilds.Qud.UI.QudMutationsModuleWindow.NodeDescription`.
- `EmbarkBuilderOverlayWindow.UpdateMenuBars` / `EmbarkBuilderOverlayWindow.BackMenuOption` / `NextMenuOption`  
  - Back/Next ボタンや Legend 文字列 (`MenuOption.Description`) の翻訳に利用。

## リスク

- `ChoiceWithColorIcon.Title` や `Breadcrumb.Title` は HUD のボタン幅に依存するため、長文化するとタイトルが自動的に隠される（`UpdateBreadcrumbs` 内で TitleText の高さを確認し、隠す場合がある）。訳語は短く保つか、UI レイアウトを再調整する必要がある。
- `CategoryMenusScroller` は `PrefixMenuOption.Description` を `UITextSkin` 一行に描画。長すぎる場合は `safeArea` によって折り返しがずれ、Prefix (`[ ]`) とテキストの間隔が崩れる。
- `MutationEntry` 説明は `BaseMutation.GetDescription()` のリッチテキストをそのまま流す。翻訳で追加の `{{` `}}` を入れる際は `MutationEntry` 共有部分（Tooltips, Hints）にも影響する点に注意。
- モジュール間イベント (`handleUIEvent`) はストリング ID で紐づいているため、ここを翻訳・変更すると機能が壊れる。あくまで表示テキストのみを対象にする。

## テスト手順

1. `New Game` → `Build Character` を選択し、**全モジュールを順に進める**。各モジュールで以下を確認:
   - Genotype / Subtype: 選択肢のタイトル・説明・アイコン、パンくず、Back/Next ボタン。
   - Mutations: カテゴリヘッダ、各突然変異の行、詳細ペイン（Variant, LongDescription）、ポイント消費表示。
   - Attributes: スライダやボーナステキスト、残ポイント表示。
   - Build Summary / Library: 翻訳済みの概要が正しく表示されるか。
2. マウス・キーボード・ゲームパッドでのナビゲーションを試し、ボタン説明 (`MenuOption.Description`) や Legend 文字列が崩れていないか確認。
3. 特殊ケース（True Kin / Mutant / 事前作成ビルド / Cybernetics）も開き、`CategoryMenusScroller` が複数カテゴリを描画する際の幅や改行をチェック。
4. `Translator/JpLog` に `ContextID` を追加し、Genotype / Subtype / Mutation / Attribute の各テキストがヒットしているか監視。未翻訳のエントリがある場合は `*.xml` や `*.json` のソースを特定して辞書追加。
