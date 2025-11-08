# ObjectBlueprints / LLM Translation Protocol

This document defines the automation contract between `scripts/objectblueprint_extract.py`, an LLM translator, and the reinsertion tool. Keep it nearby whenever you generate or consume localization payloads for `references/Base/ObjectBlueprints/*.xml`.

## 1. Extraction payload (`scripts/objectblueprint_extract.py`)

Run:

```bash
python3 scripts/objectblueprint_extract.py \
  --base-file references/Base/ObjectBlueprints/Items.xml \
  --output work/items_missing.json
```

Key fields inside the JSON:

- `objects`: ordered slice of base `<object>` entries that still lack Japanese coverage (unless `--include-present` is set).
  - `name` / `inherits` / `base_index`: raw metadata from the base XML.
  - `parts`: only the `<part>` blocks that contain user-facing text. Each part lists every attribute as:
    - `id`: stable identifier `"{Object}:{PartIndex}:{PartName}:{Attribute}"`.
    - `value`: **decoded** XML value (so `&#10;` is emitted as `\n`, `&amp;` becomes `&`, etc.).
    - `translate`: true iff this attribute requires localization. Non-textual attributes are retained for context but should be copied verbatim when reinserting.
- `pending_strings`: flattened view of all strings that need translation. This is what we hand to the LLM.
  - `id`: matches the attribute id above (use this to align responses).
  - `object`, `part`, `attribute`, `type`: context describing what you are translating (`display-name`, `description-short`, `behavior-description`, `rules-text`, `proper-name`, or `generic`).
  - `inherits`: the base object's inheritance chain (useful for tone / category).
  - `color_tags`: parsed list of any `{{TOKEN|text}}` spans that appear in the source.
  - `contains_newline`: true if the original text contained `\n` (encoded as `&#10;` in XML).
  - `contains_markup`: flag for embedded XML entities.

> **Note:** Attribute values are already unescaped when serialized to JSON. When we write them back to XML the reinsertion script will re-escape `&`, `<`, `"` and convert newline characters back into `&#10;`.

## 2. Prompting an LLM

Feed only the `pending_strings` array (plus your instructions) to the translator. Recommended guideline block:

1. Produce valid JSON of the form
   ```json
   {
     "translations": [
       {"id": "Object:Idx:Part:Attribute", "translation": "Japanese string", "notes": "... optional ..."}
     ]
   }
   ```
   - Every `id` from the payload must appear once.
   - Keep ordering identical to the input array so diffs stay predictable.
2. Respect markup:
   - Preserve every `{{TOKEN|…}}` wrapper exactly. Only translate the text segment after the pipe, never the token (`TOKEN` can be a single letter like `Y` or a phrase such as `R-W-Y sequence`).
   - Keep inline references such as `%`, `+`, `-`, dice notation (`1d6`), stats (`HP`, `AV`, `DV`, `SP`), and numerals unchanged.
   - If the English contains literal `[brackets]` or `{braces}`, keep them—they often gate engine behaviors.
3. Line breaks are shown as `\n` in JSON. Mirror them in the translation; the reinserter converts them back to `&#10;`.
4. Follow the tone established in `Mods/QudJP/Localization/ObjectBlueprints/Items.jp.xml`:
   - Item names: concise noun phrases (no sentence-ending punctuation).
   - Descriptions: descriptive prose that favors present tense and keeps lore references intact.
   - Behavior descriptions / rules text: imperative or second-person directions that match in-game UI (retain English stat abbreviations per `Docs/tasks/objectblueprints.md`, point 6).
5. Color tags sometimes wrap partial phrases (e.g., `{{Y|solar cell}}`). Translate only the inner phrase while keeping the surrounding `{{…}}`.
6. If the source already contains Japanese or proper nouns, pass them through unchanged.

### Example payload fragment

```json
{
  "pending_strings": [
    {
      "id": "LongSword:123:Render:DisplayName",
      "object": "LongSword",
      "part": "Render",
      "attribute": "DisplayName",
      "type": "display-name",
      "source": "{{Y|long sword}}",
      "color_tags": [{"code": "Y", "text": "long sword"}],
      "inherits": "LongSword"
    },
    {
      "id": "LongSword:124:Description:Short",
      "object": "LongSword",
      "part": "Description",
      "attribute": "Short",
      "type": "description-short",
      "source": "A length of finely ground steel honed into a deadly edge.",
      "color_tags": [],
      "inherits": "LongSword"
    }
  ]
}
```

Expected LLM response:

```json
{
  "translations": [
    {
      "id": "LongSword:123:Render:DisplayName",
      "translation": "{{Y|長剣}}"
    },
    {
      "id": "LongSword:124:Description:Short",
      "translation": "精密に鍛えた鋼の刃で、致命の一撃へと研ぎ澄まされている。"
    }
  ],
  "meta": {
    "model": "gpt-5",
    "glossary": ["AV", "DV", "HP"]
  }
}
```

`notes` inside each translation item is optional and can be used to flag uncertainties for human review.

## 3. Reinsertion expectations

`scripts/objectblueprint_insert.py` handles the merge:

```bash
python3 scripts/objectblueprint_insert.py \
  --payload work/items_missing.json \
  --translations work/items_translated.json
```

It will:

1. Load the extraction payload (structure + metadata).
2. Load the LLM response (`translations` array).
3. Produce `<object ... Replace="true">` snippets where each translatable attribute is replaced with the translated value while every non-text attribute is copied from the base XML.
4. Append these snippets (in base order) just before the closing `</objects>` inside `Mods/QudJP/Localization/ObjectBlueprints/Items.jp.xml` (or the file passed via `--target`).

Therefore:

- Do **not** edit `Mods/QudJP/...` manually when using this pipeline. Always let the reinsertion script apply translations so attribute order, escaping, and `Replace="true"` flags stay consistent.
- Keep the exact extraction payload you shared with the LLM—reinsertion relies on those `id` values to match translations safely.
- If you rerun extraction after editing the base XML, regenerate translations because the attribute IDs (`Object:PartIndex:Part:Attribute`) are derived from the base structure and will no longer match.

## 4. Color / formatting tag cheat sheet

| Token pattern              | Meaning / handling                              |
| -------------------------- | ----------------------------------------------- |
| `{{Y|text}}`, `{{R|text}}` | Colorized name segments – translate `text` only |
| `{{R-W-Y sequence|text}}`  | Gradient tokens – same rule as above            |
| `{{hot|text}}`, `{{cold|text}}` | Status-inflected wrappers – keep token      |
| `{{A|object}}` / `{{an|object}}` | Article macros – translate noun, keep macro |

If an unfamiliar token appears, leave it intact and translate only after the pipe. The extractor lists these tokens in `color_tags` for quick reference.

## 5. Checklist

1. `python3 scripts/objectblueprint_extract.py --base-file … --output …`
2. Send `pending_strings` + these instructions to the LLM.
3. Receive JSON response adhering to the `translations` schema.
4. `python3 scripts/objectblueprint_insert.py --payload work/items_missing.json --translations work/items_translated.json`
5. Run `python3 scripts/diff_localization.py --missing-only` to confirm coverage and update the docs.

This document lives alongside `Docs/tasks/objectblueprints.md` and inherits all glossary / style guidance listed there. Update both whenever the pipeline evolves.
