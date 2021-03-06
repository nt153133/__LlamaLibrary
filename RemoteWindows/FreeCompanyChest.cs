using System;
using System.Collections.Generic;
using ff14bot;
using LlamaLibrary.Enums;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteWindows
{
    public class FreeCompanyChest : RemoteWindow<FreeCompanyChest>
    {
        private const string WindowName = "FreeCompanyChest";

        private static class Offsets
        {
            [Offset("8B 9E ? ? ? ? 8B CF D3 EB Add 2 Read32")]
            internal static int ItemPermissions;

            [Offset("44 89 81 ? ? ? ? 4C 8D B1 ? ? ? ? Add 3 Read32")]
            internal static int CrystalsPermission;

            [Offset("44 89 89 ? ? ? ? 33 FF Add 3 Read32")]
            internal static int GilPermission;
        }

        public Dictionary<int, CompanyChestPermission> ItemTabPermissions
        {
            get
            {
                Dictionary<int, CompanyChestPermission> permissions = new Dictionary<int, CompanyChestPermission>();
                var value = Core.Memory.Read<uint>(WindowByName.Pointer + Offsets.ItemPermissions);
                for (int i = 0; i < ItemTabCount; i++)
                {
                    permissions.Add(i, (CompanyChestPermission)((value >> (2 * i)) & 3));
                }

                return permissions;
            }
        }

        public CompanyChestPermission CrystalsPermission => Core.Memory.Read<CompanyChestPermission>(WindowByName.Pointer + Offsets.CrystalsPermission);

        public CompanyChestPermission GilPermission => Core.Memory.Read<CompanyChestPermission>(WindowByName.Pointer + Offsets.GilPermission);

        public int ItemTabCount => Elements[4].TrimmedData;

        public FreeCompanyChest() : base(WindowName)
        {
            _name = WindowName;
        }

        public void SelectItemTab(int tabIndex)
        {
            SendAction(2, 3, 0, 4, (ulong)tabIndex);
        }

        public void SelectCrystalTab()
        {
            SendAction(1, 3, 1);
        }

        public void SelectGilTab()
        {
            SendAction(1, 3, 2);
        }
    }
}