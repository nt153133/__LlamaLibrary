using System;
using LlamaLibrary.RemoteAgents;

namespace LlamaLibrary.RemoteWindows
{
    public class VoteMvp : RemoteWindow<VoteMvp>
    {
        public VoteMvp() : base("VoteMvp")
        {
        }

        public void Vote(int index = 0)
        {
            if (AgentVoteMVP.Instance.PlayerCount < 1)
            {
                return;
            }

            SendAction(2, 3, 0, 3, (ulong)Math.Min(index, AgentVoteMVP.Instance.PlayerCount - 1));
        }
    }
}