using System;
using System.Text;
using System.Text.RegularExpressions;
using ff14bot;

namespace LlamaLibrary.RemoteWindows;

/// <summary>
/// Represents the in-game UI window for the <c>Out on a Limb</c> mini-game
/// </summary>
/// <remarks>
/// <para>
/// This window is a thin wrapper around the game's UI element tree. It provides methods to
/// interact with the mini-game (pressing the swing button, pausing/resuming the cursor) and
/// exposes several live values read directly from the window's element data.
/// </para>
/// <para>
/// <b>Note:</b> several members on this class are marked <see cref="Obsolete"/> because equivalent
/// or better data is available from <see cref="Directors.OutOnALimbDirector"/>.
/// That class reads directly from the director's memory and does not depend on the game window
/// being open, making it more reliable for automation purposes.
/// </para>
/// </remarks>
public class MiniGameBotanist : RemoteWindow<MiniGameBotanist>
{
    private readonly Regex _timeRegex = new(@"(\d):(\d+).*", RegexOptions.Compiled);

    /// <summary>
    /// Initialises a new instance of <see cref="MiniGameBotanist"/>, binding to the game window
    /// named <c>"MiniGameBotanist"</c>.
    /// </summary>
    public MiniGameBotanist() : base("MiniGameBotanist")
    {
    }

    /// <summary>
    /// Triggers the primary swing action in the mini-game. Used to take a swing at the log when
    /// the cursor is in position.
    /// </summary>
    /// <remarks>
    /// Performs <c>SendAction</c> call. The caller is responsible for timing the
    /// call appropriately — the cursor is moving continuously and the sweet spot is invisible.
    /// Prefer calling this once per swing rather than in a tight loop without state checks.
    /// </remarks>
    public void PressButton()
    {
        SendAction(3, 3, 0xB, 3, 0, 3, 0);
    }

    /// <summary>
    /// Pauses the oscillating cursor on the gauge. The cursor will cease moving as long as this
    /// action is sent.
    /// </summary>
    /// <remarks>
    /// The cursor automatically resumes when the window closes or the round ends. Using both
    /// <see cref="PauseCursor"/> and <see cref="ResumeCursor"/> with the same action code is
    /// functionally identical — both map to the same internal action index.
    /// </remarks>
    public void PauseCursor()
    {
        SendAction(1, 3, 0xF);
    }

    /// <summary>
    /// Alias for <see cref="PauseCursor"/> — resumes the oscillating cursor on the gauge.
    /// Both methods send the same action and are functionally identical.
    /// </summary>
    public void ResumeCursor()
    {
        SendAction(1, 3, 0xF);
    }

    //[Obsolete("Use OutOnALimbDirector.SwingsRemaining")]
    public int GetNumberOfTriesLeft => IsOpen ? Elements[11].TrimmedData : 0;

    //[Obsolete("Use OutOnALimbDirector.CurrentProgress")]
    public int GetProgressLeft => IsOpen ? Elements[12].TrimmedData : 0;

    //[Obsolete("Use OutOnALimbDirector.MaxProgress")]
    public int GetProgressTotal => IsOpen ? Elements[13].TrimmedData : 0;

    [Obsolete]
    public int GetTimeLeft
    {
        get
        {
            var data = Core.Memory.ReadString((IntPtr)Elements[15].Data, Encoding.UTF8);

            if (!_timeRegex.IsMatch(data))
            {
                return 0;
            }

            var sec = int.Parse(_timeRegex.Match(data).Groups[2].Value.Trim());
            var min = int.Parse(_timeRegex.Match(data).Groups[1].Value.Trim());

            if (min > 0)
            {
                return 60 + sec;
            }

            return sec;
        }
    }
}