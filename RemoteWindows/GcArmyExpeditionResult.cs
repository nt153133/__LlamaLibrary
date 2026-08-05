using System;
using System.Collections.Generic;
using System.Text;
using ff14bot;
using AtkValueType = LlamaLibrary.RemoteWindows.Atk.ValueType;

namespace LlamaLibrary.RemoteWindows
{
    public class GcArmyExpeditionResult : RemoteWindow<GcArmyExpeditionResult>
    {
        public static readonly Dictionary<string, int> Properties = new(StringComparer.Ordinal)
        {
            { "Succeeded", 2 },
            { "ResultText", 3 },
            { "MissionName", 4 },
            { "MemberCount", 5 },
            { "SquadronExperience", 9 },
        };

        public GcArmyExpeditionResult() : base("GcArmyExpeditionResult")
        {
        }

        public bool Succeeded => IsOpen && Elements.Length > Properties["Succeeded"] && Elements[Properties["Succeeded"]].Bool;
        public string ResultText => ReadString(Properties["ResultText"]);
        public string MissionName => ReadString(Properties["MissionName"]);
        public int MemberCount => IsOpen && Elements.Length > Properties["MemberCount"] ? Elements[Properties["MemberCount"]].Int : 0;
        public int SquadronExperience => IsOpen && Elements.Length > Properties["SquadronExperience"] ? Elements[Properties["SquadronExperience"]].Int : 0;

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
