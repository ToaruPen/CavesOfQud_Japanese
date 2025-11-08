# references ディレクトリについて

`scripts/extract_base.ps1` を実行するとゲーム本体の `CoQ_Data/StreamingAssets/Base` から必要なファイルがここにコピーされます。  
これらは **配布対象ではなく** 差分確認と翻訳テンプレ作成のための参照用です。

- `Base/` : Conversations / Books / Commands / Corpus などの最新版。
- `Base/ObjectBlueprints/` : 名前生成や固有名詞参照で必要な場合に使用。

ゲームのアップデート後は必ず再抽出してから翻訳作業を行ってください。
