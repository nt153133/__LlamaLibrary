using System;
using ff14bot;
using ff14bot.RemoteWindows;
using LlamaLibrary.ClientDataHelpers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteWindows
{
    public class LookingForGroup : RemoteWindow<LookingForGroup>
    {
        //BA ? ? ? ? 48 8B CF E8 ? ? ? ? 41 83 C8 ? BA ? ? ? ? 48 8B CF E8 ? ? ? ? 41 83 C8 ? BA ? ? ? ? 48 8B CF E8 ? ? ? ? 41 83 C8 ?

        

        public IntPtr ResultCountLocation
        {
            get
            {
                var arrayLocation = Core.Memory.Read<IntPtr>(AtkArrayDataHolder.GetNumberArray(LookingForGroupOffsets.NumberArrayIndex) + LookingForGroupOffsets.NumberArrayData_IntArray);

                return arrayLocation + LookingForGroupOffsets.ResultCountIndex;
            }
        }

        public int ResultCount => Core.Memory.Read<int>(ResultCountLocation);

        public RemoteButton? DataCenterButton => WindowByName?.FindButton(3);

        public bool DataCenterEnabled => DataCenterButton is { IsValid: true, Clickable: true };

        public RemoteButton? RecruitMembersButton => WindowByName?.FindButton(46);

        public bool RecruitMembersEnabled => RecruitMembersButton is { IsValid: true, Clickable: true };

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