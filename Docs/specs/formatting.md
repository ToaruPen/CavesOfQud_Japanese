# Formatting Spec (Markup / RTF / Wrapping) v1

This document summarizes the text-formatting pipeline used by Caves of Qud when the QudJP mod injects translations. Understand how strings flow through `Markup` -> `ClipTextToArray` -> `TextBlock / RTF` -> `TMP_Text` so we can avoid double-formatting or layout regressions.

## 1. Markup & Console Color Codes

| Notation | Role | Notes |
| --- | --- | --- |
| `{{K|text}}` | Markup macro. `Markup.Transform` expands it to console color codes such as `&k` / `^k`. | Cannot be nested. Split long strings into minimal units before creating dictionary keys. |
| `&k` / `^k` | Console color sequences. Single-character codes that store foreground/background colors. | `Sidebar.FormatToRTF` converts them to `<color=#RRGGBBAA>`. Use `&&` / `^^` to escape literal ampersands and carets. |
| `{{something}}` | Placeholder resolved by `Grammar` / `HistoricStringExpander`. | Leave `{{` `}}` intact; only replace the inner text during translation. |
| `&y^K` etc. | `StringFormat.ClipTextToArray` **propagates the last color to the next line** (`KeepColorsAcrossNewlines=true`). | If you close the tag mid-line, the next line will no longer receive `&?` / `^?`, so keep pairs balanced. |

### Markup.Transform Guidelines
- Many APIs (`StringFormat.ClipTextToArray`, `Popup.RenderBlock`, `Sidebar.FormatToRTF`, etc.) call `Markup.Transform` internally. Translation hooks should therefore run *before* the transform step.
- When `EscapeNonMarkupFormatting=true` (e.g. in `Popup.WaitNewPopupMessage`), the engine escapes `&`/`^` to `&&`/`^^`. Ensure dictionary entries match the escaped form.

## 2. ClipTextToArray / TextBlock

`XRL.UI.StringFormat.ClipTextToArray(string input, int maxWidth, ...)`

- Performs word-wrapping. If `nextWordLength + currentLine > maxWidth`, the current line is emitted and the next line starts with the pending word.
- `KeepNewlines=true` preserves original line breaks. `Popup.PopupTextBuilder` calls it with `RespectNewlines=true`, so changing newline counts in translations can change box heights.
- `KeepColorsAcrossNewlines=true` (default) inserts the active `&x` / `^y` at the beginning of the next line.
- If `TransformMarkup=true` and multiple lines exist, **each line is passed through `Markup.Transform`**. Unbalanced tags therefore cause rendering issues.
- `ConsoleLib.Console.TextBlock` also relies on ClipTextToArray and exposes width (`width`) and maximum lines (`maxLines`). `Popup.RenderBlock` typically uses width 78.

### Column Alignment / Units
- CP437 assumes ASCII = 1 column while fullwidth characters take 2 columns. Mixing double-width characters shifts wrap positions. If you convert `{0} dram` into `{0} doramu` (JP), expect different wrapping.
- When Grammar logic needs to know the position of articles or units, keep placeholders and mention the behavior in translator notes.

## 3. RTF / TMP RichText

| API | Role | Notes |
| --- | --- | --- |
| `Sidebar.FormatToRTF` | Converts Markup (`&`/`^`) into TMP `<color>` tags and maps CP437 to Unicode. | `opacity` is often `FF` (opaque). |
| `RTF.FormatToRTF` | Optionally wraps text via `TextBlock`, then calls `Sidebar.FormatToRTF`. | Per-call `blockWrap`/`opacity` values live in `Docs/pipelines.csv`. |
| `TMP_Text.SetText` / `UITextSkin.SetText` | Final rendering stage. | TMP supports `<align>`, `<line-height>`, `<sprite>`, etc. Use TMP-compliant tags only. |

