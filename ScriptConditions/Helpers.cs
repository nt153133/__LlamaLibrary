using System;
using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Directors;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.RemoteWindows;
using LlamaLibrary.Extensions;
using LlamaLibrary.Helpers;
using LlamaLibrary.Memory;
using LlamaLibrary.RemoteAgents;
using Character = LlamaLibrary.RemoteWindows.Character;

namespace LlamaLibrary.ScriptConditions
{
    /// <summary>
    /// Provides helper condition-check methods for use in bot profiles and scripts.
    /// Covers inventory checks (Ishgard, rarefied items), currency counts, item levels, relic progress, and Bozja metrics.
    /// </summary>
    public static class Helpers
    {
        private static readonly uint[] IdList =
        {
            28725, 28733, 28741, 28749, 28757, 29792, 29800, 29808, 29816, 29824, 29832, 31184, 31192, 31200, 31208, 31216, 31224, 31913, 31921, 31929, 31937, 31945, 31953, 28726, 28734, 28742, 28750, 28758, 29793, 29801, 29809, 29817, 29825, 29833, 31185, 31193, 31201, 31209, 31217, 31225, 31914, 31922, 31930, 31938, 31946, 31954, 28727, 28735, 28743, 28751, 28759, 29794, 29802, 29810, 29818, 29826, 29834, 31186, 31194, 31202, 31210, 31218, 31226, 31915, 31923, 31931, 31939,
            31947, 31955, 28728, 28736, 28744, 28752, 28760, 29795, 29803, 29811, 29819, 29827, 29835, 31187, 31195, 31203, 31211, 31219, 31227, 31916, 31924, 31932, 31940, 31948, 31956, 28729, 28737, 28745, 28753, 28761, 29796, 29804, 29812, 29820, 29828, 29836, 31188, 31196, 31204, 31212, 31220, 31228, 31917, 31925, 31933, 31941, 31949, 31957, 28730, 28738, 28746, 28754, 28762, 29797, 29805, 29813, 29821, 29829, 29837, 31189, 31197, 31205, 31213, 31221, 31229, 31918, 31926,
            31934, 31942, 31950, 31958, 28731, 28739, 28747, 28755, 28763, 29798, 29806, 29814, 29822, 29830, 29838, 31190, 31198, 31206, 31214, 31222, 31230, 31919, 31927, 31935, 31943, 31951, 31959, 28732, 28740, 28748, 28756, 28764, 29799, 29807, 29815, 29823, 29831, 29839, 31191, 31199, 31207, 31215, 31223, 31231, 31920, 31928, 31936, 31944, 31952, 31960
        };

        private static readonly uint[] IdList0 =
        {
            29896, 29897, 29901, 29902, 29903, 29909, 29910, 29911, 29912, 29913, 29919, 29920, 29921, 29922, 29923, 29929, 29930, 29931, 29932, 29933, 29939, 29940, 29941, 29942, 29943, 29946, 29947, 31278, 31279, 31283, 31284, 31285, 31291, 31292, 31293, 31294, 31295, 31301, 31302, 31303, 31304, 31305, 31311, 31312, 31313, 31314, 31315, 31318, 31319, 32007, 32008, 32012, 32013, 32014, 32020, 32021, 32022, 32023, 32024, 32030, 32031, 32032, 32033, 32034, 32040, 32041, 32042,
            32043, 32044, 32047, 32048
        };

        private static readonly uint[] IdList1 =
        {
            29894, 29895, 29898, 29899, 29900, 29904, 29905, 29906, 29907, 29908, 29914, 29915, 29916, 29917, 29918, 29924, 29925, 29926, 29927, 29928, 29934, 29935, 29936, 29937, 29938, 29944, 29945, 31276, 31277, 31280, 31281, 31282, 31286, 31287, 31288, 31289, 31290, 31296, 31297, 31298, 31299, 31300, 31306, 31307, 31308, 31309, 31310, 31316, 31317, 32005, 32006, 32009, 32010, 32011, 32015, 32016, 32017, 32018, 32019, 32025, 32026, 32027, 32028, 32029, 32035, 32036, 32037,
            32038, 32039, 32045, 32046
        };

