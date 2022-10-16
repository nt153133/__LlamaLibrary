using System.Linq;
using ff14bot.Managers;

namespace LlamaLibrary.RemoteWindows
{
    public class Mobhunt : RemoteWindow<Mobhunt>
    {
        public override AtkAddonControl WindowByName => RaptureAtkUnitManager.Controls.FirstOrDefault(i => i.Name.Contains(WindowName) && i.IsVisible);

        public Mobhunt() : base("Mobhunt")
        {
        }

        public void Accept()
        {
            SendAction(1, 3, 0);
        }
    }
}