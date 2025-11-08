# QudJP Mod

## 概要
Caves of Qud を日本語でプレイするための Mod です。  
フォント差し替え・Harmony パッチ・翻訳データを同梱しており、フォルダを丸ごと Mods ディレクトリに配置するだけで利用できます。

## フォルダ構成
- `manifest.json` – Mod メタデータ（Id/Title/Version など）。
- `Assemblies/` – `QudJP.dll` ほか Harmony ビルド成果物。
- `Fonts/` – サブセット化した CJK OTF（Harmony がランタイムで TMP Font Asset を生成）。
- `Localization/` – `Load="Merge"` 方式の日本語化 XML / TXT。
- `Docs/` – Mod 内 README / Known issues / Workshop 向けドキュメント。

## インストール
1. この `QudJP` フォルダ全体を、以下いずれかの Mods ディレクトリにコピーまたはシンボリックリンクで配置する。  
   - `%USERPROFILE%\AppData\LocalLow\Freehold Games\CavesOfQud\Mods\QudJP`  
   - （または Steam 版の）`C:\Program Files (x86)\Steam\steamapps\common\Caves of Qud\Mods\QudJP`
2. ゲームを起動し、メインメニューの Mod Manager で `Caves of Qud 日本語化 (QudJP)` を有効化する。
3. 文字化け・フォント未適用などの問題を見つけた場合は、`Player.log` を添えてリポジトリの Issue へ報告してください。

## ライセンス
- 翻訳テキスト: CC BY-SA 4.0（予定）
- Harmony コード: MIT
- フォント: 各フォントのライセンス（Docs/font_pipeline.md 参照、主に SIL Open Font License 1.1）

README / Known issues / Workshop 説明など Mod 同梱ドキュメントは `Mods/QudJP/Docs` を参照してください。
