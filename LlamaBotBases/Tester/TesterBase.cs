using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Pathing.Service_Navigation;
using ff14bot.RemoteWindows;
using LlamaBotBases.LlamaUtilities.Settings;
using LlamaLibrary;
using LlamaLibrary.Extensions;
using LlamaLibrary.Helpers;
using LlamaLibrary.JsonObjects;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Retainers;
using LlamaLibrary.Structs;
using LlamaLibrary.Utilities;
using Newtonsoft.Json;
using TreeSharp;
using static ff14bot.RemoteWindows.Talk;

namespace LlamaBotBases.Tester
{
    public class TesterBase : BotBase
    {
        private static readonly LLogger Log = new LLogger("Tester", Colors.Pink);

        private readonly SortedDictionary<string, List<string>> luaFunctions = new SortedDictionary<string, List<string>>();

        private Composite _root;

        public Dictionary<string, List<Composite>> hooks;

        private static readonly InventoryBagId[] RetainerBagIds =
        {
            InventoryBagId.Retainer_Page1, InventoryBagId.Retainer_Page2, InventoryBagId.Retainer_Page3,
            InventoryBagId.Retainer_Page4, InventoryBagId.Retainer_Page5, InventoryBagId.Retainer_Page6,
            InventoryBagId.Retainer_Page7
        };

        private static readonly InventoryBagId[] SaddlebagIds =
        {
            (InventoryBagId)0xFA0, (InventoryBagId)0xFA1//, (InventoryBagId) 0x1004,(InventoryBagId) 0x1005
        };

        public TesterBase()
        {
            Task.Factory.StartNew(() =>
            {
                Init();
                Log.Information("INIT DONE");
            });
        }

        public override string Name => "Tester";
        public override PulseFlags PulseFlags => PulseFlags.All;

        public override bool IsAutonomous => true;
        public override bool RequiresProfile => false;

        public override Composite Root => _root;

        public override bool WantButton { get; } = true;

        private static Random _rand = new Random();

        public override void OnButtonPress()
        {
            /*
             DumpLuaFunctions();
            StringBuilder sb1 = new StringBuilder();
            foreach (var obj in luaFunctions.Keys.Where(obj => luaFunctions[obj].Count >= 1))
            {
                sb1.AppendLine(obj);
                foreach (var funcName in luaFunctions[obj])
                {
                    sb1.AppendLine($"\t{funcName}");
                }
            }

            Log.Information($"\n {sb1}");
            */
            DumpOffsets();
            DumpLLOffsets();
        }

        internal void Init()
        {
            OffsetManager.Init();

        }

        private static T LoadResource<T>(string text)
        {
            return JsonConvert.DeserializeObject<T>(text);
        }

        public override void Start()
        {
            _root = new ActionRunCoroutine(r => Run());
        }

        public override void Stop()
        {
            _root = null;
        }

        //TODO Move this somewhere...oh in the two functions below it
        public static async Task InteractWithDenys(int selectString)
        {
            var npc = GameObjectManager.GetObjectByNPCId(1032900);
            if (npc == null)
            {
                await Navigation.GetTo(418, new Vector3(-160.28f, 17.00897f, -55.8437f));
                npc = GameObjectManager.GetObjectByNPCId(1032900);
            }

            if (npc != null && !npc.IsWithinInteractRange)
            {
                await Navigation.GetTo(418, new Vector3(-160.28f, 17.00897f, -55.8437f));
            }

            if (npc != null && npc.IsWithinInteractRange)
            {
                npc.Interact();
                await Coroutine.Wait(10000, () => Conversation.IsOpen);
                if (Conversation.IsOpen)
                {
                    Conversation.SelectLine((uint)selectString);
                }
            }
        }

