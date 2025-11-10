# 会話タスクボード

> 担当: Codex（ワークストリームB – GritGateHandler / CallToArmsResult / PaxKlanq / KithAndKin / Thah ほか）

Conversations.jp.xml（NPC 会話）と関連トリガー（ログ参照、Argyve 以外の導線など）を管理します。

2025-11-10（Bravo）: Otho（Signal～Ripe2／BeginGraveTidings／CompleteSpindle／IntroduceBarathrum／BarathrumHaveKey）と Q Girl（一式）を追加・翻訳（Replace="true" でノード差し替え、特殊置換維持）。
2025-11-11（Bravo）: Barathrum（Early／RecameMe／PreKlanq／PreJunk／Brightsheol1～2／TombReward～Accept／TombIntro～TombExplain3／Golem Preface～AscendConfirm3／GolemInfo／Questions）を追加・翻訳し、Meyvn 昇格～The Golem～We Are Starfreight までの導線を `Replace="true"` で再同期。
2025-11-11（Bravo）: Sparafucile（SilentGreeting～LiveDrinkSign）を追加・翻訳し、ジェスチャーベースの応答・依頼導線・クエスト分岐を `Replace="true"` で再同期。
2025-11-11（Bravo）: Iseppa（Start／Recame／Greetings／Bethesda／Omonporch／Project ほか）を追加・翻訳し、ベセスダ／オモンポーチ／クランク／睡眠・悪夢系ノードを `Replace="true"` で再同期。
2025-11-11（Bravo）: Shem（Start／YouAreSafe／-1／Friend ほか）を追加・翻訳し、クロムリングの自己紹介・ダウングレード談義・クランク考察を `Replace="true"` で再同期。
2025-11-11（Bravo）: OmonporchBarathrumites／BethesdaBaetyl／DefaultTrader を `Replace="true"` で追加し、オモンポーチ待機組・ベテル挿入プロンプト・デフォルト取引 UI を同期。
2025-11-11（Bravo）: Crowsong（Welcome／Dots／Who／Hiding／Kyakukya／Swords）を追加・翻訳し、キャクキャの詩的盗賊の応答を `Replace="true"` で同期。
2025-11-11（Bravo）: Yurl（Welcome／Story／Consortium／Fungi／Asphodel／Lease）を追加・翻訳し、コンソーシアム書記の営業トークと伯爵情報を `Replace="true"` で同期。
2025-11-11（Bravo）: Nuntu（Welcome／Village／Kyakukya／Slynth 系）を追加・翻訳し、キャクキャ村長の紹介とスリンセ受け入れ分岐を `Replace="true"` で同期。
2025-11-11（Bravo）: Pax Klanq（TextPax／Welcome～Quest6）を追加・翻訳し、Spread Pax クエスト導線とパフ交渉の会話を `Replace="true"` で同期。
2025-11-11（Bravo）: Pax Klanq（BuildGolem／GolemComponentQuestions／GolemOtherQuestions／Barathrum(s)Study）を追加し、ゴーレム素材案内を `Replace="true"` で同期。Warden Indrix／Wild Water Merchant／SusaAlchemist／Asphodel（Arrived／Chaos／Done／Waiting）／PhinaeHoshaiah を `Replace="true"` で翻訳し、Woodsprog～Crystals までの種族・動物テンプレも一括で差し替え、`py -3 scripts/diff_localization.py --missing-only --json-path Docs/backlog/conversations.json` を再実行。
2025-11-11（Bravo）: Hindriarch Keh（Start／Yes. Go on.／Accept／KindrishReturnBefore～After／Kehstions／MocksFate ほか）を `Replace="true"` で追加し、Kith and Kin クエスト入口／Love and Fear 解決後の会話を日本語化。Slynth 受け入れノードも含めて出入り条件を再現し、diff を更新した結果、残タスクは Neelahind／Eskhind／Clue_* 連鎖に集約。
2025-11-12（Bravo）: Tinker 系（Naphtaali / Human ほか）の表示名を「工匠」で統一し、監視官称号・クリフォト表記・Chrome 系中黒ルールを Creatures.jp / Conversations.jp / Books.jp / Subtypes.jp / Docs/glossary.csv / Docs/translation_process.md に反映。Rhinox 説明の英語片も訳語化し、`py -3 scripts/diff_localization.py --missing-only --base Conversations.xml` で欠落なしを確認。

