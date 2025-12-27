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
    public static class LocalPlayerExtensions
    {
        public const uint TheEndeavor = 900;
        public const uint TheEndeaverRuby = 1163;

        

        public static bool IsWalking => Core.Memory.Read<byte>(LocalPlayerExtensionsOffsets.RunWalk) == 1;

        public static bool IsFishing(this LocalPlayer play)
        {
            return FishingManager.State != FishingState.None;
        }

        public static bool IsOnFishingBoat(this LocalPlayer play)
        {
            return WorldManager.RawZoneId == TheEndeavor || WorldManager.RawZoneId == TheEndeaverRuby;
        }

        public static Location Location(this LocalPlayer play)
        {
            return new Location(WorldManager.ZoneId, Core.Me.Location);
        }

        /*internal static byte GatheringStatus(this LocalPlayer player)
        {
            return Core.Memory.Read<byte>(player.Pointer + LocalPlayerExtensionsOffsets.GatheringStateOffset);
        }*/

        public static IntPtr MinionPtr(this LocalPlayer play)
        {
            return Core.Memory.Read<IntPtr>(play.Pointer + LocalPlayerExtensionsOffsets.MinionPtr);
        }

        public static void SetRun(this LocalPlayer play)
        {
            if (IsWalking)
            {
                Core.Memory.Write<byte>(LocalPlayerExtensionsOffsets.RunWalk, 0);
            }
        }

        public static void SetWalk(this LocalPlayer play)
        {
            if (!IsWalking)
            {
                Core.Memory.Write<byte>(LocalPlayerExtensionsOffsets.RunWalk, 1);
            }
        }

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

        public static uint GCSeals(this LocalPlayer player)
        {
            uint[] sealTypes = { 20, 21, 22 };
            var bagslot = InventoryManager.GetBagByInventoryBagId(InventoryBagId.Currency).FirstOrDefault(i => i.RawItemId == sealTypes[(int)Core.Me.GrandCompany - 1]);
            return bagslot?.Count ?? 0U;
        }

        public static int MaxGCSeals(this LocalPlayer player)
        {
            var gcRank = player.GCRank();

            IntPtr rankRow = Core.Memory.CallInjectedWraper<IntPtr>(LocalPlayerExtensionsOffsets.GCGetMaxSealsByRank,
                                                                    gcRank);

            return Core.Memory.Read<int>(rankRow);
        }

        public static bool CheckCondition(this LocalPlayer player, byte condition)
        {
            return Core.Memory.Read<byte>(Offsets.Conditions + condition) == 1;
        }

        public static ulong PlayerId(this LocalPlayer player)
        {
            return Core.Memory.Read<ulong>(LocalPlayerExtensionsOffsets.PlayerId);
        }

        private static ulong _accountId;

        public static ulong AccountId(this LocalPlayer player)
        {
            if (_accountId == 0)
            {
                var accountIdLocation = Core.Memory.Read<IntPtr>(LocalPlayerExtensionsOffsets.AccountIdLocation);
                _accountId = Core.Memory.Read<ulong>(accountIdLocation + LocalPlayerExtensionsOffsets.AccountIdOffset);
            }

            return _accountId;
        }

        public static World HomeWorld(this Character? character)
        {
            return character == null ? World.SetMe : Core.Memory.Read<World>(character.Pointer + LocalPlayerExtensionsOffsets.HomeWorld);
        }

        public static int CurrentMount(this LocalPlayer player)
        {
            return Core.Memory.Read<int>(player.Pointer + LocalPlayerExtensionsOffsets.CurrentMount);
        }

        public static bool IsTank(this ClassJobType currentJob)
        {
            return currentJob is ClassJobType.Gladiator or ClassJobType.Marauder or ClassJobType.Paladin or ClassJobType.Warrior or ClassJobType.DarkKnight or ClassJobType.Gunbreaker;
        }

        //IsHealer
        public static bool IsHealer(this ClassJobType currentJob)
        {
            return currentJob is ClassJobType.Conjurer or ClassJobType.WhiteMage or ClassJobType.Scholar or ClassJobType.Astrologian;
        }

        //IsMeleeDps
        public static bool IsMeleeDps(this ClassJobType currentJob)
        {
            return currentJob is ClassJobType.Pugilist or ClassJobType.Lancer or ClassJobType.Rogue or ClassJobType.Samurai or ClassJobType.Monk or ClassJobType.Dragoon or ClassJobType.Ninja or ClassJobType.Viper;
        }

        //IsRangedDps
        public static bool IsRangedDps(this ClassJobType currentJob)
        {
            return currentJob is ClassJobType.Archer or ClassJobType.Machinist or ClassJobType.Dancer or ClassJobType.Bard;
        }

        public static GearSet[] SortedGearSets(this LocalPlayer player)
        {
            return GearsetManager.GearSets.OrderByDescending(i => i.ItemLevel).ThenByDescending(i => i.Class.IsTank()).ThenByDescending(i => i.Class.IsMeleeDps()).ThenByDescending(i => i.Class.IsRangedDps()).ToArray();
        }

        public static GearSet BestCombatGearSet(this LocalPlayer player)
        {
            return player.SortedGearSets().FirstOrDefault(i => i.Class.IsDow());
        }

        public static GearSet BestGatheringGearSet(this LocalPlayer player)
        {
            return player.SortedGearSets().FirstOrDefault(i => i.Class.IsDol());
        }

        public static GearSet BestCraftingGearSet(this LocalPlayer player)
        {
            return player.SortedGearSets().FirstOrDefault(i => i.Class.IsDoh());
        }
    }
}