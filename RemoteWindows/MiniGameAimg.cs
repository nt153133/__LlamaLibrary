using System;

namespace LlamaLibrary.RemoteWindows;
/// <summary>
/// In game window for Golden Saucer Minigame difficulty selection and cursor control. This is used for the Out on a Limb and The Finer Miner (MiniGameMiner)
/// </summary>
public class MiniGameAimg : RemoteWindow<MiniGameAimg>
{
    public MiniGameAimg() : base("MiniGameAimg")
    {
    }

    [Obsolete("Use SetDifficulty instead.")]
    public void PressButton()
    {
        //SendAction(3, 3, 0xB, 3, 2, 3, 0);
        SendAction(true, 0xB, 2, 0);
    }

    public void SetDifficulty(MiniGameDifficulty difficulty = MiniGameDifficulty.Titan)
    {
        SendAction(true, 0xB, (int)difficulty, 0);
    }

    public void PauseCursor()
    {
        SendAction(true, 0xF);
    }

    public void ResumeCursor()
    {
        SendAction(true, 0xF);
    }

    public enum MiniGameDifficulty
    {
        Cactuar = 0,
        Morbol = 1,
        Titan = 2,
    }
}