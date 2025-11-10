# Runtime Localization — Layer B/C タスク

レイヤーA（XML/JSON ローダー経由の静的テキスト）だけでは拾えない UI／ログの英語文字列や C# 側にベタ書きされた ldstr を翻訳するためのタスクリスト。仕様書に登場するレイヤーB（表示シンク）とレイヤーC（Transpiler 置換）を段階的に実装し、差分管理しやすい形にまとめる。

## ゴール
- TextMeshPro / uGUI / Popup など最終表示直前で日本語辞書を適用し、データ未翻訳の状態でも英語が残らないようにする。
- ハードコード文字列を declarative に列挙できる設定ファイルを整備し、Harmony Transpiler で ldstr を安全に置換する。
- 辞書ファイルのホットリロード、未翻訳ログ、Mod 設定からの切り替えなど、翻訳作業を支援する周辺機能を揃える。

## 参照しておくべき既存リソース
- Docs/loader_matrix.md … Load / Replace の整理表。Translator から参照するファイル一覧を追記予定。
- Docs/font_pipeline.md … 表示崩れ検証フローとの整合を維持するためのフォント方針。
- Docs/translation_status.md … Layer B/C を進めたら専用の進捗欄を増設して記録する。
- Mods/QudJP/Assemblies/src/Localization/MenuOptionLegendLocalizer.cs … 既存の辞書置換ロジック（最小実装）の参考。
- Mods/QudJP/Assemblies/src/Patches/* … Harmony パッチの命名規則とロギング方針。

## 作業項目
1. **Translator サービスの基盤**
   - Mods/QudJP/Assemblies/src/Localization/Translator.cs（新規）を追加し、JSON 辞書を読み込んで Apply(string original) を提供する。
   - 辞書フォーマットは要件定義 4.1 をベースに meta / rules / entries をサポート。色タグや HTML エンティティを保護する正規化ルールも同ファイルに実装する。
   - Docs/tasks/objectblueprints_llm.md を参考にしつつ、辞書ファイルの配置（例: Mods/QudJP/Localization/Dictionaries/*.json）と命名規則を決めて文書化する。
2. **表示シンク（Layer B）**
   - TextMeshProUGUI.SetText / TMP_Text.SetText(string, …) と XRL.UI.Popup.* を Harmony Prefix/Postfix でフックし、Translator に通してから描画する。
   - MenuOptionLegendLocalizer など既存の個別辞書ロジックは、Translator でヒットしなかった場合のフォールバックとして維持する。
   - 長文の強制改行／自動縮小を Translator.Apply かラッパーメソッド経由で切り替えられるようにし、Mod 設定（新規）で ON/OFF を制御する。
3. **辞書ホットリロード / Mod 設定**
   - FileSystemWatcher を導入し、辞書ファイル更新時に Translator インスタンスを差し替える。
   - Mod 設定画面に「言語選択」「辞書リロード」「長文フォールバック」などのトグルを追加し、Docs/tasks/ui.md に作業ログを残す。
4. **Transpiler（Layer C）**
   - Mods/QudJP/Localization/QudUTF/*.xml（仮）で targetType / targetMethod / find / replace を定義。要件定義 4.2 の例をベースにする。
   - 新しい UtfReplacementLoader ローダークラスで設定を読み込み、対応する Harmony Transpiler を自動生成する。ヒット数や競合は Debug.Log へ警告を出す。
   - 置換に失敗した場合でも Translator を通して少なくとも英語のまま表示できるフォールバックを用意する。
5. **未翻訳ログ収集**
   - Translator で未ヒットだった文字列を Logs/QudJP_Untranslated.log に 1 回だけ記録し、色タグやプレースホルダーを保持する。Mod 設定で ON/OFF とローテーションサイズを設定できるようにする。
   - 収集結果から JSON テンプレートを生成する簡易スクリプト（例: scripts/untranslated_export.py）を追加予定。
6. **ドキュメント更新**
   - Docs/loader_matrix.md に辞書ファイルと Transpiler 設定のロード手順を追記する。
   - Docs/translation_status.md の「Grammar / Population / Harmony」セクションを更新し、Layer B/C の進捗を記載する。
   - 新規辞書フォーマットやホットリロード手順を Docs/translation_process.md に追加する。
   - テスト観点を Docs/test_plan.md へ「辞書ホットリロード」「Popup 翻訳」「未翻訳ログ確認」として加える。

## 成果物チェックリスト
- [ ] Translator / Display シンクの実装とテストが完了している。
- [ ] Transpiler 設定ファイルと適用ロジックが整備されている。
- [ ] Mod 設定 UI（言語切替・リロード・ログ出力）が動作し、Docs/tasks/ui.md に記録がある。
- [ ] 辞書と Transpiler の構成を説明するドキュメント更新が反映されている。
- [ ] 新しいテストケースと QA 手順が Docs/test_plan.md に追加されている。

## 備考
- 実装の流れは「Translator → 表示シンク → ホットリロード → Transpiler → ログ収集 → ドキュメント更新」を推奨。
- 既存の Harmony パッチと競合しないよう、HarmonyPriority の調整と try/catch によるガードを検討する。

## Console bridge

### ScreenBuffer / ConsoleChar quick reference
- Classic UI は 80x25 の ConsoleLib.Console.ScreenBuffer を保有。ScreenBuffer.Width/Height は固定値だが念のため取得値を使ってループする。
- ConsoleChar 1 マスに対して Char (CP437), Tile, Foreground, Background, Detail, TileForeground/Background を持つ。UI 表示は Char + Foreground を読めば十分。Tile が来た場合は疑似絵文字扱いとし、後段 TODO へ記録する。
- ConsoleBridge 側では CP437 → Unicode 変換と <color=#RRGGBB> タグ化のみ実装済み。背景色は <mark> へ変換予定。

| Field | 備考 |
| --- | --- |
| Width / Height | 80 / 25（固定。念のためバッファの値を参照） |
| ConsoleChar.Char | CP437 コードポイント。制御コードは空白扱い。 |
| ConsoleChar.Tile | タイル ID。値が入っていたら Char より優先して描画する。 |
| ConsoleChar.Foreground | UnityEngine.Color。<color=#RRGGBB> に落とし込む。 |
| ConsoleChar.Background | まだ未使用。将来的に <mark> へ変換する。 |
| ConsoleChar.Detail | Console 用のワイヤーフレーム。現状未使用。 |

### CP437 cheat sheet
Encoding.GetEncoding(437) で大半は Unicode にマップできるが、罫線やボックス描画でよく使うコードだけ表にしておく。

| Hex | Glyph | 用途 |
| --- | --- | --- |
| 0xB3 | │ | 縦罫線 |
| 0xC4 | ─ | 横罫線 |
| 0xDA | ┌ | 左上の角 |
| 0xBF | ┐ | 右上の角 |
| 0xC0 | └ | 左下の角 |
| 0xD9 | ┘ | 右下の角 |
| 0xC2 | ┬ | 上向きの丁字 |
| 0xC1 | ┴ | 下向きの丁字 |
| 0xC3 | ├ | 左向きの丁字 |
| 0xB4 | ┤ | 右向きの丁字 |
| 0xC5 | ┼ | 交点 |

### Rendering pipeline (TMP bridge)
1. **Harmony intercept** – ConsoleBridgePatch (Mods/QudJP/Assemblies/src/Patches/ConsoleBridgePatch.cs) で TextConsole.DrawBuffer を Prefix。Classic UI (!GameManager.Instance.ModernUI) のときだけ ConsoleBridge.CaptureFrame を呼ぶ。
2. **Frame capture** – ConsoleBridge (Mods/QudJP/Assemblies/src/Console/ConsoleBridge.cs) が 25 行ぶんの <mspace=0.61> 付き文字列を組み立て、ConsoleChar.Foreground が変わるたびに <color=#RRGGBB> を挿入。StringBuilder と CP437 用 1 byte バッファをキャッシュして GC を抑える。
3. **Unity view** – ConsoleBridgeView が Canvas + TextMeshProUGUI 25 本をプールし、ConsoleBridge.PublishFrame で受け取ったフレームを Update で消費して差分行だけ .text を更新。RectTransform は BaseCellWidth/Height (8x16) を基準に画面解像度へスケールし、Classic UI のセル座標とマウス判定のズレを最小化する。

### TODO / notes
- 背景色 (ConsoleChar.Background) を <mark> タグで再現する。前景だけでは選択肢の反転色が分かりにくい。
- ConsoleChar.Tile が渡ってくるケース（地図アイコンや器具記号など）をどう描画するか決める。現状はテキストとしてエスケープしているため、必要なら <sprite> など別ルートを定義する。
- ConsoleBridge の <mspace> 値 0.61 は Noto Sans CJK の 24px を前提に調整した係数。別フォントを追加したら簡単なグリッドベンチを走らせて係数を Doc に追記する。
- 2025-11-10: 収集ログ確認時は %USERPROFILE%\AppData\LocalLow\Freehold Games\CavesOfQud\Player.log を tail しながら最新のセッションで検証すること。
