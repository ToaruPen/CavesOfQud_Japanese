# UI ローカライズアーキテクチャ（概要と変更指針）

本ドキュメントは、QudJP mod における UI ローカライズの仕組みと、コード変更時に他 UI を破壊しないための注意点をまとめたものです。`SelectableTextMenuItem` 周辺の修正要望が多く発生しているため、TMP_Text の扱いを含む全体像を共有しておきます。

## 1. 基本コンセプト

### 共通の翻訳エンジン
- すべての文字列は `QudJP.Localization.Translator` を経由して翻訳される。
- `SafeStringTranslator.SafeTranslate(value, contextId)` から `Translator.Instance.Apply` に渡り、辞書スナップショットを参照する。
- 辞書メンテナンスや context key の正規化は Translator が一元管理する（辞書ファイルを増やす場合も Translator 側の仕組みは共通）。

### UI コンポーネントごとのフック
- ゲーム本体には翻訳 API が無いため、Harmony パッチで **どのタイミングで翻訳するか** をコンポーネントごとに差し込んでいる。
  - 例: `SelectableTextMenuItemLocalizationPatch`、`TooltipTextLocalizer`、`PopupTranslationPatch` など。
- これにより UI のライフサイクルに合わせて翻訳済み文字列を設定し、辞書ロジックは共通化しつつも UI ごとの都合に応じた制御が行える。

## 2. TMP_Text とフォント

### FontManager / TMPFontGuard
- `FontManager.ApplyToText(TMP_Text, forceReplace)` で日本語フォント（NotoSansCJKjp-Subset）とマテリアルを適用。
- `TMPFontGuard.ApplyToHierarchy(root)` が PopupMessage や Tooltip などの GameObject 階層に対してフォントを一括適用する。

### UITextSkin 経由の描画
- 多くの UI は `XRL.UI.UITextSkin` を経由して TMP_Text の `.text` をセットする。
- `UITextSkinFontPatch` / `UITextSkinTranslationPatch` / `UITextSkinDebugPatch` などで Apply 処理を拡張し、翻訳やレイアウトの監視を行っている。

### SelectableTextMenuItem の特殊性
- 本体実装（decomp 参照）では `SelectableTextMenuItem.SelectChanged` 内でアイテムテキストを毎フレーム `item.SetText("{{W|...}}")` へ差し替えている。
- 多バイト文字を含む場合に TMP のメッシュ生成が 0 になりやすいため、`SelectableTextMenuItemRenderGuardPatch` で以下を実施:
  - フォント強制適用。
  - RectTransform が 0/0 になった場合の再計算。
  - メッシュが生成されなかった場合、ホットキーを強調したフォールバック文字列で再描画。
  - デバッグ用に `charCount` をログ出力（問題追跡のため追加）。

## 3. 変更時のガイドライン

1. **Translator の共通ロジックは変更せず、UI 側で「いつ翻訳を呼ぶか」を制御する。**  
   例: 新しい UI コンポーネントを翻訳したい場合は、そのクラスの `SetText` 相当のタイミングに Harmony パッチを追加する。

2. **TMP/Text のパッチは対象クラスに閉じ込める。**  
   Tooltip や Inventory など他 UI が正常に描画されている場合は、共通クラス（UITextSkin / TMP_Text）を大きく変更しない。問題のある UI 専用のパッチ（SelectableTextMenuItem など）で対処する。

3. **フォント適用は `FontManager`/`TMPFontGuard` を使い、対象階層に限定する。**  
   PopupMessage でフォントを変えたい場合でも `PopupMessageFontGuardPatch` で `TMPFontGuard.ApplyToHierarchy(__instance)` を呼ぶだけにして、他 UI へ副作用を与えない。

4. **ログを活用する。**  
   - `JpLog`（`tmp/player_log_watch.txt` → `Player.log`）で `[JP][Dict][FILE]`・`[JP][FontGuard]`・`[QudJP] Menu item mesh ...` のような診断を出すようにしている。新しいパッチを入れる場合も同様に局所ログを追加し、問題発生時に素早く切り分けられるようにする。

5. **他 UI を破壊しないことを明示した上で MR / 変更を行う。**  
   変更点の説明には「翻訳ロジックは共通」「ターゲット UI は ○○」「他コンポーネントには手を触れていない」ことを明記する。

## 4. 今後の TODO（選択肢が消える問題の切り分け）

- `SelectableTextMenuItemRenderGuardPatch` のログから、ホットキー付きラベルで `charCount=0` になるケースを重点調査。
- `UIEntryInstrumentationPatch` で取得している `SelectChanged` の順序や `UITextSkin.Apply` のサイズ計算を追跡し、どこで RectTransform が 0 になるかを特定する。
- 原因が Unity/TMP 側にある場合は、対象 UI の `useBlockWrap` や `blockWrap` の設定、`TMP_Text.ForceMeshUpdate` 後の `textInfo` を再チェックするロジックを追加する。

以上。SelectChanged 系のパッチを変更すると Tooltip や別 UI へ波及するリスクがあるため、本ドキュメントに沿って **対象クラスに処理を閉じ込める** のが原則です。
