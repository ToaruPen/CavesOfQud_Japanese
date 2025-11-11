# Factions / Reputation パイプライン仕様 v1

> **画面 / 部位:** Reputation / Factions 画面  
> **出力:** Console（`FactionsScreen`） / Unity（`FactionsStatusScreen`）

## 概要

- **クラシック UI**: `XRL.UI.FactionsScreen` が `Factions.Loop()` で参照可能な派閥を集計し、`TextBlock` で名前と説明を折り返して `ScreenBuffer` に描画、右側には `Faction.GetRepPageDescription` を表示。
- **Modern UI**: `Qud.UI.FactionsStatusScreen` が `FactionsLineData` のリストを生成し、`FactionsLine`（`UITextSkin` ベース）で派閥名、 Reputation 数値、詳細テキスト（Feeling / Rank / Pet / Holy Place / Secret）を表示。検索・ソート・折り畳みは Unity 側で処理。

## 主なクラス / メソッド

| フェーズ | Console | Unity |
| --- | --- | --- |
| 生成 | `FactionsScreen.Show` → `Factions.Loop()` → `factionsByName` (Visible only) | `FactionsStatusScreen.UpdateViewFromData` → `FactionsScreen.getFactionsByName()` → `FactionsLineData.set` |
| 整形 (一覧) | `WriteFaction`: `TextBlock(faction.DisplayName, width=30)` + `FormatFactionReputation` (`{{color|value}}`) | `FactionsLine.setData`: `barText.SetText(label)`, `barReputationText.SetText("Reputation: "+FormatFactionReputation)`, `detailsText*` via `UITextSkin` (`ToRTFCached`) |
| 説明 | `TextBlock(Faction.GetRepPageDescription(...), 28, 9999)` で右カラム | `FactionsLine` 展開部で `Faction.GetFeelingText()`, `GetRankText()`, `GetPetText()`, `GetHolyPlaceText()`, `Faction.GetPreferredSecretDescription()` |
| ソート/検索 | `FactionNameComparer` (alphabetical) | `SortMode` (by rep asc/desc or name), `FuzzySharp.Process.ExtractTop` for search |
| 描画 | `ScreenBuffer.SingleBox`, `ScreenBuffer.Write` per row, manual scroll / cursor | `FrameworkScroller.BeforeShow(sortedData)`, `Inventory-style navigation contexts, TMP RichText` |

## データフロー

### Console (`FactionsScreen`)
1. `factionsByName = Factions.Loop().Where(Visible).Select(Name).Sort(FactionNameComparer)`.
2. `cursorRow`／`topRow` に応じて `WriteFaction(scrapBuffer, GO, name, selectedState, x=3, y=row)` を呼び、派閥名（30 幅）＋ `FormatFactionReputation`（`Reputation.GetColor`）を出力。
3. 右側（x≈50）には `TextBlock(Faction.GetRepPageDescription(currentFaction), 28, ...)` を描画。スクロールバーは `ScreenBuffer.Fill`。
4. `GetHeaderMessage()` が `WakingDream` などの状態によって上部メッセージを差し込む。

### Modern (`FactionsStatusScreen`)
1. `rawData = PooledFactionsLineData.set(faction.Name, ColorUtility.CapitalizeExceptFormatting(faction.GetFormattedName()), faction.Emblem, expanded=!Collapsed)`.
2. `searchText` lazily 生成: `Faction.Name / Feeling / Rank / Pet / HolyPlace / PreferredSecret` を結合して lower-case。
3. `SortMode` に応じ `sortedData` を rep 降順 / 昇順 / アルファベットで並べ替え。
4. `controller.BeforeShow(sortedData)` がスクロール UI に行データを供給。
5. `FactionsLine.setData` 内で:
   - `expanderText` `[+]` or `[-]`
   - `barText` = `label`（`Faction.GetFormattedName`）
   - `barReputationText` = `"Reputation: "+ FormatFactionReputation`
   - `detailsText` = `Faction.GetFeelingText()`
   - `detailsText2` = Rank / Pet / Holy Place sentence
   - `detailsText3` = `Faction.GetPreferredSecretDescription`
   - `icon.FromRenderable(faction.Emblem)`

## 整形規則