## 未訳 / 対応中
- [ ] Neelahind／Eskhind／Kith and Kin 調査ノードを `Replace="true"` で翻訳し、Distant／Direct／Ruminating／Investigation／Clue_*／KithAndKinFinale などの導線とクエスト分岐（Fate／Doomed／BetterLate 系）を一括同期する。
- [x] Keh（Kith and Kin / Love and Fear 導線）を `Replace="true"` で翻訳し、Yes. Go on. ～ MocksFate と KindrishReturn 分岐・質問セットを再同期。
- [x] WardenEsthers（スティルト守護者）を翻訳し、Welcome / Praise / Huh / Folks / True / Bazaar ノードを日本語化。
- [x] MechanimistPilgrim（巡礼者汎用挨拶）を翻訳し、開始ノードを差し替え。
- [x] MechanimistLibrarian（大聖堂司書）を翻訳し、寄贈受付とスリンセ受け入れノードを日本語化。（2025-11-10 / Bravo）
- [x] Tszappur（レスェフ祠の司祭）を翻訳し、Resheph / Inside / Secret ノードと寄進応答を日本語化。（2025-11-10 / Bravo）
- [x] GenericMerchant（天幕商人のベース会話）を翻訳し、商品セリフと別れの挨拶を日本語化。
- [x] Barathrum（PreKlanq～PaxKlanq2／TombIntro～TombExplain3）を翻訳し、メイヴン昇格～墓所案内・Pax Klanq 捜索・ゴーレム計画・スピンドル上昇の導線を日本語化。（2025-11-11 / Bravo）
- [x] Sparafucile（聴覚障害を抱える銃工）を翻訳し、手話ベースの応答・各種相談ノードを日本語化。（2025-11-11 / Bravo）
- [x] Iseppa（グリットゲートの賢者）を翻訳し、Start／Recame／Greetings／Project 系など全ノードを日本語化。（2025-11-11 / Bravo）
- [x] Shem（クロムリング）を翻訳し、Start／Shem-1／Friend／Klanq／Speak ノードを `Replace="true"` で日本語化。（2025-11-11 / Bravo）
- [x] ookbinder（製本屋）を翻訳し、糊や取引用セリフを日本語化。
- [x] scribe（書記）を翻訳し、写本づくりの売り文句を差し替え。
- [x] eekeeper（養蜂家）を翻訳し、蜂や蜂蜜のセリフを日本語化。
- [x] 	inker（修理職人）を翻訳し、機械修理メニューの案内を差し替え。
- [x] schematics drafter（設計図描き）を翻訳し、設計図販売のセリフを日本語化。
- [x] pothecary（薬師）を翻訳し、疾患や菌対策の売り込みを差し替え。
- [x] herbalist（村の薬草師）を翻訳し、旅人向けの養生アドバイスを追記。
- [x] chef（料理人）を翻訳し、レシピ売りの勧誘を日本語化。
- [x] kipper（保存食売り）を翻訳し、塩漬け糧食のセリフを差し替え。
- [x] intner（葡萄酒商）を翻訳し、ワインの売り口上を日本語化。
- [x] ichormerchant（液体商）を翻訳し、液体愛に満ちた語りを和訳。
- [x] gutsmonger（内臓商）を翻訳し、真きん族向け／一般向けノードを差し替え。
- [x] hatter（帽子屋）を翻訳し、頭部装備の売り文句を日本語化。
- [x] shoemaker（靴職人）を翻訳し、足回りの冗談交じりの接客を日本語化。
- [x] haberdasher（洋品店）を翻訳し、衣装コーデの丁寧語を反映。
- [x] rmorer（甲冑職人）を翻訳し、素っ気ない接客のセリフを追加。
- [x] glover（手袋職人）を翻訳し、指を守るセリフを日本語化。
- [x] gunsmith（銃工）を翻訳し、火薬の匂いを愛でるセリフを和訳。
- [x] grenadier（榴弾商）を翻訳し、爆発物の注意書きを日本語化。
- [x] gemcutter（宝石細工師）を翻訳し、宝石談義のセリフを差し替え。
- [x] jeweler（宝飾職人）を翻訳し、貴金属の売り込みを日本語化。
- [x] Irudad（Joppa／Qud／Work 系、Girshling 報告・Gyre 解説）を日本語化。
- [x] Otho の Signal～Ripe2 ノードを翻訳し、ベイテル／オモンポーチ関連の指示を更新。（2025-11-10 / Bravo）
- [x] Otho の BeginGraveTidings／BeginCallToArms と Gritgate Mainframe Intercom を翻訳し、Call to Arms 連鎖の導線を日本語化。（2025-11-10 / Bravo）
- [x] Q Girl（工房のティンカー）を翻訳し、挨拶／rumbling／quetzal／髪・ゴーグル・ディスク・クライマー各ノードを日本語化。（2025-11-10 / Bravo）
- [x] Barathrum（Early／Recame／RecameMe／Brightsheol1）を翻訳し、墓所帰還後の会話導線を整備。（2025-11-10 / Bravo）
- [x] ImperialBiographer（Herododicus）会話を翻訳し、Brightsheol 質問・刻印・埋葬手順ノードを網羅。（2025-11-10 / Bravo）
- [x] Barathrumites ブリーフィング（OtherBarathrumites～Ereshkigal）と Omonporch Barathrumites／Barathrum's Study／Chavvah 約束ノードを日本語化。（2025-11-10 / Bravo）
- [x] Mehmet（ウォーター・ヴァイン農家）を翻訳し、村紹介／依頼導線／Red Rock 報告ノードを日本語化。（2025-11-10 / Bravo）
- [x] Tam（ドロマッド住民）を翻訳し、自己紹介／ヨッパ定住経緯／種族説明ノードを日本語化。（2025-11-10 / Bravo）
- [x] DromadTrader（ドロマッド行商）を翻訳し、挨拶・種族紹介ノードを差し替え。（2025-11-10 / Bravo）
- [x] Irudad 会話（Welcome～SlynthSettled）を翻訳し、Red Rock 報告／Gyre 解説／巡礼依頼ノードを網羅。（2025-11-10 / Bravo）
- [x] JoppaFarmerConvert／WatervineFarmerConvert／PigFarmerConvert を翻訳し、バザール奉納依頼と報告分岐を日本語化。（2025-11-10 / Bravo）
- [x] スティルト／ヨッパの行商セット（bookbinder～jeweler）を翻訳し、台詞テンプレと `SimpleGeneric` 応答を日本語化。（2025-11-10 / Bravo）

