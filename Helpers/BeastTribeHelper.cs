using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using ff14bot;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Provides access to FFXIV Beast Tribe quest data, including daily allowances, tribe ranks, and current quest progress.
    /// Beast Tribes are factions with whom the player can complete daily quests to earn reputation and unlock rewards.
    /// </summary>
    public static class BeastTribeHelper
    {
        private static readonly LLogger Log = new(nameof(BeastTribeHelper), Colors.Gold);

        

        private static readonly BeastTribeExd[] _beastTribes;

        static BeastTribeHelper()
        {
            var tribes = new List<BeastTribeExd>();

            for (var i = 1; i <= BeastTribeHelperOffsets.BeastTribeCount; i++)
            {
                var result = Core.Memory.CallInjectedWraper<IntPtr>(BeastTribeHelperOffsets.GetBeastTribeExd, i);
                var name = UppercaseFirst(Core.Memory.ReadStringUTF8(Core.Memory.CallInjectedWraper<IntPtr>(BeastTribeHelperOffsets.ResolveStringColumnIndirection, result)));
                var tribe = Core.Memory.Read<BeastTribeExdTemp>(result);

                tribes.Add(new BeastTribeExd(tribe, name));
            }

            _beastTribes = tribes.ToArray();
        }

        /// <summary>
        /// Logs the name, unlock status, and maximum rank for every Beast Tribe to the debug console.
        /// </summary>
        public static void PrintBeastTribes()
        {
            var tribes = GetBeastTribes();

            for (var i = 0; i < tribes.Length; i++)
            {
                Log.Information(tribes[i].Unlocked ? $"{_beastTribes[i].Name} - {tribes[i]} MaxRank: {_beastTribes[i].MaxRank}" : $"{_beastTribes[i].Name} - Not Unlocked");
            }
        }

        static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        /// <summary>
        /// Logs a summary of the current daily Beast Tribe quest status including how many allowances remain,
        /// how many quests are accepted, and which are still incomplete.
        /// </summary>
        public static void PrintDailies()
        {
            var dailies = GetCurrentDailies();
            var accepted = dailies.Count(i => i.Accepted);
            var finished = dailies.Count(i => i.Accepted && i.IsComplete);
            var unfinished = dailies.Where(i => i.Accepted && !i.IsComplete).Select(i => i.ID);

            Log.Information($"Daily quests left: {BeastTribeHelperOffsets.DailyQuestCount - accepted}\n\tAccepted: {accepted}\n\tFinished: {finished}\n\tCurrentDailies: {string.Join(",", unfinished)}");
        }

        /// <summary>
        /// Returns the number of Beast Tribe daily quest allowances remaining for the current reset cycle.
        /// </summary>
        /// <returns>Number of daily quests that can still be accepted today.</returns>
        /// <example>
        /// <code>
        /// var allowances = BeastTribeHelper.DailyQuestAllowance();
        /// </code>
        /// </example>
        public static int DailyQuestAllowance()
        {
            var dailies = GetCurrentDailies();
            var accepted = dailies.Count(i => i.Accepted);
            return BeastTribeHelperOffsets.DailyQuestCount - accepted;
        }

        /*
        public static string GetBeastTribeName(int index)
        {
            var result = Core.Memory.CallInjectedWraper<IntPtr>(BeastTribeHelperOffsets.GetBeastTribeExd, index);
            return result != IntPtr.Zero ? Core.Memory.ReadString(result + 0x28, Encoding.UTF8) : "";
        }

        public static int GetBeastTribeMaxRank(int index)
        {
            var result = Core.Memory.CallInjectedWraper<IntPtr>(BeastTribeHelperOffsets.GetBeastTribeExd, index);
            return result != IntPtr.Zero ? Core.Memory.Read<byte>(result + 0x22) : 0;
        }
        */

        /// <summary>
        /// Reads the player's currently accepted Beast Tribe daily quests directly from game memory.
        /// </summary>
        /// <returns>An array of <see cref="DailyQuestRead"/> structs representing the active quest slots.</returns>
        public static DailyQuestRead[] GetCurrentDailies()
        {
            //Log.Verbose($"{(BeastTribeHelperOffsets.QuestPointer + BeastTribeHelperOffsets.DailyQuestOffset).ToString("X")}");
            return Core.Memory.ReadArray<DailyQuestRead>(BeastTribeHelperOffsets.QuestPointer + BeastTribeHelperOffsets.DailyQuestOffset, BeastTribeHelperOffsets.DailyQuestCount);
        }

        /// <summary>
        /// Reads all Beast Tribe stat data (rank, reputation, etc.) for every tribe from game memory.
        /// </summary>
        /// <returns>An array of <see cref="BeastTribeStat"/> structs, one entry per tribe.</returns>
        public static BeastTribeStat[] GetBeastTribes()
        {
            //6.5
           // return Core.Memory.ReadArray<BeastTribeStat>(BeastTribeHelperOffsets.QuestPointer + 0xBC8, BeastTribeHelperOffsets.BeastTribeCount);
           return Core.Memory.ReadArray<BeastTribeStat>(BeastTribeHelperOffsets.QuestPointer + 0xCA8, BeastTribeHelperOffsets.BeastTribeCount);
        }

        /// <summary>
        /// Gets the player's current rank with a specific Beast Tribe.
        /// </summary>
        /// <param name="tribe">The 1-based index of the tribe as used in the game data.</param>
        /// <returns>The player's current rank (reputation level) with the specified tribe.</returns>
        /// <example>
        /// <code>
        /// var rank = BeastTribeHelper.GetBeastTribeRank(tribeId);
        /// </code>
        /// </example>
        public static int GetBeastTribeRank(int tribe)
        {
            var tribes = GetBeastTribes();
            return tribes[tribe - 1].Rank;
        }
    }
}