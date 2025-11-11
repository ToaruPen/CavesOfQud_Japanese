# Tooltip 繝代う繝励Λ繧､繝ｳ莉墓ｧ・v1

> **逕ｻ髱｢ / 驛ｨ菴・** Looker / 蜿ｳ繧ｯ繝ｪ繝・け Tooltip / 繧､繝ｳ繝吶Φ繝医Μ hover  
> **蜃ｺ蜉・** Unity (ModelShark.TooltipTrigger + TMP)

## 讎りｦ・
- Tooltip 縺ｮ譛ｬ譁・・ `XRL.UI.Look.GenerateTooltipContent`・・ameQueue・峨〒讒狗ｯ峨＠縲ゞI 蛛ｴ縺ｧ縺ｯ `TooltipTrigger.SetText("BodyText", RTF.FormatToRTF(...))` 縺ｫ貂｡縺吶・- `TooltipInformation` 讒矩菴薙〒 DisplayName / SubHeader / WoundLevel / LongDescription / Icon 縺御ｸ諡ｬ邂｡逅・＆繧後ｋ縲・- ContextID 萓・ `XRL.UI.Look.GenerateTooltipContent.Body`, `XRL.UI.Look.GenerateTooltipInformation.FeelingText`, `.DifficultyText`, `.WoundLevel`, `.ColonLabel`, `ModelShark.TooltipTrigger.SetText.TooltipField`.

## 荳ｻ縺ｪ繧ｯ繝ｩ繧ｹ / 繝｡繧ｽ繝・ラ

| 繝輔ぉ繝ｼ繧ｺ | 繧ｯ繝ｩ繧ｹ | 繝｡繧ｽ繝・ラ / 蛯呵・|
| --- | --- | --- |
| 逕滓・ | `XRL.UI.Look` | `GenerateTooltipInformation`・・Description.GetLongDescription`, `Strings.WoundLevel` 縺ｪ縺ｩ繧堤ｵ仙粋・峨・|
| 謨ｴ蠖｢(譁・ｭ怜・蛹・ | `XRL.UI.Look` | `GenerateTooltipContent` 縺・`StringBuilder` 竊・`Markup.Transform` 繧貞ｮ滓命縲・|
| 髱槫酔譛滓ｩ区ｸ｡縺・| `GameManager.Instance.gameQueue` | `executeAsync(() => GenerateTooltipContent())` 縺ｧ繧ｲ繝ｼ繝繝・・繧ｿ繧貞叙蠕励・|
| 謨ｴ蠖｢(RTF) | `Sidebar.FormatToRTF` / `RTF.FormatToRTF` | 譁・ｭ怜・繧・RichText・亥ｹ・~60, 繝輔か繝ｳ繝医き繝ｩ繝ｼ `FF`・峨∈螟画鋤縲・|
| 謠冗判 | `ModelShark.TooltipTrigger` | `SetText` 竊・`TMP_Text` 縺ｸ繧ｻ繝・ヨ縺励～ShowManually` 縺ｧ陦ｨ遉ｺ縲・|

## 繝・・繧ｿ繝輔Ο繝ｼ

1. `Look.ShowItemTooltipAsync` 縺・`gameQueue.executeAsync` 縺ｧ `GenerateTooltipContent(go)` 繧貞ｮ溯｡鯉ｼ・hread: `gameQueue`・峨・2. Game 繧ｹ繝ｬ繝・ラ縺ｧ `TooltipInformation` 繧呈ｧ狗ｯ・竊・`StringBuilder` 縺ｫ DisplayName / LongDescription / SubHeader / WoundLevel 繧帝・↓ `AppendLine`縲・3. `Markup.Transform` 縺ｧ濶ｲ繧ｿ繧ｰ繧帝←逕ｨ縺励◆譁・ｭ怜・繧定ｿ泌唆縲・4. UI 繧ｹ繝ｬ繝・ラ (`await The.UiContext`) 縺ｧ `RTF.FormatToRTF(contents)` 繧ゅ＠縺上・ `Sidebar.FormatToRTF(contents, "FF", 60)` 繧帝壹＠縲ゝMP RichText 縺ｫ螟画鋤縲・5. `TooltipTrigger.SetText("BodyText", ...)` 縺梧枚蟄怜・繧・TMP 縺ｫ荳弱∴縲～tooltip.ShowManually` 縺御ｽ咲ｽｮ隱ｿ謨ｴ繝ｻ陦ｨ遉ｺ縲・6. `TooltipTrigger.onHideAction` 縺・`gameQueue.queueTask` 繧堤匱陦後＠縲～AfterLookedAt` 縺ｪ縺ｩ縺ｮ繧､繝吶Φ繝医ｒ逋ｺ轣ｫ縲・
## 謨ｴ蠖｢隕丞援

