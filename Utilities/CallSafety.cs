//#define LogCall

using System;
using GreyMagic;

namespace LlamaLibrary.Utilities;

public static class CallSafety
{
    public static T CallInjectedWraper<T>(this ExternalProcessMemory memory,IntPtr address, params object[] args) where T : struct
    {

#if LogCall
        var methodInfo = new StackTrace().GetFrame(1).GetMethod();
        var className = methodInfo.ReflectedType.Name;

        ff14bot.Helpers.Logging.Write("Calling from {0} {1}", className, methodInfo.Name);
#endif

        if (address < memory.ImageBase)
        {
            throw new Exception("Address is not in the game process");
        }

        lock (memory.Executor.AssemblyLock)
        {
            return memory.CallInjected64<T>(address, args);
        }
    }

}