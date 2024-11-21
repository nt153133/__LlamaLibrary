using System;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot.Enums;
using ff14bot.Managers;
using LlamaLibrary.Extensions;
using LlamaLibrary.RemoteAgents;

namespace LlamaLibrary.RemoteWindows;

public class MateriaAttach : RemoteWindow<MateriaAttach>
{
    public MateriaAttach() : base("MateriaAttach")
    {
    }

    public void ClickItem(int index)
    {
        SendAction(4, 3uL, 1, 3, (ulong)index, 3, 1, 3, 0);
    }

    public void ClickMateria(int index)
    {
        SendAction(3, 3uL, 2, 3, (ulong)index, 3, 1);
    }

    public void ChangeGearDropDown(int index)
    {
        if (index < 0 || index > 6)
        {
            throw new ArgumentOutOfRangeException();
        }

        if (AgentMeld.Instance.SelectedCategory == index)
        {
            return;
        }

        SendAction(2, 3, 0, 3, (ulong)index);
    }

    public void SelectInventory()
    {
        ChangeGearDropDown(0);
    }

    //1 = MainHand/Offhand
    //2 = Head/Body/Hands
    //3 = Legs/Feet
    //4 = Neck/Ears
    //5 = Wrists/Rings
    //6 = Equipped Items

    public void SelectMainHandOffHand()
    {
        ChangeGearDropDown(1);
    }

    public void SelectHeadBodyHands()
    {
        ChangeGearDropDown(2);
    }

    public void SelectLegsFeet()
    {
        ChangeGearDropDown(3);
    }

    public void SelectNeckEars()
    {
        ChangeGearDropDown(4);
    }

    public void SelectWristsRings()
    {
        ChangeGearDropDown(5);
    }

    public void SelectEquippedItems()
    {
        ChangeGearDropDown(6);
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

    public async Task<bool> OpenMateriaAttachDialog(BagSlot item)
    {
        if (!Instance.IsOpen)
        {
            item.OpenMeldInterface();
            if (!await Coroutine.Wait(5000, () => Instance.IsOpen))
            {
                return false;
            }
        }

        if (MateriaAttachDialog.Instance.IsOpen)
        {
            return true;
        }

        switch (item.BagId)
        {
            case InventoryBagId.Bag1:
            case InventoryBagId.Bag2:
            case InventoryBagId.Bag3:
            case InventoryBagId.Bag4:
            case InventoryBagId.Bag5:
            case InventoryBagId.Bag6:
                SelectInventory();
                await Coroutine.Wait(5000, () => AgentMeld.Instance.SelectedCategory == 0);
                break;
            case InventoryBagId.EquippedItems:
                SelectEquippedItems();
                await Coroutine.Wait(5000, () => AgentMeld.Instance.SelectedCategory == 6);
                break;
            case InventoryBagId.Armory_MainHand:
            case InventoryBagId.Armory_OffHand:
                SelectMainHandOffHand();
                await Coroutine.Wait(5000, () => AgentMeld.Instance.SelectedCategory == 1);
                break;
            case InventoryBagId.Armory_Helmet:
            case InventoryBagId.Armory_Chest:
            case InventoryBagId.Armory_Glove:
                SelectHeadBodyHands();
                await Coroutine.Wait(5000, () => AgentMeld.Instance.SelectedCategory == 2);
                break;
            case InventoryBagId.Armory_Pants:
            case InventoryBagId.Armory_Boots:
                SelectLegsFeet();
                await Coroutine.Wait(5000, () => AgentMeld.Instance.SelectedCategory == 3);
                break;
            case InventoryBagId.Armory_Earrings:
            case InventoryBagId.Armory_Necklace:
                SelectNeckEars();
                await Coroutine.Wait(5000, () => AgentMeld.Instance.SelectedCategory == 4);
                break;
            case InventoryBagId.Armory_Writs:
            case InventoryBagId.Armory_Rings:
                SelectWristsRings();
                await Coroutine.Wait(5000, () => AgentMeld.Instance.SelectedCategory == 5);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await Coroutine.Sleep(1000);

        var itemIndex = AgentMeld.Instance.ItemIndex(item);

        if (itemIndex == -1)
        {
            ff14bot.Helpers.Logging.WriteDiagnostic("Item not found in meld list");
            return false;
        }

        Instance.ClickItem(itemIndex);

        if (!await Coroutine.Wait(5000, () => AgentMeld.Instance.IndexOfSelectedItem == itemIndex))
        {
            ff14bot.Helpers.Logging.WriteDiagnostic("Item not selected");
            return false;
        }

        await Coroutine.Sleep(500);

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