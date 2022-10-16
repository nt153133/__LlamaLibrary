using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot.RemoteWindows;
using LlamaLibrary.Enums;
using LlamaLibrary.Logging;
using LlamaLibrary.Retainers;
using LlamaLibrary.Structs;
using static ff14bot.RemoteWindows.Talk;

namespace LlamaLibrary.RemoteWindows
{
    //TODO Move element numbers to dictionary
    public class RetainerList : RemoteWindow<RetainerList>
    {
        private static readonly LLogger Log = new(nameof(RetainerList), Colors.White);

        public RetainerList() : base("RetainerList")
        {
        }

        public RetainerInfo[] OrderedRetainerList => HelperFunctions.GetOrderedRetainerArray(HelperFunctions.ReadRetainerArray());

        public int NumberOfRetainers => OrderedRetainerList.Length;

        public int NumberOfVentures => Elements[1].TrimmedData;

        public string RetainerName(int index)
        {
            return OrderedRetainerList[index].Name;
        }

        public int GetRetainerJobLevel(int index)
        {
            return OrderedRetainerList[index].Level;
        }

        public bool RetainerHasJob(int index)
        {
            return OrderedRetainerList[index].Job != 0;
        }

        public RetainerRole RetainerRole(int index)
        {
            return (RetainerRole)Elements[(index * 9) + 4].TrimmedData;
        }

        public async Task<bool> SelectRetainer(ulong retainerContentId)
        {
            var list = OrderedRetainerList;
            if (!list.Any(i => i.Unique == retainerContentId))
            {
                return false;
            }

            return await SelectRetainer(OrderedRetainerList.IndexInList(retainerContentId));
        }

        public async Task<bool> SelectRetainer(int index)
        {
            if (!IsOpen)
            {
                Log.Warning("Retainer selection window not open");
                return false;
            }

            try
            {
                Log.Verbose($"Sending Action 3UL, 2UL, 3UL, {(ulong)index}");
                SendAction(2, 3UL, 2UL, 3UL, (ulong)index);

                Log.Verbose("Waiting on Dialog");
                await Coroutine.Wait(9000, () => DialogOpen || SelectString.IsOpen);

                if (DialogOpen)
                {
                    while (!SelectString.IsOpen)
                    {
                        if (DialogOpen)
                        {
                            Log.Verbose("Sending Dialog next");
                            Next();
                            await Coroutine.Sleep(100);
                        }

                        await Coroutine.Wait(1500, () => DialogOpen || SelectString.IsOpen);
                    }
                }

                await Coroutine.Wait(9000, () => SelectString.IsOpen);
            }
            catch (Exception ex)
            {
                Log.Error($"Error selecting retainer: {ex}");
            }

            return SelectString.IsOpen;
        }
    }

    //TODO this seems like it should be moved to extensions or just made into a function in RetainerInfo
    public static class Extensions
    {
        public static int IndexInList(this RetainerInfo[] list, ulong contentId)
        {
            for (var index = 0; index < list.Length; index++)
            {
                if (list[index].Unique == contentId)
                {
                    return index;
                }
            }

            return -1;
        }
    }
}