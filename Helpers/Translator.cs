using System.Collections.Generic;
using ff14bot.Enums;
using ff14bot.Managers;

namespace LlamaLibrary.Helpers
{
    public static class Translator
    {
        public static readonly Language Language;

        public static string SummoningBell => Summoning_Bell[Language];

        public static string VentureCompleteText => Addon2385[Language];

        public static string VentureRunningText => Addon2384[Language];

        public static string AssignVentureText => Addon2386[Language];

        public static string AssignVentureInProgressText => Addon2387[Language];

        public static string SellInventory => Addon2380[Language];

        public static string SellRetainer => Addon2381[Language];

        public static string EntrustRetainer => Addon2378[Language];

        public static string SelectWard => Addon6349[Language];

        public static string EnterEmpyreum => Warp131395[Language];

        public static string EnterTheFirmament => Warp131342[Language];

        public static string Aethernet => Aetheryte1[Language];

        public static string ResidentialDistrictAethernet => Aetheryte2[Language];

        public static string VisitAnotherWorldServer => Aetheryte3[Language];

        public static string SetHomePoint => Aetheryte4[Language];

        public static string RegisterFavoredDestination => Aetheryte6[Language];

        public static string RegisterFreeDestination => Aetheryte8[Language];

        public static string TravelToInstancedArea => Aetheryte12[Language];

        public static string TravelToTheFirmament => Aetheryte0[Language];

        public static string Cancel => Aetheryte13[Language];

        public static string RelicWeaponZenithEnhancement => CustomTalk721061[Language];

        public static string RelicWeaponAtmaEnhancement => CustomTalk721062[Language];

        public static string ZodiacWeaponRecreation => CustomTalk721117[Language];

        public static string NexusWeaponEnhancement => CustomTalk721107[Language];

        public static string AnimusWeaponEnhancement => CustomTalk721069[Language];

        public static string SoulglazingNovusWeapon => CustomTalk721103[Language];

        public static string NovusWeaponEnhancement => CustomTalk721104[Language];
        public static string AnimaWeaponRecreation => CustomTalk721230[Language];

        public static string ZodiacWeaponAwakening => CustomTalk721146[Language];

        public static string MahatmaExchange => CustomTalk721147[Language];
        public static string BattlecraftLeves => GuildLeveText1[Language];

        public static string FieldcraftLeves => GuildLeveText2[Language];

        public static string TradecraftLeves => GuildLeveText3[Language];

        public static string FactionLeves => GuildLeveText4[Language];

        public static string TutorialLeves => GuildLeveText5[Language];

        public static string LeveHistoryEvaluation => GuildLeveText6[Language];

        public static string Informationonleves => GuildLeveText7[Language];

        public static string Nothing => GuildLeveText8[Language];

        public static string MaelstromLeves => GuildLeveText9[Language];

        public static string OrderoftheTwinAdderLeves => GuildLeveText10[Language];

        public static string ImmortalFlamesLeves => GuildLeveText11[Language];

        public static string SupplyProvisioningMissions => GuildLeveText12[Language];

        public static string CollectReward => GuildLeveText13[Language];

        public static string HousePersonalRoom => CmnDefHousingPersonalRoomEntrance_00178_PersonalPrivate[Language];
        public static string HouseOtherRoom => CmnDefHousingPersonalRoomEntrance_00178_OtherPrivate[Language];
        public static string HouseWorkshop => CmnDefHousingPersonalRoomEntrance_00178_Workshop[Language];
        public static string TripleTriadChallenge => Addon9184[Language];

        static Translator()
        {
            Language = DataManager.CurrentLanguage;
            if (Language == Language.MainlandTraditional)
            {
                Language = Language.Chn;
            }
        }

        private static readonly Dictionary<Language, string> Summoning_Bell = new()
        {
            { Language.Eng, "Summoning Bell" },
            { Language.Jap, "リテイナーベル" },
            { Language.Fre, "Sonnette" },
            { Language.Ger, "Krämerklingel" },
            { Language.Chn, "传唤铃" },
            { Language.TraditionalChinese, "傳喚鈴"}
        };