## レビュー待ち
- [x] 追加した会話ノードが Docs/log_watching.md の手順で Missing Node を出さないか確認。→ diff スクリプト再実行＆ Player.log 確認済み（既知の ThreadAbort ログのみ）。

## メモ
- 差分確認時は python3 scripts/diff_localization.py --missing-only --json-path Docs/backlog/conversations.json を利用する（必要に応じて再生成）。

### 追加反映（2025-11-10 / Bravo）
- Dardi／Aloysius／Hortensa／Shem の各会話を `Mods/QudJP/Localization/Conversations.jp.xml` に追加入力（Replace="true"）。
- `=name=` や `=pronouns.*=`、`=player.*=` の各プレースホルダ、`{{emote|...}}` はベースどおり維持。
- `py -3 scripts/diff_localization.py --missing-only --json-path Docs/backlog/conversations.json` で未訳ノードを再確認予定。

### 追加反映（2025-11-10 / Bravo その2）
- Jacobo／Neek／Gritgate Intercom／Mafeo を追加（Replace="true"）。
- Jacobo は Kasaphescence／Mechanimist／Songs／Orb／Machine／GritGate など問い合わせ分岐を含む。
- Gritgate Intercom は入場/受入判定（GritGateHandler）とクエスト進行に合わせた分岐を反映。

