# Caves of Qud Japanese Localization

「Caves of Qud」を日本語で快適に遊ぶための総合ローカライズ Mod です。  
会話・書籍・UI テキストを段階的に翻訳しつつ、Harmony ベースの補助 DLL と CJK 対応フォントを同梱して、ゲーム全体で文字化けなく表示できる環境を整えます。

## リポジトリ構成
- `Mods/QudJP` – 実際に配布する Mod フォルダー。manifest / Harmony DLL / Fonts / Localization / Mod 向け README を含みます。
- `Mods/QudJP/Assemblies` – Harmony プロジェクト (`QudJP.sln`). フォント管理や UI パッチなど C# 実装はこちらに配置。
- `Docs/` – フォント生成・翻訳手順・ログ監視・テスト計画などのドキュメント。
- `scripts/` – Python ベースのユーティリティ（バニラデータ抽出 / 差分レポート / コーディングガード / Mod 同期など）。
- `references/Base/` – ゲームから抽出した元データ（git ignore 済）。翻訳との差分確認に利用。

## 開発フロー（概要）
1. **ベースデータ更新**  
   ```bash
   # Windows
   python scripts/extract_base.py --game-path "C:\Program Files (x86)\Steam\steamapps\common\Caves of Qud"

   # macOS / Linux
   python3 scripts/extract_base.py --game-path "$HOME/Library/Application Support/Steam/steamapps/common/Caves of Qud"
   ```
   これで最新の Conversations / Books / ObjectBlueprints などが `references/Base` に取得される。macOS 版は Steam 配下の `Caves of Qud.app` 直上のフォルダを `--game-path` に指定する。

2. **フォント生成**  
   `Docs/font_pipeline.md` に沿って Noto Sans CJK などをサブセット化し、`Mods/QudJP/Fonts` に `*-Subset.otf` を配置。  
   Harmony 側の `FontManager` が起動時に OTF から TMP Font Asset を動的生成し、TextMeshPro / uGUI 双方へ適用します。

3. **Harmony ビルド**  
   ```powershell
   dotnet build Mods/QudJP/Assemblies/QudJP.sln `
     /p:GameDir="C:\Program Files (x86)\Steam\steamapps\common\Caves of Qud"
   ```
   で `QudJP.dll` を再生成します。ビルド成果物は `Mods/QudJP/Assemblies` に出力され、Mod 側からそのまま参照されます。

4. **翻訳ファイル編集**  
   `Mods/QudJP/Localization/*.jp.xml` を `Load="Merge"` 形式で追加。`Docs/translation_process.md` と `Docs/translation_status.md` を更新して進捗を管理します。  
   編集対象ファイルと担当カテゴリの一覧は `Docs/localization_targets.md` を参照してください。  
   カバレッジ状況は `python3 scripts/diff_localization.py --missing-only` でファイル / `<object Name>` 単位の欠落を一覧化できます（`--json-path` でレポート保存も可）。

5. **動作確認**  
   `python3 scripts/sync_mod.py` で `Mods/QudJP` を実機の Mods ディレクトリへミラーしてから、ゲーム内の Mod Manager で `Caves of Qud 日本語化` を有効化。文字化けや Missing Glyph がないか `Player.log` を監視します（`Docs/log_watching.md` 参照）。

## 主要スクリプト
| スクリプト | 用途 |
| --- | --- |
| `scripts/extract_base.py --game-path <path>` | ゲームから最新のバニラ XML/TXT を抽出し `references/Base` を更新する |
| `scripts/diff_localization.py [--missing-only] [--json-path report.json]` | 翻訳ファイル有無＋ `<object Name>` 単位の欠落をレポート。JSON も出力可能 |
| `scripts/check_encoding.py [--fail-on-issues]` | Docs / Mods を走査し、繧/縺 等のモジバケ候補を検出する |
| `scripts/sync_mod.py [--dry-run] [--exclude-fonts]` | `Mods/QudJP` を実際の Mods ディレクトリへミラーする |

## ドキュメント
- `Docs/font_pipeline.md` – Noto 系フォントのサブセット化と TMP Font Asset のランタイム生成方法。
- `Docs/translation_process.md` – 抽出～レビュー～リリース前チェックまでの翻訳フロー。
- `Docs/translation_status.md` – ファイルごとの進捗表。テンプレ追加済みのカテゴリから優先的に翻訳を進められます。
- `Docs/test_plan.md` / `Docs/log_watching.md` – QA / ログ監視手順。

## 貢献方法
Issues / Discussions / Pull Request を歓迎します。  
PR には以下を添付してもらえるとレビューがスムーズです。
1. 変更した翻訳ファイルやスクリプト。
2. 必要に応じてスクリーンショットと `Player.log`.
3. `python3 scripts/diff_localization.py --missing-only` の結果（Missing の有無）や `Docs/translation_status.md` の更新。

フォントライセンス等の注意点は `Docs` フォルダをご参照ください。

## ILSpy 参照用アーカイブ
- ゲーム本体を ILSpy で展開した参照用コードは、リポジトリ直下ではなく 1 つ上の階層にある `..\CavesOfQud_Japanese.ilspy.zip` と、同じ階層に展開済みの `..\CavesOfQud_Japanese.ilspy_extracted` に保管しています。
- 翻訳作業中は `..\CavesOfQud_Japanese.ilspy_extracted` を参照用として残し、作業完了後に zip を更新してください（どちらも Git 管理対象外）。