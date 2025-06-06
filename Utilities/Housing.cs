﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.NeoProfiles;
using ff14bot.Pathing.Service_Navigation;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;
using static ff14bot.RemoteWindows.Talk;
// ReSharper disable MemberCanBePrivate.Global

namespace LlamaLibrary.Utilities
{
    public class Housing
    {
        public static string NameStatic = "Housing Utility";
        private static readonly LLogger Log = new(NameStatic, Colors.Pink);

        public static async Task CheckHousing()
        {
            if (Navigator.NavigationProvider == null)
            {
                Navigator.PlayerMover = new SlideMover();
                Navigator.NavigationProvider = new ServiceNavigationProvider();
            }

            var output = new List<string>();
            var medium = false;
            var large = false;
            var outputMed = new List<string>();
            var outputLarge = new List<string>();

            if (ConditionParser.HasAetheryte(2))
            {
                output.AddRange(await GetLavenderPlots());
            }

            foreach (var line in output)
            {
                if (line.Contains("Medium"))
                {
                    medium = true;
                    outputMed.Add(line);
                }
                else if (line.Contains("Large"))
                {
                    large = true;
                    outputLarge.Add(line);
                }
            }

            /*
            if (large)
            {
                var message = string.Join("\n", outputLarge);
                var title = "Large";
                MessageBox.Show(message, title);
            }
            */

            if (ConditionParser.HasAetheryte(8))
            {
                output.AddRange(await GetMistsPlots());
            }

            foreach (var line in output)
            {
                if (line.Contains("Medium"))
                {
                    medium = true;
                    outputMed.Add(line);
                }
                else if (line.Contains("Large"))
                {
                    large = true;
                    outputLarge.Add(line);
                }
            }

            /*
            if (large)
            {
                var message = string.Join("\n", outputLarge);
                var title = "Large";
                MessageBox.Show(message, title);
            }
            */

            if (ConditionParser.HasAetheryte(9))
            {
                output.AddRange(await GetGobletPlots());
            }

            foreach (var line in output)
            {
                if (line.Contains("Medium"))
                {
                    medium = true;
                    outputMed.Add(line);
                }
                else if (line.Contains("Large"))
                {
                    large = true;
                    outputLarge.Add(line);
                }
            }

            /*if (large)
            {
                var message = string.Join("\n", outputLarge);
                var title = "Large";
                MessageBox.Show(message, title);
            }*/

            if (ConditionParser.HasAetheryte(111))
            {
                output.AddRange(await GetShiroganePlots());
            }

            if (ConditionParser.IsQuestCompleted(69708))
            {
                output.AddRange(await GetEmpyreumPlots());
            }

            if (output.Count == 0)
            {
                Log.Warning("No Housing Plots For Sale");
            }

            foreach (var line in output)
            {
                if (line.Contains("Small"))
                {
                    Log.Information($"{line}");
                }
                else if (line.Contains("Medium"))
                {
                    Log.Information($"{line}");
                    medium = true;
                    outputMed.Add(line);
                }
                else if (line.Contains("Large"))
                {
                    Log.Information($"{line}");
                    large = true;
                    outputLarge.Add(line);
                }
                else
                {
                    Log.Information($"{line}");
                }
            }

            /*
            if (medium)
            {
                var message = string.Join("\n", outputMed);
                var title = "Medium";
                MessageBox.Show(message, title);
            }

            if (large)
            {
                var message = string.Join("\n", outputLarge);
                var title = "Large";
                MessageBox.Show(message, title);
            }*/

            if (medium || large)
            {
            }
        }

        public static async Task<List<string>> GetMistsPlots()
        {
            if (ConditionParser.IsQuestCompleted(66750))
            {
                await GetToResidential(8);
            }
            else
            {
                await GetToMistsWindow();
            }

            if (!SelectString.IsOpen)
            {
                return new List<string>();
            }

            await OpenHousingWards();
            var list = await HousingWards();

            if (ConditionParser.IsQuestCompleted(66750))
            {
                await CloseHousingWardsNoLoad();
            }
            else
            {
                await CloseHousingWards();
            }

            return list;
        }

        public static async Task<List<string>> GetEmpyreumPlots()
        {
            if (ConditionParser.IsQuestCompleted(69708))
            {
                await GetToResidential(70);
            }
            else
            {
                Log.Error("Need to unlock Empyreum by doing the quest Ascending To Empyreum");
                return new List<string>();
            }

            if (!SelectString.IsOpen)
            {
                return new List<string>();
            }

            await OpenHousingWards();
            var list = await HousingWards();

            if (ConditionParser.IsQuestCompleted(69708))
            {
                await CloseHousingWardsNoLoad();
            }
            else
            {
                await CloseHousingWards();
            }

            return list;
        }

