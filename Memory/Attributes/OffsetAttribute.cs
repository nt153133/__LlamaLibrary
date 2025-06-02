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
using System.Reflection;
using ff14bot;
using ff14bot.Enums;
using LlamaLibrary.Memory.PatternFinders;

// ReSharper disable InvertIf
// ReSharper disable SimplifyLinqExpressionUseMinByAndMaxBy

namespace LlamaLibrary.Memory.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class OffsetAttribute : Attribute
{
    public readonly string Pattern;
    public readonly string PatternCN;
    public readonly string PatternDawntrail;
    public bool IgnoreCache;
    public int ExpectedValue;
    public virtual int Priority => 0;
    public virtual bool IsValid(ForceClientMode clientMode) => true;
    protected static readonly Language Language
#if RB_CN
        = Language.Chn;
#else
        = Language.Eng;
#endif

    public OffsetAttribute(string pattern, bool ignoreCache = false, int expectedValue = 0)
    {
        Pattern = pattern;
        if (!Pattern.StartsWith("Search ", StringComparison.Ordinal))
        {
            Pattern = "Search " + Pattern;
        }

        PatternCN = Pattern;
        PatternDawntrail = Pattern;
        IgnoreCache = ignoreCache;
        ExpectedValue = expectedValue;
    }

    protected OffsetAttribute(string pattern, string cnPattern, bool ignoreCache = false, int expectedValue = 0)
    {
        PatternCN = cnPattern;
        if (!PatternCN.StartsWith("Search ", StringComparison.Ordinal))
        {
            PatternCN = "Search " + PatternCN;
        }

        Pattern = pattern;

        if (string.IsNullOrEmpty(Pattern))
        {
            Pattern = cnPattern;
        }

        PatternDawntrail = Pattern;

        IgnoreCache = ignoreCache;
        ExpectedValue = expectedValue;
    }

    protected OffsetAttribute(string pattern, string cnPattern, string dawntrailPattern, bool ignoreCache = false, int expectedValue = 0)
    {
        PatternCN = cnPattern;
        if (!string.IsNullOrEmpty(cnPattern) && !PatternCN.StartsWith("Search ", StringComparison.Ordinal))
        {
            PatternCN = "Search " + PatternCN;
        }

        PatternDawntrail = dawntrailPattern;
        if (!string.IsNullOrEmpty(dawntrailPattern) && !PatternDawntrail.StartsWith("Search ", StringComparison.Ordinal))
        {
            PatternDawntrail = "Search " + PatternDawntrail;
        }

        Pattern = pattern;

        if (string.IsNullOrEmpty(Pattern))
        {
            Pattern = !string.IsNullOrEmpty(dawntrailPattern) ? PatternDawntrail : PatternCN;
        }

        if (!string.IsNullOrEmpty(Pattern) && !Pattern.StartsWith("Search ", StringComparison.Ordinal))
        {
            Pattern = "Search " + Pattern;
        }

        if (string.IsNullOrEmpty(PatternCN))
        {
            PatternCN = Pattern;
        }

        IgnoreCache = ignoreCache;
        ExpectedValue = expectedValue;
    }

    public string GetPattern(ForceClientMode forceClientMode = ForceClientMode.None)
    {
        return forceClientMode switch
        {
            ForceClientMode.CN        => PatternCN,
            ForceClientMode.Dawntrail => PatternDawntrail,
            _ => Language switch
            {
                Language.Chn => PatternCN,
                _            => Pattern
            }
        };
    }
}

public static class OffsetAttributeExtensions
{
    public static readonly ConcurrentDictionary<string, OffsetAttribute> AttributeCache = new ConcurrentDictionary<string, OffsetAttribute>(StringComparer.Ordinal);

    public static string GetPattern(this MemberInfo property, ForceClientMode forceClientMode = ForceClientMode.None)
    {
        return property.OffsetAttribute(forceClientMode)?.GetPattern(forceClientMode) ?? string.Empty;
    }

    public static bool IgnoreCache(this MemberInfo property, ForceClientMode forceClientMode = ForceClientMode.None)
    {
        return property.OffsetAttribute(forceClientMode)?.IgnoreCache ?? false;
    }

    public static OffsetAttribute? OffsetAttribute(this MemberInfo property, ForceClientMode forceClientMode)
    {
        if (!AttributeCache.TryGetValue(property.MemberName(), out var attribute))
        {
            attribute = property.GetCustomAttributes<OffsetAttribute>(true).Where(i => i.IsValid(forceClientMode)).OrderByDescending(x => x.Priority).FirstOrDefault();
            if (attribute != null)
            {
                //ff14bot.Helpers.Logging.Write($"Found attribute {attribute.GetType().Name} for {property.MemberName()}");
                AttributeCache.TryAdd(property.MemberName(), attribute);
            }
            else
            {
                ff14bot.Helpers.Logging.Write($"No attribute found for {property.MemberName()}");
            }
        }

        return attribute;
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
            ff14bot.Helpers.Logging.Write(e.ToString());
            return IntPtr.Zero;
        }
    }

    public static IntPtr SearchOffset(this ISearcher finder, MemberInfo field, ForceClientMode forceClientMode = ForceClientMode.None)
    {
        return finder.SearchOffset(field.GetPattern(forceClientMode));
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

public class OffsetCNAttribute : OffsetAttribute
{
    private static bool _isValid { get; } = Language == Language.Chn;

    public override bool IsValid(ForceClientMode clientMode)
    {
        return clientMode == ForceClientMode.CN || _isValid;
    }

    public override int Priority { get; } = 1;

    public OffsetCNAttribute(string pattern, bool ignoreCache = false, int expectedValue = 0) : base("", pattern, ignoreCache, expectedValue)
    {
    }
}

public class OffsetDawntrailAttribute : OffsetAttribute
{
#if !RB_DT
    private static bool _isValid { get; } = false;
#else
    private static bool _isValid { get; } = true;
#endif

    public override bool IsValid(ForceClientMode clientMode)
    {
        return clientMode == ForceClientMode.Dawntrail || _isValid;
    }

    public override int Priority { get; } = 99;

    public OffsetDawntrailAttribute(string pattern, bool ignoreCache = false, int expectedValue = 0) : base("", "", pattern, ignoreCache, expectedValue)
    {
    }
}

public enum ForceClientMode
{
    None,
    Global,
    CN,
    Dawntrail
}