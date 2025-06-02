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
using ff14bot.Navigation;
using ff14bot.NeoProfiles;
using ff14bot.Objects;
using ff14bot.RemoteAgents;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers.Housing;
using LlamaLibrary.Helpers.NPC;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteWindows;
// ReSharper disable MemberCanBeProtected.Global

namespace LlamaLibrary.Helpers.HousingTravel.Districts
{
    public class ResidentialDistrict<T> : ResidentialDistrict
        where T : ResidentialDistrict<T>, new()
    {
        private static T? _instance;
        public static T Instance => _instance ??= new T();

        protected ResidentialDistrict()
        {
        }
    }

    public abstract class ResidentialDistrict
    {
        private static readonly LLogger Log = new(nameof(ResidentialDistrict), Colors.Pink);
        private Npc? _aetheryteNpc;
        public virtual string Name { get; } = "";
        public virtual ushort ZoneId => 0;
        public virtual uint TownAetheryteId { get; }
        public virtual int RequiredQuest { get; }

        public Npc AetheryteNpc
        {
            get => _aetheryteNpc ?? new Npc(TownAetheryteId, ZoneId, TownAetheryteLocation, RequiredQuest);
            set => _aetheryteNpc = value;
        }

        public virtual Vector3 TownAetheryteLocation { get; } = Vector3.Zero;
        public virtual bool OffMesh { get; } = false;

        public virtual List<HousingAetheryte> Aetherytes => new();

        public virtual List<Vector3> TransitionStartLocations => new();

        public virtual List<Npc> TransitionNpcs => new();

        public virtual List<Vector3> TransitionEndLocations => new();
        public bool HasAetheryte => ConditionParser.HasAetheryte(TownAetheryteId);
        public bool QuestComplete => ConditionParser.IsQuestCompleted(RequiredQuest);

        public async Task<bool> OpenWardSelection()
        {
            if (WorldManager.ZoneId == ZoneId)
            {
                if (!await GetToWardTransition())
                {
                    Log.Error("Failed GetToWardTransition");
                    return false;
                }
            }
            else if (QuestComplete)
            {
                if (!await GetToResidentialAe())
                {
                    Log.Error("Failed GetToResidentialAe");
                    return false;
                }
            }
            else
            {
                if (!await WalkToResidential())
                {
                    return false;
                }
            }

            if (!SelectString.IsOpen)
            {
                Log.Error("Select string not open");
                return false;
            }

            return await SelectHousing();
        }

        public async Task<bool> SelectWard(int ward)
        {
            if (ward is < 1 or > 30)
            {
                Log.Error($"Invalid ward {ward}");
                return false;
            }

            if (HousingHelper.IsInHousingArea && ZoneId == WorldManager.ZoneId && HousingHelper.HousingPositionInfo.Ward == ward)
            {
                return true;
            }

            if (!HousingSelectBlock.Instance.IsOpen)
            {
                //Log.Information($"Not open");
                if (!await OpenWardSelection())
                {
                    Log.Error($"Can't open ward selection {ward}");
                    return false;
                }
            }

            //Log.Information($"Selecting ward {ward}");
            HousingSelectBlock.Instance.SelectWard(ward - 1);

            await Coroutine.Sleep(500);

            if (HousingSelectBlock.Instance.WindowByName == null)
            {
                return false;
            }

            HousingSelectBlock.Instance.WindowByName.SendAction(2, 3, 0, 0, 0x10);

            if (!await Coroutine.Wait(5000, () => SelectYesno.IsOpen))
            {
                return false;
            }

            SelectYesno.Yes();

            if (!await Coroutine.Wait(30000, () => CommonBehaviors.IsLoading))
            {
                return false;
            }

            return await Coroutine.Wait(new TimeSpan(0, 2, 30), () => !CommonBehaviors.IsLoading);
        }

        public async Task<bool> CloseWardSelection()
        {
            if (QuestComplete)
            {
                return await CloseHousingWardsNoLoad();
            }

            return await CloseHousingWards();
        }

        public virtual Task<bool> WalkToResidential()
        {
            return Task.FromResult(false);
        }

