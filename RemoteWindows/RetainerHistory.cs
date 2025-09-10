using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using LlamaLibrary.ClientDataHelpers;
using LlamaLibrary.JsonObjects;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Retainers;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteWindows
{
    public class RetainerHistory : RemoteWindow<RetainerHistory>
    {
        private static readonly LLogger Log = new(nameof(RetainerHistory), Colors.OrangeRed);

        

        public RetainerHistory() : base("RetainerHistory")
        {
        }

        public IntPtr NumberArrayStart
        {
            get
            {
                var arrayLocation = Core.Memory.Read<IntPtr>(AtkArrayDataHolder.GetNumberArray(RetainerHistoryOffsets.NumberArrayIndex) + RetainerHistoryOffsets.NumberArrayData_IntArray);

                var start = arrayLocation + ((RetainerHistoryOffsets.NumberArrayData_Start - 2) * 4);

                return start;
            }
        }

        public IntPtr HistoryCountLocation
        {
            get
            {
                var arrayLocation = Core.Memory.Read<IntPtr>(AtkArrayDataHolder.GetNumberArray(RetainerHistoryOffsets.NumberArrayIndex) + RetainerHistoryOffsets.NumberArrayData_IntArray);

                return arrayLocation + (RetainerHistoryOffsets.NumberArrayData_Count * 4);
            }
        }

        public int HistoryCount => Core.Memory.Read<int>(HistoryCountLocation);

        public IntPtr StringArrayStart
        {
            get
            {
                var arrayLocation = Core.Memory.Read<IntPtr>(AtkArrayDataHolder.GetStringArray(RetainerHistoryOffsets.StringArrayIndex) + RetainerHistoryOffsets.StringArrayData_StrArray);

                var start = arrayLocation + ((RetainerHistoryOffsets.StringArrayData_Start - 1) * 8);

                return start;
            }
        }

        public HistoryNumber[] HistoryNumbers => Core.Memory.ReadArray<HistoryNumber>(NumberArrayStart, HistoryCount);

        public HistoryString[] HistoryStrings => Core.Memory.ReadArray<HistoryString>(StringArrayStart, HistoryCount);

        public IntPtr RaptureStruct
        {
            get
            {
                var func = Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(UiManagerProxy.UIModule) + RetainerHistoryOffsets.GetSomethingModuleVtblFunction);
                var subModule = Core.Memory.CallInjectedWraper<IntPtr>(func, UiManagerProxy.UIModule);
                var pointer = Core.Memory.CallInjectedWraper<IntPtr>(RetainerHistoryOffsets.GetSubModule, subModule, RetainerHistoryOffsets.SubModule);
                return pointer;
            }
        }

        public async Task<Dictionary<ulong, List<RetainerSale>>> AllRetainerSales()
        {
            var result = new Dictionary<ulong, List<RetainerSale>>();
            var fullRets = await HelperFunctions.GetOrderedRetainerArray(true);
            var rets = fullRets.Where(i => i.Active).Select(i => i.Unique);
            var raptureStruct = RaptureStruct;
            var retainerIdLocation = raptureStruct + RetainerHistoryOffsets.RetainerId;

            foreach (var retainerId in rets)
            {
                var name = fullRets.First(i => i.Unique == retainerId).Name;
                Core.Memory.Write(HistoryCountLocation, 99);
                Core.Memory.Write(retainerIdLocation, retainerId);
                Core.Memory.CallInjectedWraper<IntPtr>(RetainerHistoryOffsets.RequestSales, raptureStruct);
                if (await Coroutine.Wait(5000, () => Core.Memory.NoCacheRead<int>(HistoryCountLocation) != 99))
                {
                    result.Add(retainerId, Sales.ToList());
                }
                else
                {
                    Log.Information($"History failed {name}");
                }
            }

            return result;
        }

        public List<RetainerSale> Sales
        {
            get
            {
                var count = HistoryCount;
                var result = new List<RetainerSale>(count);
                var numbers = HistoryNumbers;
                var strings = HistoryStrings;

                for (var i = 0; i < count; i++)
                {
                    result.Add(new RetainerSale(numbers[i], strings[i]));
                }

                return result;
            }
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x1C)]
    public struct HistoryNumber
    {
        public int Price;
        public int Unk1;
        public int Unk2;
        public int HQInt;
        public int TimeStamp;
        public uint ItemId;
        public int IconId;

        public bool HQ => HQInt != 0;
        public uint TrueItemId => HQ ? ItemId + 1_000_000 : ItemId;
        public DateTime SoldDateTime => RetainerHistory.UnixTimeStampToDateTime(TimeStamp);
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x28)]
    public struct HistoryString
    {
        public IntPtr Price;
        public IntPtr Count;
        public IntPtr Buyer;
        public IntPtr Date;
        public IntPtr Item;

        public int Qty
        {
            get
            {
                if (int.TryParse(Core.Memory.ReadStringUTF8(Count), out var qty))
                {
                    return qty;
                }

                return 1;
            }
        }
    }
}