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

namespace LlamaLibrary.LlamaManagers

{
    /// <summary>
    /// Static manager for tracking progress on the "Trials of the Braves" (Relic Weapon) books.
    /// Handles retrieval of localized book names and completion status for various relic stages.
    /// </summary>
    public class RelicBookManager
    {
        /// <summary>
        /// Gets the current game language used for localized strings.
        /// </summary>
        public static readonly Language Language;

        /// <summary>Gets the localized name for "The Books of Fire".</summary>
        public static string BooksofFire => CmnDefRelicWeapon025GetNote_00167_9[Language];

        /// <summary>Gets the localized name for "The Books of Fall".</summary>
        public static string BooksofFall => CmnDefRelicWeapon025GetNote_00167_10[Language];

        /// <summary>Gets the localized name for "The Books of Wind".</summary>
        public static string BooksofWind => CmnDefRelicWeapon025GetNote_00167_11[Language];

        /// <summary>Gets the localized name for "The Books of Earth".</summary>
        public static string BooksofEarth => CmnDefRelicWeapon025GetNote_00167_12[Language];

        /// <summary>Gets the localized name for "The Book of Netherfire" (Paladin).</summary>
        public static string PLDBookOfNetherfire => CmnDefRelicWeapon025GetNote_00167_13[Language];

        /// <summary>Gets the localized name for "The Book of Netherfall" (Paladin).</summary>
        public static string PLDBookOfNetherfall => CmnDefRelicWeapon025GetNote_00167_14[Language];

        /// <summary>Gets the localized name for books pertaining to swords (Paladin).</summary>
        public static string PLDBooksOfSwords => CmnDefRelicWeapon025GetNote_00167_7[Language];

        /// <summary>Gets the localized name for books pertaining to shields (Paladin).</summary>
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
            { Language.Chn, "火天文书" },
            { Language.TraditionalChinese, "火天文書" }
        };

        //CmnDefRelicWeapon025GetNote_00167_10
        private static readonly Dictionary<Language, string> CmnDefRelicWeapon025GetNote_00167_10 = new()
        {
            { Language.Eng, "The Books of Fall" },
            { Language.Jap, "水の書" },
            { Language.Fre, "Livre de l'eau" },
            { Language.Ger, "Tafel des Himmelsfalles" },
            { Language.Chn, "水天文书" },
            { Language.TraditionalChinese, "水天文書" }
        };

        //CmnDefRelicWeapon025GetNote_00167_11
        private static readonly Dictionary<Language, string> CmnDefRelicWeapon025GetNote_00167_11 = new()
        {
            { Language.Eng, "The Books of Wind" },
            { Language.Jap, "風天の書" },
            { Language.Fre, "Livre du vent céleste" },
            { Language.Ger, "Tafel des Himmelswindes" },
            { Language.Chn, "风天文书" },
            { Language.TraditionalChinese, "風天文書" }
        };


        //CmnDefRelicWeapon025GetNote_00167_12
        private static readonly Dictionary<Language, string> CmnDefRelicWeapon025GetNote_00167_12 = new()
        {
            { Language.Eng, "The Books of Earth" },
            { Language.Jap, "土天の書" },
            { Language.Fre, "Livre de la terre céleste" },
            { Language.Ger, "Tafel der Himmelserde" },
            { Language.Chn, "土天文书(提升" },
            { Language.TraditionalChinese, "土天文書" }
        };


        //CmnDefRelicWeapon025GetNote_00167_13
        private static readonly Dictionary<Language, string> CmnDefRelicWeapon025GetNote_00167_13 = new()
        {
            { Language.Eng, "The Book of Netherfire" },
            { Language.Jap, "炎獄の書" },
            { Language.Fre, "Livre du feu infernal" },
            { Language.Ger, "Tafel des Jenseitsfeuers" },
            { Language.Chn, "火狱文书" },
            { Language.TraditionalChinese, "火獄文書(提升" }
        };


