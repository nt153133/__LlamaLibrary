using System.Collections.Generic;
using System.Linq;
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
using LlamaLibrary.Helpers.NPC;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;

// ReSharper disable InconsistentNaming

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Provides navigation, NPC interaction, and action helpers for all three Grand Companies
    /// (Maelstrom, Order of the Twin Adder, Immortal Flames).
    /// </summary>
    public static class GrandCompanyHelper
    {
        private static readonly LLogger Log = new(nameof(GrandCompanyHelper), Colors.LimeGreen);

        /// <summary>
        /// Maps each Grand Company to the zone ID and world position of its headquarters.
        /// <list type="bullet">
        ///   <item><description>Immortal Flames → Ul'dah - Steps of Nald (zone 130)</description></item>
        ///   <item><description>Order of the Twin Adder → New Gridania (zone 132)</description></item>
        ///   <item><description>Maelstrom → Limsa Lominsa Upper Decks (zone 128)</description></item>
        /// </list>
        /// </summary>
        public static readonly Dictionary<GrandCompany, KeyValuePair<uint, Vector3>> BaseLocations = new()
        {
            { GrandCompany.Immortal_Flames, new KeyValuePair<uint, Vector3>(130, new Vector3(-139.3435f, 4.1f, -100.8658f)) },
            { GrandCompany.Order_Of_The_Twin_Adder, new KeyValuePair<uint, Vector3>(132, new Vector3(-67.49361f, -0.5035391f, -2.149932f)) },
            { GrandCompany.Maelstrom, new KeyValuePair<uint, Vector3>(128, new Vector3(88.8576f, 40.24876f, 71.6758f)) }
        };

        /// <summary>
        /// Maps each Grand Company to the <see cref="Npc"/> that serves as the entrance to its squadron barracks.
        /// A <see langword="null"/> value for key <c>0</c> represents players not yet enlisted in a Grand Company.
        /// </summary>
        public static readonly Dictionary<GrandCompany, Npc?> Barracks = new()
        {
            { 0, null },
            { GrandCompany.Order_Of_The_Twin_Adder, new Npc(2006962, 132, new Vector3(-79.54579f, -0.5005177f, -5.722877f), 67925) }, //Entrance to the Barracks New Gridania - Adders' Nest
            { GrandCompany.Maelstrom, new Npc(2007527, 128, new Vector3(96.51661f, 40.24842f, 62.67801f), 67926) }, //Entrance to the Barracks Limsa Lominsa Upper Decks - Maelstrom Command
            { GrandCompany.Immortal_Flames, new Npc(2007529, 130, new Vector3(-152.6426f, 4.109719f, -97.63382f), 67927) } //Entrance to the Barracks Ul'dah - Steps of Nald - Hall of Flames
        };

        /// <summary>
        /// NPC ID lookup table for Maelstrom (Limsa Lominsa Upper Decks) NPCs, keyed by <see cref="GCNpc"/> role.
        /// </summary>
        public static readonly Dictionary<GCNpc, uint> MaelstromNPCs = new()
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

        /// <summary>
        /// NPC ID lookup table for Immortal Flames (Ul'dah - Hall of Flames) NPCs, keyed by <see cref="GCNpc"/> role.
        /// </summary>
        public static readonly Dictionary<GCNpc, uint> FlameNPCs = new()
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

        /// <summary>
        /// NPC ID lookup table for Order of the Twin Adder (New Gridania - Adders' Nest) NPCs, keyed by <see cref="GCNpc"/> role.
        /// </summary>
        public static readonly Dictionary<GCNpc, uint> TwinAdderNPCs = new()
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

        /// <summary>
        /// Top-level lookup: maps each <see cref="GrandCompany"/> to its NPC role dictionary.
        /// Use <see cref="GetNpcByType(GCNpc)"/> for a convenience accessor.
        /// </summary>
        public static readonly Dictionary<GrandCompany, Dictionary<GCNpc, uint>> NpcList = new()
        {
            { GrandCompany.Immortal_Flames, FlameNPCs },
            { GrandCompany.Order_Of_The_Twin_Adder, TwinAdderNPCs },
            { GrandCompany.Maelstrom, MaelstromNPCs }
        };

        /// <summary>
        /// Gets the barracks entrance <see cref="Npc"/> for the local player's current Grand Company,
        /// or <see langword="null"/> if the player is not enlisted.
        /// </summary>
        public static Npc? BarracksNpc => Barracks[Core.Me.GrandCompany];

        /// <summary>
        /// Zone IDs corresponding to squadron barracks rooms for the three Grand Companies.
        /// </summary>
        public static readonly ushort[] BarrackRoomZones = {
            534, 535, 536
        };

        /// <summary>
        /// Gets a value indicating whether the local player is currently inside a squadron barracks room.
        /// </summary>
        public static bool IsInBarracks => BarrackRoomZones.Contains(WorldManager.ZoneId);

        /// <summary>
        /// Navigates the local player to their Grand Company headquarters.
        /// Does nothing if the player is not enlisted in a Grand Company.
        /// </summary>
        public static async Task GetToGCBase()
        {
            if (Core.Me.GrandCompany == 0)
            {
                return;
            }

            var gcBase = BaseLocations[Core.Me.GrandCompany];
            Log.Information($"Going to GC Base({Core.Me.GrandCompany})");
            await Navigation.GetTo(gcBase.Key, gcBase.Value);
        }

        /// <summary>
        /// Navigates the local player to the headquarters of the specified Grand Company.
        /// </summary>
        /// <param name="grandCompany">The Grand Company whose base to travel to.</param>
        public static async Task GetToGCBase(GrandCompany grandCompany)
        {
            var gcBase = BaseLocations[grandCompany];
            Log.Information($"Going to GC Base({grandCompany})");
            await Navigation.GetTo(gcBase.Key, gcBase.Value);
        }

        /// <summary>
        /// Navigates the local player into their Grand Company squadron barracks.
        /// Initialises the navigation provider if necessary and handles the zone-transition dialog.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the player is now inside the barracks;
        /// <see langword="false"/> if the player is not enlisted, the barracks NPC is not found, or navigation fails.
        /// </returns>
        public static async Task<bool> GetToGCBarracks()
        {
            if (Navigator.NavigationProvider == null)
            {
                Navigator.PlayerMover = new SlideMover();
                Navigator.NavigationProvider = new ServiceNavigationProvider();
            }

            if (Core.Me.GrandCompany == 0)
            {
                Log.Error("You are not in a Grand Company");
                return false;
            }

            if (WorldManager.ZoneId is 534 or 535 or 536)
            {
                Log.Information("Already in GC Barracks");
                return true;
            }

            var npc = Barracks[Core.Me.GrandCompany];

            if (npc == null)
            {
                Log.Error("No Barracks found for your Grand Company");
                return false;
            }

            /* Removing this as the quest check seems to be completely cursed.
            if (!npc.IsQuestCompleted)
            {
                Log.Error("You have not completed the quest to unlock the Barracks");
                return false;
            }
            */

            Log.Information("Moving to Barracks");

            uint[] entranceIds = { 2007527, 2007529, 2006962 };
            var entranceNpc = GameObjectManager.GameObjects.Where(r => r.IsTargetable && r.IsValid && entranceIds.Contains(r.NpcId)).OrderBy(r => r.Distance()).FirstOrDefault();
            if (entranceNpc != null)
            {
                while (Core.Me.Location.Distance2D(entranceNpc.Location) > 1.5f)
                {
                    await Coroutine.Yield();
                    await Navigation.FlightorMove(entranceNpc.Location);
                }
            }

            if (!await Navigation.GetToInteractNpc(npc, PartyYesNo.Instance) || !SelectYesno.IsOpen)
            {
                Log.Error("Failed to get to Barracks");
                return false;
            }

            Log.Information("Selecting Yes.");
            SelectYesno.ClickYes();

            if (await Coroutine.Wait(5000, () => CommonBehaviors.IsLoading))
            {
                await CommonTasks.HandleLoading();
            }

            return true;
        }

        /// <summary>
        /// Returns the NPC ID for the given role in the local player's Grand Company.
        /// </summary>
        /// <param name="npc">The NPC role to look up.</param>
        /// <returns>The NPC ID as a <see cref="uint"/>.</returns>
        public static uint GetNpcByType(GCNpc npc)
        {
            return NpcList[Core.Me.GrandCompany][npc];
        }

        /// <summary>
        /// Returns the NPC ID for the given role in the specified Grand Company.
        /// </summary>
        /// <param name="npc">The NPC role to look up.</param>
        /// <param name="grandCompany">The Grand Company to query.</param>
        /// <returns>The NPC ID as a <see cref="uint"/>.</returns>
        public static uint GetNpcByType(GCNpc npc, GrandCompany grandCompany)
        {
            return NpcList[grandCompany][npc];
        }

        /// <summary>
        /// Navigates to the local player's Grand Company base and interacts with the specified NPC role.
        /// Does nothing if the player is not enlisted in a Grand Company.
        /// </summary>
        /// <param name="npc">The NPC role to interact with.</param>
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

        /// <summary>
        /// Navigates to the specified Grand Company's base and interacts with the given NPC role.
        /// </summary>
        /// <param name="npc">The NPC role to interact with.</param>
        /// <param name="grandCompany">The Grand Company whose NPC to visit.</param>
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

        /// <summary>
        /// Purchases the specified Free Company action from the OIC Quartermaster of the given Grand Company.
        /// Navigates to the base, opens the exchange window, and completes the transaction.
        /// </summary>
        /// <param name="grandCompany">Grand Company whose OIC Quartermaster to visit.</param>
        /// <param name="actionId">The internal ID of the FC action to purchase.</param>
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

        /// <summary>
        /// Applies for a promotion with the local player's Grand Company Personnel Officer.
        /// Navigates to the GC base, opens the rank-up dialog, and confirms the promotion.
        /// </summary>
        public static async Task GoGCRankUp()
        {
            var grandCompany = Core.Me.GrandCompany;

            await GetToGCBase();

            await InteractWithNpc(GCNpc.Personnel_Officer, grandCompany);
            await Coroutine.Wait(5000, () => SelectString.IsOpen);

            Log.Information($"Apply for a promotion to {grandCompany}");

            if (!SelectString.IsOpen)
            {
                await InteractWithNpc(GCNpc.Personnel_Officer, grandCompany);
                await Coroutine.Wait(5000, () => SelectString.IsOpen);
            }

            if (SelectString.IsOpen)
            {
                Log.Information("Clicking 'Apply for a promotion'.");
                SelectString.ClickSlot(1);
            }

            await Coroutine.Wait(10000, () => Talk.DialogOpen);
            while (!GrandCompanyRankUp.Instance.IsOpen)
            {
                Talk.Next();
                await Coroutine.Sleep(500);
            }

            if (GrandCompanyRankUp.Instance.IsOpen)
            {
                GrandCompanyRankUp.Instance.Confirm();
                await Coroutine.Wait(10000, () => QuestLogManager.InCutscene || Talk.DialogOpen || !MovementManager.IsOccupied);
            }

            while (MovementManager.IsOccupied)
            {
                Talk.Next();
                await Coroutine.Sleep(500);
            }
        }

        /// <summary>
        /// Opens the Grand Company Expert Delivery window and hands in all eligible items, collecting seals.
        /// Switches the supply list to Expert Delivery mode, applies the "Hide Armory" filter, then
        /// iterates every listed item and confirms each hand-in via the reward dialog.
        /// </summary>
        public static async Task GCHandInExpert()
        {
            if (!await ExpertDelivery.MakeSureWindowOpen())
            {
                return;
            }

            /*if (!GrandCompanySupplyList.Instance.IsOpen)
            {
                await InteractWithNpc(GCNpc.Personnel_Officer);
                await Coroutine.Wait(10000, () => SelectString.IsOpen);
                if (!SelectString.IsOpen)
                {
                    Log.Error("Window is not open...maybe it didn't get to npc?");
                }

                SelectString.ClickSlot(0);
                await Coroutine.Wait(10000, () => GrandCompanySupplyList.Instance.IsOpen);
                if (!GrandCompanySupplyList.Instance.IsOpen)
                {
                    Log.Information("Window is not open...maybe it didn't get to npc?");
                }
            }*/

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