### TextMeshPro Font Handling
- `FontManager` swaps in the bundled `NotoSansCJKjp-*-Subset.otf` assets (Regular/Bold). Keep vanilla (`CavesOfQud SDF`) as fallback for unlocalized chunks.
- Allowed TMP tags: `<color>`, `<size>`, `<align>`, `<sprite>`, etc. `<font>` is not supported; use Markup colors instead.
- Missing glyphs log `Missing glyph in font asset` to `Player.log`. Update `Docs/glyphset.txt` and rebuild the subset when that happens.

## 4. Wrapping / Prohibited Breaks
- Console UI implements **only English-style word wrapping**. If JP punctuation must stay with the preceding token, translate the pair as a single segment (for example, keep the opening/closing quote plus the word inside).
- Modern UI (TMP) uses `WordWrapping`. Fullwidth chars count as 2 columns; to prevent breaking, inject `<noparse>` (TMP 3.0+) or zero-width spaces.
- `RTF.BlockWrap` often receives `maxLines=5000`, so the TMP layout ultimately decides height.

## 5. Plurals / Articles
- `Grammar.Pluralize`, `Grammar.A`, etc. tweak English output. When translations change word forms, **template the variable parts** (`{itemName}`) and keep grammar hints in translator notes.
- Strings produced by `HistoricStringExpander` or `StringEvaluator` eventually reach `Markup.Transform`, so do not break their tags when localizing.

## 6. Frequently Used Formatting APIs

| API | Typical Callers | Recommended Hook | Notes |
| --- | --- | --- | --- |
| `Markup.Transform` | Popup, MessageLog, Tooltip, etc. | Run translations before Transform whenever possible. | Mind the `List<string>` overload. |
| `StringFormat.ClipTextToArray` | Popup, Journal, QuestLog | Record `MaxWidth` and `ContextID` for debugging. | Default `KeepColorsAcrossNewlines=true`. |
| `TextBlock` | Popup, Book UI, `RTF.BlockWrap` | Same as above. | `maxLines` controls height. |
| `Sidebar.FormatToRTF` | Tooltip, MessageLog, PopupMessage | Good place to confirm final Markup->TMP conversion. | |
| `RTF.FormatToRTF` | Tooltip, MessageLogWindow | Avoid further transformations afterwards. | With `blockWrap>0`, flow is TextBlock->TMP. |

## 7. Checklist / Diagnostics
1. **Console layout**: run with `Options.ModernUI=false`, open popups/logs, and verify `<up/down for more...>` prompts behave.
2. **Unity / TMP**: monitor `Player.log` for `Missing glyph` or RichText exceptions. Compare translator hit/miss ratios per ContextID (`[JP][TR][HIT/MISS]`).
3. **Offline reproduction**: use `DumpPopup.exe` or ad-hoc scripts to call `StringFormat.ClipTextToArray` with long texts and confirm wrap/tag balance.

## 8. TODO
- Catalog TMP `<link>` / `<sprite>` usage across UI screens and document them under `Docs/pipelines/*.md`.
- If we add Japanese-style line prohibition to `ClipTextToArray`, document the pseudo implementation here before shipping.

## 9. Safe Editing of Dictionary Files
- Editing `Localization/Dictionaries/*.json` directly in editors risks corrupting BOM or quoting. **Always read/modify/write via a script** (PowerShell + `py`, etc.).
- Suggested workflow:
  1. Create a temporary `.py` script and load the file with `json.loads(Path(...).read_text(encoding='utf-8-sig'))`.
  2. Modify the target entries (e.g., `entry['context'] = "Qud.UI.PopupMessage.ShowPopup.Message"`).
  3. Save using `json.dumps(..., ensure_ascii=False, indent=2)` and `encoding='utf-8'`.
- Example:
  ```powershell
  py - <<'PY'
  import json
  from pathlib import Path

  path = Path('Mods/QudJP/Localization/Dictionaries/ui-default.ja.json')
  data = json.loads(path.read_text(encoding='utf-8-sig'))
  for entry in data['entries']:
      if entry['key'] == "' to confirm.":
          entry['context'] = "Qud.UI.PopupMessage.ShowPopup.Message"
  path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
  PY
  ```
- Delete the temporary `.py` afterwards. This keeps UTF-8/BOM intact and produces reviewable diffs.
