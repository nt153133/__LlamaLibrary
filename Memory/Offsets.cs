/*
DeepDungeon is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Orginal work done by zzi, contibutions by Omninewb, Freiheit, and mastahg
                                                                                 */

using System;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.Memory
{
#pragma warning disable CS0649
    public static partial class Offsets
    {
        //[Offset("Search 48 85 D2 0F 84 ? ? ? ? 55 56 57 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 84 24 ? ? ? ? 80 7A ? ? 41 8B E8 48 8B FA 48 8B F1 74 ? 48 8B CA E8 ? ? ? ? 48 8B C8 E8 ? ? ? ? EB ? 0F B6 42 ? A8 ? 74 ? 8B 42 ? 05 ? ? ? ? EB ? A8 ? 8B 42 ? 74 ? 05 ? ? ? ? 85 C0 0F 84 ? ? ? ? 48 89 9C 24 ? ? ? ? 48 8B CE 4C 89 B4 24 ? ? ? ? E8 ? ? ? ? 8B 9E ? ? ? ?")] //0x1BEA
        //Pre 5.4[Offset("Search 48 85 D2 0F 84 ?? ?? ?? ?? 53 55 57")]
        [Offset("40 53 55 57 41 56 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 84 24 ? ? ? ? 80 79 ? ?")]

        //        [OffsetCN("Search 48 85 D2 0F 84 ? ? ? ? 55 56 57 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 84 24 ? ? ? ? 80 7A ? ? 41 8B E8 48 8B FA 48 8B F1 74 ? 48 8B CA E8 ? ? ? ? 48 8B C8 E8 ? ? ? ? EB ? 0F B6 42 ? A8 ? 74 ? 8B 42 ? 05 ? ? ? ? EB ? A8 ? 8B 42 ? 74 ? 05 ? ? ? ? 85 C0 0F 84 ? ? ? ? 48 89 9C 24 ? ? ? ? 48 8B CE 4C 89 B4 24 ? ? ? ? E8 ? ? ? ? 8B 9E ? ? ? ?")] //0x1BEA
        public static IntPtr SalvageAgent;

        [Offset("Search 4C 8D 0D ? ? ? ? 45 33 C0 33 D2 48 8B C8 E8 ? ? ? ? Add 3 TraceRelative")]
        public static IntPtr RepairVendor;

        [Offset("Search 48 89 5C 24 ? 57 48 83 EC ? 88 51 ? 49 8B F9")]
        public static IntPtr RepairWindowOpen;

        [Offset("Search 48 8D 05 ? ? ? ? 48 89 03 B9 ? ? ? ? 4C 89 43 ? Add 3 TraceRelative")]
        public static IntPtr RepairVTable;

        [Offset("Search 48 8B 0D ? ? ? ? 4C 8B C0 33 D2 Add 3 TraceRelative")]
        public static IntPtr AtkStage;

        public static IntPtr SearchResultPtr => AtkStage;

        [Offset("Search 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 8B 93 ?? ?? ?? ?? 48 8B C8 Add 3 TraceRelative")]
        public static IntPtr g_InventoryManager;

        [Offset("Search 48 89 5C 24 ? 48 89 6C 24 ? 48 89 74 24 ? 57 48 83 EC ? 48 8B 41 ? 48 8B E9 48 83 C1 ?")]
        public static IntPtr HandInFunc;

        [Offset("Search 48 8D 05 ? ? ? ? 40 88 BB ? ? ? ? 48 89 03 Add 3 TraceRelative")]
        public static IntPtr HousingObjectVTable;

        [Offset("Search BF ? ? ? ? 66 90 48 8D 14 1E 48 8B CB E8 ? ? ? ? 48 81 C3 ? ? ? ? Add 1 Read32")]
        public static int GCTurninCount;

        [Offset("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? 48 8D 0D ? ? ? ? E8 ? ? ? ? 33 D2 48 8D 0D ? ? ? ? E8 ? ? ? ? Add 3 TraceRelative")]
        public static IntPtr GCTurnin;

        [Offset("Search 48 8D 0D ?? ?? ?? ?? BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 80 8B ?? ?? ?? ?? ?? 45 33 C9 44 8B C7 89 BB ?? ?? ?? ?? Add 3 TraceRelative")]
        public static IntPtr Conditions;

        [Offset("Search 41 8D 51 ? E8 ? ? ? ? 84 C0 75 ? 45 33 C0 48 8D 0D ? ? ? ? 41 8D 50 ? E8 ? ? ? ? EB ? 48 8B 0D ? ? ? ? Add 3 Read8")]
        public static int DesynthLock;

        [Offset("Search BA ? ? ? ? E8 ? ? ? ? 48 8B 83 ? ? ? ? 48 8B 88 ? ? ? ? Add 1 Read32")]
        public static int JumpingCondition;

        [Offset("Search 89 91 ? ? ? ? 44 89 81 ? ? ? ? C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 85 D2 Add 2 Read32")]
        public static int CurrentMettle;

        [Offset("Search 44 89 81 ? ? ? ? C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 85 D2 Add 3 Read32")]
        public static int NextReistanceRank;

        [Offset("Search 48 89 6C 24 ? 57 41 56 41 57 48 83 EC ? 48 8B E9 44 8B FA")]
        public static IntPtr OpenTradeWindow;

        [Offset("Search 44 0F B7 0D ? ? ? ? 48 8D 57 ? Add 4 TraceRelative")]
        public static IntPtr ActorController_iLvl;

        //  [Offset("Search 66 83 78 ? ? 74 ? 8B 78 ? E8 ? ? ? ? Add 3 Read8")]
        //  public static int VentureTask;
    }

    public static partial class Offsets
    {
        [Offset("Search 4C 8D 2D ? ? ? ? 66 0F 7F 44 24 ? 41 8B DF Add 3 TraceRelative")]
        public static IntPtr RetainerStats;

        [Offset("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? 48 8B F0 48 85 C0 74 ? 48 83 38 ? Add 3 TraceRelative")]
        public static IntPtr RetainerData;

        [Offset("Search 41 C6 87 ? ? ? ? ? 48 83 C4 ? 41 5F 41 5D 41 5C Add 3 Read32")]
        public static int RetainerDataLoaded;

        [Offset("Search 41 88 87 ? ? ? ? 40 0F 97 C5 Add 3 Read32")]
        public static int RetainerDataOrder;

        [Offset("Search 48 89 91 ? ? ? ? C3 ? ? ? ? ? ? ? ? 48 83 39 ? Add 3 Read32")]
        public static int CurrentRetainer;

        [Offset("Search 83 FA ? 73 ? 8B C2 0F B6 94 08 ? ? ? ? 80 FA ?")]
        public static IntPtr GetRetainerPointer;

        [Offset("Search 48 83 39 ? 4C 8B C9")]
        public static IntPtr GetNumberOfRetainers;

        [Offset("Search 48 89 5C 24 ? 48 89 6C 24 ? 48 89 74 24 ? 57 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 84 24 ? ? ? ? 8B E9 41 8B D9 48 8B 0D ? ? ? ? 41 8B F8 8B F2")]
        public static IntPtr ExecuteCommand; //RequestRetainerData

        [Offset("Search 48 8D 56 ? EB ? ? ? ? ? ? ? ? ? ? ? 40 53 Add 3 Read8")]
        public static int RetainerName;

        [Offset("Search 66 83 78 ? ? 74 ? 8B 78 ? E8 ? ? ? ? Add 3 Read8")]
        public static int VentureTask;

        [Offset("Search 8B 78 ? E8 ? ? ? ? 3B F8 Add 2 Read8")]
        public static int VentureFinishTime;

        //B9 ? ? ? ? E8 ? ? ? ? 48 8B 5C 24 ? C6 85 ? ? ? ? ? Add 1 Read32
        [Offset("Search B9 ? ? ? ? E8 ? ? ? ? 40 88 BD ? ? ? ? Add 1 Read32")]
        public static int RetainerNetworkPacket;
    }

#pragma warning restore CS0649
}