using System;
using System.Text;
using ff14bot;
using AtkValueType = LlamaLibrary.RemoteWindows.Atk.ValueType;

namespace LlamaLibrary.RemoteWindows
{
    public class GcArmyExpeditionResult : RemoteWindow<GcArmyExpeditionResult>
    {
        public GcArmyExpeditionResult() : base("GcArmyExpeditionResult")
        {
        }

        public bool Succeeded => IsOpen && Elements.Length > 2 && Elements[2].TrimmedData != 0;
        public string ResultText => ReadString(3);
        public string MissionName => ReadString(4);
        public int MemberCount => IsOpen && Elements.Length > 5 ? Elements[5].TrimmedData : 0;
        public int SquadronExperience => IsOpen && Elements.Length > 9 ? Elements[9].TrimmedData : 0;

        /// <summary>Completes the mission debriefing and closes the result window.</summary>
        public bool Complete()
        {
            if (!IsOpen)
            {
                return false;
            }

            SendAction(true, (AtkValueType.Int, 0x0));
            return true;
        }

        public override void Close() => Complete();

        private string ReadString(int index)
        {
            var values = Elements;
            return index < values.Length && values[index].Data != 0
                ? Core.Memory.ReadString((IntPtr)values[index].Data, Encoding.UTF8)
                : string.Empty;
        }
    }
}