        //Addon2378
        private static readonly Dictionary<Language, string> Addon2378 = new()
        {
            { Language.Eng, "Entrust or withdraw items. (Slots filled: )" },
            { Language.Jap, "アイテムの受け渡し　[預託中：枠]" },
            { Language.Fre, "Échanger des objets [Servant:  emplacements occupés]" },
            { Language.Ger, "Gegenstände geben oder nehmen (Bereits anvertraut: )" },
            { Language.Chn, "道具管理　[现使用栏位：]" },
            { Language.TraditionalChinese, "道具管理　[現使用欄位：]" }
        };

        //Addon2380
        private static readonly Dictionary<Language, string> Addon2380 = new()
        {
            { Language.Eng, "Sell items in your inventory on the market." },
            { Language.Jap, "マーケット出品（プレイヤー所持品から）" },
            { Language.Fre, "Mettre en vente un objet de votre inventaire" },
            { Language.Ger, "Gegenstände aus dem eigenen Inventar verkaufen" },
            { Language.Chn, "出售（玩家所持物品）" },
            { Language.TraditionalChinese, "出售（玩家所持物品）" }
        };

        //Addon2381
        private static readonly Dictionary<Language, string> Addon2381 = new()
        {
            { Language.Eng, "Sell items in your retainer's inventory on the market." },
            { Language.Jap, "マーケット出品（リテイナー所持品から）" },
            { Language.Fre, "Mettre en vente un objet du servant" },
            { Language.Ger, "Gegenstände aus dem Gehilfeninventar verkaufen" },
            { Language.Chn, "出售（雇员所持物品）" },
            { Language.TraditionalChinese, "出售（僱員所持物品）" }
        };

        //Addon2384
        private static readonly Dictionary<Language, string> Addon2384 = new()
        {
            { Language.Eng, "View venture report. (Complete on" },
            { Language.Jap, "リテイナーベンチャーの確認" },
            { Language.Fre, "Voir la tâche en cours" },
            { Language.Ger, "Laufende Unternehmung einsehen" },
            { Language.Chn, "查看雇员探险情况" },
            { Language.TraditionalChinese, "查看僱員探險情況" }
        };

        //Addon2385
        private static readonly Dictionary<Language, string> Addon2385 = new()
        {
            { Language.Eng, "View venture report. (Complete)" },
            { Language.Jap, "リテイナーベンチャーの確認　[完了]" },
            { Language.Fre, "Voir le rapport de la tâche terminée" },
            { Language.Ger, "Abgeschlossene Unternehmung einsehen" },
            { Language.Chn, "查看雇员探险情况　[结束]" },
            { Language.TraditionalChinese, "查看僱員探險情況　[結束]" }
        };

        //Addon2386
        private static readonly Dictionary<Language, string> Addon2386 = new()
        {
            { Language.Eng, "Assign venture." },
            { Language.Jap, "リテイナーベンチャーの依頼" },
            { Language.Fre, "Liste des tâches" },
            { Language.Ger, "Mit Unternehmung beauftragen" },
            { Language.Chn, "委托雇员进行探险" },
            { Language.TraditionalChinese, "委託僱員進行探險" }
        };

        //Addon2387
        private static readonly Dictionary<Language, string> Addon2387 = new()
        {
            { Language.Eng, "Assign venture. (In progress)" },
            { Language.Jap, "リテイナーベンチャーの依頼　[依頼中]" },
            { Language.Fre, "Liste des tâches [Tâche en cours]" },
            { Language.Ger, "Mit Unternehmung beauftragen (Gehilfe beschäftigt)" },
            { Language.Chn, "委托雇员进行探险　[进行中]" },
            { Language.TraditionalChinese, "委託僱員進行探險　[進行中]" }
        };