        //TODO Move this somewhere...it's used in angles skysteel profile. There is one skysteel helper or something already
        public static async Task TurninSkySteelGathering()
        {
            var GatheringItems = new Dictionary<uint, (uint Reward, uint Cost)>
            {
                { 31125, (30331, 10) },
                { 31130, (30333, 10) },
                { 31127, (30335, 10) },
                { 31132, (30337, 10) },
                { 31129, (30339, 10) },
                { 31134, (30340, 10) }
            };

            var turninItems = InventoryManager.FilledSlots.Where(i => i.IsHighQuality && GatheringItems.Keys.Contains(i.RawItemId));

            if (turninItems.Any())
            {
                await InteractWithDenys(3);
                await Coroutine.Wait(10000, () => ShopExchangeItem.Instance.IsOpen);
                if (ShopExchangeItem.Instance.IsOpen)
                {
                    Log.Verbose($"Window Open");
                    foreach (var turnin in turninItems)
                    {
                        var reward = GatheringItems[turnin.RawItemId].Reward;
                        var amt = turnin.Count / GatheringItems[turnin.RawItemId].Cost;
                        Log.Information($"Buying {amt}x{DataManager.GetItem(reward).CurrentLocaleName}");
                        await ShopExchangeItem.Instance.Purchase(reward, amt);
                        await Coroutine.Sleep(500);
                    }

                    ShopExchangeItem.Instance.Close();
                    await Coroutine.Wait(10000, () => !ShopExchangeItem.Instance.IsOpen);
                }
            }
        }

        //TODO Move this somewhere...it's used in angles skysteel profile. There is one skysteel helper or something already
        public static async Task TurninSkySteelCrafting()
        {
            var TurnItemList = new Dictionary<uint, CraftingRelicTurnin>
            {
                { 31101, new CraftingRelicTurnin(31101, 0, 1, 2000, 30315) },
                { 31109, new CraftingRelicTurnin(31109, 0, 0, 3000, 30316) },
                { 31102, new CraftingRelicTurnin(31102, 1, 1, 2000, 30317) },
                { 31110, new CraftingRelicTurnin(31110, 1, 0, 3000, 30318) },
                { 31103, new CraftingRelicTurnin(31103, 2, 1, 2000, 30319) },
                { 31111, new CraftingRelicTurnin(31111, 2, 0, 3000, 30320) },
                { 31104, new CraftingRelicTurnin(31104, 3, 1, 2000, 30321) },
                { 31112, new CraftingRelicTurnin(31112, 3, 0, 3000, 30322) },
                { 31105, new CraftingRelicTurnin(31105, 4, 1, 2000, 30323) },
                { 31113, new CraftingRelicTurnin(31113, 4, 0, 3000, 30324) },
                { 31106, new CraftingRelicTurnin(31106, 5, 1, 2000, 30325) },
                { 31114, new CraftingRelicTurnin(31114, 5, 0, 3000, 30326) },
                { 31107, new CraftingRelicTurnin(31107, 6, 1, 2000, 30327) },
                { 31115, new CraftingRelicTurnin(31115, 6, 0, 3000, 30328) },
                { 31108, new CraftingRelicTurnin(31108, 7, 1, 2000, 30329) },
                { 31116, new CraftingRelicTurnin(31116, 7, 0, 3000, 30330) }
            };

            var collectables = InventoryManager.FilledSlots.Where(i => i.IsCollectable).Select(x => x.RawItemId).Distinct();
            var collectablesAll = InventoryManager.FilledSlots.Where(i => i.IsCollectable);

            if (collectables.Any(i => TurnItemList.Keys.Contains(i)))
            {
                Log.Information("Have collectables");
                foreach (var collectable in collectablesAll)
                {
                    if (TurnItemList.Keys.Contains(collectable.RawItemId))
                    {
                        var turnin = TurnItemList[collectable.RawItemId];
                        if (collectable.Collectability < turnin.MinCollectability)
                        {
                            Log.Information($"Discarding {collectable.Name} is at {collectable.Collectability} which is under {turnin.MinCollectability}");
                            collectable.Discard();
                        }
                    }
                }

                collectables = InventoryManager.FilledSlots.Where(i => i.IsCollectable).Select(x => x.RawItemId).Distinct();

                await InteractWithDenys(2);
                await Coroutine.Wait(10000, () => CollectablesShop.Instance.IsOpen);

                if (CollectablesShop.Instance.IsOpen)
                {
                    Log.Verbose("Window open");
                    foreach (var item in collectables)
                    {
                        Log.Information($"Turning in {DataManager.GetItem(item).CurrentLocaleName}");
                        var turnin = TurnItemList[item];

                        Log.Verbose($"Pressing job {turnin.Job}");
                        CollectablesShop.Instance.SelectJob(turnin.Job);
                        await Coroutine.Sleep(500);

                        Log.Verbose($"Pressing position {turnin.Position}");
                        CollectablesShop.Instance.SelectItem(turnin.Position);
                        await Coroutine.Sleep(1000);
                        var i = 0;
                        while (CollectablesShop.Instance.TurninCount > 0)
                        {
                            Log.Verbose($"Pressing trade {i}");
                            i++;
                            CollectablesShop.Instance.Trade();
                            await Coroutine.Sleep(100);
                        }
                    }

                    CollectablesShop.Instance.Close();
                    await Coroutine.Wait(10000, () => !CollectablesShop.Instance.IsOpen);
                }
            }
        }

