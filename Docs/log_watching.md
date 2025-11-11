# ログ監視ガイド

Mod の挙動を確認する際は `build_log.txt` と `Player.log` の両方を追いかける。どちらも Unity 側が自動で更新するため、監視用シェルを開きっぱなしにしておくと便利。

## ログの場所
- **Windows**  
  - `build_log.txt` : `%USERPROFILE%\AppData\LocalLow\Freehold Games\CavesOfQud\build_log.txt`  
  - `Player.log` : `%USERPROFILE%\AppData\LocalLow\Freehold Games\CavesOfQud\Player.log`
- **macOS (Steam)**  
  - `build_log.txt` : `~/Library/Application Support/Freehold Games/CavesOfQud/build_log.txt`  
  - `Player.log` : `~/Library/Logs/Freehold Games/CavesOfQud/Player.log`

## PowerShell 監視ワンライナー
```powershell
Set-Location "$env:USERPROFILE\AppData\LocalLow\Freehold Games\CavesOfQud"

# build_log.txt
Get-Content .\build_log.txt -Tail 30 -Wait

# Player.log （JP ログと例外を重点監視）
$log = ".\Player.log"
Get-Content $log -Tail 40 -Wait |
  Select-String -Pattern '\[JP\]|\bException\b|TMP_Text|SelectableTextMenuItem|Tooltip|Popup'
```

## ログを扱う際の注意
- 退避するときは `.bak` を付けるか別フォルダへコピーし、Unity 側が参照する元ファイルは空にしない。
- `Player.log` が巨大化したら PowerShell を止めてから削除→ゲーム再起動で再生成される。
- 疑わしい行はすぐに `Issue Tracker` に貼れるよう、前後 40 行をまとめて控える。

## 監視時に注目すべきパターン
- `!` や `ERROR` が付いた `XRL` / `Harmony` ログ
- `Missing TMP_FontAsset` 系（フォント収集漏れのサイン）
- `NullReferenceException` / `Exception` / `MODERROR`
- `[QudJP]` プレフィックス（Mod が明示的に出している診断）

---

## 2025-11-11: EID 付き JP ログの読み方
`Mods/QudJP/Assemblies` に配置された `JpLog`,`UIContext`,`TMPTextSetTextPatch` 等が **EID (Event ID)** を生成し、`[JP][カテゴリ][ステージ][EID:xxxxxxxx]` 形式で `Player.log` に書き込む。

### 実戦で見るポイント
1. ゲーム起動時に `QUDJP_VERBOSE_LOG=1` をセット（PowerShell なら `$env:QUDJP_VERBOSE_LOG='1'`）。
2. まず `[JP][Popup|Tooltip|Menu|Inventory][HOOK][EID:xxxx] ...` が出ているか確認。これが出ない場合、Harmony Prefix が外れている。
3. 同じ EID で `[JP][TR]`（Translator HIT/MISS）と `[JP][TMP] set_text/IN|OUT` が続くかを見る。流れが切れている場合は UIContext のバインド漏れや別オブジェクトで発火している可能性が高い。
4. `TR` が MISS のままなら辞書追加、`TMP` の OUT で `len=0` や `overflow` が出ていればレイアウト側のガードを疑う。

### 便利なコマンド例
```powershell
$log = "$env:USERPROFILE\AppData\LocalLow\Freehold Games\CavesOfQud\Player.log"

# JP ログと例外だけをライブ表示
Get-Content $log -Tail 80 -Wait |
  Select-String -Pattern '\[JP\]|\bMODERROR\b|\bException\b'

# 直近の EID をざっと確認
Select-String -Path $log -Pattern '\[EID:([0-9a-f]{8})\]' | Select-Object -Last 20
```

これらを常時流しておけば、翻訳処理と TMP 表示のどこで異常が起きたかを即座に突き止められる。
