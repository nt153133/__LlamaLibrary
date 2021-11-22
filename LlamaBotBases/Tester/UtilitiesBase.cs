using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Pathing.Service_Navigation;
using LlamaBotBases.Tester.Tasks;
using LlamaLibrary.Extensions;
using LlamaLibrary.Logging;
using Newtonsoft.Json;
using TreeSharp;

namespace LlamaBotBases.Tester
{
    public class UtilitiesBase: BotBase
    {
        public static readonly string _name = "Llama Utilities";
        private static readonly LLogger Log = new LLogger(_name, Colors.Pink);
        public static BotTask BotTask = new BotTask();

        public override bool WantButton => true;
        public override string Name => _name;
        public override PulseFlags PulseFlags => PulseFlags.All;
        public override bool IsAutonomous => true;
        public override bool RequiresProfile => false;
        public override Composite Root => _root;

        private Composite _root;
        private Utilities settings;

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Start()
        {
            Navigator.PlayerMover = new SlideMover();
            Navigator.NavigationProvider = new ServiceNavigationProvider();

            if (BotTask.Type == TaskType.None)
            {
                Log.Error("Use the settings window to run a task");
                return;
            }

            if (BotTask.Type == TaskType.AutoFollow)
            {
                //Just....no that botbase is shit
            }
            else
            {
                _root = new ActionRunCoroutine(r => Run());
            }
        }

        public override void Stop()
        {
            base.Stop();
        }

        public override void OnButtonPress()
        {
            if (settings == null || settings.IsDisposed)
                settings = new Utilities();
            try
            {
                settings.Show();
                settings.Activate();
            }
            catch
            {
                // ignored
            }
        }

        private async Task<bool> Run()
        {
            switch (BotTask.Type)
            {
                case TaskType.MateriaRemove:
                    var bagInfo = JsonConvert.DeserializeObject<(uint, ushort)>(BotTask.TaskInfo);
                    var slot = InventoryManager.GetBagByInventoryBagId((InventoryBagId) bagInfo.Item1).First(i => i.Slot == bagInfo.Item2);
                    await RemoveMateria(slot);
                    break;
                case TaskType.AutoFollow:
                    break;
                case TaskType.Reduce:
                    break;
                case TaskType.Desynth:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            BotTask.Type = TaskType.None;

            TreeRoot.Stop("Stop Requested");
            return true;
        }

        public static async Task<bool> RemoveMateria(BagSlot bagSlot)
        {
            if (bagSlot != null && bagSlot.IsValid)
            {
                Log.Information($"Want to remove Materia from {bagSlot}");
                var count = bagSlot.MateriaCount();
                for (var i = 0; i < count; i++)
                {
                    Log.Information($"Removing materia {count - i}");
                    bagSlot.RemoveMateria();
                    await Coroutine.Sleep(6000);
                }
            }

            Log.Information($"Item now has {bagSlot.MateriaCount()} materia affixed");

            return true;
        }

    }
}