- `GenerateTooltipContent`: DisplayName 竊・blank line 竊・LongDescription 竊・blank line ﾃ・ 竊・SubHeader 竊・WoundLevel縲・ 
  陦碁・′蝗ｺ螳壹↑縺ｮ縺ｧ縲∫ｿｻ險ｳ譎ゅｂ蜷後§讒区・繝ｻ謾ｹ陦梧焚繧堤ｶｭ謖√☆繧九・- `RTF.FormatToRTF(..., "FF", 60)` 縺ｧ **蟷・60 譁・ｭ・* 繧貞燕謠舌→縺励◆謚倥ｊ霑斐＠縺瑚｡後ｏ繧後∬牡縺ｯ繝・ヵ繧ｩ繝ｫ繝医〒 #FFFF00 縺ｫ繝槭ャ繝励・- `TooltipTrigger` 縺ｯ TMP RichText 繧定ｧ｣驥医Ａ<color=#FF>` 邉ｻ繧ｿ繧ｰ縺ｯ `RTF.FormatToRTF` 蛛ｴ縺ｧ逕滓・縺輔ｌ繧九・縺ｧ縲∫ｿｻ險ｳ蛛ｴ縺ｧ `<color>` 繧定ｿｽ蜉縺吶ｋ蠢・ｦ√・縺ｪ縺・・- `TooltipInformation.SubHeader` 縺ｯ `FeelingText` + `DifficultyText` 繧・`", "` 騾｣邨舌☆繧九ょ句挨鄙ｻ險ｳ縺ｫ縺励◆縺・ｴ蜷医・ `SubHeader.Feeling`, `.Difficulty` 繧・ContextID 縺ｨ縺励※蛻・ｲ舌＆縺帙ｋ縲・
## 蜷梧悄諤ｧ