        private Task<bool> Run()
        {
            Log.Information($"HomeWorldId: {WorldHelper.HomeWorldId}, CurrentWorldId: {WorldHelper.CurrentWorldId}, DataCenterId: {WorldHelper.DataCenterId}");

            Navigator.PlayerMover = new SlideMover();
            Navigator.NavigationProvider = new ServiceNavigationProvider();
            var DeliveryNpcs = new List<CustomDeliveryNpc>()
            {
                new CustomDeliveryNpc( 1019615,478, new Vector3(-71.68203f, 206.5714f, 29.38501f), "Zhloe Aliapoh", 67087, 1), //(Zhloe Aliapoh) Idyllshire(Dravania)
                new CustomDeliveryNpc( 1020337,635, new Vector3(171.312988f, 13.02367f, -89.951965f), "M'naago", 68541, 2), //(M'naago) Rhalgr's Reach(Gyr Abania)
                new CustomDeliveryNpc( 1025878,613, new Vector3(343.984009f, -120.329468f, -306.019714f), "Kurenai", 68675, 3), //(Kurenai) The Ruby Sea(Othard)
                new CustomDeliveryNpc( 1018393,478, new Vector3(-62.3016f, 206.6002f, 23.893f), "Adkiragh", 68713, 4), //(Adkiragh) Idyllshire(Dravania)
                new CustomDeliveryNpc( 1031801,820, new Vector3(52.811401f, 82.993774f, -65.384949f), "Kai-Shirr", 69265, 5), //(Kai-Shirr) Eulmore(Eulmore)
                new CustomDeliveryNpc( 1033543,886, new Vector3(113.389771f, -20.004639f, -0.961365f), "Ehll Tou", 69425, 6), //(Ehll Tou) The Firmament(Ishgard)
                new CustomDeliveryNpc( 1035211,886, new Vector3(-115.1127f, 0f, -134.8367f), "Charlemend", 69615, 7)
            };

            using (var outputFile = new StreamWriter(@"G:\CustomDeliveryNpcs.json", false))
            {
                outputFile.Write(JsonConvert.SerializeObject(DeliveryNpcs, (Formatting)System.Xml.Formatting.Indented));
            }

            TreeRoot.Stop("Stop Requested");

            return Task.FromResult(true);
        }

        private void LogPtr(IntPtr instancePointer)
        {
            Log.Information(instancePointer.ToString("X"));
        }

        private Task TestHook()
        {
            Log.Information("LL hook");
            return Task.CompletedTask;
        }

