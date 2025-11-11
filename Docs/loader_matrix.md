# Loader / Loader Matrix

まとめ: `references/Base` から取り出したカテゴリと、Mods 側でどう差し込むかを整理する。`Player.log`（`C:\Users\takut\AppData\LocalLow\Freehold Games\CavesOfQud\Player.log`）のロードログと既存ドキュメントの記述（`Docs/translation_process.md` など）を根拠にしている。

## references/Base スナップショット（2025-11-09）

| entry | kind | contents |
| --- | --- | --- |
| `references/Base/Books.xml` | file | 書籍（`<books>`） |
| `references/Base/Commands.xml` | file | 辞書 / コマンドバインド |
| `references/Base/Conversations.xml` | file | NPC 会話ツリー |
| `references/Base/EmbarkModules.xml` | file | キャラクリ UI モジュール |
| `references/Base/Manual.xml` | file | ヘルプトピック |
| `references/Base/Mutations.xml` | file | 突然変異定義 |
| `references/Base/Options.xml` | file | 設定 UI |
| `references/Base/Corpus/` | dir | 3 本の抜粋 TXT (`Machinery`, `Meteorology`, `Thought-Forms`) |
| `references/Base/ObjectBlueprints/` | dir | `Creatures.xml` ほか 13 本のブループリント |

> `Genotypes.xml` / `Subtypes.xml` / `EmbarkModules.xml` はゲーム本体 (`CoQ_Data/StreamingAssets/Base`) に存在するが、現状 `references/Base` には未抽出。`scripts/extract_base.py` を再実行して差分を追えるようにしたい。

## カテゴリ別ローダーと `Replace` 互換性

