namespace LlamaLibrary.RemoteWindows;

/// <summary>
/// Represents The Finer Miner game window, allowing interaction with the mining mini-game in Final Fantasy XIV.
/// </summary>
public class MiniGameMiner : RemoteWindow<MiniGameMiner>
{
    public MiniGameMiner() : base("MiniGameMiner")
    {
    }

    public void PressButton()
    {
        //MiniGameMiner, true, (ValueType.Int, 0xB),(ValueType.Int, 0x0),(ValueType.Int, 0x0)
        SendAction(true, 0xB, 0, 0);
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
        SendAction(true, 0xF);
    }

    /// <summary>
    /// Alias for <see cref="PauseCursor"/> — resumes the oscillating cursor on the gauge.
    /// Both methods send the same action and are functionally identical.
    /// </summary>
    public void ResumeCursor()
    {
        SendAction(true, 0xF);
    }
}