# QudJP Mod

## 概要
Caves of Qud を日本語でプレイするための Mod。本フォルダーを `%USERPROFILE%\AppData\LocalLow\Freehold Games\CavesOfQud\Mods\QudJP` に配置することで利用できます。

## 構成
- `manifest.json` : Mod メタデータ
- `Assemblies/` : Harmony でビルドした DLL
- `Fonts/` : CJK フォント OTF/TMP Assets
- `Localization/` : `Load="Merge"` 方式の翻訳 XML/TXT
- `Docs/` : 配布時 README/既知の問題など（ゲームの Mod マネージャから参照）

## インストール
1. 本フォルダを Mods ディレクトリにコピー。
2. ゲーム内の Mod Manager で `QudJP` を有効化。
3. 文字化け等があれば `%APPDATA%` 配下のログを添えて Issue へ報告してください。

## ライセンス
- 翻訳テキスト: CC BY-SA 4.0 (予定)
- Harmony コード: MIT
- フォント: 各フォントのライセンスに従う（Docs/font_pipeline.md 参照）
