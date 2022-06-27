using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ff14bot.Enums;
using ff14bot.Managers;

namespace LlamaLibrary.Helpers
{
    public static class Translator
    {
        public static readonly Language Language;

        public static string SummoningBell => summoning_Bell[Language];

        public static string VentureCompleteText => addon2385[Language];

        public static string VentureRunningText => Addon2384[Language];

        public static string AssignVentureText => addon2386[Language];

        public static string AssignVentureInProgressText => addon2387[Language];

        public static string SellInventory => addon2380[Language];

        public static string SellRetainer => addon2381[Language];

        public static string EntrustRetainer => addon2378[Language];

        public static string SelectWard => Addon6349[Language];

        public static string EnterEmpyreum => Warp131395[Language];

        public static string EnterTheFirmament => Warp131342[Language];

        public static string Aethernet => Aetheryte1[Language];

        public static string ResidentialDistrictAethernet => Aetheryte2[Language];

        public static string VisitAnotherWorldServer => Aetheryte3[Language];

        public static string SetHomePoint => Aetheryte4[Language];

        public static string RegisterFavoredDestination => Aetheryte6[Language];

        public static string RegisterFreeDestination => Aetheryte8[Language];

        public static string TravelToInstancedArea => Aetheryte10[Language];

        public static string TravelToTheFirmament => Aetheryte0[Language];

        public static string Cancel => Aetheryte11[Language];

        static Translator()
        {
            Language = (Language)typeof(DataManager).GetFields(BindingFlags.Static | BindingFlags.NonPublic)
                .First(i => i.FieldType == typeof(Language)).GetValue(null);
        }

        //Addon # 2378
        private static Dictionary<Language, string> addon2378 = new Dictionary<Language, string>
        {
            { Language.Eng, "Entrust or withdraw items." },
            { Language.Jap, "アイテムの受け渡し" },
            { Language.Fre, "Échanger des objets" },
            { Language.Ger, "Gegenstände geben oder nehmen" },
            { Language.Chn, "" }
        };

        private static Dictionary<Language, string> summoning_Bell = new Dictionary<Language, string>
        {
            { Language.Eng, "Summoning Bell" },
            { Language.Jap, "リテイナーベル" },
            { Language.Fre, "Sonnette" },
            { Language.Ger, "Krämerklingel" },
            { Language.Chn, "传唤铃" }
        };

        //Addon # 2385
        private static Dictionary<Language, string> addon2385 = new Dictionary<Language, string>
        {
            { Language.Eng, "View venture report. (Complete)" },
            { Language.Jap, "リテイナーベンチャーの確認　[完了]" },
            { Language.Fre, "Voir le rapport de la tâche terminée" },
            { Language.Ger, "Abgeschlossene Unternehmung einsehen" },
            { Language.Chn, "查看雇员探险情况　[结束]" }
        };

        //Addon # 2384
        static Dictionary<Language, string> Addon2384 = new Dictionary<Language, string>
        {
            { Language.Eng, "View venture report. (Complete on" },
            { Language.Jap, "リテイナーベンチャーの確認　[～" },
            { Language.Fre, "Voir la tâche en cours [Fin le" },
            { Language.Ger, "Laufende Unternehmung einsehen (Abschluss am" },
            { Language.Chn, "" }
        };

        //Addon # 2386
        private static Dictionary<Language, string> addon2386 = new Dictionary<Language, string>
        {
            { Language.Eng, "Assign venture." },
            { Language.Jap, "リテイナーベンチャーの依頼" },
            { Language.Fre, "Liste des tâches" },
            { Language.Ger, "Mit Unternehmung beauftragen" },
            { Language.Chn, "委托雇员进行探险" }
        };

        //Addon # 2387
        private static Dictionary<Language, string> addon2387 = new Dictionary<Language, string>
        {
            { Language.Eng, "Assign venture. (In progress)" },
            { Language.Jap, "リテイナーベンチャーの依頼　[依頼中]" },
            { Language.Fre, "Liste des tâches [Tâche en cours]" },
            { Language.Ger, "Mit Unternehmung beauftragen (Gehilfe beschäftigt)" },
            { Language.Chn, "委托雇员进行探险　[进行中]" }
        };

        //Addon # 12590
        private static Dictionary<Language, string> addon12590 = new Dictionary<Language, string>
        {
            { Language.Eng, "None in progress" },
            { Language.Jap, "依頼なし" },
            { Language.Fre, "Aucune" },
            { Language.Ger, "Keine Unternehmung" },
            { Language.Chn, "没有探险委托" }
        };

        //Addon # 12591
        private static Dictionary<Language, string> addon12591 = new Dictionary<Language, string>
        {
            { Language.Eng, "Complete in " },
            { Language.Jap, "残り時間" },
            { Language.Fre, "Fin de la tâche dans " },
            { Language.Ger, "Noch " },
            { Language.Chn, "剩余时间" }
        };

        //Addon # 12592
        private static Dictionary<Language, string> addon12592 = new Dictionary<Language, string>
        {
            { Language.Eng, "Complete" },
            { Language.Jap, "完了" },
            { Language.Fre, "Terminée" },
            { Language.Ger, "Abgeschlossen" },
            { Language.Chn, "结束" }
        };

        //Addon # 2380
        private static Dictionary<Language, string> addon2380 = new Dictionary<Language, string>
        {
            { Language.Eng, "Sell items in your inventory on the market." },
            { Language.Jap, "マーケット出品（プレイヤー所持品から）" },
            { Language.Fre, "Mettre en vente un objet de votre inventaire" },
            { Language.Ger, "Gegenstände aus dem eigenen Inventar verkaufen" },
            { Language.Chn, "" }
        };