        private void DumpLLOffsets()
        {
            var sb = new StringBuilder();
            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder();
            foreach (var patternItem in OffsetManager.patterns.OrderBy(k => k.Key))
            {
                var name = patternItem.Key;
                var pattern = patternItem.Value.Replace("Search ", "");

                if (name.ToLowerInvariant().Contains("vtable") && name.ToLowerInvariant().Contains("agent"))
                {
                    Log.Information($"Agent_{name}, {pattern}");
                    sb1.AppendLine($"{name.Replace("Vtable", "").Replace("vtable", "").Replace("VTable", "").Replace("_", "")}, {pattern}");
                }
                else if (!name.ToLowerInvariant().Contains("exd"))
                {
                    Log.Information($"{name}, {pattern}");
                    sb.AppendLine($"{name}, {pattern}");
                }
            }

            foreach (var patternItem in OffsetManager.constants)
            {
                var name = patternItem.Key;
                var pattern = patternItem.Value.Replace("Search ", "");
                sb2.AppendLine($"{name}, {pattern}");
            }

            using (var outputFile = new StreamWriter(@"G:\AgentLL.csv", false))
            {
                outputFile.Write(sb1.ToString());
            }

            using (var outputFile = new StreamWriter(@"G:\LL.csv", false))
            {
                outputFile.Write(sb.ToString());
            }

            using (var outputFile = new StreamWriter(@"G:\Constants.csv", false))
            {
                outputFile.Write(sb2.ToString());
            }

            sb = new StringBuilder();
            var i = 0;
            foreach (var vtable in AgentModule.AgentVtables)
            {
                sb.AppendLine($"Model_{i},{Core.Memory.GetRelative(vtable).ToString("X")}");
                i++;
            }

            using (var outputFile = new StreamWriter(@"G:\AgentOffsets.csv", false))
            {
                outputFile.Write(sb.ToString());
            }
        }

        private async Task BuyHouse()
        {
            var _rnd = new Random();

            var placard = GameObjectManager.GetObjectsByNPCId(2002736).OrderBy(i => i.Distance()).FirstOrDefault();
            if (placard != null)
            {
                do
                {
                    if (!HousingSignBoard.Instance.IsOpen)
                    {
                        placard.Interact();
                        await Coroutine.Wait(3000, () => HousingSignBoard.Instance.IsOpen);
                    }

                    if (HousingSignBoard.Instance.IsOpen)
                    {
                        if (HousingSignBoard.Instance.IsForSale)
                        {
                            await Coroutine.Sleep(_rnd.Next(200, 400));
                            HousingSignBoard.Instance.ClickBuy();
                            await Coroutine.Wait(3000, () => Conversation.IsOpen);
                            if (Conversation.IsOpen)
                            {
                                await Coroutine.Sleep(_rnd.Next(50, 300));
                                Conversation.SelectLine(0);
                                await Coroutine.Wait(3000, () => SelectYesno.IsOpen);
                                SelectYesno.Yes();
                                await Coroutine.Sleep(_rnd.Next(23, 600));
                            }
                        }
                    }

                    await Coroutine.Sleep(_rnd.Next(1500, 3000));
                    placard.Interact();
                    await Coroutine.Wait(3000, () => HousingSignBoard.Instance.IsOpen);
                }
                while (HousingSignBoard.Instance.IsForSale);

                await Coroutine.Wait(3000, () => HousingSignBoard.Instance.IsOpen);
                HousingSignBoard.Instance.Close();
                await Coroutine.Wait(3000, () => !HousingSignBoard.Instance.IsOpen);
                Lua.DoString("return _G['EventHandler']:Shutdown();");
            }
        }

        private void DumpOffsets()
        {
            var off = typeof(Core).GetProperty("Offsets", BindingFlags.NonPublic | BindingFlags.Static);
            var stringBuilder = new StringBuilder();
            var i = 0;
            var p1 = 0;
            var p2 = 0;
            foreach (var p in off.PropertyType.GetFields())
            {
                var tp = p.GetValue(off.GetValue(null));
                p1 = 0;
                p2 = 0;
                foreach (var t in p.FieldType.GetFields())
                {
                    //stringBuilder.Append(string.Format("\nField: {0} \t", p2));

                    if (t.FieldType == typeof(IntPtr))
                    {
                        //IntPtr ptr = new IntPtr(((IntPtr) t.GetValue(tp)).ToInt64() - Core.Memory.ImageBase.ToInt64());
                        var ptr = (IntPtr)t.GetValue(tp);
                        stringBuilder.Append($"Struct{i + 88}_IntPtr{p1}, {Core.Memory.GetRelative(ptr).ToInt64()}\n");

                        //stringBuilder.Append(string.Format("\tPtr Offset_{0}: 0x{1:x}", p1, ptr.ToInt64()));

                        p1++;
                    }

                    p2++;
                }

                //stringBuilder.Append("\n");
                i++;
            }

            using (var outputFile = new StreamWriter($"RB{Assembly.GetEntryAssembly().GetName().Version.Build}.csv", false))
            {
                outputFile.Write(stringBuilder.ToString());
            }
        }

