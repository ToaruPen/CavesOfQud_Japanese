# パイプライン調査バックログ v1

進捗済み UI 以外にも翻訳フックを検討したい画面が多数存在する。以下は ILSpy (.ilspy/Assembly-CSharp/Qud.UI/*.cs) をざっと確認して抽出した優先候補。現時点では未調査なので、実際に着手する際は各ファイルを参照したうえで Docs/pipelines/*.md へ昇格させること。

| 優先度 | サブシステム / 画面 | 主要ファイル | 現状 | 備考 |
| --- | --- | --- | --- | --- |
| ★☆☆ | Help / Options / Keybinds | Qud.UI.HelpScreen.cs, OptionsScreen.cs, KeybindsScreen.cs | 実装進行中 (Docs/pipelines/help_options_keybinds.md) | コンソール版 (`XRLManual`, `OptionsUI`, `KeyMappingUI`) と Modern UI の差分、フック案、テスト項目まで整理済み。 |
| ★☆☆ | WorldGeneration / Embark Overlays | Qud.UI.WorldGenerationScreen.cs, EmbarkBuilderOverlayWindow.cs | 実装済み (Docs/pipelines/worldgeneration.md) | 生成ログ（Console/Modern 共通）を文書化。Embark overlay 側は `Docs/pipelines/characterbuild.md` 参照。 |

> 優先度の目安: ★★★ = 直近で必要, ★★☆ = 翻訳フック設計を早期に進めたい, ★☆☆ = 後回しでも可。
> 着手する際はこの表を更新し、調査済みになったら Docs/pipelines/*.md と pipelines.csv へ追加する。必要に応じて新たな候補を追記すること。