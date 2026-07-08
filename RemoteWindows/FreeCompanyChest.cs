using System.Collections.Generic;
using ff14bot;
using LlamaLibrary.Enums;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    /// Interaction window for the Free Company chest.
    /// Provides access to item tabs, crystals, gil, and their respective permissions.
    /// </summary>
    public class FreeCompanyChest : RemoteWindow<FreeCompanyChest>
    {
        /// <summary>
        /// Gets a dictionary mapping item tab indices to their current permission levels.
        /// </summary>
        /// <remarks>
        /// Permissions for up to 16 tabs are stored as 2-bit values in a single 32-bit field (uint).
        /// Each pair of bits corresponds to a <see cref="CompanyChestPermission"/> value.
        /// </remarks>
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

        /// <summary>
        /// Gets the current permission level for accessing crystals in the FC chest.
        /// </summary>
        public CompanyChestPermission CrystalsPermission => WindowByName != null ? Core.Memory.Read<CompanyChestPermission>(WindowByName.Pointer + FreeCompanyChestOffsets.CrystalsPermission) : CompanyChestPermission.NoAccess;

        /// <summary>
        /// Gets the current permission level for accessing gil in the FC chest.
        /// </summary>
        public CompanyChestPermission GilPermission => WindowByName != null ? Core.Memory.Read<CompanyChestPermission>(WindowByName.Pointer + FreeCompanyChestOffsets.GilPermission) : CompanyChestPermission.NoAccess;

        /// <summary>
        /// Gets the number of available item tabs in the FC chest.
        /// </summary>
        public int ItemTabCount => Elements[4].TrimmedData;

        /// <summary>
        /// Initializes a new instance of the <see cref="FreeCompanyChest"/> class.
        /// </summary>
        public FreeCompanyChest() : base("FreeCompanyChest")
        {
        }

        /// <summary>
        /// Navigates to a specific item tab in the chest interface.
        /// </summary>
        /// <param name="tabIndex">The zero-based index of the tab to select.</param>
        public void SelectItemTab(int tabIndex)
        {
            SendAction(2, 3, 0, 4, (ulong)tabIndex);
        }

        /// <summary>
        /// Switches the chest interface to the crystals tab.
        /// </summary>
        public void SelectCrystalTab()
        {
            SendAction(1, 3, 1);
        }

        /// <summary>
        /// Switches the chest interface to the gil tab.
        /// </summary>
        public void SelectGilTab()
        {
            SendAction(1, 3, 2);
        }
    }
}