        //Addon # 2381
        private static Dictionary<Language, string> addon2381 = new Dictionary<Language, string>
        {
            { Language.Eng, "Sell items in your retainer's inventory on the market." },
            { Language.Jap, "マーケット出品（リテイナー所持品から）" },
            { Language.Fre, "Mettre en vente un objet du servant" },
            { Language.Ger, "Gegenstände aus dem Gehilfeninventar verkaufen" },
            { Language.Chn, "" }
        };

        //Addon # 6349
        static Dictionary<Language, string> Addon6349 = new Dictionary<Language, string>
        {
            { Language.Eng, "Go to specified ward. (Review Tabs)" },
            { Language.Jap, "区を指定して移動（ハウスアピール確認）" },
            { Language.Fre, "Spécifier le secteur où aller (Voir les attraits)" },
            { Language.Ger, "Zum angegebenen Bezirk (Zweck der Unterkunft einsehen)" },
            { Language.Chn, "移动到指定小区" }
        };

        //Warp # 131395
        static Dictionary<Language, string> Warp131395 = new Dictionary<Language, string>
        {
            { Language.Eng, "Enter Empyreum" },
            { Language.Jap, "「エンピレアム」へ行く" },
            { Language.Fre, "Aller à Empyrée" },
            { Language.Ger, "Zum Empyreum" },
            { Language.Chn, "" }
        };

        //Warp # 131342
        static Dictionary<Language, string> Warp131342 = new Dictionary<Language, string>
        {
            { Language.Eng, "Enter the Firmament" },
            { Language.Jap, "「蒼天街」へ行く" },
            { Language.Fre, "Aller à Azurée" },
            { Language.Ger, "Nach Himmelsstadt" },
            { Language.Chn, "" }
        };

        //Aetheryte # 1
        static Dictionary<Language, string> Aetheryte1 = new Dictionary<Language, string>
        {
            { Language.Eng, "Aethernet." },
            { Language.Jap, "都市転送網" },
            { Language.Fre, "Réseau de transport urbain éthéré" },
            { Language.Ger, "Ätheryten<SoftHyphen/" },
            { Language.Chn, "" }
        };

        //Aetheryte # 2
        static Dictionary<Language, string> Aetheryte2 = new Dictionary<Language, string>
        {
            { Language.Eng, "Residential District Aethernet." },
            { Language.Jap, "冒険者居住区転送" },
            { Language.Fre, "Quartier résidentiel" },
            { Language.Ger, "Wohngebiet" },
            { Language.Chn, "" }
        };

        //Aetheryte # 3
        static Dictionary<Language, string> Aetheryte3 = new Dictionary<Language, string>
        {
            { Language.Eng, "Visit Another World Server." },
            { Language.Jap, "他のワールドへ遊びにいく" },
            { Language.Fre, "Voyager vers un autre Monde" },
            { Language.Ger, "Weltenreise" },
            { Language.Chn, "" }
        };

        //Aetheryte # 4
        static Dictionary<Language, string> Aetheryte4 = new Dictionary<Language, string>
        {
            { Language.Eng, "Set Home Point." },
            { Language.Jap, "ホームポイント登録" },
            { Language.Fre, "Enregistrer comme point de retour" },
            { Language.Ger, "Als Heimatpunkt registriert" },
            { Language.Chn, "" }
        };

        //Aetheryte # 6
        static Dictionary<Language, string> Aetheryte6 = new Dictionary<Language, string>
        {
            { Language.Eng, "Register Favored Destination." },
            { Language.Jap, "お気に入り登録" },
            { Language.Fre, "Enregistrer comme endroit favori" },
            { Language.Ger, "Als Favorit gespeichert" },
            { Language.Chn, "" }
        };

        //Aetheryte # 8
        static Dictionary<Language, string> Aetheryte8 = new Dictionary<Language, string>
        {
            { Language.Eng, "Register Free Destination." },
            { Language.Jap, "お気に入り（無料）登録" },
            { Language.Fre, "Enregistrer comme favori gratuit" },
            { Language.Ger, "Als kostenloser Favorit gespeichert" },
            { Language.Chn, "" }
        };

        //Aetheryte # 10
        static Dictionary<Language, string> Aetheryte10 = new Dictionary<Language, string>
        {
            { Language.Eng, "Travel to Instanced Area." },
            { Language.Jap, "インスタンスエリアへ移動" },
            { Language.Fre, "Changer d'instance" },
            { Language.Ger, "In ein instanziiertes Areal wechseln" },
            { Language.Chn, "" }
        };

        //Aetheryte # 11
        static Dictionary<Language, string> Aetheryte11 = new Dictionary<Language, string>
        {
            { Language.Eng, "Cancel." },
            { Language.Jap, "キャンセル" },
            { Language.Fre, "Annuler" },
            { Language.Ger, "Abbrechen" },
            { Language.Chn, "" }
        };

        //Aetheryte # 0
        static Dictionary<Language, string> Aetheryte0 = new Dictionary<Language, string>
        {
            { Language.Eng, "Travel to the Firmament." },
            { Language.Jap, "蒼天街転送" },
            { Language.Fre, "Azurée" },
            { Language.Ger, "Himmelsstadt" },
            { Language.Chn, "" }
        };
    }
}