        //CmnDefRelicWeapon025GetNote_00167_14
        private static readonly Dictionary<Language, string> CmnDefRelicWeapon025GetNote_00167_14 = new()
        {
            { Language.Eng, "The Book of Netherfall" },
            { Language.Jap, "水獄の書" },
            { Language.Fre, "Livre de l'eau infernale" },
            { Language.Ger, "Tafel des Jenseitsfalles" },
            { Language.Chn, "水狱文书(提升" },
            { Language.TraditionalChinese, "水獄文書(提升" }
        };

        //CmnDefRelicWeapon025GetNote_00167_7
        private static readonly Dictionary<Language, string> CmnDefRelicWeapon025GetNote_00167_7 = new()
        {
            { Language.Eng, "Books pertaining to swords." },
            { Language.Jap, "剣の「黄道十二文書」を選ぶ" },
            { Language.Fre, "Livres sur Curtana" },
            { Language.Ger, "Tafel des Schwerts" },
            { Language.Chn, "选择剑之“黄道文书”" },
            { Language.TraditionalChinese, "選擇劍之「黃道文書」" }
        };


        //CmnDefRelicWeapon025GetNote_00167_8
        private static readonly Dictionary<Language, string> CmnDefRelicWeapon025GetNote_00167_8 = new()
        {
            { Language.Eng, "Books pertaining to shields." },
            { Language.Jap, "盾の「黄道十二文書」を選ぶ" },
            { Language.Fre, "Livres sur le Bouclier saint" },
            { Language.Ger, "Tafel des Schilds" },
            { Language.Chn, "选择盾之“黄道文书”" },
            { Language.TraditionalChinese, "選擇盾之「黃道文書」" }
        };

        /// <summary>
        /// Gets the maximum number of objectives required for each book type in a standard relic weapon stage.
        /// </summary>
        public static Dictionary<RelicBookType, byte> MaxCount = new()
        {
            { RelicBookType.Fire, 3 },
            { RelicBookType.Fall, 3 },
            { RelicBookType.Wind, 2 },
            { RelicBookType.Earth, 1 }
        };

        /// <summary>
        /// Retrieves the number of completed objectives for a specific book type and relic ID.
        /// </summary>
        /// <param name="relicId">The ID of the relic weapon.</param>
        /// <param name="relicBookType">The category of the book (Fire, Fall, Wind, Earth).</param>
        /// <returns>The number of objectives completed.</returns>
        public static byte GetNumOfRelicNoteCompleted(uint relicId, RelicBookType relicBookType)
        {
            return Core.Memory.CallInjectedWraper<byte>(RelicBookManagerOffsets.GetNumOfRelicNoteCompleted, RelicBookManagerOffsets.UIRelicNote, relicId, (byte)relicBookType);
        }

        /// <summary>Retrieves the number of completed objectives for "Books of Fire".</summary>
        /// <param name="relicId">The ID of the relic weapon.</param>
        /// <returns>The number of objectives completed.</returns>
        public static byte NumOfFireCompleted(uint relicId)
        {
            return GetNumOfRelicNoteCompleted(relicId, RelicBookType.Fire);
        }

        /// <summary>Retrieves the number of completed objectives for "Books of Fall".</summary>
        /// <param name="relicId">The ID of the relic weapon.</param>
        /// <returns>The number of objectives completed.</returns>
        public static byte NumOfFallCompleted(uint relicId)
        {
            return GetNumOfRelicNoteCompleted(relicId, RelicBookType.Fall);
        }

        /// <summary>Retrieves the number of completed objectives for "Books of Wind".</summary>
        /// <param name="relicId">The ID of the relic weapon.</param>
        /// <returns>The number of objectives completed.</returns>
        public static byte NumOfWindCompleted(uint relicId)
        {
            return GetNumOfRelicNoteCompleted(relicId, RelicBookType.Wind);
        }