        //Addon6349
        private static readonly Dictionary<Language, string> Addon6349 = new()
        {
            { Language.Eng, "Go to specified ward. (Review Tabs)" },
            { Language.Jap, "区を指定して移動（ハウスアピール確認）" },
            { Language.Fre, "Spécifier le secteur où aller (Voir les attraits)" },
            { Language.Ger, "Zum angegebenen Bezirk (Zweck der Unterkunft einsehen)" },
            { Language.Chn, "移动到指定小区（查看房屋宣传标签" },
            { Language.TraditionalChinese, "移動到指定社區（查看房屋宣傳標籤" }
        };

        //Addon9184
        private static readonly Dictionary<Language, string> Addon9184 = new()
        {
            { Language.Eng, "Triple Triad Challenge" },
            { Language.Jap, "カード対戦を申し込む" },
            { Language.Fre, "Défier à Triple Triade" },
            { Language.Ger, "Triple Triad spielen" },
            { Language.Chn, "幻卡挑战" },
            { Language.TraditionalChinese, "幻卡挑戰" }
        };

        //Addon12590
        private static readonly Dictionary<Language, string> Addon12590 = new()
        {
            { Language.Eng, "None in progress" },
            { Language.Jap, "依頼なし" },
            { Language.Fre, "Aucune" },
            { Language.Ger, "Keine Unternehmung" },
            { Language.Chn, "没有探险委托" },
            { Language.TraditionalChinese, "沒有探險委託" }
        };

        //Addon12591
        private static readonly Dictionary<Language, string> Addon12591 = new()
        {
            { Language.Eng, "Complete in" },
            { Language.Jap, "残り時間" },
            { Language.Fre, "Fin dans" },
            { Language.Ger, "Noch" },
            { Language.Chn, "剩余时间" },
            { Language.TraditionalChinese, "剩餘時間" }
        };

        //Addon12592
        private static readonly Dictionary<Language, string> Addon12592 = new()
        {
            { Language.Eng, "Complete" },
            { Language.Jap, "完了" },
            { Language.Fre, "Terminée" },
            { Language.Ger, "Abgeschlossen" },
            { Language.Chn, "结束" },
            { Language.TraditionalChinese, "結束" }
        };

        //CustomTalk721061
        private static readonly Dictionary<Language, string> CustomTalk721061 = new()
        {
            { Language.Eng, "Relic Weapon Zenith Enhancement" },
            { Language.Jap, "「古の武器・ゼニス」の強化" },
            { Language.Fre, "Renforcer une arme antique zénith" },
            { Language.Ger, "Zenit-Waffe verstärken" },
            { Language.Chn, "强化“上古武器·天极”" },
            { Language.TraditionalChinese, "強化“上古武器·天極”" }
        };

        //CustomTalk721062
        private static readonly Dictionary<Language, string> CustomTalk721062 = new()
        {
            { Language.Eng, "Relic Weapon Atma Enhancement" },
            { Language.Jap, "「古の武器・アートマ」の強化" },
            { Language.Fre, "Renforcer une arme antique âtma" },
            { Language.Ger, "Atma-Waffe verstärken" },
            { Language.Chn, "强化“上古武器·魂晶”" },
            { Language.TraditionalChinese, "強化“上古武器·魂晶”" }
        };

        //CustomTalk721069
        private static readonly Dictionary<Language, string> CustomTalk721069 = new()
        {
            { Language.Eng, "Relic Weapon Animus Enhancement" },
            { Language.Jap, "「古の武器・アニムス」の強化" },
            { Language.Fre, "Renforcer une arme antique animus" },
            { Language.Ger, "Animus-Waffe verstärken" },
            { Language.Chn, "强化“上古武器·魂灵”" },
            { Language.TraditionalChinese, "強化“上古武器·魂靈”" }
        };

        //CustomTalk721103
        private static readonly Dictionary<Language, string> CustomTalk721103 = new()
        {
            { Language.Eng, "Relic Weapon Novus Soulglazing" },
            { Language.Jap, "「古の武器・ノウス」の絶霊化" },
            { Language.Fre, "Effectuer un scellage éthéréen" },
            { Language.Ger, "Novus-Waffe beschichten" },
            { Language.Chn, "将“上古武器·新星”绝灵化" },
            { Language.TraditionalChinese, "將“上古武器·新星”絕靈化" }
        };

