using System.Linq;
using ff14bot.Managers;

namespace LlamaLibrary.RemoteWindows;

/// <summary>
/// Interaction interface for the "Journal" window.
/// Used to view active, completed, and current quests.
/// </summary>
public class Journal : RemoteWindow<Journal>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Journal"/> class.
    /// </summary>
    public Journal() : base("Journal")
    {
    }

    /// <summary>
    /// Selects a quest in the journal by its global ID.
    /// </summary>
    /// <param name="globalId">The global identifier of the quest to select.</param>
    public void SelectQuest(int globalId)
    {
        var rawId = QuestLogManager.ActiveQuests.FirstOrDefault(i => i.GlobalId == globalId)?.RawId ?? 0;
        if (rawId == 0)
        {
            return;
        }

        SendAction(3, 3, 0xD, 3, rawId, 3, 1);
    }
}