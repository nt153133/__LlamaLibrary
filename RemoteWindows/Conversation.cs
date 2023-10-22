using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.RemoteWindows;
using LlamaLibrary.Extensions;

namespace LlamaLibrary.RemoteWindows
{
    public static class Conversation
    {
        public static bool IsOpen => SelectString.IsOpen || SelectIconString.IsOpen || CutSceneSelectString.IsOpen;
        private static readonly byte[] badbytes = new byte[] { 02, 0x16, 01, 03 };
        public static List<string> GetConversationList
        {
            get
            {
                var list = new List<string>();
                if (SelectString.IsOpen)
                {
                    list = SelectString.Lines();
                }

                if (SelectIconString.IsOpen)
                {
                    list = SelectIconString.Lines();
                }

                if (DataManager.CurrentLanguage == Language.Ger || DataManager.CurrentLanguage == Language.Fre)
                {
                    list = list.Select(x => x.StripHypen()).ToList();
                }

                return list;
            }
        }

        public static string StripHypen(this string line)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(line).Where(i=> !badbytes.Contains(i)).ToArray();
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        public static void SelectLine(uint line)
        {
            if (SelectString.IsOpen)
            {
                SelectString.ClickSlot(line);
            }
            else if (SelectIconString.IsOpen)
            {
                SelectIconString.ClickSlot(line);
            }
            else if (CutSceneSelectString.IsOpen)
            {
                CutSceneSelectString.ClickSlot(line);
            }
        }

        public static bool SelectLineContains(string line)
        {
            var index = GetConversationList.FindIndex(x => x.Contains(line, StringComparison.InvariantCultureIgnoreCase));
            if (index == -1)
            {
                return false;
            }

            SelectLine((uint)index);
            return true;
        }

        public static void SelectQuit()
        {
            uint line;
            if (SelectString.IsOpen)
            {
                line = (uint)(SelectString.LineCount - 1);
                SelectString.ClickSlot(line);
            }
            else if (SelectIconString.IsOpen)
            {
                line = (uint)(SelectIconString.LineCount - 1);
                SelectIconString.ClickSlot(line);
            }
        }
    }
}