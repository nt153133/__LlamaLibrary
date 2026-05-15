using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.NeoProfiles;
using ff14bot.Pathing.Service_Navigation;
using ff14bot.RemoteAgents;
using ff14bot.RemoteWindows;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Helpers;

/// <summary>
/// Automates feeding and managing chocobo companions stabled in player housing.
/// Handles stable navigation, food selection, Thavnairian Onion logic for capped chocobos,
/// and optional fetch-after-feeding.
/// </summary>
public static class ChocoboHelper
{
    private static readonly string Name = "ChocoboHelper";
    private static readonly Color LogColor = Colors.Yellow;
    private static readonly LLogger Log = new(Name, LogColor);

    internal static Regex TimeRegex = new(@"(?:.*?)(\d+).*", RegexOptions.Compiled);

    /// <summary>
    /// Teleports to the specified housing aetheryte, navigates to the stable location, then
    /// runs the full chocobo feeding and management workflow.
    /// </summary>
    /// <param name="AE">The aetheryte ID for the housing area to teleport to.</param>
    /// <param name="stableLoc">World position of the Chocobo Stable object.</param>
    /// <param name="CleanBefore">If <see langword="true"/>, cleans the stable with Magicked Stable Brooms before feeding.</param>
    /// <param name="ChocoboFoodId">Item ID of the food to feed to chocobos (e.g., Gysahl Greens = 4868).</param>
    /// <param name="FetchAfter">If <see langword="true"/>, fetches the player's chocobo back out of the stable after feeding.</param>
    /// <param name="UseThavnairianOnion">If <see langword="true"/>, automatically switches to Thavnairian Onions for rank-capped chocobos.</param>
    public static async Task GoGoChocobo(uint AE, Vector3 stableLoc, bool CleanBefore, uint ChocoboFoodId, bool FetchAfter, bool UseThavnairianOnion)
    {
        if (stableLoc != default)
        {
            Navigator.PlayerMover = new SlideMover();
            Navigator.NavigationProvider = new ServiceNavigationProvider();
            var house = WorldManager.AvailableLocations.FirstOrDefault(i => i.AetheryteId == AE);

            if (WorldManager.ZoneId != house.ZoneId)
            {
                Log.Information($"Teleporting to housing: {house.Name} (Zone: {DataManager.ZoneNameResults[house.ZoneId]}, Aetheryte: {house.AetheryteId})");
                await GeneralFunctions.StopBusy(dismount: false);
                await CommonTasks.Teleport(house.AetheryteId);

                Log.Information("Waiting for zone to change.");
                await Coroutine.Wait(20000, () => WorldManager.ZoneId == house.ZoneId);
            }

            if (WorldManager.ZoneId != house.ZoneId)
            {
                Log.Information("Teleport failed for some reason, trying again.");
                await GeneralFunctions.StopBusy(dismount: false);
                await CommonTasks.Teleport(house.AetheryteId);
            }

            Log.Information("Moving to selected stable location.");
            await Navigation.FlightorMove(stableLoc);
            await Main(CleanBefore, ChocoboFoodId, FetchAfter, UseThavnairianOnion);
        }
        else
        {
            Log.Information("No Stable Location set. Exiting Task.");
        }
    }

