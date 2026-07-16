using LlamaLibrary.RemoteWindows.Atk;
using ff14bot.Managers;

namespace LlamaLibrary.RemoteWindows
{
    public enum BuddyTab
    {
        Actions = 0,
        Skills = 1,
        Appearance = 2,
    }

    public class Buddy : RemoteWindow<Buddy>
    {
        private const int BuddyAgentId = 110;

        public Buddy() : base("Buddy", AgentModule.GetAgentInterfaceById(BuddyAgentId))
        {
        }

        public void OpenActionsTab()
        {
            OpenTab(BuddyTab.Actions);
        }

        public void OpenSkillsTab()
        {
            OpenTab(BuddyTab.Skills);
        }

        public void OpenAppearanceTab()
        {
            OpenTab(BuddyTab.Appearance);
        }

        public void OpenTab(BuddyTab tab)
        {
            SendAction(true, (ValueType.Int, 0xF), (ValueType.Int, (int)tab), (ValueType.Undefined, 0));
        }
    }
}
