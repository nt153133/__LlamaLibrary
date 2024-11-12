using System;
using System.Linq;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Objects;
using LlamaLibrary.Enums;
using LlamaLibrary.Helpers.NPC;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.Extensions
{
    public static class LocalPlayerExtensions
    {
        public const uint TheEndeavor = 900;
        public const uint TheEndeaverRuby = 1163;

        internal static class Offsets
        {
            /*[Offset("Search 44 88 84 0A ? ? ? ? C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 88 91 ? ? ? ? Add 4 Read32")]
            internal static int GatheringStateOffset;*/

            [Offset("Search 0F B6 15 ? ? ? ? 8D 42 ? 3C ? 77 ? FE CA 48 8D 0D ? ? ? ? Add 3 TraceRelative")]
            [OffsetDawntrail("Search 0F B6 0D ? ? ? ? FE C9  Add 3 TraceRelative")]
            internal static IntPtr CurrentGC;

            [Offset("Search 48 83 EC ? 48 8B 05 ? ? ? ? 44 8B C1 BA ? ? ? ? 48 8B 88 ? ? ? ? E8 ? ? ? ? 48 85 C0 75 ? 48 83 C4 ? C3 48 8B 00 48 83 C4 ? C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 83 EC ? 80 F9 ?")]
            internal static IntPtr GCGetMaxSealsByRank;

            //PlayerID 8byte ulong ID unique to that character which is included in MB listings
            [Offset("Search 48 8B 05 ? ? ? ? 48 8D 0D ? ? ? ? 41 8B DC Add 3 TraceRelative")]
            internal static IntPtr PlayerId;

            //7.1
            [Offset("Search 48 89 B3 ? ? ? ? 48 89 B3 ? ? ? ? 48 8B 74 24 ? 89 BB ? ? ? ? Add 3 Read32")]
            [OffsetCN("Search 48 89 BB ? ? ? ? 80 A3 ? ? ? ? ? 88 83 ? ? ? ? Add 3 Read32")]
            internal static int AccountId;


            [Offset("Search 0F B6 05 ? ? ? ? 88 83 ? ? ? ? Add 3 TraceRelative")]
            internal static IntPtr RunWalk;

            [Offset("Search 48 8B 8F ? ? ? ? 48 85 C9 74 ? 48 8B 01 FF 90 ? ? ? ? 84 C0 Add 3 Read32")]
            internal static int MinionPtr;

            [Offset("Search 0F B7 86 ? ? ? ? 66 89 85 ? ? ? ? E8 ? ? ? ? 48 8B 85 ? ? ? ? Add 3 Read32")]
            [OffsetDawntrail("Search 41 0F B7 86 ? ? ? ? 66 89 86 ? ? ? ? 48 8B 0D ? ? ? ? Add 4 Read32")]
            internal static int HomeWorld;

            [Offset("Search 66 83 B9 ? ? ? ? ? 48 8B DA Add 3 Read32")]
            [OffsetDawntrail("Search 48 8B DA 66 83 B9 ? ? ? ? ?  Add 6 Read32")]
            internal static int CurrentMount;
        }

        public static bool IsWalking => Core.Memory.Read<byte>(Offsets.RunWalk) == 1;

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
            return Core.Memory.Read<byte>(player.Pointer + Offsets.GatheringStateOffset);
        }*/

        public static IntPtr MinionPtr(this LocalPlayer play)
        {
            return Core.Memory.Read<IntPtr>(play.Pointer + Offsets.MinionPtr);
        }

        public static void SetRun(this LocalPlayer play)
        {
            if (IsWalking)
            {
                Core.Memory.Write<byte>(Offsets.RunWalk, 0);
            }
        }

        public static void SetWalk(this LocalPlayer play)
        {
            if (!IsWalking)
            {
                Core.Memory.Write<byte>(Offsets.RunWalk, 1);
            }
        }

        public static uint GCRank(this LocalPlayer player)
        {
            var gc = Core.Memory.Read<byte>(Offsets.CurrentGC);
            if (gc == 0)
            {
                return 0;
            }

            var gcRank = Core.Memory.Read<byte>(Offsets.CurrentGC + gc);

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

            IntPtr rankRow;
            lock (Core.Memory.Executor.AssemblyLock)
            {
                rankRow = Core.Memory.CallInjected64<IntPtr>(Offsets.GCGetMaxSealsByRank,
                                                             gcRank);
            }

            return Core.Memory.Read<int>(rankRow);
        }

        public static bool CheckCondition(this LocalPlayer player, byte condition)
        {
            return Core.Memory.Read<byte>(Memory.Offsets.Conditions + condition) == 1;
        }

        public static ulong PlayerId(this LocalPlayer player)
        {
            return Core.Memory.Read<ulong>(Offsets.PlayerId);
        }


        public static ulong AccountId(this LocalPlayer player)
        {
            return Core.Memory.Read<ulong>(player.Pointer + Offsets.AccountId);
        }


        public static World HomeWorld(this Character? character)
        {
            return character == null ? World.SetMe : Core.Memory.Read<World>(character.Pointer + Offsets.HomeWorld);
        }

        public static int CurrentMount(this LocalPlayer player)
        {
            return Core.Memory.Read<int>(player.Pointer + Offsets.CurrentMount);
        }

        public static bool IsTank(this ClassJobType currentJob)
        {
            return currentJob == ClassJobType.Gladiator || currentJob == ClassJobType.Marauder || currentJob == ClassJobType.Paladin || currentJob == ClassJobType.Warrior || currentJob == ClassJobType.DarkKnight || currentJob == ClassJobType.Gunbreaker;
        }

        //IsHealer
        public static bool IsHealer(this ClassJobType currentJob)
        {
            return currentJob == ClassJobType.Conjurer || currentJob == ClassJobType.WhiteMage || currentJob == ClassJobType.Scholar || currentJob == ClassJobType.Astrologian;
        }

        //IsMeleeDps
        public static bool IsMeleeDps(this ClassJobType currentJob)
        {
            return currentJob == ClassJobType.Pugilist || currentJob == ClassJobType.Lancer || currentJob == ClassJobType.Rogue || currentJob == ClassJobType.Samurai || currentJob == ClassJobType.Monk || currentJob == ClassJobType.Dragoon || currentJob == ClassJobType.Ninja;
        }

        //IsRangedDps
        public static bool IsRangedDps(this ClassJobType currentJob)
        {
            return currentJob == ClassJobType.Archer || currentJob == ClassJobType.Machinist || currentJob == ClassJobType.Dancer || currentJob == ClassJobType.Bard;
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