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
        [Offset("Search 48 89 5C 24 ? 55 57 41 56 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 84 24 ? ? ? ? 80 3D ? ? ? ? ?")]
        [OffsetDawntrail("Search 40 55 53 57 41 54 41 57 48 8D AC 24 ? ? ? ? 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 85 ? ? ? ? 80 3D ? ? ? ? ?")]
        public static IntPtr SalvageAgent;

        [Offset("Search 4C 8D 0D ? ? ? ? 45 33 C0 33 D2 Add 3 TraceRelative")]
        public static IntPtr RepairVendor;

        [Offset("Search 48 8D 05 ? ? ? ? 48 89 03 B9 ? ? ? ? 4C 89 43 ? Add 3 TraceRelative")]
        [OffsetDawntrail("Search 48 8D 05 ? ? ? ? 4C 89 43 ? 48 89 03 B9 ? ? ? ? Add 3 TraceRelative")]
        public static IntPtr RepairVTable;

        [Offset("Search 48 8B 0D ? ? ? ? 4C 8B C0 33 D2 Add 3 TraceRelative")]
        [OffsetDawntrail("Search 48 8B 05 ? ? ? ? 4C 89 44 24 ? 44 8D 47 ? Add 3 TraceRelative")]
        public static IntPtr AtkStage;

        public static IntPtr SearchResultPtr => AtkStage;

        [Offset("Search 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 8B 93 ?? ?? ?? ?? 48 8B C8 Add 3 TraceRelative")]
        public static IntPtr g_InventoryManager;

        [Offset("Search 48 89 5C 24 ? 48 89 6C 24 ? 48 89 74 24 ? 57 48 83 EC ? 48 8B 41 ? 48 8B E9 48 83 C1 ?")]
        public static IntPtr HandInFunc;

        [Offset("Search 48 8D 05 ? ? ? ? 40 88 BB ? ? ? ? 48 89 03 Add 3 TraceRelative")]
        public static IntPtr HousingObjectVTable;

        [Offset("Search BF ? ? ? ? 66 90 48 8D 14 1E 48 8B CB E8 ? ? ? ? 48 81 C3 ? ? ? ? Add 1 Read32")]
        [OffsetDawntrail("Search 41 BF ? ? ? ? 0F 1F 84 00 ? ? ? ? 8B 44 3B ? Add 2 Read32")]
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
        [OffsetDawntrail("Search 48 89 6C 24 ? 56 57 41 56 48 83 EC ? 48 8B E9 44 8B F2")]
        public static IntPtr OpenTradeWindow;

        [Offset("Search 44 0F B7 0D ? ? ? ? 48 8D 57 ? Add 4 TraceRelative")]
        [OffsetDawntrail("Search 44 0F B7 0D ? ? ? ? 48 8D 53 ? Add 4 TraceRelative")]
        public static IntPtr ActorController_iLvl;

        [Offset("Search 80 B9 ? ? ? ? ? 0F 94 C0 C3 ? ? ? ? ? ? ? ? ? 48 83 EC ? Add 2 Read32")]
        [OffsetDawntrail("Search 80 B9 ? ? ? ? ? 75 ? 80 B9 ? ? ? ? ? 75 ? B0 ? C3 32 C0 C3 ? ? ? ? ? ? ? ? 48 83 EC ? Add 2 Read32")]
        public static int InventoryManagerFCTransfering;

        [Offset("Search 48 89 5C 24 ? 57 48 83 EC ? 8B F9 E8 ? ? ? ? 48 8B D8 48 85 C0 75 ? 32 C0 48 8B 5C 24 ? 48 83 C4 ? 5F C3 0F B6 88 ? ? ? ? E8 ? ? ? ? 48 8B C8")]
        [OffsetDawntrail("Search 48 89 5C 24 ? 57 48 83 EC ? 8B D9 E8 ? ? ? ? 48 8B F8")]
        public static IntPtr IsInstanceContentCompleted;

        [Offset("Search 48 89 5C 24 ? 57 48 83 EC ? 8B F9 E8 ? ? ? ? 48 8B D8 48 85 C0 75 ? 32 C0 48 8B 5C 24 ? 48 83 C4 ? 5F C3 0F B6 88 ? ? ? ? E8 ? ? ? ? 48 85 C0")]
        [OffsetDawntrail("Search E8 ? ? ? ? 84 C0 75 ? B0 ? 48 83 C4 ? 5B C3 8B CB Add 1 TraceRelative")]
        public static IntPtr IsInstanceContentUnlocked;

        [Offset("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? 49 89 47 ? Add 3 TraceRelative")]
        public static IntPtr UIStateTelepo;

        [Offset("Search 44 8B C2 4C 8B C9 41 C1 E8 ? 41 83 F8 ? 72 ? 32 C0 C3 0F B6 CA BA ? ? ? ? 83 E1 ? 41 8B C0 D3 E2")]
        [OffsetDawntrail("Search 44 8B C2 4C 8B C9 41 C1 E8 ? 41 83 F8 ? 72 ? 32 C0 C3 0F B6 CA BA ? ? ? ? 83 E1 ? D3 E2")]
        public static IntPtr IsSecretRecipeBookUnlocked;

        [Offset("Search 40 53 48 83 EC ? 48 8B D9 0F B7 CA E8 ? ? ? ? 48 85 C0 75 ?")]
        public static IntPtr IsFolkloreBookUnlocked;

        [Offset("Search 40 53 48 83 EC ? 48 8B D9 0F B7 CA E8 ? ? ? ? 48 85 C0 75 ? 48 83 C4 ?")]
        public static IntPtr IsOrnamentUnlocked;

        [Offset("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? 84 C0 0F 84 ? ? ? ? 8B 93 ? ? ? ? Add 3 TraceRelative")]
        [OffsetDawntrail("Search 48 8D 0D ? ? ? ? 48 8B 5C 24 ? 48 83 C4 ? 5F E9 ? ? ? ? 0F B7 50 ? 48 8D 0D ? ? ? ? 48 8B 5C 24 ? 48 83 C4 ? 5F E9 ? ? ? ? 0F B7 50 ? Add 3 TraceRelative")]
        public static IntPtr PlayerState;

        [Offset("Search E8 ? ? ? ? 48 8D 4C 24 ? E8 ? ? ? ? E9 ? ? ? ? 49 8D 8E ? ? ? ? TraceCall")]
        [OffsetDawntrail("Search E8 ? ? ? ? 48 8D 4C 24 ? E8 ? ? ? ? E9 ? ? ? ? BA ? ? ? ? 48 8B CF Add 1 TraceCall")]
        public static IntPtr ExecuteCommandInner;

        [Offset("Search 48 8B 0D ?? ?? ?? ?? 48 8B DA E8 ?? ?? ?? ?? 48 85 C0 74 ?? 4C 8B 00 48 8B C8 41 FF 90 ?? ?? ?? ?? 48 8B C8 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 85 C0 74 ?? 4C 8B 00 48 8B D3 48 8B C8 48 83 C4 ?? 5B 49 FF 60 ?? 48 83 C4 ?? 5B C3 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 40 53 Add 3 TraceRelative")]
        public static IntPtr Framework;

        [Offset("Search F3 0F 11 8B ? ? ? ? F3 0F 11 0D ? ? ? ? Add 4 Read32")]
        public static int Framerate;

        [Offset("Search 0F B7 41 ? C3 ? ? ? ? ? ? ? ? ? ? ? 0F B7 C2 41 B8 ? ? ? ? Add 3 Read8")]
        public static int AnimaLight;

        [Offset("Search 48 8D 0D ? ? ? ? 40 88 74 24 ? E8 ? ? ? ? 8B D8 Add 3 TraceRelative")]
        [OffsetDawntrail("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? 48 8B 4C 24 ? 8B D0 E8 ? ? ? ? 48 8D 4C 24 ? E8 ? ? ? ? B8 ? ? ? ? 48 83 C4 ? C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 83 EC ? Add 3 TraceRelative")]
        public static IntPtr AnimaLightThing;

        [Offset("Search E8 ? ? ? ? 4C 8B A4 24 ? ? ? ? 8D 58 ? Add 1 TraceRelative")]
        public static IntPtr GetDawnContentRowCount;

        [Offset("Search E8 ? ? ? ? 48 85 C0 74 ? 8B 08 E8 ? ? ? ? 48 85 C0 74 ? 0F B7 40 ? Add 1 TraceRelative")]
        public static IntPtr GetDawnContentRow;
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

        [Offset("48 89 91 ? ? ? ? C3 ? ? ? ? ? ? ? ? 89 91 ? ? ? ? C3 ? ? ? ? ? ? ? ? ? 81 B9 ? ? ? ? ? ? ? ? Add 3 Read32")]
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

        [Offset("Search B9 ? ? ? ? E8 ? ? ? ? 40 88 BD ? ? ? ? Add 1 Read32")]
        public static int RetainerNetworkPacket;
    }

    public static partial class Offsets
    {
        [Offset("Search E8 ?? ?? ?? ?? 80 7B 1D 01 TraceCall")]
        public static IntPtr GetUiModule;
    }

#pragma warning restore CS0649
}