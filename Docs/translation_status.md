# 翻訳進捗表

| ファイル / カテゴリ | 状態 | 備考 |
| --- | --- | --- |
| Conversations | 進行中 | 2025-11-12: 監視官／工匠／クリフォト翻訳を整え、Chrome 系の中黒ルールと Rhinox 説明の英語片を処理。Conversations.jp.xml で Joppa～Barathrumites／Pax Klanq／Thah／Kith & Kin などを反映済み。JoppaFarmerConvert／WatervineFarmerConvert／CannibalConvert／IssachariConvert／PigFarmerConvert／Mehmet／Tam／DromadTrader／ConsortiumGlowpad／WardenYrame／Nima Ruda／StarappleFarmer／PigFarmer／CrabFarmer／LeechFarmer／CatHerder／AmoebaFarmer／SnailFarmer／GoatHerder／BeetleFarmer／SapientBear／Cannibal／WardenEsthers／MechanimistPilgrim／MechanimistLibrarian／Tszappur／GenericMerchant／ookbinder／scribe／eekeeper／	inker／schematics drafter／pothecary／herbalist／chef／kipper／
intner／ichormerchant／gutsmonger／hatter／shoemaker／haberdasher／rmorer／glover／gunsmith／grenadier／gemcutter／jeweler／Q Girl／Barathrum PreKlanq～PaxKlanq2・TombIntro～TombExplain3／Sparafucile／Iseppa／Shem／OmonporchBarathrumites／BethesdaBaetyl／DefaultTrader／Crowsong／Yurl／Nuntu／Pax Klanq（TextPax 含む）／Otho Signal～Ripe2／BeginGraveTidings～BeginCallToArms／Gritgate Mainframe Intercom／Barathrum Early～Brightsheol1／ImperialBiographer／Barathrumites ブリーフィング・Omonporch・Chavvah ノード も追加済みで、残りのイベントは diff レポートで随時確認。2025-11-10: Bravo が Call to Arms 連鎖／Q Girl／Barathrum 入口／Imperial Biographer／Omonporch Barathrumites／Chavvah 約束ノード／Irudad／Mehmet／Tam／DromadTrader／ConsortiumGlowpad／Warden Yrame／Nima Ruda／Starapple Farmer／Joppa Convert 系を追加入力し、「ジョッパ」表記も「ヨッパ」に統一。2025-11-11: Barathrum（Early～AscendConfirm3／TombBody1～3／TombExplain1～3／PaxKlanq2～Accept／Golem 連鎖／Ascend／GolemInfo／Questions）、Sparafucile、Iseppa、Shem、オモンポーチ派バラサラム／ベセスダのベテル／DefaultTrader UI／Crowsong／Yurl／Nuntu／Pax Klanq を `Replace="true"` で再投入し、墓所帰還～We Are Starfreight／オモンポーチ待機組／ベテル操作／UI プロンプト／キャクキャ・パックス関連導線を同期。2025-11-11 PM: Pax Klanq（BuildGolem／GolemComponentQuestions／GolemOtherQuestions／Barathrum[s]Study）／Warden Indrix／Wild Water Merchant／SusaAlchemist／Asphodel（Arrived／Chaos／Done／Waiting）／PhinaeHoshaiah を追加し、Woodsprog～Crystals のテンプレ会話を `Replace="true"` で日本語化して diff レポートを更新。同 PM: Hindriarch Keh（Start～MocksFate／KindrishReturnBefore～After／SlynthRequest 系）を `Replace="true"` で追加し、Kith and Kin 入口～Love and Fear 解決後の会話と KindrishReturn 分岐を日本語化。差分再実行で、残タスクが Neelahind／Eskhind／Clue_* ノードに集約されたことを確認。2025-11-12: BaseConversation 汎用選択肢／ManyEyes／Thah 動的候補／SlynthSettler・Wanderer／Chavvah/Tau 系（Dreamer／Thicksalt／Tammuz／TauNoLonger／WanderingTau／TauCompanion／TauSoft）／Santalalotze／Nephilim／GyreWight* を `Replace="true"` で訳出し、diff_missing を解消。 |
intner／ichormerchant／gutsmonger／hatter／shoemaker／haberdasher／rmorer も追加済みで、残りのイベントは diff レポートで随時確認。 |
intner／ichormerchant／gutsmonger も追加済みで、残りのイベントは diff レポートで随時確認。 |
| Books | 進行中 | `Books.jp.xml` へベイ・ラー詩篇＋手紙に加え、Mechanimist 系（Klanq／TemplarDomesticant／Preacher1～4／HighSermon）と CyberIntro／AlchemistMutterings／Quotes／Skybear／MimicandMadpole／TeleporterOrbs／Sonnet／CrimeandPunishment／AphorismsAboutBirds／DarkCalculus／TornGraphPaper／Animals／EntropytoHierarchy／EntropytoHierarchy2／DisquisitionOnTheMaladyOfTheMimic／Lives1／Across1／Across2／Across3／Canticles3 を反映。さらに Base 由来の 19 件（RuinOfHouseIsner ～ EndCredits）も 2025-11-09 時点で確認済みで、`py -3 scripts/diff_localization.py --missing-only --base Books.xml` 実行結果は未訳なし。`Corpus/` 本編は未訳なので Docs/tasks/books.md を参照。2025-11-11: Wiki検証に沿って Kasaphescence／Ol' Uri／crimson swift／firmament／Oh の表記を整え、The Artless Beauty の「湯」→「水」を修正、"Disquisition..." タイトルへ「ミミック病論」を追記。 |
| Commands | 進行中 | `Commands.jp.xml` を `Replace="true"` 方針で更新。UI 表記との整合レビュー中。 |
| EmbarkModules | 完了 | `EmbarkModules.jp.xml` を Load="Replace" で再有効化し、各モード／カテゴリ UI を最新ベースに同期。 |
| Genotypes / Subtypes | 進行中 | `Genotypes.jp.xml` / `Subtypes.jp.xml` で名称訳は完了。選択画面の幅調整と追加派生に注意。 |
| Mutations | 進行中 | `Mutations.jp.xml` に Morphotypes / Physical / PhysicalDefects / Mental / MentalDefects を実装済み。残りカテゴリは未着手。 |
| Options | 進行中 | `Options.jp.xml` で全カテゴリの DisplayText を訳出済み。細部 UI テスト待ち。 |
| ObjectBlueprints | 進行中 | `Mods/QudJP/Localization/ObjectBlueprints/*.jp.xml` をカテゴリ単位で整備中。2025-11-12: Items.jp.xml の Batch15～18 を `work/items_missing_batch18.json`／`_translated_batch18.json` から取り込み、Laser Rifle／Force Modulator／Bionic Hand など複合語の DisplayName に中黒 `・` を再配置。Love／Blaze／Hulk honey／Salve／Sphynx salt／Shade oil／Ubernostrum／Rubbergum／Nectar トニックの説明も日本語化し、同日追加で BaseTierBack/Arm/Floating Tier7-8 テンプレと全コープス系／DataDisk／Phial／StorageTank／Gourd／Entropy Cyst／Magnetic Bottle／Fungal Infection／Security Card などテンプレを訳出して diff_missing を解消 (`py -3 scripts/diff_localization.py --missing-only` = 0)。Creatures.jp.xml は Batch28 まで挿入済みで、Mopango／Mechanimist／Templar／Gyre Wight 系など Tier 別に進行。Foods／Furniture／HiddenObjects／PhysicalPhenomena／Widgets／WorldTerrain／ZoneTerrain も Load="Merge" で差分反映し、カテゴリごとに `py -3 scripts/diff_localization.py --missing-only --base ObjectBlueprints/*` を都度実行して漏れをチェック。|
| Corpus (Lore) | 進行中 | `Machinery-of-the-Universe-excerpt.jp.txt` / `Meteorology-Weather-Explained-excerpt.jp.txt` / `Thought-Forms-excerpt.jp.txt` を追加。残りの `Corpus/*.jp.txt` は Docs/tasks/books.md を参照。 |
| Grammar / Population / Harmony | 未着手 | Harmony フック側で翻訳テーブルを組み込む予定。 |

