using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ff14bot;
using LlamaLibrary.RemoteWindows.Atk;
using AtkValueType = LlamaLibrary.RemoteWindows.Atk.ValueType;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    /// Read-only access to the adventurer squadron roster window.
    /// </summary>
    public class GcArmyMemberList : RemoteWindow<GcArmyMemberList>
    {
        private const int MemberStart = 5;
        private const int MemberStride = 15;
        private const int MaximumMembers = 8;

        public GcArmyMemberList() : base("GcArmyMemberList")
        {
        }

        public string RankText => ReadString(0);
        public string AttributeText => ReadString(1);
        public string AllocationText => ReadString(2);
        public int TrainingPhysical => ReadAttribute(0);
        public int TrainingMental => ReadAttribute(1);
        public int TrainingTactical => ReadAttribute(2);

        /// <summary>
        /// Clicks a member row using the addon's verified callback. Callers
        /// should verify the resulting active state before continuing.
        /// </summary>
        public bool SelectMember(int slot)
        {
            if (!IsOpen || slot < 0 || slot >= Members.Count)
            {
                return false;
            }

            SendAction(true, (AtkValueType.Int, 0xB), (AtkValueType.Int, slot));
            return true;
        }

        public IReadOnlyList<GcArmyMember> Members
        {
            get
            {
                var result = new List<GcArmyMember>();
                if (!IsOpen)
                {
                    return result;
                }

                var values = Elements;
                for (var slot = 0; slot < MaximumMembers; slot++)
                {
                    var offset = MemberStart + (slot * MemberStride);
                    if (offset + 14 >= values.Length)
                    {
                        continue;
                    }

                    var name = ReadString(values, offset + 1);
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    var state = values[offset].TrimmedData;
                    result.Add(new GcArmyMember(
                        slot,
                        name,
                        ReadString(values, offset + 2),
                        ReadString(values, offset + 3),
                        values[offset + 4].TrimmedData,
                        values[offset + 5].TrimmedData,
                        state,
                        values[offset + 8].TrimmedData,
                        values[offset + 9].TrimmedData,
                        values[offset + 10].TrimmedData,
                        ReadString(values, offset + 13)));
                }

                return result;
            }
        }

        private string ReadString(int index)
        {
            var values = Elements;
            return index < values.Length ? ReadString(values, index) : string.Empty;
        }

        private static string ReadString(ff14bot.RemoteWindows.TwoInt[] values, int index)
            => values[index].Data == 0 ? string.Empty : Core.Memory.ReadString((IntPtr)values[index].Data, Encoding.UTF8);

        private int ReadAttribute(int index)
        {
            var match = Regex.Match(AttributeText, @"(\d+)\s*/\s*(\d+)\s*/\s*(\d+)");
            return match.Success && int.TryParse(match.Groups[index + 1].Value, out var value) ? value : 0;
        }
    }

    public sealed class GcArmyMember
    {
        public GcArmyMember(int slot, string name, string className, int classId, int level, bool isActive, int physical, int mental, int tactical, string tactic)
            : this(slot, name, className, string.Empty, classId, level, isActive ? 3 : 0, physical, mental, tactical, tactic)
        {
        }

        public GcArmyMember(int slot, string name, string className, string portraitTexturePath, int classId, int level, bool isActive, int physical, int mental, int tactical, string tactic)
            : this(slot, name, className, portraitTexturePath, classId, level, isActive ? 3 : 0, physical, mental, tactical, tactic)
        {
        }

        public GcArmyMember(int slot, string name, string className, string portraitTexturePath, int classId, int level, int state, int physical, int mental, int tactical, string tactic)
        {
            Slot = slot;
            Name = name;
            ClassName = className;
            PortraitTexturePath = portraitTexturePath;
            ClassId = classId;
            Level = level;
            State = state;
            Physical = physical;
            Mental = mental;
            Tactical = tactical;
            Tactic = tactic;
        }

        public int Slot { get; }
        public string Name { get; }
        public string ClassName { get; }
        public string PortraitTexturePath { get; }
        public int ClassId { get; }
        public int Level { get; }
        public int State { get; }
        public bool IsActive => State == 3;
        public bool IsOnMission => State == 4;
        public int Physical { get; }
        public int Mental { get; }
        public int Tactical { get; }
        public string Tactic { get; }
    }
}
