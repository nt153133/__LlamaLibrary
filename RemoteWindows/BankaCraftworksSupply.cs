using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot.Managers;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers;
using LlamaLibrary.RemoteAgents;

namespace LlamaLibrary.RemoteWindows;

public class BankaCraftworksSupply : RemoteWindow<BankaCraftworksSupply>
{
    public BankaCraftworksSupply() : base("BankaCraftworksSupply")
    {
    }

    public static readonly Dictionary<string, int> Properties = new(System.StringComparer.Ordinal)
    {
        {
            "TurnInItemId",
            8
        },
        {
            "EsteemLevel",
            6
        },
        {
            "Esteem",
            7
        },
    };

    public int TurnInItemId => Elements[Properties["TurnInItemId"]].Int;

    public uint TurnInItemRawItemId => Elements[Properties["TurnInItemId"]].UInt % 500_000;

    public int EsteemLevel => Elements[Properties["EsteemLevel"]].Int;

    public int Esteem => Elements[Properties["Esteem"]].Int;

    public void Deliver()
    {
        SendAction(1, 3, 0);
    }

#if RB_DT
    public async Task HandOverItems()
    {
        if (HelpWindow.Instance.IsOpen)
        {
            HelpWindow.Instance.Close();
            await Coroutine.Wait(5000, () => HelpWindow.Instance.IsOpen);
        }

        var slots = InventoryManager.FilledSlots.Where(i => i.TrueItemId == TurnInItemId).OrderByDescending(i => i.Collectability).Take(6 - Esteem);

        foreach (var slot in slots)
        {
            if (slot != null)
            {
                AgentBankaCraftworksSupply.Instance.HandIn(slot);
                await Coroutine.Sleep(700);
            }
        }

        await Coroutine.Sleep(500);

        Deliver();

        await Coroutine.Wait(5000, () => Talk.DialogOpen || SelectYesno.IsOpen);

        if (SelectYesno.IsOpen)
        {
            SelectYesno.Yes();
            await Coroutine.Wait(5000, () => Talk.DialogOpen);
        }

        await GeneralFunctions.SmallTalk(5000);

        /*await Coroutine.Wait(10000, () => QuestLogManager.InCutscene);

        while (QuestLogManager.InCutscene)
        {
            AgentCutScene.Instance.PromptSkip();
            if (AgentCutScene.Instance.CanSkip && SelectString.IsOpen)
            {
                SelectString.ClickSlot(0);
            }

            await Coroutine.Yield();
        }

        await Coroutine.Wait(20000, () => JournalResult.IsOpen || Talk.DialogOpen);

        if (Talk.DialogOpen)
        {
            await GeneralFunctions.SmallTalk(1000);
            await Coroutine.Wait(20000, () => JournalResult.IsOpen);
        }

        if (JournalResult.IsOpen)
        {
            JournalAccept.Accept();
            await Coroutine.Wait(20000, () => !JournalResult.IsOpen);

            await GeneralFunctions.SmallTalk(5000);
        }
        */
    }
#endif
}