        //CustomTalk721104
        private static readonly Dictionary<Language, string> CustomTalk721104 = new()
        {
            { Language.Eng, "Relic Weapon Novus Enhancement" },
            { Language.Jap, "「古の武器・ノウス」の強化" },
            { Language.Fre, "Renforcer une arme antique novus" },
            { Language.Ger, "Novus-Waffe verstärken" },
            { Language.Chn, "强化“上古武器·新星”" },
            { Language.TraditionalChinese, "強化“上古武器·新星”" }
        };

        //CustomTalk721107
        private static readonly Dictionary<Language, string> CustomTalk721107 = new()
        {
            { Language.Eng, "Relic Weapon Nexus Modification" },
            { Language.Jap, "「古の武器・ネクサス」の調整" },
            { Language.Fre, "Ajuster une arme antique nexus" },
            { Language.Ger, "Nexus-Waffe modifizieren" },
            { Language.Chn, "调整“上古武器·镇魂”" },
            { Language.TraditionalChinese, "調整“上古武器·鎮魂”" }
        };

        //CustomTalk721117
        private static readonly Dictionary<Language, string> CustomTalk721117 = new()
        {
            { Language.Eng, "Zodiac Weapon Recreation" },
            { Language.Jap, "「ゾディアックウェポン」の再創造" },
            { Language.Fre, "Forger une arme du zodiaque" },
            { Language.Ger, "Zodiak-Waffe rekonstruieren" },
            { Language.Chn, "再铸“黄道武器”" },
            { Language.TraditionalChinese, "再鑄“黃道武器”" }
        };

        //CustomTalk721146
        private static readonly Dictionary<Language, string> CustomTalk721146 = new()
        {
            { Language.Eng, "Zodiac Weapon Awakening" },
            { Language.Jap, "「ゾディアックウェポン」を覚醒させる" },
            { Language.Fre, "Réveiller la conscience d'une arme du zodiaque" },
            { Language.Ger, "Zodiak-Waffe erwecken" },
            { Language.Chn, "让“黄道武器”觉醒" },
            { Language.TraditionalChinese, "讓“黃道武器”覺醒" }
        };

        //CustomTalk721147
        private static readonly Dictionary<Language, string> CustomTalk721147 = new()
        {
            { Language.Eng, "Mahatma Exchange" },
            { Language.Jap, "「マハトマ」を交換する" },
            { Language.Fre, "Obtenir des mahatma" },
            { Language.Ger, "Mahatma erwerben" },
            { Language.Chn, "交换圣魂" },
            { Language.TraditionalChinese, "交換聖魂" }
        };

        //CustomTalk721230
        private static readonly Dictionary<Language, string> CustomTalk721230 = new()
        {
            { Language.Eng, "Anima Weapon Recreation" },
            { Language.Jap, "「アニマウェポン」を再創造する" },
            { Language.Fre, "Recréer une arme anima" },
            { Language.Ger, "Anima-Waffe rekonstruieren" },
            { Language.Chn, "再铸元灵武器" },
            { Language.TraditionalChinese, "再鑄聖靈武器" }
        };

        //Aetheryte0
        private static readonly Dictionary<Language, string> Aetheryte0 = new()
        {
            { Language.Eng, "." },
            { Language.Jap, "" },
            { Language.Fre, "" },
            { Language.Ger, "" },
            { Language.Chn, "" },
            { Language.TraditionalChinese, "" }
        };

        //Aetheryte1
        private static readonly Dictionary<Language, string> Aetheryte1 = new()
        {
            { Language.Eng, "Aethernet." },
            { Language.Jap, "都市転送網" },
            { Language.Fre, "Réseau de transport urbain éthéré" },
            { Language.Ger, "Ätheryten<SoftHyphen/" },
            { Language.Chn, "都市传送网" },
            { Language.TraditionalChinese, "都市傳送網" }
        };

