using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.XmlEngine;
using ff14bot.Behavior;
using ff14bot.Navigation;
using ff14bot.NeoProfiles;
using ff14bot.Pathing.Service_Navigation;
using ff14bot.RemoteWindows;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Utilities;
using TreeSharp;

namespace LlamaBotBases.OrderbotTags
{
    [XmlElement("LLGoToHousing")]
    public class LLGoToHousing : LLProfileBehavior
    {
        private bool _isDone;

        [XmlAttribute("District")]
        [XmlAttribute("district")]
        [DefaultValue("Mists")]
        public string District { get; set; }

        [XmlAttribute("Ward")]
        [XmlAttribute("ward")]
        [DefaultValue(0)]
        public int Ward { get; set; }


        public override bool HighPriority => true;

        public override bool IsDone => _isDone;

        public LLGoToHousing() : base() { }

        protected override void OnStart()
        {
        }

        protected override void OnDone()
        {
        }

        protected override void OnResetCachedDone()
        {
            _isDone = false;
        }

        protected override Composite CreateBehavior()
        {
            return new ActionRunCoroutine(r => GoToWard(Ward, District));
        }

        private async Task GoToWard(int ward, string district)
        {

            Navigator.PlayerMover = new SlideMover();
            Navigator.NavigationProvider = new ServiceNavigationProvider();

            if (district.Equals("Lavender Beds", StringComparison.InvariantCultureIgnoreCase))
            {
                if (ConditionParser.IsQuestCompleted(66748))
                {
                    await Housing.GetToResidential(2);
                    await Housing.OpenHousingWards();
                    Log.Information($"Traveling to ward {district} Ward - {ward}");
                    HousingSelectBlock.Instance.SelectWard(ward - 1);
                    HousingSelectBlock.Instance.GoToWard(ward - 1);
                    await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
                    if (SelectYesno.IsOpen)
                    {
                        SelectYesno.Yes();
                    }

                    await Coroutine.Sleep(5000);

                    if (CommonBehaviors.IsLoading)
                    {
                        await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
                    }
                }
                else
                {
                    Log.Error("Please complete the quest 'Where the Heart Is (The Lavender Beds)'");
                }


            }

            if (district.Equals("Mists", StringComparison.InvariantCultureIgnoreCase))
            {
                if (ConditionParser.IsQuestCompleted(66750))
                {
                    await Housing.GetToResidential(8);
                    await Housing.OpenHousingWards();
                    Log.Information($"Traveling to ward {district} Ward - {ward}");
                    HousingSelectBlock.Instance.SelectWard(ward - 1);
                    HousingSelectBlock.Instance.GoToWard(ward - 1);
                    await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
                    if (SelectYesno.IsOpen)
                    {
                        SelectYesno.Yes();
                    }

                    await Coroutine.Sleep(5000);

                    if (CommonBehaviors.IsLoading)
                    {
                        await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
                    }
                }
                else
                {
                    Log.Error("Please complete the quest 'Where the Heart Is (The Mists)'");
                }
            }

            if (district.Equals("Goblet", StringComparison.InvariantCultureIgnoreCase))
            {
                if (ConditionParser.IsQuestCompleted(66749))
                {
                    await Housing.GetToResidential(9);
                    await Housing.OpenHousingWards();
                    Log.Information($"Traveling to ward {district} Ward - {ward}");
                    HousingSelectBlock.Instance.SelectWard(ward - 1);
                    HousingSelectBlock.Instance.GoToWard(ward - 1);
                    await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
                    if (SelectYesno.IsOpen)
                    {
                        SelectYesno.Yes();
                    }

                    await Coroutine.Sleep(5000);

                    if (CommonBehaviors.IsLoading)
                    {
                        await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
                    }
                }
                else
                {
                    Log.Error("Please complete the quest 'Where the Heart Is (The Goblet)'");
                }
            }

            if (district.Equals("Shirogane", StringComparison.InvariantCultureIgnoreCase))
            {
                if (ConditionParser.IsQuestCompleted(68167))
                {
                    await Housing.GetToResidential(111);
                    await Housing.OpenHousingWards();
                    Log.Information($"Traveling to ward {district} Ward - {ward}");
                    HousingSelectBlock.Instance.SelectWard(ward - 1);
                    HousingSelectBlock.Instance.GoToWard(ward - 1);
                    await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
                    if (SelectYesno.IsOpen)
                    {
                        SelectYesno.Yes();
                    }

                    await Coroutine.Sleep(5000);

                    if (CommonBehaviors.IsLoading)
                    {
                        await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
                    }
                }
                else
                {
                    Log.Error("Please complete the quest 'I Dream of Shirogane'");
                }
            }

            _isDone = true;
        }
    }
}