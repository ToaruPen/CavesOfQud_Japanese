# 書籍 / ロア タスクボード

`Books.jp.xml` と `Corpus/` 配下の長文テキストを扱います。二重チェック推奨。

## 未訳 / 対応中
- [完了 / Codex] `Corpus/Machinery-of-the-Universe-excerpt.txt`（Base 直下）を翻訳し、`Mods/QudJP/Localization/Corpus/Machinery-of-the-Universe-excerpt.jp.txt` を新規作成。
- [完了 / Codex] `Corpus/Meteorology-Weather-Explained-excerpt.txt` を翻訳し、同名 `.jp.txt` を用意。
- [完了 / Codex] `Corpus/Thought-Forms-excerpt.txt` を翻訳し、同名 `.jp.txt` を用意。
- [保留] `Corpus/Corpus/*`（Machinery / Meteorology / Thought-Forms の派生3本）も同様に `.jp.txt` を作成。※現行の StreamingAssets/Base には当該派生ファイルが存在しないため原文待ち。
- [完了 / Charlie] Books.jp.xml に詩（FearinBeyLah）・説話（LoveinBeyLah）・手紙（DagashasSpur）のフォーマットサンプルと CyberIntro / AlchemistMutterings / Quotes / Skybear / MimicandMadpole / TeleporterOrbs / Sonnet / CrimeandPunishment / AphorismsAboutBirds / DarkCalculus / TornGraphPaper / Animals / EntropytoHierarchy / EntropytoHierarchy2 / DisquisitionOnTheMaladyOfTheMimic / Lives1 / Across1 / Across2 / Across3 / Canticles3 を追加。
- [完了 / Charlie] Mechanimist 系（Klanq／TemplarDomesticant／Preacher1～4／HighSermon）を翻訳済み。
- [完了 / Alpha] Books.jp.xml に `CrimeandPunishment` / `AphorismsAboutBirds` / `BloodstainedSheaf` / `Sheaf1` / `Animals` / `EntropytoHierarchy` / `Across1` / `Across2` / `Across3` を追加（短文・箴言・散文カテゴリ）。
  - [完了 / Charlie] Base 由来の 19 件（RuinOfHouseIsner / Stopsvalinn / AmaranthinePrism / BloodandFear / TheArtlessBeauty / FaunsoftheMeadow / EtaandtheEarthling1 / CouncilAtGammaRock / VaamsLens / KahsLoop / NachamsRibbon / TheRecitation / MurmursPrayer / InMaqqomYd / GolemOperatingManual / ModuloMoonStair / HistoryofJoppaVol1 / HistoryofJoppaVol2 / EndCredits）を `Mods/QudJP/Localization/Books.jp.xml` で確認・整備し、`py -3 scripts/diff_localization.py --missing-only --base Books.xml`（2025-11-09 実行）で未訳ゼロを確認済み。

## 校正予定
- [ ] 書籍カテゴリごとに語彙統一ルールを `Docs/glossary.csv` に追記。

## メモ
- 大量のテキストを扱う際は CAT ツールを使っても良いが、UTF-8 / LF を崩さないこと。
