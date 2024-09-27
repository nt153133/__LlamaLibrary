using System.Threading.Tasks;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.NeoProfile;
using ff14bot.NeoProfiles;
using LlamaLibrary.Events;
using LlamaLibrary.Extensions;
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
        LoginEvents.SetAccountId();
        LoginEvents.UpdateInfo();
        TreeRoot.OnStart += bot =>
        {
            if (!Core.IsInGame)
            {
                return;
            }

            if (LoginEvents.LastKnownCharacterId != Core.Me.PlayerId())
            {
                LoginEvents.InvokeOnCharacterSwitched(true);
            }
        };
        return true;
    }
}