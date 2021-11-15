using System;
using System.IO;
using Clio.XmlEngine;
using ff14bot;
using ff14bot.NeoProfiles;
using TreeSharp;
using Action = TreeSharp.Action;

namespace LlamaBotBases.OrderbotTags
{
    [XmlElement("ALoadProfile")]
    [XmlElement("LLoadProfile")]

    public class LLoadProfileTag : LLProfileBehavior
    {
        private bool _done;

        [XmlAttribute("Path")]
        public string ProfileName { get; set; }

        public override bool IsDone => _done;

        public LLoadProfileTag() : base() { }

        protected override Composite CreateBehavior()
        {
            return new PrioritySelector(
                new Decorator(
                    ret => TreeRoot.IsRunning,
                    new Action(r =>
                    {
                        NeoProfileManager.Load(newProfilePath, true);
                        NeoProfileManager.UpdateCurrentProfileBehavior();
                        _done = true;
                    })
                )
            );
        }

        private string newProfilePath;
        protected override void OnStart()
        {
            var CurrentProfile = NeoProfileManager.CurrentProfile.Path;

            // Support for store profiles.
            // Absolute path to a store profile.
            if (IsStoreProfile(ProfileName))
            {
                newProfilePath = Slashify(ProfileName);
                return;
            }

            // Relative path to a store profile
            if (IsStoreProfile(CurrentProfile))
            {
                newProfilePath = Slashify(CurrentProfile + "/../" + ProfileName);
                return;
            }

            // Convert path name to absolute, and canonicalize it...
            var absolutePath = Path.Combine(Path.GetDirectoryName(CurrentProfile), ProfileName);
            absolutePath = Path.GetFullPath(absolutePath);
            var canonicalPath = new Uri(absolutePath).LocalPath;
            newProfilePath = Slashify(canonicalPath);

            Log.Information($"Changing profile to {ProfileName}");
        }

        /// <summary>
        /// This gets called when a while loop starts over so reset anything that is used inside the IsDone check.
        /// </summary>
        protected override void OnResetCachedDone()
        {
            _done = false;
        }

        protected override void OnDone()
        {
        }

        private bool IsStoreProfile(string path)
        {
            return path.StartsWith("store://");
        }

        // Converts all slashes to back-slashes if path is local; otherwise converts all back-slashes to slashes
        private string Slashify(string path)
        {
            return IsStoreProfile(path) ? path.Replace(@"\", "/") : path.Replace("/", @"\");
        }
    }
}