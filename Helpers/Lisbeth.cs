using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Settings;
using LlamaLibrary.Logging;

#pragma warning disable CS8603
#pragma warning disable CS8602

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Reflection-based bridge to the Lisbeth crafting/gathering plugin.
    /// Provides access to Lisbeth's order execution, zone travel, gear management,
    /// hook registration, and crafting lifecycle API methods without a hard compile-time dependency.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1214:Readonly fields should appear before non-readonly fields")]
    public static class Lisbeth
    {
        private static readonly LLogger Log = new("LisbethHelper", Colors.MediumPurple);

        private static object? _lisbeth;
        private static MethodInfo? _orderMethod;
        private static readonly MethodInfo _travelMethod;
        public static Func<string>? _getCurrentAreaName;
        private static readonly Func<Task>? _stopGentlyAndWait;
        private static Func<Task>? _equipOptimalGear;
        private static Func<Task>? _extractMateria;
        private static Func<Task>? _selfRepair;
        private static Func<Task>? _selfRepairWithMenderFallback;
        private static Func<Task>? _stopGently;
        private static Action<string, Func<Task>>? _addHook;
        private static Action<string, Func<Task>>? _addCraftHook;
        private static Action<string, Func<Task>>? _addCompletionHook;
        private static Action<string, Func<Task>>? _addGrindHook;
        private static Action<string, Func<Task>>? _addGatherHook;
        private static Action<string>? _removeHook;
        private static Action<string>? _removeCraftHook;
        private static Action<string>? _removeCompletionHook;
        private static Action<string>? _removeGrindHook;
        private static Action<string>? _removeGatherHook;
        private static Func<List<string>>? _getHookList;
        private static Func<Task<bool>>? _exitCrafting;
        private static Func<HashSet<uint>>? _getAllOrderItems;
        private static Action<HashSet<uint>>? _setTrashExclusionItems;
        private static Func<Task> _makeEquipment;

        private static Func<Task<bool>>? _isProductKeyValid;

        //private static Func<string, Vector3, Func<bool>, bool, Task<bool>>? _travelToWithArea;
        private static Func<uint, uint, Vector3, Func<bool>, bool, Task<bool>>? _travelTo;

        private static Func<uint, Vector3, Func<bool>, bool, Task<bool>>? _travelToWithoutSubzone;
        private static Action _openWindow;
        private static Func<string, Task<string>> _getOrderExpansionAsJson;
        private static Func<Character, Task> _kill;

        static Lisbeth()
        {
            FindLisbeth();
        }

        internal static void FindLisbeth()
        {
            var loader = BotManager.Bots
                .FirstOrDefault(c => string.Equals(c.Name, "Lisbeth", StringComparison.Ordinal));

            if (loader == null)
            {
                return;
            }

            var lisbethObjectProperty = loader.GetType().GetProperty("Lisbeth");
            var lisbeth = lisbethObjectProperty?.GetValue(loader);
            var orderMethod = lisbeth?.GetType().GetMethod("ExecuteOrders");
            if (lisbeth != null)
            {
                var apiObject = lisbeth.GetType().GetProperty("Api")?.GetValue(lisbeth);
                if (orderMethod == null)
                {
                    return;
                }

                if (apiObject != null)
                {
                    var m = apiObject.GetType().GetMethod("GetCurrentAreaName");
                    if (m != null)
                    {
                        try
                        {
                            _getCurrentAreaName = (Func<string>)Delegate.CreateDelegate(typeof(Func<string>), apiObject, "GetCurrentAreaName");
                            _stopGently = (Func<Task>)Delegate.CreateDelegate(typeof(Func<Task>), apiObject, "StopGently");
                            _kill = (Func<Character, Task>)Delegate.CreateDelegate(typeof(Func<Character, Task>), apiObject, "Kill");

                            //_stopGentlyAndWait = (Func<Task>) Delegate.CreateDelegate(typeof(Func<Task>), apiObject, "StopGentlyAndWait");
                            _addHook = (Action<string, Func<Task>>)Delegate.CreateDelegate(typeof(Action<string, Func<Task>>), apiObject, "AddHook");
                            _removeHook = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), apiObject, "RemoveHook");
                            _addCraftHook = (Action<string, Func<Task>>)Delegate.CreateDelegate(typeof(Action<string, Func<Task>>), apiObject, "AddCraftCycleHook");
                            _addCompletionHook = (Action<string, Func<Task>>)Delegate.CreateDelegate(typeof(Action<string, Func<Task>>), apiObject, "AddCompletionHook");
                            _removeCraftHook = (Action<string>) Delegate.CreateDelegate(typeof(Action<string>), apiObject, "RemoveCraftCycleHook");
                            _removeCompletionHook = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), apiObject, "RemoveCompletioneHook");
                            _getHookList = (Func<List<string>>)Delegate.CreateDelegate(typeof(Func<List<string>>), apiObject, "GetHookList");
                            _exitCrafting = (Func<Task<bool>>)Delegate.CreateDelegate(typeof(Func<Task<bool>>), apiObject, "ExitCrafting");
                            _equipOptimalGear = (Func<Task>)Delegate.CreateDelegate(typeof(Func<Task>), apiObject, "EquipOptimalGear");
                            _extractMateria = (Func<Task>)Delegate.CreateDelegate(typeof(Func<Task>), apiObject, "ExtractMateria");
                            _selfRepair = (Func<Task>)Delegate.CreateDelegate(typeof(Func<Task>), apiObject, "SelfRepair");
                            _selfRepairWithMenderFallback = (Func<Task>)Delegate.CreateDelegate(typeof(Func<Task>), apiObject, "SelfRepairWithMenderFallback");
                            _travelToWithoutSubzone = (Func<uint, Vector3, Func<bool>, bool, Task<bool>>)Delegate.CreateDelegate(typeof(Func<uint, Vector3, Func<bool>, bool, Task<bool>>), apiObject, "TravelToWithoutSubzone");
                            _travelTo = (Func<uint, uint, Vector3, Func<bool>, bool, Task<bool>>)Delegate.CreateDelegate(typeof(Func<uint, uint, Vector3, Func<bool>, bool, Task<bool>>), apiObject, "TravelTo");

                            //_travelToWithArea = (Func<string, Vector3, Func<bool>, bool, Task<bool>>)Delegate.CreateDelegate(typeof(Func<string, Vector3, Func<bool>, bool, Task<bool>>), apiObject, "TravelToWithArea");
                            _openWindow = (Action)Delegate.CreateDelegate(typeof(Action), apiObject, "OpenWindow");
                            _getOrderExpansionAsJson = (Func<string, Task<string>>)Delegate.CreateDelegate(typeof(Func<string, Task<string>>), apiObject, "GetOrderExpansionAsJson");
                            _isProductKeyValid = (Func<Task<bool>>)Delegate.CreateDelegate(typeof(Func<Task<bool>>), apiObject, "IsProductKeyValid");

                            // 6.51f3
                            _getAllOrderItems = (Func<HashSet<uint>>)Delegate.CreateDelegate(typeof(Func<HashSet<uint>>), apiObject, "GetAllOrderItems");
                            _setTrashExclusionItems = (Action<HashSet<uint>>)Delegate.CreateDelegate(typeof(Action<HashSet<uint>>), apiObject, "SetTrashExclusions");

                            // 7.01
                            _addGrindHook = (Action<string, Func<Task>>)Delegate.CreateDelegate(typeof(Action<string, Func<Task>>), apiObject, "AddGrindCycleHook");
                            _addGatherHook = (Action<string, Func<Task>>)Delegate.CreateDelegate(typeof(Action<string, Func<Task>>), apiObject, "AddGatherCycleHook");
                            _removeGatherHook = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), apiObject, "RemoveGatherCycleHook");
                            _removeGrindHook = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), apiObject, "RemoveGrindCycleHook");

                            // 7.3
                            _makeEquipment = (Func<Task>)Delegate.CreateDelegate(typeof(Func<Task>), apiObject, "OptimizeEquipment");
                        }
                        catch (Exception e)
                        {
                            Log.Error(e.ToString());
                        }
                    }
                }
            }

            _orderMethod = orderMethod;
            _lisbeth = lisbeth;

            //_travelMethod = travelMethod;

            Log.Information("Lisbeth found.");
        }

        /// <summary>
        /// Returns <see langword="true"/> if the Lisbeth plugin is loaded and its product key is valid.
        /// </summary>
        public static async Task<bool> HasLisbeth()
        {
            try
            {
                if (BotManager.Bots.FirstOrDefault(c => c.EnglishName == "Lisbeth") == null)
                {
                    return false;
                }

                //return await Lisbeth.GetOrderExpansionAsJson("[{\"Group\":1,\"Item\":1,\"Amount\":1,\"Enabled\":true,\"Type\":\"None\"}]") != null;
                return await IsProductKeyValid();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns <see langword="true"/> if the Lisbeth plugin is loaded and its product key is valid,
        /// without wrapping the check in a RebornBuddy coroutine.
        /// </summary>
        public static async Task<bool> HasLisbethNoCoroutine()
        {
            try
            {
                if (BotManager.Bots.FirstOrDefault(c => c.EnglishName == "Lisbeth") == null)
                {
                    return false;
                }

                return await IsProductKeyValidNoCoroutine();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Gets the name of the area Lisbeth is currently operating in.</summary>
        public static string GetCurrentAreaName => _getCurrentAreaName.Invoke();

        /// <summary>
        /// Commands Lisbeth to kill a specific mob, moving into pull range first if needed.
        /// </summary>
        /// <param name="mob">The character object to kill.</param>
        public static async Task Kill(Character mob)
        {
            if (Core.Me.Distance(mob) + 1 >= RoutineManager.Current.PullRange)
            {
                Log.Information("Outside of pull range so getting closer");
                await Navigation.FlightorMove(mob.Location, () => Core.Me.Distance(mob) < (RoutineManager.Current.PullRange / 2));
                if (Core.Me.IsMounted)
                {
                    await CommonTasks.StopAndDismount();
                }
            }

            var mount = CharacterSettings.Instance.UseMount;
            CharacterSettings.Instance.UseMount = false;
            await _kill(mob);
            CharacterSettings.Instance.UseMount = mount;
        }

        /// <summary>
        /// Passes a JSON order string to Lisbeth and waits for it to complete.
        /// </summary>
        /// <param name="json">Lisbeth order JSON (array of order objects).</param>
        /// <returns><see langword="true"/> if orders completed successfully.</returns>
        public static async Task<bool> ExecuteOrders(string json)
        {
            if (_orderMethod != null)
            {
                return await (Task<bool>)_orderMethod.Invoke(_lisbeth, new object[] { json, false })!;
            }

            FindLisbeth();
            if (_orderMethod == null)
            {
                return false;
            }

            return await (Task<bool>)_orderMethod.Invoke(_lisbeth, new object[] { json, false })!;
        }

        /// <summary>
        /// Like <see cref="ExecuteOrders"/>, but instructs Lisbeth to ignore the configured home world setting.
        /// </summary>
        /// <param name="json">Lisbeth order JSON.</param>
        /// <returns><see langword="true"/> if orders completed successfully.</returns>
        public static async Task<bool> ExecuteOrdersIgnoreHome(string json)
        {
            if (_orderMethod != null)
            {
                return await (Task<bool>)_orderMethod.Invoke(_lisbeth, new object[] { json, true })!;
            }

            FindLisbeth();
            if (_orderMethod == null)
            {
                return false;
            }

            return await (Task<bool>)_orderMethod.Invoke(_lisbeth, new object[] { json, true })!;
        }

        [Obsolete("Use TravelToZones instead")]
        public static Task<bool> TravelTo(string area, Vector3 position, Func<bool>? condition = null, bool land = true)
        {
            return Task.FromResult(false);
            /*
            if (condition == null)
            {
                condition = AlwaysTrue;
            }

            if (_travelToWithArea != null)
            {
                return await _travelToWithArea(area, position, condition, land);
            }

            FindLisbeth();
            if (_travelToWithArea == null)
            {
                return false;
            }

            return await _travelToWithArea(area, position, condition, land);
            */
        }

        /// <summary>
        /// Travels to <paramref name="position"/> in <paramref name="zoneId"/> using Lisbeth's travel API.
        /// Prefers the subzone-less overload; pass <paramref name="subzoneId"/> &gt; 0 to use a specific subzone.
        /// </summary>
        /// <param name="zoneId">Territory/zone ID to travel to.</param>
        /// <param name="subzoneId">Subzone ID (pass 0 to use zone-only travel).</param>
        /// <param name="position">Target world position.</param>
        /// <param name="condition">Optional stop condition; Lisbeth stops traveling when it returns <see langword="true"/>.</param>
        /// <param name="land">If <see langword="true"/>, dismounts/lands on arrival.</param>
        /// <returns><see langword="true"/> if travel completed successfully.</returns>
        [Obsolete("Stop using subzones")]
        public static async Task<bool> TravelToZones(uint zoneId, uint subzoneId, Vector3 position, Func<bool>? condition = null, bool land = true)
        {
            condition ??= AlwaysTrue;
            return subzoneId > 0
                ? await _travelTo(zoneId, subzoneId, position, condition, land)
                : await _travelToWithoutSubzone(zoneId, position, condition, land);
        }

        /// <summary>Travels to <paramref name="position"/> in the specified zone using Lisbeth's subzone-less travel API.</summary>
        /// <param name="zoneId">Territory/zone ID to travel to.</param>
        /// <param name="position">Target world position.</param>
        /// <param name="condition">Optional stop condition.</param>
        /// <param name="land">If <see langword="true"/>, dismounts/lands on arrival.</param>
        /// <returns><see langword="true"/> if travel completed successfully.</returns>
        public static async Task<bool> TravelToZones(uint zoneId, Vector3 position, Func<bool>? condition = null, bool land = true)
        {
            condition ??= AlwaysTrue;
            Log.Information($"Lisbeth Travel: Zone:{zoneId}  Pos:{position}");
            return await _travelToWithoutSubzone(zoneId, position, condition, land);
        }

        /// <summary>Gracefully stops Lisbeth's current task without aborting mid-craft.</summary>
        public static async Task StopGently()
        {
            await _stopGently();
        }

        /// <summary>Opens the Lisbeth settings/order window.</summary>
        public static void OpenSettings()
        {
            _openWindow();
        }

        private static bool AlwaysTrue()
        {
            return true;
        }

        /// <summary>Registers a named hook that Lisbeth calls at the start of each task cycle.</summary>
        /// <param name="name">Unique hook identifier.</param>
        /// <param name="function">The async function to invoke.</param>
        public static void AddHook(string name, Func<Task> function)
        {
            _addHook?.Invoke(name, function);
        }

        /// <summary>Removes a previously registered task-cycle hook by name.</summary>
        /// <param name="name">The hook identifier to remove.</param>
        public static void RemoveHook(string name)
        {
            _removeHook?.Invoke(name);
        }

        /// <summary>Registers a hook called when a Lisbeth order list completes.</summary>
        /// <param name="name">Unique hook identifier.</param>
        /// <param name="function">The async function to invoke on completion.</param>
        public static void AddCompletionHook(string name, Func<Task> function)
        {
            _addCompletionHook?.Invoke(name, function);
        }

        /// <summary>Removes a previously registered completion hook by name.</summary>
        /// <param name="name">The hook identifier to remove.</param>
        public static void RemoveCompletionHook(string name)
        {
            _removeCompletionHook?.Invoke(name);
        }

        /// <summary>Registers a hook called before each individual craft cycle.</summary>
        /// <param name="name">Unique hook identifier.</param>
        /// <param name="function">The async function to invoke.</param>
        public static void AddCraftHook(string name, Func<Task> function)
        {
            _addCraftHook?.Invoke(name, function);
        }

        [Obsolete("Use AddCraftHook instead")]
        public static void AddCraftCycleHook(string name, Func<Task> function)
        {
            _addCraftHook?.Invoke(name, function);
        }

        /// <summary>Removes a previously registered craft-cycle hook by name.</summary>
        /// <param name="name">The hook identifier to remove.</param>
        public static void RemoveCraftHook(string name)
        {
            _removeCraftHook?.Invoke(name);
        }

        [Obsolete("Use RemoveCraftHook instead")]
        public static void RemoveCraftCycleHook(string name)
        {
            _removeCraftHook?.Invoke(name);
        }

        /// <summary>Returns a list of all currently registered hook names.</summary>
        /// <returns>List of hook name strings, or empty if Lisbeth is not loaded.</returns>
        public static List<string> GetHookList()
        {
            return _getHookList?.Invoke() ?? new List<string>();
        }

        /// <summary>Exits the current crafting session cleanly (e.g., closes the synthesis window).</summary>
        /// <returns><see langword="true"/> if crafting was successfully exited.</returns>
        public static Task<bool> ExitCrafting()
        {
            return _exitCrafting?.Invoke() ?? Task.FromResult(false);
        }

        /// <summary>Equips the optimal gear set for the current job using Lisbeth's gear optimizer.</summary>
        public static async Task EquipOptimalGear()
        {
            await _equipOptimalGear?.Invoke();
        }

        /// <summary>Extracts all materia from overmelded gear using Lisbeth's materia extraction logic.</summary>
        public static async Task ExtractMateria()
        {
            await _extractMateria?.Invoke();
        }

        /// <summary>Self-repairs all gear using repair materials in the player's inventory.</summary>
        public static async Task SelfRepair()
        {
            await _selfRepair?.Invoke();
        }

        /// <summary>Self-repairs gear if possible; falls back to visiting a mender NPC if repair materials are insufficient.</summary>
        public static async Task SelfRepairWithMenderFallback()
        {
            await _selfRepairWithMenderFallback?.Invoke();
        }

        /// <summary>
        /// Expands a Lisbeth order JSON string by resolving ingredient requirements and returns the expanded JSON.
        /// </summary>
        /// <param name="orderJson">The original compact order JSON string.</param>
        /// <returns>An expanded order JSON string with ingredient sub-orders resolved.</returns>
        public static async Task<string> GetOrderExpansionAsJson(string orderJson)
        {
            return await _getOrderExpansionAsJson(orderJson);
        }

        public static async Task<bool> IsProductKeyValid()
        {
            return await Coroutine.ExternalTask(_isProductKeyValid());
        }

        public static async Task<bool> IsProductKeyValidNoCoroutine()
        {
            return await _isProductKeyValid();
        }

        /// <summary>Returns all item IDs currently referenced by active Lisbeth orders.</summary>
        /// <returns>A set of item row IDs, or an empty set if Lisbeth is not loaded.</returns>
        public static HashSet<uint> GetAllOrderItems()
        {
            return _getAllOrderItems?.Invoke();
        }

        /// <summary>
        /// Sets the list of item IDs that Lisbeth should never discard as trash, even if they are not in an active order.
        /// </summary>
        /// <param name="items">Set of item row IDs to protect from discard. <see langword="null"/> is a no-op.</param>
        public static void SetTrashExclusionItems(HashSet<uint>? items)
        {
            if (items == null) { return; }

            _setTrashExclusionItems?.Invoke(items);
        }

        /// <summary>Registers a hook called before each grind cycle (kill/gather loop iteration).</summary>
        /// <param name="name">Unique hook identifier.</param>
        /// <param name="function">The async function to invoke.</param>
        public static void AddGrindHook(string name, Func<Task> function)
        {
            _addGrindHook?.Invoke(name, function);
        }

        /// <summary>Registers a hook called before each gather cycle.</summary>
        /// <param name="name">Unique hook identifier.</param>
        /// <param name="function">The async function to invoke.</param>
        public static void AddGatherHook(string name, Func<Task> function)
        {
            _addGatherHook?.Invoke(name, function);
        }

        /// <summary>Removes a previously registered grind-cycle hook by name.</summary>
        /// <param name="name">The hook identifier to remove.</param>
        public static void RemoveGrindHook(string name)
        {
            _removeGrindHook?.Invoke(name);
        }

        /// <summary>Removes a previously registered gather-cycle hook by name.</summary>
        /// <param name="name">The hook identifier to remove.</param>
        public static void RemoveGatherHook(string name)
        {
            _removeGatherHook?.Invoke(name);
        }

        /// <summary>Optimizes the player's equipped gear via Lisbeth's equipment optimizer (Dawntrail 7.3+ API).</summary>
        public static async Task MakeEquipment()
        {
            if (_makeEquipment != null) { await _makeEquipment(); }
        }
    }
}