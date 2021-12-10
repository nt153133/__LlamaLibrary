using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.NeoProfiles;
using ff14bot.Objects;
using ff14bot.RemoteWindows;
using LlamaLibrary.Extensions;
using LlamaLibrary.Helpers;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Structs;

namespace LlamaLibrary.Retainers
{
    //TODO This whole shitshow needs to get moved to a Helper class but it is used in a lot of external things so it's going to be messy
    public static class HelperFunctions
    {
        private static readonly LLogger Log = new LLogger("Helpers", Colors.Pink);

        public const InventoryBagId RetainerGilId = InventoryBagId.Retainer_Gil;

        public const InventoryBagId PlayerGilId = InventoryBagId.Currency;

        public static readonly InventoryBagId[] PlayerInventoryBagIds = new InventoryBagId[6]
        {
            InventoryBagId.Bag1,
            InventoryBagId.Bag2,
            InventoryBagId.Bag3,
            InventoryBagId.Bag4,
            InventoryBagId.Bag5,
            InventoryBagId.Bag6
        };

        public static readonly InventoryBagId[] RetainerBagIds =
        {
            InventoryBagId.Retainer_Page1, InventoryBagId.Retainer_Page2, InventoryBagId.Retainer_Page3,
            InventoryBagId.Retainer_Page4, InventoryBagId.Retainer_Page5, InventoryBagId.Retainer_Page6,
            InventoryBagId.Retainer_Page7
        };

        private static readonly List<(uint ZoneId, Vector3 Location)> SummoningBells = new List<(uint ZoneId, Vector3 Location)>
        {
            (129, new Vector3(-223.743042f, 16.006714f, 41.306152f)), //Limsa Lominsa Lower Decks(Limsa Lominsa)
            (129, new Vector3(-266.376831f, 16.006714f, 41.275635f)), //Limsa Lominsa Lower Decks(Limsa Lominsa)
            (129, new Vector3(-149.279053f, 18.203979f, 20.553894f)), //Limsa Lominsa Lower Decks(Limsa Lominsa)
            (129, new Vector3(-123.888062f, 17.990356f, 21.469421f)), //Limsa Lominsa Lower Decks(Limsa Lominsa)
            (131, new Vector3(148.91272f, 3.982544f, -44.205383f)), //Ul'dah - Steps of Thal(Ul'dah)
            (131, new Vector3(111.161987f, 4.104675f, -72.343079f)), //Ul'dah - Steps of Thal(Ul'dah)
            (131, new Vector3(153.185303f, 3.982544f, 13.229492f)), //Ul'dah - Steps of Thal(Ul'dah)
            (131, new Vector3(118.547363f, 4.013123f, -93.003784f)), //Ul'dah - Steps of Thal(Ul'dah)
            (133, new Vector3(160.234863f, 15.671021f, -55.649719f)), //Old Gridania(Gridania)
            (133, new Vector3(169.726074f, 15.487854f, -81.895203f)), //Old Gridania(Gridania)
            (133, new Vector3(171.007812f, 15.487854f, -101.487854f)), //Old Gridania(Gridania)
            (133, new Vector3(160.234863f, 15.671021f, -136.369934f)), //Old Gridania(Gridania)
            (156, new Vector3(34.50061f, 28.976807f, -762.233948f)), //Mor Dhona(Mor Dhona)
            (156, new Vector3(11.001709f, 28.976807f, -734.554077f)), //Mor Dhona(Mor Dhona)
            (419, new Vector3(-151.171204f, -12.64978f, -11.764771f)), //The Pillars(Ishgard)
            (478, new Vector3(34.775269f, 208.148193f, -50.858398f)), //Idyllshire(Dravania)
            (478, new Vector3(0.38147f, 206.469727f, 51.407593f)), //Idyllshire(Dravania)
            (628, new Vector3(19.394226f, 4.043579f, 53.025024f)), //Kugane(Kugane)
            (635, new Vector3(-57.633362f, -0.01532f, 49.30188f)), //Rhalgr's Reach(Gyr Abania)
            (819, new Vector3(-69.840576f, -7.705872f, 123.491211f)), //The Crystarium(The Crystarium)
            (819, new Vector3(-64.255798f, 19.97406f, -144.274109f)), //The Crystarium(The Crystarium)
            (820, new Vector3(7.186951f, 83.17688f, 31.448853f)), //Eulmore(Eulmore)
            (962, new Vector3(41.85025f, 2.453362f, -38.73349f)) //Old Sharlayan
        };

