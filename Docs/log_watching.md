# ログ監視手順

Mod 開発中は `build_log.txt`（読み込み時）と `Player.log`（実行時）を常に監視し、XML マージエラーや Harmony 例外を即座に把握します。

## ログの場所
- `build_log.txt` : `%USERPROFILE%\AppData\LocalLow\Freehold Games\CavesOfQud\build_log.txt`
- `Player.log` : `%USERPROFILE%\AppData\LocalLow\Freehold Games\CavesOfQud\Player.log`

## PowerShell 監視コマンド
```powershell
# build_log.txt
Set-Location "$env:USERPROFILE\AppData\LocalLow\Freehold Games\CavesOfQud"
Get-Content -Path .\build_log.txt -Tail 30 -Wait

# Player.log
Get-Content -Path .\Player.log -Tail 50 -Wait
```

## ローテーション
ログが肥大化したらゲームを終了し、`.bak` などにリネームしてから再起動→再生成する。削除前に必要なエラーメッセージは必ず Issue やノートに貼り付けておく。

## 重点的にチェックする項目
- `!` から始まる XRL ローダーの警告や `Load="Merge"` 失敗。
- Harmony `ERROR` / `EXCEPTION` 行。
- `Missing TMP_FontAsset` などフォント参照エラー。
- 自動生成テキストの NullReference など、翻訳中の表記揺れで発生しがちな例外。

監視を自動化したい場合は `scripts/watch_logs.ps1` を後日追加予定。***