        //Aetheryte2
        private static readonly Dictionary<Language, string> Aetheryte2 = new()
        {
            { Language.Eng, "Residential District Aethernet." },
            { Language.Jap, "冒険者居住区転送" },
            { Language.Fre, "Quartier résidentiel" },
            { Language.Ger, "Wohngebiet" },
            { Language.Chn, "冒险者住宅区传送" },
            { Language.TraditionalChinese, "冒險者住宅區傳送" }
        };

        //Aetheryte3
        private static readonly Dictionary<Language, string> Aetheryte3 = new()
        {
            { Language.Eng, "Visit Another World Server." },
            { Language.Jap, "他のワールドへ遊びにいく" },
            { Language.Fre, "Voyager vers un autre Monde" },
            { Language.Ger, "Weltenreise" },
            { Language.Chn, "跨界传送" },
            { Language.TraditionalChinese, "跨界傳送" }
        };

        //Aetheryte4
        private static readonly Dictionary<Language, string> Aetheryte4 = new()
        {
            { Language.Eng, "Set Home Point." },
            { Language.Jap, "ホームポイント登録" },
            { Language.Fre, "Enregistrer comme point de retour" },
            { Language.Ger, "Als Heimatpunkt registriert" },
            { Language.Chn, "设置返回点" },
            { Language.TraditionalChinese, "設定返回點" }
        };

        //Aetheryte6
        private static readonly Dictionary<Language, string> Aetheryte6 = new()
        {
            { Language.Eng, "Register Favored Destination." },
            { Language.Jap, "お気に入り登録" },
            { Language.Fre, "Enregistrer comme endroit favori" },
            { Language.Ger, "Als Favorit gespeichert" },
            { Language.Chn, "添加到收藏夹" },
            { Language.TraditionalChinese, "新增到收藏夾" }
        };

        //Aetheryte8
        private static readonly Dictionary<Language, string> Aetheryte8 = new()
        {
            { Language.Eng, "<Gui(67)/" },
            { Language.Jap, "<Gui(67)/" },
            { Language.Fre, "Enregistrer comme favori gratuit (Identificateur)" },
            { Language.Ger, "<Gui(67)/" },
            { Language.Chn, "<Gui(67)/" },
            { Language.TraditionalChinese, "<Gui(67)/" }
        };


        //Aetheryte12
        private static readonly Dictionary<Language, string> Aetheryte12 = new()
        {
            { Language.Eng, "Travel to Instanced Area." },
            { Language.Jap, "インスタンスエリアへ移動" },
            { Language.Fre, "Changer d'instance" },
            { Language.Ger, "In ein instanziiertes Areal wechseln" },
            { Language.Chn, "切换副本区" },
            { Language.TraditionalChinese, "切換副本區" }
        };


        //Aetheryte13
        private static readonly Dictionary<Language, string> Aetheryte13 = new()
        {
            { Language.Eng, "Cancel." },
            { Language.Jap, "キャンセル" },
            { Language.Fre, "Annuler" },
            { Language.Ger, "Abbrechen" },
            { Language.Chn, "取消" },
            { Language.TraditionalChinese, "取消" }
        };

        //Warp131342
        private static readonly Dictionary<Language, string> Warp131342 = new()
        {
            { Language.Eng, "Enter the Firmament" },
            { Language.Jap, "「蒼天街」へ行く" },
            { Language.Fre, "Aller à Azurée" },
            { Language.Ger, "Nach Himmelsstadt" },
            { Language.Chn, "前往天穹街" },
            { Language.TraditionalChinese, "前往蒼天街" }
        };

        //Warp131395
        private static readonly Dictionary<Language, string> Warp131395 = new()
        {
            { Language.Eng, "Enter Empyreum" },
            { Language.Jap, "「エンピレアム」へ行く" },
            { Language.Fre, "Aller à Empyrée" },
            { Language.Ger, "Zum Empyreum" },
            { Language.Chn, "前往穹顶皓天" },
            { Language.TraditionalChinese, "前往穹頂皓天" }
        };

        //GuildLeveText1
        private static readonly Dictionary<Language, string> GuildLeveText1 = new()
        {
            { Language.Eng, "Battlecraft Leves." },
            { Language.Jap, "傭兵稼業" },
            { Language.Fre, "Les mandats de mercenariat" },
            { Language.Ger, "Gefechtserlasse" },
            { Language.Chn, "佣兵任务" },
            { Language.TraditionalChinese, "傭兵任務" }
        };

