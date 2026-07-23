using System;
using System.Collections.Generic;
using System.Text;
using ff14bot;
using LlamaLibrary.ClientDataHelpers;
using LlamaLibrary.RemoteWindows.Atk;
using AtkValueType = LlamaLibrary.RemoteWindows.Atk.ValueType;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    /// Read-only access to the autonomous adventurer squadron mission window.
    /// </summary>
    public class GcArmyExpedition : RemoteWindow<GcArmyExpedition>
    {
        private const int MissionStart = 9;
        private const int MissionStride = 4;

        public GcArmyExpedition() : base("GcArmyExpedition")
        {
        }

        private const int NumberArrayIndex = 113;
        private const int AgentSelectedTabOffset = 0x40;
        private const int AgentSelectedRowOffset = 0x44;

        public int SelectedCategory => ReadAgentInt(AgentSelectedTabOffset, ReadInt(0));
        public GcArmyMissionCategory SelectedCategoryType => (GcArmyMissionCategory)SelectedCategory;
        public int CategoryCount => ReadInt(1);
        public int SelectedMissionIndex => ReadAgentInt(AgentSelectedRowOffset, -1);
        public bool CanDeploy => ReadBool(8);

        /// <summary>Gets the selected mission's game-data row ID.</summary>
        public int SelectedMissionId => ReadNumber(1);

        /// <summary>Gets the squadron's current accumulated EXP.</summary>
        public int CurrentSquadronExperience => ReadNumber(0);

        /// <summary>Gets the Squadron EXP awarded by the selected mission.</summary>
        public int MissionExperienceReward => ReadNumber(3);

        /// <summary>Gets the Squadron EXP awarded by the selected mission.</summary>
        public int SquadronExperience => MissionExperienceReward;

        /// <summary>Gets the company-seal expenditure required to deploy.</summary>
        public int Expenditure => ReadNumber(7);

        // The GcArmyExpedition number array stores both required values and
        // current totals in the UI's displayed P/M/T order. Sapper Strike
        // confirmed required indices 8/9/10 as 370/355/345.
        public int RequiredPhysical => ReadNumber(8);
        public int RequiredMental => ReadNumber(9);
        public int RequiredTactical => ReadNumber(10);
        public int CurrentPhysical => ReadNumber(11);
        public int CurrentMental => ReadNumber(12);
        public int CurrentTactical => ReadNumber(13);

        /// <summary>
        /// Gets the live addon number-array data used by counter nodes, including
        /// mission requirement and current-squadron attribute totals.
        /// </summary>
        public IReadOnlyList<int> NumberData
        {
            get
            {
                var numberArray = AtkArrayDataHolder.NumberArray(NumberArrayIndex);
                if (numberArray == IntPtr.Zero)
                {
                    return Array.Empty<int>();
                }

                var count = Math.Min(Math.Max(Core.Memory.Read<int>(numberArray + 0x8), 0), 1024);
                var data = Core.Memory.Read<IntPtr>(numberArray + 0x28);
                return data == IntPtr.Zero || count == 0
                    ? Array.Empty<int>()
                    : Core.Memory.ReadArray<int>(data, count);
            }
        }

        public IReadOnlyList<string> Categories => new[] { ReadString(2), ReadString(3), ReadString(4) };

        /// <summary>Gets the localized visible label for the selected category.</summary>
        public string SelectedCategoryName
        {
            get
            {
                var categories = Categories;
                return SelectedCategory >= 0 && SelectedCategory < categories.Count
                    ? categories[SelectedCategory]
                    : SelectedCategoryType.ToString();
            }
        }

        /// <summary>Selects an autonomous mission category currently unlocked for this squadron.</summary>
        public bool SelectCategory(int index)
        {
            // Fresh squadrons initially expose only Trainee Missions. Routine
            // and Priority tabs are added later, so validate against the live
            // category count without dereferencing localized label pointers.
            var categoryCount = Math.Min(3, Math.Max(CategoryCount, 0));
            if (!IsOpen || index < 0 || index >= categoryCount)
            {
                return false;
            }

            SendAction(
                true,
                (AtkValueType.Int, 0xB),
                (AtkValueType.Int, index),
                (AtkValueType.Undefined, 0),
                (AtkValueType.Undefined, 0),
                (AtkValueType.Undefined, 0),
                (AtkValueType.Undefined, 0));
            return true;
        }

        public bool SelectCategory(GcArmyMissionCategory category) => SelectCategory((int)category);

        /// <summary>
        /// Selects a mission using its row index in the current category.
        /// Callers should wait for SelectedMissionIndex to match before continuing.
        /// </summary>
        public bool SelectMission(int rowIndex)
        {
            if (!IsOpen || rowIndex < 0)
            {
                return false;
            }

            SendAction(
                true,
                (AtkValueType.Int, 0xC),
                (AtkValueType.Int, rowIndex),
                (AtkValueType.Undefined, 0),
                (AtkValueType.Undefined, 0),
                (AtkValueType.Undefined, 0),
                (AtkValueType.Undefined, 0));
            return true;
        }

        /// <summary>Opens the Edit Squadron roster for the selected mission.</summary>
        public void OpenEditSquad()
        {
            SendAction(
                true,
                (AtkValueType.Int, 0xD),
                (AtkValueType.Undefined, 0),
                (AtkValueType.Undefined, 0),
                (AtkValueType.Undefined, 0),
                (AtkValueType.Undefined, 0),
                (AtkValueType.Undefined, 0),
                (AtkValueType.Undefined, 0));
        }

        /// <summary>
        /// Opens the deployment confirmation for the currently selected mission.
        /// The caller must validate the selected mission, active squad, and CanDeploy
        /// before invoking this action and handle the resulting confirmation window.
        /// </summary>
        public bool Deploy()
        {
            // City Patrol is row/mission 0 and is a valid Rank 1 deployment.
            if (!IsOpen || SelectedMissionIndex < 0 || !CanDeploy)
            {
                return false;
            }

            SendAction(true, (AtkValueType.Int, 0xE));
            return true;
        }

        public IReadOnlyList<GcArmyMission> Missions
        {
            get
            {
                var result = new List<GcArmyMission>();
                var values = Elements;
                var count = Math.Min(Math.Max(ReadInt(values, 6), 0), 50);
                for (var index = 0; index < count; index++)
                {
                    var offset = MissionStart + (index * MissionStride);
                    if (offset + 3 >= values.Length || values[offset + 1].Data == 0)
                    {
                        break;
                    }

                    result.Add(new GcArmyMission(
                        index,
                        values[offset].TrimmedData,
                        ReadString(values, offset + 1),
                        ReadString(values, offset + 2),
                        values[offset + 3].TrimmedData != 0));
                }

                return result;
            }
        }

        private int ReadInt(int index)
        {
            var values = Elements;
            return ReadInt(values, index);
        }

        private static int ReadInt(ff14bot.RemoteWindows.TwoInt[] values, int index)
            => index < values.Length ? values[index].TrimmedData : 0;

        private bool ReadBool(int index) => ReadInt(index) != 0;

        private int ReadNumber(int index)
        {
            var values = NumberData;
            return index >= 0 && index < values.Count ? values[index] : 0;
        }

        private int ReadAgentInt(int offset, int fallback)
        {
            var agent = WindowByName?.TryFindAgentInterface();
            return agent == null || agent.Pointer == IntPtr.Zero
                ? fallback
                : Core.Memory.Read<int>(agent.Pointer + offset);
        }

        private string ReadString(int index)
        {
            var values = Elements;
            return index < values.Length ? ReadString(values, index) : string.Empty;
        }

        private static string ReadString(ff14bot.RemoteWindows.TwoInt[] values, int index)
        {
            if (index < 0 || index >= values.Length || values[index].Data == 0)
            {
                return string.Empty;
            }

            try
            {
                return Core.Memory.ReadString((IntPtr)values[index].Data, Encoding.UTF8);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // Addon string storage can be replaced between Elements and
                // ReadString while switching tabs. A missing label must not
                // abort or restart the Squadron workflow.
                return string.Empty;
            }
        }
    }

    public enum GcArmyMissionCategory
    {
        TraineeMission = 0,
        RoutineMission = 1,
        PriorityMission = 2
    }

    public sealed class GcArmyMission
    {
        public GcArmyMission(int index, int id, string name, string levelText, bool unlocked)
        {
            Index = index;
            Id = id;
            Name = name;
            LevelText = levelText;
            Unlocked = unlocked;
        }

        public int Index { get; }
        public int Id { get; }
        public string Name { get; }
        public string LevelText { get; }
        public bool Unlocked { get; }
    }
}
