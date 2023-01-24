using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.NeoProfiles;
using ff14bot.Objects;
using ff14bot.RemoteAgents;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers.NPC;
using LlamaLibrary.Logging;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.Helpers
{
    public static class CrystallineMeanHelper
    {
        private static readonly LLogger Log = new(nameof(CrystallineMeanHelper), Colors.Gold);

        //Iola
        //Blacksmith, Armorer, Goldsmith
        private static readonly Npc Iola = new(1027233, 819, new Vector3(7.44313f, 20.186f, -125.8074f));

        //Thiuna
        //Carpenter, Leatherworker, Weaver
        private static readonly Npc Thiuna = new(1027234, 819, new Vector3(8.28885f, 20.186f, -137.4566f));

        //Bethric
        //Alchemist, Culinarian
        private static readonly Npc Bethric = new(1027235, 819, new Vector3(-19.51636f, 20.186f, -130.1748f));

        //Qeshi-Rae
        //Miner, Botanist
        private static readonly Npc QeshiRae = new(1027236, 819, new Vector3(-3.158691f, 20.186f, -148.9128f));

        //Frithrik
        //Fisher
        private static readonly Npc Frithrik = new(1027237, 819, new Vector3(-8.957031f, 20.186f, -119.1882f));

        public static readonly Dictionary<ClassJobType, Npc> FacetNPCs = FacetNPCs = new Dictionary<ClassJobType, Npc>()
        {
            { ClassJobType.Blacksmith, Iola },
            { ClassJobType.Armorer, Iola },
            { ClassJobType.Goldsmith, Iola },
            { ClassJobType.Carpenter, Thiuna },
            { ClassJobType.Leatherworker, Thiuna },
            { ClassJobType.Weaver, Thiuna },
            { ClassJobType.Alchemist, Bethric },
            { ClassJobType.Culinarian, Bethric },
            { ClassJobType.Miner, QeshiRae },
            { ClassJobType.Botanist, QeshiRae },
            { ClassJobType.Fisher, Frithrik },
        };

        private static Npc? ClassNpc => FacetNPCs.ContainsKey(Core.Me.CurrentJob) ? FacetNPCs[Core.Me.CurrentJob] : default;

        public static async Task<bool> CraftItems()
        {
            var npc = ClassNpc;

            if (npc == default)
            {
                Log.Error($"Must be one of these classes {string.Join(",", FacetNPCs.Keys)}");
                return false;
            }

            // Need to travel to NPC based on Current class
            var facetNpc = await GetToNpc(npc);

            if (facetNpc == default)
            {
                Log.Error($"Failed to get to {npc.Name}");
                return false;
            }

            // Interact with NPCID based on the class from the above chart
            facetNpc.Target();
            facetNpc.Interact();
            await Buddy.Coroutines.Coroutine.Wait(10000, () => LlamaLibrary.RemoteWindows.HugeCraftworksSupply.Instance.IsOpen || Talk.DialogOpen || QuestLogManager.InCutscene);

            if (QuestLogManager.InCutscene)
            {
                if (Talk.DialogOpen)
                {
                    Log.Information("Dealing with Talk Cutscene");
                    while (Talk.DialogOpen)
                    {
                        Talk.Next();
                        await Coroutine.Wait(200, () => !Talk.DialogOpen);
                        await Coroutine.Wait(500, () => Talk.DialogOpen);
                        await Coroutine.Sleep(200);
                        await Coroutine.Yield();
                    }
                }

                await SkipCutscene();

                if (Talk.DialogOpen)
                {
                    Log.Information("Dealing with Talk Cutscene #2");
                    while (Talk.DialogOpen)
                    {
                        Talk.Next();
                        await Coroutine.Wait(200, () => !Talk.DialogOpen);
                        await Coroutine.Wait(500, () => Talk.DialogOpen);
                        await Coroutine.Sleep(200);
                        await Coroutine.Yield();
                    }
                }

                facetNpc.Interact();
                await Coroutine.Sleep(1000);
            }

            if (Talk.DialogOpen)
            {
                Log.Information("Dealing with Talk #0");
                while (Talk.DialogOpen)
                {
                    Talk.Next();
                    await Coroutine.Wait(200, () => !Talk.DialogOpen);
                    await Coroutine.Wait(500, () => Talk.DialogOpen);
                    await Coroutine.Sleep(200);
                    await Coroutine.Yield();
                }

                facetNpc.Interact();
                await Coroutine.Sleep(1000);
            }

            if (LlamaLibrary.RemoteWindows.HugeCraftworksSupply.Instance.IsOpen)
            {
                Log.Information($"Generating Lisbeth order");
                if (!await GenerateLisbethOrder())
                {
                    Log.Error("Something went wrong calling lisbeth");
                    return false;
                }
            }

            // Need to travel to NPC based on Current class

            facetNpc = await GetToNpc(npc);

            if (facetNpc == default)
            {
                Log.Error("Failed to get to NPC");
                return false;
            }

            // Interact with NPCID based on the class from the above chart
            facetNpc.Interact();

            if (!await Buddy.Coroutines.Coroutine.Wait(10000, () => LlamaLibrary.RemoteWindows.HugeCraftworksSupply.Instance.IsOpen || Talk.DialogOpen || QuestLogManager.InCutscene))
            {
                Log.Error("Something went wrong waiting on window or dialogs");
                return false;
            }

            if (Talk.DialogOpen)
            {
                Log.Information("Dealing with Talk #1");
                await DealWithTalk();
            }

            if (QuestLogManager.InCutscene)
            {
                Log.Information("Dealing with Cutscene #1");
                await SkipCutscene();
            }

            if (await Buddy.Coroutines.Coroutine.Wait(5000, () => LlamaLibrary.RemoteWindows.HugeCraftworksSupply.Instance.IsOpen) && LlamaLibrary.RemoteWindows.HugeCraftworksSupply.Instance.IsOpen)
            {
                Log.Information("Item Hand over");
                await LlamaLibrary.RemoteWindows.HugeCraftworksSupply.Instance.HandOverItems();
            }

            var timer = Stopwatch.StartNew();
            Log.Information("Before while #1");
            while (!LlamaLibrary.RemoteWindows.HugeCraftworksSupplyResul.Instance.IsOpen && timer.ElapsedMilliseconds < 10_000)
            {
                if (QuestLogManager.InCutscene)
                {
                    Log.Information("Skip cutscene window not open");
                    await SkipCutscene();
                }

                await DealWithTalk();

                await Coroutine.Sleep(200);
            }

            timer.Stop();
            Log.Information("Past while #1");

            //await Coroutine.Wait(-1, () => LlamaLibrary.RemoteWindows.HugeCraftworksSupplyResul.Instance.IsOpen || );
            //timer = Stopwatch.StartNew();
            while (!LlamaLibrary.RemoteWindows.HugeCraftworksSupplyResul.Instance.IsOpen && !QuestLogManager.InCutscene && timer.ElapsedMilliseconds < 10_000)
            {
                await DealWithTalk();
                await Coroutine.Sleep(200);
            }

            Log.Information("Past while #2");
            await DealWithTalk();

            if (QuestLogManager.InCutscene || LlamaLibrary.RemoteWindows.HugeCraftworksSupplyResul.Instance.IsOpen)
            {
                while (!LlamaLibrary.RemoteWindows.HugeCraftworksSupplyResul.Instance.IsOpen && QuestLogManager.InCutscene)
                {
                    Log.Information("Dealing with cutscene.");
                    if (QuestLogManager.InCutscene && AgentCutScene.Instance.CanSkip)
                    {
                        AgentCutScene.Instance.PromptSkip();
                        await Coroutine.Wait(5000, () => SelectString.IsOpen);
                        if (SelectString.IsOpen)
                        {
                            SelectString.ClickSlot(0);
                        }
                    }

                    await DealWithTalk();

                    await Coroutine.Sleep(500);
                }

                if (LlamaLibrary.RemoteWindows.HugeCraftworksSupplyResul.Instance.IsOpen)
                {
                    Log.Information("Closing result window");
                    LlamaLibrary.RemoteWindows.HugeCraftworksSupplyResul.Instance.Close();
                    await Coroutine.Wait(5000, () => !LlamaLibrary.RemoteWindows.HugeCraftworksSupplyResul.Instance.IsOpen && (!QuestLogManager.InCutscene || Talk.DialogOpen));

                    //await Coroutine.Sleep(1000);
                }

                Log.Information($"Waiting for talk window {MovementManager.IsOccupied} {Inventory.IsBusy}");
                await Coroutine.Wait(5000, () => Talk.DialogOpen);
                await DealWithTalk();

                Log.Information($"Waiting for talk window {MovementManager.IsOccupied} {Inventory.IsBusy}");
                await Coroutine.Wait(5000, () => Talk.DialogOpen || (!Inventory.IsBusy && !MovementManager.IsOccupied));
                await DealWithTalk();

                Log.Information($"Waiting for talk window {MovementManager.IsOccupied} {Inventory.IsBusy}");
                await Coroutine.Wait(5000, () => Talk.DialogOpen || (!Inventory.IsBusy && !MovementManager.IsOccupied));
                await DealWithTalk();
                Log.Information($"Done Waiting for talk window {MovementManager.IsOccupied} {Inventory.IsBusy}");
            }

            return true;
        }

        public static async Task<bool> GenerateLisbethOrder()
        {
            if (!LlamaLibrary.RemoteWindows.HugeCraftworksSupply.Instance.IsOpen)
            {
                return false;
            }

            var outList = new List<LlamaLibrary.Structs.LisbethOrder>();
            var requestedID = LlamaLibrary.RemoteWindows.HugeCraftworksSupply.Instance.TurnInItemId;
            var qty = 0;
            var orderType = "Class";
            var needHQ = true;

            LlamaLibrary.RemoteWindows.HugeCraftworksSupply.Instance.Close();

            if (Core.Me.CurrentJob is ClassJobType.Alchemist or ClassJobType.Culinarian or
                ClassJobType.Miner or ClassJobType.Botanist)
            {
                qty = 18;
            }
            else
            {
                qty = 6;
            }

            if (Core.Me.CurrentJob is ClassJobType.Miner or ClassJobType.Botanist)
            {
                orderType = $"Gather";
            }
            else if (Core.Me.CurrentJob == ClassJobType.Fisher)
            {
                orderType = $"Fisher";
            }
            else
            {
                orderType = $"{Core.Me.CurrentJob}";
            }

            if (Core.Me.CurrentJob is ClassJobType.Miner or ClassJobType.Botanist or ClassJobType.Fisher)
            {
                needHQ = false;
            }
            else
            {
                needHQ = true;
            }

            var order = new LlamaLibrary.Structs.LisbethOrder(1, 1, requestedID, qty - ConditionParser.HqItemCount((uint)requestedID), orderType, needHQ);
            outList.Add(order);
            Log.Information($"Sending order of {qty - ConditionParser.HqItemCount((uint)requestedID)} x {DataManager.GetItem((uint)requestedID).CurrentLocaleName} to Lisbeth");
            return await LlamaLibrary.Helpers.Lisbeth.ExecuteOrders(Newtonsoft.Json.JsonConvert.SerializeObject(outList, Newtonsoft.Json.Formatting.None));
        }

        public static async Task<bool> SkipCutscene()
        {
            Log.Information("Dealing with cutscene.");

            await DealWithTalk();

            if (QuestLogManager.InCutscene && AgentCutScene.Instance.CanSkip)
            {
                AgentCutScene.Instance.PromptSkip();
                await Coroutine.Wait(5000, () => SelectString.IsOpen);
                if (SelectString.IsOpen)
                {
                    SelectString.ClickSlot(0);
                }
            }

            return true;
        }

        public static async Task DealWithTalk()
        {
            if (Talk.DialogOpen)
            {
                while (Talk.DialogOpen)
                {
                    Talk.Next();
                    await Coroutine.Wait(200, () => !Talk.DialogOpen);
                    await Coroutine.Wait(500, () => Talk.DialogOpen);
                    await Coroutine.Sleep(200);
                    await Coroutine.Yield();
                }
            }
        }

        private static async Task<GameObject?> GetToNpc(Npc npc)
        {
            var facetNpc = npc.GameObject;

            if (facetNpc == default)
            {
                await Navigation.GetTo(npc.Location);
                facetNpc = npc.GameObject;
            }

            if (facetNpc != null && !facetNpc.IsWithinInteractRange)
            {
                await Navigation.FlightorMove(facetNpc.Location, 4);
            }

            return facetNpc;
        }
    }
}