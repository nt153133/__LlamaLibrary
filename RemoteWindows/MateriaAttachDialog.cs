using System.Threading.Tasks;
using Buddy.Coroutines;
using LlamaLibrary.RemoteAgents;

namespace LlamaLibrary.RemoteWindows;

public class MateriaAttachDialog : RemoteWindow<MateriaAttachDialog>
{
    public MateriaAttachDialog() : base("MateriaAttachDialog")
    {
    }

    public int MeldChance => Elements[43].TrimmedData;

    public void ClickAttach()
    {
        SendAction(1, 3, 0);
    }

    public void ClickCancel()
    {
        SendAction(1, 3, 1);
    }
}