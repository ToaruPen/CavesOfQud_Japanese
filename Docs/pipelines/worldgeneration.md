# World Generation / Creation Progress パイプライン v1

> **対象 UI**
> - Modern World Generation (`Qud.UI.WorldGenerationScreen`)
> - コンソール進捗 (`XRL.UI.WorldCreationProgress`)
>
> **主要ソース** `Qud.UI.WorldGenerationScreen`, `XRL.UI.WorldCreationProgress`, `XRL.CharacterBuilds.Qud.QudGameBootModule`, `XRL.World.WorldFactory`, `XRL.World.WorldBuilders.JoppaWorldBuilder`

---

> **Contract (2025.11)** Translate at WorldCreationProgress / Popup entry points and deliver final text to TMP without extra hooks.
## 1. 全体像
- キャラクター作成完了後、`QudGameBootModule` が `WorldCreationProgress.Begin(totalSteps)` を呼びつつ、モダン UI 用に `WorldGenerationScreen.ShowWorldGenerationScreen(209)`（例）を await する。
- 進捗メッセージは **常に `WorldCreationProgress.NextStep/StepProgress` を経由**しており、その中で `WorldGenerationScreen.AddMessage` / `WorldGenerationScreen.IncrementProgress` も呼ばれる。よって翻訳は `WorldCreationProgress` 側へ集中させるのが最小。
- 名言（Quote）は `BookUI.Books["Quotes"]` から取得。コンソールとモダン UI で別乱数 (`Stat.Random` vs `Stat.RandomCosmetic`) を使うので、翻訳は Book XML 側で統一する。

| フェーズ | Console (`WorldCreationProgress`) | Modern (`WorldGenerationScreen`) |
| --- | --- | --- |
| 初期化 | `Begin(totalSteps)` → `Page = random quote`, `Steps = []` | `_ShowWorldGenerationScreen(totalSteps)` で UI リセット、quote を `quoteText` / `attributionText` へセット |
| ステップ開始 | `NextStep("Generating topography...", stepCount)` が `Steps.Add(StepEntry)` + `ScreenBuffer` 描画 + `AddMessage(Text)` | `_AddMessage` で `progressTexts[0..4]` を更新（直前と同じ文字列ならスキップ） |
| ステップ進捗 | `StepProgress("Generating rivers...")` が `CurrentStep.StepText` を更新し `ScreenBuffer` に進捗バーを描画、`AddMessage(StepText)` も呼ぶ | `_IncrementProgress` が `ProgressBasis`（`{{Y| . . .}}`）を更新し、アイコンのアルファ値を上げる |
| 完了 | `StepProgress(..., Last=true)` で最終バー 100% → `HideWorldGenerationScreen()` を呼んで close | `_HideWorldGenerationScreen` が `await The.UiContext; Hide()` |

---

## 2. コンソール版（WorldCreationProgress）

### データフロー
1. `Begin(totalSteps)` : ランダム引用 (`BookUI.Books["Quotes"]`), `Steps.Clear()`, `CurrentStep = -1`.
2. `NextStep(text, maxSteps)` :
   - `WorldGenerationScreen.AddMessage(text)` （モダン UI へ転送）
   - 直前ステップを完了扱いに更新
   - 新しい `StepEntry { Text=text, MaxSteps=maxSteps, CurrentStep=0 }` を `Steps` に追加
   - `Draw()` で 80x25 へ `[ Creating World ]`, 各ステップ + 進捗バー (`DrawProgressBar`) を描画
3. `StepProgress(stepText, Last=false)` :
   - `WorldGenerationScreen.IncrementProgress()`（モダン UI）
   - `WorldGenerationScreen.AddMessage(stepText)`
   - `Steps[CurrentStep].StepText = stepText`, `CurrentStep` のバーを 1 tick 進める
   - `Draw(Last)` で最下段の大バーを更新

### 文字列ソース
- `QudGameBootModule` : `"Initializing protocols..."`, `"Planting world seeds..."`, `"Starting game!"` など。
- `WorldFactory` / `JoppaWorldBuilder` : `"Generating rivers..."`, `"Building forts..."`, `"Generating village history..."` 等をループ中に大量送信。
- これらは **すべてハードコードされた英語の `string`**。翻訳辞書で `WorldGen.Step.<Token>` のように ID 化したい。

### フォーマット
- `ScreenBuffer.Write("&g^y±")` 等で `&` カラーコードを使用。翻訳後も `&` フォーマットを維持。
- 引用文は `&y`（黄色）で下部に描画。`BookUI` で翻訳済みの行がそのまま使われる。

### フック候補
- `WorldCreationProgress.NextStep` / `StepProgress` Prefix で `text` / `stepText` を辞書ルックアップし、`Steps` に入る前に翻訳。ここで `WorldGenerationScreen` へ送られる文字列も同時に置き換わる。
- `WorldCreationProgress.Draw` のヘッダー `[ Creating World ]`, `Complete!`, `Loading...` など定型句も `Translator.Get("WorldGen.Header.CreatingWorld")` へ差し替え。
- `BookUI.Books["Quotes"]` は `Docs/pipelines/journal.md` 側で翻訳する想定だが、念のため `WorldCreationProgress.Begin` で `Page.Lines` にアクセスする前にチェックを入れておく。