### 追加反映（2025-11-11 / Bravo）
- Jacobo／Neek／Gritgate Intercom／Gritgate Mainframe Intercom を `Replace="true"` で再投入し、Base の段落／`=name=`／`{{emote|…}}`／`&amp;B` 記法／`GritGateHandler` `Door` 属性など特殊タグを保持。
- Gritgate Intercom 系は `[Place …]` 系のトレイ指示とクエスト制御（Stamped/Scratched/Argyve ディスク、Merchant's Token、More Than a Willing Spirit）を最新ベースどおりに同期。
- diff スクリプト（`py -3 scripts/diff_localization.py --missing-only --json-path Docs/backlog/conversations.json`）を再実行して Missing から外れたことを確認予定。

### 追加反映（2025-11-11 / Bravo その2）
- JoppaFarmerConvert を丸ごと翻訳し、SpeakNoMore／Stilted／Bauble／WhyNot など 20 ノード超を `Replace="true"` で追加。`=pronouns.*=` や `ReceiveItem` テーブル指定を維持。
- Watervine／Cannibal／Issachari／Pig farmer 各 Convert 会話も Base と同じ継承構造で訳出し、`FindTrinket`／`KilledInDesert` の `Load="Replace"` パッチを日本語化。
- convert 系クエスト（O Glorious Shekhinah!）で使われるトリガー／`SetBooleanState` を保持しつつ、diff スクリプトで Missing から除外されたことを確認予定。

### 追加反映（2025-11-11 / Bravo その3）
- Irudad（BaseSlynthMayor 継承）を全ノード差し替え。`Welcome`／Red Rock 報告連鎖／Gyre 説明／Slynth 受け入れ分岐まで `Replace="true"` で翻訳し、`{{emote|*...*}}`・`=player.FormalAddressTerm=` などケースを忠実に維持。
- Girshling 分岐の `IfHaveState`（RedrockGyreWight*）や O Glorious Shekhinah!／Canticle 連携トピックをベース通りに残し、Slynth 定住イベントの応答も日本語化。
- diff スクリプトを再実行して Irudad 系ノードが未訳リストから消えたことを確認。

### 追加反映（2025-11-11 / Bravo その4）
- Mehmet 会話を `Replace="true"` で追加。Red Rock 依頼導線／村紹介／`What's Eating the Watervine?` クエスト開始／ギルシュリング死骸の報告分岐を日本語化。
- `choice ID="MehmetIntroduce"` の `Load="Remove"`／`SetBooleanState`、クエスト条件や `Priority` を保持して diff から外れることを確認予定。

### 追加反映（2025-11-11 / Bravo その5）
- Tam 会話を `Replace="true"` で追加し、自己紹介／ジョッパ定住理由／ドロマド紹介ノードを日本語化。`=player.apparentSpecies=`・`=player.formalAddressTerm=` を維持。

### 追加反映（2025-11-11 / Bravo その6）
- DromadTrader を `Replace="true"` で追加し、挨拶と種族紹介ノードを日本語化（簡易テンプレは Tam 版に合わせて統一）。

### 追加反映（2025-11-11 / Bravo その7）
- ConsortiumGlowpad の同行商ノードを `Replace="true"` で追加し、囁きセリフと取り扱い案内を日本語化。

### 追加反映（2025-11-11 / Bravo その8）
- WardenYrame を `Replace="true"` で追加し、挨拶バリエーション／Red Rock・Rust Wells・Grit Gate・Stilt の導線、および自己紹介・職務説明ノードを翻訳。

### 追加反映（2025-11-11 / Bravo その9）
- Nima Ruda を `Replace="true"` で追加し、薬師自己紹介／商品案内／出自とリーダーシップ談義ノードを日本語化。

### 追加反映（2025-11-11 / Bravo その10）
- StarappleFarmer を `Replace="true"` で追加し、スタ―アップル売りの勧誘台詞を日本語化（`=player.formalAddressTerm=` を保持）。

### 追加反映（2025-11-11 / Bravo その11）
- PigFarmer 会話を `Replace="true"` で追加し、豚農家の挨拶／注意喚起／販売口上を日本語化。

### 追加反映（2025-11-11 / Bravo その12）
- CrabFarmer を `Replace="true"` で追加し、カニ飼育者の勧誘台詞を日本語化。

