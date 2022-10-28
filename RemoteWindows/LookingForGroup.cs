using System;
using ff14bot;
using ff14bot.RemoteWindows;
using LlamaLibrary.ClientDataHelpers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteWindows
{
    public class LookingForGroup : RemoteWindow<LookingForGroup>
    {
        //BA ? ? ? ? 48 8B CF E8 ? ? ? ? 41 83 C8 ? BA ? ? ? ? 48 8B CF E8 ? ? ? ? 41 83 C8 ? BA ? ? ? ? 48 8B CF E8 ? ? ? ? 41 83 C8 ?

        private static class Offsets
        {
            [Offset("Search 83 B8 ? ? ? ? ? 7D ? 48 8D 9E ? ? ? ? Add 2 Read32")]
            internal static int ResultCountIndex;

            [Offset("Search BA ? ? ? ? 48 8B C8 4C 8B 00 41 FF 50 ? 48 8B F8 48 85 C0 0F 84 ? ? ? ? 45 33 C0 Add 1 Read8")]
            internal static int NumberArrayIndex;

            [Offset("Search 48 8B 41 ? 48 63 D2 44 39 04 90 Add 3 Read8")]
            internal static int NumberArrayData_IntArray;
        }

        public IntPtr ResultCountLocation
        {
            get
            {
                var arrayLocation = Core.Memory.Read<IntPtr>(AtkArrayDataHolder.GetNumberArray(Offsets.NumberArrayIndex) + Offsets.NumberArrayData_IntArray);

                return arrayLocation + Offsets.ResultCountIndex;
            }
        }

        public int ResultCount => Core.Memory.Read<int>(ResultCountLocation);

        public RemoteButton DataCenterButton => WindowByName.FindButton(3);

        public bool DataCenterEnabled => DataCenterButton.IsValid && DataCenterButton.Clickable;

        public RemoteButton RecruitMembersButton => WindowByName.FindButton(46);

        public bool RecruitMembersEnabled => RecruitMembersButton.IsValid && RecruitMembersButton.Clickable;

        public LookingForGroup() : base("LookingForGroup")
        {
        }

        public void RecruitMembers()
        {
            SendAction(1, 3, 0xE);
        }

        public void SelectDataCenterTab()
        {
            SendAction(2, 3, 0x14, 4, 0);
        }
    }
}