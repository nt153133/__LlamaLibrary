using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot;
using LlamaLibrary.Enums;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.Helpers.Housing;

public static class ResidentialHousingManager
{
    internal static class Offsets
    {
        //6.3
        [Offset("Search E8 ? ? ? ? 48 8B C8 E8 ? ? ? ? 0F B6 C8 0F B6 E8 E8 ? ? ? ? 84 C0 74 ? Add 1 TraceRelative")]
        [OffsetDawntrail("Search 33 C9 E8 ? ? ? ? 48 8B C8 E8 ? ? ? ?  Add 2 TraceCall")]
        internal static IntPtr GetResidentObject;

        [Offset("Search 40 53 48 83 EC ? 48 8B D9 48 83 F9 ? 74 ?")]
        internal static IntPtr CheckValid;
    }

    private static readonly int[] ResidentialIndexes = { 0, 1, 2, 3, 5, 6 };

    public static long GetResidentialObject(int index, uint subIndex = 0xFFFFFFFF)
    {
        return Core.Memory.CallInjectedWraper<long>(Offsets.GetResidentObject, index, subIndex);
    }

    public static bool CheckValid(long index)
    {
        return Core.Memory.CallInjectedWraper<bool>(Offsets.CheckValid, index);
    }

    public static IEnumerable<ResidenceInfo> GetResidences()
    {
        var list = (from i in ResidentialIndexes let obj = GetResidentialObject(i) where CheckValid(obj) select new ResidenceInfo(obj, (HouseLocationIndex)i)).ToList();

        var residential1 = GetResidentialObject(4, 0);
        if (CheckValid(residential1))
        {
            list.Add(new ResidenceInfo(residential1, HouseLocationIndex.SharedEstate1));
        }

        //Shared estate 2
        residential1 = GetResidentialObject(4, 1);
        if (CheckValid(residential1))
        {
            list.Add(new ResidenceInfo(residential1, HouseLocationIndex.SharedEstate2));
        }

        return list;
    }

    //Function that takes in a long and returns turple of 3 shorts and 2 bytes
    //The only parameter is the long returned from GetResidentObjectPtr
    //The first short is the plot number
    //The second short is the ward
    //The third short is the zone
    //The first byte is the world
    //The second byte is the unknown
    //The long parameter is a packed value of the 5 values above
    //The function unpacks the long into the 5 values and returns them
    //Generate the function
    //The function is called GetResidentObjectInfo
    //The function is static
    //The function is public
    //The function returns a turple of 3 shorts and 2 bytes
    //The function takes in a long
}