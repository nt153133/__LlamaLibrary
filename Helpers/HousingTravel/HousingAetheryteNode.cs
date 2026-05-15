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

/// <summary>
/// A custom navigation graph node that teleports the player between two housing-district
/// aetheryte shards within the same residential zone.
/// </summary>
/// <remarks>
/// When the navigation system processes this node it moves to the <c>Source</c> aetheryte,
/// interacts with it to open the housing teleport window, and then travels to the <c>Target</c>
/// aetheryte — handling the loading screen automatically.
/// </remarks>
public class HousingAetheryteNode : NavGraph.CustomLogicNode
{
    HousingAetheryte Source { get; }
    HousingAetheryte Target { get; }

    /// <summary>
    /// Initialises a new <see cref="HousingAetheryteNode"/> that teleports from
    /// <paramref name="source"/> to <paramref name="target"/>.
    /// </summary>
    /// <param name="source">The aetheryte the player must interact with to initiate the teleport.</param>
    /// <param name="target">The aetheryte the player will be transported to.</param>
    public HousingAetheryteNode(HousingAetheryte source, HousingAetheryte target) : base()
    {
        Source = source;
        Target = target;
    }

    /// <summary>
    /// Gets a value indicating whether the node's goal has already been satisfied and the node
    /// should be popped from the navigation stack without executing its logic.
    /// </summary>
    /// <value>
    /// <see langword="true"/> when the player is in the correct zone, inside a housing area,
    /// within 8 yalms of the destination, and not currently moving.
    /// </value>
    public override bool ShouldPop =>
        WorldManager.ZoneId == ZoneId &&
        HousingHelper.IsInHousingArea &&
        Core.Me.Location.Distance2D(EndLocation) < 8f &&
        !MovementManager.IsMoving;


    /// <summary>
    /// Executes the aetheryte teleport: navigates to the source shard, interacts with it,
    /// selects the target via <c>AgentTelepotTown</c>, and waits for the loading screen.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the teleport completed successfully;
    /// <see langword="false"/> if the source NPC could not be found or the teleport window did not open.
    /// </returns>
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