### 注意点
- `StepProgress` は `WorldFactory` のネストした `for` ループ内で頻繁に呼ばれるため、翻訳関数はキャッシュするか `Span` ベースで低コスト化する。
- 同じ文字列が短時間に大量に流れてくる（例: `"Generating rivers..."`）。`WorldGenerationScreen._AddMessage` は直前の 1 行だけ重複排除するので、翻訳の際に余計な句読点やスペースを追加するとデバウンスされなくなる。
- `totalSteps` / `MaxSteps` は 200+ に設定される。`WorldCreationProgress.Begin` と `WorldGenerationScreen._Show` の `totalSteps` を一致させないとプログレスバーがズレる。

---

## 3. Modern UI（WorldGenerationScreen）

### 初期化 `_ShowWorldGenerationScreen(totalSteps)`
1. `await The.UiContext;` で Unity スレッドに移動し、`progressTexts[]` / `progressLines` を空白でリセット。
2. `InitIcons()` が `PopulationManager.RollOneFrom("DynamicSemanticTable:EonIconX")` 的な Blueprint を引き、`UIThreeColorProperties` にセット。翻訳対象ではない。
3. ランダム引用: `BookUI.Books["Quotes"][Stat.RandomCosmetic(...)]` から `list` を作成し、最後の行を `attributionText`、それ以外を `quoteText` に流す（両方 `UITextSkin.SetText`）。
4. `progressText.SetText(ProgressBasis)` で `{{Y|}}` ベースのバーを表示し `Show()`。

### 更新
- `_AddMessage(string message)` : `await The.UiContext;` → `progressLines` の末尾が同じ文字列なら return。直近 5 行だけ `progressTexts` に書き込む。
- `_IncrementProgress()` : `myStep++`, `totalProgress = (float)completedChars/(ProgressBasis.Length-18)`、`progressText.SetText(...)` で `■` を進める。`UpdateIcons()` で各 `eonIcons[i].image.color.a` を `totalProgress` に合わせる。
- `HideWorldGenerationScreen()` : `await The.UiContext; Hide();`.

### フック候補
- 文字列自体は `WorldCreationProgress` で翻訳済みにするのが理想。モダン UI 専用の補正が必要な場合（例: `message` に `{{c|}}` マークアップを追加したい等）は `_AddMessage` Prefix で `ref message` を加工する。
- プログレスバー `ProgressBasis` の文言（`{{Y|}}` + `■`）を差し替える場合、`WorldGenerationScreen` だけで完結させられる。
- 引用表示で英語のまま残るケースがあれば `BookUI` → `WorldGenerationScreen._Show` の連携を確認する。

### 注意点
- `WorldCreationScreen.ShowWorldGenerationScreen` は `await SingletonWindowBase<...>.instance?._Show...` を呼ぶ非同期メソッド。Harmony で Prefix を入れる際は `async` メソッド特有の state machine に注意。
- `_AddMessage` / `_IncrementProgress` は `WorldCreationProgress` から頻繁に呼ばれる。翻訳処理をここに入れる場合は `await` のオーバーヘッドが積み重なる点に留意。
- `quoteText.SetText` は `list` の複数行を `StringBuilder` で結合している。行間や引用符を翻訳時に崩さないよう `Book XML` 側で整形する。

### テスト観点
1. 生成ステップ（地形/歴史/派閥など）で表示される英語がすべて翻訳されるか。Console / Modern 双方で同じ訳になるか。
2. 連続する同一行（例: `"Generating rivers..."` が多発）で重複抑制が働き、ログが 5 行に収まるか。
3. 引用と著者（最終行）が翻訳済みのまま位置ズレしないか。
4. `WorldGenerationScreen.HideWorldGenerationScreen()` のタイミングで画面が真っ黒にならないか（`await The.UiContext` による race 条件がないか）。

---

## 4. Hook 設計メモ
1. **Context ID 案**:
   - `WorldGen/StepTitle/GeneratingTopography`
   - `WorldGen/StepProgress/GeneratingRivers`
   - `WorldGen/Header/CreatingWorld`
   - `WorldGen/Header/Complete`
2. `WorldCreationProgress.NextStep` / `StepProgress` で `JpLog.Hit(ContextId, original, translated)` を記録し、翻訳漏れを識別。
3. 既存の `Docs/pipelines/hook_plan.md` へ「WorldGeneration / Embark Overlays」行を追加し、`WorldCreationProgress` を 1st priority、`WorldGenerationScreen` をフォールバックとして列挙。
4. `Options.ModernUI=false`（コンソール UI 使用時）でも文字列が変わらないことを QA する。

## 5. ������ (2025-11-11)
- `QudJP.Patches.WorldGenerationLocalizationPatch`
  - `WorldCreationProgress.NextStep` / `StepProgress` Prefix �� `XRL.UI.WorldCreationProgress.(NextStep|StepProgress).Text` �� `SafeStringTranslator` �ɒʂ��A�Q�[�������O�� Unity ���g�[�X�g�𓯎��ɓ��{��֍����ւ��B
  - `WorldCreationProgress.Draw` �� Transpiler �ŉ��ς��A`[ Creating World ]` �w�b�_�[�� `: &GComplete!` �o�i�[�� `XRL.UI.WorldCreationProgress.Draw.(Header|Complete)` �R���e�L�X�g����擾����悤����B
- �|��R��� `JpLog` �� `ctx=XRL.UI.WorldCreationProgress.*` ���m�F�B��ʌĂяo���̂��ߎ����L���b�V���ŃI�[�o�[�w�b�h�͖����ł��郌�x���iProfile �� 0.05ms �����j�B
- TODO: Modern UI �� `WorldGenerationScreen._AddMessage` �ɓƎ��}�[�N�A�b�v������K�v���o���ꍇ�͕ʓr�t�H�[���o�b�N�p�b�`�������B