        //GuildLeveText2
        private static readonly Dictionary<Language, string> GuildLeveText2 = new()
        {
            { Language.Eng, "Fieldcraft Leves." },
            { Language.Jap, "採集稼業" },
            { Language.Fre, "Les mandats de récolte" },
            { Language.Ger, "Sammelerlasse" },
            { Language.Chn, "采集任务" },
            { Language.TraditionalChinese, "採集任務" }
        };

        //GuildLeveText3
        private static readonly Dictionary<Language, string> GuildLeveText3 = new()
        {
            { Language.Eng, "Tradecraft Leves." },
            { Language.Jap, "製作稼業" },
            { Language.Fre, "Les mandats d'artisanat" },
            { Language.Ger, "Fertigungserlasse" },
            { Language.Chn, "制作任务" },
            { Language.TraditionalChinese, "製作任務" }
        };

        //GuildLeveText4
        private static readonly Dictionary<Language, string> GuildLeveText4 = new()
        {
            { Language.Eng, "Faction Leves." },
            { Language.Jap, "ファクションリーヴ" },
            { Language.Fre, "Les mandats de faction" },
            { Language.Ger, "Fraktionserlasse" },
            { Language.Chn, "特殊部队理符" },
            { Language.TraditionalChinese, "特殊部隊理符" }
        };

        //GuildLeveText5
        private static readonly Dictionary<Language, string> GuildLeveText5 = new()
        {
            { Language.Eng, "Tutorial Leves." },
            { Language.Jap, "チュートリアル" },
            { Language.Fre, "Les mandats pour débutants" },
            { Language.Ger, "Ein Tutorial" },
            { Language.Chn, "新手教程" },
            { Language.TraditionalChinese, "新手教程" }
        };

        //GuildLeveText6
        private static readonly Dictionary<Language, string> GuildLeveText6 = new()
        {
            { Language.Eng, "Leve History Evaluation." },
            { Language.Jap, "履歴評価を受ける" },
            { Language.Fre, "Une évaluation" },
            { Language.Ger, "Evaluierung bisheriger Freibriefe" },
            { Language.Chn, "接受履历评价" },
            { Language.TraditionalChinese, "接受履歷評價" }
        };

        //GuildLeveText7
        private static readonly Dictionary<Language, string> GuildLeveText7 = new()
        {
            { Language.Eng, "Information on leves." },
            { Language.Jap, "説明を聞く" },
            { Language.Fre, "Des explications sur les mandats" },
            { Language.Ger, "Genauere Erklärungen" },
            { Language.Chn, "听取说明" },
            { Language.TraditionalChinese, "聽取說明" }
        };

        //GuildLeveText8
        private static readonly Dictionary<Language, string> GuildLeveText8 = new()
        {
            { Language.Eng, "Nothing." },
            { Language.Jap, "キャンセル" },
            { Language.Fre, "Annuler" },
            { Language.Ger, "Abbrechen" },
            { Language.Chn, "取消" },
            { Language.TraditionalChinese, "取消" }
        };

        //GuildLeveText9
        private static readonly Dictionary<Language, string> GuildLeveText9 = new()
        {
            { Language.Eng, "Maelstrom Leves." },
            { Language.Jap, "黒渦団任務" },
            { Language.Fre, "Mandats du Maelstrom" },
            { Language.Ger, "Aufträge des Mahlstroms" },
            { Language.Chn, "黑涡团任务" },
            { Language.TraditionalChinese, "黑渦團任務" }
        };

        //GuildLeveText10
        private static readonly Dictionary<Language, string> GuildLeveText10 = new()
        {
            { Language.Eng, "Order of the Twin Adder Leves." },
            { Language.Jap, "双蛇党任務" },
            { Language.Fre, "Mandats des Deux Vipères" },
            { Language.Ger, "Aufträge der Bruderschaft der Morgenviper" },
            { Language.Chn, "双蛇党任务" },
            { Language.TraditionalChinese, "雙蛇黨任務" }
        };