        public static readonly uint GilItemId = DataManager.GetItem("Gil").Id; // 1;

        public static int UnixTimestamp => (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

        public static ulong CurrentRetainer => Core.Memory.NoCacheRead<ulong>(Offsets.RetainerData + Offsets.CurrentRetainer);

        public static bool FilterStackable(BagSlot item)
        {
            if (item.IsCollectable)
            {
                return false;
            }

            if (item.Item.StackSize < 2)
            {
                return false;
            }

            return item.Count != item.Item.StackSize;
        }

        public static uint NormalRawId(uint trueItemId)
        {
            if (trueItemId > 500_000U)
            {
                return trueItemId - 500_000U;
            }

            if (trueItemId > 1_000_000U)
            {
                return trueItemId - 1_000_000U;
            }

            return trueItemId;
        }

        public static GameObject NearestSummoningBell()
        {
            var list = GameObjectManager.GameObjects
                .Where(r => r.Name == Translator.SummoningBell)
                .OrderBy(j => j.DistanceSqr())
                .ToList();

            if (list.Count <= 0)
            {
                Log.Error("No Summoning Bell Found");
                return null;
            }

            var bell = list[0];

            Log.Information($"Found nearest bell: {bell} Distance: {bell.Distance2D(Core.Me.Location)}");

            return bell;
        }

        public static bool GetRetainerGil()
        {
            var playerGilSlot = InventoryManager.GetBagByInventoryBagId(PlayerGilId).Where(r => r.IsFilled).FirstOrDefault(item => item.RawItemId == GilItemId);
            var retainerGilSlot = InventoryManager.GetBagByInventoryBagId(RetainerGilId).Where(r => r.IsFilled).FirstOrDefault(item => item.RawItemId == GilItemId);

            if (retainerGilSlot == null || playerGilSlot == null || retainerGilSlot.Count <= 0)
            {
                return false;
            }

            Log.Information($"Retainer: {retainerGilSlot.Count:n0}  Player: {playerGilSlot.Count:n0}");

            return retainerGilSlot.Move(playerGilSlot);
        }

        public static async Task<bool> UseSummoningBell()
        {
            await GoToSummoningBell();
            var bell = FindSummoningBell();

            if (bell == null || !bell.IsWithinInteractRange)
            {
                Log.Error("No summoning bell near by");
                return false;
            }

            bell.Interact();
            await Coroutine.Wait(3000, () => RetainerList.Instance.IsOpen);
            if (!RetainerList.Instance.IsOpen)
            {
                bell.Interact();
                await Coroutine.Wait(5000, () => RetainerList.Instance.IsOpen);
            }

            return RetainerList.Instance.IsOpen;
        }

        public static async Task<bool> GoToSummoningBell()
        {
            var searchBell = FindSummoningBell();
            if (searchBell != null)
            {
                if (searchBell.IsWithinInteractRange)
                {
                    Log.Information("Found bell in Interact Range");
                    return true;
                }

                if (await Navigation.GetTo(WorldManager.ZoneId, searchBell.Location))
                {
                    Log.Information("Used Navgraph/Flightor to get there");
                    if (searchBell.IsWithinInteractRange)
                    {
                        return true;
                    }
                }
            }

            (uint ZoneId, Vector3 Location) bellLocation;
            var tries = 0;
            if (SummoningBells.Any(i => i.Item1 == WorldManager.ZoneId))
            {
                Log.Information("Found a bell in our zone");
                bellLocation = SummoningBells.Where(i => i.Item1 == WorldManager.ZoneId).OrderBy(r => Core.Me.Location.DistanceSqr(r.Item2)).First();
            }
            else
            {
                var foundBell = false;
                var rand = new Random();
                List<uint> zoneList = new List<uint>();
                List<(uint bellzone, uint truezone)> zoneMap = new List<(uint bellzone, uint truezone)>();

                foreach (var bell in SummoningBells)
                {
                    uint zone = bell.ZoneId;
                    switch (zone)
                    {
                        case 131:
                            zone = DataManager.AetheryteCache.Values.FirstOrDefault(i => i.Id == 9).ZoneId;
                            break;
                        case 133:
                            zone = DataManager.AetheryteCache.Values.FirstOrDefault(i => i.Id == 2).ZoneId;
                            break;
                        case 419:
                            zone = DataManager.AetheryteCache.Values.FirstOrDefault(i => i.Id == 70).ZoneId;
                            break;
                    }

                    if (DataManager.AetheryteCache.Values.Any(i => i.ZoneId == zone && i.IsAetheryte))
                    {
                        zoneMap.Add((bell.ZoneId, zone));
                        zoneList.Add(zone);
                    }
                }

                var zoneId = FindCheapestZone(zoneList);
                bellLocation = SummoningBells.First(i => i.ZoneId == zoneMap.First(j=> j.truezone == zoneId).bellzone);
                /*do
                {
                    tries++;
                    var index = rand.Next(0, SummoningBells.Count);
                    bellLocation = SummoningBells[index];
                    var ae = DataManager.AetheryteCache.Values.FirstOrDefault(i => i.ZoneId == bellLocation.ZoneId && i.IsAetheryte);

                    if (ae == default(AetheryteResult))
                    {
                        switch (bellLocation.ZoneId)
                        {
                            case 131:
                                ae = DataManager.AetheryteCache.Values.FirstOrDefault(i => i.Id == 9);
                                break;
                            case 133:
                                ae = DataManager.AetheryteCache.Values.FirstOrDefault(i => i.Id == 2);
                                break;
                            case 419:
                                ae = DataManager.AetheryteCache.Values.FirstOrDefault(i => i.Id == 70);
                                break;
                        }
                    }

                    if (ae != default(AetheryteResult))
                    {
                        if (ConditionParser.HasAetheryte(ae.Id))
                        {
                            Log.Information($"{bellLocation.ZoneId} can get to ConditionParser.HasAetheryte({ae.Id}) = {ConditionParser.HasAetheryte(ae.Id)} {ae.EnglishName}");
                            foundBell = true;
                        }
                    }
                    else
                    {
                        Log.Warning($"{bellLocation.ZoneId} can't find AE");
                    }
                }
                while (!foundBell && tries < 5)*/;
            }

            Log.Information($"Going to bell {bellLocation.ZoneId} {bellLocation.Location}");
            if (await Navigation.GetTo(bellLocation.ZoneId, bellLocation.Location))
            {
                var bell = FindSummoningBell();
                if (bell != null)
                {
                    Log.Information($"{bell.Name} {bell.Location} {WorldManager.CurrentZoneName} {bell.IsWithinInteractRange}");
                }
                else
                {
                    Log.Warning($"Couldn't find bell at {bellLocation.Item2} {bellLocation.Item1}");
                }

                return bell != null;
            }

            return false;
        }

        public static uint FindCheapestZone(IEnumerable<uint> zones)
        {
            return zones.OrderBy(j => WorldManager.AvailableLocations.First(i => i.ZoneId == j).GilCost)
                .First();
        }

        public static GameObject FindSummoningBell()
        {
            uint[] bellIds = { 2000072, 2000401, 2000403, 2000439, 2000441, 2000661, 2001271, 2001358, 2006565, 2010284, 196630 };
            return GameObjectManager.GameObjects.Where(i => i.IsVisible && bellIds.Contains(i.NpcId)).OrderBy(r => r.DistanceSqr()).FirstOrDefault();
        }

        public static async Task<bool> OpenRetainerList()
        {
            if (!RetainerList.Instance.IsOpen)
            {
                await UseSummoningBell();
                await Coroutine.Wait(5000, () => RetainerList.Instance.IsOpen);
            }

            if (!RetainerList.Instance.IsOpen)
            {
                Log.Error("Can't find open bell either you have none or not near a bell");
            }

            return RetainerList.Instance.IsOpen;
        }

        public static async Task<bool> RetainerHandleVentures()
        {
            if (!SelectString.IsOpen)
            {
                return false;
            }

            if (SelectString.Lines().Contains(Translator.VentureCompleteText))
            {
                //Log.Information("Venture Done");
                SelectString.ClickLineEquals(Translator.VentureCompleteText);

                await Coroutine.Wait(5000, () => RetainerTaskResult.IsOpen);

                if (!RetainerTaskResult.IsOpen)
                {
                    Log.Error("RetainerTaskResult didn't open");
                    return false;
                }

                var taskId = AgentRetainerVenture.Instance.RetainerTask;

                var task = ResourceManager.VentureData.Value.FirstOrDefault(i => i.Id == taskId);

                if (task != default(RetainerTaskData))
                {
                    Log.Information($"Finished Venture {task.Name}");
                    Log.Information($"Reassigning Venture {task.Name}");
                }
                else
                {
                    Log.Information($"Finished Venture");
                    Log.Information($"Reassigning Venture");
                }

                RetainerTaskResult.Reassign();

                await Coroutine.Wait(5000, () => RetainerTaskAsk.IsOpen);
                if (!RetainerTaskAsk.IsOpen)
                {
                    Log.Error("RetainerTaskAsk didn't open");
                    return false;
                }

                await Coroutine.Wait(2000, RetainerTaskAskExtensions.CanAssign);
                if (RetainerTaskAskExtensions.CanAssign())
                {
                    RetainerTaskAsk.Confirm();
                }
                else
                {
                    Log.Error($"RetainerTaskAsk Error: {RetainerTaskAskExtensions.GetErrorReason()}");
                    RetainerTaskAsk.Close();
                }

                await Coroutine.Wait(1500, () => Talk.DialogOpen || SelectString.IsOpen);
                await Coroutine.Sleep(200);
                if (Talk.DialogOpen)
                {
                    Talk.Next();
                }

                await Coroutine.Sleep(200);
                await Coroutine.Wait(5000, () => SelectString.IsOpen);
            }
            else
            {
                Log.Information("Venture Not Done");
            }

            return true;
        }

        public static async Task<bool> CloseRetainerList()
        {
            if (RetainerList.Instance.IsOpen)
            {
                RetainerList.Instance.Close();
                await Coroutine.Wait(5000, () => !RetainerList.Instance.IsOpen);
            }

            return !RetainerList.Instance.IsOpen;
        }

        public static async Task<bool> VerifiedRetainerData()
        {
            if (Core.Memory.Read<uint>(Offsets.RetainerData) != 0)
            {
                return true;
            }

            await ForceGetRetainerData();
            return Core.Memory.Read<uint>(Offsets.RetainerData) != 0;
        }

        internal static bool VerifiedRetainerDataSync()
        {
            if (Core.Memory.Read<uint>(Offsets.RetainerData) != 0)
            {
                return true;
            }

            ForceGetRetainerDataSync();
            return Core.Memory.Read<uint>(Offsets.RetainerData) != 0;
        }

        public static async Task ForceGetRetainerData()
        {
            RequestRetainerData();
            await Coroutine.Wait(3000, () => Core.Memory.Read<uint>(Offsets.RetainerData) != 0);
            await Coroutine.Wait(3000, () => Core.Memory.Read<byte>(Offsets.RetainerData + Offsets.RetainerDataLoaded) != 0);
        }

        public static void ForceGetRetainerDataSync()
        {
            RequestRetainerData();
            WaitUntil(() => Core.Memory.Read<uint>(Offsets.RetainerData) != 0, timeout: 3000);
            WaitUntil(() => Core.Memory.Read<byte>(Offsets.RetainerData + Offsets.RetainerDataLoaded) != 0, timeout: 3000);
        }

        public static RetainerInfo[] ReadRetainerArray()
        {
            return Core.Memory.ReadArray<RetainerInfo>(Offsets.RetainerData, FuncNumberOfRetainers());
        }

        public static RetainerInfo[] GetRetainerArraySync(bool force = false)
        {
            if (force)
            {
                ForceGetRetainerDataSync();

                return ReadRetainerArray();
            }

            if (VerifiedRetainerDataSync())
            {
                return ReadRetainerArray();
            }

            return new RetainerInfo[0];
        }

        public static async Task<RetainerInfo[]> GetRetainerArray(bool force = false)
        {
            if (force)
            {
                await ForceGetRetainerData();

                return ReadRetainerArray();
            }

            if (await VerifiedRetainerData())
            {
                return ReadRetainerArray();
            }

            return new RetainerInfo[0];
        }

        public static async Task<RetainerInfo[]> GetOrderedRetainerArray(bool force = false)
        {
            var retainers = await GetRetainerArray(force);
            return retainers.Length == 0 ? retainers : GetOrderedRetainerArray(retainers);
        }

        public static RetainerInfo[] GetOrderedRetainerArraySync(bool force = false)
        {
            var retainers = GetRetainerArraySync(force);
            return retainers.Length == 0 ? retainers : GetOrderedRetainerArray(retainers);
        }

        public static RetainerInfo[] GetOrderedRetainerArray(RetainerInfo[] retainers)
        {
            if (retainers.Length == 0)
            {
                return retainers;
            }

            var count = retainers.Length;
            var result = new RetainerInfo[count];

            var order = Core.Memory.ReadArray<byte>(Offsets.RetainerData + Offsets.RetainerDataOrder, 0xA);

            if (order[0] == 255)
            {
                return retainers;
            }

            var index = 0;

            for (var i = 0; i < count; i++)
            {
                result[index] = retainers[order[i]];
                index++;
            }

            return result;
        }

        public static int FuncNumberOfRetainers()
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                return Core.Memory.CallInjected64<int>(
                    Offsets.GetNumberOfRetainers,
                    Offsets.RetainerData
                );
            }
        }

