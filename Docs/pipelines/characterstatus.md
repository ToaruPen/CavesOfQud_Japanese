# Character Status / Skills パイプライン v1

> **画面 / 部位:** `StatusScreens` 内の「Attributes & Powers」タブ（キャラクター統計 / Mutations / Effects）  
> **出力:** Unity (`CharacterStatusScreen` + 各 `Character*Line`) – `StatusScreensScreen` 経由で表示

## 概要

- `Qud.UI.CharacterStatusScreen` がプレイヤーの `Statistic` / `BaseMutation` / `Effect` を収集し、4 本の `FrameworkScroller`（Primary/Secondary/Resistance Attributes, Mutations, Effects）にデータを流す。
- 各行 (`CharacterAttributeLine`, `CharacterMutationLine`, `CharacterEffectLine`) は `UITextSkin` を使って値・名称・補足を描画。選択すると右側の詳細パネル (`UITextSkin`) が更新される。
- 上部の基本情報（名前・クラス・レベル・重量等）やポイント表示 (`attributePointsText`, `mutationPointsText`) も `CharacterStatusScreen.UpdateViewFromData` 内で `SetText` される。

## 主なクラス / メソッド

| フェーズ | クラス | メソッド / 備考 |
| --- | --- | --- |
| データ収集 | `CharacterStatusScreen.UpdateData` | `mutations`（`StatusScreen.GetMutationList`）、`effects`（`GO.Effects`）、`stats`（`GO.Statistics.Values`）をリスト化。 |
| 表示更新 | `CharacterStatusScreen.UpdateViewFromData` | `FrameworkScroller.BeforeShow(...)` へ `CharacterAttributeLineData` / `CharacterMutationLineData` / `CharacterEffectLineData` を渡し、トップテキスト類（名前・レベル・ポイント）も `UITextSkin.SetText`. |
| 属性行 | `CharacterAttributeLine.setData` | `attributeText.SetText(stats.GetShortDisplayName())`, `valueText.SetText`（色付け + 特殊処理 AV/DV/MS/CP）、`modifierText` で `[+N]` など。 |
| Mutation 行 | `CharacterMutationLine.setData` | `mutation.GetDisplayName()` と `GetUIDisplayLevel()` を `UITextSkin` へ。`[Physical/Mental]` 判別は詳細表示で行う。 |
| Effect 行 | `CharacterEffectLine.setData` | `effect.DisplayName` をそのまま表示。 |
| 詳細パネル | `HandleHighlightAttribute` / `HandleHighlightMutation` / `HandleHighlightEffect` | 選択中の要素に応じて右側 `UITextSkin` / `mutationDetails` に長文説明を流し込む。Mutation では `GetDescription`, `GetLevelText`, `GetIcon`, `GetMutationTermEvent` を使用。 |

## データフロー

1. `StatusScreensScreen` から `CharacterStatusScreen` をアクティブ化 ⇒ `GO = StatusScreensScreen.GO`.
2. `UpdateData()`:
   - `effects = GO.Effects` から説明のある `Effect` のみ収集。
   - `mutations = StatusScreen.GetMutationList(GO)` + `PsychicGlimmer` プロキシ。
   - `stats = GO.Statistics.Values`.
3. `UpdateViewFromData()`:
   - `primary/secondary/resistance Attributes` → `FrameworkScroller.BeforeShow` に `CharacterAttributeLineData`.
   - `mutationsController` → `CharacterMutationLineData`.
   - `effectsController` → `CharacterEffectLineData`.
   - 上部 UI (`nameText`, `classText`, `levelText`, `attributePointsText`, `mutationPointsText`) に `string.Format` で組んだ Markup 付き文字列を `SetText`.
   - `playerIcon.FromRenderable(GO.RenderForUI("StatusScreen,Character"))`.
