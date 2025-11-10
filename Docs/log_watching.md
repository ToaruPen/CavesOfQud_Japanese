# ログ監視ガイド

Mod を検証するときは `build_log.txt`（ビルド時）と `Player.log`（実行時）を常時監視し、翻訳／UI の経路を一本鎖で追跡できるようにします。

## ログパス
- **Windows**  
  - `build_log.txt` : `%USERPROFILE%\AppData\LocalLow\Freehold Games\CavesOfQud\build_log.txt`  
  - `Player.log` : `%USERPROFILE%\AppData\LocalLow\Freehold Games\CavesOfQud\Player.log`
- **macOS (Steam)**  
  - `build_log.txt` : `~/Library/Application Support/Freehold Games/CavesOfQud/build_log.txt`  
  - `Player.log` : `~/Library/Logs/Freehold Games/CavesOfQud/Player.log`

## PowerShell 監視テンプレート
```powershell
Set-Location "$env:USERPROFILE\AppData\LocalLow\Freehold Games\CavesOfQud"

# build_log.txt
Get-Content .\build_log.txt -Tail 30 -Wait

# Player.log （JP/EID ログをフィルタ表示）
$log = ".\Player.log"
Get-Content $log -Tail 40 -Wait |
  Select-String -Pattern '\[JP\]|\bException\b|TMP_Text|SelectableTextMenuItem|Tooltip|Popup'
```

## ローテーション
ログが肥大化したらゲームを終了し、`.bak` などに退避 → 再起動して再生成する。削除前に必要なエラーは必ず Issue やノートへ貼り付けておくこと。

## 重点的に見る項目
- `!` 始まりの XRL ローダー警告や `Load="Merge"` 失敗。
- Harmony `ERROR` / `EXCEPTION`。
- `Missing TMP_FontAsset` 等のフォント参照エラー。
- 自動生成テキストや翻訳パイプラインで起こりやすい NullReference。

---

## 2025-11-11 追記 — EID 付き診断ログ

`Mods/QudJP/Assemblies` に診断基盤 (`JpLog`, `UIContext`, `TMPTextSetTextPatch`) を導入し、翻訳から描画までを **EID (Event ID)** で追跡できるようになりました。

### 仕組み
- 環境変数 `QUDJP_VERBOSE_LOG` が `0` / `false` でない限り、`UnityEngine.Debug.Log` に  
  `"[JP][カテゴリ][ステージ][EID:xxxxxxxx] メッセージ"` 形式で出力します。
- Popup/Tooltip/会話メニュー等の入り口で EID を払い出し、`Translator.Apply` と `TMP_Text.SetText` のログも同じ EID を使うので、「辞書 HIT → SetText/IN → SetText/OUT → 描画」の一本鎖を辿れます。
- `Translator.Apply` は HIT/MISS とキー長を記録し、空訳は必ず原文へフォールバック。
- `TMP_Text.SetText` は IN/OUT で文字長、`GetRenderedValues(true)`、フォント、wrap/overflow を記録。空文字を書いた場合はスタックトレースを出力。
- ログスパム対策として同一メッセージは 1 キー 50 回で打ち止めし、以降は `(suppressed further repeats)` の 1 行だけを追加します。

### 便利コマンド
```powershell
# JP ログ常時監視
$log = "$env:USERPROFILE\AppData\LocalLow\Freehold Games\CavesOfQud\Player.log"
Get-Content $log -Tail 40 -Wait |
  Select-String -Pattern '\[JP\]|\bException\b|TMP_Text|SelectableTextMenuItem|Tooltip|Popup'

# ある EID の履歴を抽出
Select-String -Path $log -Pattern '\[EID:([0-9a-f]{8})\]'
```

### 解析フロー例
1. 監視コマンドを起動したまま、ゲーム内で問題の UI（終了ダイアログ、会話選択肢、インベントリ比較など）を再現する。
2. ログに出てきた `EID:xxxxxxxx` を控え、同じ EID の `[JP][TR]`（辞書 HIT/MISS）、`[JP][TMP] SetText/IN`（割当に渡った文字列）、`SetText/OUT`（描画結果）を追う。
3. `TR` が HIT なのに `SetText/IN` で空なら割当段階、`IN` は非空なのに `OUT` で空なら描画設定（wrap/overflow/fallback フォント）を疑う。  
   `TR` が MISS なら辞書やキー正規化のミス。
4. 収集した EID ごとの流れを元に、Translator/TokenNormalizer/RenderGuard のどこを触るか判断する。

`QUDJP_VERBOSE_LOG=0` または `false` を環境変数に設定すると `[JP]` ログを無効化できます（通常プレイや動画撮影時など）。***
