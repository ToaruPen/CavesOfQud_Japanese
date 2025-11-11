# Ability Bar / Ability Manager パイプライン v1

> **対象 UI**  
> - HUD Ability Bar (`AbilityBar`): アクティブ効果、ターゲット情報、ホットバーのボタン群  
> - Ability Manager (`AbilityManagerScreen`): 詳細選択スクリーン＋キーバインド管理  
> **主要ソース** `Qud.UI.AbilityBar`, `Qud.UI.AbilityBarButton`, `Qud.UI.AbilityManagerScreen`, `AbilityManagerLine`, `AbilityManagerLineData`, `XRL.World.ActivatedAbilities`

---

## 1. Ability Bar（HUD）

### 仕組み
- `AbilityBar` は `Stage` 画面の `AfterRender`（`XRLCore.AfterRender += AbilityBar.AfterRender`）でプレイヤー状態を読み、UI スレッドへ渡す文字列を構築する。
- **アクティブ効果:** `AfterRender` で `player.Effects` を走査し `Effect.GetDescription()` を `Markup.Wrap` で包んで `SB` に連結 → `effectText = text?.ToRTFCached()` → `EffectText.SetText`。  
  `EffectsDirty` フラグで最小限の再描画に抑えているので、翻訳は `GetDescription()` 側で行うか `EffectText.SetText` 直前で。
- **ターゲット情報:** `Sidebar.CurrentTarget` があれば `TargetText`／`TargetHealthText` を `{{C|TARGET: ...}}` 形式で生成（DisplayName + Wound/Feeling/Difficulty）。`Strings.WoundLevel` などを経由するため、翻訳は API 側か `AbilityBar` の `targetText` 更新時に行う。
- **ボタン群:** `ActivatedAbilities.GetAbilityListOrderedByPreference()` を `AbilityDescription` に詰め、1 ボタン = `AbilityBarButton` インスタンス。  
  `Tinkering` では `AbilityBar` で `text.SetText` するわけではなく、`AbilityBarButton` が `command` に対応する `Keyboard.PushMouseEvent("Command:CmdAbility...")` を発火。
  ラベル文面は `AbilityManagerLine` と同じく `AbilityEntry.DisplayName` を使うため、翻訳を `ActivatedAbilityEntry.DisplayName` / `Description` で統一すると HUD/Manager 双方が揃う。
- **ページング／ホットキー:** `AbilityCommandText`/`CycleCommandText` は `ControlManager.getCommandInputDescription("Ability Page Up")` などを文字列化し `UITextSkin.Apply` で更新。  
  ここを翻訳するなら `AbilityBar.UpdateAbilitiesText` 内で `SetText` する文面を変える。

### フック候補
- `Effect.GetDescription` or `AbilityBar.AfterRender` (effect string)  
  - 効果説明を唯一のソースにしているので、Effect クラス側の `GetDescription` を翻訳すれば、ログや他 UI でも同じ結果を得られる。
- `Strings.WoundLevel`, `Description.GetFeelingDescription`, `GetDifficultyDescription`  
  - ターゲット情報の構成要素。ここを翻訳すると AbilityBar/Look/Examine でも同じ訳が使われる。
- `ActivatedAbilityEntry.DisplayName` / `.Description`  
  - Ability Bar ボタン、Ability Manager リスト、ログメッセージなどの共通フィールド。  
  - HUD 特有の追加タグ（`[attack]`, `[disabled]`, `{{C|X turn cooldown}}`, `{{Y|<hotkey>}}`）は `AbilityManagerLine.setData` にハードコードされているので、そのメソッドにもフックが必要。
- `AbilityBar.UpdateAbilitiesText` / `AbilityBarButton.UpdateText`  
  - 「ABILITIES」「page X of Y」「Use Ability」説明の翻訳。`AbilityBarButton.Hotkey.text` は `ControlManager` が返す文字列を流し込んでいるため、必要なら `ControlManager` の辞書を利用。

### 注意点
- Ability Bar の `effectText`/`targetText` は UI スレッドとは別ロックで書き換わる。フックで例外が出るとロックが開放されず HUD が更新されなくなるので try/finally 必須。
- `AbilityBarButton` は `TMP` ではなく `TextMeshProUGUI` の `.text` を直接書き換えている。`SetText` は使われないので、翻訳で RichText を使う場合は `TMP` が解釈できるタグを使用する。
- ability list には `abilitiesDirty` → `RefreshAbilityButtons` → `AbilityButtons[i].SetActive` の非同期処理があり、`AbilityDescription.Entry` の再利用（プール）が前提。翻訳ロジックで `Entry` を保持し続けないこと。