| category | base data | localization asset | loader / source | merge / replace の扱い | 根拠 |
| --- | --- | --- | --- | --- | --- |
| Books | `references/Base/Books.xml` | `Mods/QudJP/Localization/Books.jp.xml` | `BookManager` が `<books>` を逐次読み込み。`Load="Merge"` が機能し、`<object>` 単位での差し替えのみ。 | `Replace="true"` を付けなくても差分挿入できる（現行 jp ファイルは `Replace` 未使用）。`CDATA` で本文を包む。 | jp ファイルの構造: `Mods/QudJP/Localization/Books.jp.xml:1-13` |
| Corpus TXT | `references/Base/Corpus/*.txt` | 未作成 (`Docs/tasks/books.md`) | `JournalAPI` が TXT を 1:1 読み込む。 | XML ではなく TXT。ファイル名末尾を `.jp.txt` にすることで並列ロード。 | TODO 表記: `Docs/tasks/books.md:5-11` |
| Commands | `references/Base/Commands.xml` | `Mods/QudJP/Localization/Commands.jp.xml` | `ConsoleLib.CommandSystem` が `commands` を一括ロード。 | ルートで `Load="Merge"` 非対応。要素数が多いため `Replace="true"` を不用意に付けると警告が出る。 | 既存 jp ファイルに `Load` も `Replace` も無し (`Mods/QudJP/Localization/Commands.jp.xml:1-40`) |
| Conversations | `references/Base/Conversations.xml` | `Mods/QudJP/Localization/Conversations.jp.xml` | `ConversationFactory`（`Player.log:180-181` の `Loading Conversations.xml`）。 | ルートに `Load="Merge"` を置き、`<conversation>` / `<node>` などで `Replace="true"` が有効。 | `Mods/QudJP/Localization/Conversations.jp.xml:2-34` に `Load="Merge"` と `Replace="true"` が共存し、ログには警告無し (`Player.log:180-181`) |
| EmbarkModules | `references/Base/EmbarkModules.xml` | `Mods/QudJP/Localization/EmbarkModules.jp.xml` | `XmlDataHelper` 経由で `CharacterBuild` モジュールをロード。 | `Docs/translation_process.md:11-15` が示す通り、`Replace="true"` は無視される。差し替えたい `<module>` / `<window>` / `<mode>` に直接 `Load="Replace"` を指定する。現行ファイルは未対応なので修正候補。 | `Docs/translation_process.md:11-15`, `Docs/tasks/ui.md:18-23` |
| Genotypes / Subtypes | `CoQ_Data/.../Genotypes.xml` etc. | `Mods/QudJP/Localization/Genotypes.jp.xml`, `Subtypes.jp.xml` | 同じく `XmlDataHelper`。 | `Replace="true"` を使わず、要素自体に `Load="Replace"` を置く必要あり（まだ未導入）。 | ガイドライン: `Docs/tasks/ui.md:18-23` |
| Manual | `references/Base/Manual.xml` | `Mods/QudJP/Localization/Manual.jp.xml` + Harmony patch (`Mods/QudJP/Docs/manual/ManualPatch.jp.manual`) | ゲーム内ヘルプは `XmlDataHelper` 管理だが、実際の翻訳は Harmony (`ManualLocalizationPatch`). | Loader に渡す XML は空スタブでよい。本文は C# から注入。`Replace` 非対応。 | コメント: `Mods/QudJP/Localization/Manual.jp.xml:2-7` |
| Mutations | `references/Base/Mutations.xml` | `Mods/QudJP/Localization/Mutations.jp.xml` | `XmlDataHelper` (`Player.log:158-159`)。 | `Replace="true"` は警告対象。`<mutation Load="Replace">` 形式で再生成するスクリプトが必要。 | `Docs/translation_process.md:11-15` |
| Options | `references/Base/Options.xml` | `Mods/QudJP/Localization/Options.jp.xml` | `XmlDataHelper` の `OptionsManager`. | ファイル全体の `Load="Merge"` は未サポート。DisplayText など値だけを更新し、`Replace="true"` は使わない（`Player.log` で `Unused attribute "Replace"` が出る）。 | `Docs/tasks/ui.md:15-23` |
| ObjectBlueprints | `references/Base/ObjectBlueprints/*.xml` | `Mods/QudJP/Localization/ObjectBlueprints/*.jp.xml` | `ObjectManager` が `Load="Merge"` を解釈 (`Player.log:158-159` 直後に `Loading object blueprints`). | `<objects Load="Merge">` + `<object ... Replace="true">` のベストプラクティス。LLM パイプライン (`Docs/tasks/objectblueprints_llm.md`) もこの前提。 | 実装例: `Mods/QudJP/Localization/ObjectBlueprints/Items.jp.xml:1-20` |
| Commands / Keymap extras | `references/Base/Commands.xml` ほか | `Mods/QudJP/Localization/Commands.jp.xml` | 同上 | `Replace` 不可。 | 同上 |

### XmlDataHelper ファミリー
- 対象: `Genotypes`, `Subtypes`, `Mutations`, `EmbarkModules`, `Options`, `Manual`, `ManualPatch`.
- `Docs/translation_process.md:11-19` にある通り、要素単位で `Load="Replace"` を指定しないと `Player.log` に `Unused attribute "Replace"` が出る。
- 現在の jp ファイルはまだ `Load="Replace"` を持っていないため、差し替えツールを整備してリライトしたい。
- Harmony で `ModManager.ForEachFile` をフックし、`Mods/QudJP/Localization/*.jp.xml` を `XmlDataHelper.Parse("*.xml", includeMods:true)` に差し挟む。実装: `Mods/QudJP/Assemblies/src/Patches/ModManagerLocalizationFilePatch.cs` ＋ `LocalizationAssetResolver.TryInjectOverride`。
- 併せて `XmlDataHelper.AssertExtraAttributes` をパッチ (`XmlDataHelperLegacyAttributePatch`) し、QudJP の `.jp.xml` では `Load` / `Replace` 属性を自動で既読扱いにして Player.log の `Unused attribute` 警告を抑制する。ロジックは `LocalizationAssetResolver.IgnoreLegacyAttributes`。