    /// <summary>
    /// Core stable interaction workflow: optionally cleans the stable, stables the chocobo if not already stabled,
    /// feeds it (switching to Thavnairian Onions if rank-capped), and optionally fetches the chocobo back out.
    /// </summary>
    /// <param name="CleanBefore">If <see langword="true"/>, uses up to 3 Magicked Stable Brooms before feeding.</param>
    /// <param name="ChocoboFoodId">Item ID of the food to feed to chocobos.</param>
    /// <param name="FetchAfter">If <see langword="true"/>, retrieves the player's chocobo after the feeding session.</param>
    /// <param name="UseThavnairianOnion">If <see langword="true"/>, switches to Thavnairian Onions for rank-capped chocobos.</param>
    /// <returns><see langword="true"/> when the workflow completes successfully.</returns>
    public static async Task<bool> Main(bool CleanBefore, uint ChocoboFoodId, bool FetchAfter, bool UseThavnairianOnion)
    {
        if (CleanBefore)
        {
            for (var i = 1; i <= 3; i++)
            {
                if (ConditionParser.HasAtLeast(8168, 1))
                {
                    foreach (var unit in GameObjectManager.GameObjects.OrderBy(r => r.Distance()))
                    {
                        if (string.Equals(unit.EnglishName, "Chocobo Stable", StringComparison.Ordinal))
                        {
                            unit.Interact();
                            break;
                        }
                    }

                    await Coroutine.Wait(5000, () => SelectString.IsOpen);
                    if (!SelectString.IsOpen)
                    {
                        continue;
                    }

                    SelectString.ClickSlot((uint)SelectString.LineCount - 3);
                    await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
                    Log.Information($"Cleaning Stable {i}");
                    SelectYesno.ClickYes();
                    await Coroutine.Sleep(2000);
                }
                else
                {
                    Log.Information("No Magicked Stable Broom left");
                    break;
                }
            }
        }

        if (!ChocoboManager.IsStabled)
        {
            foreach (var unit in GameObjectManager.GameObjects.OrderBy(r => r.Distance()))
            {
                if (string.Equals(unit.EnglishName, "Chocobo Stable", StringComparison.Ordinal))
                {
                    unit.Interact();
                    break;
                }
            }

            await Coroutine.Wait(5000, () => SelectString.IsOpen);
            if (SelectString.IsOpen)
            {
                SelectString.ClickSlot(1);
                await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
                Log.Information("Chocobo Stabled");
                SelectYesno.ClickYes();
            }
            else
            {
                Log.Information("Failed to open menu");
            }
        }

        await Coroutine.Sleep(3000);
        foreach (var unit in GameObjectManager.GameObjects.OrderBy(r => r.Distance()))
        {
            if (string.Equals(unit.EnglishName, "Chocobo Stable", StringComparison.Ordinal))
            {
                unit.Interact();
                break;
            }
        }

        await Coroutine.Wait(5000, () => SelectString.IsOpen);
        if (SelectString.IsOpen)
        {
            SelectString.ClickSlot(0);
            await Coroutine.Wait(5000, () => HousingChocoboList.IsOpen);
            await Coroutine.Sleep(1500);
            if (HousingChocoboList.IsOpen)
            {
                //Look for our chocobo
                var items = HousingChocoboList.Items;
                //512 possible chocobos, 14 items per page...
                for (uint stableSection = 0; stableSection < AgentHousingBuddyList.Instance.TotalPages; stableSection++)
                {
                    if (stableSection != AgentHousingBuddyList.Instance.CurrentPage)
                    {
                        Log.Information($"Switching to page {stableSection}");
                        HousingChocoboList.SelectSection(stableSection);
                        await Coroutine.Sleep(5000);
                        items = HousingChocoboList.Items;
                    }

                    for (uint i = 0; i < items.Length; i++)
                    {
                        if (string.IsNullOrEmpty(items[i].PlayerName))
                        {
                            continue;
                        }

                        if (i == 0)
                        {
                            if (items[i].ReadyAt < DateTime.Now)
                            {
                                Log.Information("Selecting my Chocobo");
                                HousingChocoboList.SelectMyChocobo();
                                if (await Coroutine.Wait(5000, () => SelectYesno.IsOpen) && !UseThavnairianOnion)
                                {
                                    Log.Information($"{items[i].ChocoboName}, {items[i].PlayerName}'s chocobo is maxed out");
                                    SelectYesno.ClickNo();
                                    await Coroutine.Sleep(1000);
                                    continue;
                                }

                                if (await Coroutine.Wait(5000, () => SelectYesno.IsOpen) && UseThavnairianOnion)
                                {
                                    if (ConditionParser.HasAtLeast(8166, 1))
                                    {
                                        Log.Information($"{items[i].ChocoboName}, {items[i].PlayerName}'s chocobo is maxed out, changing food to Thavnairian Onion");
                                        SelectYesno.ClickNo();
                                        await Coroutine.Wait(1000, () => !SelectYesno.IsOpen);
                                        await Coroutine.Sleep(500);
                                        Log.Information($"Selecting {items[i].ChocoboName}, {items[i].PlayerName}'s chocobo on page {stableSection}");
                                        HousingChocoboList.SelectMyChocobo();
                                        await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
                                        SelectYesno.ClickYes();
                                        ChocoboFoodId = 8166;
                                    }
                                    else
                                    {
                                        Log.Information($"{items[i].ChocoboName}, {items[i].PlayerName}'s chocobo is maxed out but you don't have any Thavnairian Onion");
                                        SelectYesno.ClickNo();
                                        await Coroutine.Sleep(1000);
                                        continue;
                                    }
                                }

                                Log.Information("Waiting for inventory menu to appear....");
                                //Wait for the inventory window to open and be ready
                                //Technically the inventory windows are always 'open' so we check if their callbackhandler has been set
                                if (!await Coroutine.Wait(5000, () => AgentInventoryContext.Instance.CallbackHandlerSet))
                                {
                                    Log.Information("Inventory menu failed to appear, aborting current iteration.");
                                    continue;
                                }

                                Log.Information($"Feeding Chocobo : Food Name : {DataManager.GetItem(ChocoboFoodId).CurrentLocaleName}, Food ID : {ChocoboFoodId}");
                                AgentHousingBuddyList.Instance.Feed(ChocoboFoodId);
                                if (await Coroutine.Wait(5000, () => SelectYesno.IsOpen))
                                {
                                    SelectYesno.ClickYes();
                                    await Coroutine.Sleep(1000);
                                }

                                Log.Information("Waiting for cutscene to start....");
                                if (await Coroutine.Wait(5000, () => QuestLogManager.InCutscene))
                                {
                                    Log.Information("Waiting for cutscene to end....");
                                    await Coroutine.Wait(Timeout.Infinite, () => !QuestLogManager.InCutscene);
                                }

                                Log.Information("Waiting for menu to reappear....");
                                await Coroutine.Wait(Timeout.Infinite, () => HousingChocoboList.IsOpen);
                                await Coroutine.Sleep(1000);
                            }
                            else
                            {
                                Log.Information($"{items[i].ChocoboName}, {items[i].PlayerName}'s chocobo can't be fed yet ...");
                            }
                        }
                        else if (string.Equals(items[i].PlayerName, Core.Me.Name, StringComparison.OrdinalIgnoreCase) || string.Equals("All", Core.Me.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            if (items[i].ReadyAt < DateTime.Now)
                            {
                                Log.Information($"Selecting {items[i].ChocoboName}, {items[i].PlayerName}'s chocobo on page {stableSection}");
                                HousingChocoboList.SelectChocobo(i);
                                //Chocobo is maxed out, don't interact with it again
                                if (await Coroutine.Wait(5000, () => SelectYesno.IsOpen) && !UseThavnairianOnion)
                                {
                                    Log.Information($"{items[i].ChocoboName}, {items[i].PlayerName}'s chocobo is maxed out");
                                    SelectYesno.ClickNo();
                                    await Coroutine.Sleep(1000);
                                    continue;
                                }

                                //Chocobo is maxed out, don't interact with it again
                                if (await Coroutine.Wait(5000, () => SelectYesno.IsOpen) && UseThavnairianOnion)
                                {
                                    if (ConditionParser.HasAtLeast(8166, 1))
                                    {
                                        Log.Information($"{items[i].ChocoboName}, {items[i].PlayerName}'s chocobo is maxed out, changing food to Thavnairian Onion");
                                        SelectYesno.ClickNo();
                                        await Coroutine.Wait(1000, () => !SelectYesno.IsOpen);
                                        await Coroutine.Sleep(500);
                                        Log.Information($"Selecting {items[i].ChocoboName}, {items[i].PlayerName}'s chocobo on page {stableSection}");
                                        HousingChocoboList.SelectChocobo(i);
                                        await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
                                        SelectYesno.ClickYes();
                                        ChocoboFoodId = 8166;
                                    }
                                    else
                                    {
                                        Log.Information($"{items[i].ChocoboName}, {items[i].PlayerName}'s chocobo is maxed out but you don't have any Thavnairian Onion");
                                        SelectYesno.ClickNo();
                                        await Coroutine.Sleep(1000);
                                        continue;
                                    }
                                }

                                Log.Information("Waiting for inventory menu to appear....");
                                //Wait for the inventory window to open and be ready
                                //Technically the inventory windows are always 'open' so we check if their callbackhandler has been set
                                if (!await Coroutine.Wait(5000, () => AgentInventoryContext.Instance.CallbackHandlerSet))
                                {
                                    Log.Information("Inventory menu failed to appear, aborting current iteration.");
                                    continue;
                                }

                                Log.Information($"Feeding Chocobo : Food Name : {DataManager.GetItem(ChocoboFoodId).CurrentLocaleName}, Food ID : {ChocoboFoodId}");
                                AgentHousingBuddyList.Instance.Feed(ChocoboFoodId);
                                if (await Coroutine.Wait(5000, () => SelectYesno.IsOpen))
                                {
                                    SelectYesno.ClickYes();
                                    await Coroutine.Sleep(1000);
                                }

                                Log.Information("Waiting for cutscene to start....");
                                if (await Coroutine.Wait(5000, () => QuestLogManager.InCutscene))
                                {
                                    Log.Information("Waiting for cutscene to end....");
                                    await Coroutine.Wait(Timeout.Infinite, () => !QuestLogManager.InCutscene);
                                }

                                Log.Information("Waiting for menu to reappear....");
                                await Coroutine.Wait(Timeout.Infinite, () => HousingChocoboList.IsOpen);
                                await Coroutine.Sleep(1000);
                            }
                            else
                            {
                                Log.Information($"{items[i].ChocoboName}, {items[i].PlayerName}'s chocobo can't be fed yet ...");
                            }
                        }
                    }
                }

                await Coroutine.Sleep(500);
                HousingChocoboList.Close();
                await Coroutine.Wait(5000, () => !HousingChocoboList.IsOpen);
            }
            else if (HousingMyChocobo.IsOpen)
            {
                var matches = TimeRegex.Match(HousingMyChocobo.Lines[0]);
                if (!matches.Success)
                {
                    //We are ready to train now
                    HousingMyChocobo.SelectLine(0);
                    //Chocobo is maxed out, don't interact with it again
                    if (await Coroutine.Wait(5000, () => SelectYesno.IsOpen) && !UseThavnairianOnion)
                    {
                        Log.Information("Your chocobo is maxed out");
                        SelectYesno.ClickNo();
                        await Coroutine.Sleep(1000);
                    }

                    //Chocobo is maxed out, don't interact with it again
                    if (await Coroutine.Wait(5000, () => SelectYesno.IsOpen) && UseThavnairianOnion)
                    {
                        if (ConditionParser.HasAtLeast(8166, 1))
                        {
                            Log.Information("Your chocobo is maxed out, changing food to Thavnairian Onion");
                            SelectYesno.ClickNo();
                            await Coroutine.Wait(1000, () => !SelectYesno.IsOpen);
                            await Coroutine.Sleep(500);
                            HousingMyChocobo.SelectLine(0);
                            await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
                            SelectYesno.ClickYes();
                            ChocoboFoodId = 8166;
                        }
                        else
                        {
                            Log.Information("Your chocobo is maxed out but you don't have any Thavnairian Onion");
                            SelectYesno.ClickNo();
                            await Coroutine.Sleep(1000);
                        }
                    }

                    Log.Information("Waiting for inventory menu to appear....");
                    //Wait for the inventory window to open and be ready
                    //Technically the inventory windows are always 'open' so we check if their callbackhandler has been set
                    if (!await Coroutine.Wait(5000, () => AgentInventoryContext.Instance.CallbackHandlerSet))
                    {
                        Log.Information("Inventory menu failed to appear, aborting current iteration.");
                        return true;
                    }

                    Log.Information($"Feeding Chocobo : Food Name : {DataManager.GetItem(ChocoboFoodId).CurrentLocaleName}, Food ID : {ChocoboFoodId}");
                    AgentHousingBuddyList.Instance.Feed(ChocoboFoodId);
                    if (await Coroutine.Wait(5000, () => SelectYesno.IsOpen))
                    {
                        SelectYesno.ClickYes();
                        await Coroutine.Sleep(1000);
                    }

                    Log.Information("Waiting for cutscene to start....");
                    if (await Coroutine.Wait(5000, () => QuestLogManager.InCutscene))
                    {
                        Log.Information("Waiting for cutscene to end....");
                        await Coroutine.Wait(Timeout.Infinite, () => !QuestLogManager.InCutscene);
                    }

                    Log.Information("Waiting for menu to reappear....");
                    await Coroutine.Wait(Timeout.Infinite, () => HousingMyChocobo.IsOpen);
                    await Coroutine.Sleep(1000);
                }
                else
                {
                    Log.Information("Your chocobo can't be fed yet ...");
                }

                await Coroutine.Sleep(500);
                HousingMyChocobo.Close();
                await Coroutine.Wait(5000, () => !HousingMyChocobo.IsOpen);
            }
            else
            {
                Log.Information("Failed to open Chocobo list");
            }

            SelectString.ClickSlot((uint)SelectString.LineCount - 1);
            await Coroutine.Wait(5000, () => !SelectString.IsOpen);
        }
        else
        {
            Log.Information("Failed to open menu");
        }

        await Coroutine.Sleep(3000);
        if (!FetchAfter)
        {
            return true;
        }

        {
            foreach (var unit in GameObjectManager.GameObjects.OrderBy(r => r.Distance()))
            {
                if (string.Equals(unit.EnglishName, "Chocobo Stable", StringComparison.Ordinal))
                {
                    unit.Interact();
                    break;
                }
            }

            await Coroutine.Wait(5000, () => SelectString.IsOpen);
            if (SelectString.IsOpen)
            {
                SelectString.ClickSlot(1);
                await Coroutine.Wait(5000, () => HousingMyChocobo.IsOpen);
                if (HousingMyChocobo.IsOpen)
                {
                    HousingMyChocobo.SelectLine(3);
                    await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
                    SelectYesno.ClickYes();
                    Log.Information("Chocobo Fetch");
                }
                else
                {
                    Log.Information("Failed to acces to my chocobo");
                    SelectString.ClickSlot((uint)SelectString.LineCount - 1);
                    await Coroutine.Wait(5000, () => !SelectString.IsOpen);
                }
            }
            else
            {
                Log.Information("Failed to open menu");
            }
        }
        return true;
    }
}