        private void DumpLuaFunctions()
        {
            var func = "local values = {} for key,value in pairs(_G) do table.insert(values, key); end return unpack(values);";

            var retValues = Lua.GetReturnValues(func);
            foreach (var ret in retValues.Where(ret => !ret.StartsWith("_") && !ret.StartsWith("Luc") && !ret.StartsWith("Stm") && !char.IsDigit(ret[ret.Length - 1]) && !char.IsLower(ret[0])))
            {
                if (ret.Contains(":"))
                {
                    var name = ret.Split(':')[0];
                    if (luaFunctions.ContainsKey(name))
                    {
                        continue;
                    }

                    luaFunctions.Add(name, GetSubFunctions(name));
                }
                else
                {
                    if (luaFunctions.ContainsKey(ret))
                    {
                        continue;
                    }

                    luaFunctions.Add(ret, GetSubFunctions(ret));
                }
            }
        }

        private static List<string> GetSubFunctions(string luaObject)
        {
            var func = $"local values = {{}} for key,value in pairs(_G['{luaObject}']) do table.insert(values, key); end return unpack(values);";
            var functions = new List<string>();
            try
            {
                var retValues = Lua.GetReturnValues(func);
                functions.AddRange(retValues.Where(ret => !ret.Contains("_") && !ret.Contains("OnSequence") && !ret.StartsWith("On") && !ret.Contains("className") && !ret.Contains("referenceCount") && !ret.Contains("ACTOR")));
            }
            catch
            {
            }

            functions.Sort();
            return functions;
        }

        public async Task<bool> RetainerCheck(RetainerInfo retainer)
        {
            if (retainer.Job != ClassJobType.Adventurer)
            {
                if (retainer.VentureTask != 0)
                {
                    var now = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    var timeLeft = retainer.VentureEndTimestamp - now;

                    if (timeLeft <= 0 && SpecialCurrencyManager.GetCurrencyCount(SpecialCurrency.Venture) > 2)
                    {
                        await CheckVentures();
                    }
                    else
                    {
                        Log.Information($"Venture will be done at {RetainerInfo.UnixTimeStampToDateTime(retainer.VentureEndTimestamp)}");
                    }
                }
            }

            if (RetainerSettings.Instance.DepositFromPlayer)
            {
                await RetainerRoutine.DumpItems();
            }

            Log.Information("Done checking against player inventory");

            //Log.Debug($"{RetainerInfo.UnixTimeStampToDateTime(retainer.VentureEndTimestamp)}");

            return true;
        }

        public async Task<bool> CheckVentures()
        {
            if (!SelectString.IsOpen)
            {
                return false;
            }

            if (SelectString.Lines().Contains(Translator.VentureCompleteText))
            {
                Log.Information("Venture Done");
                SelectString.ClickLineEquals(Translator.VentureCompleteText);

                await Coroutine.Wait(5000, () => RetainerTaskResult.IsOpen);

                if (!RetainerTaskResult.IsOpen)
                {
                    Log.Error("RetainerTaskResult didn't open");
                    return false;
                }

                var taskId = AgentRetainerVenture.Instance.RetainerTask;

                var task = ResourceManager.VentureData.Value.First(i => i.Id == taskId);

                Log.Information($"Finished Venture {task.Name}");
                Log.Information($"Reassigning Venture {task.Name}");

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

                await Coroutine.Wait(1500, () => DialogOpen);
                await Coroutine.Sleep(200);
                if (DialogOpen)
                {
                    Next();
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
    }
}