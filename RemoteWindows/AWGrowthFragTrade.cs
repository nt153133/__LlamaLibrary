using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers;
using LlamaLibrary.Helpers.NPC;
using LlamaLibrary.Logging;

namespace LlamaLibrary.RemoteWindows
{
    public class AWGrowthFragTrade : RemoteWindow<AWGrowthFragTrade>
    {
        public static LLogger Log = new LLogger("AWGrowthFragTrade", Colors.Lavender);

        public static Npc Ulan = new Npc(1017108, 478, new Vector3(16.1935f, 213f, -70.60201f));


        public AWGrowthFragTrade() : base("AWGrowthFragTrade")
        {
        }

        public void SelectExchange(int index)
        {
            SendAction(2, 3, 1, 3, (ulong)index);
        }

        public void Trade()
        {
            SendAction(1, 3, 3, 0, 1);
        }

        public void SetQuantity(int quantity)
        {
            SendAction(1, 3, 2, 3, (ulong)quantity);
        }

        public static async Task<bool> CloseExchangeWindow()
        {
            if (SelectString.IsOpen)
            {
                SelectString.ClickSlot((uint)(SelectString.LineCount - 1));
                await Coroutine.Wait(5000, () => !SelectString.IsOpen);
            }

            if (!Instance.IsOpen)
            {
                return true;
            }

            Instance.Close();
            await Coroutine.Wait(10000, () => !Instance.IsOpen && SelectString.IsOpen);

            if (SelectString.IsOpen)
            {
                SelectString.ClickSlot((uint)(SelectString.LineCount - 1));
                await Coroutine.Wait(5000, () => !SelectString.IsOpen);
            }

            return !Instance.IsOpen;
        }

        public static async Task<bool> OpenExchangeWindow()
        {
            if (Instance.IsOpen)
            {
                return true;
            }

            if (SelectString.IsOpen)
            {
                SelectString.ClickSlot((uint)(SelectString.LineCount - 1));
                await Coroutine.Wait(5000, () => SelectString.IsOpen == false);
            }

            if (!await Navigation.GetToInteractNpcSelectString(Ulan, 1, Instance))
            {
                Log.Information("Failed to get to Ulan");
                return false;
            }

            return Instance.IsOpen;
        }
    }
}