        private static readonly uint[] IdList2 =
        {
            29994, 29995, 29996, 29997, 29998, 29999, 30000, 30001, 30002, 30003, 30004, 30005, 30008, 30009, 30006, 30007, 30010, 30011, 30012, 30013, 31578, 31579, 31580, 31581, 31582, 31583, 31584, 31585, 31586, 31587, 31588, 31589, 31590, 31591, 31592, 31593, 31594, 31595, 31596, 31597, 31598, 31599, 31600, 31601, 31602, 31603, 32882, 32883, 32884, 32885, 32886, 32887, 32888, 32889, 32890, 32891, 32892, 32893, 32894, 32895, 32896, 32897, 32898, 32899, 32900, 32901, 32902,
            32903, 32904, 32905, 32906, 32907
        };

        private static readonly uint[] IdList3 =
		{
			35626, 35627, 35628, 35629, 35630, 35631, 35632, 35633, 35634, 35635, 35636, 35637, 35638, 35639, 35640, 35641, 35642, 35643, 35644, 35645, 35646, 35647, 35648, 35649, 35650, 35651, 35652, 35653, 35654, 35655, 35656, 35657, 35658, 35659, 35660, 35661, 35662, 35663, 35664, 35665,
			44185, 44186, 44187, 44188, 44189, 44190, 44191, 44192, 44193, 44194, 44195, 44196, 44197, 44198, 44199, 44200, 44201, 44202, 44203, 44204, 44205, 44206, 44207, 44208, 44209, 44210, 44211, 44212, 44213, 44214, 44215, 44216, 44217, 44218, 44219, 44220, 44221, 44222, 44223, 44224, 44225, 44226, 44227, 44228, 44229, 44230, 44231, 44232
		};

        private static bool hasLisbeth;
        private static bool checkedLisbeth;

        /// <summary>
        /// Counts the number of Ishgard Restoration turn-in items in the player's inventory with collectability over 50.
        /// </summary>
        /// <returns>The count of matching items.</returns>
        public static int HasIshgardItem()
        {
            return InventoryManager.FilledSlots.Count(i => IdList.Contains(i.RawItemId) && i.IsCollectable && i.Collectability > 50);
        }

        /// <summary>
        /// Counts the number of Rarefied (collectable) items in the player's inventory with collectability over 50.
        /// </summary>
        /// <returns>The count of matching items.</returns>
        public static int HasRarefiedItem()
        {
            return InventoryManager.FilledSlots.Count(i => IdList3.Contains(i.RawItemId) && i.IsCollectable && i.Collectability > 50);
        }

        /// <summary>
        /// Checks if the player's client language is set to Chinese.
        /// </summary>
        /// <returns><see langword="true"/> if Chinese; otherwise <see langword="false"/>.</returns>
        public static bool IsCNClient()
        {
            return Translator.Language == Language.Chn;
        }

        /// <summary>
        /// Checks if the chocobo companion has enough skill points to learn the next skill in its current role.
        /// </summary>
        /// <returns><see langword="true"/> when the current companion role can rank up; otherwise <see langword="false"/>.</returns>
        public static bool HasEnoughSP()
        {
            return LlamaLibrary.RemoteWindows.BuddySkill.Instance.CanLearnNextSkillForActiveRole();
        }

        /// <summary>
        /// Checks if the player has at least 10 Ishgard Restoration mining materials in their inventory.
        /// </summary>
        /// <returns><see langword="true"/> if possessed; otherwise <see langword="false"/>.</returns>
        public static bool HasIshgardGatheringMining()
        {
            return InventoryManager.FilledSlots.Any(i => IdList0.Contains(i.RawItemId) && i.Count >= 10);
        }

        /// <summary>
        /// Checks if the player has at least 10 Ishgard Restoration botany materials in their inventory.
        /// </summary>
        /// <returns><see langword="true"/> if possessed; otherwise <see langword="false"/>.</returns>
        public static bool HasIshgardGatheringBotanist()
        {
            return InventoryManager.FilledSlots.Any(i => IdList1.Contains(i.RawItemId) && i.Count >= 10);
        }

        /// <summary>
        /// Checks if the player has at least 1 Ishgard Restoration fishing material in their inventory.
        /// </summary>
        /// <returns><see langword="true"/> if possessed; otherwise <see langword="false"/>.</returns>
        public static bool HasIshgardGatheringFisher()
        {
            return InventoryManager.FilledSlots.Any(i => IdList2.Contains(i.RawItemId) && i.Count >= 1);
        }

        /// <summary>
        /// Checks if the player has at least one Normal Quality (NQ) copy of the specified item.
        /// </summary>
        /// <param name="itemID">The numeric ID of the item.</param>
        /// <returns><see langword="true"/> if an NQ copy is found; otherwise <see langword="false"/>.</returns>
        public static bool LLHasItemNQ(int itemID)
        {
            return InventoryManager.FilledSlots.Count(i => i.RawItemId == itemID && i.IsHighQuality == false) >= 1;
        }

