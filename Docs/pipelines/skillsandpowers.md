# Skills & Powers Status Screen パイプライン v1

> **対象 UI / 画面** `StatusScreens` キャンバスの Skills タブ（スキルツリー + Power 詳細ビュー）  
> **参照 ILSpy** `Qud.UI.SkillsAndPowersStatusScreen`, `Qud.UI.SkillsAndPowersLine`, `Qud.UI.SkillsAndPowersLineData`, `XRL.UI.SPNode`, `XRL.UI.SkillsAndPowersScreen`

## 概要
- `SkillsAndPowersScreen.BuildNodes(GameObject)` が `SkillEntry` / `PowerEntry` から `SPNode` リストを構築し、`Skill → Power` の二階層ツリーを作る。各 `SPNode` は `Name`, `Description`, `UIIcon`, `requirements`, `SearchText` をキャッシュする。
- Unity 側の `SkillsAndPowersStatusScreen.ShowScreen` は `StatusScreensScreen` から渡されたプレイヤー `GO` を保存し、`FrameworkScroller` + `ButtonBar` を初期化して `SkillsAndPowersLine` プレハブへ行データを流し込む。
- 左ペイン（リスト）は `SkillsAndPowersLine.setData` が `UITextSkin` 経由で行テキストを整形する。Skill 行は `[+]/[-]` 展開フラグと「Starting Cost」文字列、Power 行は `SPNode.ModernUIText` が返すレンダリング済みテキストをそのまま表示する。
- 右ペイン（詳細パネル）は `SkillsAndPowersStatusScreen.UpdateDetailsFromNode` が呼ばれ、`detailsText`, `skillNameText`, `learnedText`, `requirementsText`, `requiredSkillsText`, `nameBlockText`, `statBlockText`, `spText` などの `UITextSkin` にマークアップ付き文字列を流し込む。
- 検索は `BaseStatusScreen.filterText` を利用しており、`FuzzySharp.Process.ExtractTop` で `SPNode.SearchText` と照合する。翻訳後も検索が機能するように `SPNode.SearchText` をローカライズ済み文字列と同期させる必要がある。

## 主な処理 / メソッド

| コンポーネント | メソッド | 役割 / メモ |
| --- | --- | --- |
| データ構築 (`XRL.UI`) | `SkillsAndPowersScreen.BuildNodes`, `SPNode` | スキルとパワーを走査し、展開状態や `SearchText` を保持する `SPNode` を生成。`SPNode.Description` は `SkillEntry.GetFormattedDescription` / `PowerEntry.GetFormattedDescription` をキャッシュする。 |
| Unity 画面 (`Qud.UI`) | `SkillsAndPowersStatusScreen.ShowScreen` | `nameBlockText = Grammar.MakePossessive(GO.DisplayName) + " Skills"`、`statBlockText` は 6 ステータスを `{{g|}}` で包んだ行にフォーマット。`spText` で `Skill Points (SP)` を表示。 |
| 詳細パネル | `SkillsAndPowersStatusScreen.UpdateDetailsFromNode` | `SPNode.IsLearned(GO)` に応じて `[Unlearned]/[Learned]` の色を変え、`requirementsText` で SP コストと `PowerEntry.requirements` を `:: {{C|cost}} SP ::` 形式で並べる。`requiredSkillsText` は `Power.Requires`/`Exclusion` を展開し、習得済みなら `{{G|[Skill]}}`、未習得なら `{{R|[...]}}` で表示。 |
| リスト行 | `SkillsAndPowersLine.setData` | Skill 行: `skillExpander`, `skillText`, `skillRightText`、`skillIcon` を設定。Power 行: `powerText.SetText(entry.ModernUIText(go))`。どちらも `UITextSkin.SetText`（TMP）までに翻訳を適用する必要がある。 |
| 入力 / 検索 | `SkillsAndPowersStatusScreen.UpdateViewFromData` | `lineData` を `PooledFrameworkDataElement` から再利用しつつ `controller.BeforeShow` に渡す。`filterText` が空でなければ `FuzzySharp.Process.ExtractTop` で 50 ヒットを取り出し表示。 |

## 処理フロー
1. `StatusScreensScreen` でタブが開かれると `SkillsAndPowersStatusScreen.ShowScreen(GO, parent)` が呼ばれ、`playerIcon.FromRenderable(GO.RenderForUI())` まで初期化して `UpdateData()` → `UpdateViewFromData()` → `UpdateDetailsFromNode(firstEntry)` を実行。
2. `UpdateData()` は `SkillsAndPowersScreen.BuildNodes(StatusScreensScreen.GO)` を再実行し、現在の SP を `spText` に反映。
3. `UpdateViewFromData()` が `SkillsAndPowersScreen.Nodes` からルート／展開済みノードのみを `lineData` に入れ、検索中は `Process.ExtractTop` 結果でフィルターした行だけ `FrameworkScroller` に流し込む。
4. `FrameworkScroller.BeforeShow(lineData)` 後、各行で `SkillsAndPowersLine.setData` が呼ばれ、`UITextSkin.SetText(ToRTFCached)` までに計算済み文字列を渡す。ハイライト・選択は `HandleHighlightObject` / `HandleSelectItem` で詳細パネルを更新。
5. `SkillsAndPowersLine.Accept` が押されると `SkillsAndPowersScreen.SelectNode` → `APIDispatch.RunAndWaitAsync` で実際にスキル購入が走り、戻りで `UpdateData()` → `UpdateViewFromData()` が再実行される。

