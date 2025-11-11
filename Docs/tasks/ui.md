# UI タスクボード

Options / EmbarkModules / Menus など UI 系 XML の未訳タスクを管理します。
python3 scripts/diff_localization.py --missing-only の出力で UI ファイルが挙がったら、このファイルに反映してください。

## 未訳 / 対応中
- [ ] Options.jp.xml: Legacy UI と Misc Prompts のプレースホルダーを最終レビューする。
- [x] EmbarkModules.jp.xml: Load="Replace" で UI 全体を再適用し、Mutations / Cybernetics の参照テキストも最新ベースに揃えた。
- [x] Genotypes.jp.xml / Subtypes.jp.xml: キャラ作成画面のジェノタイプ + カースト／天職の名称・説明を日本語化し、DisplayName とステータス訳を付与する。
- [ ] Manual.jp.xml: クイックスタート／能力値／耐性・温度／スキル／突然変異／アーティファクト／トニック／武器と防具／戦闘／セーヴィングスロー／アクションコストを追加済み。残るトピックも同じ方針で翻訳する。

## 完了／確認待ち
- [ ] UI 同期後に python3 scripts/sync_mod.py を実行し、ゲーム内で UI が崩れていないか確認する。
- [x] メインメニュー（New Game / Continue / Records / Options / Mods / Redeem Code / Modding Toolkit / Credits / Help）のラベルを Harmony パッチ `MainMenuLocalizationPatch` で日本語化した。左・右両列ともゲーム起動時に反映されるか確認済み。

## メモ
- メインメニューの `LeftOptions` / `RightOptions` は `MainMenu.Show()` 内で `BeforeShow(...)` に渡された直後に表示が描画される。`SetupContext()` 以降では間に合わないため、Harmony パッチは `Show()` プレフィックスで `Text` を書き換える（静的コンストラクターや `SetupContext()` へのパッチでは反映されない）。
- UI の個別要素（例: ジェノタイプ説明など）は ObjectBlueprints 側を参照。ここではウィンドウや説明枠のみ取り扱う。
- Options.jp.xml はファイル丸ごと置き換え未サポートのため、Load="Merge" のまま DisplayText 等の値だけ更新する。Replace="true" を付けると Player.log に Unused attribute 警告が出る。
- Genotypes.jp.xml / Subtypes.jp.xml / Mutations.jp.xml / EmbarkModules.jp.xml は `<genotype>` / `<class>` / `<category>` / `<module>` / `<window>` を **Load="Replace"** にし、不要な `Replace="true"` を使わない。XmlDataHelper が `Replace` 属性を解釈しないため、将来の警告を防げる。
- Mods/QudJP/Docs/manual/ManualPatch.jp.manual（旧 Localization/Manual.jp.xml）は Harmony パッチ専用のデータソース。ゲームの XML ローダーに渡すと Quickstart などが二重登録されてクラッシュするので、Localization/Manual.jp.xml には空スタブを残す。
- メニュー文言は基本的に XML 側（Options / EmbarkModules / Genotypes / Subtypes / Mutations / Commands）で差し替える。Harmony はフォント・レイアウト調整やメニュー構築タイミングの例外対応に限定し、文言上書きが必要な場合もまずは該当 XML（例: `Commands.jp.xml` の `<command>` DisplayText）で翻訳できないか確認する。
- UI 全体の入力ガイド（FrameworkScroller ベースのホットキー欄）は `Commands.jp.xml` を参照する Harmony パッチ（`FrameworkScroller.BeforeShow` prefix）で Description を置き換えている。`navigate / select / quit / back` などは literal フォールバックも用意済み。XML を更新したら全画面へ自動反映される。
- 画面下部のレジェンド（`[Space] select` など）は `KeyMenuOption.Render(string prefix, string text)` に直接渡されるプレーン文字列を描画しているため、`MenuOption.Description` を書き換えるだけでは反映されない。`MenuOptionLegendLocalizer`（辞書を一元管理）＋ `KeyMenuOptionRenderLocalizationPatch` で `text` を書き換え、FrameworkScroller 系パッチよりも後に文字列が更新されるようにしている。Harmony で `Render` をフックする際は引数名 `text` を指定しないと初期化に失敗し、タイトル画面を含む全パッチが適用されないので注意すること。
