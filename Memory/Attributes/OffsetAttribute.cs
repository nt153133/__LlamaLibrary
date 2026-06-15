/*
DeepDungeon is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Original work done by zzi, contibutions by Omninewb, Freiheit, and mastahg
                                                                                 */

using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Reflection;
using ff14bot;
using ff14bot.Enums;
using LlamaLibrary.Memory.PatternFinders;

// ReSharper disable InvertIf
// ReSharper disable SimplifyLinqExpressionUseMinByAndMaxBy

namespace LlamaLibrary.Memory.Attributes;

[Flags]
public enum OffsetFlags
{
    Global = 1 << 0,
    China = 1 << 1,
    Korea = 1 << 2,
    TraditionalChinese = 1 << 3,
    ReservedRegion = 1 << 4,
    ReservedRegion2 = 1 << 5,

    /// <summary>
    /// Expansion that is active on the global client
    /// </summary>
    CurrentExpansion = 1 << 6,
    PreviousExpansion = 1 << 7,

    AllServers = Global | China | Korea | TraditionalChinese | ReservedRegion | ReservedRegion2,
    NonGlobalServers = China | Korea | TraditionalChinese | ReservedRegion | ReservedRegion2,
}

#region attributes

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
public class OffsetAttribute : Attribute
{
    public readonly string Pattern;
    public readonly int ExpectedValue;
    public readonly OffsetFlags Flags;

    public OffsetAttribute(string pattern, int expectedValue = 0, OffsetFlags flags = OffsetFlags.AllServers)
    {
        if (!pattern.StartsWith("Search ", StringComparison.Ordinal))
        {
            pattern = "Search " + pattern;
        }

        Pattern = pattern;
        Flags = flags;
        ExpectedValue = expectedValue;
    }

