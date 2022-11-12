using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers.Classes;
using LlamaLibrary.Helpers.NPC;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Helpers
{
    public static class NavigationHelper
    {
        private static readonly LLogger Log = new("NavigationHelper", Colors.MediumPurple);

        public static List<Npc> StartingInnKeepers = new()
        {
            new Npc(1000102, 132, new Vector3(26.2571f, -8.000001f, 100.4172f), 65665), //Antoinaut (Innkeep) New Gridania - The Roost
            new Npc(1000974, 128, new Vector3(12.93333f, 39.99998f, 11.71152f), 66005), //Mytesyn (Innkeep) Limsa Lominsa Upper Decks - Mizzenmast Inn
            new Npc(1001976, 130, new Vector3(29.31324f, 6.999999f, -80.32259f), 65856) //Otopa Pottopa (Innkeep) Ul'dah - Steps of Nald - The Hourglass
        };

        public static List<Npc> InnKeepers = new()
        {
            new Npc(1011193, 418, new Vector3(84.43542f, 15.09468f, 33.54416f), 67116), //Bamponcet (Innkeep) Foundation - Cloud Nine
            new Npc(1018981, 628, new Vector3(-86.10562f, 19f, -198.8428f), 68005), //Ushitora (Innkeep) Kugane - Bokairo Inn
            new Npc(1027231, 819, new Vector3(62.32343f, 1.716012f, 249.2582f), 68838), //manager of suites The Crystarium - The Pendants
            new Npc(1037293, 962, new Vector3(-100.3702f, 3.933468f, 2.429576f), 69915) //Ojika Tsunjika (Annex Administrator) Old Sharlayan - The Baldesion Annex
        };

        public static ushort[] InnRoomZones =
        {
            177, 178, 179, 429, 629, 843, 990
        };

        public static bool IsInInnRoom => InnRoomZones.Contains(WorldManager.ZoneId);

        public static async Task<bool> GoToGlamourDresser()
        {
            if (RaptureAtkUnitManager.GetWindowByName("MiragePrismPrismBox") != null)
            {
                return true;
            }

            var dresser = GameObjectManager.GetObjectByNPCId(2009439);
            if (dresser != null)
            {
                await InteractWithNpc(dresser);

                return true;
            }

            if (!StartingInnKeepers.Any(i => i.IsQuestCompleted))
            {
                Log.Information("Need to get further in the MSQ");
                return false;
            }

            var innKeeps = StartingInnKeepers.Concat(InnKeepers.Where(i => i.IsQuestCompleted && i.CanGetTo));
            if (Core.Me.GrandCompany != 0 && GrandCompanyHelper.BarracksNpc is { IsQuestCompleted: true })
            {
                innKeeps = innKeeps.Append(GrandCompanyHelper.BarracksNpc);
            }

            var bestInn = NpcHelper.GetClosestNpc(innKeeps);

            if (bestInn is null)
            {
                Log.Error("No Inn Keepers Found");
                return false;
            }

            if (bestInn.Equals(GrandCompanyHelper.BarracksNpc))
            {
                Log.Information("Going to Barracks");
                if (!await GrandCompanyHelper.GetToGCBarracks())
                {
                    Log.Error("Failed to get to Barracks");
                    return false;
                }
            }
            else
            {
                Log.Information($"Going to Inn {bestInn.Location.ZoneName}");
                if (!await GoToInnRoom(bestInn))
                {
                    Log.Error("Failed to get an Inn");
                    return false;
                }
            }

            dresser = GameObjectManager.GetObjectByNPCId(2009439);

            if (dresser == null)
            {
                Log.Error("Couldn't find the glamour dresser");
                return false;
            }

            await InteractWithNpc(dresser);

            return await Coroutine.Wait(10000, () => RaptureAtkUnitManager.GetWindowByName("MiragePrismPrismBox") != null);
        }

        public static async Task<bool> InteractWithNpc(GameObject? obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.IsWithinInteractRange)
            {
                obj.Target();
                obj.Interact();
                return true;
            }

            if (await Navigation.GroundMove(obj.Location, 1.5f))
            {
                obj.Target();
                obj.Interact();
            }
            else
            {
                await Navigation.OffMeshMoveInteract(obj);
            }

            return true;
        }

        public static async Task<bool> GoToInnRoom()
        {
            if (!StartingInnKeepers.Any(i => i.IsQuestCompleted))
            {
                Log.Information("Need to get further in the MSQ");
                return false;
            }

            var innKeeps = StartingInnKeepers.Concat(InnKeepers.Where(i => i.IsQuestCompleted && i.CanGetTo));

            var bestInn = NpcHelper.GetClosestNpc(innKeeps);

            if (bestInn is null)
            {
                Log.Error("No Inn Keepers Found");
                return false;
            }

            Log.Information($"Going to Inn {bestInn.Location.ZoneName}");
            return await GoToInnRoom(bestInn);
        }

        public static async Task<bool> GoToInnRoom(Npc npc)
        {
            if (IsInInnRoom)
            {
                Log.Information("Already in Inn Room");
                return true;
            }

            if (StartingInnKeepers.Contains(npc) && !StartingInnKeepers.Any(i => i.IsQuestCompleted))
            {
                Log.Information("Need to get further in the MSQ");
                return false;
            }

            if (InnKeepers.Contains(npc) && !npc.IsQuestCompleted)
            {
                Log.Information($"Need to finish quest {npc.QuestRequiredId}");
                return false;
            }

            if (!await Navigation.GetToInteractNpcSelectString(npc, 0))
            {
                Log.Error($"Failed to get to {npc.Name}");
                return false;
            }

            var time = PulseTimer.StartNew(new TimeSpan(0, 1, 0));
            while (!CommonBehaviors.IsLoading && !time.Completed)
            {
                if (Talk.DialogOpen)
                {
                    while (Talk.DialogOpen)
                    {
                        Talk.Next();
                        await Coroutine.Wait(100, () => !Talk.DialogOpen);
                        await Coroutine.Wait(100, () => Talk.DialogOpen);
                        await Coroutine.Sleep(100);
                    }
                }

                await Coroutine.Sleep(100);
            }

            if (time.Completed)
            {
                Log.Error("Timed out trying to get to Inn Room");
                return false;
            }

            await CommonTasks.HandleLoading();

            return IsInInnRoom;
        }
    }
}