### 追加反映（2025-11-11 / Bravo その13）
- LeechFarmer を `Replace="true"` で追加し、ヒル農家の販売トーク・注意喚起を日本語化。

### 追加反映（2025-11-11 / Bravo その14）
- CatHerder を `Replace="true"` で追加し、猫飼いの勧誘台詞／注意文を日本語化。

### 追加反映（2025-11-11 / Bravo その15）
- AmoebaFarmer を `Replace="true"` で追加し、アメーバ農家の勧誘／注意／ユーモア台詞を日本語化。

### 追加反映（2025-11-11 / Bravo その16）
- SnailFarmer を `Replace="true"` で追加し、螺旋殻・ジャイア言及を含む台詞セットを日本語化。

### 追加反映（2025-11-11 / Bravo その17）
- GoatHerder／BeetleFarmer／SapientBear／Cannibal／Arconaut／Snapjaw／Issachari／MechanimistPilgrim を `Replace="true"` で追加し、遊牧系 NPC の挨拶テンプレと口調バリエーションを日本語化。
- MechanimistLibrarian と Tszappur を `Replace="true"` で追加し、Slynth 受け入れ分岐・図書館案内・Resheph 祠の説明や寄進応答を翻訳、`GiveReshephSecret` などの choice ID／`LibrarianGiveBook` パートを維持。
- Stilt 商人テンプレ（bookbinder～jeweler）と gutsmonger True Kin 分岐を一括で `Replace="true"` 追加し、`=player.*=` プレースホルダ・`IfTrueKin` 条件を保持したまま売り文句と注意書きを日本語化。
- `py -3 scripts/diff_localization.py --missing-only --json-path Docs/backlog/conversations.json` を再実行し、対象ノードが Missing 一覧から外れたことを確認。
- 2025-11-11: Mopango 監視者（Nacham / Dagasha / Vaam / Kah）と Rainwater Shomer（Brightsheol ゲート）を Replace=\"true\" で Mods/QudJP/Localization/Conversations.jp.xml に追加。特殊置換（=ifplayerplural:…=, =player.formalAddressTerm=, {{emote|…}} 等）を保持。py -3 scripts/diff_localization.py --missing-only --json-path Docs/backlog/conversations.json で差分確認済み。
- 2025-11-11 PM2: Mopango 見張り（Dadogom / Gyamyo / Yona）を Replace=\"true\" で Mods/QudJP/Localization/Conversations.jp.xml に追加。Repulsive Device 交信ノード（Device / Commune / Deny）・信条・自己紹介分岐を含め、特殊置換（=ifplayerplural:…=, =name=, =factionaddress:Mopango|capitalize=, {{emote|…}}）を保持。
- 2025-11-11 PM3: Imperial Biographer（Herododicus）一連を Replace=\"true\" で追加（Start/Who/SultanGone/Mark/Mark2/Brightsheol/OnlyWay/BearTheMark/EntombMe*/DoThat* 系含む）。語彙を既存訳に合わせて『巻かれた仔羊』『薄界』『ブライトシェオル』『レシェフ』『モロク』等を採用。
- 2025-11-11 PM3.5: Mopango 創建者（Goek / Mak / Geeub）一連を Replace=\"true\" で追加。Slynth 受け入れ分岐、体躯・パイプ・Freehold の話題、笑い・問答のノード、創建回想（Founder1～3）を反映。diff_localization 再実行で founders 系は解消、次は Krka / Bep / Une など周辺住民とサブ話題（Freehold2 / OtherPlagues / Pipes 派生 等）を予定。
- 2025-11-11 PM4: Yd 周辺の住人を追加: Bep / Krka / Une / Rokhas / ManyEyes / Thah（Replace=\"true\"）。Goek に OtherPlagues / Freehold2 ノードを追加して founders 残項目を解消。diff_localization 上の未訳は Tilli 系や Thah のクエスト進行ノード（Landing Pads の動的ノード群）などに集約。
- 2025-11-11 PM4.5: 追加入力 — Krka（Apothecary2 / Freehold2 / Freehold3）、Tilli（Tillifergaewicz）、Thah の Landing Pads 動的ノード（LandingPadsCommentary / CandidatesReady / GiveStar / SlynthDecision / SlynthResolution / SlynthLeave）。diff_localization 上からこれらは解消。未訳は Chavvah 系ダイナミックと議論ツリー（DynamicVillageMayor など）に集約。
- 2025-11-11 PM5: Chavvah/Tzedech/Tikva/Miryam/DynamicVillageMayor を追加（Replace="true"）。ChavvahPrime はクエスト導線（Chime/Work/Work2/Active/Done）に加え Gyredream/Ascent/Barathrum*・Physiology ノード、Slynth 受け入れ系を訳出。Eyn Roj 前の鳴石（ChavvahFrontChime）/ TauChime も訳。diff_localization 未訳は汎用ダイナミック一式（AskForWork 等）と Chavvah 派生の残り分岐に集約。
- 2025-11-12 AM: BaseConversation の汎用ノード（AskForWork／DirectToWork／MoreToAsk／Goodbye／AskName／TellName）を `Replace="true"` で追加して日本語化。`QuestSignpost` と `AskName` パートを維持し、汎用 NPC の選択肢テンプレを補完。
- 2025-11-12 AM2: ManyEyes の MaqqomTell 以降（Mean／NonMoloch）を訳出し、Thah の LandingPadsCommentary に Mechanimists／Mopango／Pariahs／YdFreehold／Hindren／Chavvah／Dynamic のテキストと候補選択肢・`ThahDynamicCommentary` パートを追加。SlynthSettler／SlynthWanderer もテンプレ会話として挿入し、`RevealHydropon` のフックを保持。
- 2025-11-12 PM: Chavvah 関連の残ノードを一括で `Replace="true"` 追加（Dreamer／Thicksalt／Tammuz／TauNoLonger／WanderingTau／TauCompanion／TauSoft）。`{{emote|…}}`、`IfTestState`、贈与フラグ（TauHeadpiece）をベース通り維持しつつ、-else/-then・Gyredream 語彙を既存訳に合わせた。
- 2025-11-12 PM2: Santalalotze（Parasite／Dangerous／Animal／Memory／Uncertain）と Nephilim 水儀式パッチを `Replace="true"` で追加。さらに GyreWightAgolgot/Bethsaida/Shug'ruith/Rermadon/Qas/Qon の会話を日本語化し、コア台詞と固有ラインを翻訳。
- 2025-11-12 PM2.5: `py -3 scripts/diff_localization.py --missing-only --json-path Docs/backlog/conversations.json` を再実行し、Conversations.jp.xml の Missing が解消されたことを確認。