    [Obsolete("Remove boolean property")]
    public OffsetAttribute(string p, bool a, int exv) : this(p, exv)
    {
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class IgnoreCacheAttribute : Attribute
{
    public IgnoreCacheAttribute()
    {
    }
}

/// <summary>
/// Attribute for offsets that are only valid on non-global servers (China, Korea, TC, etc.)
/// </summary>
public class OffsetNGAttribute : OffsetAttribute
{
    public OffsetNGAttribute(string pattern, bool ignoreCache = false, int expectedValue = 0) : base(pattern, expectedValue, OffsetFlags.NonGlobalServers)
    {
    }
}

/// <summary>
/// Attribute for offsets that are only valid on china servers
/// </summary>
public class OffsetCNAttribute : OffsetAttribute
{
    public OffsetCNAttribute(string pattern, bool ignoreCache = false, int expectedValue = 0) : base(pattern, expectedValue, OffsetFlags.China)
    {
    }
}

/// <summary>
/// Attribute for offsets that are only valid on traditional china servers
/// </summary>
public class OffsetTCAttribute : OffsetAttribute
{
    public OffsetTCAttribute(string pattern, bool ignoreCache = false, int expectedValue = 0) : base(pattern, expectedValue, OffsetFlags.TraditionalChinese)
    {
    }
}

#endregion

public static class OffsetAttributeExtensions
{
    public static readonly ConcurrentDictionary<string, OffsetAttribute[]> AttributeCache = new ConcurrentDictionary<string, OffsetAttribute[]>(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<string, bool> IgnoreCacheCache = new ConcurrentDictionary<string, bool>(StringComparer.Ordinal);

    public static string GetPattern(this MemberInfo property, ClientRegion forceClientMode = ClientRegion.NotSpecified)
    {
        var attribute = property.GetAttribute(forceClientMode);
        return attribute?.Pattern ?? string.Empty;
    }

    public static OffsetAttribute? GetAttribute(this MemberInfo property, ClientRegion forceClientMode = ClientRegion.NotSpecified)
    {
        var attributes = property.OffsetAttributes(forceClientMode);
        if (attributes == null || attributes.Length == 0)
        {
            return null;
        }

        var regionFlag = forceClientMode == ClientRegion.NotSpecified
            ? OffsetManager.ActiveRecord.RegionFlag
            : forceClientMode.ConvertClientMode();
        OffsetAttribute? best = null;
        var bestSpecificity = int.MaxValue;

        foreach (var attribute in attributes)
        {
            if (!HasAnyFlag(attribute.Flags, regionFlag))
            {
                continue;
            }

            var specificity = attribute.Flags.Count();
            if (specificity >= bestSpecificity)
            {
                continue;
            }

            best = attribute;
            bestSpecificity = specificity;
        }

        return best;
    }

    public static OffsetAttribute? GetAttribute2(this MemberInfo property, ClientRegion forceClientMode = ClientRegion.NotSpecified)
    {
        var attributes = property.OffsetAttributes(forceClientMode);
        if (attributes == null || attributes.Length == 0)
        {
            return null;
        }

        var regionFlag = forceClientMode.ConvertClientMode();
        OffsetAttribute? best = null;
        var bestSpecificity = int.MaxValue;

        foreach (var attribute in attributes)
        {
            if (forceClientMode != ClientRegion.NotSpecified && !HasAnyFlag(attribute.Flags, regionFlag))
            {
                continue;
            }

            var specificity = attribute.Flags.Count();
            if (specificity >= bestSpecificity)
            {
                continue;
            }

            best = attribute;
            bestSpecificity = specificity;
        }

        return best;
    }

    public static string GetRegionString(this OffsetFlags flags)
    {
        if (flags.HasFlag(OffsetFlags.Global) || flags == OffsetFlags.AllServers)
        {
            return "Global";
        }

        if (flags.HasFlag(OffsetFlags.NonGlobalServers))
        {
            return "NonGlobalServers";
        }

        if (flags.HasFlag(OffsetFlags.China))
        {
            return "China";
        }

        if (flags.HasFlag(OffsetFlags.Korea))
        {
            return "Korea";
        }

        if (flags.HasFlag(OffsetFlags.TraditionalChinese))
        {
            return "TraditionalChinese";
        }

        return "Unknown";
    }

    public static bool IgnoreCache(this MemberInfo property)
    {
        return IgnoreCacheCache.GetOrAdd(
            property.MemberName(),
            _ => property.IsDefined(typeof(IgnoreCacheAttribute), true));
    }

    public static OffsetAttribute[]? OffsetAttributes(this MemberInfo property, ClientRegion forceClientMode)
    {
        var memberName = property.MemberName();
        return AttributeCache.GetOrAdd(memberName, _ =>
        {
            var rawAttributes = property.GetCustomAttributes(typeof(OffsetAttribute), true);
            var attributes = new OffsetAttribute[rawAttributes.Length];
            for (var i = 0; i < rawAttributes.Length; i++)
            {
                attributes[i] = (OffsetAttribute)rawAttributes[i];
            }

            if (attributes.Length > 0)
            {
                return attributes;
            }

            ff14bot.Helpers.Logging.Write($"No attribute found for {memberName}");
            return attributes;
        });
    }

    public static IntPtr SearchOffset(this ISearcher finder, string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
        {
            ff14bot.Helpers.Logging.Write("Pattern is null or empty");
            return IntPtr.Zero;
        }

        try
        {
            return finder.FindSingle(pattern);
        }
        catch (Exception e)
        {
            return IntPtr.Zero;
        }
    }

    /// <summary>
    /// Counts the number of individual flag values set in the specified OffsetFlags enumeration.
    /// </summary>
    /// <param name="flags">The OffsetFlags value to evaluate. Each set bit is considered a flag.</param>
    /// <returns>The number of flags that are set in the provided OffsetFlags value.</returns>
    internal static int Count(this OffsetFlags flags)
    {
        return BitOperations.PopCount((uint)flags);
    }

    /// <summary>
    /// Searches for an offset given a field and a searcher
    /// Searches through all OffsetAttributes on the field sorted by most specific to least specific
    /// </summary>
    /// <param name="finder"></param>
    /// <param name="field"></param>
    /// <param name="forceClientMode"></param>
    /// <returns>IntPtr.Zero if no offset could be found, otherwise the value of the offset</returns>
    public static IntPtr SearchOffset(this ISearcher finder, MemberInfo field, ClientRegion forceClientMode = ClientRegion.NotSpecified)
    {
        var patterns = field.OffsetAttributes(forceClientMode);
        if (patterns == null || patterns.Length == 0)
        {
            return IntPtr.Zero;
        }

        var regionFlag = OffsetManager.ActiveRecord.RegionFlag;

        for (var specificity = 1; specificity <= 32; specificity++)
        {
            foreach (var pattern in patterns)
            {
                if (pattern.Flags.Count() != specificity || !HasAnyFlag(pattern.Flags, regionFlag))
                {
                    continue;
                }

                var result = finder.SearchOffset(pattern.Pattern);
                if (result > IntPtr.Zero)
                {
                    return result;
                }
            }
        }

        return IntPtr.Zero;
    }

    private static bool HasAnyFlag(OffsetFlags flags, OffsetFlags flag)
    {
        return (flags & flag) != 0;
    }

    public static void SetValue(this MemberInfo field, IntPtr offset)
    {
        switch (field)
        {
            case FieldInfo fieldInfo:
                if (fieldInfo.FieldType == typeof(IntPtr))
                {
                    fieldInfo.SetValue(null, offset);
                }
                else
                {
                    fieldInfo.SetValue(null, offset.ToInt32());
                }

                break;
            case PropertyInfo propertyInfo:
                if (propertyInfo.PropertyType == typeof(IntPtr))
                {
                    propertyInfo.SetValue(null, offset);
                }
                else
                {
                    propertyInfo.SetValue(null, offset.ToInt32());
                }

                break;
        }
    }

    public static Type? GetMemberType(this MemberInfo field)
    {
        return field switch
        {
            FieldInfo fieldInfo       => fieldInfo.FieldType,
            PropertyInfo propertyInfo => propertyInfo.PropertyType,
            Type type                 => type,
            _                         => null
        };
    }

    public static void SetValue(this MemberInfo field, long offset)
    {
        try
        {
            switch (field)
            {
                case FieldInfo fieldInfo:
                    if (fieldInfo.FieldType == typeof(IntPtr))
                    {
                        var value = field.CacheAsRelativeAddress()
                            ? Core.Memory.GetAbsolute(new IntPtr(offset))
                            : new IntPtr(offset);
                        fieldInfo.SetValue(null, value);
                    }
                    else
                    {
                        fieldInfo.SetValue(null, (int)offset);
                    }

                    break;
                case PropertyInfo propertyInfo:
                    if (propertyInfo.PropertyType == typeof(IntPtr))
                    {
                        var value = field.CacheAsRelativeAddress()
                            ? Core.Memory.GetAbsolute(new IntPtr(offset))
                            : new IntPtr(offset);
                        propertyInfo.SetValue(null, value);
                    }
                    else
                    {
                        propertyInfo.SetValue(null, (int)offset);
                    }

                    break;
            }
        }
        catch (Exception e)
        {
            ff14bot.Helpers.Logging.Write(e.ToString());
            ff14bot.Helpers.Logging.Write($"Failed to set {field.Name} with value {offset:X} {offset.GetType()}");
            switch (field)
            {
                case FieldInfo fieldInfo:
                    ff14bot.Helpers.Logging.Write($"Field type: {fieldInfo.FieldType}");
                    break;
                case PropertyInfo propertyInfo:
                    ff14bot.Helpers.Logging.Write($"Property type: {propertyInfo.PropertyType}");
                    break;
            }

            throw;
        }
    }

    public static void SetValue(this MemberInfo info, object instance, IntPtr offset)
    {
        switch (info)
        {
            case FieldInfo fieldInfo:
                fieldInfo.SetValue(instance, fieldInfo.FieldType == typeof(IntPtr) ? offset : offset.ToInt32());
                break;
            case PropertyInfo propertyInfo:
                propertyInfo.SetValue(instance, propertyInfo.PropertyType == typeof(IntPtr) ? offset : offset.ToInt32());
                break;
        }
    }

    public static void SetValue(this MemberInfo info, object? instance, object? offset)
    {
        switch (info)
        {
            case FieldInfo fieldInfo:
                fieldInfo.SetValue(instance, offset);
                break;
            case PropertyInfo propertyInfo:
                propertyInfo.SetValue(instance, offset);
                break;
        }
    }

    public static IntPtr GetOffset(this MemberInfo field, ISearcher finder, ClientRegion forceClientMode = ClientRegion.NotSpecified)
    {
        if (!field.IgnoreCache() && OffsetManager.OffsetCache.TryGetValue(field.MemberName(), out var offset))
        {
            switch (field.GetMemberType())
            {
                case { } type when type == typeof(IntPtr):
                    return field.CacheAsRelativeAddress(forceClientMode)
                        ? Core.Memory.GetAbsolute(new IntPtr(offset))
                        : new IntPtr(offset);
                case { } type when type == typeof(int):
                    return new IntPtr((int)offset);
            }
        }

        var result = finder.SearchOffset(field, forceClientMode);
        if (result != IntPtr.Zero)
        {
            try
            {
                var cacheValue = field.GetMemberType() == typeof(IntPtr)
                    ? field.CacheAsRelativeAddress(forceClientMode)
                        ? Core.Memory.GetRelative(result).ToInt64()
                        : result.ToInt64()
                    : result.ToInt32();

                OffsetManager.OffsetCache.TryAdd(field.MemberName(), cacheValue);
            }
            catch (Exception e)
            {
                ff14bot.Helpers.Logging.Write($"{field.MemberName()} with value {result.ToInt64():X} failed to add to cache.");
                ff14bot.Helpers.Logging.Write(e.ToString());
            }
        }

        return result;
    }

    private static bool CacheAsRelativeAddress(this MemberInfo field, ClientRegion forceClientMode = ClientRegion.NotSpecified)
    {
        if (field.GetMemberType() != typeof(IntPtr))
        {
            return false;
        }

        var pattern = field.GetAttribute(forceClientMode)?.Pattern;
        return pattern == null || !ReadsImmediateValue(pattern);
    }

    private static bool ReadsImmediateValue(string pattern)
    {
        return pattern.Contains(" Read8", StringComparison.OrdinalIgnoreCase)
               || pattern.Contains(" Read16", StringComparison.OrdinalIgnoreCase)
               || pattern.Contains(" Read32", StringComparison.OrdinalIgnoreCase)
               || pattern.Contains(" Read64", StringComparison.OrdinalIgnoreCase);
    }

    public static string MemberName(this MemberInfo field)
    {
        return field switch
        {
            FieldInfo fieldInfo       => $"{fieldInfo.DeclaringType?.FullName}.{fieldInfo.Name}",
            PropertyInfo propertyInfo => $"{propertyInfo.DeclaringType?.FullName}.{propertyInfo.Name}",
            _                         => string.Empty
        };
    }

    public static OffsetFlags ConvertClientMode(this ClientRegion clientRegion)
    {
        return clientRegion switch
        {
            ClientRegion.Global          => OffsetFlags.Global,
            ClientRegion.China          => OffsetFlags.China,
            ClientRegion.Korea          => OffsetFlags.Korea,
            ClientRegion.TraditionalChinese => OffsetFlags.TraditionalChinese,
            _                            => OffsetFlags.AllServers
        };
    }
}

/*
public enum ForceClientMode
{
    None,
    Global,
    CN,
    Dawntrail,
    TC,
    KR
}*/
