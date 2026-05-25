using System.Windows.Media;
using ff14bot.Managers;
using ff14bot.RemoteWindows;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Retainers
{
    /// <summary>
    /// Provides methods for interacting with the retainer task selection window (SelectString).
    /// </summary>
    //TODO this is a sad copy of a window since it's only a select string window. I think i have better code for it somewhere.
    public class RetainerTasks
    {
        private static readonly LLogger Log = new(nameof(RetainerRoutine), Colors.White);

        /// <summary>
        /// Gets a value indicating whether the retainer task selection window is currently open.
        /// </summary>
        public static bool IsOpen => SelectString.IsOpen;

        /// <summary>
        /// Attempts to open the retainer's inventory by clicking the first slot in the task menu.
        /// </summary>
        /// <returns><see langword="true"/> if the command was sent; otherwise <see langword="false"/>.</returns>
        public static bool OpenInventory()
        {
            if (IsOpen)
            {
                SelectString.ClickSlot(0);
                return true;
            }

            Log.Information("Retainer task window not open");
            return false;
        }

        /// <summary>
        /// Attempts to close the retainer's inventory window by sending a close action to either the standard or large inventory window.
        /// </summary>
        /// <returns><see langword="true"/> if the window was closed or already closed; otherwise <see langword="false"/>.</returns>
        public static bool CloseInventory()
        {
            if (!IsInventoryOpen())
            {
                return true;
            }

            if (RaptureAtkUnitManager.GetWindowByName("InventoryRetainer") != null)
            {
                RaptureAtkUnitManager.GetWindowByName("InventoryRetainer").SendAction(1, 3, uint.MaxValue);
                return true;
            }

            if (RaptureAtkUnitManager.GetWindowByName("InventoryRetainerLarge") != null)
            {
                RaptureAtkUnitManager.GetWindowByName("InventoryRetainerLarge").SendAction(1, 3, uint.MaxValue);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Closes the retainer task menu by clicking the last available slot (typically "Quit").
        /// </summary>
        /// <returns><see langword="true"/> if the task window is no longer open; otherwise <see langword="false"/>.</returns>
        public static bool CloseTasks()
        {
            if (IsOpen)
            {
                SelectString.ClickSlot((uint)(SelectString.LineCount - 1));
            }

            return !IsOpen;
        }

        /// <summary>
        /// Checks if either the standard or large retainer inventory window is currently open in the UI.
        /// </summary>
        /// <returns><see langword="true"/> if a retainer inventory window is found; otherwise <see langword="false"/>.</returns>
        public static bool IsInventoryOpen()
        {
            return RaptureAtkUnitManager.GetWindowByName("InventoryRetainer") != null ||
                   RaptureAtkUnitManager.GetWindowByName("InventoryRetainerLarge") != null;
        }

        /// <summary>
        /// Internal utility class containing localized strings for retainer task menu options.
        /// </summary>
        internal static class RetainerTaskStrings
        {
            //For partial string searches use SelectIconString.ClickLineContains(string) and not Equals
#if RB_CN
			internal static string Inventory = "道具管理";
            internal static string Gil = "金币管理";
            internal static string SellYourInventory = "出售（玩家所持物品）";
            internal static string SellRetainerInventory = "出售（雇员所持物品）";
            internal static string SaleHistory = "查看出售记录";
            internal static string ViewVentureReport = "查看雇员探险情况"; //Use Partial Search
            internal static string AssignVenture = "委托雇员进行探险"; //Use Partial Search since it adds (Complete) or (In Progress)
            internal static string ViewGear = "设置雇员装备";
            internal static string ResetClass = "设置雇员职业"; //Use Partial Search
            internal static string Quit = "让雇员返回";
#else
            internal static string Inventory = "Entrust or withdraw items.";
            internal static string Gil = "Entrust or withdraw gil.";
            internal static string SellYourInventory = "Sell items in your inventory on the market";
            internal static string SellRetainerInventory = "Sell items in your retainer's inventory on the market.";
            internal static string SaleHistory = "View sale history.";
            internal static string ViewVentureReport = "View venture report."; //Use Partial Search
            internal static string AssignVenture = "Assign venture."; //Use Partial Search since it adds (Complete) or (In Progress)
            internal static string ViewGear = "View retainer attributes and gear.";
            internal static string ResetClass = "Reset retainer class."; //Use Partial Search
            internal static string Quit = "Quit.";
#endif
        }
    }
}