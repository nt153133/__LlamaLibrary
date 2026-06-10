using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Enums;
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
    /// <summary>
    /// Provides automation for FFXIV Custom Deliveries, including crafting, material purchasing via <see cref="GilShopping"/>,
    /// and handing in items to various satisfaction NPCs.
    /// </summary>
    public static class CustomDeliveries
    {
        /// <summary>
        /// Gets the static display name for this utility.
        /// </summary>
        public static string NameStatic => "Custom Deliveries";

        private static readonly LLogger Log = new(NameStatic, Colors.Gold);

        /// <summary>
        /// Automatically runs custom deliveries for all NPCs whose unlock quests are completed.
        /// Iterates NPCs in descending index order and completes available deliveries.
        /// </summary>
        /// <param name="dohClass">The Disciple of the Hand class to use for crafting.</param>
        /// <returns><see langword="true"/> upon completion.</returns>
        public static async Task<bool> RunCustomDeliveries(DohClasses dohClass = DohClasses.Carpenter)
        {
            if (Navigator.NavigationProvider == null)
            {
                Navigator.PlayerMover = new SlideMover();
                Navigator.NavigationProvider = new ServiceNavigationProvider();
            }

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

        /// <summary>
        /// Runs custom deliveries for a specific selection of NPCs.
        /// Checks unlock status for each selected NPC before attempting to craft and hand in.
        /// </summary>
        /// <param name="doZhloe">Flag for Zhloe Aliapoh.</param>
        /// <param name="doMnaago">Flag for M'naago.</param>
        /// <param name="doKurenai">Flag for Kurenai.</param>
        /// <param name="doAdkiragh">Flag for Adkiragh.</param>
        /// <param name="doKaishirr">Flag for Kai-Shirr.</param>
        /// <param name="doEhlltou">Flag for Ehll Tou.</param>
        /// <param name="doCharlemend">Flag for Charlemend.</param>
        /// <param name="doAmeliance">Flag for Ameliance.</param>
        /// <param name="doAnden">Flag for Anden.</param>
        /// <param name="doMargrat">Flag for Margrat.</param>
        /// <param name="doNitowikwe">Flag for Nitowikwe.</param>
        /// <param name="doTiisolJa">Flag for Tiisol Ja.</param>
        /// <param name="dohClass">The Disciple of the Hand class to use for crafting.</param>
        /// <returns><see langword="true"/> upon completion.</returns>
        public static async Task<bool> RunCustomDeliveriesBySelection(bool doZhloe, bool doMnaago, bool doKurenai, bool doAdkiragh, bool doKaishirr, bool doEhlltou, bool doCharlemend, bool doAmeliance, bool doAnden, bool doMargrat, bool doNitowikwe, bool doTiisolJa, DohClasses dohClass = DohClasses.Carpenter)
        {
            if (Navigator.NavigationProvider == null)
            {
                Navigator.PlayerMover = new SlideMover();
                Navigator.NavigationProvider = new ServiceNavigationProvider();
            }

            var DeliveryNpcs = ResourceManager.CustomDeliveryNpcs.Value;

            if (doZhloe)
            {
                var npc = DeliveryNpcs.Find(i => string.Equals(i.Name, "Zhloe Aliapoh", StringComparison.Ordinal));

                if (npc != null && !QuestLogManager.IsQuestCompleted((uint)npc.RequiredQuest))
                {
                    await NotUnlocked(npc);
                }
                else
                {
                    await CraftThenHandinNpc(npc, dohClass, false);
                }
            }

            if (doMnaago)
            {
                var npc = DeliveryNpcs.Find(i => string.Equals(i.Name, "M'naago", StringComparison.Ordinal));

                if (npc != null && !QuestLogManager.IsQuestCompleted((uint)npc.RequiredQuest))
                {
                    await NotUnlocked(npc);
                }
                else
                {
                    await CraftThenHandinNpc(npc, dohClass, false);
                }
            }

            if (doKurenai)
            {
                var npc = DeliveryNpcs.Find(i => string.Equals(i.Name, "Kurenai", StringComparison.Ordinal));

                if (npc != null && !QuestLogManager.IsQuestCompleted((uint)npc.RequiredQuest))
                {
                    await NotUnlocked(npc);
                }
                else
                {
                    await CraftThenHandinNpc(npc, dohClass, false);
                }
            }

            if (doAdkiragh)
            {
                var npc = DeliveryNpcs.Find(i => string.Equals(i.Name, "Adkiragh", StringComparison.Ordinal));

                if (npc != null && !QuestLogManager.IsQuestCompleted((uint)npc.RequiredQuest))
                {
                    await NotUnlocked(npc);
                }
                else
                {
                    await CraftThenHandinNpc(npc, dohClass, false);
                }
            }

            if (doKaishirr)
            {
                var npc = DeliveryNpcs.Find(i => string.Equals(i.Name, "Kai-Shirr", StringComparison.Ordinal));

                if (npc != null && !QuestLogManager.IsQuestCompleted((uint)npc.RequiredQuest))
                {
                    await NotUnlocked(npc);
                }
                else
                {
                    await CraftThenHandinNpc(npc, dohClass, false);
                }
            }

            if (doEhlltou)
            {
                var npc = DeliveryNpcs.Find(i => string.Equals(i.Name, "Ehll Tou", StringComparison.Ordinal));

                if (npc != null && !QuestLogManager.IsQuestCompleted((uint)npc.RequiredQuest))
                {
                    await NotUnlocked(npc);
                }
                else
                {
                    await CraftThenHandinNpc(npc, dohClass, false);
                }
            }

            if (doCharlemend)
            {
                var npc = DeliveryNpcs.Find(i => string.Equals(i.Name, "Charlemend", StringComparison.Ordinal));

                if (npc != null && !QuestLogManager.IsQuestCompleted((uint)npc.RequiredQuest))
                {
                    await NotUnlocked(npc);
                }
                else
                {
                    await CraftThenHandinNpc(npc, dohClass, false);
                }
            }

            if (doAmeliance)
            {
                var npc = DeliveryNpcs.Find(i => string.Equals(i.Name, "Ameliance", StringComparison.Ordinal));

                if (npc != null && !QuestLogManager.IsQuestCompleted((uint)npc.RequiredQuest))
                {
                    await NotUnlocked(npc);
                }
                else
                {
                    await CraftThenHandinNpc(npc, dohClass, false);
                }
            }

            if (doAnden)
            {
                var npc = DeliveryNpcs.Find(i => string.Equals(i.Name, "Anden", StringComparison.Ordinal));

                if (npc != null && !QuestLogManager.IsQuestCompleted((uint)npc.RequiredQuest))
                {
                    await NotUnlocked(npc);
                }
                else
                {
                    await CraftThenHandinNpc(npc, dohClass, false);
                }
            }

            if (doMargrat)
            {
                var npc = DeliveryNpcs.Find(i => string.Equals(i.Name, "Margrat", StringComparison.Ordinal));

                if (npc != null && !QuestLogManager.IsQuestCompleted((uint)npc.RequiredQuest))
                {
                    await NotUnlocked(npc);
                }
                else
                {
                    await CraftThenHandinNpc(npc, dohClass, false);
                }
            }

            if (doNitowikwe)
            {
                var npc = DeliveryNpcs.Find(i => string.Equals(i.Name, "Nitowikwe", StringComparison.Ordinal));

                if (npc != null && !QuestLogManager.IsQuestCompleted((uint)npc.RequiredQuest))
                {
                    await NotUnlocked(npc);
                }
                else
                {
                    await CraftThenHandinNpc(npc, dohClass, false);
                }
            }

            if (doTiisolJa)
            {
                var npc = DeliveryNpcs.Find(i => string.Equals(i.Name, "Tiisol Ja", StringComparison.Ordinal));

                if (npc != null && !QuestLogManager.IsQuestCompleted((uint)npc.RequiredQuest))
                {
                    await NotUnlocked(npc);
                }
                else
                {
                    await CraftThenHandinNpc(npc, dohClass, false);
                }
            }

            TreeRoot.Stop("Stop Requested");
            return true;
        }

        /// <summary>
        /// Displays an error message and stops the bot when a specific custom delivery NPC is not yet unlocked.
        /// </summary>
        /// <param name="deliveryNpc">The NPC that is currently locked.</param>
        public static async Task NotUnlocked(CustomDeliveryNpc deliveryNpc)
        {
            var message = $"{DataManager.GetLocalizedNPCName((int)deliveryNpc.npcId)} not unlocked.\nPlease complete the quest '{DataManager.GetLocalizedQuestName(deliveryNpc.RequiredQuest)}' or run the unlock profile.";

            Core.OverlayManager.AddToast(() => $"{message}", TimeSpan.FromMilliseconds(25000), Color.FromRgb(29, 226, 213), Color.FromRgb(13, 106, 175), new FontFamily("Gautami"));
            Log.Error($"{message}");
            TreeRoot.Stop($"{message}");
        }

        /// <summary>
        /// Retrieves the specific item ID and remaining delivery allowance for a Disciple of the Hand (DOH) delivery to the given NPC.
        /// </summary>
        /// <param name="deliveryNpc">The target custom delivery NPC.</param>
        /// <returns>A tuple containing the required item ID and the remaining allowances.</returns>
        public static async Task<(uint ItemId, int DeliveriesReamining)> GetCraftingDeliveryItems(CustomDeliveryNpc deliveryNpc)
        {
            await AgentSatisfactionSupply.Instance.LoadWindow(deliveryNpc.Index);
            var items = new List<uint>();

            /*
            if (deliveryNpc.npcId != AgentSatisfactionSupply.Instance.NpcId)
            {
                Log.Information($"Bad Npc ID: {AgentSatisfactionSupply.Instance.NpcId}");
                return (0, 0);
            }
            */

            Log.Information($"{deliveryNpc.Name}");
            Log.Information($"\tDeliveries Remaining:{AgentSatisfactionSupply.Instance.DeliveriesRemaining}");
            Log.Information($"\tDoH: {DataManager.GetItem(AgentSatisfactionSupply.Instance.DoHItemId)}");
            items.Add(AgentSatisfactionSupply.Instance.DoHItemId);

            return (AgentSatisfactionSupply.Instance.DoHItemId, AgentSatisfactionSupply.Instance.DeliveriesRemaining);
        }

        /// <summary>
        /// Manages the full workflow for a specific NPC: checks satisfaction level, calls <see cref="Lisbeth"/> to craft
        /// required items (after purchasing materials via <see cref="GilShopping"/>), and performs the hand-in.
        /// </summary>
        /// <param name="deliveryNpc">The NPC to process.</param>
        /// <param name="dohClass">The crafting class to use.</param>
        /// <param name="stopAtFiveHearts">If <see langword="true"/>, skips NPCs who have already reached maximum satisfaction (5 hearts).</param>
        public static async Task CraftThenHandinNpc(CustomDeliveryNpc? deliveryNpc, DohClasses dohClass = DohClasses.Carpenter, bool stopAtFiveHearts = true)
        {
            if (deliveryNpc == null)
            {
                Log.Error("Delivery NPC is null");
                return;
            }

            await AgentSatisfactionSupply.Instance.LoadWindow(deliveryNpc.Index);
            var items = new List<uint>();
            /*
            if (deliveryNpc.npcId != AgentSatisfactionSupply.Instance.NpcId)
            {
                Log.Information($"Bad Npc ID: {AgentSatisfactionSupply.Instance.NpcId}");
                return;
            }
            */

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

            var outList = new List<LisbethOrder>
            {
                // if (deliveryNpc.npcId == 1025878)
                //  {
                //      outList.Add(new LisbethOrder(0, 1, (int)AgentSatisfactionSupply.Instance.DoLItemId, Math.Min(3, (int)AgentSatisfactionSupply.Instance.DeliveriesRemaining), "Gather", true));
                //  }
                //  else
                //  {
                new LisbethOrder(0, 1, (int)AgentSatisfactionSupply.Instance.DoHItemId, Math.Max(3, (int)AgentSatisfactionSupply.Instance.DeliveriesRemaining), dohClass.ToString(), true)
            };

            //  }

            var order = JsonConvert.SerializeObject(outList, Formatting.None).Replace("Hq", "Collectable");

            if (!InventoryManager.FilledSlots.Any(i => items.Contains(i.RawItemId)))
            {
                if (order != "" && !InventoryManager.FilledSlots.Any(i => items.Contains(i.RawItemId)))
                {
                    await GeneralFunctions.StopBusy();
                    Log.Information($"Calling Lisbeth for {DataManager.GetItem(AgentSatisfactionSupply.Instance.DoHItemId).CurrentLocaleName} {order}");
                    try
                    {
                        var recipe = LookUpRecipe(AgentSatisfactionSupply.Instance.DoHItemId, dohClass);
                        if (recipe != null)
                        {
                            Log.Information($"Buy mats for {DataManager.GetItem(AgentSatisfactionSupply.Instance.DoHItemId)}");
                            if (!await GilShopping.GetRequiredItems(recipe, Math.Max(3, (int)AgentSatisfactionSupply.Instance.DeliveriesRemaining)))
                            {
                                Log.Information($"Failed to buy mats for {DataManager.GetItem(AgentSatisfactionSupply.Instance.DoHItemId)}");
                                return;
                            }
                        }

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

        /// <summary>
        /// Attempts to find a stored recipe for a specific item produced by a Disciple of the Hand (DOH) class.
        /// Currently relies on <see cref="ResourceManager.Recipes_Anden"/>.
        /// </summary>
        /// <param name="resultingItem">The item ID produced by the recipe.</param>
        /// <param name="dohClass">The crafting class to filter by.</param>
        /// <returns>The <see cref="StoredRecipe"/> if found; otherwise <see langword="null"/>.</returns>
        public static StoredRecipe? LookUpRecipe(uint resultingItem, DohClasses dohClass = DohClasses.Carpenter)
        {
            if (!ResourceManager.Recipes_Anden.TryGetValue(resultingItem, out var recipe))
            {
                return null;
            }

            return recipe.Find(i => i.CraftingClass == (ClassJobType)dohClass);
        }

        /// <summary>
        /// Performs the physical hand-in of custom delivery items to an NPC.
        /// Handles NPC interaction, dialog selection, and the satisfaction result window (including cutscenes).
        /// </summary>
        /// <param name="deliveryNpc">The NPC to hand items to.</param>
        /// <returns><see langword="true"/> if the hand-in was completed; otherwise <see langword="false"/>.</returns>
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

            await DealWithTalk();

            if (SatisfactionSupply.Instance.IsOpen)
            {
                SatisfactionSupply.Instance.Close();
                await Coroutine.Wait(10000, () => !SatisfactionSupply.Instance.IsOpen);
            }

            await DealWithTalk();

            await Coroutine.Wait(1000, () => Conversation.IsOpen);
            if (Conversation.IsOpen)
            {
                Conversation.SelectLine((uint)(Conversation.GetConversationList.Count - 1));
            }

            await DealWithTalk();

            return true;
        }

        /// <summary>
        /// Dismisses active NPC talk dialogs by repeatedly sending the "Next" action until the dialog is closed.
        /// </summary>
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

        /// <summary>
        /// Logs current satisfaction and delivery status for all unlocked custom delivery NPCs to the console.
        /// </summary>
        /// <returns><see langword="true"/> upon completion (bot stops when finished).</returns>
        public static async Task<bool> DebugCustomDeliveries()
        {
            var DeliveryNpcs = ResourceManager.CustomDeliveryNpcs.Value;
            Log.Information(AgentSatisfactionSupply.Instance.DeliveriesPointer.ToString("x8"));
            foreach (var npc in DeliveryNpcs.Where(i => ConditionParser.IsQuestCompleted(i.RequiredQuest)).OrderBy(i => i.Index))
            {
                await AgentSatisfactionSupply.Instance.LoadWindow(npc.Index);

                Log.Information($"{npc.Name}");
                Log.Information($"\tHeartLevel:{AgentSatisfactionSupply.Instance.HeartLevel}");
                Log.Information($"\tRep:{AgentSatisfactionSupply.Instance.CurrentRep}/{AgentSatisfactionSupply.Instance.MaxRep}");
                Log.Information($"\tDeliveries Remaining:{AgentSatisfactionSupply.Instance.DeliveriesRemaining}");
                Log.Information($"\tDoH: {DataManager.GetItem(AgentSatisfactionSupply.Instance.DoHItemId)}");
                Log.Information($"\tDoL: {DataManager.GetItem(AgentSatisfactionSupply.Instance.DoLItemId)}");
                Log.Information($"\tFsh: {DataManager.GetItem(AgentSatisfactionSupply.Instance.FshItemId)}");
            }

            TreeRoot.Stop("Stop Requested");
            return true;
        }
    }
}