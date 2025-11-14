# Caves of Qud Japanese Localization

日本語で Caves of Qud を遊ぶための総合ローカライズ Mod です。  
ゲーム内の UI / テキストほぼ全域を Harmony + TextMeshPro でパッチし、安定した表示と翻訳プロセスを提供します。

## リポジトリ構成
| パス | 説明 |
| --- | --- |
| `Mods/QudJP` | Mod 本体。manifest・Harmony DLL・フォント・Localization XML/TXT・Workshop 向け README などが入ります。 |
| `Mods/QudJP/Assemblies` | Harmony プロジェクト (`QudJP.sln`)。フォント管理・UI パッチ・翻訳フックの C# 実装を配置。 |
| `Docs/` | フォント／翻訳パイプライン、テスト計画、ログ監視などの技術資料。すべて UTF-8 (BOM 無し) で保存します。 |
| `Docs/pipelines` | UI サブシステムごとの処理まとめ。Tooltip / Inventory / WorldGen … などを Markdown と `pipelines.csv` で管理。 |
| `scripts/` | Python ベースの補助ツール群。ベースデータ抽出、翻訳差分、エンコーディング検証、Mod ディレクトリ同期など。 |
| `references/Base` | ゲームから抽出した原文 XML/TXT。`.gitignore` 対象のため必要に応じて `scripts/extract_base.py` で更新してください。 |

## フォントとアイコン
- `Docs/font_pipeline.md` の手順に沿って Noto Sans CJK JP をサブセット化し、`Mods/QudJP/Fonts` に `*-Subset.otf` を配置します。
- `FontManager` が起動時にフォントを `TMP_FontAsset.CreateFontAsset` から生成し、`TMP_Settings` の default/fallback を更新します。`UITextSkin` / `TMP_InputField` / `UnityEngine.UI.Text` の Harmony パッチで一括適用されます。
- `<sprite>` アイコンは `RegisterSpriteAssets` が Qud の Sprite Asset を自動検出し、`TMP_Settings.defaultSpriteAsset` および各 SpriteAsset の `fallbackSpriteAssets` に挿入します。

## ビルド
```powershell
dotnet build Mods/QudJP/Assemblies/QudJP.sln `
  -c Release `
  /p:GameDir="C:\Program Files (x86)\Steam\steamapps\common\Caves of Qud"
```
ビルド成果物 `QudJP.dll` は `Mods/QudJP/Assemblies/bin/Release` に出力されます。`scripts/sync_mod.py` を使うと Mod ディレクトリへ自動コピーできます。

## 翻訳ワークフロー
1. **ベース更新**  
   ```powershell
   python scripts/extract_base.py --game-path "C:\Program Files (x86)\Steam\steamapps\common\Caves of Qud"
   ```  
   で最新の Conversations / Books / ObjectBlueprints などを `references/Base` に展開します。
2. **翻訳編集**  
   `Mods/QudJP/Localization/*.jp.xml` を編集し、必要な場合は `Docs/translation_process.md` のガイドに従って Glossary / Status を更新します。
3. **差分確認**  
   ```powershell
   python scripts/diff_localization.py --missing-only
   ```  
   で未翻訳エントリをチェックし、`Docs/translation_status.md` に反映します。
4. **Mod 同期 & 動作確認**  
   ```powershell
   python scripts/sync_mod.py
   ```  
   で `Mods/QudJP` をゲームの Mod ディレクトリへコピーし、`Player.log` を監視しながら実機確認します。

## スクリプト一覧
| コマンド | 用途 |
| --- | --- |
| `scripts/extract_base.py --game-path <path>` | ゲームから最新の XML/TXT を抽出し `references/Base` を更新。 |
| `scripts/diff_localization.py [--missing-only] [--json-path report.json]` | `<object Name>` 単位で翻訳の欠落・変更をレポート。 |
| `scripts/check_encoding.py [--fail-on-issues]` | Docs / Mods の文字コードを検査し、UTF-8 以外や制御文字を検知。 |
| `scripts/sync_mod.py [--dry-run] [--exclude-fonts]` | 作業リポジトリとゲーム側 Mod ディレクトリを同期。 |

## ドキュメント
- `Docs/pipelines/*.md` はサブシステムごとの Hook 契約や ContextID、テスト観点をまとめた資料です。`pipelines.csv` と併せて常に最新化します。
- `Docs/test_plan.md` は QA 観点、`Docs/log_watching.md` は `Player.log` の監視手順を記載しています。
- すべてのドキュメントで UTF-8 (BOM 無し) を強制し、PR では `scripts/check_encoding.py` の結果を貼る運用です。

## コントリビューション指針
1. 変更対象のパイプライン資料を更新し、ContextID / Hook ポイント / 既知の制約を明記してください。
2. 必要に応じて `scripts/check_encoding.py` と `python scripts/diff_localization.py --missing-only` の結果を添付し、再現手順も共有します。
3. フォント周りを変更する場合は `<sprite>` / PUA の扱いを `Docs/font_pipeline.md` に追記します。
4. `Player.log` の抜粋（Missing glyph や RichText 警告など）を添えて、動作確認済みであることを示してください。

Issues / Discussions / Pull Request はいつでも歓迎です。質問があればお気軽にどうぞ。
