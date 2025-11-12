# World Generation / Creation Progress パイプライン v1

> **対象 UI**: Modern World Generation (`Qud.UI.WorldGenerationScreen`), Console 進捗 (`XRL.UI.WorldCreationProgress`)
> 
> **主要ソース**: `Qud.UI.WorldGenerationScreen`, `XRL.UI.WorldCreationProgress`, `XRL.CharacterBuilds.Qud.QudGameBootModule`, `XRL.World.WorldFactory`, `XRL.World.WorldBuilders.JoppaWorldBuilder`
> 
> **Contract (2025-11)**: `WorldCreationProgress.NextStep/StepProgress` で翻訳を確定し、Modern UI へ転送されるメッセージも同じ文字列を共有する。TMP 側では `UITextSkin.SetText` の呼び出し順序のみ維持し、追加のパッチを挿入しない。

## 全体像
- キャラクター作成完了後、`QudGameBootModule` が `WorldCreationProgress.Begin(totalSteps)` を呼び出しつつ、Modern UI 用に `WorldGenerationScreen.ShowWorldGenerationScreen(totalSteps)` を `await` する。
- 進捗メッセージはすべて `WorldCreationProgress.NextStep`（ステップ開始）または `StepProgress`（途中経過）経由で Modern UI にも複製される。Console/Modern どちらも同じ原文を共有するため、翻訳は `WorldCreationProgress` 側で行うと重複がない。
- 名言／引用は `BookUI.Books["Quotes"]` から読み込まれる。Console は `Stat.Random`、Modern は `Stat.RandomCosmetic` を使用するため、翻訳は Book XML 側で統一済みであることを確認する。

## Console パス（WorldCreationProgress）
1. `Begin(totalSteps)`
   - `Steps.Clear()`、`CurrentStep = -1`。引用を `Page = random quote` としてセットし、`ScreenBuffer` で 80x25 の UI を初期化する。
2. `NextStep(text, maxSteps)`
   - 直前ステップを完了扱いにし、新しい `StepEntry { Text, MaxSteps }` を `Steps` へ追加。
   - `WorldGenerationScreen.AddMessage(text)` を呼び、Modern UI 側にも反映させる。
   - `[ Creating World ]` ヘッダーと進捗バーを `ScreenBuffer` へ描画。
3. `StepProgress(stepText, last)`
   - `WorldGenerationScreen.IncrementProgress()` と `AddMessage(stepText)` を呼ぶ。
   - 現在の `StepEntry` のバーを 1 tick 進め、`Draw(Last)` で画面全体を再描画。

**翻訳 Hook**
- `WorldCreationProgress.NextStep` Prefix: `text` を `SafeStringTranslator.SafeTranslate(text, "WorldGen.StepTitle")` などの Context で置換。
- `WorldCreationProgress.StepProgress` Prefix: `stepText` を同じ辞書で翻訳し、Modern UI 用のメッセージも同じ値にする。
- `WorldCreationProgress.Draw` 内の定型句（`[ Creating World ]`, `Complete!`, `Loading...`）は `SafeStringTranslator.SafeTranslate` で一括置換しておき、Console と Modern の表示を揃える。

## Modern パス（WorldGenerationScreen）
1. `_ShowWorldGenerationScreen(totalSteps)`
   - `await The.UiContext;` の後で `progressTexts`, `progressLines` を初期化し、引用 (`quoteText`, `attributionText`) を `UITextSkin.SetText` で描画。
   - プログレスバーのベース `ProgressBasis = {{Y|..........}}` を設定し `Show()` を呼ぶ。
2. `_AddMessage(message)`
   - 直前 5 行の履歴に入れ替えがなければ `progressTexts[]` を更新。入力文字列はすでに `WorldCreationProgress` で翻訳済みであるため追加処理は不要。
3. `_IncrementProgress()`
   - `totalProgress` を更新し、`progressText.SetText(...)` が `UITextSkin` 経由で TMP へ書き込む。
4. `_HideWorldGenerationScreen()`
   - `await The.UiContext; Hide();` で UI をクローズ。

**注意点**
- `_AddMessage` / `_IncrementProgress` は `WorldCreationProgress` から高頻度で呼ばれるため、ここで翻訳ロジックを実行すると GC コストが跳ね上がる。翻訳は gameQueue 側で完結させる。
- `totalSteps` がずれると Console / Modern のバー進行度が一致しなくなる。Hook を追加する際は `WorldCreationProgress.Begin` と `WorldGenerationScreen.ShowWorldGenerationScreen` の引数が揃っているかログで確認する。
- Book 引用は `Docs/pipelines/journal.md` 側で管理する。WorldGen 側で改めて翻訳しない。

## QA
1. Console/Modern 双方で `WorldGenerationProgress.NextStep` の出力が同じ訳語になっているか `Player.log` と画面で確認する。
2. `Missing glyph` や RichText エラーが出ていないか `Player.log` を tail し、KeyWord（`Missing character`, `Sprite not found`）で検出する。
3. `_AddMessage` の 5 行ヒストリに同一メッセージが連続して送られた場合も重複排除が働くかを確認する（"Generating rivers..." など）。
4. 進捗バー 100% 到達時 (`StepProgress(last:true)`) に Modern UI が確実に `Hide()` されるか、`The.UiContext` の `await` が詰まっていないかを確認する。