        /// <summary>Retrieves the number of completed objectives for "Books of Earth".</summary>
        /// <param name="relicId">The ID of the relic weapon.</param>
        /// <returns>The number of objectives completed.</returns>
        public static byte NumOfEarthCompleted(uint relicId)
        {
            return GetNumOfRelicNoteCompleted(relicId, RelicBookType.Earth);
        }

        /// <summary>Checks if a specific book type is fully completed for the given relic ID.</summary>
        /// <param name="relicId">The ID of the relic weapon.</param>
        /// <param name="relicBookType">The category of the book.</param>
        /// <returns><see langword="true"/> if all objectives are completed; otherwise <see langword="false"/>.</returns>
        public static bool IsBookCompleted(uint relicId, RelicBookType relicBookType)
        {
            return GetNumOfRelicNoteCompleted(relicId, relicBookType) >= MaxCountByType(relicId, relicBookType);
        }

        /// <summary>
        /// Gets the total number of objectives required for a specific relic ID and book type,
        /// accounting for special cases like Paladin's Curtana and Holy Shield.
        /// </summary>
        /// <param name="relicId">The ID of the relic weapon.</param>
        /// <param name="relicBookType">The category of the book.</param>
        /// <returns>The total number of objectives to complete.</returns>
        public static byte MaxCountByType(uint relicId, RelicBookType relicBookType)
        {
            return relicId switch
            {
                7824 when relicBookType is RelicBookType.Fall or RelicBookType.Fire => 2,
                7833 when relicBookType is RelicBookType.Fall or RelicBookType.Fire => 1,
                _                                                                   => MaxCount[relicBookType]
            };
        }

        /// <summary>Checks if the "Books of Fire" are completed.</summary>
        /// <param name="relicId">The ID of the relic weapon.</param>
        /// <returns><see langword="true"/> if completed; otherwise <see langword="false"/>.</returns>
        public static bool IsFireCompleted(uint relicId)
        {
            return IsBookCompleted(relicId, RelicBookType.Fire);
        }

        /// <summary>Checks if the "Books of Fall" are completed.</summary>
        /// <param name="relicId">The ID of the relic weapon.</param>
        /// <returns><see langword="true"/> if completed; otherwise <see langword="false"/>.</returns>
        public static bool IsFallCompleted(uint relicId)
        {
            return IsBookCompleted(relicId, RelicBookType.Fall);
        }

        /// <summary>Checks if the "Books of Wind" are completed.</summary>
        /// <param name="relicId">The ID of the relic weapon.</param>
        /// <returns><see langword="true"/> if completed; otherwise <see langword="false"/>.</returns>
        public static bool IsWindCompleted(uint relicId)
        {
            return IsBookCompleted(relicId, RelicBookType.Wind);
        }

        /// <summary>Checks if the "Books of Earth" are completed.</summary>
        /// <param name="relicId">The ID of the relic weapon.</param>
        /// <returns><see langword="true"/> if completed; otherwise <see langword="false"/>.</returns>
        public static bool IsEarthCompleted(uint relicId)
        {
            return IsBookCompleted(relicId, RelicBookType.Earth);
        }

        /// <summary>
        /// Generates a localized progress string (e.g., "The Books of Fire (1/3)") for the specified relic and book type.
        /// </summary>
        /// <param name="relicId">The ID of the relic weapon.</param>
        /// <param name="relicBookType">The category of the book.</param>
        /// <returns>A formatted progress string.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the book type is invalid.</exception>
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

    /// <summary>
    /// Categorizes the different types of "Trials of the Braves" relic books.
    /// </summary>
    public enum RelicBookType : byte
    {
        /// <summary>The Books of Fire.</summary>
        Fire = 0,

        /// <summary>The Books of Fall (Water).</summary>
        Fall = 1,

        /// <summary>The Books of Wind.</summary>
        Wind = 2,

        /// <summary>The Books of Earth.</summary>
        Earth = 3
    }
}