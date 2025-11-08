# UTF-8 Safety Checklist

This project treats UTF-8 (LF, final newline) as the single source of truth for every text asset.  
Follow this checklist every time you work on the repository to avoid reintroducing mojibake.

## 1. Prepare Your Shell
1. **Windows PowerShell 5.x**  
   Run `powershell.exe -ExecutionPolicy Bypass -File scripts/ensure_utf8.ps1` at the start of every session.  
   This sets `chcp 65001` and forces the console output encoding to UTF-8.  
   _Optional:_ run `scripts/install_utf8_profile.ps1` once to add the same snippet to your profile.
2. **PowerShell 7+ / macOS / Linux**  
   Run `pwsh -ExecutionPolicy Bypass -File scripts/ensure_utf8.ps1`.

## 2. Editing Files
- Use editors that respect `.editorconfig` (VS Code, Neovim, etc.). Verify the status bar shows **UTF-8** and **LF** before saving.
- When you touch files via shell commands, always pass `-Encoding utf8` to `Get-Content`, `Set-Content`, `Out-File`, etc.

## 3. Encode/Decode Checks
1. **Before starting work** – run `python3 scripts/check_encoding.py --fail-on-issues`.  
   Catching legacy CP932 fragments before you edit prevents spreading broken bytes.
2. **Before committing / after finishing work** – run the same command again.  
   Treat it like `lint` or `format`: it should pass cleanly every time.  
   Consider wiring it into pre-commit hooks or CI if you automate workflows.

## 4. Common Pitfalls
- **“Everything looks fine in PowerShell”** – Not true unless you forced UTF-8; CP932 often renders Japanese text but writes Windows-31J bytes back to disk.
- **Copy/pasting from external docs** – confirm the pasted content stayed UTF-8 by re-running the encoding script.
- **Bulk edits via scripts** – if a script is missing `encoding='utf-8'`, wrap it with the `ensure_utf8` helper or update it before running in batch.

Staying disciplined with these steps keeps the repository clean and avoids red herrings during translation work.
