using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing.Service_Navigation;
using ff14bot.RemoteAgents;
using ff14bot.RemoteWindows;
using GreyMagic;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Provides helpers for housing garden management: teleporting to a garden, watering plants,
    /// fertilizing with Fish Meal (item 7767), and planting seeds with soil.
    /// </summary>
    public static class GardenHelper
    {
        private static readonly LLogger Log = new("TheGardener", Colors.LawnGreen);

        /// <summary>
        /// Reads the currently-selected soil item struct from the <see cref="AgentHousingPlant"/> agent memory.
        /// </summary>
        public static HousingPlantSelectedItemStruct SoilStruct => Core.Memory.Read<HousingPlantSelectedItemStruct>(AgentHousingPlant.Instance.Pointer + GardenHelperOffsets.StructOffset);

        /// <summary>
        /// Reads the currently-selected seed item struct from the <see cref="AgentHousingPlant"/> agent memory
        /// (immediately follows the <see cref="SoilStruct"/> in memory).
        /// </summary>
        public static HousingPlantSelectedItemStruct SeedStruct => Core.Memory.Read<HousingPlantSelectedItemStruct>(AgentHousingPlant.Instance.Pointer + GardenHelperOffsets.StructOffset + MarshalCache<HousingPlantSelectedItemStruct>.Size);

        /*
        public static async Task GoGarden(uint AE, Vector3 gardenLoc)
        {
            Navigator.PlayerMover = new SlideMover();
            Navigator.NavigationProvider = new ServiceNavigationProvider();
            var house = WorldManager.AvailableLocations.FirstOrDefault(i => i.AetheryteId == AE);

            Log.Information($"Teleporting to housing: (ZID: {DataManager.ZoneNameResults[house.ZoneId]}, AID: {house.AetheryteId}) {house.Name}");
            await CommonTasks.Teleport(house.AetheryteId);

            Log.Debug("Waiting for zone to change.");
            await Coroutine.Wait(20000, () => WorldManager.ZoneId == house.ZoneId);

            Log.Information("Moving to selected garden plot.");

            if (gardenLoc != null)
            {
                await Navigation.FlightorMove(gardenLoc);
                await GardenHelper.Main(gardenLoc);
            }
        }
                */

        /// <summary>
        /// Teleports to the housing zone identified by aetheryte <paramref name="AE"/>, moves to
        /// <paramref name="gardenLoc"/>, and runs the watering/fertilizing cycle via <see cref="Main"/>.
        /// Does nothing when <paramref name="gardenLoc"/> is the default value.
        /// </summary>
        /// <remarks>The <paramref name="plantPlan"/> parameter is accepted but currently unused.</remarks>
        /// <param name="AE">Aetheryte ID of the housing zone to teleport to.</param>
        /// <param name="gardenLoc">World position of the garden plot to tend.</param>
        /// <param name="plantPlan">Reserved for future planting plan support; not currently used.</param>
        public static async Task GoGarden(uint AE, Vector3 gardenLoc, List<Tuple<uint, uint>> plantPlan)
        {
            if (gardenLoc != default)
            {
                if (Navigator.NavigationProvider == null)
                {
                    Navigator.PlayerMover = new SlideMover();
                    Navigator.NavigationProvider = new ServiceNavigationProvider();
                }

                var house = WorldManager.AvailableLocations.FirstOrDefault(i => i.AetheryteId == AE);

                Log.Information($"Teleporting to housing: {house.Name} (Zone: {DataManager.ZoneNameResults[house.ZoneId]}, Aetheryte: {house.AetheryteId})");
                await GeneralFunctions.StopBusy(dismount: false);
                await CommonTasks.Teleport(house.AetheryteId);

                Log.Information("Waiting for zone to change.");
                await Coroutine.Wait(20000, () => WorldManager.ZoneId == house.ZoneId);

                if (WorldManager.ZoneId != house.ZoneId)
                {
                    Log.Information("Teleport failed for some reason, trying again.");
                    await CommonTasks.Teleport(house.AetheryteId);
                }

                Log.Information("Moving to selected garden plot.");
                await Navigation.FlightorMove(gardenLoc);
                await Main(gardenLoc);
            }
            else
            {
                Log.Information("No Garden Location set. Exiting Task.");
            }
        }

        /// <summary>
        /// When <c>true</c>, all plants within range are watered regardless of whether they actually need it.
        /// When <c>false</c> (default), only plants flagged as needing water by <see cref="GardenManager.NeedsWatering"/> are watered.
        /// </summary>
        public static bool AlwaysWater { get; set; }

        /// <summary>
        /// Waters all plants within 10 yalms of <paramref name="gardenLoc"/> that need attention,
        /// then fertilizes all plants in range using Fish Meal (item ID 7767) from the player's bags.
        /// </summary>
        /// <param name="gardenLoc">Centre point of the garden plot; used to filter nearby plants.</param>
        /// <returns><c>true</c> when the routine completes (always).</returns>
        public static async Task<bool> Main(Vector3 gardenLoc)
        {
            var watering = GardenManager.Plants.Where(r => !Blacklist.Contains(r) && r.Distance2D(gardenLoc) < 10).ToArray();
            foreach (var plant in watering)
            {
                //Water it if it needs it or if we have fertilized it 5 or more times.
                if (AlwaysWater || GardenManager.NeedsWatering(plant))
                {
                    var result = GardenManager.GetCrop(plant);
                    if (result != null)
                    {
                        Log.Information($"Watering {result} {plant.ObjectId:X}");
                        await Navigation.FlightorMove(plant.Location);
                        plant.Interact();
                        if (!await Coroutine.Wait(5000, () => Talk.DialogOpen))
                        {
                            continue;
                        }

                        Talk.Next();
                        if (!await Coroutine.Wait(5000, () => SelectString.IsOpen))
                        {
                            continue;
                        }

                        if (!await Coroutine.Wait(5000, () => SelectString.LineCount > 0))
                        {
                            continue;
                        }

                        if (SelectString.LineCount == 4)
                        {
                            SelectString.ClickSlot(1);
                            await Coroutine.Sleep(2300);
                        }
                        else
                        {
                            Log.Information("Plant is ready to be harvested");
                            SelectString.ClickSlot(1);
                            await Coroutine.Sleep(1000);
                        }
                    }
                    else
                    {
                        Log.Error($"GardenManager.GetCrop returned null {plant.ObjectId:X}");
                    }
                }
            }

            var slots = GeneralFunctions.MainBagsFilledSlots().Where(x => x.RawItemId == 7767).ToList();
            Log.Information($"Found {slots.Count} slots filled with fish meal.");
            if (slots.Count < 1)
            {
                Log.Information("No fertilizer in bag, skipping fertilize.");
                return true;
            }
            var plants = GardenManager.Plants.Where(r => r.Distance2D(gardenLoc) < 10).ToArray();
            foreach (var plant in plants)
            {
                var result = GardenManager.GetCrop(plant);
                if (result == null)
                {
                    continue;
                }

                Log.Information($"Fertilizing {GardenManager.GetCrop(plant)} {plant.ObjectId:X}");
                await Navigation.FlightorMove(plant.Location);
                plant.Interact();
                if (!await Coroutine.Wait(5000, () => Talk.DialogOpen))
                {
                    continue;
                }

                Talk.Next();
                if (!await Coroutine.Wait(5000, () => SelectString.IsOpen))
                {
                    continue;
                }

                if (!await Coroutine.Wait(5000, () => SelectString.LineCount > 0))
                {
                    continue;
                }

                if (SelectString.LineCount == 4)
                {
                    SelectString.ClickSlot(0);
                    if (await Coroutine.Wait(2000, () => GardenManager.ReadyToFertilize))
                    {
                        if (GardenManager.Fertilize() != FertilizeResult.Success)
                        {
                            continue;
                        }

                        Log.Information($"Plant with objectId {plant.ObjectId:X} was fertilized");
                        await Coroutine.Sleep(2300);
                    }
                    else
                    {
                        Log.Information($"Plant with objectId {plant.ObjectId:X} not able to be fertilized, trying again later");
                    }
                }
                else
                {
                    Log.Information("Plant is ready to be harvested");
                    SelectString.ClickSlot(1);
                    await Coroutine.Sleep(1000);
                }
            }

            return true;
        }

        /// <summary>
        /// Plants <paramref name="seeds"/> and <paramref name="soil"/> in the currently open
        /// <see cref="HousingGardening"/> window using the native plant function, then confirms
        /// the placement dialogue.
        /// </summary>
        /// <param name="seeds">Bag slot containing the seed item.</param>
        /// <param name="soil">Bag slot containing the soil item.</param>
        public static async Task Plant(BagSlot seeds, BagSlot soil)
        {
            Core.Memory.CallInjectedWraper<IntPtr>(GardenHelperOffsets.PlantFunction,
                                                   AgentHousingPlant.Instance.Pointer,
                                                   (uint)soil.BagId,
                                                   soil.Slot);
            Core.Memory.CallInjectedWraper<IntPtr>(GardenHelperOffsets.PlantFunction,
                                                   AgentHousingPlant.Instance.Pointer,
                                                   (uint)seeds.BagId,
                                                   seeds.Slot);

            await Coroutine.Wait(5000, () => SeedStruct.ItemId == seeds.RawItemId && SoilStruct.ItemId == soil.RawItemId);
            HousingGardening.Confirm();
            await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
            if (SelectYesno.IsOpen)
            {
                SelectYesno.Yes();
            }

            await Coroutine.Wait(5000, () => !SelectYesno.IsOpen);
        }

        /// <summary>
        /// Finds the garden plot matching <paramref name="GardenIndex"/> and <paramref name="PlantIndex"/>
        /// within 10 yalms of the player, then delegates to <see cref="Plant(EventObject?,BagSlot,BagSlot)"/>.
        /// </summary>
        /// <param name="GardenIndex">The housing gardening index of the target plot.</param>
        /// <param name="PlantIndex">The plant slot index within the target plot.</param>
        /// <param name="seeds">Bag slot containing the seed item.</param>
        /// <param name="soil">Bag slot containing the soil item.</param>
        public static async Task Plant(int GardenIndex, int PlantIndex, BagSlot seeds, BagSlot soil)
        {
            var plants = GardenManager.Plants.Where(i => i.Distance(Core.Me.Location) < 10);
            EventObject? plant = null;
            foreach (var tmpPlant in plants)
            {
                var _GardenIndex = Lua.GetReturnVal<int>($"return _G['{tmpPlant.LuaString}']:GetHousingGardeningIndex();");
                if (_GardenIndex != GardenIndex)
                {
                    continue;
                }

                var _PlantIndex = Lua.GetReturnVal<int>($"return _G['{tmpPlant.LuaString}']:GetHousingGardeningPlantIndex();");
                if (_PlantIndex != PlantIndex)
                {
                    continue;
                }

                var _Plant = DataManager.GetItem(Lua.GetReturnVal<uint>($"return _G['{tmpPlant.LuaString}']:GetHousingGardeningPlantCrop();"));
                if (_Plant != null)
                {
                    plant = tmpPlant;
                    break;
                }
            }

            if (plant != null)
            {
                await Plant(plant, seeds, soil);
            }
        }

        /// <summary>
        /// Moves to <paramref name="plant"/>, opens the gardening interaction menu, navigates to the
        /// <see cref="HousingGardening"/> window, and calls <see cref="Plant(BagSlot,BagSlot)"/> to
        /// complete the planting. Does nothing if <paramref name="plant"/> is <c>null</c>.
        /// </summary>
        /// <param name="plant">The garden plot event object to interact with.</param>
        /// <param name="seeds">Bag slot containing the seed item.</param>
        /// <param name="soil">Bag slot containing the soil item.</param>
        public static async Task Plant(EventObject? plant, BagSlot seeds, BagSlot soil)
        {
            if (plant != null)
            {
                if (!plant.IsWithinInteractRange)
                {
                    await Navigation.FlightorMove(plant.Location);
                }

                if (plant.IsWithinInteractRange)
                {
                    plant.Interact();
                    await Coroutine.Wait(5000, () => Talk.DialogOpen);
                    if (Talk.DialogOpen)
                    {
                        Talk.Next();
                    }

                    await Coroutine.Wait(5000, () => Conversation.IsOpen);
                    if (Conversation.IsOpen)
                    {
                        Conversation.SelectLine(0);
                    }

                    await Coroutine.Wait(5000, () => HousingGardening.IsOpen);
                    if (HousingGardening.IsOpen)
                    {
                        await Plant(seeds, soil);
                    }
                }
            }
        }
    }
}