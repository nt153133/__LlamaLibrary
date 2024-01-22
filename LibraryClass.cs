using System.Threading.Tasks;
using ff14bot.AClasses;
using LlamaLibrary.Memory;

namespace LlamaLibrary;

public class LibraryClass : ILibrary
{
    public async Task<bool> PreOffsetWarmup()
    {
        return await OffsetManager.InitLib();
    }

    public async Task<bool> PostOffsetWarmup()
    {
        OffsetManager.SetPostOffsets();
        OffsetManager.SetScriptsThread();
        return true;
    }
}