- Console:
  - 派閥名は `TextBlock(..., width=30, height=5)` で折り返し。訳文が長いと 5 行を超えてスクロール範囲を圧迫するので注意。
  - Reputation 数値は `{$"{num,5}"}`（5 桁右寄せ）を `{{color|}}` で包む。翻訳では桁指定や数値フォーマットを崩さない。
  - 説明は `TextBlock(..., width=28)` で右カラムに収める。`Faction.GetRepPageDescription` は改行入り英語文のため、翻訳時に改行位置を調整する。
- Unity:
  - すべて `UITextSkin` → `ToRTFCached(blockWrap=72)` → TMP RichText。`detailsText3.blockWrap` は画面サイズで 40/60 に切り替わるため、長文でも自動折り返しされる。
  - `barReputationText` などの固定ラベルは `"Reputation: "` を含む英語文。翻訳時は `SetText` に渡す文字列ごと差し替える。
  - `expanderText` には `{{C|+}}` / `{{C|-}}` の Markup を使用。記号は UI ロジックが参照するため変更不可。

## 同期性

- `FactionsScreen.Show` はゲームスレッドで `Keyboard.getvk` を直接処理し、`ScreenBuffer` へ描画。
- `FactionsStatusScreen.UpdateViewFromData` もゲームスレッドでリストを整形し、`FrameworkScroller` が Unity UI と同期。`UITextSkin.Apply` が UI スレッドで実行されるが、テキスト差し替えはゲームスレッドからで OK。

## 置換安全点（推奨フック）

- `Faction.GetFormattedName`, `Faction.DisplayName`, `Faction.GetFeelingText`, `Faction.GetRankText`, `Faction.GetPetText`, `Faction.GetHolyPlaceText`, `Faction.GetPreferredSecretDescription`, `Faction.GetRepPageDescription`  
  - ContextID 例: `XRL.World.Faction.GetRepPageDescription.Body`, `XRL.World.Faction.GetFeelingText.Line`.  
  - これらの戻り値を翻訳すれば Console / Unity / 他 UI（会話等）にも反映される。
- `XRL.UI.FactionsScreen.GetHeaderMessage`  
  - 状態メッセージを翻訳しておくと上部表示に反映。
- `Qud.UI.FactionsLine.setData`  
  - `barReputationText`, `detailsText`, `detailsText2`, `detailsText3` の `SetText` 呼び出しを Harmony で差し替えれば、UI 固有の語順変更が可能。
- `FactionsStatusScreen.HandleCmdOptions` / `MenuOption` テキスト  
  - ソートメニューの `"Highest reputation"` 等を翻訳する場合はここにフック。

## 例文 / トークン

- `FormatFactionReputation`: `"{{G|  350}}"`, `"{{r| -750}}"`  
- `Faction.GetFeelingText`: `"{{G|Friendly}} (You are honored by the Wardens.)"`  
- `Faction.GetRankText`: `"{{Y|Rank 3: Guardian}}"`  
- `FactionsScreen` header: `"{{K|You have no knowledge of the sultans.}}"`（別タブ参照）  
- `FactionsLine` 展開文: `"{{y|They welcome your presence.}} {{K|Their pets are snapjaws.}} {{C|Holy place: Bethesda Susa.}}"`  

## リスク

- `TextBlock` 幅超過: Console で派閥名や説明を長文化するとレイアウト崩れ / スクロール負担が増大。
- `FormatFactionReputation` の `string.Format("{0,5}")` を翻訳で壊すと桁揃えが崩れ、Console/Unity 双方で桁がズレる。
- `FactionsLine` の `detailsSection.SetActive(expanded)` は `expanderText` と同期しているため、翻訳で `[+]`/`[-]` を消すとユーザーが状態を判別しづらくなる。
- `FuzzySharp` 検索は `searchText` を lowercase ASCII で比較するため、日本語訳を search 対象にしたい場合は `searchText` を翻訳後の文字列で再生成する必要がある（要追加設計）。

## テスト手順

1. Console (`Options.ModernUI=false`) で `Reputation` タブを開き、上下スクロール・説明欄スクロール (`[` `]` など) を操作し、派閥名／説明／Reputation 数字の翻訳と整形を確認。
2. Modern UI (`StatusScreens` → `Reputation`) でカテゴリ展開／折り畳み、Sort メニュー、検索バーを操作し、`detailsText*` が崩れずに表示されるか確認。
3. Reputation 値を上下させる（デバッグコマンド等）か異なる派閥を可視化して `FormatFactionReputation` の色変化をチェック。
4. `Translator/JpLog` に `ContextID`（Feeling/Ranks/Secrets など）を追加し、ヒット率を監視して未翻訳の派閥テキストを洗い出す。
