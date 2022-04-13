using System.Collections.Generic;
using System.Threading.Tasks;
using Buddy.Coroutines;
using LlamaLibrary.RemoteAgents;

namespace LlamaLibrary.RemoteWindows
{
    public class DawnStory : RemoteWindow<DawnStory>
    {
        private const string WindowName = "DawnStory";

        public DawnStory() : base(WindowName)
        {
            _name = WindowName;
        }

        public async Task<bool> SelectDuty(int dutyId)
        {
            if (!await Coroutine.Wait(8000, () => AgentDawnStory.Instance.IsLoaded))
            {
                return false;
            }

            var duties = AgentDawnStory.Instance.Duties;
            int index = -1;
            for (int i = 0; i < duties.Length; i++)
            {
                if (duties[i].DutyId == dutyId)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                return false;
            }

            if (AgentDawnStory.Instance.SelectedDuty == duties[index].TableKey)
            {
                return true;
            }

            SendAction(2, 3, 0xC, 4, (ulong)index);

            return await Coroutine.Wait(5000, () => AgentDawnStory.Instance.SelectedDuty == duties[index].TableKey);
        }

        public void Commence()
        {
            SendAction(1, 3, 0xE);
        }
    }
}