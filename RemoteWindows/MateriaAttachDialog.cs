using System.Threading.Tasks;
using Buddy.Coroutines;
using LlamaLibrary.RemoteAgents;

namespace LlamaLibrary.RemoteWindows;

public class MateriaAttachDialog : RemoteWindow<MateriaAttachDialog>
{
    public MateriaAttachDialog() : base("MateriaAttachDialog")
    {
    }

    public int MeldChance
    {
        get
        {
#if RB_DT
            return Elements[41].TrimmedData;
#else
            return Elements[43].TrimmedData;
#endif
        }
    }

    public void ClickAttach()
    {
        SendAction(1, 3, 0);
    }

    public void ClickCancel()
    {
        SendAction(1, 3, 1);
    }
}