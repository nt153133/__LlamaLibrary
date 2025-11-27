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
        //7.1
        [Offset("Search E8 ? ? ? ? 48 8B C7 48 8B 5C 24 ? 48 8B 74 24 ? 48 83 C4 ? 5F C3 48 8D 4B ? Add 1 TraceRelative")]
        public static IntPtr SalvageAgent;

        [Offset("Search 4C 8D 0D ? ? ? ? 45 33 C0 33 D2 Add 3 TraceRelative")]
        public static IntPtr RepairVendor;

        [Offset("Search 48 8D 05 ? ? ? ? 4C 89 43 ? 48 89 03 B9 ? ? ? ? Add 3 TraceRelative")]
        public static IntPtr RepairVTable;

        //7.3
        [Offset("Search 48 8B 0D ? ? ? ? 48 8B D3 48 8B 49 ? E8 ? ? ? ? Add 3 TraceRelative")]
        [OffsetTC("Search 48 8B 05 ? ? ? ? 4C 89 44 24 ? 44 8D 47 ? Add 3 TraceRelative")]
        public static IntPtr AtkStage;

        public static IntPtr SearchResultPtr => AtkStage;

        //7.3
        [Offset("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? 8B 93 ? ? ? ? 48 8B 08 Add 3 TraceRelative")]
        [OffsetTC("Search 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 8B 93 ?? ?? ?? ?? 48 8B C8 Add 3 TraceRelative")]
        public static IntPtr g_InventoryManager;

        [Offset("Search 48 89 5C 24 ? 48 89 6C 24 ? 48 89 74 24 ? 57 48 83 EC ? 48 8B 41 ? 48 8B E9 48 83 C1 ?")]
        public static IntPtr HandInFunc;

        [Offset("Search 48 8D 05 ? ? ? ? 40 88 BB ? ? ? ? 48 89 03 Add 3 TraceRelative")]
        public static IntPtr HousingObjectVTable;

        [Offset("Search 41 BF ? ? ? ? 0F 1F 84 00 ? ? ? ? 8B 44 3B ? Add 2 Read32")]
        public static int GCTurninCount;

        //7.3
        [Offset("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? 33 F6 48 8D 0D ? ? ? ? Add 3 TraceRelative")]
        [OffsetTC("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? 48 8D 0D ? ? ? ? E8 ? ? ? ? 33 D2 48 8D 0D ? ? ? ? E8 ? ? ? ? Add 3 TraceRelative")]
        public static IntPtr GCTurnin;

        [Offset("Search 48 8D 0D ?? ?? ?? ?? BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 80 8B ?? ?? ?? ?? ?? 45 33 C9 44 8B C7 89 BB ?? ?? ?? ?? Add 3 TraceRelative")]
        public static IntPtr Conditions;

        //7.3
        [Offset("Search 41 8D 51 ? E8 ? ? ? ? 84 C0 75 ? 45 33 C0 48 8D 0D ? ? ? ? 41 8D 50 ? E8 ? ? ? ? 33 D2 Add 3 Read8")]
        [OffsetTC("Search 41 8D 51 ? E8 ? ? ? ? 84 C0 75 ? 45 33 C0 48 8D 0D ? ? ? ? 41 8D 50 ? E8 ? ? ? ? EB ? 48 8B 0D ? ? ? ? Add 3 Read8")]
        public static int DesynthLock;

        [Offset("Search BA ? ? ? ? E8 ? ? ? ? 48 8B 83 ? ? ? ? 48 8B 88 ? ? ? ? Add 1 Read32")]
        public static int JumpingCondition;

        [Offset("Search 89 91 ? ? ? ? 44 89 81 ? ? ? ? C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 85 D2 Add 2 Read32")]
        public static int CurrentMettle;

        [Offset("Search 44 89 81 ? ? ? ? C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 85 D2 Add 3 Read32")]
        public static int NextReistanceRank;

        [Offset("Search 48 89 6C 24 ? 56 57 41 56 48 83 EC ? 48 8B E9 44 8B F2")]
        public static IntPtr OpenTradeWindow;

        [Offset("Search 44 0F B7 0D ? ? ? ? 48 8D 53 ? Add 4 TraceRelative")]
        public static IntPtr ActorController_iLvl;

        [Offset("Search 80 B9 ? ? ? ? ? 75 ? 80 B9 ? ? ? ? ? 75 ? B0 ? C3 32 C0 C3 ? ? ? ? ? ? ? ? 48 83 EC ? Add 2 Read32")]
        public static int InventoryManagerFCTransfering;

        //7.1
        [Offset("Search E8 ? ? ? ? 84 C0 74 ? 48 FF C3 48 83 FB ? 72 ? B0 ? 48 8B 4C 24 ? Add 1 TraceRelative")]
        public static IntPtr IsInstanceContentCompleted;

        //7.1
        [Offset("Search E8 ? ? ? ? 3C ? 75 ? 32 C0 48 8B 5C 24 ? 48 8B 74 24 ? Add 1 TraceRelative")]
        public static IntPtr IsInstanceContentUnlocked;

        //7.3
        [Offset("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? 49 89 47 ? BA ? ? ? ? Add 3 TraceRelative")]
        [OffsetTC("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? 49 89 44 24 ? Add 3 TraceRelative")]
        public static IntPtr UIStateTelepo;

        [Offset("Search 44 8B C2 4C 8B C9 41 C1 E8 ? 41 83 F8 ? 72 ? 32 C0 C3 0F B6 CA BA ? ? ? ? 83 E1 ? D3 E2")]
        public static IntPtr IsSecretRecipeBookUnlocked;

        [Offset("Search 40 53 48 83 EC ? 48 8B D9 0F B7 CA E8 ? ? ? ? 48 85 C0 0F 84 ? ? ? ?")]
        public static IntPtr IsFolkloreBookUnlocked;

        [Offset("Search 40 53 48 83 EC ? 48 8B D9 0F B7 CA E8 ? ? ? ? 48 85 C0 75 ? 48 83 C4 ?")]
        public static IntPtr IsOrnamentUnlocked;

        [Offset("Search 48 8D 0D ? ? ? ? 48 8B 5C 24 ? 48 83 C4 ? 5F E9 ? ? ? ? 0F B7 50 ? 48 8D 0D ? ? ? ? 48 8B 5C 24 ? 48 83 C4 ? 5F E9 ? ? ? ? 0F B7 50 ? Add 3 TraceRelative")]
        public static IntPtr PlayerState;

        //7.3
        [Offset("Search E8 ? ? ? ? 48 8D 4C 24 ? E8 ? ? ? ? E9 ? ? ? ? FF 50 ? TraceCall")]
        [OffsetTC("Search E8 ? ? ? ? 48 8D 4C 24 ? E8 ? ? ? ? E9 ? ? ? ? BA ? ? ? ? 48 8B CF Add 1 TraceCall")]
        public static IntPtr ExecuteCommandInner;

        [Offset("Search 48 8B 0D ?? ?? ?? ?? 48 8B DA E8 ?? ?? ?? ?? 48 85 C0 74 ?? 4C 8B 00 48 8B C8 41 FF 90 ?? ?? ?? ?? 48 8B C8 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 85 C0 74 ?? 4C 8B 00 48 8B D3 48 8B C8 48 83 C4 ?? 5B 49 FF 60 ?? 48 83 C4 ?? 5B C3 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 40 53 Add 3 TraceRelative")]
        public static IntPtr Framework;

        [Offset("Search F3 0F 11 8B ? ? ? ? F3 0F 11 0D ? ? ? ? Add 4 Read32")]
        public static int Framerate;

        [Offset("Search 0F B7 41 ? C3 ? ? ? ? ? ? ? ? ? ? ? 0F B7 C2 41 B8 ? ? ? ? Add 3 Read8")]
        public static int AnimaLight;

        [Offset("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? 48 8B 4C 24 ? 8B D0 E8 ? ? ? ? 48 8D 4C 24 ? E8 ? ? ? ? B8 ? ? ? ? 48 83 C4 ? C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 83 EC ? Add 3 TraceRelative")]
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

        [Offset("Search E8 ? ? ? ? 48 8B 06 48 8B CE FF 50 ? 44 89 76 ? E9 ? ? ? ? 40 84 FF Add 1 TraceRelative")]
        [OffsetCN("Search 48 89 5C 24 ? 48 89 6C 24 ? 48 89 74 24 ? 57 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 84 24 ? ? ? ? 8B E9 41 8B D9 48 8B 0D ? ? ? ? 41 8B F8 8B F2")]
        public static IntPtr ExecuteCommand; //RequestRetainerData

        [Offset("Search 48 8D 56 ? EB ? ? ? ? ? ? ? ? ? ? ? 40 53 Add 3 Read8")]
        public static int RetainerName;

        [Offset("Search 66 83 78 ? ? 74 ? 8B 78 ? E8 ? ? ? ? Add 3 Read8")]
        public static int VentureTask;

        [Offset("Search 8B 78 ? E8 ? ? ? ? 3B F8 Add 2 Read8")]
        public static int VentureFinishTime;

        [Offset("Search B9 ? ? ? ? E8 ? ? ? ? 40 88 BD ? ? ? ? Add 1 Read32")]
        public static int RetainerNetworkPacket;

        [Offset("Search E8 ? ? ? ? 0F B6 F0 48 8D 5C 24 ? Add 1 TraceRelative")]
        internal static IntPtr SendAction;

        [Offset("Search 66 83 FA ? 75 ? 53 48 83 EC ? 48 8B D9 BA ? ? ? ? 48 8D 4C 24 ?")]
        internal static IntPtr DialogueOkay;
    }

    public static partial class Offsets
    {
        [Offset("Search E8 ?? ?? ?? ?? 80 7B 1D 01 TraceCall")]
        public static IntPtr GetUiModule;
    }


    public static class AchievementsOffsets
    {

        [Offset("Search E8 ?? ?? ?? ?? 04 30 FF C3 TraceCall")]
        internal static IntPtr IsCompletePtr;

        [Offset("Search 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 04 30 FF C3 Add 3 TraceRelative")]
        internal static IntPtr AchievementInstancePtr;

        [Offset("Search C7 81 ? ? ? ? ? ? ? ? 45 33 C9 B9 ? ? ? ? Add 2 Read32")]
        internal static int AchievementState;

        [Offset("Search 48 83 EC ?? C7 81 ?? ?? ?? ?? ?? ?? ?? ?? 45 33 C9")]
        internal static IntPtr RequestAchievementFunction;

        [Offset("Search 44 89 81 ? ? ? ? 44 89 89 ? ? ? ? C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 8B 81 ? ? ? ? Add 3 Read32")]
        internal static int AchievementCurrentProgress;

        [Offset("Search C7 81 ? ? ? ? ? ? ? ? 45 33 C9 B9 ? ? ? ? Add 2 Read32")]
        internal static int SingleAchievementState;

    }

    public static class ActionHelperOffsets
    {

        [Offset("Search E8 ? ? ? ? 41 89 9E ? ? ? ? EB 0B TraceCall")]
        internal static IntPtr DoAction;


        //7.3
        [Offset("Search  48 8D 0D ? ? ? ? E8 ? ? ? ? 44 8B C0 4D 85 F6 Add 3 TraceRelative")]
        [OffsetTC("Search 48 8D 0D ? ? ? ? 41 B9 ? ? ? ? 44 88 6C 24 ? Add 3 TraceRelative")]
        internal static IntPtr ActionManagerParam;

        //41 B8 ? ? ? ? 89 5C 24 ? 48 8D 0D ? ? ? ? E8 ? ? ? ? 84 C0 75 ?

        //7.1
        [Offset("Search 41 B8 ? ? ? ? 89 7C 24 ? E8 ? ? ? ? Add 2 Read32")]
        internal static int DecipherSpell;

    }

    public static class AgentAWGrowthFragTradeOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? 48 8D 4B ? 48 89 03 33 D2 Add 3 TraceRelative")]
        internal static IntPtr Vtable;

        //(AgentPtr, index, qty)
        //7.3
        [Offset("Search 4C 8B DC 55 56 57 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 84 24 ? ? ? ? 48 83 B9 ? ? ? ? ?")]
        [OffsetTC("Search 4C 8B DC 55 56 57 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 44 24 ? 48 83 B9 ? ? ? ? ? 41 8B E8 48 63 FA")]
        internal static IntPtr BuyFunction;

        [Offset("Search 49 8D 4D ? 4C 8D 0D ? ? ? ? Add 3 Read8")]
        internal static int ArrayBase;

        [Offset("Search 45 89 BD ? ? ? ? 49 8D 4D ? Add 3 Read32")]
        internal static int ArrayCount;

    }

    public static class AgentAchievementOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? 4C 89 7F ? 48 89 07 48 8D B7 ? ? ? ? Add 3 TraceRelative")]
        internal static IntPtr VTable;

        //7.3
        [Offset("Search 41 8B 46 ? 41 3B C7 Add 3 Read8")]
        [OffsetTC("Search 41 8B 46 ? 3B C5 Add 3 Read8")]
        internal static int Status;

    }

    public static class AgentAetherWheelOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? 48 89 06 4C 8D 76 ? Add 3 TraceRelative")]

        internal static IntPtr VTable;

        [Offset("Search 49 8D 75 ? F3 0F 10 3D ? ? ? ? Add 3 Read8")]
        internal static int ArrayOffset;

    }

    public static class AgentBagSlotOffsets
    {


        //7.3
        [Offset("Search 48 8D 05 ? ? ? ? 48 8B F1 48 89 01 48 8B 89 ? ? ? ? 48 85 C9 74 ? E8 ? ? ? ? Add 3 TraceRelative")]
        [OffsetTC("Search 48 8D 05 ? ? ? ? 48 89 79 ? 48 8B D9 89 79 Add 3 TraceRelative")]
        internal static IntPtr VTable;

        //7.1
        [Offset("Search 48 8B 89 ? ? ? ? 48 85 C9 74 ? E8 ? ? ? ? 48 C7 86 ? ? ? ? ? ? ? ? 48 8D 8E ? ? ? ? E8 ? ? ? ? 48 8D 8E ? ? ? ? E8 ? ? ? ? BF ? ? ? ? 48 8D 9E ? ? ? ? 48 83 EB ? Add 3 Read32")]
        internal static int Offset;

        [Offset("Search 48 8B 48 ? 48 85 C9 0F 84 ? ? ? ? 8B 93 ? ? ? ? Add 3 Read8")]
        internal static int FuncOffset;

    }

    public static class AgentBankaCraftworksSupplyOffsets
    {

        //7.2 hf
        [Offset("Search 48 8D 0D ? ? ? ? 83 7B ? ? 48 89 4C 24 ? 48 8D 0D ? ? ? ? 88 54 24 ? 41 0F 94 C0 83 3B ? 48 89 4C 24 ? 48 8D 4C 24 ? 48 8B 40 ? 0F 94 C2 48 89 44 24 ? E8 ? ? ? ? 48 8D 4C 24 ? E8 ? ? ? ? 48 83 C4 ? 5B C3 0F 1F 00 Add 3 TraceRelative")]
        internal static IntPtr Vtable;

        //7.2 hf
        [Offset("Search FF 50 ? 84 C0 74 ? 48 85 DB 74 ? 48 8B 03 48 8B CB FF 50 ? 48 8B 4F ? 3B 41 ? 75 ? 48 8B 03 48 8B CB FF 90 ? ? ? ? 48 8B 4F ? 66 3B 81 ? ? ? ? 72 ? 48 8B CB E8 ? ? ? ? 48 85 C0 75 ? B0 ? 48 8B 5C 24 ? 48 83 C4 ? 5F C3 48 8B 5C 24 ? 32 C0 48 83 C4 ? 5F C3 ? ? ? ? ? ? ? ? ? ? ? 40 53 Add 2 Read8")]
        //0x28
        internal static int PointerOffset;

    }

    public static class AgentCharacterOffsets
    {

        //7.1
        [Offset("Search 48 8D 05 ? ? ? ? 48 89 06 48 8D 8E ? ? ? ? 0F 57 C0 Add 3 TraceRelative")]
        internal static IntPtr Vtable;

    }

    public static class AgentContentsInfoOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? BE ? ? ? ? 48 89 03 48 8D 7B ? Add 3 TraceRelative")]
        internal static IntPtr VTable;

    }

    public static class AgentDawnOffsets
    {

        [Offset("Search 45 0F B6 5E ? 45 84 DB 75 ? 49 8B 8C D9 ? ? ? ? Add 4 Read8")]
        internal static int DawnTrustId;

        [Offset("Search 48 8D 05 ? ? ? ? C6 46 ? ? 33 ED Add 3 TraceRelative")]
        internal static IntPtr DawnVtable;
        /*[Offset("Search 41 88 46 ? E8 ? ? ? ? C6 43 ? ? Add 3 Read8")]
        internal static int DawnIsScenario;*/

    }

    public static class AgentDawnStoryOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? C6 43 ? ? 48 89 03 48 8B C3 48 C7 43 ? ? ? ? ? Add 3 TraceRelative")]
        internal static IntPtr Vtable;

        //7.3
        [Offset("Search 48 89 35 ? ? ? ? 48 8B 74 24 ? 48 83 C4 ? C3 ? ? ? ? ? ? ? ? ? ? 48 83 EC ? Add 3 TraceRelative")]
        [OffsetTC("Search 48 89 05 ? ? ? ? 48 83 C4 ? C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 83 EC ? Add 3 TraceRelative")]
        internal static IntPtr DutyListPtr;

        //7.3
        [Offset("Search 49 8D 96 ? ? ? ? E8 ? ? ? ? 48 8B 8C 24 ? ? ? ? Add 3 Read32")]
        [OffsetTC("Search 48 8D 99 ? ? ? ? 48 8D 4C 24 ? Add 3 Read32")]
        internal static int DutyListStart;

        [Offset("Search BF ? ? ? ? 48 8B D3 48 8B CE Add 1 Read32")]
        internal static int DutyCount;

        //7.2
        [Offset("Search 8B 5F ? C7 47 ? ? ? ? ? ? ? ? FF 50 ? 8B D3 48 8B C8 ? ? ? 41 FF 90 ? ? ? ? 48 8B 5C 24 ? 48 83 C4 ? 5F C3 ? ? ? ? ? ? ? 83 79 Add 2 Read8")]
        internal static int Loaded;

    }

    public static class AgentFateProgressOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? 66 C7 46 ? ? ? 48 89 06 Add 3 TraceRelative")]
        internal static IntPtr VTable;

        //[Offset("Search 66 89 47 ? 48 8B 5C 24 ? 48 8B 74 24 ? Add 3 Read8")]
        //internal static int LoadedZones;
        //[Offset("Search 48 8B 47 ? 48 8B CF 48 89 47 ? 33 C0 Add 3 Read8")]
        //internal static int ZoneStructs;

    }

    public static class AgentFishGuide2Offsets
    {

        //7.3
        [Offset("Search 48 8D 05 ? ? ? ? 33 F6 48 89 07 B9 ? ? ? ? Add 3 TraceRelative")]
        [OffsetTC("Search 48 8D 05 ? ? ? ? 48 89 07 48 8D 4F ? 33 C0 48 89 77 ? Add 3 TraceRelative")]
        internal static IntPtr Vtable;

        //6.3 0x58
        [Offset("Search 48 8B 4B ? 44 8B C7 48 8B 41 ? Add 3 Read8")]
        internal static int InfoOffset;

        //0x28
        [Offset("Search 48 8B 41 ? 48 8B 51 ? 48 2B D0 Add 3 Read8")]
        internal static int StartingPointer;

        //0x30
        [Offset("Search 48 8B 51 ? 48 2B D0 48 C1 FA ? 4C 3B C2 Add 3 Read8")]
        internal static int EndingPointer;

    }

    public static class AgentFreeCompanyChestOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? ? ? ? 48 8D 59 ? 48 8D 05 ? ? ? ? 48 89 51 ? 48 89 41 ? 8D 75 Add 3 TraceRelative")]
        internal static IntPtr VTable;

        [Offset("Search E8 ? ? ? ? 8B 8B ? ? ? ? 84 C0 74 ? 83 C9 ? 89 8B ? ? ? ? 48 83 C4 ? 5B C3 F6 C1 ? 0F B6 C0 Add 1 TraceRelative")]
        internal static IntPtr BagRequestCall;

        [Offset("Search 0F B6 7B ? BA ? ? ? ? E8 ? ? ? ? BA ? ? ? ? 89 7C 24 ? Add 3 Read8")]
        internal static int SelectedTabIndex;

        [Offset("Search 0F B6 7B ? 48 8D 4C 24 ? BA ? ? ? ? C7 44 24 ? ? ? ? ? E8 ? ? ? ? 89 7C 24 ? 48 8D 4C 24 ? 0F B6 7B ? Add 3 Read8")]
        internal static int CrystalsTabSelected;

        [Offset("Search 89 83 ? ? ? ? EB ? 48 8B 4B ? 48 8B 01 FF 50 ? 8B 93 ? ? ? ? Add 2 Read32")]
        internal static int GilTabSelected;

        [Offset("Search 88 83 ? ? ? ? EB ? 48 8D 4E ? Add 2 Read32")]
        internal static int GilWithdrawDeposit;

        [Offset("Search 89 83 ? ? ? ? 48 8B CB E8 ? ? ? ? 48 8B 5C 24 ? 40 0F B6 C7 Add 3 Read32")]
        internal static int GilAmountTransfer;

        [Offset("Search 89 BB ? ? ? ? 74 ? 48 8B CB E8 ? ? ? ? 48 8B 5C 24 ? Add 2 Read32")]
        internal static int GilCount;

        [Offset("Search 38 83 ? ? ? ? 0F 84 ? ? ? ? 88 83 ? ? ? ? 48 8B CB Add 2 Read32")]
        internal static int FullyLoaded;

    }

    public static class AgentFreeCompanyOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? 48 8B F9 48 89 01 48 8D 05 ? ? ? ? 48 89 41 ? 48 8D 05 ? ? ? ? 48 89 41 ? 48 81 C1 ? ? ? ? Add 3 TraceRelative")]
        internal static IntPtr VTable;

        [Offset("Search 8B 93 ? ? ? ? 39 93 ? ? ? ? Add 2 Read32")]
        internal static int HistoryCount;

        [Offset("Search 48 8B 41 ? 48 8B 40 ? C3 ? ? ? ? ? ? ? 48 8B 41 ? 48 8B 40 ? C3 ? ? ? ? ? ? ? 48 8B 41 ? 48 8B 40 ? C3 ? ? ? ? ? ? ? 48 89 5C 24 ? Add 3 Read8")]
        internal static int off1;

        [Offset("Search 48 8B 40 ? C3 ? ? ? ? ? ? ? 48 8B 41 ? 48 8B 40 ? C3 ? ? ? ? ? ? ? 48 8B 41 ? 48 8B 40 ? C3 ? ? ? ? ? ? ? 48 89 5C 24 ? Add 3 Read8")]
        internal static int off2;

        [Offset("Search 4C 8B 80 ? ? ? ? 4D 85 C0 0F 84 ? ? ? ? 83 EB ? Add 3 Read32")]
        internal static int off3;

        [Offset("Search 49 8B 40 ? 48 63 D1 0F B7 1C 90 Add 3 Read8")]
        internal static int off4;

        [Offset("Search 8B 70 ? 85 F6 75 ? 8B 91 ? ? ? ? Add 2 Read8")]
        internal static int CurrentCount;

        [Offset("Search 8B 58 ? 85 DB 75 ? 8B 97 ? ? ? ? Add 2 Read8")]
        internal static int ActionCount;

    }

    public static class AgentGoldSaucerInfoOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? 48 89 03 48 8D 4B ? 48 8D 05 ? ? ? ? 48 89 7B ? Add 3 TraceRelative")]
        internal static IntPtr VTable;

    }

    public static class AgentGrandCompanyExchangeOffsets
    {

        //0x
        [Offset("Search 48 8D 05 ? ? ? ? 48 89 01 48 8D 05 ? ? ? ? 48 89 41 ? E9 ? ? ? ? ? ? ? ? ? ? 40 53 48 83 EC ? 48 8B D9 Add 3 TraceRelative")]
        internal static IntPtr Vtable;

        //0x69 byte
        [Offset("Search 0F B6 51 ? 48 8B CB 88 93 ? ? ? ? Add 3 Read8")]
        internal static int Rank;

        //0x6a byte
        [Offset("Search 0F B6 40 ? FE C0 Add 3 Read8")]
        internal static int Category;

        //BuyItem (ShopPtr, 0, index, count)
        [Offset("Search E8 ? ? ? ? 0F B6 D8 84 C0 74 ? 48 8B 0D ? ? ? ? E8 ? ? ? ? 48 8B C8 48 8B 10 FF 92 ? ? ? ? Add 1 TraceRelative")]
        internal static IntPtr BuyItem;

    }

    public static class AgentGrandCompanySupplyOffsets
    {

        //0x
        [Offset("Search 48 8D 05 ? ? ? ? 48 8B D9 48 89 01 E8 ? ? ? ? 48 8D 05 ? ? ? ? 48 8B CB 48 89 43 ? 48 83 C4 ? 5B E9 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 83 79 ? ? Add 3 TraceRelative")]
        internal static IntPtr Vtable;

        //0x68 ptr to GCSupplyItem[]
        [Offset("Search 48 03 5D ? 0F B6 83 ? ? ? ? Add 3 Read8")]
        internal static int ItemArrayStart;

        //7.3
        //0x78 int
        [Offset("Search 44 3B 65 ? 0F 82 ? ? ? ? 44 8B B4 24 ? ? ? ? Add 3 Read8")]
        [OffsetTC("Search 44 3B 65 ? 0F 82 ? ? ? ? 44 8B BC 24 ? ? ? ? Add 3 Read8")]
        internal static int ArrayCount;

        //0x90 byte
        [Offset("Search 66 3B 85 ? ? ? ? 0F 85 ? ? ? ? Add 3 Read32")]
        internal static int HandinType;

        //0x93 byte
        [Offset("Search 0F B6 85 ? ? ? ? 3A C2 Add 3 Read32")]
        internal static int ExpertFilter;

        //0x70 ptr to int[]
        //7.3
        [Offset("Search 49 8B 44 24 ? 8B D3 Add 4 Read8")]
        [OffsetTC("Search 49 8B 46 ? 8B D3 Add 3 Read8")]
        internal static int SortArray;

    }

    public static class AgentHWDScoreOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? 48 89 03 0F 57 C0 C7 43 ? ? ? ? ? Add 3 TraceRelative")]
        internal static IntPtr VTable;

    }

    public static class AgentHandInOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? 48 89 01 48 8D 05 ? ? ? ? 48 89 41 ? 48 8B D9 Add 3 TraceRelative")]
        internal static IntPtr VTable;

        [Offset("Search 48 89 5C 24 ? 48 89 6C 24 ? 48 89 74 24 ? 57 48 83 EC ? 48 8B 41 ? 48 8B E9 48 83 C1 ?")]
        internal static IntPtr HandInFunc;

        [Offset("Search 48 89 41 ? 48 8B D9 48 85 FF Add 3 Read8")]
        internal static int HandinParmOffset;

    }

    public static class AgentHousingOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? C6 47 ? ? 48 89 07 48 8D 4F ? Add 3 TraceRelative")]
        internal static IntPtr VTable;

    }

    public static class AgentHousingSelectBlockOffsets
    {

        [Offset("Search 4C 8D 2D ? ? ? ? 48 89 74 24 ? BD ? ? ? ? Add 3 TraceRelative")]
        internal static IntPtr VTable;

        //7.3
        [Offset("Search 44 0F B6 46 ? 48 8B C8 0F B7 56 ? Add 4 Read8")]
        [OffsetTC("Search 44 0F B6 45 ? 48 8B C8 0F B7 55 ? Add 4 Read8")]
        internal static int WardNumber;

        [Offset("Search 49 8D 4C 24 ? 33 C0 Add 4 Read8")]
        internal static int PlotOffset;

    }

    public static class AgentHousingSignBoardOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? 48 89 7B ? 48 89 03 48 8D 4B ? 66 89 7B ? Add 3 TraceRelative")]
        internal static IntPtr VTable;

        //0x3A
        //7.3
        [Offset("Search 44 88 63 ? 40 88 7B ? Add 3 Read8")]
        [OffsetTC("Search 44 88 73 ? 40 88 7B ? E8 ? ? ? ? Add 3 Read8")]
        internal static int Ward;

        //7.3
        [Offset("Search 40 88 7B ? 66 44 89 43 ? Add 3 Read8")]
        [OffsetTC("Search 40 88 7B ? E8 ? ? ? ? 48 85 C0 74 ? 0F B7 40 ? Add 3 Read8")]
        internal static int Plot;

        //7.3
        [Offset("Search 66 44 89 7B ? 44 88 63 ? Add 4 Read8")]
        [OffsetTC("Search 66 89 73 ? 44 88 73 ? Add 3 Read8")]
        internal static int Zone;

        [Offset("Search 40 88 7B ? 88 43 ? Add 3 Read8")]
        internal static int ForSale;

        [Offset("Search 88 43 ? E8 ? ? ? ? 48 8B 4B ? Add 2 Read8")]
        internal static int Size;

        //7.3
        [Offset("Search 0F 11 4B ? F2 41 0F 10 46 ? Add 3 Read8")]
        [OffsetTC("Search 0F 11 4B ? F2 41 0F 10 45 ? Add 3 Read8")]
        internal static int WinningLotteryNumber;

        //7.3
        [Offset("Search 0F 11 43 ? 41 0F 10 4E ? 0F 11 4B ? F2 41 0F 10 46 ? Add 3 Read32")]
        [OffsetTC("Search 0F 11 43 ? 41 0F 10 4D ? Add 3 Read8")]
        internal static int LotteryEntryCount;

        //7.3
        [Offset("Search 48 89 86 ? ? ? ? 48 8B 01 FF 50 ? 4D 8D 86 ? ? ? ? Add 3 Read32")]
        [OffsetTC("Search 49 89 87 ? ? ? ? 48 8B 01 Add 3 Read32")]
        internal static int FcOwned;

    }

    public static class AgentInclusionShopOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? 48 89 01 48 8D 05 ? ? ? ? 48 89 41 ? 48 8D 05 ? ? ? ? 48 89 41 ? E9 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 40 53 48 83 EC ? 48 8B DA Add 3 TraceRelative")]
        internal static IntPtr Vtable;

        //(__int64 AgentPointer, char Category)
        [Offset("Search 48 8B 41 ? 4C 8B D1 80 88 ? ? ? ? ?")]
        internal static IntPtr SetCategory;

        //0x18
        [Offset("Search 48 8B 49 ? 48 89 6C 24 ? 48 85 C9 Add 3 Read8")]
        internal static int FirstPointer;

        //0x10
        [Offset("Search 48 8B 69 ? 49 8B F0 48 8B DA 48 8B F9 Add 3 Read8")]
        internal static int SecondPointer;

        //0x20
        //7.1
        [Offset("Search 41 8B 44 24 ? 3D ? ? ? ? 74 ? 3D ? ? ? ? 75 ? Add 4 Read8")]
        internal static int ShopKey;

        //0x38
        [Offset("Search 48 8B 4F ? 89 81 ? ? ? ? 48 8B 47 ? C6 80 ? ? ? ? ? Add 3 Read8")]
        internal static int PointerToStartOfShopThing;

        //0x1177
        [Offset("Search 40 3A AB ? ? ? ? 0F 82 ? ? ? ? Add 3 Read32")]
        internal static int NumberOfCategories;

        //0x1223
        //7.1
        [Offset("Search 40 38 B9 ? ? ? ? 0F 86 ? ? ? ? 4C 8B 6C 24 ? Add 3 Read32")]
        internal static int NumberOfSubCategories;

        //0x11D1
        //6.5Done
        [Offset("Search 0F B6 82 ? ? ? ? 4C 6B C0 ? Add 3 Read32")]
        internal static int SubCategory;

        //0x11A8
        [Offset("Search 41 0F B6 80 ? ? ? ? 42 0F B6 94 00 ? ? ? ? Add 4 Read32")]
        internal static int Category;

        //0x1180
        //6.5Done
        [Offset("Search 42 0F B6 94 00 ? ? ? ? 32 C0 Add 5 Read32")]
        internal static int CategoryArray;

        //0x208
        [Offset("Search 4C 03 84 10 ? ? ? ? Add 4 Read32")]
        internal static int SubCategoryArrayStart;

        //0x88
        //6.5
        [Offset("Search 48 69 C1 ? ? ? ? 4C 03 84 10 ? ? ? ? Add 3 Read32")]
        internal static int StructSizeCategory;


        [Offset("Search 0F B6 98 ? ? ? ? E8 ? ? ? ? 4C 8B 7C 24 ? Add 3 Read32")]
        internal static int ItemCount;

        //0x19d0
        //6.5 Done
        [Offset("Search 48 69 D1 ? ? ? ? 4B 8B 8C 01 ? ? ? ? Add 3 Read32")]
        internal static int StructSizeSubCategory;

        //0x6C
        //6.5Done
        [Offset("Search 8B 4B ? 85 C9 74 ? E8 ? ? ? ? 48 85 C0 74 ? 8B 4B ? Add 2 Read8")]
        internal static int StructSizeItem;

        //0x175
        [Offset("Search 43 3A 84 01 ? ? ? ? Add 4 Read32")]
        internal static int CategorySubCount;

        //0x19C9
        [Offset("Search 80 BC 0A ? ? ? ? ? 74 ? 49 8B 52 ? Add 3 Read32")]
        internal static int SubCategoryEnabled;

        //7.1
        [Offset("Search 47 8B 64 B5 ? Add 4 Read8")]
        internal static int ItemStructAdjustment;

    }

    public static class AgentInventoryBuddyOffsets
    {

        //6.4
        [Offset("Search 48 8D 05 ? ? ? ? BE ? ? ? ? 48 89 07 48 8D 5F ? 48 8D 05 ? ? ? ? 33 ED 48 89 47 ? 0F 1F 40 ? Add 3 TraceRelative")]
        internal static IntPtr VTable;

    }

    public static class AgentItemAppraisalOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? 48 89 03 33 C0 48 89 43 ? 48 89 43 ? 48 89 83 ? ? ? ? Add 3 TraceRelative")]
        internal static IntPtr VTable;

        //7.3
        [Offset("Search 66 C7 87 ? ? ? ? ? ? 48 8D 45 ? Add 3 Read8")]
        [OffsetTC("Search 66 C7 87 ? ? ? ? ? ? 48 8D 4D ? Add 3 Read8")]
        internal static int ItemAppraisalReady;

    }

    public static class AgentItemDetailOffsets
    {

        //0x18C9FC0
        //7.2
        [Offset("Search 48 8D 05 ? ? ? ? 48 8B F9 ? ? ? 48 81 C1 ? ? ? ? E8 ? ? ? ? 48 8D 8F ? ? ? ? E8 ? ? ? ? 48 8B 8F Add 3 TraceRelative")]
        internal static IntPtr Vtable;

        //0x5
        [Offset("Search 44 8B 8F ? ? ? ? 4C 8B C6 48 8B D5 48 8B CF E8 ? ? ? ? 4C 8B C6 48 8B D5 48 8B CF 0F B6 D8 E8 ? ? ? ? 84 DB 48 8B 5C 24 ? 75 ? 45 33 C9 C6 44 24 ? ? 45 33 C0 48 8B CE 41 8D 51 ? E8 ? ? ? ? 48 8B 4F ? Add 3 Read32")]
        internal static int ItemID;

    }

    public static class AgentJournalDetailOffsets
    {

        //7.3
        [Offset("Search 48 8D 05 ? ? ? ? 48 89 54 24 ? 48 89 03 Add 3 TraceRelative")]
        [OffsetTC("Search 48 8D 05 ? ? ? ? 48 89 07 48 8D 8F ? ? ? ? C7 87 ? ? ? ? ? ? ? ? Add 3 TraceRelative")]
        internal static IntPtr VTable;

    }

    public static class AgentLookingForGroupOffsets
    {

        [Offset("Search 48 8D 05 ?? ?? ?? ?? 48 8B F1 48 89 01 48 8D 05 ?? ?? ?? ?? 48 89 41 ?? E8 ?? ?? ?? ?? Add 3 TraceRelative")]
        internal static IntPtr VTable;

        //0x2240
        [Offset("Search 48 8D 8B ? ? ? ? 41 B8 ? ? ? ? 48 8B F8 Add 3 Read32")]
        internal static int CommentString;

    }

    public static class AgentMJIGatheringNoteBookOffsets
    {

        //6.4
        [Offset("Search 48 8D 05 ? ? ? ? 48 89 01 E9 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 89 5C 24 ? 48 89 74 24 ? 57 48 83 EC ? 32 C0 49 8B F0 48 83 79 ? ? 48 8B FA 48 8B D9 74 ? 45 85 C9 7E ? 83 6C 24 ? ? 75 ? 49 8B C8 E8 ? ? ? ? 83 F8 ? Add 3 TraceRelative")]
        internal static IntPtr VTable;

    }

    public static class AgentMJIHudOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? 48 C7 43 ? ? ? ? ? 48 89 03 48 8B C3 66 C7 43 ? ? ? 48 83 C4 ? 5B C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 8D 05 ? ? ? ? 48 89 01 E9 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 89 5C 24 ? 48 89 74 24 ? 48 89 7C 24 ? 41 56 48 83 EC ? 48 8B D9 Add 3 TraceRelative")]
        internal static IntPtr VTable;

        [Offset("Search 49 8B 4E ? 0F B6 81 ? ? ? ? 24 ? Add 3 Read8")]
        internal static int InfoPtr;

        [Offset("Search 89 81 ? ? ? ? 49 8B 46 ? 8B 88 ? ? ? ? Add 2 Read32")]
        internal static int CurrentExp;

    }

    public static class AgentMJIPouchOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? 48 89 01 E9 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 89 5C 24 ? 48 89 74 24 ? 57 48 83 EC ? 32 C0 49 8B F0 48 83 79 ? ? 48 8B FA 48 8B D9 74 ? 45 85 C9 7E ? 83 6C 24 ? ? 75 ? 49 8B C8 E8 ? ? ? ? 83 E8 ? Add 3 TraceRelative")]
        internal static IntPtr VTable;

    }

    public static class AgentMJIRecipeNoteBookOffsets
    {

        //6.4
        [Offset("Search 48 8D 05 ? ? ? ? 48 C7 43 ? ? ? ? ? 48 89 03 48 8B C3 48 83 C4 ? 5B C3 ? ? ? ? ? ? ? 48 8D 05 ? ? ? ? 48 89 01 E9 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 89 5C 24 ? 48 89 74 24 ? 57 48 83 EC ? 48 8B D9 48 8B FA 32 C9 Add 3 TraceRelative")]
        internal static IntPtr VTable;

    }

    public static class AgentMeldOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? 33 FF 48 89 03 48 8D 4B ? Add 3 TraceRelative")]
        internal static IntPtr VTable;

        [Offset("Search 0F B6 9F ? ? ? ? 48 8D 8D ? ? ? ? BA ? ? ? ? 44 89 AD ? ? ? ? Add 3 Read32")]
        internal static int CanMeld;

        [Offset("Search 89 83 ? ? ? ? 48 89 83 ? ? ? ? 48 89 83 ? ? ? ? E8 ? ? ? ? 48 8B 8B ? ? ? ? Add 2 Read32")]
        internal static int ItemsToMeldCount;

        //7.3
        [Offset("Search 66 41 89 86 ? ? ? ? E8 ? ? ? ? 85 C0 Add 4 Read32")]
        [OffsetTC("Search 66 89 86 ? ? ? ? E8 ? ? ? ? 85 C0 Add 3 Read32")]
        internal static int IndexOfSelectedItem;

        [Offset("Search 0F BF BE ? ? ? ? 4D 8D 64 24 ? Add 3 Read32")]
        internal static int MateriaCount;

        [Offset("Search 48 8B 85 ? ? ? ? 48 0F BF 95 ? ? ? ? Add 3 Read32")]
        internal static int StructStart;

        [Offset("Search 48 8B 88 ? ? ? ? 4C 8B 04 D1 Add 3 Read32")]
        internal static int ListPtr;

        [Offset("Search 89 86 ? ? ? ? 48 8B CE E8 ? ? ? ? E9 ? ? ? ? Add 2 Read32")]
        internal static int SelectedCategory;

    }

    public static class AgentMinionNoteBookOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? 48 89 03 48 8B C3 48 83 C4 ? 5B C3 ? ? ? ? ? ? ? ? ? 40 53 48 83 EC ? 48 8D 05 ? ? ? ? 48 8B D9 48 89 01 48 81 C1 ? ? ? ? E8 ? ? ? ? 48 8B 4B ? 48 85 C9 74 ? 48 8B 53 ? 41 B8 ? ? ? ? 48 2B D1 48 83 E2 ? E8 ? ? ? ? 33 C0 48 89 43 ? 48 89 43 ? 48 89 43 ? 48 8B CB 48 83 C4 ? 5B E9 ? ? ? ? ? ? ? ? ? ? ? 48 83 EC ? BA ? ? ? ? E8 ? ? ? ? 48 85 C0 74 ? 48 8B 80 ? ? ? ? 8B 40 ? C1 E8 ? F6 D0 Add 3 TraceRelative")]
        internal static IntPtr VTable;

        [Offset("Search 48 8B 73 ? 4C 8B 53 ? Add 3 Read8")] //Could be wrong
        internal static int AgentOffset;

        [Offset("Search 83 3D ? ? ? ? ? 7D ? 32 C0 Add 2 TraceRelative")]
        internal static IntPtr MinionCount;

        //6.4
        [Offset("Search E8 ? ? ? ? 48 85 C0 0F 84 ? ? ? ? 66 83 78 ? ? 0F 86 ? ? ? ? TraceCall")]
        internal static IntPtr GetCompanion;

    }

    public static class AgentOutOnLimbOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? 48 8D 4F ? 48 89 07 E8 ? ? ? ? 33 C9 Add 3 TraceRelative")]
        internal static IntPtr VTable;

        [Offset("Search 41 80 BE ? ? ? ? ? 0F 84 ? ? ? ? BA ? ? ? ? Add 3 Read32")]
        internal static int IsReady;

        [Offset("Search 41 C6 86 ? ? ? ? ? EB ? 41 C6 86 ? ? ? ? ? 4C 8D 9C 24 ? ? ? ? Add 3 Read32")]
        internal static int CursorLocked;

        [Offset("Search 89 9F ? ? ? ? 48 8B 5C 24 ? 89 B7 ? ? ? ? 48 8B 74 24 ? 89 AF ? ? ? ? Add 2 Read32")]
        internal static int DoubleDownRemaining;

        [Offset("Search 48 8B AA ? ? ? ? 48 8B D9 48 85 ED 0F 84 ? ? ? ? 48 8B 89 ? ? ? ? Add 3 Read32")]
        internal static int LastOffset;

        [Offset("Search 48 8B 40 ? 48 8B CF 4C 8B 0F Add 3 Read8")]
        internal static int LastLastOffset;

    }

    public static class AgentRecommendEquipOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? C6 43 ? ? 48 89 03 48 8B C3 C7 43 ? ? ? ? ? Add 3 TraceRelative")]
        internal static IntPtr VTable;

    }

    public static class AgentRetainerCharacterOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? 48 89 06 48 8D 4E ? 48 8D 05 ? ? ? ? 48 89 46 ? E8 ? ? ? ? 33 ED Add 3 TraceRelative")]
        internal static IntPtr VTable;

    }

    public static class AgentRetainerInventoryOffsets
    {

        //7.2
        //48 8D 05 ? ? ? ? 48 89 6E ? 48 89 06 48 8D 9E ? ? ? ?
        [Offset("Search 48 8D 05 ? ? ? ? 48 89 6E ? 48 89 06 48 8D 9E ? ? ? ? Add 3 TraceRelative")]
        [OffsetTC("Search 48 8D 05 ? ? ? ? 48 89 6F ? 48 89 07 48 8D 9F ? ? ? ? Add 3 TraceRelative")]
        internal static IntPtr VTable;

        [Offset("Search 48 8B 8B ? ? ? ? 48 85 C9 74 ? 48 83 C4 ? 5B E9 ? ? ? ? B0 ? Add 3 Read32")]
        internal static int ShopOffset;

    }

    public static class AgentRetainerListOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? 48 8B F1 48 89 01 48 81 C1 ? ? ? ? E8 ? ? ? ? 48 8D 8E ? ? ? ? E8 ? ? ? ? BF ? ? ? ? 48 8D 9E ? ? ? ? 48 83 EB ? 48 8B CB E8 ? ? ? ? 48 83 EF ? 75 ? 48 8B CE Add 3 TraceRelative")]
        internal static IntPtr VTable;

        [Offset("Search 48 8D 8E ? ? ? ? 33 D2 41 B8 ? ? ? ? E8 ? ? ? ? 48 8D 8E ? ? ? ? E8 ? ? ? ? Add 3 Read32")]
        internal static int AgentRetainerOffset;

        [Offset("Search 83 FB ? 72 ? 33 D2 48 8D 4C 24 ? E8 ? ? ? ? 48 8D 15 ? ? ? ? Add 2 Read8")]
        internal static int MaxRetainers;

    }

    public static class AgentRetainerVentureOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? 48 89 03 33 C0 66 89 43 ? 48 89 43 ? 88 43 ? Add 3 TraceRelative")]
        internal static IntPtr VTable;

        //7.3
        [Offset("Search 48 8B 4E ? 4C 8B E0 48 8B 01 Add 3 Read8")]
        [OffsetTC("Search 48 8B 49 ? 48 8B 01 FF 50 ? BA ? ? ? ? 48 8B C8 4C 8B 00 41 FF 50 ? 48 8B 4D ? 4C 8B E8 48 8B 01 Add 3 Read8")]
        internal static int RetainerTask;

    }

    public static class AgentSatisfactionSupplyOffsets
    {

        //6.3
        [Offset("Search 48 8D 05 ? ? ? ? 48 89 6B ? 48 89 03 48 8D BB ? ? ? ? Add 3 TraceRelative")]
        // pre6.3 [OffsetCN("Search 48 8D 05 ? ? ? ? C6 43 ? ? 48 89 03 48 8D 4B ? Add 3 TraceRelative")]
        internal static IntPtr VTable;

        //6.3
        [Offset("Search 4C 8D 83 ? ? ? ? BA ? ? ? ? 48 8D 0D ? ? ? ? E8 ? ? ? ? 84 C0 0F 84 ? ? ? ? 4C 8D 83 ? ? ? ? Add 3 Read8")]
        // pre6.3 [OffsetCN("Search 4C 8D 47 ? BA ? ? ? ? 48 8D 0D ? ? ? ? Add 3 Read8")]
        internal static int DoHItemId;

        //6.3
        [Offset("Search 4C 8D 83 ? ? ? ? BA ? ? ? ? 48 8D 0D ? ? ? ? E8 ? ? ? ? 84 C0 0F 84 ? ? ? ? 4C 8D 83 ? ? ? ? Add 3 Read32")]
        // pre6.3 [OffsetCN("Search 4C 8D 87 ? ? ? ? BA ? ? ? ? 48 8D 0D ? ? ? ? E8 ? ? ? ? 84 C0 0F 84 ? ? ? ? 4C 8D 87 ? ? ? ? Add 3 Read32")]
        internal static int DoLItemId;

        //6.3
        [Offset("Search 8B 8B ? ? ? ? 48 89 83 ? ? ? ? E8 ? ? ? ? 8B 8B ? ? ? ? 48 89 83 ? ? ? ? E8 ? ? ? ? 8B 8B ? ? ? ? Add 2 Read32")]
        // pre6.3 [OffsetCN("Search 4C 8D 87 ? ? ? ? BA ? ? ? ? 48 8D 0D ? ? ? ? E8 ? ? ? ? 84 C0 0F 84 ? ? ? ? 8B 4F ? Add 3 Read32")]
        internal static int FshItemId;

        //7.3
        [Offset("Search 0F B7 7B ? BA ? ? ? ? E8 ? ? ? ? 41 89 BE ? ? ? ? 49 8D 8E ? ? ? ? Add 3 Read8")]
        [OffsetTC("Search 0F B7 73 ? BA ? ? ? ? E8 ? ? ? ? 89 B5 ? ? ? ? 48 8D 8D ? ? ? ? Add 3 Read8")]
        internal static int CurrentRep;

        //7.3
        [Offset("Search 0F B7 7B ? BA ? ? ? ? E8 ? ? ? ? 41 89 BE ? ? ? ? BA ? ? ? ? Add 3 Read8")]
        [OffsetTC("Search 0F B7 73 ? BA ? ? ? ? E8 ? ? ? ? Add 3 Read8")]
        internal static int MaxRep;

        //6.3 broke but it's not using it
        //[Offset("Search 8B 53 ? 4C 8D 43 ? 48 8B CB Add 2 Read8")]
        //[OffsetTC("Search 8B 53 ? 4C 8D 43 ? 48 8B CB Add 2 Read8")]
        //internal static int Npc;

        //7.3
        [Offset("Search 89 43 ? E8 ? ? ? ? 48 8B D0 49 8D 4E ? Add 2 Read8")]
        [OffsetTC("Search 89 43 ? E8 ? ? ? ? 48 8B D0 48 8D 4D ? E8 ? ? ? ? Add 2 Read8")]
        internal static int HeartLevel;

        [Offset("Search 44 0F B6 43 ? 89 43 ? Add 4 Read8")]
        internal static int DeliveriesRemaining;

        [Offset("Search 48 89 5C 24 ? 48 89 6C 24 ? 48 89 74 24 ? 57 48 83 EC ? 48 8B 01 41 0F B6 E9 41 8B F8")]
        internal static IntPtr OpenWindow;
        /*
        #if RB_CN
                    //6.3 changes to no offset
                    [Offset("Search 4C 8D 43 ? 48 8B CB E8 ? ? ? ? BA ? ? ? ? Add 3 Read8")]
                    [OffsetCN("Search 4C 8D 43 ? 48 8B CB E8 ? ? ? ? BA ? ? ? ? Add 3 Read8")]
                    internal static int NpcId;
        #endif
        */

    }

    public static class AgentSharlayanCraftworksSupplyOffsets
    {

        //7.2
        [Offset("Search 48 8D 0D ? ? ? ? 83 7F ? ? 48 89 4C 24 ? 48 8D 0D ? ? ? ? 88 54 24 ? 41 0F 94 C0 ? ? ? 48 89 4C 24 ? 48 8D 4C 24 ? 48 8B 40 ? 0F 94 C2 48 89 44 24 ? E8 ? ? ? ? 48 8D 4C 24 ? E8 ? ? ? ? E9 Add 3 TraceRelative")]
        internal static IntPtr Vtable;

        /*
        [Offset("Search 48 89 5C 24 ? 48 89 6C 24 ? 48 89 74 24 ? 57 48 83 EC ? 48 8B 41 ? 48 8D 71 ? 48 8B F9 41 8B D8 48 8B CE 8B EA FF 50 ? 84 C0 0F 84 ? ? ? ? 48 8B CE E8 ? ? ? ? 84 C0 0F 84 ? ? ? ? 48 8B 47 ? 83 B8 ? ? ? ? ? 0F 85 ? ? ? ? 44 8B C5 4C 89 74 24 ? 8B D3 48 8D 0D ? ? ? ? E8 ? ? ? ? 48 8B D0 48 8B CE 4C 8B F0 E8 ? ? ? ? 84 C0 74 ? 48 8B 7F ? 33 C9 0F B6 57 ? 85 D2 74 ? 0F 1F 80 ? ? ? ? 81 7C CF ? ? ? ? ? 8B D9 74 ? FF C1 3B CA 72 ? EB ? 45 0F B7 46 ? 48 8D 0D ? ? ? ? 41 8B 16 E8 ? ? ? ? 41 8B 06 48 8B CE 89 44 DF ? 41 0F BF 46 ? 89 44 DF ? E8 ? ? ? ? 33 D2 45 33 C9 45 33 C0 8D 4A ? E8 ? ? ? ? 4C 8B 74 24 ? 48 8B 5C 24 ? 48 8B 6C 24 ? 48 8B 74 24 ? 48 83 C4 ? 5F C3 ? ? ? ? ? ? 48 89 5C 24 ? 57 48 83 EC ? 48 8B 01 48 8B DA 48 8B F9 FF 50 ? 84 C0 74 ? 48 85 DB 74 ? 80 7B ? ? 74 ? 48 8B CB E8 ? ? ? ? 48 8B C8 E8 ? ? ? ? 8B C8 EB ? 0F B6 43 ? 8B 4B ? A8 ? 74 ? 81 C1 ? ? ? ? EB ? A8 ? 74 ?")]
        internal static IntPtr HandIn;
        */

        //0x28
        //7.3
        //[OffsetValueNA(0x30)]//V-- seems wild, likely to break every patch? so just use a small definite pattern that has the same chance to break?
        //[OffsetValueCN(0x30)]//V-- seems wild, likely to break every patch? so just use a small definite pattern that has the same chance to break?
        //[OffsetTC("Search FF 50 ? 84 C0 0F 84 ? ? ? ? 48 8D 4F ? E8 ? ? ? ? 84 C0 74 ? 48 8B 47 ? 83 B8 ? ? ? ? ? 75 ? 44 8B C5 48 8D 0D ? ? ? ? 8B D6 E8 ? ? ? ? 48 8B D0 48 8D 4F ? 48 8B F0 E8 ? ? ? ? 84 C0 74 ? 48 8B 4F ? 33 D2 44 0F B6 41 ? 45 85 C0 74 ? 0F 1F 00 81 7C D1 ? ? ? ? ? 74 ? FF C2 41 3B D0 72 ? EB ? 4C 8B C6 48 8D 4F ? E8 ? ? ? ? 33 D2 45 33 C9 45 33 C0 8D 4A ? E8 ? ? ? ? 48 8B 5C 24 ? 48 8B 6C 24 ? 48 8B 74 24 ? 48 83 C4 ? 5F C3 ? ? ? ? ? ? ? ? ? ? ? ? 48 89 5C 24 ? 57 48 83 EC ? 48 8B 01 48 8B DA 48 8B F9 FF 50 ? 84 C0 74 ? 48 85 DB 74 ? 48 8B 03 48 8B CB FF 50 ? 48 8B 4F ? 3B 41 ? 75 ? 48 8B 03 48 8B CB FF 90 ? ? ? ? A8 ? 74 ? 48 8B 03 48 8B CB FF 50 ? 0F B7 C8 EB ? 33 C9 48 8B 47 ? 66 3B 88 ? ? ? ? 72 ? 48 8B CB E8 ? ? ? ? 48 85 C0 75 ? B0 ? 48 8B 5C 24 ? 48 83 C4 ? 5F C3 48 8B 5C 24 ? 32 C0 Add 2 Read8")]
        //        internal static int PointerOffset = 0x30;

    }

    public static class AgentTripleTriadCoinExchangeOffsets
    {

        [Offset("Search 3B 59 ? 0F 83 ? ? ? ? 48 8B 41 ? Add 2 Read8")]
        internal static int CardCount;

        [Offset("Search 48 03 79 ? 41 89 1C 06 Add 3 Read8")]
        internal static int ListPtr;

        [Offset("Search 48 8D 05 ? ? ? ? 48 8B D3 48 8D 4F ? 48 89 07 E8 ? ? ? ? 48 8B 5C 24 ? 33 C0 48 89 47 ? 48 89 47 ? 48 89 47 ? 48 89 47 ? 89 87 ? ? ? ? Add 3 TraceRelative")]
        internal static IntPtr VTable;

        //[Offset("Search 41 54 41 57 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 84 24 ? ? ? ? 48 8B 01")]
        //internal static IntPtr OpenSellWindow;

        //7.3
        [Offset("Search 41 8B 96 ? ? ? ? 49 8B CE 49 8B 46 ? Add 3 Read8")]
        [OffsetTC("Search 8B 96 ? ? ? ? 48 8B CE 48 8B 46 ? Add 2 Read8")]
        internal static int SelectedCardIndex;

        //7.3
        [Offset("Search 49 8B 46 ? 8B 14 90 48 69 D2 ? ? ? ? Add 3 Read8")]
        [OffsetTC("Search 48 8B 46 ? 8B 14 90 Add 3 Read8")]
        internal static int CardIndexArray;

    }

    public static class AgentVoteMVPOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? 48 89 03 33 C0 48 89 43 ? 48 89 43 ? 48 8B C3 48 83 C4 ? 5B C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 40 53 Add 3 TraceRelative")]
        // pre 6.3 [OffsetCN("Search 48 8D 05 ? ? ? ? 48 89 03 33 C0 48 89 43 ? 89 43 ? 48 8B C3 48 83 C4 ? 5B C3 ? ? ? ? ? ? 40 53 Add 3 TraceRelative")]
        internal static IntPtr VTable;

        //7.3
        [Offset("Search 8B 7B ? 44 3B F7 Add 2 Read8")]
        [OffsetTC("Search 8B 5E ? 44 3B F3 Add 2 Read8")]
        internal static int PlayerCount;

        //7.3
        [Offset("Search 48 03 4B ? E8 ? ? ? ? BA ? ? ? ? 48 8B CE 48 8B F8 E8 ? ? ? ? 48 89 7E ? 41 FF C6 Add 3 Read8")]
        [OffsetTC("Search 48 03 4E ? E8 ? ? ? ? 41 8D 56 ? Add 3 Read8")]
        internal static int ArrayStart;

    }

    public static class AgentWorldTravelSelectOffsets
    {

        [Offset("Search 48 8D 05 ? ? ? ? 48 89 03 48 8D 7B ? 48 89 6B ? 48 89 6B ? 8D 75 ? 48 89 6B ? 48 89 6B ? Add 3 TraceRelative")]
        internal static IntPtr VTable;

        [Offset("Search E8 ? ? ? ? 48 85 C0 74 ? 80 78 ? ? 75 ? 8B 44 24 ? Add 1 TraceRelative")]
        [Obsolete]
        internal static IntPtr ExdData__getWorld;

        //6.4
        [Offset("Search 48 8B 43 ? 0F B7 3C B0 Add 3 Read8")]
        internal static int ChoicesOffset;

        //7.3
        [Offset("Search 66 89 7B ? E8 ? ? ? ? 44 0F B7 C7 Add 3 Read8")]
        [OffsetTC("Search 66 89 7B ? E8 ? ? ? ? 0F B7 D0 Add 3 Read8")]
        internal static int CurrentWorldOffset;

        //6.4
        [Offset("Search 3B 43 ? 0F 8F ? ? ? ? 48 8B 4B ? Add 2 Read8")]
        internal static int MaxWorldOffset;

    }

    public static class AtkArrayDataHolderOffsets
    {

        [Offset("Search 41 FF 50 48 ? 8B 4F 08 48 8B F0 48 8B 11 FF 52 40 BA ? ? ? ? Add 4 Read8")]
        internal static int AtkModule_vf9;

        //7.3
        [Offset("Search 4C 8B 41 ? 48 8B C8 41 FF D0 4C 8B E0 48 85 F6 Add 3 Read8")]
        [OffsetTC("Search 41 FF 50 ? 4C 8B E0 48 85 F6 0F 84 ? ? ? ? 48 85 C0 Add 3 Read8")]
        internal static int AtkModule_vfStringArray;

    }

    public static class BagSlotExtensionsOffsets
    {

        [Offset("Search E8 ? ? ? ? 48 8D 8B ? ? ? ? 48 8B 11 Add 1 TraceRelative")]
        public static IntPtr ItemDiscardFunc;

        //7.3
        [Offset("Search E8 ? ? ? ? 45 33 F6 44 89 73 ? 41 C6 47 ? ? E9 ? ? ? ? 44 0F B7 83 ? ? ? ? 48 8D 0D ? ? ? ? 8B 93 ? ? ? ? E8 ? ? ? ? 48 8B CF E8 ? ? ? ? 85 C0 0F 8E ? ? ? ? 48 8B CF E8 ? ? ? ? 44 0F B7 83 ? ? ? ? 48 8D 0D ? ? ? ? 8B 93 ? ? ? ? 44 8B C8 E8 ? ? ? ? 45 33 F6 44 89 73 ? 41 C6 47 ? ? E9 ? ? ? ? 44 0F B7 83 ? ? ? ? Add 1 TraceRelative")]
        [OffsetTC("Search E8 ? ? ? ? 45 33 ED 44 89 6B ? 41 C6 44 24 ? ? E9 ? ? ? ? 44 0F B7 83 ? ? ? ? 48 8D 0D ? ? ? ? 8B 93 ? ? ? ? E8 ? ? ? ? 48 8B CF E8 ? ? ? ? 85 C0 0F 8E ? ? ? ? 48 8B CF E8 ? ? ? ? 44 0F B7 83 ? ? ? ? 48 8D 0D ? ? ? ? 8B 93 ? ? ? ? 44 8B C8 E8 ? ? ? ? 45 33 ED 44 89 6B ? 41 C6 44 24 ? ? E9 ? ? ? ? 44 0F B7 83 ? ? ? ? Add 1 TraceRelative")]
        public static IntPtr ItemLowerQualityFunc;

        [Offset("Search 40 55 53 56 57 41 55 41 57 48 8D 6C 24 ? 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 45 ? 8D B2 ? ? ? ?")]
        public static IntPtr ItemSplitFunc;

        [Offset("Search 48 89 5C 24 ? 56 48 83 EC ? 80 3D ? ? ? ? ? 48 8B F2")]
        public static IntPtr MeldWindowFunc;

        [Offset("Search 48 89 5C 24 ? 48 89 74 24 ? 57 48 83 EC ? 41 0F BF F8")]
        public static IntPtr ExtractMateriaFunc;

        [Offset("Search 48 8D 0D ? ? ? ? 8B D0 E8 ? ? ? ? 83 7E ? ? Add 3 TraceRelative")]
        public static IntPtr ExtractMateriaParam;

        //This client function does desynth, remove materia and reduce depending on the 2nd param
        //7.3
        [Offset("Search E8 ? ? ? ? 33 D2 48 8B CE E8 ? ? ? ? 48 8B BC 24 ? ? ? ? Add 1 TraceRelative")]
        [OffsetTC("Search 48 89 5C 24 ? 48 89 6C 24 ? 48 89 74 24 ? 57 41 54 41 55 41 56 41 57 48 83 EC ? 41 0F BF E9")]
        public static IntPtr RemoveMateriaFunc;

        //7.3
        [Offset("Search BA ? ? ? ? 48 8B CF E8 ? ? ? ? 33 D2 48 8B CE E8 ? ? ? ? 48 8B BC 24 ? ? ? ? Add 1 Read32")]
        [OffsetTC("Search BA ? ? ? ? E8 ? ? ? ? 33 D2 48 8B CE E8 ? ? ? ? 48 8D 55 F0 Add 1 Read32")]
        public static int DesynthId;

        //7.3
        [Offset("Search BA ? ? ? ? 44 8B 07 48 8B C8 C7 44 24 ? ? ? ? ? E8 ? ? ? ? 48 8B 5C 24 ? 48 83 C4 ? 5F C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 89 5C 24 ? Add 1 Read32")]
        [OffsetTC("Search BA ? ? ? ? E8 ? ? ? ? 48 8B 7C 24 ? 48 8B 5C 24 ? 84 C0 Add 1 Read32")]
        public static int ReduceId;

        //7.3
        [Offset("Search 0F B7 44 7B ? 66 85 C0 0F 84 ? ? ? ? 48 8B 74 24 ? Add 4 Read8")]
        [OffsetTC("Search 0F B7 44 7B ? 66 85 C0 0F 84 ? ? ? ? 4C 8B 74 24 ? Add 4 Read8")]
        public static int BagSlotMateriaType;

        //7.3
        [Offset("Search 0F B6 44 18 ? 0F B6 C0 0F BF 74 46 ? Add 4 Read8")]
        [OffsetTC("Search 0F B6 44 18 ? 4D 8B 37 Add 4 Read8")]
        public static int BagSlotMateriaLevel;

        //7.3
        [Offset("Search BA ? ? ? ? 48 8B CF E8 ? ? ? ? EB ? 48 8B 01 Add 1 Read32")]
        [OffsetTC("Search BA ? ? ? ? E8 ? ? ? ? EB ? 48 8B 01 48 8B D9 Add 1 Read32")]
        public static int RemoveMateriaId;

        [Offset("Search 48 89 6C 24 ? 48 89 74 24 ? 57 48 83 EC ? 83 B9 ? ? ? ? ? 41 8B F0 8B EA 48 8B F9 0F 85 ? ? ? ?")]
        public static IntPtr TradeBagSlot;

        [Offset("Search 48 89 5C 24 ? 48 89 6C 24 ? 48 89 74 24 ? 57 48 83 EC 40 8B CA ")]
        public static IntPtr BagSlotUseItem;

        [Offset("Search 48 89 6C 24 ? 56 41 56 41 57 48 83 EC ? 45 8B F9 45 0F B7 F0")]
        public static IntPtr RemoveFromSaddle;

        [Offset("Search 48 89 5C 24 ? 48 89 6C 24 ? 48 89 74 24 ? 57 48 83 EC ? 45 33 DB 41 8B F9 45 8B D3 41 0F B7 F0 8B EA 48 8B D9 48 8B C1 0F 1F 80 ? ? ? ? 80 38 ? 75 ? 41 FF C3 49 FF C2 48 83 C0 ? 49 81 FA ? ? ? ? 7C ? EB ? 49 63 C3 48 6B D0 ? 48 03 D3 C6 02 ? 74 ? C7 42 ? ? ? ? ? 44 8B C7 89 6A ? 66 89 72 ? 89 7A ? 8B 81 ? ? ? ? 89 42 ? 0F B7 D6 44 8B 89 ? ? ? ? 8B CD E8 ? ? ? ? 8B 8B ? ? ? ? B8 ? ? ? ? FF C1 F7 E1 8B C1 2B C2 ? ? 03 C2 C1 E8 ? 69 C0 ? ? ? ? 2B C8 0F BA E9 ? 89 8B ? ? ? ? 48 8B 5C 24 ? 48 8B 6C 24 ? 48 8B 74 24 ? 48 83 C4 ? 5F C3 ? ? ? ? ? ? ? ? 66 83 FA ?")]
        public static IntPtr RetainerRetrieveQuantity;

        //7.3
        [Offset("Search E8 ? ? ? ? 33 D2 45 33 C9 45 33 C0 8D 4A ? E8 ? ? ? ? E9 ? ? ? ? 48 8D 4F ? Add 1 TraceRelative")]
        [OffsetTC("Search E8 ? ? ? ? 33 D2 45 33 C9 45 33 C0 8D 4A ? E8 ? ? ? ? EB ? 48 8D 4B ? Add 1 TraceRelative")]
        public static IntPtr EntrustRetainerFunc;

        [Offset("Search 48 89 6C 24 ? 48 89 74 24 ? 57 48 83 EC ? 80 B9 ? ? ? ? ? 41 8B F0")]
        public static IntPtr SellFunc;

        [Offset("Search 48 89 5C 24 ? 48 89 6C 24 ? 48 89 74 24 ? 41 56 48 83 EC ? 45 8B F1")]
        public static IntPtr AddToSaddle;

        //7.3
        [Offset("Search E8 ? ? ? ? EB ? 89 AB ? ? ? ? Add 1 TraceRelative")]
        [OffsetTC("Search E8 ? ? ? ? EB ? 44 89 B3 ? ? ? ? Add 1 TraceRelative")]
        public static IntPtr FCChestMove;

        [Offset("Search 48 89 6C 24 ? 48 89 74 24 ? 57 48 83 EC ? 48 63 F2 48 8B F9")]
        public static IntPtr PlaceAetherWheel;

        [Offset("Search 48 8B 05 ? ? ? ? 48 85 C0 74 ? 83 B8 ? ? ? ? ? 75 ? E8 ? ? ? ? Add 3 TraceRelative")]
        public static IntPtr EventHandlerOff;

        [Offset("Search 48 89 5C 24 ? 48 89 74 24 ? 57 48 83 EC ? 8B CA 41 0F BF F0")]
        internal static IntPtr MeldItem;

        [Offset("Search E8 ? ? ? ? 84 C0 74 ? C6 87 ? ? ? ? ? 48 8B 5C 24 ? 48 8B 74 24 ? 48 83 C4 ? 5F C3 ? ? ? ? ? ? ? ? ? 41 56 TraceCall")]
        internal static IntPtr DyeItem;

        [Offset("Search 66 89 43 ? 8B 47 ? 89 43 ? 0F B6 47 ? 88 43 ? E8 ? ? ? ? 85 C0 Add 3 Read8")]
        public static int StainId;

        [Offset("Search 40 55 41 55 41 57 48 8D 6C 24 ? 48 81 EC ? ? ? ? 83 B9 ? ? ? ? ?")]
        public static IntPtr StoreroomToInventory;

        [Offset("Search E8 ? ? ? ? 48 89 BB ? ? ? ? 83 BB ? ? ? ? ? TraceCall")]
        public static IntPtr InventoryToStoreroom;

        [Offset("Search E8 ? ? ? ? 89 83 ? ? ? ? C7 44 24 ? ? ? ? ? TraceCall")]
        internal static IntPtr GetPostingPriceSlot;



    }

    public static class BeastTribeHelperOffsets
    {

        [Offset("Search E8 ? ? ? ? BA ? ? ? ? 48 8B C8 48 83 C4 ? E9 ? ? ? ? ? ? ? ? ? ? E9 ? ? ? ? TraceCall")]
        internal static IntPtr GetQuestPointer;

        [Offset("Search 48 8D 81 ? ? ? ? 66 0F 1F 44 00 ? 66 39 50 ? 74 ? 41 FF C0 Add 3 Read32")]
        internal static int DailyQuestOffset;

        [Offset("Search 41 83 F8 ? 72 ? 32 C0 C3 0F B6 40 ? Add 3 Read8")]
        internal static int DailyQuestCount;

        //7.2
        [Offset("Search E8 ? ? ? ? 48 85 C0 74 ? 3A 58 ? 73 ? TraceCall")]
        [OffsetTC("Search E8 ? ? ? ? 48 85 C0 74 ? 3A 58 ? TraceCall")]
        internal static IntPtr GetBeastTribeExd;

        [Offset("Search E8 ? ? ? ? 4C 8B C8 EB ? 4C 8D 0D ? ? ? ? TraceCall")]
        internal static IntPtr ResolveStringColumnIndirection;

        //7.1
        [Offset("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? C6 84 24 ? ? ? ? ?  Add 3 TraceRelative")]
        internal static IntPtr QuestPointer;

        //6.4
        [Offset("Search 48 81 C1 ? ? ? ? 48 03 C9 0F B6 1C C8 Add 3 Read32")]
        internal static int BeastTribeStart;

        [Offset("Search 66 89 BC C8 ? ? ? ? Add 4 Read32")]
        internal static int BeastTribeRep;

        //6.4
        [Offset("Search 83 FB ? 73 ? E8 ? ? ? ? 8B CB 48 81 C1 ? ? ? ? 48 03 C9 0F B6 1C C8 Add 2 Read8")]
        internal static int BeastTribeCount;

    }

    public static class BlueMageSpellBookOffsets
    {

        [Offset("Search 48 8D 0D ? ? ? ? 8B D3 E8 ? ? ? ? 45 84 F6  Add 3 TraceRelative")]
        internal static IntPtr ActionManager;

        [Offset("Search E8 ? ? ? ? 45 84 F6 74 2F TraceCall")]
        internal static IntPtr SetSpell;

        [OffsetTC("Search 83 FE ? 0F 87 ? ? ? ? 48 89 58 ? Add 2 Read8")]
        [Offset("Search 83 FA ? 77 ? 48 63 C2 8B 84 81 ? ? ? ? C3 33 C0 C3 ? ? ? ? ? ? ? ? ? ? ? ? ? 48 89 6C 24 ? Add 2 Read8")]
        internal static int MaxActive;

        [OffsetTC("Search 83 FA ? 77 ? 48 63 C2 8B 84 81 Add 11 Read32")]
        [Offset("Search 8B 84 81 ? ? ? ? C3 33 C0 C3 ? ? ? ? ? ? ? ? ? ? ? ? ? 48 89 6C 24 ? Add 3 Read32")]
        internal static int BluSpellActiveOffset;

    }

    public static class CraftingHelperOffsets
    {

        [Offset("Search 4C 8D 0D ? ? ? ? 4C 8B 11 44 0F B7 41 ?  Add 3 TraceRelative")]
        internal static IntPtr DohLastAction;

        [Offset("Search 40 53 48 83 EC ? 8B D9 81 F9 ? ? ? ?")]
        internal static IntPtr HasCraftedRecipe;

        //7.1
        [Offset("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? C6 84 24 ? ? ? ? ? Add 3 TraceRelative")]
        internal static IntPtr QuestPointer;

        [Offset("Search 81 F9 ? ? ? ? 73 38 Add 2 Read32")]
        internal static int NumberOfRecipes;

        [Offset("Search 81 F9 ? ? ? ? 73 ? 44 0F B6 84 01 ? ? ? ?  Add 2 Read32")]
        internal static int LengthOfArray;

        [Offset("Search 44 0F B6 84 01 ? ? ? ? 0F B6 CB  Add 5 Read32")]
        internal static int OffsetRecipes;

    }

    public static class CurrencyHelperOffsets
    {

        [Offset("Search 48 8B 1D ? ? ? ? 48 85 DB 74 27 48 8D 4B 20 E8 ? ? ? ? 48 8D 4B 10 E8 ? ? ? ? 48 8B CB E8 ? ? ? ? BA ? ? ? ? 48 8B CB E8 ? ? ? ? 33 D2 Add 3 TraceRelative")]
        internal static IntPtr SpecialCurrencyStorage;

        [Offset("Search E8 ? ? ? ? 41 89 47 0C TraceCall")]
        internal static IntPtr GetSpecialCurrencyItemId;

        [Offset("Search 48 89 5C 24 ? 48 89 74 24 ? 57 48 83 EC ? 8B F1 E8 ? ? ? ? 33 DB 8B F8 85 C0 74 ? 66 90 8B CB E8 ? ? ? ? 39 70 ? 74 ? FF C3 3B DF 72 ? 33 C0 48 8B 5C 24 ? 48 8B 74 24 ? 48 83 C4 ? 5F C3 8B 00")]
        internal static IntPtr GetTomeItemId;

    }

    public static class DirectorHelperOffsets
    {
        [Offset("Search E8 ? ? ? ? 89 6C 24 38 44 8B F5 TraceCall")]
        public static IntPtr GetTodoArgs;

        [Offset("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? 89 6C 24 38 Add 3 TraceRelative")]
        public static IntPtr ActiveDirector;
    }

    public static class EventNpcExtensionsOffsets
    {

        [Offset("Search 44 0F 47 F3 44 89 B7 ? ? ? ? Add 7 Read32")]
        internal static int IconID;

    }

    public static class FreeCompanyChestOffsets
    {
        // For Dawntrail I assume these are in the same order.
        [Offset("Search 89 91 ? ? ? ? 4C 8B F1 44 89 81 ? ? ? ? Add 2 Read32")]
        internal static int ItemPermissions;

        [Offset("Search 44 89 81 ? ? ? ? 44 89 89 ? ? ? ? 48 8D B1 ? ? ? ? Add 3 Read32")]
        internal static int CrystalsPermission;

        [Offset("Search 44 89 89 ? ? ? ? 48 8D B1 ? ? ? ? Add 3 Read32")]
        internal static int GilPermission;
    }

    public static class GardenHelperOffsets
    {

        [Offset("Search E8 ? ? ? ? 48 8B CB C7 43 ? ? ? ? ? E8 ? ? ? ? 48 8B 5C 24 ? B0 01 TraceCall")]
        internal static IntPtr PlantFunction;

        [Offset("Search 41 8B 4E ? 8D 93 ? ? ? ? Add 3 Read8")]
        internal static int StructOffset;

    }

    public static class GrandCompanyShopOffsets
    {

        [Offset("Search 0F B6 0D ? ? ? ? FE C9  Add 3 TraceRelative")]
        internal static IntPtr CurrentGC;

        [Offset("Search 48 83 EC ? 48 8B 05 ? ? ? ? 44 8B C1 BA ? ? ? ? 48 8B 88 ? ? ? ? E8 ? ? ? ? 48 85 C0 75 ? 48 83 C4 ? C3 48 8B 00 48 83 C4 ? C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 83 EC ? 80 F9 ?")]
        internal static IntPtr GCGetMaxSealsByRank;

        //7.3
        [Offset("Search 48 8D 9E ? ? ? ? 4C 89 AC 24 ? ? ? ? 45 32 E4 Add 3 Read32")]
        [OffsetTC("Search 48 8D 9E ? ? ? ? 4C 89 A4 24 ? ? ? ? Add 3 Read32")]
        internal static int GCArrayStart;

        [Offset("Search 83 F8 ? 0F 82 ? ? ? ? 41 0F B6 97 ? ? ? ? Add 2 Read8")]
        internal static int GCShopCount;

        [Offset("Search 48 8B 05 ? ? ? ? 33 C9 40 84 FF 48 0F 45 C1 48 89 05 ? ? ? ? Add 3 TraceRelative")]
        internal static IntPtr GCShopPtr;

    }

    public static class GrandCompanySupplyListOffsets
    {

        //0x
        [Offset("Search E8 ? ? ? ? 49 8D 8F ? ? ? ? E8 ? ? ? ? 48 8B 8B ? ? ? ? 8B F8 TraceCall")]
        internal static IntPtr SetFilter;

    }

    public static class GuildLeveOffsets
    {

        [Offset("Search 88 05 ? ? ? ? E8 ? ? ? ? 48 8B C8 48 83 C4 ? Add 2 TraceRelative")]
        public static IntPtr AllowancesPtr;

    }

    public static class HWDLotteryOffsets
    {


        [Offset("Search E8 ? ? ? ? 32 C0 48 8B 5C 24 ? 48 8B 74 24 ? 48 83 C4 ? 5F C3 48 8B CB E8 ? ? ? ? 32 C0 TraceCall")]
        internal static IntPtr KupoFunction;

    }

    public static class HousingHelperOffsets
    {

        [Offset("Search 48 8B 0D ? ? ? ? E8 ? ? ? ? 0F B6 F8 84 C0 75 18 44 8B C5 Add 3 TraceRelative")]
        internal static IntPtr PositionInfoAddress;

        [Offset("Search 48 8B 1D ? ? ? ? 48 83 FB FF Add 3 TraceRelative")] // Needs to be tested
        internal static IntPtr HouseLocationArray;

        [Offset("Search E8 ? ? ? ? BA ? ? ? ? 48 8B F8 8D 4A 02 TraceCall")]
        internal static IntPtr GetCurrentHouseId;

        [Offset("Search E8 ?? ?? ?? ?? 0F B6 D8 3C FF TraceCall")]
        internal static IntPtr GetCurrentPlot;

        [Offset("Search 48 8B 41 ? 48 85 C0 74 ? 8B 80 ? ? ? ? 48 C1 E8 ?")]
        internal static IntPtr GetCurrentWard;

    }

    public static class HousingSelectBlockOffsets
    {
        [Offset("Search 89 87 ? ? ? ? 8B D0 48 39 B7 ? ? ? ? Add 2 Read32")]
        internal static int EligibilityArray;
    }

    public static class HuntHelperOffsets
    {

        [Offset("Search 48 8D 8B ? ? ? ? 48 83 FE 16 Add 3 Read32")]
        internal static int AcceptedHuntBitfieldOffset;

        [Offset("Search 48 89 5C 24 ? 56 48 83 EC 20 0F B6 DA 40 32 F6")]
        internal static IntPtr CheckMobBoardUnlocked;

        [Offset("Search 48 83 EC 28 48 8B 05 ? ? ? ? 44 8B C1 BA 2f 00 00 00 48 8B 88 ? ? ? ? E8 ? ? ? ? 48 85 C0 75 05 48 83 C4 28 ")]
        internal static IntPtr Client__ExdData__getBNpcName;

        [Offset("Search E8 ? ? ? ? 48 85 C0 74 ? 0F B6 40 ? 3B E8 TraceCall")]
        internal static IntPtr Client__ExdData__getMobHuntOrder;

        //7.3
        [Offset("Search E8 ? ? ? ? 48 85 C0 74 ? 0F B7 40 ? 66 85 C0 74 ? FE CB 0F B6 CB 03 C8 TraceCall")]
        [OffsetTC("Search E8 ?? ?? ?? ?? 48 85 C0 74 ?? 0F B7 40 ?? 66 85 C0 74 ?? 40 FE CF TraceCall")]
        internal static IntPtr Client__ExdData__getMobHuntOrderType;

        //7.3
        [Offset("Search E8 ? ? ? ? 48 89 45 ? 48 8B D8 48 85 C0 0F 84 ? ? ? ? 48 8D 4D ? TraceCall")]
        [OffsetTC("Search E8 ? ? ? ? 4C 8B E8 48 85 C0 0F 84 ? ? ? ? 48 8D 4D ? TraceCall")]
        internal static IntPtr Client__ExdData__getMobHuntTarget;

        [Offset("Search 41 80 FE ? 0F 83 ? ? ? ? 41 0F B6 44 0E ? 48 89 74 24 ? 41 8B F6 48 89 7C 24 ? 4C 89 64 24 ? Add 2 Read8")]
        internal static IntPtr CountMobHuntOrderType;

        [Offset("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? 8B CF E8 ? ? ? ? Add 3 TraceRelative")]
        internal static IntPtr HuntData;

        [Offset("Search 42 8B 44 89 ? C3 Add 4 Read8")]
        internal static int KillCountOffset;

        [Offset("Search E8 ? ? ? ? 8B CF E8 ? ? ? ? 0F B7 D0 TraceCall")]
        internal static IntPtr YeetHuntOrderType;

    }

    public static class InputStringOffsets
    {

        [Offset("Search 48 8B 81 ? ? ? ? 48 85 C0 75 ? 48 8B 81 ? ? ? ? 48 85 C0 74 ? 48 8B 80 ? ? ? ? Add 3 Read32")]
        internal static int AtkComponentTextInput; //0x230

        [Offset("Search 48 8B 80 ? ? ? ? 80 38 ? 0F 95 C0 84 C0 75 ? 49 8B 00 Add 3 Read32")]
        internal static int StringPtr; //0xE0

        [Offset("Search 48 8D 1D ? ? ? ? BA ? ? ? ? 48 8D 4D ? E8 ? ? ? ? 4C 8D 45 ? Add 3 TraceRelative")]
        internal static IntPtr UnkStatic;

    }

    public static class InstanceQuestDungeonHookOffsets
    {

        //4? 55 53 57 4? 8d ?? ?4 c9 4? 81 ec ?? ?? ?? ?? 4? 8b 05 ?? ?? ?? ??
        // +9 = patch location // + 16 = hook location
        [Offset("Search 40 55 53 57 48 8d ? ? c9 48 81 ec ? ? ? ? 48 8b 05 ? ? ? ? 48 33 c4 48 89 45 ? 8b c2 41 8b f9 48 8b d9 2d ? ? ? ?")]
        internal static IntPtr PatchLocation;

        [Offset("Search 40 55 53 57 48 8d ? ? c9 48 81 ec ? ? ? ? 48 8b 05 ? ? ? ? 48 33 c4 48 89 45 ? 8b c2 41 8b f9 48 8b d9 2d ? ? ? ? Add C Read32")]
        public static IntPtr SubAmt;

    }

    public static class InventoryUpdatePatchOffsets
    {

        [Offset("Search E9 ? ? ? ? 48 83 C4 ? C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 8B 0D ? ? ? ?")]
        internal static IntPtr PatchLocation;

        [IgnoreCache]
        [Offset("Search E9 ? ? ? ? 48 83 C4 ? C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 8B 0D ? ? ? ? TraceCall")]
        internal static IntPtr OriginalJump;

        //7.1
        [Offset("Search E8 ? ? ? ? 33 DB FF C6 48 8D 3D ? ? ? ? Add 1 TraceRelative")]
        internal static IntPtr OrginalCall;

    }

    public static class ItemFinderOffsets
    {

        [Offset("Search 48 8B 0D ?? ?? ?? ?? 48 8B DA E8 ?? ?? ?? ?? 48 85 C0 74 ?? 4C 8B 00 48 8B C8 41 FF 90 ?? ?? ?? ?? 48 8B C8 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 85 C0 74 ?? 4C 8B 00 48 8B D3 48 8B C8 48 83 C4 ?? 5B 49 FF 60 ?? 48 83 C4 ?? 5B C3 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 40 53 Add 3 TraceRelative")]
        internal static IntPtr GFramework2;

        [Offset("Search E8 ? ? ? ? 48 8B D8 48 85 C0 74 ? 66 85 FF TraceCall")]
        internal static IntPtr GetUiModule;

        //Broken pattern but it should be 0x88
        [Offset("Search 48 FF A0 88 00 00 00 49 8B 00 Add 3 Read8")] //Yes it's hard coded but just as a refrence since this isn't used anywhere.
        internal static int GetRaptureItemFinder;

        //7.1
        [Offset("Search 49 8B 86 ? ? ? ? 49 8D 8E ? ? ? ? 33 D2 FF 50 ? 41 0F B6 86 ? ? ? ? Add A Read32")]
        internal static int RaptureItemFinder;

        [Offset("Search 49 8B 8F ? ? ? ? 48 89 B4 24 ? ? ? ? 48 8B D9 Add 3 Read32")]
        internal static int TreeStartOff;

        [Offset("Search 48 8D 83 ? ? ? ? 48 89 74 24 ? 48 8D 8B ? ? ? ? Add 3 Read32")]
        internal static int SaddleBagItemIds;

        [Offset("Search 48 8D 8B ? ? ? ? 48 89 7C 24 ? 4C 89 64 24 ? Add 3 Read32")]
        internal static int SaddleBagItemQtys;

        //7.1
        [Offset("Search 49 8D 9D ? ? ? ? BF ? ? ? ? 0F 1F 40 ? Add 3 Read32")]
        internal static int GlamourDresserItemIds;

        //7.3
        [Offset("Search 80 B9 ? ? ? ? ? 48 8B D9 74 ? 48 83 C4 ? Add 2 Read32")]
        [OffsetTC("Search 80 B9 ? ? ? ? ? 48 8B E9 74 Add 2 Read32")]
        internal static int GlamourDresserCached;

    }

    public static class LocalPlayerExtensionsOffsets
    {

        /*[Offset("Search 44 88 84 0A ? ? ? ? C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 88 91 ? ? ? ? Add 4 Read32")]
        internal static int GatheringStateOffset;*/

        [Offset("Search 0F B6 0D ? ? ? ? FE C9  Add 3 TraceRelative")]
        internal static IntPtr CurrentGC;

        [Offset("Search 48 83 EC ? 48 8B 05 ? ? ? ? 44 8B C1 BA ? ? ? ? 48 8B 88 ? ? ? ? E8 ? ? ? ? 48 85 C0 75 ? 48 83 C4 ? C3 48 8B 00 48 83 C4 ? C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 83 EC ? 80 F9 ?")]
        internal static IntPtr GCGetMaxSealsByRank;

        //PlayerID 8byte ulong ID unique to that character which is included in MB listings
        [Offset("Search 48 8B 05 ? ? ? ? 48 8D 0D ? ? ? ? 41 8B DC Add 3 TraceRelative")]
        internal static IntPtr PlayerId;

        //Useless as of 7.2, kept for reference. New method is in AccountIdLocation which only works on the current account (not other players) and still works on CN 7.1
        //[Offset("Search 48 89 B3 ? ? ? ? 48 89 B3 ? ? ? ? 48 8B 74 24 ? 89 BB ? ? ? ? Add 3 Read32")]
        //internal static int AccountId;

        [Offset("Search 48 8B 3D ? ? ? ? 48 85 FF 74 ? 48 89 5C 24 ? Add 3 TraceRelative")]
        internal static IntPtr AccountIdLocation;

        [Offset("Search 48 89 77 ? C7 07 ? ? ? ? Add 3 Read8")]
        internal static int AccountIdOffset;

        [Offset("Search 0F B6 05 ? ? ? ? 88 83 ? ? ? ? Add 3 TraceRelative")]
        internal static IntPtr RunWalk;

        [Offset("Search 48 8B 8F ? ? ? ? 48 85 C9 74 ? 48 8B 01 FF 90 ? ? ? ? 84 C0 Add 3 Read32")]
        internal static int MinionPtr;

        [Offset("Search 41 0F B7 86 ? ? ? ? 66 89 86 ? ? ? ? 48 8B 0D ? ? ? ? Add 4 Read32")]
        internal static int HomeWorld;

        [Offset("Search 48 8B DA 66 83 B9 ? ? ? ? ?  Add 6 Read32")]
        internal static int CurrentMount;

    }

    public static class LookingForGroupConditionOffsets
    {
        //7.3
        [Offset("Search BA ? ? ? ? 48 8B 8B ? ? ? ? E8 ? ? ? ? 41 8B 86 ? ? ? ? Add 8 Read32")]
        [OffsetTC("Search BA ? ? ? ? 48 8B 8B ? ? ? ? E8 ? ? ? ? 49 8D 8E ? ? ? ? Add 8 Read32")]
        internal static int AtkComponentTextInputNodePtr;

        [Offset("Search 48 8D 97 ? ? ? ? 48 8B 05 ? ? ? ? 33 F6 Add 3 Read32")]
        internal static int TextFieldPtr;
    }

    public static class LookingForGroupOffsets
    {
        [Offset("Search 83 B8 ? ? ? ? ? 7D ? 48 8D 9E ? ? ? ? Add 2 Read32")]
        internal static int ResultCountIndex;

        //7.3
        [Offset("Search BA ? ? ? ? 48 8B 08 4C 8B 41 ? 48 8B C8 41 FF D0 48 8B F8 48 85 C0 0F 84 ? ? ? ? 45 33 C0 Add 1 Read8")]
        [OffsetTC("Search BA ? ? ? ? 48 8B C8 4C 8B 00 41 FF 50 ? 48 8B F8 48 85 C0 0F 84 ? ? ? ? 45 33 C0 Add 1 Read8")]
        internal static int NumberArrayIndex;

        [Offset("Search 48 8B 41 ? 48 63 D2 44 39 04 90 Add 3 Read8")]
        internal static int NumberArrayData_IntArray;
    }

    public static class NpcHelperOffsets
    {

        //7.1
        [Offset("Search E8 ? ? ? ? 0F B6 48 ? 85 C9 0F 84 ? ? ? ? TraceCall")]
        internal static IntPtr GetENpcResident;

    }

    public static class OutOnALimbDirectorOffsets
    {

        [Offset("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? 48 8B D0 48 8D 0D ? ? ? ? B8 ? ? ? ? Add 3 TraceRelative")]
        internal static IntPtr ActiveDirectorPtr;

        //7.3
        [Offset("Search 48 89 5C 24 ? 57 48 83 EC ? 48 8B 39 48 8B 07")]
        [OffsetTC("Search 48 89 5C 24 ? 57 48 83 EC ? 48 8B 01 48 8B F9 48 8B 18 E8 ? ? ? ?")]
        internal static IntPtr RemainingTimeFunction;

        //7.3
        [Offset("Search 89 86 ? ? ? ? 0F B6 45 ? 88 86 ? ? ? ? 8B 45 ? Add 2 Read32")]
        [OffsetTC("Search 89 87 ? ? ? ? 0F B6 46 ? 88 87 ? ? ? ? 8B 46 ? 89 87 ? ? ? ? 8B 46 ? Add 2 Read32")]
        internal static int SwingResult;

        //7.3
        [Offset("Search 89 86 ? ? ? ? 8B 45 ? 89 86 ? ? ? ? 0F B6 86 ? ? ? ? Add 2 Read32")]
        [OffsetTC("Search 89 87 ? ? ? ? 8B 46 ? 89 87 ? ? ? ? 0F B6 87 ? ? ? ? Add 2 Read32")]
        internal static int CurrentPayout;

        //7.3
        [Offset("Search 89 86 ? ? ? ? 0F B6 86 ? ? ? ? 48 6B D0 ? Add 2 Read32")]
        [OffsetTC("Search 89 87 ? ? ? ? 0F B6 87 ? ? ? ? 48 6B D0 ? Add 2 Read32")]
        internal static int DoubleDownPayout;

        //7.3
        [Offset("Search 66 89 86 ? ? ? ? 8B 96 ? ? ? ? Add 3 Read32")]
        [OffsetTC("Search 66 89 87 ? ? ? ? 8B 97 ? ? ? ? Add 3 Read32")]
        internal static int ProgressNeeded;

        //7.3
        [Offset("Search C6 86 ? ? ? ? ? 8B 45 ? Add 2 Read32")]
        [OffsetTC("Search C6 87 ? ? ? ? ? 8B 46 ? 89 87 ? ? ? ? 83 7E ? ? Add 2 Read32")] //7.2
        internal static int SwingsTaken;

        [Offset("Search 80 3D ? ? ? ? ? 0F 84 ? ? ? ? 48 8D 0D ? ? ? ? E8 ? ? ? ? 84 C0 74 ? Add 2 TraceRelative")]
        internal static IntPtr IsActiveByte;

    }

    public static class PointMenuManagerOffsets
    {

        //7.3
        [Offset("Search 48 8B 89 ? ? ? ? 4D 8B F1 48 8B 87 ? ? ? ? Add 3 Read32")]
        [OffsetTC("Search 48 8B 89 ? ? ? ? 49 8B E9 41 8B F0 Add 3 Read32")]
        internal static int ObjectCount;


    }

    public static class RaceChocoboManagerOffsets
    {

        //7.3
        [Offset("Search 48 8D 0D ? ? ? ? 8B DA E8 ? ? ? ? 48 85 C0 75 ? 48 83 C4 ? 5B C3 8B D3 48 8B C8 E8 ? ? ? ? B0 ? 48 83 C4 ? 5B C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 40 53 Add 3 TraceRelative")]
        [OffsetTC("Search 48 8d 0d ?? ?? ?? ?? 0f b7 d8 e8 ?? ?? ?? ?? 0f b7 Add 3 TraceRelative")]
        internal static IntPtr Instance;

    }

    public static class RelicBookManagerOffsets
    {

        [Offset("Search 48 8D 0D ? ? ? ? 8B D3 E8 ? ? ? ? 48 8B 4C 24 ? Add 3 TraceRelative")]
        internal static IntPtr UIRelicNote;

        [Offset("Search 48 89 5C 24 ? 48 89 6C 24 ? 48 89 74 24 ? 57 41 56 41 57 48 83 EC 20 45 8B F8 ")]
        internal static IntPtr GetNumOfRelicNoteCompleted;

    }

    public static class RequestHelperOffsets
    {

        [Offset("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? 44 0F B6 E8 EB ? Add 3 TraceRelative")]
        internal static IntPtr RequestInfo;

        //7.3
        [Offset("Search 44 8B 44 CB ? 48 8B 8B ? ? ? ? E8 ? ? ? ? 48 8B 8B ? ? ? ? 48 8B 93 ? ? ? ? 48 8B 01 48 8B 5C 24 ? 48 83 C4 ? 5F 48 FF A0 ? ? ? ? 48 8B 81 ? ? ? ? Add 4 Read8")]
        [OffsetTC("Search 44 8B 44 CF ? 48 8B 8F ? ? ? ? E8 ? ? ? ? 48 8B 8F ? ? ? ? 48 8B 97 ? ? ? ? 48 8B 01 48 8B 5C 24 ? 48 83 C4 30 5F 48 FF A0 98 00 00 00 48 8B 81 ? ? ? ? Add 4 Read8")]
        internal static int ItemListStart;

        //7.3
        [Offset("Search 0F B6 81 ? ? ? ? 48 8B D9 0F B6 51 ? Add 3 Read32")]
        [OffsetTC("Search 0F B6 81 ? ? ? ? 48 8B F9 0F B6 51 08 Add 3 Read32")]
        internal static int ItemCount;

        //7.3
        [Offset("Search 0F B6 51 ? 3A C2 0F 83 ? ? ? ? Add 3 Read8")]
        [OffsetTC("Search 48 8B F9 0F B6 51 ? Add 6 Read8")]
        internal static int ItemCount2;

    }

    public static class ResidentialHousingManagerOffsets
    {

        //6.3
        [Offset("Search 33 C9 E8 ? ? ? ? 48 8B C8 E8 ? ? ? ?  Add 2 TraceCall")]
        internal static IntPtr GetResidentObject;

        [Offset("Search 40 53 48 83 EC ? 48 8B D9 48 83 F9 ? 74 ?")]
        internal static IntPtr CheckValid;

    }

    public static class RetainerHireOffsets
    {

        [Offset("Search 0F B6 15 ? ? ? ? EB ? Add 3 TraceRelative")]
        internal static IntPtr MaxRetainers;

    }

    public static class RetainerHistoryOffsets
    {

        [Offset("Search 48 8B 41 ? 48 63 D2 44 39 04 90 Add 3 Read8")]
        internal static int NumberArrayData_IntArray;

        [Offset("Search 44 8B C5 BA ? ? ? ? 48 8B CE E8 ? ? ? ? 48 8B 4F 08 Add 4 Read32")]
        internal static int NumberArrayData_Count;

        [Offset("Search 41 BD ? ? ? ? 4C 89 7C 24 ? 41 BF ? ? ? ? 89 6C 24 68 Add 13 Read32")]
        internal static int NumberArrayData_Start;

        [Offset("Search BA ? ? ? ? 48 8B C8 4C 8B 00 41 FF 50 48 48 8B 4F 08 48 8B F0 Add 1 Read32")]
        internal static int NumberArrayIndex;

        //7.3
        [Offset("Search BA ? ? ? ? 48 8B 08 4C 8B 41 ? 48 8B C8 41 FF D0 4C 8B E0 48 85 F6 Add 1 Read32")]
        [OffsetTC("Search BA ? ? ? ? 48 8B C8 4C 8B 00 41 FF 50 50 4C 8B E0 48 85 F6 Add 1 Read32")]
        internal static int StringArrayIndex;

        [Offset("Search 48 8B 51 ? 0F 84 ? ? ? ? Add 3 Read8")]
        internal static int StringArrayData_StrArray;

        [Offset("Search 41 BD ? ? ? ? 4C 89 7C 24 ? 41 BF ? ? ? ? 89 6C 24 68 Add 2 Read32")]
        internal static int StringArrayData_Start;

        // GetSubModule
        [Offset("Search E8 ? ? ? ? 48 85 C0 74 ? 4C 8B 00 48 8B D3 48 8B C8 48 83 C4 ? 5B 49 FF 60 ? 48 83 C4 ? 5B C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 40 53 TraceCall")]
        internal static IntPtr GetSubModule;

        // vfunc 33 of UIModule
        [Offset("Search 41 FF 90 ? ? ? ? 48 8B C8 BA ? ? ? ? E8 ? ? ? ? 48 85 C0 74 ? 4C 8B 00 48 8B D3 48 8B C8 48 83 C4 ? 5B 49 FF 60 ? 48 83 C4 ? 5B C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 40 53 Add 3 Read32")]
        internal static int GetSomethingModuleVtblFunction;

        // Submodule number 9
        [Offset("Search BA ? ? ? ? E8 ? ? ? ? 48 85 C0 74 ? 4C 8B 00 48 8B D3 48 8B C8 48 83 C4 ? 5B 49 FF 60 ? 48 83 C4 ? 5B C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 40 53 Add 1 Read8")]
        internal static int SubModule;

        [Offset("Search 48 8B 8B ? ? ? ? 48 8D 54 24 ? 48 89 4C 24 ? 45 33 C9 48 8B C8 Add 3 Read32")]
        internal static int RetainerId;

        [Offset("Search 40 53 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 84 24 ? ? ? ? 48 83 B9 ? ? ? ? ?")]
        internal static IntPtr RequestSales;

    }

    public static class ScreenshotHelperOffsets
    {

        //7.1
        [Offset("Search E8 ? ? ? ? 84 C0 75 ? C6 05 ? ? ? ? ? E8 ? ? ? ? 48 89 05 ? ? ? ? Add 1 TraceRelative")]
        internal static IntPtr ScreenshotFunc;

        [Offset("Search 48 89 5C 24 ? 57 48 83 EC ? BB ? ? ? ? 83 FA ?")]
        internal static IntPtr CallbackFunction;

        [Offset("Search 48 8B 0D ? ? ? ? 48 8D 15 ? ? ? ? 45 33 C0 E8 ? ? ? ? 84 C0 Add 3 TraceRelative")]
        internal static IntPtr ScreenshotStruct;

        //7.1
        [Offset("Search C6 05 ? ? ? ? ? E8 ? ? ? ? 48 8B 5C 24 ? Add 2 TraceRelative")]
        internal static IntPtr ScreenshotState;

        [Offset("Search 48 8D 4B ? 48 8D 44 24 ? 48 3B C1 Add 3 Read8")]
        internal static int Filename;

        [Offset("Search C6 43 ? ? B0 ? 48 83 C4 ? 5B C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 40 53 48 81 EC ? ? ? ? Add 2 Read16")]
        internal static int Busy;

        //7.1
        //TODO: Update this offset
        /*[Offset("Search F3 0F 10 15 ? ? ? ? 0F 57 C0 0F 2F D0 Add 4 TraceRelative")]
        [OffsetCN("Search F3 0F 10 15 ? ? ? ? 0F 57 C0 0F 2F D0 Add 4 TraceRelative")]
        internal static IntPtr FloatThing;*/

    }

    public static class ShopProxyOffsets
    {

        //7.3
        [Offset("Search 8B 5B ? FF 50 ? F6 05 ? ? ? ? ? 48 89 44 24 ? C7 44 24 ? ? ? ? ? 48 C7 44 24 ? ? ? ? ? 89 5C 24 ? 0F 85 ? ? ? ? Add 2 Read8")]
        [OffsetTC("Search 8B 5B ? FF 50 08 F6 05 ? ? ? ? ? 48 89 44 24 ? C7 44 24 ? ? ? ? ? 48 C7 44 24 ? ? ? ? ? 89 5C 24 68 0F 85 ? ? ? ? 48 8B 0D ? ? ? ? E8 ? ? ? ? 48 8B C8 Add 2 Read8")]
        internal static int ShopIdPointer;

    }

    public static class SnipeManagerOffsets
    {

        [Offset("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? 84 C0 74 05 45 32 E4 EB 0F Add 3 TraceRelative")]
        internal static IntPtr Instance;

        [Offset("Search 8B 83 ? ? ? ? 89 45 10 48 8D 45 10 48 89 45 18 48 8D 42 01 48 3B C8 77 17 41 8B D6 48 8D 4C 24 ? E8 ? ? ? ? 48 8B 54 24 ? 48 8B 4C 24 ? Add 2 Read32")]
        internal static int Id; //0x5940

        //0F B6 83 ? ? ? ? 3C ? 0F 85 ? ? ? ? F3 0F 10 83 ? ? ? ?
        [Offset("Search 0F B6 83 ? ? ? ? 3C ? 0F 85 ? ? ? ? F3 0F 10 83 ? ? ? ? Add 3 Read32")]
        internal static int Active;

        //66 C7 83 ? ? ? ? ? ? E9 ? ? ? ? 48 63 83 ? ? ? ? Add 3 Read32
        [Offset("Search 66 C7 83 ? ? ? ? ? ? E9 ? ? ? ? 48 63 83 ? ? ? ? Add 3 Read32")]
        internal static int State;

        //48 8B 8B ? ? ? ? 48 8B 0C D1 Add 3 Read32
        [Offset("Search 48 8D 14 C9 48 8B 8B ? ? ? ? Add 7 Read32")]
        internal static int SnipeObjects;

        //0F B6 83 ? ? ? ? 3C ? 0F 85 ? ? ? ? F3 0F 10 83 ? ? ? ? Add 3 Read32
        [Offset("Search 0F B6 83 ? ? ? ? 3C ? 0F 85 ? ? ? ? F3 0F 10 83 ? ? ? ? Add 3 Read32")]
        internal static int Shoot;

        //0F B6 47 ? 88 83 ? ? ? ? 0F B6 47 ? 88 83 ? ? ? ? 44 88 B3 ? ? ? ? Add 3 Read8
        [Offset("Search 0F B6 47 ? 88 83 ? ? ? ? 0F B6 47 ? 88 83 ? ? ? ? 44 88 B3 ? ? ? ? Add 3 Read8")]
        internal static int ShootParam;

        // 44 89 A3 ? ? ? ? 66 C7 83 ? ? ? ? ? ? EB ? Add 3 Read32
        [Offset("Search 44 89 A3 ? ? ? ? 66 C7 83 ? ? ? ? ? ? EB ? Add 3 Read32")]
        internal static int ShootData;

        //0F B6 47 ? 88 83 ? ? ? ? 44 88 B3 ? ? ? ?
        [Offset("Search 0F B6 47 ? 88 83 ? ? ? ? 44 88 B3 ? ? ? ? Add 3 Read8")]
        internal static int ShootParam2;

    }

    public static class SquadronStatusOffsets
    {

        [Offset("Search 8B 3D ? ? ? ? 8B D8 3B F8 Add 2 TraceRelative")]
        internal static IntPtr SquadronStatus;

    }

    public static class TeleportHelperOffsets
    {

        //7.3
        [Offset("Search E8 ? ? ? ? 49 89 47 ? BA ? ? ? ? TraceCall")]
        [OffsetTC("Search E8 ? ? ? ? 49 89 44 24 ? 4C 8B F8 TraceCall")]
        internal static IntPtr UpdatePlayerAetheryteList;

        [Offset("Search E8 ? ? ? ? 48 8B 4B ? 84 C0 48 8B 01 74 ? Add 1 TraceRelative")]
        internal static IntPtr TeleportWithSettings;

        [Offset("Search 48 8D 0D ? ? ? ? 8B FA E8 ? ? ? ? 48 8B 4B ? Add 3 TraceRelative")]
        internal static IntPtr Telepo;

    }

    public static class TimersOffsets
    {

        [Offset("Search 48 83 EC ? 48 8B 0D ? ? ? ? E8 ? ? ? ? 48 85 C0 74 ? 48 8B C8 48 83 C4 ? E9 ? ? ? ? E8 ? ? ? ?")]
        internal static IntPtr GetCurrentTime;

        [Offset("Search 48 83 EC 28 48 8B 05 ? ? ? ? 44 8B C1 BA 1e 01 00 00 48 8B 88 ? ? ? ? E8 ? ? ? ? 48 85 C0 75 05 48 83 C4 28")]
        internal static IntPtr GetCycleExd;

    }

    public static class UIInputHelperOffsets
    {

        [Offset("Search 48 8B 48 ? 48 8B 01 48 8B 88 ? ? ? ? 48 89 4C 24 ? Add 3 Read8")]
        internal static int off1; //0x28

        //7.3
        [Offset("Search 48 8B 78 ? E8 ? ? ? ? 41 B0 ? Add 3 Read8")]
        [OffsetTC("Search 48 8B 43 ? 48 8D 95 ? ? ? ? 41 b0 ? Add 3 Read8")]
        internal static int off2; //0x18

        //7.3
        [Offset("Search 48 8B 70 ? 48 8B 06 48 8B 78 ? E8 ? ? ? ? 41 B0 ? Add 3 Read8")]
        [OffsetTC("Search 48 8B 48 ? 48 8B 01 FF 50 ? 48 8D 8D ? ? ? ? Add 3 Read8")]
        internal static int off3; //0x8

        //7.3
        [Offset("Search  48 89 5C 24 ? 48 89 74 24 ? 57 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 84 24 ? ? ? ? 41 0F B6 F0")]
        [OffsetTC("Search 48 89 5C 24 ? 55 56 57 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 84 24 ? ? ? ? 41 0F B6 F8")]
        internal static IntPtr SendStringToFocus;

        //7.3
        [Offset("Search E8 ? ? ? ? EB ? 33 ED 48 89 2F TraceCall")]
        [OffsetTC("Search E8 ? ? ? ? EB ? 33 DB 48 89 1F TraceCall")]
        internal static IntPtr Utf8StringCtor;

        [Offset("Search E8 ? ? ? ? 49 8D 8F ? ? ? ? 49 8D 97 ? ? ? ? TraceCall")]
        internal static IntPtr Utf8SetString;

        //7.3
        [Offset("Search E8 ? ? ? ? 4C 8B C7 48 8D 55 ? 49 8B CE E8 ? ? ? ? 48 8D 4D ? 48 89 45 ? TraceCall")]
        [OffsetTC("Search E8 ? ? ? ? 48 8B 43 ? 48 8D 54 24 ? 41 B0 ? 48 8B 48 ? 48 8B 01 FF 50 ? 48 8D 4C 24 ? E8 ? ? ? ? 48 8B 8C 24 ? ? ? ? 48 33 CC E8 ? ? ? ? 48 8B 9C 24 ? ? ? ? TraceCall")]
        internal static IntPtr Utf8StringFromSequenceCtor;

        //7.3
        [Offset("Search 48 8B 41 ? 48 8D 4D ? 75 ? Add 3 Read8")]
        [OffsetTC("Search 48 8B 4B ? 48 8D ? ? 66 89 75 ? 66 89 7D ? 4C 89 7D ? 48 89 45 A7 48 8B 01 FF 50 08 Add 3 Read8")]
        internal static int CurrentTextControl; //0x8

    }

    public static class UIStateOffsets
    {

        [Offset("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? 48 8B 8B ? ? ? ? 48 8B 01 Add 3 TraceRelative")]
        internal static IntPtr Instance;

        [Offset("Search 40 53 48 83 EC ? 48 8B D9 66 85 D2 74 ?")]
        internal static IntPtr CardUnlocked;

        [Offset("Search 48 89 5C 24 ? 57 48 83 EC ? 48 8B F9 0F B7 CA E8 ? ? ? ? 48 85 C0")]
        internal static IntPtr EmoteUnlocked;

        [Offset("Search 48 8D 0D ? ? ? ? 0F B6 04 08 84 D0 75 ? B8 ? ? ? ? Add 3 TraceRelative")]
        internal static IntPtr MinionArray;

        [Offset("Search E8 ? ? ? ? 48 85 C0 74 ? 66 83 B8 ? ? ? ? ? 0F 84 ? ? ? ? TraceCall")]
        internal static IntPtr ExdGetItem;

        [Offset("Search E8 ?? ?? ?? ?? 83 F8 01 75 03 TraceCall")]
        internal static IntPtr IsItemActionUnlocked;

        [Offset("Search 0F B7 8A ? ? ? ? E8 ? ? ? ? 48 8B F8 Add 3 Read32")]
        internal static int ItemActionOffset;

    }

    public static class WorldHelperOffsets
    {

        [Offset("Search 48 8d 4f ? 0f b7 10 e8 ? ? ? ? 48 ? ? 74 ? 48 8b ? Add 3 Read8")]
        internal static int Offset1;

        [Offset("Search 41 8B CC 41 89 8F ? ? ? ? Add 6 Read32")]
        internal static int DCOffset;

        /*
        #if !RB_CN
                    [Offset("Search 88 99 ? ? ? ? E8 ? ? ? ? 48 8B 08 Add 2 Read8")]
                    internal static int NewDcOffset;
        #endif
        */


        [Offset("Search 0F B7 98 ? ? ? ? 66 85 FF Add 3 Read32")]
        internal static int CurrentWorld;

        [Offset("Search 0F B7 81 ? ? ? ? 66 89 44 24 ? 48 8D 4C 24 ? Add 3 Read32")]
        internal static int HomeWorld;

        [Offset("Search 0F B7 4A 02 E8 ? ? ? ? 48 85 C0 Add 4 TraceCall")]
        internal static IntPtr GetPlaceName;

    }

    public static class mapsOffsets
    {

        /// <summary>
        ///     Pointer to where the game stores the map subkey.
        /// </summary>
        [Offset("Search 48 8d 0d ?? ?? ?? ?? e8 ?? ?? ?? ?? 48 8d 0d ?? ?? ?? ?? 0f b7 d8 e8 ?? ?? ?? ?? Add 3 TraceRelative")]
        internal static IntPtr CurrentMap;

    }
}
