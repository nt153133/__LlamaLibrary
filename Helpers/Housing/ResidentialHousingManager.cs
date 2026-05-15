using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot;
using LlamaLibrary.Enums;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers.Housing;

/// <summary>
/// Low-level accessor for the game's <c>ResidentialHousingManager</c>, which provides
/// the player's registered housing residences (private estate, FC estate, shared estates,
/// apartment, etc.).
/// </summary>
public static class ResidentialHousingManager
{
    

    private static readonly int[] ResidentialIndexes = { 0, 1, 2, 3, 5, 6 };

    /// <summary>
    /// Retrieves a raw residence object pointer by slot index from the game's residential housing manager.
    /// </summary>
    /// <param name="index">The slot index (0–6, excluding 4 which is used for shared estates).</param>
    /// <param name="subIndex">
    /// The sub-index for shared estate slots (0 = first shared estate, 1 = second).
    /// Pass <c>0xFFFFFFFF</c> (default) for non-shared slots.
    /// </param>
    /// <returns>A packed <see langword="long"/> encoding the plot, ward, zone, world, and other data.</returns>
    public static long GetResidentialObject(int index, uint subIndex = 0xFFFFFFFF)
    {
        return Core.Memory.CallInjectedWraper<long>(ResidentialHousingManagerOffsets.GetResidentObject, index, subIndex);
    }

    /// <summary>
    /// Verifies that a packed residence value represents a valid, registered housing location.
    /// </summary>
    /// <param name="index">The packed residence value returned by <see cref="GetResidentialObject"/>.</param>
    /// <returns>
    /// <see langword="true"/> when the value represents a valid residence; otherwise <see langword="false"/>.
    /// </returns>
    public static bool CheckValid(long index)
    {
        return Core.Memory.CallInjectedWraper<bool>(ResidentialHousingManagerOffsets.CheckValid, index);
    }

    /// <summary>
    /// Enumerates all valid <see cref="ResidenceInfo"/> records registered to the current character,
    /// including private estate, FC estate, FC room, apartment, apartment room, and up to two
    /// shared estates.
    /// </summary>
    /// <returns>A sequence of valid <see cref="ResidenceInfo"/> instances.</returns>
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