### ObjectBlueprints / Conversations
- `Player.log:158-181` の並びを見ると `Mutations.xml` → `object blueprints` → `Conversations.xml` の順で読み込まれており、`Replace="true"` を付けた `Objects` / `Conversations` でも警告は出ていない。
- `Docs/tasks/objectblueprints_llm.md` に沿って LLM 抽出→再挿入 (`scripts/objectblueprint_{extract,insert}.py`) パイプラインを継続。

### Corpus / Books
- 書籍 (`Books.xml`) は XML、Corpus は TXT。`Docs/tasks/books.md` の ToDo を満たすには `.jp.txt` を Mods 側に 1:1 置く必要がある。
- 書籍本文は `<property Name="Text"><![CDATA[...]]]>` で扱う。`Load="Merge"`＋`Replace` 無しで OK。

## 今後のスクリプト／検証計画

1. **XmlDataHelper リライタ**  
   - 目的: `Genotypes/Subtypes/Mutations/EmbarkModules/Options` を解析し、対象要素へ機械的に `Load="Replace"` を付与する。  
   - 実装案: `scripts/xmldatahelper_rewrite.py`（仮）で元 XML を読み込み、`Replace="true"` を削除して `Load="Replace"` を追加→Mod 側に書き戻す。  
   - 連携: 変換後に `python scripts/check_encoding.py --fail-on-issues` を必須実行。

2. **Conversations / Books の抽出＆挿入パイプライン**  
   - `Docs/tasks/objectblueprints_llm.md` をテンプレに、`scripts/conversation_extract.py` / `conversation_insert.py`、`scripts/books_extract.py` / `books_insert.py` を追加する。  
   - 出力フォーマット: `pending_strings` を JSON 化し、LLM へ渡す仕様を再利用。会話ノード ID (`<conversation>/<node/@ID>`) や `{{token|` 構文を維持する。  
   - 挿入時は `<conversation Load="Merge">` を保持したまま `Replace="true"` を付けた `<node>` を再配置。

3. **オンデマンド ロード検証**  
   - `Mods/QudJP/Localization` にテスト用ファイルを作り、`Player.log` で `Unused attribute "Replace"` が出ないか監視する。GUI 起動が難しい場合は `build_log.txt` を Tail し、静的解析だけでも `Load="Replace"` 変換結果を確認する。  
   - 監視コマンドは `Docs/log_watching.md` の `Get-Content -Tail` 手順を利用。

4. **references/Base の継続更新**  
   - `scripts/extract_base.py` を最新ゲームに対して再実行し、`references/Base/Genotypes.xml` など欠けているファイルを補充する。  
   - 補充後はこのマトリクスをアップデートし、カテゴリ別 ToDo (`Docs/tasks/*.md`) とリンクさせる。

## 参考ログ

- `Player.log:125-181` … `Loading Compat.xml`～`Loading Conversations.xml` までのロードシーケンス。`Conversations` で `Load="Merge"` + `Replace="true"` を使っても警告無し。  
- `Docs/translation_process.md:11-19` … `XmlDataHelper` 系の Replace 制限。  
- `Docs/tasks/ui.md:15-23` … Options / Genotypes など UI XML の取り扱い注意。  
- `Docs/tasks/objectblueprints_llm.md` … LLM パイプライン仕様。  
- `Docs/tasks/books.md` … Corpus TXT の未訳タスク。

### Console / TMP Bridge
| category | base data | localization asset | loader / source | merge / replace | 備考 |
| --- | --- | --- | --- | --- | --- |
| Classic Console | Runtime ConsoleLib.Console.TextConsole -> ScreenBuffer | (Harmony) ConsoleBridge | Prefix intercept on TextConsole.DrawBuffer (ConsoleBridgePatch) | n/a (runtime render only) | Mods/QudJP/Assemblies/src/Console/* で ScreenBuffer→TMP に変換。Classic UI (!GameManager.ModernUI) のときだけ 80x25 行を <color> つき文字列に落とし、ConsoleBridgeView が差分描画する。 |