        /// <summary>
        /// Checks if the player has at least one High Quality (HQ) copy of the specified item.
        /// </summary>
        /// <param name="itemID">The numeric ID of the item.</param>
        /// <returns><see langword="true"/> if an HQ copy is found; otherwise <see langword="false"/>.</returns>
        public static bool LLHasItemHQ(int itemID)
        {
            return InventoryManager.FilledSlots.Count(i => i.RawItemId == itemID && i.IsHighQuality) >= 1;
        }

        /// <summary>
        /// Checks if the player has at least one collectable copy of the specified item.
        /// </summary>
        /// <param name="itemID">The numeric ID of the item.</param>
        /// <returns><see langword="true"/> if a collectable copy is found; otherwise <see langword="false"/>.</returns>
        public static bool LLHasItemCollectable(int itemID)
        {
            return InventoryManager.FilledSlots.Count(i => i.RawItemId == itemID && i.IsCollectable) >= 1;
        }

        /// <summary>
        /// Gets the player's current number of Skybuilders' Scrips.
        /// </summary>
        /// <returns>The count of scrips.</returns>
        public static int GetSkybuilderScrips()
        {
            return (int)SpecialCurrencyManager.GetCurrencyCount(SpecialCurrency.SkybuildersScrips);
        }

        /// <summary>
        /// Checks if the player currently has any Timeworn Map in their inventory.
        /// </summary>
        /// <returns><see langword="true"/> if a map is possessed; otherwise <see langword="false"/>.</returns>
        public static bool HasMap()
        {
            return ActionHelper.HasMap();
        }

        /// <summary>
        /// Calculates the average item level of all currently equipped gear.
        /// </summary>
        /// <returns>The average item level as an integer.</returns>
        public static int AverageItemLevel()
        {
            return InventoryManager.EquippedItems.Where(k => k.IsFilled).Sum(i => i.Item.ItemLevel) / InventoryManager.EquippedItems.Count(k => k.IsFilled);
        }

        /// <summary>
        /// Returns the player's current effective item level, read from the ActorController.
        /// </summary>
        /// <returns>The item level as an integer.</returns>
        public static int CurrentItemLevel()
        {
            return Core.Memory.Read<ushort>(Offsets.ActorController_iLvl);
        }

        /// <summary>
        /// Returns the spirit bond percentage of the player's MainHand weapon, often used to track Novus light progress.
        /// </summary>
        /// <returns>The spirit bond percentage (0-100).</returns>
        public static int NovusLightLevel()
        {
            return (int)(InventoryManager.EquippedItems.First().SpiritBond * 100);
        }

        /// <summary>
        /// Returns the spirit bond percentage of the player's MainHand weapon, used to track Zodiac light progress.
        /// </summary>
        /// <returns>The spirit bond percentage (0-100).</returns>
        public static int ZodiacLightLevel()
        {
            return (int)(InventoryManager.EquippedItems.First().SpiritBond * 100);
        }

        /// <summary>
        /// Returns the number of Mahatma steps completed for the Zodiac weapon, derived from spirit bond.
        /// </summary>
        /// <returns>The number of completed Mahatma steps.</returns>
        public static int ZodiacCompletedMahatma()
        {
            return ZodiacLightLevel() / 500;
        }

        /// <summary>
        /// Returns the progress (0-499) within the current Mahatma step for the Zodiac weapon.
        /// </summary>
        /// <returns>The current step progress.</returns>
        public static int ZodiacMahatmaProgress()
        {
            return ZodiacLightLevel() % 500;
        }

        /// <summary>
        /// Determines if the current Mahatma step for the Zodiac weapon is complete (progress reached 80).
        /// </summary>
        /// <returns><see langword="true"/> if complete; otherwise <see langword="false"/>.</returns>
        public static bool ZodiacMahatmaIsDone()
        {
            return (ZodiacLightLevel() % 500) == 80;
        }

        /// <summary>
        /// Gets the index (1-12) of the current Mahatma being infused.
        /// </summary>
        /// <returns>The Mahatma index.</returns>
        public static int ZodiacCurrent()
        {
            return ZodiacCompletedMahatma() + 1;
        }

