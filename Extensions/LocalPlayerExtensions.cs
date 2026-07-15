using System;
using System.Linq;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Objects;
using LlamaLibrary.Enums;
using LlamaLibrary.Helpers.NPC;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="LocalPlayer"/> and related types.
    /// Includes utilities for state checking, currency retrieval, and gearset management.
    /// </summary>
    public static class LocalPlayerExtensions
    {
        /// <summary>
        /// Zone ID for 'The Endeavor' (Ocean Fishing boat).
        /// </summary>
        public const uint TheEndeavor = 900;

        /// <summary>
        /// Zone ID for 'The Endeavor' (Ocean Fishing boat) during the Ruby route.
        /// </summary>
        public const uint TheEndeaverRuby = 1163;

        /// <summary>
        /// Gets a value indicating whether the player is currently in the 'Walk' state (as opposed to 'Run').
        /// </summary>
        public static bool IsWalking => Core.Memory.Read<byte>(LocalPlayerExtensionsOffsets.RunWalk) == 1;

        /// <summary>
        /// Checks if the local player is currently fishing.
        /// </summary>
        /// <param name="play">The local player instance.</param>
        /// <returns><see langword="true"/> if the player is fishing; otherwise <see langword="false"/>.</returns>
        public static bool IsFishing(this LocalPlayer play)
        {
            return FishingManager.State != FishingState.None;
        }

        /// <summary>
        /// Checks if the local player is currently on the Ocean Fishing boat.
        /// </summary>
        /// <param name="play">The local player instance.</param>
        /// <returns><see langword="true"/> if the player is in one of the ocean fishing zones; otherwise <see langword="false"/>.</returns>
        public static bool IsOnFishingBoat(this LocalPlayer play)
        {
            return WorldManager.RawZoneId == TheEndeavor || WorldManager.RawZoneId == TheEndeaverRuby;
        }

        /// <summary>
        /// Returns a <see cref="Location"/> object representing the player's current zone and coordinates.
        /// </summary>
        /// <param name="play">The local player instance.</param>
        /// <returns>A <see cref="Location"/> containing the current zone ID and position.</returns>
        public static Location Location(this LocalPlayer play)
        {
            return new Location(WorldManager.ZoneId, Core.Me.Location);
        }

        /*internal static byte GatheringStatus(this LocalPlayer player)
        {
            return Core.Memory.Read<byte>(player.Pointer + LocalPlayerExtensionsOffsets.GatheringStateOffset);
        }*/

        /// <summary>
        /// Gets the memory address (pointer) of the currently summoned minion.
        /// </summary>
        /// <param name="play">The local player instance.</param>
        /// <returns>The <see cref="IntPtr"/> to the minion object in memory.</returns>
        public static IntPtr MinionPtr(this LocalPlayer play)
        {
            return Core.Memory.Read<IntPtr>(play.Pointer + LocalPlayerExtensionsOffsets.MinionPtr);
        }

        /// <summary>
        /// Sets the player's movement mode to 'Run'.
        /// </summary>
        /// <param name="play">The local player instance.</param>
        public static void SetRun(this LocalPlayer play)
        {
            if (IsWalking)
            {
                Core.Memory.Write<byte>(LocalPlayerExtensionsOffsets.RunWalk, 0);
            }
        }

        /// <summary>
        /// Sets the player's movement mode to 'Walk'.
        /// </summary>
        /// <param name="play">The local player instance.</param>
        public static void SetWalk(this LocalPlayer play)
        {
            if (!IsWalking)
            {
                Core.Memory.Write<byte>(LocalPlayerExtensionsOffsets.RunWalk, 1);
            }
        }

        /// <summary>
        /// Gets the player's current Grand Company rank.
        /// </summary>
        /// <param name="player">The local player instance.</param>
        /// <returns>The numerical rank ID within the current Grand Company.</returns>
        public static uint GCRank(this LocalPlayer player)
        {
            var gc = Core.Memory.Read<byte>(LocalPlayerExtensionsOffsets.CurrentGC);
            if (gc == 0)
            {
                return 0;
            }

            var gcRank = Core.Memory.Read<byte>(LocalPlayerExtensionsOffsets.CurrentGC + gc);

            return gcRank;
        }

        /// <summary>
        /// Gets the current number of Grand Company seals for the player's active Grand Company.
        /// </summary>
        /// <param name="player">The local player instance.</param>
        /// <returns>The total number of seals held.</returns>
        public static uint GCSeals(this LocalPlayer player)
        {
            uint[] sealTypes = { 20, 21, 22 };
            var bagslot = InventoryManager.GetBagByInventoryBagId(InventoryBagId.Currency).FirstOrDefault(i => i.RawItemId == sealTypes[(int)Core.Me.GrandCompany - 1]);
            return bagslot?.Count ?? 0U;
        }

        /// <summary>
        /// Gets the maximum number of Grand Company seals the player can hold based on their current rank.
        /// </summary>
        /// <param name="player">The local player instance.</param>
        /// <returns>The maximum seal capacity.</returns>
        public static int MaxGCSeals(this LocalPlayer player)
        {
            var gcRank = player.GCRank();

            IntPtr rankRow = Core.Memory.CallInjectedWraper<IntPtr>(LocalPlayerExtensionsOffsets.GCGetMaxSealsByRank,
                                                                    gcRank);

            return Core.Memory.Read<int>(rankRow);
        }

        /// <summary>
        /// Checks if a specific game condition is currently active for the player.
        /// </summary>
        /// <param name="player">The local player instance.</param>
        /// <param name="condition">The condition index to check.</param>
        /// <returns><see langword="true"/> if the condition is met; otherwise <see langword="false"/>.</returns>
        public static bool CheckCondition(this LocalPlayer player, byte condition)
        {
            return Core.Memory.Read<byte>(Offsets.Conditions + condition) == 1;
        }

        /// <summary>
        /// Condition flag index for the Free Trial state (FFXIVClientStructs Client::Game::Condition, FieldOffset 69).
        /// </summary>
        private const byte OnFreeTrialCondition = 69;

        /// <summary>
        /// Checks whether the current client is running on a Free Trial account.
        /// </summary>
        /// <param name="player">The local player instance.</param>
        /// <returns><see langword="true"/> if the account is a Free Trial account; otherwise <see langword="false"/>.</returns>
        public static bool IsOnFreeTrial(this LocalPlayer player)
        {
            return player.CheckCondition(OnFreeTrialCondition);
        }

        /// <summary>
        /// Determines if the player has a specific permission, optionally excluding certain conditions.
        /// </summary>
        /// <param name="player">The local player instance.</param>
        /// <param name="permissionId">The ID of the permission to check.</param>
        /// <param name="excludedCondition1">The first condition to exclude from the check.</param>
        /// <param name="excludedCondition2">The second condition to exclude from the check.</param>
        /// <returns><see langword="true"/> if the player has permission; otherwise <see langword="false"/>.</returns>
        public static bool HasPermission(this LocalPlayer player, int permissionId, int excludedCondition1 = 0, int excludedCondition2 = 0)
        {
            return Core.Memory.CallInjectedWraper<byte>(LocalPlayerExtensionsOffsets.HasPermission,Offsets.Conditions, permissionId, excludedCondition1, excludedCondition2) == 1;
        }

        /// <summary>
        /// Gets the unique 64-bit identifier for the player's character.
        /// </summary>
        /// <param name="player">The local player instance.</param>
        /// <returns>The unique <see cref="ulong"/> character ID.</returns>
        public static ulong PlayerId(this LocalPlayer player)
        {
            return Core.Memory.Read<ulong>(LocalPlayerExtensionsOffsets.PlayerId);
        }

        private static ulong _accountId;

        /// <summary>
        /// Gets the unique 64-bit identifier for the player's account.
        /// </summary>
        /// <param name="player">The local player instance.</param>
        /// <returns>The unique <see cref="ulong"/> account ID.</returns>
        public static ulong AccountId(this LocalPlayer player)
        {
            if (_accountId == 0)
            {
                var accountIdLocation = Core.Memory.Read<IntPtr>(LocalPlayerExtensionsOffsets.AccountIdLocation);
                _accountId = Core.Memory.Read<ulong>(accountIdLocation + LocalPlayerExtensionsOffsets.AccountIdOffset);
            }

            return _accountId;
        }

        /// <summary>
        /// Retrieves the home world of the specified character.
        /// </summary>
        /// <param name="character">The character to check.</param>
        /// <returns>The character's <see cref="World"/>; returns <see cref="World.SetMe"/> if the character is null.</returns>
        public static World HomeWorld(this Character? character)
        {
            return character == null ? World.SetMe : Core.Memory.Read<World>(character.Pointer + LocalPlayerExtensionsOffsets.HomeWorld);
        }

        /// <summary>
        /// Gets the ID of the mount the player is currently riding.
        /// </summary>
        /// <param name="player">The local player instance.</param>
        /// <returns>The mount ID, or 0 if not mounted.</returns>
        public static int CurrentMount(this LocalPlayer player)
        {
            return Core.Memory.Read<int>(player.Pointer + LocalPlayerExtensionsOffsets.CurrentMount);
        }

        /// <summary>
        /// Determines if the specified job is a Tank role.
        /// </summary>
        /// <param name="currentJob">The job type to check.</param>
        /// <returns><see langword="true"/> if the job is a tank; otherwise <see langword="false"/>.</returns>
        public static bool IsTank(this ClassJobType currentJob)
        {
            return currentJob is ClassJobType.Gladiator or ClassJobType.Marauder or ClassJobType.Paladin or ClassJobType.Warrior or ClassJobType.DarkKnight or ClassJobType.Gunbreaker;
        }

        /// <summary>
        /// Determines if the specified job is a Healer role.
        /// </summary>
        /// <param name="currentJob">The job type to check.</param>
        /// <returns><see langword="true"/> if the job is a healer; otherwise <see langword="false"/>.</returns>
        public static bool IsHealer(this ClassJobType currentJob)
        {
            return currentJob is ClassJobType.Conjurer or ClassJobType.WhiteMage or ClassJobType.Scholar or ClassJobType.Astrologian;
        }

        /// <summary>
        /// Determines if the specified job is a Melee DPS role.
        /// </summary>
        /// <param name="currentJob">The job type to check.</param>
        /// <returns><see langword="true"/> if the job is a melee DPS; otherwise <see langword="false"/>.</returns>
        public static bool IsMeleeDps(this ClassJobType currentJob)
        {
            return currentJob is ClassJobType.Pugilist or ClassJobType.Lancer or ClassJobType.Rogue or ClassJobType.Samurai or ClassJobType.Monk or ClassJobType.Dragoon or ClassJobType.Ninja or ClassJobType.Viper;
        }

        /// <summary>
        /// Determines if the specified job is a Physical Ranged DPS role.
        /// </summary>
        /// <param name="currentJob">The job type to check.</param>
        /// <returns><see langword="true"/> if the job is a physical ranged DPS; otherwise <see langword="false"/>.</returns>
        public static bool IsRangedDps(this ClassJobType currentJob)
        {
            return currentJob is ClassJobType.Archer or ClassJobType.Machinist or ClassJobType.Dancer or ClassJobType.Bard;
        }

        /// <summary>
        /// Sorts the player's gearsets by item level, then by role priority (Tank > Melee > Ranged).
        /// </summary>
        /// <param name="player">The local player instance.</param>
        /// <returns>An array of <see cref="GearSet"/> objects sorted by priority.</returns>
        public static GearSet[] SortedGearSets(this LocalPlayer player)
        {
            return GearsetManager.GearSets.OrderByDescending(i => i.ItemLevel).ThenByDescending(i => i.Class.IsTank()).ThenByDescending(i => i.Class.IsMeleeDps()).ThenByDescending(i => i.Class.IsRangedDps()).ToArray();
        }

        /// <summary>
        /// Retrieves the best available gearset for Disciples of War (combat jobs).
        /// </summary>
        /// <param name="player">The local player instance.</param>
        /// <returns>The highest priority combat <see cref="GearSet"/>, or <see langword="null"/> if none are found.</returns>
        public static GearSet BestCombatGearSet(this LocalPlayer player)
        {
            return player.SortedGearSets().FirstOrDefault(i => i.Class.IsDow());
        }

        /// <summary>
        /// Retrieves the best available gearset for Disciples of the Land (gathering jobs).
        /// </summary>
        /// <param name="player">The local player instance.</param>
        /// <returns>The highest priority gathering <see cref="GearSet"/>, or <see langword="null"/> if none are found.</returns>
        public static GearSet BestGatheringGearSet(this LocalPlayer player)
        {
            return player.SortedGearSets().FirstOrDefault(i => i.Class.IsDol());
        }

        /// <summary>
        /// Retrieves the best available gearset for Disciples of the Hand (crafting jobs).
        /// </summary>
        /// <param name="player">The local player instance.</param>
        /// <returns>The highest priority crafting <see cref="GearSet"/>, or <see langword="null"/> if none are found.</returns>
        public static GearSet BestCraftingGearSet(this LocalPlayer player)
        {
            return player.SortedGearSets().FirstOrDefault(i => i.Class.IsDoh());
        }
    }
}
