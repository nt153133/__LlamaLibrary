using System;
using ff14bot;
using LlamaLibrary.RemoteWindows.Atk;
using AtkValueType = LlamaLibrary.RemoteWindows.Atk.ValueType;

namespace LlamaLibrary.RemoteWindows
{
    public enum BuddySkillRole
    {
        Defender = 0,
        Attacker = 1,
        Healer = 2,
    }

    public class BuddySkill : RemoteWindow<BuddySkill>
    {
        private const int BuddyOffset = 0x1EF0;
        private const int CompanionInfoOffset = 0x2370;
        private const int CompanionSkillPointsOffset = 0x3A;
        private const int CompanionLevelsOffset = 0x3B;
        private const int CompanionActiveCommandOffset = 0x3E;

        public BuddySkill() : base("BuddySkill")
        {
        }

        public int SkillPoints
        {
            get => ReadCompanionInfoByte(CompanionSkillPointsOffset);
        }

        public int DefenderLevel => GetRoleLevel(BuddySkillRole.Defender);

        public int AttackerLevel => GetRoleLevel(BuddySkillRole.Attacker);

        public int HealerLevel => GetRoleLevel(BuddySkillRole.Healer);

        public int ActiveCommand => ReadCompanionInfoByte(CompanionActiveCommandOffset);

        public int GetRoleLevel(BuddySkillRole role)
        {
            return ReadCompanionInfoByte(CompanionLevelsOffset + (int)role);
        }

        public int GetNextSkillCost(BuddySkillRole role)
        {
            return GetRoleLevel(role) + 1;
        }

        public bool CanLearnNextSkill(BuddySkillRole role)
        {
            var roleLevel = GetRoleLevel(role);
            return roleLevel < 10 && SkillPoints >= roleLevel + 1;
        }

        public bool TryGetActiveRole(out BuddySkillRole role)
        {
            switch (ActiveCommand)
            {
                case 0:
                    role = BuddySkillRole.Defender;
                    return true;
                case 1:
                    role = BuddySkillRole.Attacker;
                    return true;
                case 2:
                    role = BuddySkillRole.Healer;
                    return true;
                default:
                    role = BuddySkillRole.Attacker;
                    return false;
            }
        }

        public bool CanLearnNextSkillForActiveRole()
        {
            return TryGetActiveRole(out var role) && CanLearnNextSkill(role);
        }

        public void LearnNextDefenderSkill()
        {
            LearnNextSkill(BuddySkillRole.Defender);
        }

        public void LearnNextAttackerSkill()
        {
            LearnNextSkill(BuddySkillRole.Attacker);
        }

        public void LearnNextHealerSkill()
        {
            LearnNextSkill(BuddySkillRole.Healer);
        }

        public void LearnNextSkill(BuddySkillRole role)
        {
            SendAction(true, (AtkValueType.Int, 0xE), (AtkValueType.Int, (int)role), (AtkValueType.Undefined, 0));
        }

        private static int ReadCompanionInfoByte(int offset)
        {
            var uiState = Helpers.UIState.Instance;
            return uiState == IntPtr.Zero
                ? 0
                : Core.Memory.Read<byte>(IntPtr.Add(uiState, BuddyOffset + CompanionInfoOffset + offset));
        }
    }
}