        /// <summary>
        /// Returns the current aetheric density for the Anima weapon progress, read from memory.
        /// </summary>
        /// <returns>The density value as an integer.</returns>
        public static int AethericDensity()
        {
            return Core.Memory.Read<int>(Offsets.AnimaLightThing + Offsets.AnimaLight);
        }

        /// <summary>
        /// Checks if the current game version is greater than or equal to the specified version.
        /// </summary>
        /// <param name="version">The version number to compare against.</param>
        /// <returns><see langword="true"/> if version is greater or equal; otherwise <see langword="false"/>.</returns>
        public static bool IsVersionGreater(float version)
        {
            if (OffsetManager.ActiveRecord.CurrentGameVersion >= version)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves the progress value (Item1) for a specific objective index in the current duty instance.
        /// </summary>
        /// <param name="objective">The zero-based objective index.</param>
        /// <returns>The progress value, or -1 if not in a duty.</returns>
        public static int GetInstanceTodo(int objective)
        {
            if (DirectorManager.ActiveDirector is InstanceContentDirector activeAsInstance)
            {
                return activeAsInstance.GetTodoArgs(objective).Item1;
            }

            return -1;
        }

        /// <summary>
        /// Checks if any member of the current party (other than the local player) is alive and in combat.
        /// </summary>
        /// <returns><see langword="true"/> if party is in combat; otherwise <see langword="false"/>.</returns>
        public static bool IsPartyInCombat()
        {
            return PartyManager.VisibleMembers.Any(x => !x.IsMe && x.BattleCharacter.IsAlive && x.BattleCharacter.InCombat);
        }

        /// <summary>
        /// Gets the current count of Grand Company seals for the player's active Grand Company.
        /// </summary>
        /// <returns>The number of seals possessed.</returns>
        public static int CurrentGCSeals()
        {
            uint[] sealTypes = { 20, 21, 22 };
            var bagslot = InventoryManager.GetBagByInventoryBagId(InventoryBagId.Currency).FirstOrDefault(i => i.RawItemId == sealTypes[(int)Core.Me.GrandCompany - 1]);
            return (int)(bagslot?.Count ?? 0U);
        }

        /// <summary>
        /// Gets the maximum capacity of Grand Company seals for the local player.
        /// </summary>
        /// <returns>The seal limit.</returns>
        public static int MaxGCSeals()
        {
            return Core.Me.MaxGCSeals();
        }

        /// <summary>
        /// Retrieves the overhead icon ID for the specified NPC.
        /// </summary>
        /// <param name="npcID">The numeric ID of the NPC.</param>
        /// <returns>The icon ID, or 0 if NPC is not found.</returns>
        public static int GetNPCIconId(int npcID)
        {
            var npc = GameObjectManager.GetObjectByNPCId((uint)npcID);
            if (npc != null)
            {
                return (int)npc.IconId();
            }

            return 0;
        }

        /// <summary>
        /// Gets the total amount of Gil currently held in the player's currency bag.
        /// </summary>
        /// <returns>The amount of Gil.</returns>
        public static int GilCount()
        {
            return (int)(InventoryManager.GetBagByInventoryBagId(InventoryBagId.Currency).Where(r => r.IsFilled).FirstOrDefault(item => item.RawItemId == DataManager.GetItem("Gil").Id)?.Count ?? 0);
        }

        /// <summary>
        /// Checks if the remaining time in the current dungeon instance is greater than the specified milliseconds.
        /// </summary>
        /// <param name="time">The time threshold in milliseconds.</param>
        /// <returns><see langword="true"/> if more time remains; otherwise <see langword="false"/>.</returns>
        public static bool MsLeftInDungeonGt(long time)
        {
            if (DirectorManager.ActiveDirector == null)
            {
                return false;
            }

            if (DirectorManager.ActiveDirector is InstanceContentDirector activeAsInstance)
            {
                return activeAsInstance.TimeLeftInDungeon.Milliseconds > time;
            }

            return false;
        }

        /// <summary>
        /// Gets the player's current Mettle amount in Bozja/Zadnor instances.
        /// </summary>
        /// <returns>The current Mettle value.</returns>
        public static int CurrentMettle()
        {
            if (DirectorManager.ActiveDirector == null)
            {
                return 0;
            }

            return (int)Core.Memory.Read<uint>(DirectorManager.ActiveDirector.Pointer + Offsets.CurrentMettle);
        }

        /// <summary>
        /// Checks if any Dynamic Event (Critical Engagement or Skirmish) is currently active.
        /// </summary>
        /// <returns><see langword="true"/> if an event is active; otherwise <see langword="false"/>.</returns>
        public static bool IsAnyDynamicEventActive()
        {
            if (DynamicEventManager.Events == null)
            {
                return false;
            }

            if (DynamicEventManager.Events.Any(e => e.IsActive))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the amount of Mettle required for the next Resistance Rank in Bozja/Zadnor.
        /// </summary>
        /// <returns>The required Mettle value.</returns>
        public static int NextResistanceRank()
        {
            if (DirectorManager.ActiveDirector == null)
            {
                return 0;
            }

            return (int)Core.Memory.Read<uint>(DirectorManager.ActiveDirector.Pointer + Offsets.NextReistanceRank);
        }

        /// <summary>
        /// Determines if the specified duty is currently unlocked and available in the Duty Finder.
        /// </summary>
        /// <param name="duty">The numeric ID of the duty.</param>
        /// <returns><see langword="true"/> if available; otherwise <see langword="false"/>.</returns>
        public static bool IsDutyAvailable(int duty)
        {
            return GeneralFunctions.IsDutyUnlocked((uint)duty);
        }

        /// <summary>
        /// Checks if the player has unlocked the specified secret recipe book (Master Recipe Tome).
        /// </summary>
        /// <param name="tomeId">The item ID of the tome.</param>
        /// <returns><see langword="true"/> if unlocked; otherwise <see langword="false"/>.</returns>
        public static bool IsSecretRecipeBookUnlocked(int tomeId)
        {
            return CraftingHelper.IsFolkloreBookUnlockedItem((uint)tomeId);
        }

        /// <summary>
        /// Detects whether the Dalamud plugin framework is active in the current process.
        /// </summary>
        /// <returns><see langword="true"/> if detected; otherwise <see langword="false"/>.</returns>
        public static bool DalamudDetected()
        {
            return GeneralFunctions.DalamudDetected();
        }

        /// <summary>
        /// Checks if the player has unlocked the specified Folklore gathering book.
        /// </summary>
        /// <param name="tomeId">The item ID of the tome.</param>
        /// <returns><see langword="true"/> if unlocked; otherwise <see langword="false"/>.</returns>
        public static bool IsFolkloreBookUnlocked(int tomeId)
        {
            return CraftingHelper.IsFolkloreBookUnlockedItem((uint)tomeId);
        }

        /// <summary>
        /// Checks if the player has ever completed the specified duty instance.
        /// </summary>
        /// <param name="duty">The numeric ID of the duty.</param>
        /// <returns><see langword="true"/> if completed; otherwise <see langword="false"/>.</returns>
        public static bool HasDutyBeenCompleted(int duty)
        {
            return GeneralFunctions.IsDutyComplete((uint)duty);
        }

        /*public static int GetLeveTodoArgsItem1(int index)
        {
            if (DirectorManager.ActiveDirector == null) return -1;
            var type = DirectorManager.ActiveDirector.GetType();

            if (type == typeof(ff14bot.Directors.BattleLeveConciliate))
                return (DirectorManager.ActiveDirector as BattleLeveConciliate).GetTodoArgs(index).Item1;
            if (type == typeof(ff14bot.Directors.BattleLeveSweep))
                return (DirectorManager.ActiveDirector as BattleLeveSweep).GetTodoArgs(index).Item1;
            if (type == typeof(ff14bot.Directors.BattleLeveDetect))
                return (DirectorManager.ActiveDirector as BattleLeveDetect).GetTodoArgs(index).Item1;
            if (type == typeof(ff14bot.Directors.BattleLeveGuide))
                return (DirectorManager.ActiveDirector as BattleLeveGuide).GetTodoArgs(index).Item1;

            DirectorManager.ActiveDirector.GetTodoArgs(index)
        }*/

        /// <summary>
        /// Opens the Character window and saves the current equipment loadout as the active gear set.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, returning <see langword="true"/> if successful.</returns>
        public static async Task<bool> UpdateGearSet()
        {
            if (!Character.Instance.IsOpen)
            {
                //Logger.Info("Character window not open");
                AgentCharacter.Instance.Toggle();

                //Logger.Info("Toggled");
                try
                {
                    await Coroutine.Wait(10000, () => Character.Instance.IsOpen);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            if (!Character.Instance.IsOpen)
            {
                return false;
            }

            //Logger.Info($"Can click {Character.Instance.CanUpdateGearSet()}");
            if (!Character.Instance.CanUpdateGearSet())
            {
                Character.Instance.Close();
                return false;
            }

            Character.Instance.UpdateGearSet();

            try
            {
                await Coroutine.Wait(10000, () => SelectYesno.IsOpen);
            }
            catch (Exception)
            {
                if (Character.Instance.IsOpen)
                {
                    Character.Instance.Close();
                }

                return true;
            }

            if (SelectYesno.IsOpen)
            {
                SelectYesno.Yes();
            }

            try
            {
                await Coroutine.Wait(10000, () => !SelectYesno.IsOpen);
            }
            catch (Exception)
            {
                return true;
            }

            //await Coroutine.Sleep(200);

            if (Character.Instance.IsOpen)
            {
                Character.Instance.Close();
            }

            return true;
        }

        /// <summary>
        /// Asynchronously waits for a condition to become <see langword="true"/>, checking at the specified frequency.
        /// </summary>
        /// <param name="condition">The function to evaluate.</param>
        /// <param name="frequency">The delay between checks in milliseconds.</param>
        /// <param name="timeout">The maximum time to wait in milliseconds. Pass -1 for infinite.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="TimeoutException">Thrown if the timeout is reached before the condition is met.</exception>
        public static async Task WaitUntil(Func<bool> condition, int frequency = 25, int timeout = -1)
        {
            var waitTask = Task.Run(async () =>
            {
                while (!condition())
                {
                    await Task.Delay(frequency);
                }
            });

            if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeout)))
            {
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Calculates the squared distance from the player to the specified coordinate string.
        /// </summary>
        /// <param name="loc">A string representation of a Vector3 (e.g. "100, 20, -300").</param>
        /// <returns>The squared distance as an integer.</returns>
        public static int DistanceSqrTo(string loc)
        {
            return (int)Core.Me.Location.DistanceSqr(new Vector3(loc));
        }

        /// <summary>
        /// Checks if the Lisbeth plugin is currently present (cached value).
        /// </summary>
        /// <returns><see langword="true"/> if Lisbeth was found; otherwise <see langword="false"/>.</returns>
        public static bool HasLisbeth()
        {
            return hasLisbeth;
        }

        /// <summary>
        /// Asynchronously verifies if Lisbeth is loaded and updates the cached <see cref="HasLisbeth"/> value.
        /// </summary>
        /// <returns>A <see cref="Task"/> returning <see langword="true"/> if Lisbeth is found.</returns>
        public static async Task<bool> CheckLisbeth()
        {
            if (checkedLisbeth)
            {
                return hasLisbeth;
            }

            hasLisbeth = await Lisbeth.HasLisbeth();
            checkedLisbeth = true;
            return hasLisbeth;
        }

        /// <summary>
        /// Synchronously checks if the player possesses the specified item across all storage systems (Inventory, Retainers, Saddlebags, Dresser).
        /// </summary>
        /// <param name="itemId">The numeric item ID.</param>
        /// <returns><see langword="true"/> if possessed; otherwise <see langword="false"/>.</returns>
        public static bool HasItemAnywhere(int itemId)
        {
            return UIState.HasItemSync((uint)itemId);
        }

        /// <summary>
        /// Asynchronously checks if the player possesses the specified item, forcing a glamour dresser refresh if needed.
        /// </summary>
        /// <param name="itemId">The numeric item ID.</param>
        /// <returns>A <see cref="Task"/> returning <see langword="true"/> if possessed.</returns>
        public static async Task<bool> HasItemCheck(int itemId)
        {
            return await UIState.HasItem((uint)itemId, true);
        }

        /// <summary>
        /// Checks if the player currently has a minion summoned.
        /// </summary>
        /// <returns><see langword="true"/> if a minion is summoned; otherwise <see langword="false"/>.</returns>
        public static bool IsMinionSummoned()
        {
            return MinionHelper.IsMinionSummoned;
        }

        /// <summary>
        /// Checks if the player has unlocked the specified minion.
        /// </summary>
        /// <param name="itemId">The numeric ID of the minion.</param>
        /// <returns><see langword="true"/> if unlocked; otherwise <see langword="false"/>.</returns>
        public static bool IsMinionUnlocked(int itemId)
        {
            return UIState.MinionUnlocked(itemId);
        }

        /// <summary>
        /// Gets the NPC ID of the player's currently summoned minion.
        /// </summary>
        /// <returns>The minion's NPC ID, or 0 if no minion is summoned.</returns>
        public static int MinionId()
        {
            if (!MinionHelper.IsMinionSummoned)
            {
                return 0;
            }

            return MinionHelper.MinionId;
        }
    }
}