        //GuildLeveText11
        private static readonly Dictionary<Language, string> GuildLeveText11 = new()
        {
            { Language.Eng, "Immortal Flames Leves." },
            { Language.Jap, "不滅隊任務" },
            { Language.Fre, "Mandats des Immortels" },
            { Language.Ger, "Aufträge der Legion der Unsterblichen" },
            { Language.Chn, "恒辉队任务" },
            { Language.TraditionalChinese, "不滅隊任務" }
        };

        //GuildLeveText12
        private static readonly Dictionary<Language, string> GuildLeveText12 = new()
        {
            { Language.Eng, "Supply & Provisioning Missions." },
            { Language.Jap, "調達任務" },
            { Language.Fre, "Missions de ravitaillement" },
            { Language.Ger, "Liefereinsätze" },
            { Language.Chn, "筹备任务" },
            { Language.TraditionalChinese, "籌備任務" }
        };

        //GuildLeveText13
        private static readonly Dictionary<Language, string> GuildLeveText13 = new()
        {
            { Language.Eng, "Collect Reward." },
            { Language.Jap, "報酬を受け取る" },
            { Language.Fre, "Retirer une récompense" },
            { Language.Ger, "Meine Belohnung" },
            { Language.Chn, "领取报酬" },
            { Language.TraditionalChinese, "領取報酬" }
        };

        //CmnDefHousingPersonalRoomEntrance_00178_1
        private static readonly Dictionary<Language, string> CmnDefHousingPersonalRoomEntrance_00178_PersonalPrivate = new()
        {
            { Language.Eng, "Move to your private chambers" },
            { Language.Jap, "自分の個室に移動する" },
            { Language.Fre, "Aller dans votre chambre" },
            { Language.Ger, "Das eigene Zimmer betreten" },
            { Language.Chn, "移动到自己的房间" }
        };

        //CmnDefHousingPersonalRoomEntrance_00178_2
        private static readonly Dictionary<Language, string> CmnDefHousingPersonalRoomEntrance_00178_OtherPrivate = new()
        {
            { Language.Eng, "Move to specified private chambers" },
            { Language.Jap, "個室番号を指定して移動（ハウスアピール確認）" },
            { Language.Fre, "Spécifier la chambre où aller (Voir les attraits)" },
            { Language.Ger, "Ein bestimmtes Zimmer betreten (Zweck der Unterkunft einsehen)" },
            { Language.Chn, "移动到指定号码房间（查看房屋宣传标签）" }
        };

        //CmnDefHousingPersonalRoomEntrance_00178_11
        private static readonly Dictionary<Language, string> CmnDefHousingPersonalRoomEntrance_00178_Workshop = new()
        {
            { Language.Eng, "Move to the company workshop" },
            { Language.Jap, "地下工房に移動する" },
            { Language.Fre, "Aller dans l'atelier de compagnie" },
            { Language.Ger, "schaftswerkstätte betreten" },
            { Language.Chn, "移动到部队工房" }
        };

        public static string Hard => HardText[Language];

        private static readonly Dictionary<Language, string> HardText = new()
        {
            { Language.Eng, "Hard" },
            { Language.Jap, "Hard" },
            { Language.Fre, "Hard" },
            { Language.Ger, "Hard" },
            { Language.Chn, "高难本" }
        };

        public static string Extreme => ExtremeText[Language];

        private static readonly Dictionary<Language, string> ExtremeText = new()
        {
            { Language.Eng, "Extreme" },
            { Language.Jap, "Extreme" },
            { Language.Fre, "Extreme" },
            { Language.Ger, "Extreme" },
            { Language.Chn, "极难本" }
        };

        public static string Savage => SavageText[Language];

        private static readonly Dictionary<Language, string> SavageText = new()
        {
            { Language.Eng, "Savage" },
            { Language.Jap, "Savage" },
            { Language.Fre, "Savage" },
            { Language.Ger, "Savage" },
            { Language.Chn, "绝难本" }
        };
    }
}