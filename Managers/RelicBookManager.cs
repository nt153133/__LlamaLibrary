// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using LlamaLibrary.Helpers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Managers

{
    public class RelicBookManager
    {
        public static readonly Language Language;

        public static string BooksofFire => CmnDefRelicWeapon025GetNote_00167_9[Language];
        public static string BooksofFall => CmnDefRelicWeapon025GetNote_00167_10[Language];
        public static string BooksofWind => CmnDefRelicWeapon025GetNote_00167_11[Language];
        public static string BooksofEarth => CmnDefRelicWeapon025GetNote_00167_12[Language];
        public static string PLDBookOfNetherfire => CmnDefRelicWeapon025GetNote_00167_13[Language];
        public static string PLDBookOfNetherfall => CmnDefRelicWeapon025GetNote_00167_14[Language];
        public static string PLDBooksOfSwords => CmnDefRelicWeapon025GetNote_00167_7[Language];
        public static string PLDBooksOfShields => CmnDefRelicWeapon025GetNote_00167_8[Language];

        static RelicBookManager()
        {
            Language = DataManager.CurrentLanguage;
        }

        //CmnDefRelicWeapon025GetNote_00167_9
        private static readonly Dictionary<Language, string> CmnDefRelicWeapon025GetNote_00167_9 = new()
        {
            { Language.Eng, "The Books of Fire" },
            { Language.Jap, "炎の書" },
            { Language.Fre, "Livre du feu" },
            { Language.Ger, "Tafel des Himmelsfeuers" },
            { Language.Chn, "火天文书" }
        };

        //CmnDefRelicWeapon025GetNote_00167_10
        private static readonly Dictionary<Language, string> CmnDefRelicWeapon025GetNote_00167_10 = new()
        {
            { Language.Eng, "The Books of Fall" },
            { Language.Jap, "水の書" },
            { Language.Fre, "Livre de l'eau" },
            { Language.Ger, "Tafel des Himmelsfalles" },
            { Language.Chn, "水天文书" }
        };

        //CmnDefRelicWeapon025GetNote_00167_11
        private static readonly Dictionary<Language, string> CmnDefRelicWeapon025GetNote_00167_11 = new()
        {
            { Language.Eng, "The Books of Wind" },
            { Language.Jap, "風の書" },
            { Language.Fre, "Livre du vent" },
            { Language.Ger, "Tafel des Himmelswindes" },
            { Language.Chn, "风天文书" }
        };

        //CmnDefRelicWeapon025GetNote_00167_12
        private static readonly Dictionary<Language, string> CmnDefRelicWeapon025GetNote_00167_12 = new()
        {
            { Language.Eng, "The Books of Earth" },
            { Language.Jap, "土の書" },
            { Language.Fre, "Livre de la terre" },
            { Language.Ger, "Tafel der Himmelserde" },
            { Language.Chn, "土天文书" }
        };

        //CmnDefRelicWeapon025GetNote_00167_13
        private static readonly Dictionary<Language, string> CmnDefRelicWeapon025GetNote_00167_13 = new()
        {
            { Language.Eng, "The Book of Netherfire" },
            { Language.Jap, "炎獄の書" },
            { Language.Fre, "Livre du feu" },
            { Language.Ger, "Tafel des Jenseitsfeuers" },
            { Language.Chn, "火狱文书" }
        };

        //CmnDefRelicWeapon025GetNote_00167_14
        private static readonly Dictionary<Language, string> CmnDefRelicWeapon025GetNote_00167_14 = new()
        {
            { Language.Eng, "The Book of Netherfall" },
            { Language.Jap, "水獄の書" },
            { Language.Fre, "Livre de l'eau" },
            { Language.Ger, "Tafel des Jenseitsfalles" },
            { Language.Chn, "水狱文书" }
        };

        //CmnDefRelicWeapon025GetNote_00167_7
        private static readonly Dictionary<Language, string> CmnDefRelicWeapon025GetNote_00167_7 = new()
        {
            { Language.Eng, "Books pertaining to swords." },
            { Language.Jap, "剣の「黄道十二文書」を選ぶ" },
            { Language.Fre, "Livres sur Curtana" },
            { Language.Ger, "Tafel des Schwerts" },
            { Language.Chn, "选择剑之“黄道文书”" }
        };

        //CmnDefRelicWeapon025GetNote_00167_8
        private static readonly Dictionary<Language, string> CmnDefRelicWeapon025GetNote_00167_8 = new()
        {
            { Language.Eng, "Books pertaining to shields." },
            { Language.Jap, "盾の「黄道十二文書」を選ぶ" },
            { Language.Fre, "Livres sur le Bouclier saint" },
            { Language.Ger, "Tafel des Schilds" },
            { Language.Chn, "选择盾之“黄道文书”" }
        };

        public static Dictionary<RelicBookType, byte> MaxCount = new()
        {
            { RelicBookType.Fire, 3 },
            { RelicBookType.Fall, 3 },
            { RelicBookType.Wind, 2 },
            { RelicBookType.Earth, 1 }
        };

        public static byte GetNumOfRelicNoteCompleted(uint relicId, RelicBookType relicBookType)
        {
            return Core.Memory.CallInjectedWraper<byte>(RelicBookManagerOffsets.GetNumOfRelicNoteCompleted, RelicBookManagerOffsets.UIRelicNote, relicId, (byte)relicBookType);
        }

        public static byte NumOfFireCompleted(uint relicId)
        {
            return GetNumOfRelicNoteCompleted(relicId, RelicBookType.Fire);
        }

        public static byte NumOfFallCompleted(uint relicId)
        {
            return GetNumOfRelicNoteCompleted(relicId, RelicBookType.Fall);
        }

        public static byte NumOfWindCompleted(uint relicId)
        {
            return GetNumOfRelicNoteCompleted(relicId, RelicBookType.Wind);
        }

        public static byte NumOfEarthCompleted(uint relicId)
        {
            return GetNumOfRelicNoteCompleted(relicId, RelicBookType.Earth);
        }

        public static bool IsBookCompleted(uint relicId, RelicBookType relicBookType)
        {
            return GetNumOfRelicNoteCompleted(relicId, relicBookType) >= MaxCountByType(relicId, relicBookType);
        }

        public static byte MaxCountByType(uint relicId, RelicBookType relicBookType)
        {
            return relicId switch
            {
                7824 when relicBookType is RelicBookType.Fall or RelicBookType.Fire => 2,
                7833 when relicBookType is RelicBookType.Fall or RelicBookType.Fire => 1,
                _                                                                   => MaxCount[relicBookType]
            };
        }

        public static bool IsFireCompleted(uint relicId)
        {
            return IsBookCompleted(relicId, RelicBookType.Fire);
        }

        public static bool IsFallCompleted(uint relicId)
        {
            return IsBookCompleted(relicId, RelicBookType.Fall);
        }

        public static bool IsWindCompleted(uint relicId)
        {
            return IsBookCompleted(relicId, RelicBookType.Wind);
        }

        public static bool IsEarthCompleted(uint relicId)
        {
            return IsBookCompleted(relicId, RelicBookType.Earth);
        }

        public static string ProgressString(uint relicId, RelicBookType relicBookType)
        {
            if (relicId == 7833 && relicBookType == RelicBookType.Fire)
            {
                return string.Format(CmnDefRelicWeapon025GetNote_00167_13[Translator.Language], NumOfFireCompleted(relicId), MaxCountByType(relicId, relicBookType));
            }

            if (relicId == 7833 && relicBookType == RelicBookType.Fall)
            {
                return string.Format(CmnDefRelicWeapon025GetNote_00167_14[Translator.Language], NumOfFallCompleted(relicId), MaxCountByType(relicId, relicBookType));
            }

            return relicBookType switch
            {
                RelicBookType.Fire  => string.Format(CmnDefRelicWeapon025GetNote_00167_9[Translator.Language], NumOfFireCompleted(relicId), MaxCountByType(relicId, relicBookType)),
                RelicBookType.Fall  => string.Format(CmnDefRelicWeapon025GetNote_00167_10[Translator.Language], NumOfFallCompleted(relicId), MaxCountByType(relicId, relicBookType)),
                RelicBookType.Wind  => string.Format(CmnDefRelicWeapon025GetNote_00167_11[Translator.Language], NumOfWindCompleted(relicId), MaxCountByType(relicId, relicBookType)),
                RelicBookType.Earth => string.Format(CmnDefRelicWeapon025GetNote_00167_12[Translator.Language], NumOfEarthCompleted(relicId), MaxCountByType(relicId, relicBookType)),
                _                   => throw new ArgumentOutOfRangeException(nameof(relicBookType), relicBookType, null)
            };
        }

        
    }

    public enum RelicBookType : byte
    {
        Fire = 0,
        Fall = 1,
        Wind = 2,
        Earth = 3
    }
}