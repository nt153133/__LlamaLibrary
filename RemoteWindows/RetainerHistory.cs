using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ff14bot;
using LlamaLibrary.ClientDataHelpers;
using LlamaLibrary.JsonObjects;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteWindows;

public class RetainerHistory : RemoteWindow<RetainerHistory>
{
    private const string WindowName = "RetainerHistory";

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

    public int HistoryCount
    {
        get
        {
            var arrayLocation = Core.Memory.Read<IntPtr>(AtkArrayDataHolder.GetNumberArray(Offsets.NumberArrayIndex) + Offsets.NumberArrayData_IntArray);

            var count = Core.Memory.Read<int>(arrayLocation + ((Offsets.NumberArrayData_Count) * 4));

            return count;
        }
    }

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

    public int Qty => int.Parse(Core.Memory.ReadStringUTF8(Count));
}