        public static async Task<List<string>> GetLavenderPlots()
        {
            if (ConditionParser.IsQuestCompleted(66748))
            {
                await GetToResidential(2);
            }
            else
            {
                await GetToLavenderWindow();
            }

            if (!SelectString.IsOpen)
            {
                return new List<string>();
            }

            await OpenHousingWards();
            var list = await HousingWards();

            if (ConditionParser.IsQuestCompleted(66748))
            {
                await CloseHousingWardsNoLoad();
            }
            else
            {
                await CloseHousingWards();
            }

            return list;
        }

        public static async Task<List<string>> GetGobletPlots()
        {
            if (ConditionParser.IsQuestCompleted(66749))
            {
                await GetToResidential(9);
            }
            else
            {
                await GetToGobletWindow();
            }

            if (!SelectString.IsOpen)
            {
                return new List<string>();
            }

            await OpenHousingWards();
            var list = await HousingWards();

            if (ConditionParser.IsQuestCompleted(66749))
            {
                await CloseHousingWardsNoLoad();
            }
            else
            {
                await CloseHousingWards();
            }

            return list;
        }

        public static async Task<List<string>> GetShiroganePlots()
        {
            if (ConditionParser.IsQuestCompleted(68167))
            {
                await GetToResidential(111);
            }
            else
            {
                await GetToShiroganeWindow();
            }

            if (!SelectString.IsOpen)
            {
                return new List<string>();
            }

            await OpenHousingWards();
            var list = await HousingWards();

            if (ConditionParser.IsQuestCompleted(68167))
            {
                await CloseHousingWardsNoLoad();
            }
            else
            {
                await CloseHousingWards();
            }

            return list;
        }

        public static async Task GetToResidential(uint aetheryteId)
        {
            if (!ConditionParser.HasAetheryte(aetheryteId))
            {
                return;
            }

            if (!WorldManager.TeleportById(aetheryteId))
            {
                return;
            }

            do
            {
                await Coroutine.Sleep(2000);
            }
            while (Core.Me.IsCasting);

            await Coroutine.Sleep(2000);
            if (CommonBehaviors.IsLoading)
            {
                await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
            }

            await Coroutine.Wait(10000, () => GameObjectManager.GetObjectByNPCId(aetheryteId) != null);
            await Coroutine.Sleep(2000);

            var unit = GameObjectManager.GetObjectByNPCId(aetheryteId);

            if (!unit.IsWithinInteractRange)
            {
                Log.Warning($"Not in range {unit.Distance2D()}");
                var target = unit.Location;
                if (WorldManager.RawZoneId == 129)
                {
                    target = new Vector3(-89.30112f, 18.80033f, -2.019181f);
                }
                else if (WorldManager.RawZoneId == 628)
                {
                    // Kugane
                    target = new Vector3(48.03579f, 4.549999f, -31.83851f);
                }

                //await CommonTasks.MoveAndStop(new MoveToParameters(target.FanOutRandom(2f), unit.Name), 2f, true);
                if (aetheryteId == 70)
                {
                    await Navigation.OffMeshMoveInteract(unit);
                }
                else
                {
                    await Navigation.GetTo(WorldManager.ZoneId, target);
                }

                /*                Navigator.PlayerMover.MoveTowards(target);
                                while (!unit.IsWithinInteractRange)
                                {
                                    Navigator.PlayerMover.MoveTowards(target);
                                    await Coroutine.Sleep(100);
                                }

                                Navigator.PlayerMover.MoveStop();*/
            }
            else
            {
                Log.Verbose($"In range {unit.Distance2D()}");
            }

            unit.Target();
            unit.Interact();

            await Coroutine.Wait(5000, () => SelectString.IsOpen);
            if (SelectString.IsOpen)
            {
                SelectString.ClickLineContains(Translator.Language == Language.Chn ? "冒险者住宅区传送" : "Residential");
            }

            await Coroutine.Sleep(500);
            await Coroutine.Wait(5000, () => SelectString.IsOpen);
        }

        public static async Task GetToLavenderWindow()
        {
            await Navigation.GetTo(148, new Vector3(199.5991f, -32.04532f, 324.2699f));

            uint FerryNpc = 1005656;

            var unit = GameObjectManager.GetObjectByNPCId(FerryNpc);

            if (!unit.IsWithinInteractRange)
            {
                var target = unit.Location;
                Navigator.PlayerMover.MoveTowards(target);
                while (!unit.IsWithinInteractRange)
                {
                    Navigator.PlayerMover.MoveTowards(target);
                    await Coroutine.Sleep(100);
                }

                Navigator.PlayerMover.MoveStop();
            }

            unit.Target();
            unit.Interact();

            await Coroutine.Wait(5000, () => SelectIconString.IsOpen);

            if (SelectIconString.IsOpen)
            {
                SelectIconString.ClickLineContains(Translator.Language == Language.Chn ? "薰衣草苗圃" : "Lavender Beds");

                await Coroutine.Wait(5000, () => DialogOpen || SelectString.IsOpen);
            }

            if (DialogOpen)
            {
                Next();
            }

            await Coroutine.Wait(3000, () => SelectString.IsOpen);
        }