        public virtual async Task<bool> GetToWardTransition()
        {
            if (TransitionNpcs.Count != 0)
            {
                var npc = NpcHelper.GetClosestNpc(TransitionNpcs);

                if (npc == null)
                {
                    Log.Error("Failed to find npc");
                    return false;
                }

                if (!await TravelWithinZone(npc.Location.Coordinates))
                {
                    Log.Error($"Could not get to transition npc {npc}");
                    return false;
                }

                var gameObject = npc.GameObject;

                if (gameObject == null)
                {
                    Log.Error($"Could not find transition npc {npc}");
                    return false;
                }

                gameObject.Target();
                gameObject.Interact();

                if (!await Coroutine.Wait(5000, () => Talk.DialogOpen || Conversation.IsOpen))
                {
                    Log.Error($"Could not interact with npc {npc}");
                    return false;
                }

                await DealWithTalk();

                if (!await Coroutine.Wait(5000, () => Conversation.IsOpen))
                {
                    Log.Error($"Could not interact with npc (post talk) {npc}");
                    return false;
                }
            }
            else
            {
                var meLocation = Core.Me.Location;
                var closestTransition = TransitionStartLocations.OrderBy(i => i.Distance2DSqr(meLocation)).First();

                if (!await TravelWithinZone(closestTransition))
                {
                    Log.Error($"Could not get to transition location {closestTransition}");
                    return false;
                }

                while (!Conversation.IsOpen)
                {
                    Navigator.PlayerMover.MoveTowards(TransitionEndLocations.OrderBy(i => i.Distance2DSqr(closestTransition)).First());
                    await Coroutine.Sleep(50);

                    //Navigator.PlayerMover.MoveStop();
                }

                Navigator.PlayerMover.MoveStop();
                if (!await Coroutine.Wait(5000, () => Conversation.IsOpen))
                {
                    Log.Error("Could not get to select string");
                    return false;
                }
            }

            return Conversation.IsOpen;
        }

        public async Task<bool> GetToResidentialAe()
        {
            if (!HasAetheryte)
            {
                return false;
            }

            var unit = await Navigation.GetToAE(TownAetheryteId);

            if (unit is null)
            {
                return false;
            }

            unit.Target();
            unit.Interact();

            if (!await Coroutine.Wait(5000, () => SelectString.IsOpen))
            {
                unit = await Navigation.GetToAE(TownAetheryteId);
                if (unit is null)
                {
                    return false;
                }

                unit.Target();
                unit.Interact();
                await Coroutine.Wait(5000, () => SelectString.IsOpen);
            }

            if (!SelectString.IsOpen)
            {
                return false;
            }

            SelectString.ClickLineContains(Translator.ResidentialDistrictAethernet);

            return await Coroutine.Wait(5000, () => SelectString.IsOpen && SelectString.Lines().Any(i => i.Contains(Translator.SelectWard)));

            //return await Coroutine.Wait(5000, () => SelectString.IsOpen);
        }

        private static async Task<bool> SelectHousing()
        {
            if (!SelectString.IsOpen)
            {
                Log.Error("No Select String open ward");
                return false;
            }

            if (!SelectString.ClickLineContains(Translator.SelectWard))
            {
                Log.Error($"Could not select line {Translator.SelectWard}");
                return false;
            }

            return await Coroutine.Wait(5000, () => HousingSelectBlock.Instance.IsOpen);
        }

        public static async Task<bool> CloseHousingWards()
        {
            if (!HousingSelectBlock.Instance.IsOpen)
            {
                return true;
            }

            HousingSelectBlock.Instance.Close();

            await Coroutine.Wait(5000, () => Conversation.IsOpen);

            if (Conversation.IsOpen)
            {
                Conversation.SelectQuit();
                await Coroutine.Wait(5000, () => !Conversation.IsOpen);
            }

            await Coroutine.Sleep(500);

            if (CommonBehaviors.IsLoading)
            {
                await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
            }

            return !HousingSelectBlock.Instance.IsOpen;
        }

        private static async Task<bool> CloseHousingWardsNoLoad()
        {
            if (!HousingSelectBlock.Instance.IsOpen)
            {
                return true;
            }

            HousingSelectBlock.Instance.Close();

            await Coroutine.Wait(5000, () => SelectString.IsOpen);

            if (Conversation.IsOpen)
            {
                Conversation.SelectQuit();
                await Coroutine.Wait(5000, () => !Conversation.IsOpen);
            }

            return !HousingSelectBlock.Instance.IsOpen;
        }

        public HousingAetheryte? ClosestHousingAetheryte(Vector3 location)
        {
            return Aetherytes.OrderBy(i => i.Location.Distance2DSqr(location)).FirstOrDefault();
        }

