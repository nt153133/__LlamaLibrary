using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using ff14bot;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;

namespace LlamaLibrary.Helpers
{
    public static class BeastTribeHelper
    {
        private static readonly LLogger Log = new(nameof(BeastTribeHelper), Colors.Gold);

        private static class Offsets
        {
            [Offset("Search E8 ? ? ? ? BA ? ? ? ? 48 8B C8 48 83 C4 ? E9 ? ? ? ? ? ? ? ? ? ? E9 ? ? ? ? TraceCall")]
            internal static IntPtr GetQuestPointer;

            [Offset("Search 48 8D 81 ? ? ? ? 66 0F 1F 44 00 ? 66 39 50 ? 74 ? 41 FF C0 Add 3 Read32")]
            internal static int DailyQuestOffset;

            [Offset("Search 41 83 F8 ? 72 ? 32 C0 C3 0F B6 40 ? Add 3 Read8")]
            internal static int DailyQuestCount;

            [Offset("Search E8 ? ? ? ? 48 85 C0 74 ? 3A 58 ? TraceCall")]
            internal static IntPtr GetBeastTribeExd;

            [Offset("Search E8 ? ? ? ? 4C 8B C8 EB ? 4C 8D 0D ? ? ? ? TraceCall")]
            internal static IntPtr ResolveStringColumnIndirection;

            //7.1
            [Offset("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? C6 84 24 ? ? ? ? ?  Add 3 TraceRelative")]
            //[OffsetCN("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? C6 44 24 ? ?  Add 3 TraceRelative")]
            internal static IntPtr QuestPointer;

            //6.4
            [Offset("Search 48 81 C1 ? ? ? ? 48 03 C9 0F B6 1C C8 Add 3 Read32")]
            internal static int BeastTribeStart;

            [Offset("Search 66 89 BC C8 ? ? ? ? Add 4 Read32")]
            internal static int BeastTribeRep;

            //6.4
            [Offset("Search 83 FB ? 73 ? E8 ? ? ? ? 8B CB 48 81 C1 ? ? ? ? 48 03 C9 0F B6 1C C8 Add 2 Read8")]
            internal static int BeastTribeCount;
        }

        private static readonly BeastTribeExd[] _beastTribes;

        static BeastTribeHelper()
        {
            var tribes = new List<BeastTribeExd>();

            for (var i = 1; i <= Offsets.BeastTribeCount; i++)
            {
                var result = Core.Memory.CallInjected64<IntPtr>(Offsets.GetBeastTribeExd, i);
                var name = UppercaseFirst(Core.Memory.ReadStringUTF8(Core.Memory.CallInjected64<IntPtr>(Offsets.ResolveStringColumnIndirection, result)));
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

            Log.Information($"Daily quests left: {Offsets.DailyQuestCount - accepted}\n\tAccepted: {accepted}\n\tFinished: {finished}\n\tCurrentDailies: {string.Join(",", unfinished)}");
        }

        public static int DailyQuestAllowance()
        {
            var dailies = GetCurrentDailies();
            var accepted = dailies.Count(i => i.Accepted);
            return Offsets.DailyQuestCount - accepted;
        }

        /*
        public static string GetBeastTribeName(int index)
        {
            var result = Core.Memory.CallInjected64<IntPtr>(Offsets.GetBeastTribeExd, index);
            return result != IntPtr.Zero ? Core.Memory.ReadString(result + 0x28, Encoding.UTF8) : "";
        }

        public static int GetBeastTribeMaxRank(int index)
        {
            var result = Core.Memory.CallInjected64<IntPtr>(Offsets.GetBeastTribeExd, index);
            return result != IntPtr.Zero ? Core.Memory.Read<byte>(result + 0x22) : 0;
        }
        */

        public static DailyQuestRead[] GetCurrentDailies()
        {
            //Log.Verbose($"{(Offsets.QuestPointer + Offsets.DailyQuestOffset).ToString("X")}");
            return Core.Memory.ReadArray<DailyQuestRead>(Offsets.QuestPointer + Offsets.DailyQuestOffset, Offsets.DailyQuestCount);
        }

        public static BeastTribeStat[] GetBeastTribes()
        {
            //6.5
            return Core.Memory.ReadArray<BeastTribeStat>(Offsets.QuestPointer + 0xBC8, Offsets.BeastTribeCount);
        }

        public static int GetBeastTribeRank(int tribe)
        {
            var tribes = GetBeastTribes();
            return tribes[tribe - 1].Rank;
        }
    }
}