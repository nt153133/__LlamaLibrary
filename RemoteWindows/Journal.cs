using System.Linq;
using ff14bot.Managers;

namespace LlamaLibrary.RemoteWindows;

public class Journal : RemoteWindow<Journal>
{
    public Journal() : base("Journal")
    {
    }

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