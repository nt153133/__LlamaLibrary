using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ff14bot;

namespace LlamaLibrary.RemoteWindows
{
    //TODO Move element numbers to dictionary
    public class Dawn : RemoteWindow<Dawn>
    {
        private readonly List<TrustNPC> npcList;

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

        public int NumberOfTrustsAvailable => Elements[73].TrimmedData;
        public int SelectedTrustId => Elements[74].TrimmedData;
        public string SelectedTrustName => Core.Memory.ReadString((IntPtr)Elements[75].Data, Encoding.UTF8);

        public TrustNPC? SelectedNpc1 => GetTrustNpc(Elements[34].TrimmedData);
        public TrustNPC? SelectedNpc2 => GetTrustNpc(Elements[35].TrimmedData);
        public TrustNPC? SelectedNpc3 => GetTrustNpc(Elements[36].TrimmedData);

        public TrustNPC? Npc1 => GetTrustNpc(Elements[10].TrimmedData);
        public TrustNPC? Npc2 => GetTrustNpc(Elements[11].TrimmedData);
        public TrustNPC? Npc3 => GetTrustNpc(Elements[12].TrimmedData);
        public TrustNPC? Npc4 => GetTrustNpc(Elements[13].TrimmedData);
        public TrustNPC? Npc5 => GetTrustNpc(Elements[14].TrimmedData);
        public TrustNPC? Npc6 => GetTrustNpc(Elements[15].TrimmedData);

        public int Npc1Level => Elements[43].TrimmedData;
        public int Npc1Leve2 => Elements[44].TrimmedData;
        public int Npc1Leve3 => Elements[45].TrimmedData;

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