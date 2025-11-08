# UTF-8 セーフティ手順

翻訳作業中に文字化けを再発させないための常設ガードです。

## 1. セッションの起動
1. PowerShell を開いたらまず `scripts/ensure_utf8.ps1` を実行し、入出力エンコーディングとコードページを UTF-8 に固定します。  
   ```powershell
   powershell.exe -ExecutionPolicy Bypass -File scripts/ensure_utf8.ps1
   ```
2. 毎回手動で実行したくない場合は `scripts/install_utf8_profile.ps1` を 1 度だけ実行します。これにより PowerShell プロファイルに自動起動ブロックが追加され、以降はコンソールを開くだけで UTF-8 化されます。  
   ```powershell
   powershell.exe -ExecutionPolicy Bypass -File scripts/install_utf8_profile.ps1
   ```

## 2. ファイル操作
- `Get-Content` / `Set-Content` / `Out-File` を使用する際は、必ず `-Encoding utf8` を付けて実行してください。
- VS Code / Neovim などのエディタも `.editorconfig` の設定に従い、保存時は UTF-8 / LF / 末尾改行ありを守ります。

## 3. コミット前チェック
1. 作業の締めに `scripts/check_encoding.ps1 -FailOnIssues` を実行します。
2. もし `繧` / `縺` といった典型的なモジバケ文字が検出されたら、UTF-8 で表示できるエディタで該当ファイルを開き、实际に壊れていないか確認してから修正します。

## 4. よくある落とし穴
- Windows PowerShell 5.x は `chcp` を変更しない限り CP932 のままなので、`scripts/ensure_utf8.ps1` を通さずに `Get-Content` すると “表示だけ” 化けた状態が見えます。保存前に必ず UTF-8 で読み直すこと。
- Mac / Linux でも `nkf` や `iconv` を使って別のコードページに変換したまま保存すると同様に壊れます。常に UTF-8 を選択してください。

上記を徹底すれば、「表示だけの文字化け」と「ファイル自体が壊れた状態」を取り違えることがなくなり、翻訳資産を安全に保てます。