- 逕滓・ (`GenerateTooltipInformation` / `Content`) 縺ｯ `gameQueue` 蟆ら畑縲ゅ％縺薙〒 UI 繧ｪ繝悶ず繧ｧ繧ｯ繝医↓隗ｦ繧九→繧ｯ繝ｩ繝・す繝･縺吶ｋ縲・- 謠冗判 (`TooltipTrigger.SetText`) 縺ｯ `uiQueue`・・The.UiContext` or `GameManager.Instance.uiQueue.queueTask`・峨〒螳溯｡後・- 鄙ｻ險ｳ蜃ｦ逅・ｒ蜈･繧後ｋ縺ｨ縺阪・ **縺ｩ縺｡繧峨・繧ｭ繝･繝ｼ縺ｧ蜻ｼ縺ｰ繧後ｋ縺・*繧・ContextID 蜷阪↓蜷ｫ繧√ｋ・井ｾ・ `Look.GenerateTooltipContent.Body` vs `TooltipTrigger.SetText.Body`・峨・
## 鄂ｮ謠帛ｮ牙・轤ｹ・域耳螂ｨ繝輔ャ繧ｯ・・
- `Harmony Prefix: XRL.UI.Look.GenerateTooltipInformation`  
  - ContextID: `XRL.UI.Look.GenerateTooltipInformation.DisplayName` 縺ｪ縺ｩ縲・ 
  - 髟ｷ譁・・遏ｭ譁・ｒ蛻・牡縺励※菫晄戟縺ｧ縺阪ｋ縲ょ推繝輔ぅ繝ｼ繝ｫ繝峨′蜀榊茜逕ｨ縺輔ｌ繧九◆繧√∝・繝輔か繝ｼ繝槭ャ繝域凾縺ｮ驥崎､・ｒ髦ｲ縺偵ｋ縲・- `Harmony Prefix: XRL.UI.Look.GenerateTooltipContent`  
  - ContextID: `XRL.UI.Look.GenerateTooltipContent.Body`.  
  - SubHeader / LongDescription 繧偵∪縺ｨ繧√※蜃ｦ逅・＠縺溘＞蝣ｴ蜷医・縺薙■繧峨〒鄙ｻ險ｳ縺励～Markup.Transform` 蜑阪↓蟾ｮ縺苓ｾｼ繧縲・- `Harmony Prefix: ModelShark.TooltipTrigger.SetText`・医ヰ繝・け繧｢繝・・・・ 
  - ContextID: `ModelShark.TooltipTrigger.SetText.TooltipField`.  
  - RTF 螟画鋤蠕後・譁・ｭ怜・繧堤峩謗･蟾ｮ縺玲崛縺医ｋ譛邨よ焔谿ｵ縲ゅ◆縺縺・RTF 陦ｨ迴ｾ・・{\rtf1...}`・峨・蛻ｶ邏・′蜴ｳ縺励＞縺溘ａ縲∵･ｵ蜉幃∩縺代ｋ縲・
## 萓区枚 / 繝医・繧ｯ繝ｳ

- DisplayName: `"{{G|" + go.BaseDisplayName + "}}"`  
- LongDescription: `Description.GetLongDescription` 逕ｱ譚･縺ｧ `{{d|...}}` 繧ｿ繧ｰ繧貞性繧縲・ 
- SubHeader: `"neutral, average"` 縺ｮ繧医≧縺ｪ `Feeling, Difficulty` 繝・く繧ｹ繝医・ 
- 迚ｹ谿・Tooltip: `Look.QueueLookerTooltip` 縺ｧ縺ｯ `ParameterizedTextField` (`DisplayName`, `ConText`, `WoundLevel`, `LongDescription`) 縺ｮ蜷・ヵ繧｣繝ｼ繝ｫ繝峨↓蛻･縲・・ RTF 繧呈ｵ√＠霎ｼ繧縲・
## 繝ｪ繧ｹ繧ｯ

- `GenerateTooltipContent` 蜀・・陦碁・燕謠舌′蟠ｩ繧後ｋ縺ｨ `TooltipTrigger` 繝ｬ繧､繧｢繧ｦ繝医′譛溷ｾ・壹ｊ縺ｫ譖ｴ譁ｰ縺輔ｌ縺ｪ縺・ｼ育ｩｺ陦瑚・菴薙′ UI 縺ｮ繧ｹ繝壹・繧ｵ縺ｨ縺励※菴ｿ繧上ｌ繧具ｼ峨・- `RTF.FormatToRTF` 縺ｯ蛻ｶ蠕｡譁・ｭ暦ｼ・\`・峨ｒ繧ｨ繧ｹ繧ｱ繝ｼ繝励☆繧九◆繧√∫ｿｻ險ｳ縺瑚ｿｽ蜉縺励◆ `\` 繧剃ｺ碁㍾蛹悶＠縺ｪ縺・ｈ縺・ｳｨ諢上・- `TooltipTrigger` 縺ｯ蜀榊茜逕ｨ繧ｭ繝｣繝・す繝･縺後≠繧九◆繧√∫ｿｻ險ｳ縺励◆譁・ｭ怜・縺ｫ `StringBuilder` 繧剃ｿ晄戟縺輔○繧九→ GC 繧ｳ繧ｹ繝医′蠅励☆縲ょｸｸ縺ｫ `string` 縺ｧ霑斐☆縲・
## 繝・せ繝域焔鬆・
1. 繧ｲ繝ｼ繝蜀・〒 `l` (Look) 竊・莉ｻ諢上・繧ｪ繝悶ず繧ｧ繧ｯ繝医ｒ繧ｿ繝ｼ繧ｲ繝・ヨ縲６I Tooltip 縺檎ｿｻ險ｳ貂医∩縺九ヾubHeader 縺梧Φ螳夐壹ｊ縺狗｢ｺ隱阪・2. 繧､繝ｳ繝吶Φ繝医Μ逕ｻ髱｢縺ｧ繧｢繧､繝・Β縺ｫ繝槭え繧ｹ hover縲ＡLook.ShowItemTooltipAsync` 繝代せ縺ｧ `RTF.FormatToRTF` 縺悟ｴｩ繧後↑縺・°繝√ぉ繝・け縲・3. `Player.log` 縺ｫ `TooltipTrigger` / `TMP` 縺ｮ RichText 繧ｨ繝ｩ繝ｼ縺悟・縺ｦ縺・↑縺・°逶｣隕悶・4. `Translator/JpLog` 縺ｧ `ContextID=Look.GenerateTooltipContent.Body` 縺ｮ繝偵ャ繝域焚繧堤｢ｺ隱阪＠縲∵悴鄙ｻ險ｳ縺梧ｮ九ｋ蝣ｴ蜷医・繧ｭ繝ｼ豁｣隕丞喧繧定ｿｽ蜉縲・
## Context 覚書

- `XRL.UI.Look.GenerateTooltipInformation.FeelingText` / `.DifficultyText` / `.WoundLevel` で SubHeader・傷レベルの辞書キーを管理し、`XRL.UI.Look.GenerateTooltipContent.Body` で本文をまとめて確定する（`Markup.Transform` の前）。
- `XRL.UI.Look.GenerateTooltipInformation.ColonLabel` を使うと `Weight:` などのラベル行を辞書で上書きできる。
- `ModelShark.TooltipTrigger.SetText.TooltipField` では `%DisplayName%` 等のプレースホルダを RTF 化済みのまま差し替える。UI へ渡す前に `<color>` 付きかどうかを判定し、二重変換を避ける。

- HookGuard: `TooltipParamMapCache` + `TranslationContextGuards` で gameQueue で決定した各 ParameterizedTextField の値を記憶し、`TMP_Text.set_text` 側では該当 EID/フィールド一致時に Translator をバイパスして Player.log の `MISS` スパムと RTF の二重整形を防ぐ。