        public static async Task OpenHousingWards()
        {
            if (SelectString.IsOpen)
            {
                SelectString.ClickLineContains(Translator.Language == Language.Chn ? "移动到指定小区" : "Go to specified");

                await Coroutine.Wait(5000, () => HousingSelectBlock.Instance.IsOpen);
            }
        }

        public static async Task GetToGobletWindow()
        {
            await Navigation.GetTo(140, new Vector3(317.0663f, 67.27534f, 232.8395f));

            var zoneChange = new Vector3(316.7798f, 67.13619f, 236.8774f);

            while (!SelectString.IsOpen)
            {
                Navigator.PlayerMover.MoveTowards(zoneChange);
                await Coroutine.Sleep(50);
                Navigator.PlayerMover.MoveStop();
            }

            Navigator.PlayerMover.MoveStop();
            await Coroutine.Wait(3000, () => SelectString.IsOpen);
        }

        public static async Task GetToMistsWindow()
        {
            await Navigation.GetTo(135, new Vector3(597.4801f, 61.59979f, -110.7737f));

            var zoneChange = new Vector3(598.1823f, 61.52054f, -108.3216f);

            while (!SelectString.IsOpen)
            {
                Navigator.PlayerMover.MoveTowards(zoneChange);
                await Coroutine.Sleep(50);
                Navigator.PlayerMover.MoveStop();
            }

            Navigator.PlayerMover.MoveStop();
            await Coroutine.Wait(3000, () => SelectString.IsOpen);
        }

        public static async Task GetToShiroganeWindow()
        {
            await Navigation.GetTo(628, new Vector3(-116.2294f, -7.010099f, -40.55866f));

            uint FerryNpc = 1019006;

            var unit = GameObjectManager.GetObjectByNPCId(FerryNpc);

            if (!unit.IsWithinInteractRange)
            {
                var target = unit.Location;
                Navigator.PlayerMover.MoveTowards(target);
                while (!unit.IsWithinInteractRange)
                {
                    Navigator.PlayerMover.MoveTowards(target);
                    await Coroutine.Sleep(100);
                }

                Navigator.PlayerMover.MoveStop();
            }

            unit.Target();
            unit.Interact();

            await Coroutine.Wait(5000, () => DialogOpen);

            if (DialogOpen)
            {
                Next();
            }

            await Coroutine.Wait(3000, () => SelectString.IsOpen);
        }

        public static async Task CloseHousingWards()
        {
            if (HousingSelectBlock.Instance.IsOpen)
            {
                HousingSelectBlock.Instance.Close();

                await Coroutine.Wait(5000, () => SelectString.IsOpen);

                if (SelectString.IsOpen)
                {
                    SelectString.ClickSlot((uint)(SelectString.LineCount - 1));
                    await Coroutine.Wait(5000, () => !SelectString.IsOpen);
                }

                await Coroutine.Sleep(500);

                if (CommonBehaviors.IsLoading)
                {
                    await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
                }
            }
        }

        private static async Task CloseHousingWardsNoLoad()
        {
            if (HousingSelectBlock.Instance.IsOpen)
            {
                HousingSelectBlock.Instance.Close();

                await Coroutine.Wait(5000, () => SelectString.IsOpen);

                if (SelectString.IsOpen)
                {
                    SelectString.ClickSlot((uint)(SelectString.LineCount - 1));
                    await Coroutine.Wait(5000, () => !SelectString.IsOpen);
                }

                await Coroutine.Sleep(500);
            }
        }

        private static async Task<List<string>> HousingWards()
        {
            var output = new List<string>();
            if (HousingSelectBlock.Instance.IsOpen)
            {
                for (var i = 0; i < HousingSelectBlock.Instance.NumberOfWards; i++)
                {
                    HousingSelectBlock.Instance.SelectWard(i);

                    await Coroutine.Sleep(500);

                    //Log.Information($"Ward {AgentHousingSelectBlock.Instance.WardNumber + 1}");
                    var plotStatus = AgentHousingSelectBlock.Instance.ReadPlots(HousingSelectBlock.Instance.NumberOfPlots);

                    for (var j = 0; j < plotStatus.Length; j++)
                    {
                        if (plotStatus[j] == 0)
                        {
                            var price = HousingSelectBlock.Instance.PlotPrice(j);
                            var size = "";

                            var bytes = Encoding.ASCII.GetBytes(HousingSelectBlock.Instance.PlotString(j).Split(' ')[1]);
                            if (bytes.Length > 9)
                            {
                                switch (bytes[9])
                                {
                                    case 72:
                                        size = " (Small) ";
                                        break;
                                    case 1:
                                        size = " (Medium) ";
                                        break;
                                    case 2:
                                        size = " (Large) ";
                                        break;
                                }
                            }

                            //Log.Information($"{HousingSelectBlock.Instance.HousingWard} Plot {j+1} {size} -  {price}");
                            output.Add($"{HousingSelectBlock.Instance.HousingWard} Plot {j + 1} {size} -  {price}");
                        }
                    }

                    await Coroutine.Sleep(200);
                }
            }

            return output;
        }
    }
}