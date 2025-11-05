/*
DeepDungeon is licensed under a
Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.

Original work done by zzi, contibutions by Omninewb, Freiheit, and mastahg
                                                                                 */

using System;
using System.Collections.Concurrent;
using System.Linq;
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
    Global = 1<<0,
    China = 1<<1,
    Korea = 1<<2,
    TraditionalChinese = 1<<3,
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

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property,AllowMultiple = true)]
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
    public OffsetAttribute(string p,bool a,int exv) : this(p, exv)
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

#endregion

public static class OffsetAttributeExtensions
{
    public static readonly ConcurrentDictionary<string, OffsetAttribute[]> AttributeCache = new ConcurrentDictionary<string, OffsetAttribute[]>(StringComparer.Ordinal);

    public static string GetPattern(this MemberInfo property, ForceClientMode forceClientMode = ForceClientMode.None)
    {
        //TODO: figure out the best string to return, or maybe just remove this logic?
        return "";
    }

    public static bool IgnoreCache(this MemberInfo property)
    {
        return property.GetCustomAttributes<IgnoreCacheAttribute>().Any();
    }

    public static OffsetAttribute[]? OffsetAttributes(this MemberInfo property, ForceClientMode forceClientMode)
    {
        if (!AttributeCache.TryGetValue(property.MemberName(), out var attributes))
        {
            attributes = property.GetCustomAttributes<OffsetAttribute>(true).ToArray();
            if (attributes.Length > 0)
            {
                AttributeCache.TryAdd(property.MemberName(), attributes);
            }
            else
            {
                ff14bot.Helpers.Logging.Write($"No attribute found for {property.MemberName()}");
            }
        }

        return attributes;
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
    public static IntPtr SearchOffset(this ISearcher finder, MemberInfo field, ForceClientMode forceClientMode = ForceClientMode.None)
    {

        var patterns = field.OffsetAttributes(forceClientMode)?.Where(r=>r.Flags.HasFlag(OffsetManager.ActiveRecord.RegionFlag)).OrderBy(r=>r.Flags.Count());

        if (patterns != null)
        {
            foreach (var pattern in patterns)
            {
                var result =  finder.SearchOffset(pattern.Pattern);
                if (result > IntPtr.Zero)
                {
                    return result;
                }
            }
        }
        return IntPtr.Zero;
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
                        fieldInfo.SetValue(null, Core.Memory.GetAbsolute(new IntPtr(offset)));
                    }
                    else
                    {
                        fieldInfo.SetValue(null, (int)offset);
                    }

                    break;
                case PropertyInfo propertyInfo:
                    if (propertyInfo.PropertyType == typeof(IntPtr))
                    {
                        propertyInfo.SetValue(null, Core.Memory.GetAbsolute(new IntPtr(offset)));
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

    public static IntPtr GetOffset(this MemberInfo field, ISearcher finder, ForceClientMode forceClientMode = ForceClientMode.None)
    {
        if (!field.IgnoreCache() && OffsetManager.OffsetCache.TryGetValue(field.MemberName(), out var offset))
        {
            switch (field.GetMemberType())
            {
                case { } type when type == typeof(IntPtr):
                    return Core.Memory.GetAbsolute(new IntPtr(offset));
                case { } type when type == typeof(int):
                    return new IntPtr((int)offset);
            }
        }

        var result = finder.SearchOffset(field, forceClientMode);
        if (result != IntPtr.Zero)
        {
            try
            {
                OffsetManager.OffsetCache.TryAdd(field.MemberName(), field.GetMemberType() == typeof(IntPtr) ? Core.Memory.GetRelative(result).ToInt64() : result.ToInt32());
            }
            catch (Exception e)
            {
                ff14bot.Helpers.Logging.Write($"{field.MemberName()} with value {result.ToInt64():X} failed to add to cache.");
                ff14bot.Helpers.Logging.Write(e.ToString());
            }
        }

        return result;
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
}

public enum ForceClientMode
{
    None,
    Global,
    CN,
    Dawntrail
}