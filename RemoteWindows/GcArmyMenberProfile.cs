namespace LlamaLibrary.RemoteWindows
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using ff14bot;
    using ff14bot.Managers;
    using ff14bot.RemoteWindows;
    using AtkValueType = Atk.ValueType;

    /// <summary>
    /// Read-only access to the adventurer squadron enlistment-paper profile.
    /// The game addon name intentionally spells "Menber".
    /// </summary>
    public class GcArmyMenberProfile : RemoteWindow<GcArmyMenberProfile>
    {
        public GcArmyMenberProfile() : base("GcArmyMenberProfile")
        {
        }

        public override AtkAddonControl? WindowByName
        {
            get
            {
                // Controls is the manager's de-duplicated view and can expose
                // only the first addon when the game draws the candidate and
                // selected roster member with the same addon name. The raw
                // collection retains both addon instances and their pointers.
                var windows = RaptureAtkUnitManager.GetRawControls
                    .Where(control => control != null && control.IsVisible && control.Name == WindowName)
                    .ToArray();

                // Capacity replacement draws the candidate and selected member
                // as two instances of the same addon. Prefer the existing-member
                // profile so Name/Discharge target the selected roster member.
                return windows.FirstOrDefault(IsExistingProfile) ?? windows.FirstOrDefault();
            }
        }

        public string PortraitPath => ReadString(0);
        public string Name => ReadString(1);
        public string RaceAndGender => ReadString(2);
        public string RecruitmentArea => ReadString(3);
        public int ClassIndex => ReadInt(4);
        public string ClassName => ReadString(5);
        public string LevelText => ReadString(6);
        public int Level => ParseTrailingInteger(LevelText);
        public string ExperienceText => ReadString(7);
        public int Physical => ReadInt(8);
        public int Mental => ReadInt(9);
        public int Tactical => ReadInt(10);
        public string ChemistryEffect => ReadString(11);
        public string ChemistryCondition => ReadString(12);
        public string ProfileMessage => ReadString(13);
        public string EnlistmentReason => ReadString(13);
        public string PrimaryActionLabel => ReadString(15);
        public string DismissLabel => ReadString(16);
        public string PostponeLabel => ReadString(17);
        public string EnlistedDateText => ReadString(22);

        // The capacity-replacement screen is one addon, even though it draws
        // the pending candidate on the left and the selected roster member on
        // the right. Atk values 21 and 24 distinguish the actionable existing-
        // member profile from the candidate profile without localized labels.
        public bool IsExistingMemberProfile => ReadBool(21) && ReadBool(24);
        public bool IsCandidateProfile => IsOpen && !IsExistingMemberProfile;
        public bool IsQuestionState => IsCandidateProfile && ReadBool(23);
        public bool IsRecruitState => IsCandidateProfile && !ReadBool(23);
        public bool HasPrimaryAction => !string.IsNullOrWhiteSpace(PrimaryActionLabel);
        public bool HasSecondaryAction => !string.IsNullOrWhiteSpace(DismissLabel);
        public bool HasReturnAction => !string.IsNullOrWhiteSpace(PostponeLabel);

        /// <summary>
        /// Raw trailing profile classification. Captured candidates commonly
        /// expose values such as "Independent" here.
        /// </summary>
        public string Classification => ReadString(36);

        /// <summary>Begins the candidate-question conversation.</summary>
        public bool Question()
        {
            if (!IsCandidateProfile)
            {
                return false;
            }

            return ActivatePrimaryAction();
        }

        /// <summary>
        /// Activates Recruit after Question has changed the state of the primary
        /// button. The game uses the same 0xD callback for both labels.
        /// Callers must verify <see cref="PrimaryActionLabel"/> changed before use.
        /// </summary>
        public bool Recruit()
        {
            if (!IsCandidateProfile)
            {
                return false;
            }

            return ActivatePrimaryAction();
        }

        /// <summary>
        /// Opens the confirmation used to permanently dismiss the current
        /// candidate. Callback 0xE is only valid on a candidate profile.
        /// </summary>
        public bool Dismiss()
        {
            if (!IsCandidateProfile || !HasSecondaryAction)
            {
                return false;
            }

            SendAction(true, (AtkValueType.Int, 0xE));
            return true;
        }

        /// <summary>
        /// Opens the discharge confirmation after an existing member has been
        /// selected during a capacity replacement. The game reuses callback 0xD.
        /// Callers must verify the primary label/state before use.
        /// </summary>
        public bool Discharge()
        {
            if (!IsExistingMemberProfile)
            {
                return false;
            }

            return ActivatePrimaryAction();
        }

        /// <summary>
        /// Activates the single primary button currently exposed by the addon.
        /// Callback 0xD is reused for Question, Recruit, and Discharge; those
        /// actions are mutually exclusive states, never simultaneous buttons.
        /// </summary>
        public bool ActivatePrimaryAction()
        {
            if (!IsOpen || !HasPrimaryAction)
            {
                return false;
            }

            SendAction(true, (AtkValueType.Int, 0xD));
            return true;
        }

        /// <summary>Postpones the enlistment decision without dismissing the candidate.</summary>
        public bool Postpone()
        {
            if (!IsOpen)
            {
                return false;
            }

            SendAction(true, (AtkValueType.Int, 0xF));
            return true;
        }

        /// <summary>
        /// Returns from the selected-member discharge profile to the capacity
        /// selection list. The game reuses the 0xF callback used by Postpone.
        /// </summary>
        public bool Return()
        {
            if (!IsOpen || !IsExistingMemberProfile || !HasReturnAction)
            {
                return false;
            }

            SendAction(true, (AtkValueType.Int, 0xF));
            return true;
        }

        private int ReadInt(int index)
        {
            var values = Elements;
            return index < values.Length ? values[index].TrimmedData : 0;
        }

        private bool ReadBool(int index)
        {
            var values = Elements;
            return index < values.Length && values[index].TrimmedData != 0;
        }

        private static bool IsExistingProfile(AtkAddonControl control)
        {
            var values = ReadElements(control);
            return values.Length > 24 && values[21].TrimmedData != 0 && values[24].TrimmedData != 0;
        }

        private string ReadString(int index)
        {
            var values = Elements;
            return index < values.Length && values[index].Data != 0
                ? Core.Memory.ReadString((IntPtr)values[index].Data, Encoding.UTF8)
                : string.Empty;
        }

        private static int ParseTrailingInteger(string text)
        {
            var match = Regex.Match(text ?? string.Empty, @"\d+\s*$");
            return match.Success && int.TryParse(match.Value, out var value) ? value : 0;
        }
    }
}
