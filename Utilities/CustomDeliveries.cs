using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.NeoProfiles;
using ff14bot.Pathing.Service_Navigation;
using ff14bot.RemoteAgents;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers;
using LlamaLibrary.JsonObjects;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Structs;
using Newtonsoft.Json;

namespace LlamaLibrary.Utilities
{
    public static class CustomDeliveries
    {
        public static string NameStatic => "Custom Deliveries";
        private static readonly LLogger Log = new LLogger(NameStatic, Colors.Gold);

        public static async Task<bool> RunCustomDeliveries(DohClasses dohClass = DohClasses.Carpenter)
        {
            Navigator.PlayerMover = new SlideMover();
            Navigator.NavigationProvider = new ServiceNavigationProvider();
            var DeliveryNpcs = ResourceManager.CustomDeliveryNpcs.Value;

            foreach (var npc in DeliveryNpcs.Where(i => ConditionParser.IsQuestCompleted(i.RequiredQuest)).OrderByDescending(i => i.Index))
            {
                await CraftThenHandinNpc(npc, dohClass);
                /*if (AgentSatisfactionSupply.Instance.DeliveriesRemaining == 0)
                {
                    Log.Information("Out of delivery allowances");
                    break;
                }*/
            }

            TreeRoot.Stop("Stop Requested");
            return true;
        }

