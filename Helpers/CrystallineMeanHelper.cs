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
using ff14bot.Navigation;
using ff14bot.NeoProfiles;
using ff14bot.Objects;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers.NPC;
using LlamaLibrary.JsonObjects.Lisbeth;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.Helpers
{
    public static class CrystallineMeanHelper
    {
        private static readonly string Name = "CrystallineMeanHelper";
        private static readonly Color LogColor = Colors.Gold;
        private static readonly LLogger Log = new LLogger(Name, LogColor);

        //Iola
        //Blacksmith, Armorer, Goldsmith
        private static readonly Npc Iola = new Npc(1027233, 819, new Vector3(7.44313f, 20.186f, -125.8074f));

        //Thiuna
        //Carpenter, Leatherworker, Weaver
        private static readonly Npc Thiuna = new Npc(1027234, 819, new Vector3(8.28885f, 20.186f, -137.4566f));

        //Bethric
        //Alchemist, Culinarian
        private static readonly Npc Bethric = new Npc(1027235, 819, new Vector3(-19.51636f, 20.186f, -130.1748f));

        //Qeshi-Rae
        //Miner, Botanist
        private static readonly Npc QeshiRae = new Npc(1027236, 819, new Vector3(-3.158691f, 20.186f, -148.9128f));

        //Frithrik
        //Fisher
        private static readonly Npc Frithrik = new Npc(1027237, 819, new Vector3(-8.957031f, 20.186f, -119.1882f));

        public static readonly Dictionary<ClassJobType, Npc> facetNPCs = facetNPCs = new Dictionary<ClassJobType, Npc>()
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

        private static Npc ClassNpc => facetNPCs.ContainsKey(Core.Me.CurrentJob) ? facetNPCs[Core.Me.CurrentJob] : default;

        public static async Task<bool> CraftItems()
        {
            var npc = ClassNpc;

            if (npc == default)
            {
                Log.Error($"Must be one of these classes {string.Join(",", facetNPCs.Keys)}");
                return false;
            }

            // Need to travel to NPC based on Current class
            var facetNpc = await GetToNpc(npc);

            // Interact with NPCID based on the class from the above chart
            facetNpc.Target();
            facetNpc.Interact();
            await Buddy.Coroutines.Coroutine.Wait(10000, () => LlamaLibrary.RemoteWindows.HugeCraftworksSupply.Instance.IsOpen || Talk.DialogOpen || QuestLogManager.InCutscene);

            if (QuestLogManager.InCutscene)
            {
                await SkipCutscene();

                facetNpc.Interact();
                await Coroutine.Sleep(1000);
            }

            if (Talk.DialogOpen)
            {
                while (Talk.DialogOpen)
                {
                    await LlamaLibrary.Helpers.GeneralFunctions.SmallTalk();
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

            // Interact with NPCID based on the class from the above chart
            facetNpc.Interact();

            if (!await Buddy.Coroutines.Coroutine.Wait(10000, () => LlamaLibrary.RemoteWindows.HugeCraftworksSupply.Instance.IsOpen || Talk.DialogOpen || QuestLogManager.InCutscene))
            {
                Log.Error("Something went wrong waiting on window or dialogs");
                return false;
            }

            if (QuestLogManager.InCutscene)
            {
                await SkipCutscene();
            }

            if (Talk.DialogOpen)
            {
                while (Talk.DialogOpen)
                {
                    await LlamaLibrary.Helpers.GeneralFunctions.SmallTalk();
                }
            }

            if (LlamaLibrary.RemoteWindows.HugeCraftworksSupply.Instance.IsOpen)
            {
                await LlamaLibrary.RemoteWindows.HugeCraftworksSupply.Instance.HandOverItems();
            }

            while (!LlamaLibrary.RemoteWindows.HugeCraftworksSupplyResul.Instance.IsOpen)
            {
                if (QuestLogManager.InCutscene)
                {
                    await SkipCutscene();
                }

                if (Talk.DialogOpen)
                {
                    while (Talk.DialogOpen)
                    {
                        await LlamaLibrary.Helpers.GeneralFunctions.SmallTalk();
                    }
                }

                await Coroutine.Sleep(200);
            }

            await Coroutine.Wait(-1, () => LlamaLibrary.RemoteWindows.HugeCraftworksSupplyResul.Instance.IsOpen);

            if (LlamaLibrary.RemoteWindows.HugeCraftworksSupplyResul.Instance.IsOpen)
            {
                LlamaLibrary.RemoteWindows.HugeCraftworksSupplyResul.Instance.Close();
            }

            if (QuestLogManager.InCutscene)
            {
                await SkipCutscene();
            }

            if (Talk.DialogOpen)
            {
                while (Talk.DialogOpen)
                {
                    await LlamaLibrary.Helpers.GeneralFunctions.SmallTalk();
                }
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
            int qty = 0;

            LlamaLibrary.RemoteWindows.HugeCraftworksSupply.Instance.Close();

            if (Core.Me.CurrentJob == ClassJobType.Alchemist || Core.Me.CurrentJob == ClassJobType.Culinarian)
            {
                qty = 18;
            }
            else
            {
                qty = 6;
            }

            var order = new LlamaLibrary.Structs.LisbethOrder(1, 1, requestedID, qty - ConditionParser.HqItemCount((uint)requestedID), Core.Me.CurrentJob.ToString(), true);
            outList.Add(order);
            Log.Information($"Sending order to Lisbeth");
            return await LlamaLibrary.Helpers.Lisbeth.ExecuteOrders(Newtonsoft.Json.JsonConvert.SerializeObject(outList, Newtonsoft.Json.Formatting.None));
        }

        public static async Task<bool> SkipCutscene()
        {
            if (QuestLogManager.InCutscene)
            {
                TreeRoot.StatusText = "InCutscene";
                if (ff14bot.RemoteAgents.AgentCutScene.Instance != null)
                {
                    ff14bot.RemoteAgents.AgentCutScene.Instance.PromptSkip();
                    await Coroutine.Wait(2000, () => SelectString.IsOpen || SelectYesno.IsOpen);
                    if (SelectString.IsOpen)
                    {
                        SelectString.ClickSlot(0);
                    }

                    if (SelectYesno.IsOpen)
                    {
                        SelectYesno.Yes();
                    }
                }
            }

            return true;
        }

        private static async Task<GameObject> GetToNpc(Npc npc)
        {
            var facetNpc = npc.GameObject;

            if (facetNpc == default)
            {
                await Navigation.GetTo(npc.Location);
                facetNpc = npc.GameObject;
            }

            if (!facetNpc.IsWithinInteractRange)
            {
                await Navigation.FlightorMove(facetNpc.Location, 4);
            }

            return facetNpc;
        }
    }
}