## レビュー指摘メモ（2025-11-11 / Alpha）
- [x] Pax Klanq の Golem 解説に対する英語版レビューを受領。以下の修正を TODO として管理する。（2025-11-13 / Alpha 対応完了）
  - [x] **Catalyst**: 原文通り「純液 3 ドラムでスープを触媒化し栄養を循環させる」までの記述に留め、余計なゲーム仕様の補足は削除。（Conversations.jp.xml:8955-8958）
  - [x] **Armament**: 「メタクローム拳タイプ」を「メタクロームの拳形（こぶしかた）」など具体的な表現に変更。（Conversations.jp.xml:8965-8968）
  - [x] **固有名・用語の統一**（Conversations.jp.xml / Docs/glossary.csv 同日更新）:
    - Pax Klanq → 「パクス・クランク」に統一。
    - Q Girl → 「Qガール」。
    - Moghra’yi → 「モグラヤイ」。
    - Sparafucile → 「スパラフチレ」。
    - Jalopy → 「ジャロピー」（初出のみ簡単な補足）。
    - Eaters/Tomb of the Eaters → 「イーター」「イーターの墓所」。
  - [x] **Indrix**: 「漆黒の護符」→「アマランサスのプリズム」。（Conversations.jp.xml:9050）
  - [x] **色タグ崩れ**: `&COmonporch&y` など開閉の乱れを修正し、必要なら地名だけの表記に戻す。（Conversations.jp.xml:7258）
- [x] 上記対応後に `py -3 scripts/diff_localization.py --missing-only` を実行し、glossary も更新。レビュー結果を再共有する。（2025-11-13 / Alpha）
