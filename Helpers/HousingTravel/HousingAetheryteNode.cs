using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Pathing;
using ff14bot.RemoteAgents;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers.Housing;

namespace LlamaLibrary.Helpers.HousingTravel.Districts;

public class HousingAetheryteNode : NavGraph.CustomLogicNode
{
    HousingAetheryte Source { get; }
    HousingAetheryte Target { get; }

    public HousingAetheryteNode(HousingAetheryte source, HousingAetheryte target) : base()
    {
        Source = source;
        Target = target;
    }

    public override bool ShouldPop =>
        WorldManager.ZoneId == ZoneId &&
        HousingHelper.IsInHousingArea &&
        Core.Me.Location.Distance2D(EndLocation) < 8f &&
        !MovementManager.IsMoving;


    public override async Task<bool> Logic()
    {
        if (ShouldPop)
            return true;

        if (!Navigator.InPosition(Core.Me.Location, Source.Location, 3f))
        {
            await CommonTasks.MoveAndStop(new MoveToParameters(Source.Location), 3f, true);
            return true;
        }

        var ae = GameObjectManager.GetObjectByNPCId(Source.NpcId);
        if (ae == null)
            return false;

        ae.Target();
        ae.Interact();

        if (!await Coroutine.Wait(5000, () => TelepotTown.IsOpen))
            return false;

        AgentTelepotTown.Instance.TeleportByAetheryteId(Target.Key);

        await Coroutine.Wait(-1, () => CommonBehaviors.IsLoading);
        await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
        await Coroutine.Sleep(1000);

        Navigator.Stop();
        return true;
    }

}