using System.Collections.Generic;

namespace LlamaLibrary.RemoteWindows;

public class MateriaAttachDialog : RemoteWindow<MateriaAttachDialog>
{
    public static readonly Dictionary<string, int> Properties = new(System.StringComparer.Ordinal)
    {
        { "MeldChance", 41 },
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
