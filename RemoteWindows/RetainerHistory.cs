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
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.Retainers;

namespace LlamaLibrary.RemoteWindows;

public class RetainerHistory : RemoteWindow<RetainerHistory>
{
    private const string WindowName = "RetainerHistory";

    private static readonly LLogger Log = new LLogger("RetainerHistory", Colors.OrangeRed);

    private static class Offsets
    {
        [Offset("Search 48 8B 41 ? 48 63 D2 44 39 04 90 Add 3 Read8")]
        internal static int NumberArrayData_IntArray;

        [Offset("Search BA ? ? ? ? 49 8B CC E8 ? ? ? ? 4C 8B 7C 24 ? 48 8B 74 24 ? Add 1 Read32")]
        internal static int NumberArrayData_Count;

        [Offset("Search BF ? ? ? ? 41 BE ? ? ? ? 90 Add 7 Read32")]
        internal static int NumberArrayData_Start;

        [Offset("Search BA ? ? ? ? 48 8B C8 4C 8B 10 41 FF 52 ? 49 8B 4D ? 4C 8B E0 Add 1 Read32")]
        internal static int NumberArrayIndex;

        [Offset("Search BA ? ? ? ? 48 8B C8 4C 8B 00 41 FF 50 ? 48 8B E8 4D 85 E4 Add 1 Read32")]
        internal static int StringArrayIndex;

        [Offset("Search 48 8B 43 ? 48 63 CA 45 84 C9 Add 3 Read8")]
        internal static int StringArrayData_StrArray;

        [Offset("Search BF ? ? ? ? 41 BE ? ? ? ? 90 Add 1 Read32")]
        internal static int StringArrayData_Start;

        // GetSubModule
        [Offset("E8 ? ? ? ? 48 85 C0 74 ? 4C 8B 00 48 8B D3 48 8B C8 48 83 C4 ? 5B 49 FF 60 ? 48 83 C4 ? 5B C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 40 53 TraceCall")]
        internal static IntPtr GetSubModule;

        // vfunc 33 of UIModule
        [Offset("41 FF 90 ? ? ? ? 48 8B C8 BA ? ? ? ? E8 ? ? ? ? 48 85 C0 74 ? 4C 8B 00 48 8B D3 48 8B C8 48 83 C4 ? 5B 49 FF 60 ? 48 83 C4 ? 5B C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 40 53 Add 3 Read32")]
        internal static int GetSomethingModuleVtblFunction;

        // Submodule number 9
        [Offset("BA ? ? ? ? E8 ? ? ? ? 48 85 C0 74 ? 4C 8B 00 48 8B D3 48 8B C8 48 83 C4 ? 5B 49 FF 60 ? 48 83 C4 ? 5B C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 40 53 Add 1 Read8")]
        internal static int SubModule;

        [Offset("48 8B 8B ? ? ? ? 48 8D 54 24 ? 48 89 4C 24 ? 45 33 C9 48 8B C8 Add 3 Read32")]
        internal static int RetainerId;

        [Offset("40 53 48 81 EC ? ? ? ? 48 8B 05 ? ? ? ? 48 33 C4 48 89 84 24 ? ? ? ? 48 83 B9 ? ? ? ? ?")]
        internal static IntPtr RequestSales;
    }

    public RetainerHistory() : base(WindowName)
    {
    }

    public IntPtr NumberArrayStart
    {
        get
        {
            var arrayLocation = Core.Memory.Read<IntPtr>(AtkArrayDataHolder.GetNumberArray(Offsets.NumberArrayIndex) + Offsets.NumberArrayData_IntArray);

            var start = arrayLocation + ((Offsets.NumberArrayData_Start - 2) * 4);

            return start;
        }
    }

    public IntPtr HistoryCountLocation
    {
        get
        {
            var arrayLocation = Core.Memory.Read<IntPtr>(AtkArrayDataHolder.GetNumberArray(Offsets.NumberArrayIndex) + Offsets.NumberArrayData_IntArray);

            return arrayLocation + ((Offsets.NumberArrayData_Count) * 4);
        }
    }

    public int HistoryCount => Core.Memory.Read<int>(HistoryCountLocation);

    public IntPtr StringArrayStart
    {
        get
        {
            var arrayLocation = Core.Memory.Read<IntPtr>(AtkArrayDataHolder.GetStringArray(Offsets.StringArrayIndex) + Offsets.StringArrayData_StrArray);

            var start = arrayLocation + ((Offsets.StringArrayData_Start - 1) * 8);

            return start;
        }
    }

    public HistoryNumber[] HistoryNumbers => Core.Memory.ReadArray<HistoryNumber>(NumberArrayStart, HistoryCount);

    public HistoryString[] HistoryStrings => Core.Memory.ReadArray<HistoryString>(StringArrayStart, HistoryCount);

    public IntPtr RaptureStruct
    {
        get
        {
            var func = Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(UiManagerProxy.UIModule) + Offsets.GetSomethingModuleVtblFunction);
            var subModule = Core.Memory.CallInjected64<IntPtr>(func, UiManagerProxy.UIModule);
            var pointer = Core.Memory.CallInjected64<IntPtr>(Offsets.GetSubModule, subModule, Offsets.SubModule);
            return pointer;
        }
    }

    public async Task<Dictionary<ulong, List<RetainerSale>>> AllRetainerSales()
    {
        var result = new Dictionary<ulong, List<RetainerSale>>();
        var fullRets = await HelperFunctions.GetOrderedRetainerArray(true);
        var rets = fullRets.Where(i => i.Active).Select(i => i.Unique);
        var raptureStruct = RaptureStruct;
        var retainerIdLocation = raptureStruct + Offsets.RetainerId;

        foreach (var retainerId in rets)
        {
            var name = fullRets.First(i => i.Unique == retainerId).Name;
            Core.Memory.Write(HistoryCountLocation, 99);
            Core.Memory.Write(retainerIdLocation, retainerId);
            Core.Memory.CallInjected64<IntPtr>(Offsets.RequestSales, raptureStruct);
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
            int count = HistoryCount;
            var result = new List<RetainerSale>(count);
            var numbers = HistoryNumbers;
            var strings = HistoryStrings;

            for (int i = 0; i < count; i++)
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