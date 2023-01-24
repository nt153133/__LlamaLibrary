using System.Collections.Generic;
using ff14bot;
using LlamaLibrary.Enums;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteWindows
{
    public class FreeCompanyChest : RemoteWindow<FreeCompanyChest>
    {
        private static class Offsets
        {
            [Offset("Search 8B 9E ? ? ? ? 8B CF D3 EB Add 2 Read32")]
            internal static int ItemPermissions;

            [Offset("Search 44 89 81 ? ? ? ? 4C 8D B1 ? ? ? ? Add 3 Read32")]
            internal static int CrystalsPermission;

            [Offset("Search 44 89 89 ? ? ? ? 33 FF Add 3 Read32")]
            internal static int GilPermission;
        }

        public Dictionary<int, CompanyChestPermission> ItemTabPermissions
        {
            get
            {
                if (WindowByName == null)
                {
                    return new Dictionary<int, CompanyChestPermission>();
                }

                var permissions = new Dictionary<int, CompanyChestPermission>();
                var value = Core.Memory.Read<uint>(WindowByName.Pointer + Offsets.ItemPermissions);
                for (var i = 0; i < ItemTabCount; i++)
                {
                    permissions.Add(i, (CompanyChestPermission)((value >> (2 * i)) & 3));
                }

                return permissions;
            }
        }

        public CompanyChestPermission CrystalsPermission => WindowByName != null ? Core.Memory.Read<CompanyChestPermission>(WindowByName.Pointer + Offsets.CrystalsPermission) : CompanyChestPermission.NoAccess;

        public CompanyChestPermission GilPermission => WindowByName != null ? Core.Memory.Read<CompanyChestPermission>(WindowByName.Pointer + Offsets.GilPermission) : CompanyChestPermission.NoAccess;

        public int ItemTabCount => Elements[4].TrimmedData;

        public FreeCompanyChest() : base("FreeCompanyChest")
        {
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