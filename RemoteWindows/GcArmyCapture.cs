using System;
using System.Collections.Generic;
using System.Text;
using ff14bot;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    /// Access to the adventurer squadron command mission window.
    /// </summary>
    public class GcArmyCapture : RemoteWindow<GcArmyCapture>
    {
        private const int DutyStart = 11;
        private const int DutyStride = 6;

        public GcArmyCapture() : base("GcArmyCapture")
        {
        }

        public int DutyCount => IsOpen && Elements.Length > 7 ? Elements[7].TrimmedData : 0;
        public int SelectedDutyIndex => IsOpen && Elements.Length > 8 ? Elements[8].TrimmedData : -1;
        public bool CanCommence => IsOpen && Elements.Length > 10 && Elements[10].TrimmedData != 0;

        public IReadOnlyList<GcArmyCommandMission> Duties
        {
            get
            {
                var result = new List<GcArmyCommandMission>();
                var values = Elements;
                var count = Math.Min(Math.Max(DutyCount, 0), 50);
                for (var index = 0; index < count; index++)
                {
                    var offset = DutyStart + (index * DutyStride);
                    if (offset + 5 >= values.Length || values[offset + 1].Data == 0)
                    {
                        break;
                    }

                    result.Add(new GcArmyCommandMission(
                        index,
                        values[offset].TrimmedData,
                        ReadString(values, offset + 1),
                        ReadString(values, offset + 2),
                        values[offset + 3].TrimmedData != 0,
                        values[offset + 4].TrimmedData != 0,
                        values[offset + 5].TrimmedData != 0));
                }

                return result;
            }
        }

        public void Commence()
        {
            SendAction(1, 3, 0xd);
        }

        /// <summary>
        /// Sets the squadron command mission.
        /// </summary>
        /// <param name="index">The duty index from the list starting at 0.</param>
        public void SelectDuty(int index)
        {
            SendAction(2, 3, 0xB, 4, (ulong)index);
        }

        private static string ReadString(ff14bot.RemoteWindows.TwoInt[] values, int index)
            => values[index].Data == 0 ? string.Empty : Core.Memory.ReadString((IntPtr)values[index].Data, Encoding.UTF8);
    }

    public sealed class GcArmyCommandMission
    {
        public GcArmyCommandMission(int index, int id, string name, string levelText, bool flag1, bool flag2, bool flag3)
        {
            Index = index;
            Id = id;
            Name = name;
            LevelText = levelText;
            Flag1 = flag1;
            Flag2 = flag2;
            Flag3 = flag3;
        }

        public int Index { get; }
        public int Id { get; }
        public string Name { get; }
        public string LevelText { get; }
        public bool Flag1 { get; }
        public bool Flag2 { get; }
        public bool Flag3 { get; }
    }
}
