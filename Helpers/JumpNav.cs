using System;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Common;
using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Helpers;

/// <summary>
/// Encapsulates a jump navigation route between two world positions.
/// Moves the player to a start zone and executes a movement-jump to reach an elevated end position,
/// useful for traversing geometry that the pathfinder cannot navigate directly.
/// </summary>
public class JumpNav
{
    public static readonly LLogger Log = new("Jump", Colors.AliceBlue);

    /// <summary>The world position marking the beginning of the jump route.</summary>
    public Vector3 Start { get; }

    /// <summary>The world position marking the intended landing spot at the end of the jump.</summary>
    public Vector3 End { get; }

    private Vector2[] JumpStartSquare { get; }
    private Vector2[] JumpEndSquare { get; }

    /// <summary>
    /// Initializes a new <see cref="JumpNav"/> with the specified start and end positions.
    /// </summary>
    /// <param name="start">World position from which to begin the jump.</param>
    /// <param name="end">Target world position to land at after jumping.</param>
    public JumpNav(Vector3 start, Vector3 end)
    {
        Start = start;
        End = end;
        JumpStartSquare = CreateSquare(new Vector2(start.X, start.Z), 1.5f);
        JumpEndSquare = CreateSquare(new Vector2(end.X, end.Z), 1.5f);
    }

    /// <summary>Returns <see langword="true"/> if the local player is currently within the jump start zone.</summary>
    public bool IsAtStart() => MathEx.IsPointInPoly(Core.Me.Location, JumpStartSquare);

    /// <summary>Returns <see langword="true"/> if the local player is currently within the jump end zone.</summary>
    public bool IsAtEnd() => MathEx.IsPointInPoly(Core.Me.Location, JumpEndSquare);

    /// <summary>Navigates the player to the start zone of this jump route.</summary>
    /// <returns><see langword="true"/> if the player reaches the start zone; otherwise <see langword="false"/>.</returns>
    public async Task<bool> MoveToStart()
    {
        if (IsAtStart())
        {
            return true;
        }

        await Navigation.FlightorMove(Start, 1f);
        return IsAtStart();
    }

    /// <summary>Navigates the player to the end zone of this jump route.</summary>
    /// <returns><see langword="true"/> if the player reaches the end zone; otherwise <see langword="false"/>.</returns>
    public async Task<bool> MoveToEnd()
    {
        if (IsAtEnd())
        {
            return true;
        }

        await Navigation.FlightorMove(End, 1f);
        return IsAtEnd();
    }

    /// <summary>
    /// Executes the full jump sequence: moves to the start zone, faces the end position,
    /// presses the movement-forward and jump inputs, then waits to land near the end zone.
    /// </summary>
    /// <returns><see langword="true"/> if the player successfully lands in the end zone; otherwise <see langword="false"/>.</returns>
    public async Task<bool> Jump()
    {
        if (IsAtEnd())
        {
            return true;
        }

        if (!await MoveToStart())
        {
            Log.Error("Not in correct start area");
            return false;
        }

        var oldHeading = Core.Me.Heading;
        Core.Me.Face(End);

        await Coroutine.Wait(1000, () => Math.Abs(Core.Me.Heading - oldHeading) > 0.1f);

        MovementManager.MoveForwardStart();

        await Coroutine.Sleep(200);

        MovementManager.Jump();

        if (!await Coroutine.Wait(1000, () => GeneralFunctions.IsJumping))
        {
            Log.Error("Failed to jump");
            return false;
        }

        await Coroutine.Wait(7000, () => !GeneralFunctions.IsJumping);

        MovementManager.MoveStop();

        if (await MoveToEnd())
        {
            return IsAtEnd();
        }

        Log.Error("Not in correct area");
        return false;
    }

    /// <summary>
    /// Creates a square polygon centered at <paramref name="center"/> with the given half-extent <paramref name="radius"/>.
    /// Used to define start/end zone polygons for <see cref="IsAtStart"/> and <see cref="IsAtEnd"/> checks.
    /// </summary>
    /// <param name="center">The 2D center point (X/Z plane) of the square.</param>
    /// <param name="radius">Half-width of the square (distance from center to each edge).</param>
    /// <returns>A four-vertex <see cref="Vector2"/> array representing the square's corners.</returns>
    public static Vector2[] CreateSquare(Vector2 center, float radius)
    {
        Vector2[] square = new Vector2[4];
        square[0] = new Vector2(center.X - radius, center.Y - radius);
        square[1] = new Vector2(center.X + radius, center.Y - radius);
        square[2] = new Vector2(center.X + radius, center.Y + radius);
        square[3] = new Vector2(center.X - radius, center.Y + radius);
        return square;
    }
}