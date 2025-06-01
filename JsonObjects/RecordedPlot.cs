using System;
using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.RemoteWindows;
using ff14bot.ServiceClient;
using LlamaLibrary.Enums;
using LlamaLibrary.Extensions;
using LlamaLibrary.Helpers;
using LlamaLibrary.Helpers.Housing;
using LlamaLibrary.Helpers.NPC;
using LlamaLibrary.Logging;
using Newtonsoft.Json;

namespace LlamaLibrary.JsonObjects;

public class RecordedPlot : IEquatable<RecordedPlot>
{
    private static LLogger Log = new("RecordedPlot");

    [JsonIgnore]
    public const uint PlacardNpcId = 2002736;

    [JsonIgnore]
    public const uint ApartmentEntranceNpcId = 2007402;

    [JsonIgnore]
    public const uint HouseEntranceNpcId = 2002737;

    public HousingZone HousingZone { get; set; }
    public int Plot { get; set; }
    public PlotSize Size { get; set; }
    public Vector3 PlacardLocation { get; set; }
    public Vector3 EntranceLocation { get; set; }
    public Vector3 CenterLocation { get; set; }
    public float Radius { get; set; }
    public bool IsInSubdivision { get; set; }

    [JsonIgnore]
    public bool IsApartment => Plot > 60;

    [JsonIgnore]
    public BoundingCircle BoundingCircle => new BoundingCircle { Center = CenterLocation, Radius = Radius };

    [JsonIgnore]
    public Npc Npc => new(2002736, (ushort)HousingZone, PlacardLocation);

    [JsonIgnore]
    public GameObject? EntranceObj => GameObjectManager.GetObjectsByNPCIds<EventObject>(new[] { HouseEntranceNpcId, ApartmentEntranceNpcId }).FirstOrDefault(i => BoundingCircle.ContainsIgnoreZ(i.Location));

    public RecordedPlot(HousingZone housingZone, int plot, bool isInSubdivision)
    {
        HousingZone = housingZone;
        Plot = plot;
        IsInSubdivision = isInSubdivision;
    }

    public RecordedPlot(HousingZone housingZone, Vector3 placardLocation, bool isInSubdivision)
    {
        HousingZone = housingZone;
        PlacardLocation = placardLocation;
        IsInSubdivision = isInSubdivision;
    }

    public RecordedPlot()
    {
    }

    public async Task<bool> Enter()
    {
        var entrance = EntranceObj;

        if (entrance == null)
        {
            Log.Error($"Entrance null for {HousingZone} {Plot}");
            return false;
        }

        if (!entrance.IsWithinInteractRange)
        {
            Log.Information("Moving to entrance");
            if (!await Navigation.FlightorMove(entrance.Location, () => entrance.IsWithinInteractRange))
            {
                Log.Error($"Can't get to entrance {HousingZone} {Plot}");
                return false;
            }
        }

        Navigator.NavigationProvider.ClearStuckInfo();
        Navigator.Stop();
        await Coroutine.Wait(5000, () => !GeneralFunctions.IsJumping);
        entrance.Interact();

        switch (entrance.NpcId)
        {
            case HouseEntranceNpcId:
            {
                Log.Information($"Entering house {HousingZone} W{HousingHelper.CurrentWard}P{Plot}");

                if (await Coroutine.Wait(10000, () => SelectYesno.IsOpen))
                {
                    SelectYesno.Yes();
                }
                else
                {
                    if (!await Navigation.FlightorMove(entrance.Location, 2f))
                    {
                        Log.Error($"Can't get to entrance {HousingZone} {Plot}");
                        return false;
                    }

                    entrance.Interact();
                    if (!await Coroutine.Wait(10000, () => SelectYesno.IsOpen))
                    {
                        Log.Error($"Can't get to entrance {HousingZone} {Plot}");
                        return false;
                    }
                }

                if (SelectYesno.IsOpen)
                {
                    SelectYesno.Yes();
                }

                break;
            }

            case ApartmentEntranceNpcId:
            {
                Log.Information("Entering apartment");

                await Coroutine.Wait(10000, () => SelectString.IsOpen);
                if (SelectString.IsOpen)
                {
                    SelectString.ClickSlot(0);
                }

                if (!await Coroutine.Wait(5000, () => RaptureAtkUnitManager.GetWindowByName("MansionSelectRoom") != null))
                {
                    Log.Error("Failed to enter apartment selection window");
                    return false;
                }

                return true;
            }
        }

        if (await Coroutine.Wait(10000, () => CommonBehaviors.IsLoading))
        {
            await CommonTasks.HandleLoading();
        }

        return HousingHelper.IsInsideHouse;
    }

    public bool Equals(RecordedPlot? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return HousingZone == other.HousingZone && Plot == other.Plot && PlacardLocation.Equals(other.PlacardLocation) && IsInSubdivision == other.IsInSubdivision;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((RecordedPlot)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(HousingZone, Plot, PlacardLocation, IsInSubdivision);
    }

    public override string ToString()
    {
        return $"HousingZone: {HousingZone}, Plot: {Plot}, Size: {Size}, PlacardLocation: {PlacardLocation}, EntranceLocation: {EntranceLocation}, IsInSubdivision: {IsInSubdivision}";
    }
}