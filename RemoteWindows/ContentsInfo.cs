using System;
using System.Text;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using LlamaLibrary.RemoteAgents;

namespace LlamaLibrary.RemoteWindows
{
    public class ContentsInfo : RemoteWindow<ContentsInfo>
    {
        public ContentsInfo() : base("ContentsInfo")
        {
        }

        public override async Task<bool> Open()
        {
            if (IsOpen)
            {
                return true;
            }

            AgentContentsInfo.Instance.Toggle();
            await Coroutine.Wait(5000, () => IsOpen);

            return IsOpen;
        }

        public void OpenGCSupplyWindow()
        {
            SendAction(2, 3, 0xC, 3, 1);
        }

        public void OpenMasterPieceSupplyWindow()
        {
            SendAction(2, 3, 0xC, 3, 6);
        }

        public string GetElementString(int index)
        {
            return Elements[index].Data != 0 ? Core.Memory.ReadString((IntPtr)Elements[index].Data, Encoding.UTF8) : "";
        }

        public int GetNumberOfBeastTribeAllowance()
        {
            var line = Instance.GetElementString(50);
            return line == "" ? 0 : int.Parse(line.Split(':')[1].Trim());
        }
    }
}