### テスト
1. 効果が増減したとき（薬やバフ）に `EffectText` が重複せず、日本語でも `Markup` が破綻しないか。
2. ターゲットが敵/味方/オブジェクトに変わった際に難易度/感覚ラベルが正しく変わるか。
3. Ability ボタンの `[attack]`, `[disabled]`, `[astrally tethered]` ラベルが崩れないか。
4. ゲームパッド／キーボードを切り替えても `Use Ability` ホットキーが正しく再描画されるか。

---

## 2. Ability Manager Screen

### 仕組み
- `AbilityManagerScreen.BeforeShow()` で `AbilityManager.PlayerAbilities`（`ActivatedAbilities`）を走査し、`AbilityManagerLineData` を生成。  
  - カテゴリ行 (`category != null`): `'[-] Mutation'` のように `[+]/[-]` を付与。  
  - Ability 行: `ability.DisplayName`, `[attack]`, `[disabled]`, `[{{C|N}} turn cooldown]`, `[{{g|Toggled on}}]` などを組み合わせて `StringBuilder` で `UITextSkin` に渡す。
- **検索:** `searchText` に Popup (`CmdFilter`) で入力した語を保存し、`FuzzySharp.Process.ExtractTop`（最大 50件）で `AbilityManagerLineData.searchText`（`DisplayName + Description`）と照合。  
  `FILTER_ITEMS.Description`（画面左下のメニュー）に “search: foo” を入れて状況を示す。
- **並び替え:** `SortMode.Custom` (デフォルト) は `ActivatedAbilities.PreferenceOrder` を編集 (`MoveItem`, ドラッグ＆ドロップ, `V Positive/Negative`)。`SortMode.Class` は `Ability.Class` でグループ化し `classCollapsed` が `[+]/[-]` を制御。
- **右ペイン詳細:** 行ハイライト時 (`leftSideScroller.onHighlight`) に `rightSideIcon`, `rightSideHeaderText`, `rightSideDescriptionArea` を更新。  
  - “Type: Mutation/Skill/Item” ラベル（`AppendColored("y","Type: ")`）  
  - `Ability.Description` がそのまま下に表示される。
- **ホットキー管理:** `AbilityManagerLine.HandleRebind/HandleRemoveBind` で `CommandBindingManager` を操作し、他コマンドの衝突確認ダイアログ (`KeybindsScreen`) を挟んだうえで `AbilityBar.markDirty()` を呼ぶ。

### フック候補
- `ActivatedAbilityEntry.DisplayName/Description`（データ側）  
  - ここを翻訳できれば Ability Bar と Ability Manager 双方に適用。
- `Qud.UI.AbilityManagerLine.setData` (`Qud.UI.AbilityManagerLine.Text`)  
  - `[attack]`, `[disabled]`, `[{{C|N}} turn cooldown]`, `[astrally tethered]`, `[{{g|Toggled on}}]`, `<hotkey>` といった UI 固有ラベルを翻訳する最適地点。
- `Qud.UI.AbilityManagerScreen.rightSide*`  
  - `rightSideHeaderText`/`rightSideDescriptionArea` は `SetText` にプレーン文字列を渡している。ここで `Type:` ラベルや `Ability.Description` を差し替える。
- `AbilityManagerScreen.FilterItems` / `AbilityManagerLineData.searchText`  
  - Fuzzy 検索を日本語で成立させるため、`searchText` を翻訳済み文字列で構築し、`ToLower()` ではなく `CultureInfo` に合わせた処理を挿む案を検討。

### 注意点
- `AbilityManagerLine` はドラック＆ドロップ中に `AbilityManagerSpacer` (透明オブジェクト) を点灯させている。フックで再描画を遅らせると DnD 操作がもたつくため、重い処理は避ける。
- `searchText` は `lower()` で比較しているため、全角カナや大文字小文字が混ざるとヒットしないケースがある。翻訳後に `ToLowerInvariant()` を呼ぶ場合は `CultureInfo` に留意。
- ホットキーリバインドは非同期 (`Popup.ShowKeybindAsync`, `CommandBindingManager.GetRebindAsync`) で UI を中断するため、翻訳フックは await 中に例外を出さないようにする。

### テスト
1. カテゴリの `[+]/[-]` トグルが翻訳後も機能し、Collapsed 状態が保存されるか。
2. Fuzzy 検索で日本語キーワードを入力した際に期待通りの能力だけが残るか。
3. Ability をドラッグで並び替えた後も `PreferenceOrder` が保存され、Ability Bar に反映されるか。
4. キーバインド変更ダイアログのメッセージが翻訳された際に制御文字や `{ }` を壊さないか。
