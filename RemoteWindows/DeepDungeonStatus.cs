using System.Threading.Tasks;
using ff14bot.Managers;
using LlamaLibrary.RemoteAgents;

namespace LlamaLibrary.RemoteWindows;

public class DeepDungeonStatus : RemoteWindow<DeepDungeonStatus>
{
    public DeepDungeonStatus() : base("DeepDungeonStatus")
    {
    }

    public async Task CastMagicite()
    {

        if (!IsOpen)
        {
            await Open();
        }

        if (IsOpen)
        {
            SendAction(2, 3, 0xC, 0x3, 0x0);
        }
    }

    public override async Task<bool> Open()
    {
        if (IsOpen)
        {
            return true;
        }

        AgentModule.ToggleAgentInterfaceById(AgentDeepDungeonStatus.IdRaw);
        return await WaitTillWindowOpen();
    }
}