状態ラベルの目安: `未着手` / `進行中` / `レビュー中` / `完了`。
| Classic UI Bridge | 新規 | ConsoleBridgePatch + ConsoleBridge/View で Classic UI の 80x25 ScreenBuffer を TMP へ橋渡し。文字化け調査・CP437→Unicode 変換・色タグ化は実装済み。背景 <mark> と Tile の扱いは TODO。 |

## 更新履歴
- 2025-11-11: Conversations／Books／ObjectBlueprints の派閥・地名を `Docs/glossary.csv` の Short 列ルールに沿って一括更新（例：六日のスティルト→スティルト、メカニマス教団→メカニマス教、喰らう者たちの墓所→Eatersの墓所など）。`py -3 scripts/diff_localization.py` でフォーマット確認済みで、次カテゴリ作業へ進む準備完了。
- 2025-11-11: Conversations に Barathrum（Early～AscendConfirm3／TombIntro～Accept／Golem 系ノード）を再投入し、Meyvn 昇格・墓所案内・Golem Quest・We Are Starfreight 開幕までの導線を `Replace="true"` で同期。
- 2025-11-11: Conversations に Sparafucile（SilentGreeting～LiveDrinkSign）を追加し、手話ベースの案内・各種依頼導線・ジェスチャーレスポンスを `Replace="true"` で反映。
- 2025-11-11: Conversations に Iseppa（Start／Recame／Greetings／Bethesda／Omonporch／Project 系）を追加し、ベセスダ・オモンポーチ案内や睡眠／悪夢／哲学トークを `Replace="true"` で翻訳。
- 2025-11-11: Conversations に Shem（Start／YouAreSafe／-1／Friend ほか）を追加し、クロムリングの自己紹介・ダウングレード談義・クランク考察を `Replace="true"` で翻訳。
- 2025-11-10: Conversations に Otho（Signal～Ripe2／BeginGraveTidings／CompleteSpindle／IntroduceBarathrum／BarathrumHaveKey）と Q Girl（Start／Rumbling／Quetzal／Hair／Goggles／Tinker／Disk／Climber）を追加し、Replace="true" で構造を保ったまま訳文を投入。UTF-8/LF を維持し、特殊タグ（&amp;mQ Girl&amp;y／&amp;Wbaetyl&amp;y／変数 `=factionaddress:…=` 等）を保持。
- 2025-11-10: Conversations に Dardi／Aloysius／Hortensa／Shem（Shem -1）系の会話ノードを追加（Replace="true"）し、プレースホルダと emote 記法を維持。
- 2025-11-10: Conversations に Jacobo／Neek／Gritgate Intercom／Mafeo を追加（Replace="true"）。語彙（カサフェセンス等）は既存訳に合わせ、Q Girl 表記を統一。
- 2025-11-11: Conversations の Jacobo／Neek／Gritgate Intercom／Gritgate Mainframe Intercom を最新ベースに合わせて再投入し、UTF-8 での置換と diff レポート更新を実施。
- 2025-11-11: Conversations に JoppaFarmerConvert／WatervineFarmerConvert／CannibalConvert／IssachariConvert／PigFarmerConvert を追加し、Convert 系クエストの派生ノード（Bauble／WhyNot／Stilted ほか）を `Replace="true"` で翻訳。
- 2025-11-11: Conversations に Irudad を追加し、Red Rock 報告／ジャイア説明／Slynth 受け入れ分岐など Base 由来の全ノードを `Replace="true"` で差し替えた。
- 2025-11-11: Conversations に Mehmet を追加し、依頼導線と Red Rock 報告トピックを `Replace="true"` で反映。
- 2025-11-11: Conversations に Tam を追加し、自己紹介・ジョッパ定住・ドロマド種族紹介ノードを `Replace="true"` で翻訳。
- 2025-11-11: Conversations に DromadTrader を追加し、挨拶と種族紹介テンプレを `Replace="true"` で統一。
- 2025-11-11: Conversations に ConsortiumGlowpad を追加し、コンソーシアム商人の導線を `Replace="true"` で翻訳。
- 2025-11-11: Conversations に WardenYrame を追加し、警備導線（Rustwells／Red Rock／Grit Gate／Stilt）と自己紹介ノードを `Replace="true"` で反映。
- 2025-11-11: Conversations に Nima Ruda を追加し、薬師紹介・商品案内・出自と継承ノードを `Replace="true"` で翻訳。
- 2025-11-11: Conversations に StarappleFarmer を追加し、行商風の挨拶・商品勧誘テキストを `Replace="true"` で翻訳。
- 2025-11-11: Conversations に PigFarmer を追加し、豚農家の勧誘テキストを `Replace="true"` で翻訳。
- 2025-11-11: Conversations に CrabFarmer を追加し、カニ飼育者の販売台詞を `Replace="true"` で反映。
- 2025-11-11: Conversations に LeechFarmer を追加し、ヒル農家の会話テンプレを `Replace="true"` で翻訳。
- 2025-11-11: Conversations に CatHerder を追加し、猫飼いの挨拶・販売台詞を `Replace="true"` で翻訳。
- 2025-11-11: Conversations に AmoebaFarmer を追加し、アメーバ農家の勧誘テキストを `Replace="true"` で翻訳。
- 2025-11-11: Conversations に SnailFarmer を追加し、螺旋殻談義を含む台詞セットを `Replace="true"` で翻訳。
- 2025-11-11: Conversations に GoatHerder／BeetleFarmer／SapientBear／Cannibal／Arconaut／Snapjaw／Issachari／MechanimistPilgrim／MechanimistLibrarian／Tszappur／bookbinder～jeweler を `Replace="true"` で追加し、Stilt の行商テンプレと巡礼・機械教ノードを同期。