        public static async Task<int> GetNumberOfRetainers()
        {
            var verified = await VerifiedRetainerData();
            if (!verified)
            {
                return 0;
            }

            return FuncNumberOfRetainers();
        }

        public static void RequestRetainerData()
        {
            lock (Core.Memory.Executor.AssemblyLock)
            {
                Core.Memory.CallInjected64<IntPtr>(
                    Offsets.ExecuteCommand,
                    (uint)Offsets.RetainerNetworkPacket,
                    0U,
                    0U,
                    0U,
                    0U
                );
            }
        }

        private static bool WaitUntil(Func<bool> condition, int frequency = 25, int timeout = -1, bool checkWindows = false)
        {
            var t = Task.Run(async () =>
            {
                var waitTask = Task.Run(async () =>
                {
                    while (!condition())
                    {
                        if (checkWindows)
                        {
                            RaptureAtkUnitManager.Update();
                        }

                        await Task.Delay(frequency);
                    }
                });

                if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeout)))
                {
                    throw new TimeoutException();
                }

                return condition();
            });

            try
            {
                t.Wait();
            }
            catch (AggregateException)
            {
            }

            return condition();
        }

        public static async Task CheckVentureTask()
        {
            var verified = await VerifiedRetainerData();
            if (!verified)
            {
                return;
            }

            var rets = await GetOrderedRetainerArray(true);

            var now = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            if (rets.Any(i => i.Active && i.VentureTask != 0 && (i.VentureEndTimestamp - now) <= 0 && SpecialCurrencyManager.GetCurrencyCount(SpecialCurrency.Venture) > 2))
            {
                await GeneralFunctions.StopBusy(dismount: false);

                if (DutyManager.InInstance || CraftingLog.IsOpen || FishingManager.State != FishingState.None || MovementManager.IsOccupied || CraftingManager.IsCrafting)
                {
                    Log.Error("Something went wrong: character is busy or in duty");
                    return;
                }

                var bell = await GoToSummoningBell();

                if (bell == false)
                {
                    Log.Error("No summoning bell near by");
                    return;
                }

                await RetainerRoutine.ReadRetainers(RetainerCheckOnlyVenture);
            }
            else
            {
                Log.Information("No Ventures Complete");
            }
        }

        public static async Task<bool> RetainerCheckOnlyVenture(RetainerInfo retainer)
        {
            if (retainer.VentureTask != 0)
            {
                var now = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                var timeLeft = retainer.VentureEndTimestamp - now;

                if (timeLeft <= 0 && SpecialCurrencyManager.GetCurrencyCount(SpecialCurrency.Venture) > 2)
                {
                    await RetainerHandleVentures();
                }
                else
                {
                    Log.Information($"Venture will be done at {RetainerInfo.UnixTimeStampToDateTime(retainer.VentureEndTimestamp)}");
                }
            }

            return true;
        }
    }
}