using System;

namespace LlamaLibrary.Memory.PatternFinders;

public interface ISearcher : IDisposable
{
    public IntPtr ImageBase { get; }

    /// <summary>
    /// Expects pattern to be in the following format:
    /// 48 8D 05 ?? ?? ?? ?? 48 C7 43 ?? ?? ?? ?? ?? 48 8D 4B ?? 48 89 03 66 C7 43 ?? ?? ?? Add 3 TraceRelative
    /// Available Commands:
    ///  Add # - Shifts the searcher this # is from the start of the pattern. So add 1 moves us to byte 2. add 2 moves to byte 3 etc.
    ///  Sub # - Shifts the searcher this # is from the start of the pattern. so sub 1 moves us to byte -1. sub 2 moves us to byte -2 etc.
    ///  Read8 - Reads a byte from the resulting address
    ///  Read16 - Reads 2 bytes (16bits) from the resulting address
    ///  Read32 - Reads 4 bytes (32bits) from the resulting address
    ///  Read64 - Reads 8 bytes (64bits) from the resulting address
    ///  TraceRelative - Follow the relative address used in calls and lea's
    ///  TraceCall - Should basically do Add 1 TraceRelative on a pattern for a function call ie E8 ?? ?? ?? ?? Doesn't really work right now.
    /// </summary>
    /// <param name="pattern">Hex based pattern with ? as wildcards.</param>
    /// <returns>Pointer to memory address.</returns>
    //public IntPtr FindSingle(string pattern);

    /// <summary>
    /// Expects pattern to be in the following format:
    /// 48 8D 05 ?? ?? ?? ?? 48 C7 43 ?? ?? ?? ?? ?? 48 8D 4B ?? 48 89 03 66 C7 43 ?? ?? ?? Add 3 TraceRelative
    /// Available Commands:
    ///  Add # - Shifts the searcher this # is from the start of the pattern. So add 1 moves us to byte 2. add 2 moves to byte 3 etc.
    ///  Sub # - Shifts the searcher this # is from the start of the pattern. so sub 1 moves us to byte -1. sub 2 moves us to byte -2 etc.
    ///  Read8 - Reads a byte from the resulting address
    ///  Read16 - Reads 2 bytes (16bits) from the resulting address
    ///  Read32 - Reads 4 bytes (32bits) from the resulting address
    ///  Read64 - Reads 8 bytes (64bits) from the resulting address
    ///  TraceRelative - Follow the relative address used in calls and lea's
    ///  TraceCall - Should basically do Add 1 TraceRelative on a pattern for a function call ie E8 ?? ?? ?? ?? Doesn't really work right now.
    /// </summary>
    /// <param name="pattern">Hex based pattern with ? as wildcards.</param>
    /// <param name="dontRebase"></param>
    /// <returns>Pointer to memory address. IntPtr.Zero if not found.</returns>
    public IntPtr FindSingle(string pattern, bool dontRebase = true);

    /// <summary>
    /// Expects pattern to be in the following format:
    /// 48 8D 05 ?? ?? ?? ?? 48 C7 43 ?? ?? ?? ?? ?? 48 8D 4B ?? 48 89 03 66 C7 43 ?? ?? ?? Add 3 TraceRelative
    /// Available Commands:
    ///  Add # - Shifts the searcher this # is from the start of the pattern. So add 1 moves us to byte 2. add 2 moves to byte 3 etc.
    ///  Sub # - Shifts the searcher this # is from the start of the pattern. so sub 1 moves us to byte -1. sub 2 moves us to byte -2 etc.
    ///  Read8 - Reads a byte from the resulting address
    ///  Read16 - Reads 2 bytes (16bits) from the resulting address
    ///  Read32 - Reads 4 bytes (32bits) from the resulting address
    ///  Read64 - Reads 8 bytes (64bits) from the resulting address
    ///  TraceRelative - Follow the relative address used in calls and lea's
    ///  TraceCall - Should basically do Add 1 TraceRelative on a pattern for a function call ie E8 ?? ?? ?? ?? Doesn't really work right now.
    /// </summary>
    /// <param name="pattern">Hex based pattern with ? as wildcards.</param>
    /// <returns>Array of pointers matching the pattern.</returns>
    public IntPtr[] SearchMany(string pattern);
}