## 更新履歴（Conversations）
- 2025-11-11: Mopango 監視者（Nacham / Dagasha / Vaam / Kah）＋ Rainwater Shomer を Replace=\"true\" で追加。特殊置換（=ifplayerplural:…=, =player.formalAddressTerm=, {{emote|…}} 等）を保持し、差分は py -3 scripts/diff_localization.py --missing-only --json-path Docs/backlog/conversations.json で確認済み。
- 2025-11-11 PM2: Mopango 監視関連の見張り（Dadogom / Gyamyo / Yona）を Replace=\"true\" で追加。diff_localization 再実行で未訳から消失を確認。次は Imperial Biographer 系（ノード: BiographerWho / Mark / Brightsheol / Unfinished / SultanGone ほか）を予定。
- 2025-11-11 PM3: Imperial Biographer 追加完了。diff_localization で未訳から消失を確認。次は Mopango 創建者系（Goek / Mak / Geeub と各分岐）を予定。
- 2025-11-11 PM3.5: Goek / Mak / Geeub を追加完了。以降の未訳は Krka / Bep / Une / Tilli / Rokhas / Many-Eyes / Thah など Freehold 周辺の会話群。順次対応します。
- 2025-11-11 PM4: Freehold クラスタ（Goek/OtherPlagues, Bep, Krka, Une, Rokhas, ManyEyes, Thah）を追加。次は Tilli 系（Tillifergaewicz ほか）と Thah の Landing Pads 動的ノード（SlynthDecision など）を予定。
- 2025-11-11 PM4.5: Thah の動的分岐まで訳出。残は Chavvah 会話群（Chavvah / ChavvahPrime / Chime / Physiology* / Tau* など）とダイナミック村長テンプレ。次のバッチで対応予定。
- 2025-11-11 PM5: ChavvahPrime / Tzedech / Tikva / Miryam / DynamicVillageMayor / ChavvahFrontChime / TauChime を追加。次は汎用ダイナミック（AskForWork / DirectToWork / MoreToAsk / AskName / Goodbye など）と Chavvah の残り分岐（WelcomeHub / Twofirm 由来の詳細、Tau* 派生）を予定。
