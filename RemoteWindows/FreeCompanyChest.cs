using System.Collections.Generic;
using ff14bot;
using LlamaLibrary.Enums;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteWindows
{
    public class FreeCompanyChest : RemoteWindow<FreeCompanyChest>
    {
        

        public Dictionary<int, CompanyChestPermission> ItemTabPermissions
        {
            get
            {
                if (WindowByName == null)
                {
                    return new Dictionary<int, CompanyChestPermission>();
                }

                var permissions = new Dictionary<int, CompanyChestPermission>();
                var value = Core.Memory.Read<uint>(WindowByName.Pointer + FreeCompanyChestOffsets.ItemPermissions);
                for (var i = 0; i < ItemTabCount; i++)
                {
                    permissions.Add(i, (CompanyChestPermission)((value >> (2 * i)) & 3));
                }

                return permissions;
            }
        }

        public CompanyChestPermission CrystalsPermission => WindowByName != null ? Core.Memory.Read<CompanyChestPermission>(WindowByName.Pointer + FreeCompanyChestOffsets.CrystalsPermission) : CompanyChestPermission.NoAccess;

        public CompanyChestPermission GilPermission => WindowByName != null ? Core.Memory.Read<CompanyChestPermission>(WindowByName.Pointer + FreeCompanyChestOffsets.GilPermission) : CompanyChestPermission.NoAccess;

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