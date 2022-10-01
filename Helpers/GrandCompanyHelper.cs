using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Pathing.Service_Navigation;
using ff14bot.RemoteWindows;
using LlamaLibrary.Enums;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;
using static LlamaLibrary.Helpers.GeneralFunctions;

namespace LlamaLibrary.Helpers
{
    public static class GrandCompanyHelper
    {
        private static readonly string Name = "GrandCompanyHelper";
        private static readonly Color LogColor = Colors.LimeGreen;
        private static readonly LLogger Log = new LLogger(Name, LogColor, LogLevel.Information);

        public static Dictionary<GrandCompany, KeyValuePair<uint, Vector3>> BaseLocations = new Dictionary<GrandCompany, KeyValuePair<uint, Vector3>>
        {
            { GrandCompany.Immortal_Flames, new KeyValuePair<uint, Vector3>(130, new Vector3(-139.3435f, 4.1f, -100.8658f)) },
            { GrandCompany.Order_Of_The_Twin_Adder, new KeyValuePair<uint, Vector3>(132, new Vector3(-67.49361f, -0.5035391f, -2.149932f)) },
            { GrandCompany.Maelstrom, new KeyValuePair<uint, Vector3>(128, new Vector3(88.8576f, 40.24876f, 71.6758f)) }
        };

        public static Dictionary<GCNpc, uint> MaelstromNPCs = new Dictionary<GCNpc, uint>
        {
            { GCNpc.Flyer, 1011820 },
            { GCNpc.Mage, 1003248 },
            { GCNpc.OIC_Administrator, 1003247 },
            { GCNpc.Personnel_Officer, 1002388 },
            { GCNpc.OIC_Quartermaster, 1002389 },
            { GCNpc.OIC_Officer_of_Arms, 1005183 },
            { GCNpc.Quartermaster, 1002387 },
            { GCNpc.Company_Chest, 2000470 },
            { GCNpc.Hunt_Board, 2004438 },
            { GCNpc.Entrance_to_the_Barracks, 2007527 },
            { GCNpc.Commander, 1003281 },
            { GCNpc.Squadron_Sergeant, 1016986 },
            { GCNpc.Hunt_Billmaster, 1009552 }
        };

        public static Dictionary<GCNpc, uint> FlameNPCs = new Dictionary<GCNpc, uint>
        {
            { GCNpc.Flyer, 1011818 },
            { GCNpc.Mage, 1004380 },
            { GCNpc.Personnel_Officer, 1002391 },
            { GCNpc.OIC_Administrator, 1002392 },
            { GCNpc.OIC_Quartermaster, 1003925 },
            { GCNpc.Quartermaster, 1002390 },
            { GCNpc.OIC_Officer_of_Arms, 1004513 },
            { GCNpc.Company_Chest, 2000470 },
            { GCNpc.Commander, 1004576 },
            { GCNpc.Entrance_to_the_Barracks, 2007529 },
            { GCNpc.Hunt_Board, 2004440 },
            { GCNpc.Squadron_Sergeant, 1016987 },
            { GCNpc.Hunt_Billmaster, 1001379 }
        };

        public static Dictionary<GCNpc, uint> TwinAdderNPCs = new Dictionary<GCNpc, uint>
        {
            { GCNpc.Flyer, 1011819 },
            { GCNpc.Mage, 1004381 },
            { GCNpc.OIC_Administrator, 1002395 },
            { GCNpc.Personnel_Officer, 1002394 },
            { GCNpc.OIC_Quartermaster, 1000165 },
            { GCNpc.Commander, 1000168 },
            { GCNpc.Quartermaster, 1002393 },
            { GCNpc.Hunt_Billmaster, 1009152 },
            { GCNpc.Company_Chest, 2000470 },
            { GCNpc.Hunt_Board, 2004439 },
            { GCNpc.OIC_Officer_of_Arms, 1004401 },
            { GCNpc.Squadron_Sergeant, 1016924 },
            { GCNpc.Entrance_to_the_Barracks, 2006962 }
        };

        public static Dictionary<GrandCompany, Dictionary<GCNpc, uint>> NpcList = new Dictionary<GrandCompany, Dictionary<GCNpc, uint>>
        {
            { GrandCompany.Immortal_Flames, FlameNPCs },
            { GrandCompany.Order_Of_The_Twin_Adder, TwinAdderNPCs },
            { GrandCompany.Maelstrom, MaelstromNPCs }
        };

