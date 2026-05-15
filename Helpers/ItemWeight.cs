using System.Collections.Generic;
using System.Windows.Media;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using LlamaLibrary.Extensions;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Provides weighted scoring for items to help determine relative gear upgrade value for a given job.
    /// Weights are based on primary stats relevant to the job category (DoH, DoL, or combat job via
    /// <see cref="ClassItemWeightStorage"/>), and special starter/relic items are always given
    /// the highest possible weight so they are never discarded at low levels.
    /// </summary>
    public static class ItemWeight
    {
        private static readonly LLogger Log = new(nameof(ItemWeight), Colors.Aquamarine);

        /// <summary>
        /// Calculates a weight score for <paramref name="item"/> based on the attributes relevant to
        /// <paramref name="job"/>. Returns <c>-1</c> when the item is not a weapon or armour piece, or
        /// when it is not valid for the given job.
        /// Special starter / relic items (e.g., the Weathered weapons and early-level relics) return
        /// <c>5000</c> when the character is still within the level range where they represent a
        /// meaningful upgrade.
        /// </summary>
        /// <param name="item">The item to score.</param>
        /// <param name="job">
        /// The job to score for. Defaults to <see cref="ClassJobType.Adventurer"/>, which resolves to
        /// the player's current job via <see cref="Core.Me.CurrentJob"/>.
        /// </param>
        /// <returns>A non-negative weight score, or <c>-1</c> when the item is ineligible.</returns>
        public static float GetItemWeight(Item item, ClassJobType job = ClassJobType.Adventurer)
        {
            if (!MainHandsAndOffHands.Contains(item.EquipmentCatagory) && !item.IsArmor && !item.IsWeapon)
            {
                return -1f;
            }

            if (job == ClassJobType.Adventurer)
            {
                job = Core.Me.CurrentJob;
            }

            if (!item.IsValidForClass(job))
            {
                return -1f;
            }

            var level = ClassJobLevel(job);
            if ((item.Id == 2634 || item.Id == 2633) && level <= 10)
            {
                return 5000f;
            }

            if (item.Id == 8567 && level <= 25)
            {
                return 5000f;
            }

            if (item.Id == 14043 && level <= 30)
            {
                return 5000f;
            }

            if (item.Id == 16039 && level <= 50)
            {
                return 5000f;
            }

            if (item.Id == 24589 && level <= 70)
            {
                return 5000f;
            }

            float weight = 0;
            Dictionary<ItemAttribute, float> values;
            if (job.IsDoh())
            {
                values = DoHWeights;
            }
            else if (job.IsDol())
            {
                values = DoLWeights;
            }
            else
            {
                values = ClassItemWeightStorage.Instance.Values;
            }

            foreach (var itemStat in item.Attributes)
            {
                if (values.TryGetValue(itemStat.Key, out var value))
                {
                    weight += itemStat.Value * value;
                }
            }

            return weight;
        }

        /// <summary>
        /// Attribute weights used for Disciples of the Hand jobs
        /// (Craftsmanship, Control, and CP each weighted equally at 1.0).
        /// </summary>
        private static readonly Dictionary<ItemAttribute, float> DoHWeights = new()
        {
            { ItemAttribute.Craftsmanship, 1 },
            { ItemAttribute.Control, 1 },
            { ItemAttribute.CP, 1 }
        };

        /// <summary>
        /// Attribute weights used for Disciples of the Land jobs
        /// (Gathering, Perception, and GP each weighted equally at 1.0).
        /// </summary>
        private static readonly Dictionary<ItemAttribute, float> DoLWeights = new()
        {
            { ItemAttribute.Gathering, 1 },
            { ItemAttribute.Perception, 1 },
            { ItemAttribute.GP, 1 }
        };

        /// <summary>
        /// Returns the current level of the player for <paramref name="job"/>.
        /// For advanced jobs (e.g., <see cref="ClassJobType.Paladin"/>) that do not have a direct
        /// level entry, the level of the corresponding base class is returned instead
        /// (e.g., <see cref="ClassJobType.Gladiator"/> for Paladin).
        /// </summary>
        /// <param name="job">The job whose level to retrieve.</param>
        /// <returns>The character's current level for the job, or <c>1</c> on failure.</returns>
        public static ushort ClassJobLevel(ClassJobType job)
        {
            try
            {
                return Core.Me.Levels[job];
            }
            catch (KeyNotFoundException)
            {
                switch (job)
                {
                    case ClassJobType.Paladin:
                        return Core.Me.Levels[ClassJobType.Gladiator];
                    case ClassJobType.Monk:
                        return Core.Me.Levels[ClassJobType.Pugilist];
                    case ClassJobType.Warrior:
                        return Core.Me.Levels[ClassJobType.Marauder];
                    case ClassJobType.Dragoon:
                        return Core.Me.Levels[ClassJobType.Lancer];
                    case ClassJobType.Bard:
                        return Core.Me.Levels[ClassJobType.Archer];
                    case ClassJobType.WhiteMage:
                        return Core.Me.Levels[ClassJobType.Conjurer];
                    case ClassJobType.BlackMage:
                        return Core.Me.Levels[ClassJobType.Thaumaturge];
                    case ClassJobType.Scholar:
                        return Core.Me.Levels[ClassJobType.Arcanist];
                    case ClassJobType.Summoner:
                        return Core.Me.Levels[ClassJobType.Arcanist];
                    case ClassJobType.Ninja:
                        return Core.Me.Levels[ClassJobType.Rogue];
                    default:
                        Log.Error($"Couldn't find level for {job}.");
                        return 1;
                }
            }
        }

        /*
        private static List<ItemUiCategory> MainHands => (from itemUi in (ItemUiCategory[]) Enum.GetValues(typeof(ItemUiCategory))
                                                          let name = Enum.GetName(typeof(ItemUiCategory), itemUi)
                                                          where name != null
                                                          where name.EndsWith("_Arm") || name.EndsWith("_Arms") || name.EndsWith("_Primary_Tool") || name.EndsWith("_Grimoire") select itemUi)
                                                         .ToList();

        private static List<ItemUiCategory> OffHands => (from itemUi in (ItemUiCategory[]) Enum.GetValues(typeof(ItemUiCategory))
                                                         let name = Enum.GetName(typeof(ItemUiCategory), itemUi)
                                                         where name != null
                                                         where name.Equals("Shield") || name.EndsWith("_Secondary_Tool") select itemUi)
                                                        .ToList();
        */

        /// <summary>
        /// All main-hand and off-hand weapon/tool <see cref="ItemUiCategory"/> values recognised by the weight system.
        /// Items whose <see cref="Item.EquipmentCatagory"/> is not in this list (and that are not armour)
        /// receive a weight of <c>-1</c> and are ignored.
        /// </summary>
        public static readonly List<ItemUiCategory> MainHandsAndOffHands = new()
        {
            ItemUiCategory.Pugilists_Arm,
            ItemUiCategory.Gladiators_Arm,
            ItemUiCategory.Marauders_Arm,
            ItemUiCategory.Archers_Arm,
            ItemUiCategory.Lancers_Arm,
            ItemUiCategory.One_handed_Thaumaturges_Arm,
            ItemUiCategory.Two_handed_Thaumaturges_Arm,
            ItemUiCategory.One_handed_Conjurers_Arm,
            ItemUiCategory.Two_handed_Conjurers_Arm,
            ItemUiCategory.Arcanists_Grimoire,
            ItemUiCategory.Shield,
            ItemUiCategory.Carpenters_Primary_Tool,
            ItemUiCategory.Carpenters_Secondary_Tool,
            ItemUiCategory.Blacksmiths_Primary_Tool,
            ItemUiCategory.Blacksmiths_Secondary_Tool,
            ItemUiCategory.Armorers_Primary_Tool,
            ItemUiCategory.Armorers_Secondary_Tool,
            ItemUiCategory.Goldsmiths_Primary_Tool,
            ItemUiCategory.Goldsmiths_Secondary_Tool,
            ItemUiCategory.Leatherworkers_Primary_Tool,
            ItemUiCategory.Leatherworkers_Secondary_Tool,
            ItemUiCategory.Weavers_Primary_Tool,
            ItemUiCategory.Weavers_Secondary_Tool,
            ItemUiCategory.Alchemists_Primary_Tool,
            ItemUiCategory.Alchemists_Secondary_Tool,
            ItemUiCategory.Culinarians_Primary_Tool,
            ItemUiCategory.Culinarians_Secondary_Tool,
            ItemUiCategory.Miners_Primary_Tool,
            ItemUiCategory.Miners_Secondary_Tool,
            ItemUiCategory.Botanists_Primary_Tool,
            ItemUiCategory.Botanists_Secondary_Tool,
            ItemUiCategory.Fishers_Primary_Tool,
            ItemUiCategory.Rogues_Arms,
            ItemUiCategory.Dark_Knights_Arm,
            ItemUiCategory.Machinists_Arm,
            ItemUiCategory.Astrologians_Arm,
            ItemUiCategory.Samurais_Arm,
            ItemUiCategory.Red_Mages_Arm,
            ItemUiCategory.Scholars_Arm,
            ItemUiCategory.Fishers_Secondary_Tool,
            ItemUiCategory.Blue_Mages_Arm,
            ItemUiCategory.Gunbreakers_Arm,
            ItemUiCategory.Dancers_Arm
        };

        /// <summary>Main-hand-only weapon and primary-tool categories (excludes shields and secondary tools).</summary>
        public static readonly List<ItemUiCategory> MainHands = new()
        {
            ItemUiCategory.Pugilists_Arm,
            ItemUiCategory.Gladiators_Arm,
            ItemUiCategory.Marauders_Arm,
            ItemUiCategory.Archers_Arm,
            ItemUiCategory.Lancers_Arm,
            ItemUiCategory.One_handed_Thaumaturges_Arm,
            ItemUiCategory.Two_handed_Thaumaturges_Arm,
            ItemUiCategory.One_handed_Conjurers_Arm,
            ItemUiCategory.Two_handed_Conjurers_Arm,
            ItemUiCategory.Arcanists_Grimoire,
            ItemUiCategory.Carpenters_Primary_Tool,
            ItemUiCategory.Blacksmiths_Primary_Tool,
            ItemUiCategory.Armorers_Primary_Tool,
            ItemUiCategory.Goldsmiths_Primary_Tool,
            ItemUiCategory.Leatherworkers_Primary_Tool,
            ItemUiCategory.Weavers_Primary_Tool,
            ItemUiCategory.Alchemists_Primary_Tool,
            ItemUiCategory.Culinarians_Primary_Tool,
            ItemUiCategory.Miners_Primary_Tool,
            ItemUiCategory.Botanists_Primary_Tool,
            ItemUiCategory.Fishers_Primary_Tool,
            ItemUiCategory.Rogues_Arms,
            ItemUiCategory.Dark_Knights_Arm,
            ItemUiCategory.Machinists_Arm,
            ItemUiCategory.Astrologians_Arm,
            ItemUiCategory.Samurais_Arm,
            ItemUiCategory.Red_Mages_Arm,
            ItemUiCategory.Scholars_Arm,
            ItemUiCategory.Blue_Mages_Arm,
            ItemUiCategory.Gunbreakers_Arm,
            ItemUiCategory.Dancers_Arm
        };

        /// <summary>Off-hand weapon and secondary-tool categories (shields and all crafting/gathering secondary tools).</summary>
        public static readonly List<ItemUiCategory> OffHands = new()
        {
            ItemUiCategory.Shield,
            ItemUiCategory.Carpenters_Secondary_Tool,
            ItemUiCategory.Blacksmiths_Secondary_Tool,
            ItemUiCategory.Armorers_Secondary_Tool,
            ItemUiCategory.Goldsmiths_Secondary_Tool,
            ItemUiCategory.Leatherworkers_Secondary_Tool,
            ItemUiCategory.Weavers_Secondary_Tool,
            ItemUiCategory.Alchemists_Secondary_Tool,
            ItemUiCategory.Culinarians_Secondary_Tool,
            ItemUiCategory.Miners_Secondary_Tool,
            ItemUiCategory.Botanists_Secondary_Tool,
            ItemUiCategory.Fishers_Secondary_Tool
        };
    }
}