        public static async Task CraftThenHandinNpc(CustomDeliveryNpc deliveryNpc, DohClasses dohClass = DohClasses.Carpenter, bool stopAtFiveHearts = true)
        {
            await AgentSatisfactionSupply.Instance.LoadWindow(deliveryNpc.Index);
            var items = new List<uint>();
            if (deliveryNpc.npcId != AgentSatisfactionSupply.Instance.NpcId)
            {
                Log.Information($"Bad Npc ID: {AgentSatisfactionSupply.Instance.NpcId}");
                return;
            }

            Log.Information($"{deliveryNpc.Name}");
            Log.Information($"\tHeartLevel:{AgentSatisfactionSupply.Instance.HeartLevel}");
            Log.Information($"\tRep:{AgentSatisfactionSupply.Instance.CurrentRep}/{AgentSatisfactionSupply.Instance.MaxRep}");
            Log.Information($"\tDeliveries Remaining:{AgentSatisfactionSupply.Instance.DeliveriesRemaining}");
            Log.Information($"\tDoH: {DataManager.GetItem(AgentSatisfactionSupply.Instance.DoHItemId)}");
            items.Add(AgentSatisfactionSupply.Instance.DoHItemId);
            Log.Information($"\tDoL: {DataManager.GetItem(AgentSatisfactionSupply.Instance.DoLItemId)}");
            items.Add(AgentSatisfactionSupply.Instance.DoLItemId);
            Log.Information($"\tFsh: {DataManager.GetItem(AgentSatisfactionSupply.Instance.FshItemId)}");
            items.Add(AgentSatisfactionSupply.Instance.FshItemId);

            if ((stopAtFiveHearts && AgentSatisfactionSupply.Instance.HeartLevel == 5) || AgentSatisfactionSupply.Instance.DeliveriesRemaining == 0)
            {
                Log.Information($"{deliveryNpc.Name} Satisfaction Level is Maxed or out of deliveries, skipping");
                return;
            }

            var outList = new List<LisbethOrder>();

            // if (deliveryNpc.npcId == 1025878)
            //  {
            //      outList.Add(new LisbethOrder(0, 1, (int)AgentSatisfactionSupply.Instance.DoLItemId, Math.Min(3, (int)AgentSatisfactionSupply.Instance.DeliveriesRemaining), "Gather", true));
            //  }
            //  else
            //  {
            outList.Add(new LisbethOrder(0, 1, (int)AgentSatisfactionSupply.Instance.DoHItemId, Math.Min(3, (int)AgentSatisfactionSupply.Instance.DeliveriesRemaining), dohClass.ToString(), true));
            //  }

            var order = JsonConvert.SerializeObject(outList, Formatting.None).Replace("Hq", "Collectable");

            if (!InventoryManager.FilledSlots.Any(i => items.Contains(i.RawItemId)))
            {
                if (order != "" && !InventoryManager.FilledSlots.Any(i => items.Contains(i.RawItemId)))
                {
                    await GeneralFunctions.StopBusy();
                    Log.Information($"Calling Lisbeth with {order}");
                    try
                    {
                        await Lisbeth.ExecuteOrdersIgnoreHome(order);
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }
            }

            if (InventoryManager.FilledSlots.Any(i => items.Contains(i.RawItemId)) && AgentSatisfactionSupply.Instance.DeliveriesRemaining > 0)
            {
                Log.Information("Have items to turn in");
                await HandInCustomNpc(deliveryNpc);
            }
        }

        public static async Task<bool> HandInCustomNpc(CustomDeliveryNpc deliveryNpc)
        {
            var npc = GameObjectManager.GetObjectByNPCId(deliveryNpc.npcId);

            if (npc == default || !npc.IsWithinInteractRange)
            {
                await Navigation.GetTo(deliveryNpc.Zone, deliveryNpc.Location);
                npc = GameObjectManager.GetObjectByNPCId(deliveryNpc.npcId);
            }

            if (npc == default)
            {
                return false;
            }

            npc.Interact();

            await Coroutine.Wait(10000, () => Talk.DialogOpen);

            if (!Talk.DialogOpen)
            {
                npc.Interact();

                await Coroutine.Wait(10000, () => Talk.DialogOpen);
            }

            while (Talk.DialogOpen)
            {
                Talk.Next();
                await Coroutine.Sleep(200);
                await Coroutine.Yield();
            }

            await Coroutine.Wait(10000, () => Conversation.IsOpen);
            await Coroutine.Sleep(500);

            Log.Information("Choosing 'Make a delivery.'");
            Conversation.SelectLine(0);
            await Coroutine.Wait(1000, () => Talk.DialogOpen);

            if (Talk.DialogOpen)
            {
                while (Talk.DialogOpen)
                {
                    Talk.Next();
                    await Coroutine.Sleep(200);
                    await Coroutine.Yield();
                }
            }

            await Coroutine.Wait(10000, () => SatisfactionSupply.Instance.IsOpen);

            if (SatisfactionSupply.Instance.IsOpen)
            {
                do
                {
                    Log.Information("Turning in items");

                    if (AgentSatisfactionSupply.Instance.DeliveriesRemaining < 1)
                    {
                        break;
                    }

                    if (AgentSatisfactionSupply.Instance.HasDoHTurnin)
                    {
                        SatisfactionSupply.Instance.ClickItem(0);
                    }
                    else if (AgentSatisfactionSupply.Instance.HasDoLTurnin)
                    {
                        SatisfactionSupply.Instance.ClickItem(1);
                    }
                    else if (AgentSatisfactionSupply.Instance.HasFshTurnin)
                    {
                        SatisfactionSupply.Instance.ClickItem(2);
                    }

                    await Coroutine.Wait(10000, () => Request.IsOpen);

                    Log.Information("Selecting items.");
                    await CommonTasks.HandOverRequestedItems();
                    await Coroutine.Sleep(500);

                    if (SelectYesno.IsOpen)
                    {
                        SelectYesno.Yes();
                        await Coroutine.Wait(5000, () => !SelectYesno.IsOpen);
                    }

                    while (!SatisfactionSupply.Instance.IsOpen && !QuestLogManager.InCutscene)
                    {
                        if (Talk.DialogOpen)
                        {
                            Talk.Next();
                            await Coroutine.Sleep(200);
                        }

                        await Coroutine.Sleep(500);
                    }

                    if (QuestLogManager.InCutscene)
                    {
                        while (!SatisfactionSupplyResult.Instance.IsOpen && QuestLogManager.InCutscene)
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

                            if (Talk.DialogOpen)
                            {
                                Talk.Next();
                                await Coroutine.Sleep(200);
                            }

                            await Coroutine.Sleep(500);
                        }

                        if (SatisfactionSupplyResult.Instance.IsOpen)
                        {
                            Log.Debug("Clicking Accept.");
                            SatisfactionSupplyResult.Instance.Confirm();
                        }

                        await Coroutine.Wait(5000, () => Talk.DialogOpen);
                        while (Talk.DialogOpen)
                        {
                            Talk.Next();
                            await Coroutine.Wait(200, () => !Talk.DialogOpen);
                            await Coroutine.Wait(500, () => Talk.DialogOpen);
                            await Coroutine.Sleep(200);
                            await Coroutine.Yield();
                        }

                        await Coroutine.Sleep(500);
                        await Coroutine.Wait(5000, () => Talk.DialogOpen);
                        while (Talk.DialogOpen)
                        {
                            Talk.Next();
                            await Coroutine.Wait(200, () => !Talk.DialogOpen);
                            await Coroutine.Wait(500, () => Talk.DialogOpen);
                            await Coroutine.Sleep(200);
                            await Coroutine.Yield();
                        }

                        await Coroutine.Sleep(500);
                        await Coroutine.Wait(5000, () => Talk.DialogOpen);
                        while (Talk.DialogOpen)
                        {
                            Talk.Next();
                            await Coroutine.Wait(200, () => !Talk.DialogOpen);
                            await Coroutine.Wait(500, () => Talk.DialogOpen);
                            await Coroutine.Sleep(200);
                            await Coroutine.Yield();
                        }

                        break;
                    }

                    await Coroutine.Wait(5000, () => SatisfactionSupply.Instance.IsOpen);
                    if (!SatisfactionSupply.Instance.IsOpen)
                    {
                        break;
                    }
                }
                while (AgentSatisfactionSupply.Instance.DeliveriesRemaining > 0 && AgentSatisfactionSupply.Instance.HasAnyTurnin);
            }

            if (SatisfactionSupply.Instance.IsOpen)
            {
                SatisfactionSupply.Instance.Close();
                await Coroutine.Wait(10000, () => !SatisfactionSupply.Instance.IsOpen);
            }

            await Coroutine.Wait(1000, () => Conversation.IsOpen);
            if (Conversation.IsOpen)
            {
                Conversation.SelectLine((uint)(Conversation.GetConversationList.Count - 1));
            }

            return true;
        }
    }
}