        public static async Task GetToGCBase()
        {
            if (Core.Me.GrandCompany == 0)
            {
                return;
            }

            var gcBase = BaseLocations[Core.Me.GrandCompany];
            Log.Information($"{Core.Me.GrandCompany} {gcBase.Key} {gcBase.Value}");
            await Navigation.GetTo(gcBase.Key, gcBase.Value);
        }

        public static async Task GetToGCBarracks()
        {
            Navigator.PlayerMover = new SlideMover();
            Navigator.NavigationProvider = new ServiceNavigationProvider();

            // Not in Barracks
            Log.Information($"Moving to Barracks");
            await GrandCompanyHelper.InteractWithNpc(GCNpc.Entrance_to_the_Barracks);
            await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
            await Buddy.Coroutines.Coroutine.Sleep(500);
            if (ff14bot.RemoteWindows.SelectYesno.IsOpen)
            {
                Log.Information($"Selecting Yes.");
                ff14bot.RemoteWindows.SelectYesno.ClickYes();
            }

            await Coroutine.Wait(5000, () => CommonBehaviors.IsLoading);
            while (CommonBehaviors.IsLoading)
            {
                Log.Information($"Waiting for zoning to finish...");
                await Coroutine.Wait(-1, () => (!CommonBehaviors.IsLoading));
            }
        }

        public static uint GetNpcByType(GCNpc npc)
        {
            return NpcList[Core.Me.GrandCompany][npc];
        }

        public static async Task InteractWithNpc(GCNpc npc)
        {
            if (Core.Me.GrandCompany == 0)
            {
                return;
            }

            var targetNpc = GameObjectManager.GetObjectByNPCId(NpcList[Core.Me.GrandCompany][npc]);
            if (targetNpc == null || !targetNpc.IsWithinInteractRange)
            {
                await GetToGCBase();
                targetNpc = GameObjectManager.GetObjectByNPCId(NpcList[Core.Me.GrandCompany][npc]);
            }

            if (targetNpc == null)
            {
                return;
            }

            if (!targetNpc.IsWithinInteractRange)
            {
                await Navigation.OffMeshMoveInteract(targetNpc);
            }

            if (targetNpc.IsWithinInteractRange)
            {
                targetNpc.Interact();
            }
        }

        public static async Task GetToGCBase(GrandCompany grandCompany)
        {
            var gcBase = BaseLocations[grandCompany];
            Log.Information($"{grandCompany} {gcBase.Key} {gcBase.Value}");
            await Navigation.GetTo(gcBase.Key, gcBase.Value);
        }

        public static uint GetNpcByType(GCNpc npc, GrandCompany grandCompany)
        {
            return NpcList[grandCompany][npc];
        }

        public static async Task InteractWithNpc(GCNpc npc, GrandCompany grandCompany)
        {
            var targetNpc = GameObjectManager.GetObjectByNPCId(NpcList[grandCompany][npc]);
            if (targetNpc == null || !targetNpc.IsWithinInteractRange)
            {
                await GetToGCBase(grandCompany);
                targetNpc = GameObjectManager.GetObjectByNPCId(NpcList[grandCompany][npc]);
            }

            if (targetNpc == null)
            {
                return;
            }

            if (!targetNpc.IsWithinInteractRange)
            {
                await Navigation.OffMeshMoveInteract(targetNpc);
            }

            if (targetNpc.IsWithinInteractRange)
            {
                targetNpc.Interact();
            }
        }

        public static async Task BuyFCAction(GrandCompany grandCompany, int actionId)
        {
            await InteractWithNpc(GCNpc.OIC_Quartermaster, grandCompany);
            await Coroutine.Wait(5000, () => Talk.DialogOpen || Conversation.IsOpen);

            if (!Talk.DialogOpen)
            {
                await InteractWithNpc(GCNpc.OIC_Quartermaster, grandCompany);
                await Coroutine.Wait(5000, () => Talk.DialogOpen);
            }

            if (Talk.DialogOpen || Conversation.IsOpen)
            {
                if (Talk.DialogOpen)
                {
                    Talk.Next();
                    await Coroutine.Wait(5000, () => Conversation.IsOpen);
                }

                if (Conversation.IsOpen)
                {
                    Conversation.SelectLine(0);
                    await Coroutine.Wait(10000, () => FreeCompanyExchange.Instance.IsOpen);
                    if (FreeCompanyExchange.Instance.IsOpen)
                    {
                        await Coroutine.Sleep(500);
                        await FreeCompanyExchange.Instance.BuyAction(actionId);
                        await Coroutine.Sleep(500);
                        FreeCompanyExchange.Instance.Close();
                    }
                }
            }
        }