4. 利用者がラインをハイライトすると `HandleHighlight*` が呼ばれ、対応する `UITextSkin` (`primaryAttributesDetails` 等) に詳細テキストを設定。
5. 操作（Buy Stat / Show Mutation Popup / Buy Mutation）などは `APIDispatch.RunAndWaitAsync` 経由でゲームロジックを呼び、完了後に `UpdateViewFromData()` で再描画。

## 整形規則

- `valueText` や `mutationPointsText` などは `{{color|...}}` の Markup を直接組み立てている。翻訳でフォーマットを変える場合は `string.Format` を差し替えつつ `{`/`}` を維持。
- `CharacterAttributeLine` の `valueText` は特定ステータスに応じて計算が違う（AV/DV/MA/MoveSpeed/CP など）。翻訳時も `Stats.GetCombat*` の呼び出し順序を崩さない。
- `mutationDetails` に表示する文章は `StringBuilder` で `GetDescription()` + `GetLevelText()` を連結し、`{{w|}}` や `\n\n` を挿入。改行ロジックを壊さない。
- `CharacterMutationLine` は `ShouldShowLevel()` に応じて `(RANK X/10)` 表示を付与。`{{y|...}}` で全体をラップ。

## 置換安全点（推奨フック）

- `CharacterStatusScreen.UpdateViewFromData`  
  - ContextID 例: `Qud.UI.CharacterStatusScreen.HeaderLine`, `.AttributePoints`, `.MutationPoints`.  
  - トップ部（名前/クラス/レベル行）やポイント表示をここで翻訳。
- `CharacterAttributeLine.setData`  
  - ContextID: `Qud.UI.CharacterAttributeLine.AttributeText`, `.ValueText`.  
  - `Statistic.GetShortDisplayName()` が英語前提の場合、ここで辞書置換。
- `CharacterMutationLine.setData`  
  - ContextID: `Qud.UI.CharacterMutationLine.ItemText`.  
  - Mutation 名やランク表示を差し替える。
- `CharacterEffectLine.setData`  
  - ContextID: `Qud.UI.CharacterEffectLine.ItemText`.  
  - `Effect.DisplayName` を翻訳。
- 詳細パネル (`HandleHighlightMutation`, `HandleHighlightAttribute`)  
  - 長文説明を翻訳したい場合は Mutation / Statistic 側 (`GetDescription`, `GetHelpText`) を直接翻訳したほうが再利用性が高い。

## リスク

- `FrameworkScroller.BeforeShow` に渡すリストは頻繁に再生成されるため、翻訳で重い処理を挟むと UI がカクつく。`Statistic` / `Mutation` クラス側でキャッシュするほうが良い。
- `CharacterAttributeLine.valueText` は色決定ロジックが複雑（数値によって G/C/R）。翻訳で `{{` `}}` を壊すと色も壊れる。
- `mutationTerm`（`mutations` / `mutation` / `Mutation`）は `GetMutationTermEvent` でゲーム内状況に応じて動的に決まる。ここを翻訳するならイベント結果を差し替える必要がある。
- 小画面 (`Media.sizeClass < Medium`) では詳細パネルの表示箇所が変わるため、長文を入れるとスクロールがないまま切り捨てられる。

## テスト手順

1. `StatusScreensScreen` を開き、「Attributes & Powers」タブで以下を確認:
   - Primary/Secondary/Resistance 各セクションの属性名・値・色付け。
   - Mutation/Efffect リストと右側詳細パネルの内容。
   - 上部の名前/クラス/レベル/重量行、ポイント表示。
2. `Gamepad` / `Keyboard` の切り替えで `MenuOption` 説明の表示が変わるため、翻訳済み文字列が切れないか確認。
3. Mutation 詳細で `This rank / Next rank` 表示や Morphotype 表記 (`[Morphotype]`) が正しく翻訳されているかを確認。
4. 効果 (`Effects` タブ) で `Effect.GetDescription()` の改行と色タグが崩れていないか確認。
