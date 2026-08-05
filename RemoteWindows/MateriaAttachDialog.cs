using System.Collections.Generic;

namespace LlamaLibrary.RemoteWindows;

public class MateriaAttachDialog : RemoteWindow<MateriaAttachDialog>
{
    public static readonly Dictionary<string, int> Properties = new(System.StringComparer.Ordinal)
    {
#if RB_DT
        { "MeldChance", 41 },
#else
        { "MeldChance", 43 },
#endif
    };

    public MateriaAttachDialog() : base("MateriaAttachDialog")
    {
    }

    public int MeldChance => Elements[Properties["MeldChance"]].Int;

    public void ClickAttach()
    {
        SendAction(1, 3, 0);
    }

    public void ClickCancel()
    {
        SendAction(1, 3, 1);
    }
}