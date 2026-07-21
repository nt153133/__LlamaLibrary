using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ff14bot;
using AtkValueType = LlamaLibrary.RemoteWindows.Atk.ValueType;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    /// Read-only access to the adventurer squadron training window.
    /// </summary>
    public class GcArmyTraining : RemoteWindow<GcArmyTraining>
    {
        private static readonly string[] AttributeChanges =
        {
            "+40 physical, -20 mental, -20 tactical",
            "-20 physical, +40 mental, -20 tactical",
            "-20 physical, -20 mental, +40 tactical",
            "+20 physical, +20 mental, -40 tactical",
            "+20 physical, -40 mental, +20 tactical",
            "-40 physical, +20 mental, +20 tactical",
            "No attribute change"
        };

        public GcArmyTraining() : base("GcArmyTraining")
        {
        }

        /// <summary>
        /// Gets the zero-based regimen callback index. The addon stores 0 for no
        /// selection and 1-7 for regimen callbacks 0-6.
        /// </summary>
        public int SelectedIndex => IsOpen && Elements.Length > 11 ? Elements[11].TrimmedData - 1 : -1;
        public bool IsCompletedRegimen
            => IsOpen && string.IsNullOrEmpty(ReadString(2)) && !string.IsNullOrEmpty(ReadString(22));
        public int CurrentPhysical => ReadInt(13);
        public int PreviewPhysical => ReadInt(14);
        public int CurrentMental => ReadInt(15);
        public int PreviewMental => ReadInt(16);
        public int CurrentTactical => ReadInt(17);
        public int PreviewTactical => ReadInt(18);
        public int AttributeCap => ReadInt(21);
        public string AllocationText => ReadString(19);
        public string SessionsRemainingText => ReadString(23);
        public int SessionsRemaining
        {
            get
            {
                var match = Regex.Match(SessionsRemainingText, @"\d+\s*$");
                return match.Success && int.TryParse(match.Value, out var value) ? value : -1;
            }
        }

        public IReadOnlyList<GcArmyTrainingRegimen> Regimens
        {
            get
            {
                var result = new List<GcArmyTrainingRegimen>();
                for (var index = 0; index < AttributeChanges.Length; index++)
                {
                    result.Add(new GcArmyTrainingRegimen(index, ReadString(2 + index), AttributeChanges[index]));
                }

                return result;
            }
        }

        /// <summary>Selects a training regimen without commencing it.</summary>
        public bool SelectRegimen(int index)
        {
            if (!IsOpen || index < 0 || index >= AttributeChanges.Length)
            {
                return false;
            }

            SendAction(true, (AtkValueType.Int, 0xB), (AtkValueType.Int, index));
            return true;
        }

        /// <summary>
        /// Opens the confirmation for the selected training regimen.
        /// The caller is responsible for checking the preview and confirming SelectYesno.
        /// </summary>
        public bool CommenceSelected()
        {
            if (!IsOpen || SelectedIndex < 0 || SelectedIndex >= AttributeChanges.Length)
            {
                return false;
            }

            SendAction(true, (AtkValueType.Int, 0xC), (AtkValueType.Int, SelectedIndex));
            return true;
        }

        /// <summary>Acknowledges a completed one-hour regimen and closes the result window.</summary>
        public bool ConfirmCompletedRegimen()
        {
            if (!IsCompletedRegimen)
            {
                return false;
            }

            SendAction(true, (AtkValueType.Int, 0x1));
            return true;
        }

        private int ReadInt(int index)
        {
            var values = Elements;
            return index < values.Length ? values[index].TrimmedData : 0;
        }

        private string ReadString(int index)
        {
            var values = Elements;
            return index < values.Length && values[index].Data != 0
                ? Core.Memory.ReadString((IntPtr)values[index].Data, Encoding.UTF8)
                : string.Empty;
        }
    }

    public sealed class GcArmyTrainingRegimen
    {
        public GcArmyTrainingRegimen(int index, string name, string attributeChange)
        {
            Index = index;
            Name = name;
            AttributeChange = attributeChange;
        }

        public int Index { get; }
        public string Name { get; }
        public string AttributeChange { get; }
    }
}