        public static async Task GoGCRankUp()
        {
            var grandCompany = Core.Me.GrandCompany;

            await GetToGCBase();

            await InteractWithNpc(GCNpc.Personnel_Officer, grandCompany);
            await Buddy.Coroutines.Coroutine.Wait(5000, () => SelectString.IsOpen);

            Log.Information($"Apply for a promotion to {grandCompany}");

            if (!SelectString.IsOpen)
            {
                await InteractWithNpc(GCNpc.Personnel_Officer, grandCompany);
                await Coroutine.Wait(5000, () => SelectString.IsOpen);
            }

            if (SelectString.IsOpen)
            {
                Log.Information($"Clicking 'Apply for a promotion'.");
                ff14bot.RemoteWindows.SelectString.ClickSlot(1);
            }

            await Buddy.Coroutines.Coroutine.Wait(10000, () => Talk.DialogOpen);
            while (!GrandCompanyRankUp.Instance.IsOpen)
            {
                Talk.Next();
                await Coroutine.Sleep(500);
            }

            GrandCompanyRankUp.Instance.Confirm();

            await SmallTalk(500);
        }

        public static async Task GCHandInExpert()
        {
            if (!GrandCompanySupplyList.Instance.IsOpen)
            {
                await InteractWithNpc(GCNpc.Personnel_Officer);
                await Coroutine.Wait(5000, () => SelectString.IsOpen);
                if (!SelectString.IsOpen)
                {
                    Log.Error("Window is not open...maybe it didn't get to npc?");
                }

                SelectString.ClickSlot(0);
                await Coroutine.Wait(5000, () => GrandCompanySupplyList.Instance.IsOpen);
                if (!GrandCompanySupplyList.Instance.IsOpen)
                {
                    Log.Information("Window is not open...maybe it didn't get to npc?");
                }
            }

            if (GrandCompanySupplyList.Instance.IsOpen)
            {
                if (AgentGrandCompanySupply.Instance.HandinType != GCSupplyType.Expert)
                {
                    Log.Debug("Switching to Expert Delivery");
                    await GrandCompanySupplyList.Instance.SwitchToExpertDelivery();
                }

                //Check Filter
                if (AgentGrandCompanySupply.Instance.ExpertFilter != GCFilter.HideArmory)
                {
                    Log.Debug($"Setting filter to hide armory it's currently {AgentGrandCompanySupply.Instance.ExpertFilter}");
                    GrandCompanySupplyList.Instance.SetExpertFilter((byte)GCFilter.HideArmory);
                    await Coroutine.Wait(2000, () => AgentGrandCompanySupply.Instance.ExpertFilter == GCFilter.HideArmory);
                }

                //Get them items
                var items = AgentGrandCompanySupply.Instance.ExpertSupplyItems;

                //var i = 0;
                var count = items.Length;
                Log.Information($"Item Count {count}");
                if (count > 0)
                {
                    for (var index = 0; index < count; index++)
                    {
                        //var item = windowItemIds[index];
                        //Log.Information($"{index}");
                        var oldCount = AgentGrandCompanySupply.Instance.SupplyItemCount;
                        Log.Information("Clicking");
                        GrandCompanySupplyList.Instance.ClickItem(0);
                        await Coroutine.Wait(1000, () => SelectYesno.IsOpen);
                        if (SelectYesno.IsOpen)
                        {
                            //Log.Debug("Waiting for select yes/no");
                            //if (await Coroutine.Wait(5000, () => SelectYesno.IsOpen))
                            //{
                                SelectYesno.Yes();
                            //}
                        }

                        if (!await GrandCompanySupplyReward.Instance.WaitTillWindowOpen(10000))
                        {
                            Log.Error("Reward window did not show");
                            return;
                        }

                        GrandCompanySupplyReward.Instance.Confirm();

                        if (!await GrandCompanySupplyList.Instance.WaitTillWindowOpen(10000))
                        {
                            Log.Error("Reward window did not close");
                            return;
                        }

                        //i += 1;
                        await Coroutine.Wait(5000, () => AgentGrandCompanySupply.Instance.SupplyItemCount < oldCount);
                    }
                }

                if (GrandCompanySupplyList.Instance.IsOpen)
                {
                    await Coroutine.Sleep(500);
                    GrandCompanySupplyList.Instance.Close();
                    await Coroutine.Wait(5000, () => SelectString.IsOpen);
                    if (SelectString.IsOpen)
                    {
                        SelectString.ClickSlot((uint)(SelectString.LineCount - 1));
                    }
                }
            }
        }
    }
}
