using System.ComponentModel;
using System.IO;
using ff14bot.Helpers;
using LlamaLibrary.Helpers;

namespace LlamaBotBases.LlamaUtilities.Settings
{
    public class HuntsSettings : JsonSettings
    {
        public static HuntsSettings Instance => _settings ?? (_settings = new HuntsSettings());

        private static HuntsSettings _settings;

        private bool _ARRHunts;

        private bool _VerteranClanHunts;

        private bool _NutClanHunts;

        public HuntsSettings() : base(Path.Combine(JsonHelper.UniqueCharacterSettingsDirectory, "RetainerSettings.json"))
        {

        }

        [Description("Complete ARR Daily Hunts")]
        [DefaultValue(true)]
        public bool ARRHunts
        {
            get => _ARRHunts;
            set
            {
                if (_ARRHunts != value)
                {
                    _ARRHunts = value;
                    Save();
                }
            }
        }

        [Description("Complete Veteran Clan Mark Daily Hunts")]
        [DefaultValue(true)]
        public bool VerteranClanHunts
        {
            get => _VerteranClanHunts;
            set
            {
                if (_VerteranClanHunts != value)
                {
                    _VerteranClanHunts = value;
                    Save();
                }
            }
        }

        [Description("Complete Nut Clan Mark Daily Hunts")]
        [DefaultValue(true)]
        public bool NutClanHunts
        {
            get => _NutClanHunts;
            set
            {
                if (_NutClanHunts != value)
                {
                    _NutClanHunts = value;
                    Save();
                }
            }
        }
    }
}