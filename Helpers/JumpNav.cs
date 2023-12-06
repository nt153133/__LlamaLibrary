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

public class JumpNav
{
    public static readonly LLogger Log = new("Jump", Colors.AliceBlue);
    public Vector3 Start { get; }
    public Vector3 End { get; }

    private Vector2[] JumpStartSquare { get; }
    private Vector2[] JumpEndSquare { get; }

    public JumpNav(Vector3 start, Vector3 end)
    {
        Start = start;
        End = end;
        JumpStartSquare = CreateSquare(new Vector2(start.X, start.Z), 1.5f);
        JumpEndSquare = CreateSquare(new Vector2(end.X, end.Z), 1.5f);
    }

    public bool IsAtStart() => MathEx.IsPointInPoly(Core.Me.Location, JumpStartSquare);

    public bool IsAtEnd() => MathEx.IsPointInPoly(Core.Me.Location, JumpEndSquare);

    public async Task<bool> MoveToStart()
    {
        if (IsAtStart())
        {
            return true;
        }

        await Navigation.FlightorMove(Start, 1f);
        return IsAtStart();
    }

    public async Task<bool> MoveToEnd()
    {
        if (IsAtEnd())
        {
            return true;
        }

        await Navigation.FlightorMove(End, 1f);
        return IsAtEnd();
    }

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