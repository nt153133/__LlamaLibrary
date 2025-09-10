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

        public static void PrintDailies()
        {
            var dailies = GetCurrentDailies();
            var accepted = dailies.Count(i => i.Accepted);
            var finished = dailies.Count(i => i.Accepted && i.IsComplete);
            var unfinished = dailies.Where(i => i.Accepted && !i.IsComplete).Select(i => i.ID);

            Log.Information($"Daily quests left: {BeastTribeHelperOffsets.DailyQuestCount - accepted}\n\tAccepted: {accepted}\n\tFinished: {finished}\n\tCurrentDailies: {string.Join(",", unfinished)}");
        }

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

        public static DailyQuestRead[] GetCurrentDailies()
        {
            //Log.Verbose($"{(BeastTribeHelperOffsets.QuestPointer + BeastTribeHelperOffsets.DailyQuestOffset).ToString("X")}");
            return Core.Memory.ReadArray<DailyQuestRead>(BeastTribeHelperOffsets.QuestPointer + BeastTribeHelperOffsets.DailyQuestOffset, BeastTribeHelperOffsets.DailyQuestCount);
        }

        public static BeastTribeStat[] GetBeastTribes()
        {
            //6.5
           // return Core.Memory.ReadArray<BeastTribeStat>(BeastTribeHelperOffsets.QuestPointer + 0xBC8, BeastTribeHelperOffsets.BeastTribeCount);
           return Core.Memory.ReadArray<BeastTribeStat>(BeastTribeHelperOffsets.QuestPointer + 0xCA8, BeastTribeHelperOffsets.BeastTribeCount);
        }

        public static int GetBeastTribeRank(int tribe)
        {
            var tribes = GetBeastTribes();
            return tribes[tribe - 1].Rank;
        }
    }
}