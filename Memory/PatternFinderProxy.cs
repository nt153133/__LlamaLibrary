using System;
using System.Windows.Media;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.PatternFinders;

namespace LlamaLibrary.Memory;

public static class PatternFinderProxy
{
    private static readonly DebounceDispatcher DebounceDispatcher = new DebounceDispatcher(_ => Dispose());
    private static readonly LLogger Log = new LLogger("PatternFinderProxy", Colors.Blue);
    private static readonly object Lock = new object();
    private static GreyMagicPf? _patternFinder;

    public static GreyMagicPf PatternFinder
    {
        get
        {
            lock (Lock)
            {
                if (_patternFinder == null)
                {
                    //Log.Information("Creating new PatternFinder");
                    _patternFinder = new GreyMagicPf();
                }

                DebounceDispatcher.Debounce(30_000);

                return _patternFinder;
            }
        }
    }

    public static void Dispose()
    {
        //Log.Information("Disposing PatternFinder");

        lock (Lock)
        {
            _patternFinder?.Dispose();
            _patternFinder = null;
        }

        GC.Collect();
    }
}