## フォーマット / UI ノート
- 右ペインのステータス列 (`statBlockText`) は `{{K|{{g|STR:}}}}` のように入れ子マークアップを多用するため、`SetText` 前で翻訳・整形を完了させる必要がある。
- `requirementsText` は `StringBuilder` で `:: {{C|SP}} ::` を作り、Power で追加条件があると `requirement.Render(GO, sb)` が生テキストを差し込む。ここで翻訳されない語句（例: Stat 名、装備カテゴリ）が含まれる場合は `PowerEntry` 側を翻訳しておくと他 UI でも恩恵を受けられる。
- `requiredSkillsText` と `SPNode.ModernUIText` は `SkillFactory` / `MutationFactory` から `Entry.Name` を取得している。名称を翻訳する場合は `SkillEntry.Name` / `PowerEntry.Name` を直接ローカライズするのが安全（他のスキル UI でも同じ値を使う）。
- `SPNode.SearchText` は `Skill/Power` の Name + Description を結合して lower-case 化している。翻訳後に検索ヒットさせるには `SearchText` 生成タイミングで訳語を反映する（または `SkillsAndPowersStatusScreen.UpdateViewFromData` で `entry.SearchText` を差し替える）。
- `SkillsAndPowersLine.powerText` には `SPNode.ModernUIText(GO)` の結果（`{{G|:Power}}` など）がそのまま入るため、`SPNode.ModernUIText` を翻訳フックすれば AbilityBar や将来の AbilityManager HUD でも一貫した表示になる。

## 翻訳フック候補
- `Qud.UI.SkillsAndPowersStatusScreen.UpdateDetailsFromNode`  
  - ContextID 想定: `Qud.UI.SkillsAndPowersStatusScreen.DetailsText`, `.SkillName`, `.LearnedText`, `.RequirementsText`, `.RequiredSkillsText`, `.NameBlockText`, `.StatBlockText`, `.SPText`.  
  - `UITextSkin.SetText` に渡す直前で翻訳すれば、詳細パネル全体をコントロールできる。
- `Qud.UI.SkillsAndPowersLine.setData`  
  - ContextID: `Qud.UI.SkillsAndPowersLine.SkillText`, `.SkillRightText`, `.PowerText`.  
  - Skill 行の `Starting Cost [xx sp]` や Power 行の requirement ラベルをここで差し替える。`SPNode.ModernUIText` にグローバルフックを入れる案も併記。
- `XRL.UI.SPNode.ModernUIText` / `SPNode.Description` / `SkillEntry.GetFormattedDescription`  
  - ルートデータ側で翻訳しておくと、Classic `SkillsAndPowersScreen` や AbilityBar など他 UI にも波及する。  
  - 併せて `SPNode.SearchText` をローカライズ文字列で再計算する。
- `SkillsAndPowersStatusScreen.UpdateData` (`spText`, `nameBlockText`, `statBlockText`)  
  - `Grammar.MakePossessive(GO.DisplayName)` 由来の “Player's Skills” など所有格文字列を差し替える場合はここでフックする。

## 注意点 / バッドノウハウ
- `SkillsAndPowersLine` は `NavigationContext` を 1 行ずつ保持しており、`Context.commandHandlers["Accept"] = Accept` が `APIDispatch.RunAndWaitAsync` で非同期購入を呼ぶ。ここで UI スレッドをブロックしないように、翻訳フックも極力軽量にする。
- `SkillsAndPowersStatusScreen.HandleVPositive/Negative` が全スキルの Expand をトグルする。表示対象が急に増減するため、翻訳キャッシュを ContextID 単位で持つ際はノード ID ではなく文字列キーでキャッシュする方が安全。
- `requiredSkillsText` で `{{K|[none]}}` がハードコードされている。翻訳時に角括弧を別の文字へ差し替えると `SetText` 後の幅計算に影響するので注意。

## 確認ポイント
1. 翻訳後も検索（`/` 入力）がヒットするか。`filterText` を日本語で入力すると `FuzzySharp` が低スコアになりやすいので、`SearchText` のローマ字併記なども検討。
2. `[Learned] / [Unlearned]`、`Starting Cost`、`Skill Points (SP)` などの固定フレーズがマークアップ崩れせず表示されるか。
3. `requirementsText` で複数 requirement を持つパワー（例: Acrobatics → Tumbling) を開き、`::` セパレーターと改行が期待通りにレンダリングされるか。
4. `requiredSkillsText` が `Requires` + `Exclusion` を混在させたケース（例: `Phasic` 系）でも色替えと改行が崩れないか。
5. スキル購入後に `UpdateData` → `UpdateViewFromData` が再実行され、翻訳済み文字列がキャッシュに残って差し替わらない問題がないか（`SPNode` の `_Description` / `_SearchText` はキャッシュされるため、翻訳フック側で無効化・再計算が必要）。
