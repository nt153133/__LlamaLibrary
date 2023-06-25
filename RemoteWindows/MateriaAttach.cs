using System.Threading.Tasks;
using Buddy.Coroutines;
using LlamaLibrary.RemoteAgents;

namespace LlamaLibrary.RemoteWindows;

public class MateriaAttach : RemoteWindow<MateriaAttach>
{
    public MateriaAttach() : base("MateriaAttach")
    {
    }

    public void ClickItem(int index)
    {
        SendAction(3, 3uL, 1, 3, (ulong)index, 3, 1);
    }

    public void ClickMateria(int index)
    {
        SendAction(3, 3uL, 2, 3, (ulong)index, 3, 1);
    }

    public void ChangeGearDropDown(int index)
    {
        SendAction(2, 3, 0, 3, (ulong)index);
    }

    public async Task<bool> OpenMateriaAttachDialog()
    {
        if (!Instance.IsOpen)
        {
            return false;
        }

        if (MateriaAttachDialog.Instance.IsOpen)
        {
            return true;
        }

        if (!await FindValidMateria())
        {
            return false;
        }

        await Coroutine.Wait(3000, () => AgentMeld.Instance.CanMeld || AgentMeld.Instance.Ready);
        return Instance.IsOpen;
    }

    public async Task<bool> FindValidMateria()
    {
        if (AgentMeld.Instance.IndexOfSelectedItem == 255)
        {
            return false;
        }

        if (AgentMeld.Instance.MateriaCount == 0)
        {
            return false;
        }

        var selectedItem = AgentMeld.Instance.IndexOfSelectedItem;

        for (var i = 0; i < AgentMeld.Instance.MateriaCount; i++)
        {
            Instance.ClickMateria(i);
            if (!await Coroutine.Wait(1500, () => MateriaAttachDialog.Instance.IsOpen))
            {
                continue;
            }

            if (MateriaAttachDialog.Instance.MeldChance != 0)
            {
                break;
            }

            MateriaAttachDialog.Instance.ClickCancel();
            await Coroutine.Wait(5000, () => !MateriaAttachDialog.Instance.IsOpen);
            /*
            await Coroutine.Wait(5000, () => AgentMeld.Instance.IndexOfSelectedItem != selectedItem);
            ff14bot.Helpers.Logging.WriteDiagnostic("need to click item");
            Instance.ClickItem(selectedItem);
            ff14bot.Helpers.Logging.WriteDiagnostic("clicked item");
            await Coroutine.Wait(5000, () => AgentMeld.Instance.IndexOfSelectedItem == selectedItem && AgentMeld.Instance.MateriaCount > 0);
            */
        }

        if (!MateriaAttachDialog.Instance.IsOpen)
        {
            return false;
        }

        return MateriaAttachDialog.Instance.IsOpen;
    }
}