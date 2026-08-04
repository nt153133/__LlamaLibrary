using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ff14bot;

namespace LlamaLibrary.RemoteWindows
{
    public class Dawn : RemoteWindow<Dawn>
    {
        private readonly List<TrustNPC> npcList;

        public static readonly Dictionary<string, int> Properties = new(StringComparer.Ordinal)
        {
            { "NumberOfTrustsAvailable", 73 },
            { "SelectedTrustId", 74 },
            { "SelectedTrustName", 75 },
            { "SelectedNpc1", 34 },
            { "SelectedNpc2", 35 },
            { "SelectedNpc3", 36 },
            { "Npc1", 10 },
            { "Npc2", 11 },
            { "Npc3", 12 },
            { "Npc4", 13 },
            { "Npc5", 14 },
            { "Npc6", 15 },
            { "Npc1Level", 43 },
            { "Npc1Leve2", 44 },
            { "Npc1Leve3", 45 },
        };

        public Dawn() : base("Dawn")
        {
            npcList = new List<TrustNPC>
            {
                new TrustNPC("Alphinaud", 82061, 82081, 1),
                new TrustNPC("Alisaie", 82062, 82082, 2),
                new TrustNPC("Thancred", 82063, 82083, 3),
                new TrustNPC("Minfilia", 82064, 82084, 4),
                new TrustNPC("Urianger", 82065, 82085, 5),
                new TrustNPC("Y'shtola", 82066, 82086, 6),
                new TrustNPC("Ryne", 82067, 82087, 7),
                new TrustNPC("Lyna", 82068, 82088, 8),
                new TrustNPC("Crystal Exarch", 82069, 82089, 9),
                new TrustNPC("Crystal Exarch", 82069, 82089, 9),
                new TrustNPC("Crystal Exarch", 82069, 82089, 9)
            };
        }

        public int NumberOfTrustsAvailable => Elements[Properties["NumberOfTrustsAvailable"]].TrimmedData;
        public int SelectedTrustId => Elements[Properties["SelectedTrustId"]].TrimmedData;
        public string SelectedTrustName => Core.Memory.ReadString((IntPtr)Elements[Properties["SelectedTrustName"]].Data, Encoding.UTF8);

        public TrustNPC? SelectedNpc1 => GetTrustNpc(Elements[Properties["SelectedNpc1"]].TrimmedData);
        public TrustNPC? SelectedNpc2 => GetTrustNpc(Elements[Properties["SelectedNpc2"]].TrimmedData);
        public TrustNPC? SelectedNpc3 => GetTrustNpc(Elements[Properties["SelectedNpc3"]].TrimmedData);

        public TrustNPC? Npc1 => GetTrustNpc(Elements[Properties["Npc1"]].TrimmedData);
        public TrustNPC? Npc2 => GetTrustNpc(Elements[Properties["Npc2"]].TrimmedData);
        public TrustNPC? Npc3 => GetTrustNpc(Elements[Properties["Npc3"]].TrimmedData);
        public TrustNPC? Npc4 => GetTrustNpc(Elements[Properties["Npc4"]].TrimmedData);
        public TrustNPC? Npc5 => GetTrustNpc(Elements[Properties["Npc5"]].TrimmedData);
        public TrustNPC? Npc6 => GetTrustNpc(Elements[Properties["Npc6"]].TrimmedData);

        public int Npc1Level => Elements[Properties["Npc1Level"]].TrimmedData;
        public int Npc1Leve2 => Elements[Properties["Npc1Leve2"]].TrimmedData;
        public int Npc1Leve3 => Elements[Properties["Npc1Leve3"]].TrimmedData;

        public bool CanRegister()
        {
            if (WindowByName == null)
            {
                return false;
            }

            var remoteButton = WindowByName.FindButton(36);
            return remoteButton != null && remoteButton.Clickable;
        }

        public void Register()
        {
            if (WindowByName != null)
            {
                WindowByName.SendAction(1, 3, 14);
            }
        }

        public void SetTrust(int trust)
        {
            if (WindowByName != null)
            {
                WindowByName.SendAction(2, 3, 15, 4, (ulong)trust);
            }
        }

        public override void Close()
        {
            if (WindowByName != null)
            {
                WindowByName.SendAction(1, 3, 0);
            }
        }

        public void PressNpcSelection(int npc)
        {
            if (WindowByName != null && npc < 6)
            {
                WindowByName.SendAction(2, 3, 12, 4, (ulong)npc);
            }
        }

        public void ToggleScenario()
        {
            if (WindowByName != null)
            {
                WindowByName.SendAction(1, 3, 17);
            }
        }

        private TrustNPC? GetTrustNpc(int id)
        {
            return npcList.Any(i => i.Id1 == id || i.Id2 == id) ? npcList.FirstOrDefault(i => i.Id1 == id || i.Id2 == id) : null;
        }
    }

    public class TrustNPC
    {
        public TrustNPC(string name, int id1, int id2, int classId)
        {
            Name = name;
            Id1 = id1;
            Id2 = id2;
            ClassId = classId;
        }

        public string Name { get; }
        public int Id1 { get; }
        public int Id2 { get; }
        public int ClassId { get; }
    }
}