        public bool ShouldUseAethernet(Vector3 location)
        {
            if (WorldManager.ZoneId != ZoneId)
            {
                return false;
            }

            if (!HousingHelper.IsInHousingArea || HousingHelper.IsInsideHouse)
            {
                return false;
            }

            /*
            Log.Information($"ClosestHousingAetheryte dist: {ClosestHousingAetheryte(Core.Me.Location).Location.Distance(Core.Me.Location)}");

            Log.Information($"location dist: {location.Distance(Core.Me.Location)}");

            Log.Information($"ClosestHousingAetheryte Location dist: {ClosestHousingAetheryte(location).Location.Distance(Core.Me.Location)}");
            */
            var closestAetheryte = ClosestHousingAetheryte(location);
            var closestAetheryteMe = ClosestHousingAetheryte(Core.Me.Location);

            if (closestAetheryte is null || closestAetheryteMe is null)
            {
                return false;
            }

            if (closestAetheryte.Subdivision != closestAetheryteMe.Subdivision)
            {
                return true;
            }

            if (!closestAetheryte.Equals(closestAetheryteMe))
            {
                return closestAetheryteMe.Location.Distance(Core.Me.Location) < location.Distance(Core.Me.Location);
            }

            return false;
        }

        public async Task<bool> TravelWithinZone(Vector3 destination, float stopDistance = 3)
        {
            if (WorldManager.ZoneId != ZoneId)
            {
                return false;
            }

            if (ShouldUseAethernet(destination))
            {
                var closestAe = ClosestHousingAetheryte(Core.Me.Location);

                if (closestAe is null)
                {
                    return false;
                }

                //Log.Information($"{closestAe}");

                Log.Information($"Using Aetheryte {closestAe.Name}");
                try
                {
                    if (!await Navigation.GroundMove(closestAe.Location, 1f))
                    {
                        Log.Error("Couldn't get to ae");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                    return false;
                }

                var ae = GameObjectManager.GetObjectByNPCId(closestAe.NpcId);

                if (ae == null)
                {
                    Log.Error($"Couldn't find ae {closestAe}");
                    return false;
                }

                ae.Target();
                ae.Interact();
                if (!await Coroutine.Wait(5000, () => TelepotTown.IsOpen))
                {
                    Log.Error($"Open ae window {closestAe}");
                    return false;
                }

                AgentTelepotTown.Instance.TeleportByAetheryteId(ClosestHousingAetheryte(destination)!.Key);

                await Coroutine.Wait(-1, () => CommonBehaviors.IsLoading);
                await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
                await Coroutine.Sleep(1000);
            }

            Log.Information($"Moving to destination within distance {stopDistance}");
            await Navigation.GroundMove(destination, stopDistance);

            return Core.Me.Location.Distance(destination) < 5;
        }

        /*
        public async Task<List<PlotForSale>> HousingWards(int wardStart = 1, int wardEnd = 24)
        {
            var output = new List<PlotForSale>();
            if (HousingSelectBlock.Instance.IsOpen)
            {
                for (var i = wardStart-1; i < wardEnd; i++)
                {
                    HousingSelectBlock.Instance.SelectWard(i);

                    await Coroutine.Sleep(500);

                    //Log.Information($"Ward {AgentHousingSelectBlock.Instance.WardNumber + 1}");
                    var plotStatus = AgentHousingSelectBlock.Instance.ReadPlots(HousingSelectBlock.Instance.NumberOfPlots);

                    for (var j = 0; j < plotStatus.Length; j++)
                    {
                        if (plotStatus[j] == 0)
                        {
                            var price = int.Parse(GetNumbers(HousingSelectBlock.Instance.PlotPrice(j)));
                            var size = PlotSize.Small;

                            var bytes = Encoding.ASCII.GetBytes(HousingSelectBlock.Instance.PlotString(j).Split(' ')[1]);
                            if (bytes.Length > 9)
                            {
                                switch (bytes[9])
                                {
                                    case 72:
                                        size = PlotSize.Small;
                                        break;
                                    case 1:
                                        size = PlotSize.Medium;
                                        break;
                                    case 2:
                                        size = PlotSize.Large;
                                        break;
                                }
                            }

                            var plot = new HousingPlot(this.ToHousingDistrict(), i+1, j + 1, size);
                            output.Add(new PlotForSale(plot, price, false));
                            //Log.Information($"{HousingSelectBlock.Instance.HousingWard} Plot {j+1} {size} -  {price}");
                            //output.Add($"{HousingSelectBlock.Instance.HousingWard} Plot {j + 1} {size} -  {price}");
                        }
                    }

                    await Coroutine.Sleep(200);
                }
            }

            return output;
        }
        */

        private static string GetNumbers(string input)
        {
            return new string(input.Where(char.IsDigit).ToArray());
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
    }
}