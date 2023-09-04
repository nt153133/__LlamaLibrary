using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot.RemoteWindows;
using LlamaLibrary.Extensions;

namespace LlamaLibrary.RemoteWindows
{
    public static class Conversation
    {
        public static bool IsOpen => SelectString.IsOpen || SelectIconString.IsOpen || CutSceneSelectString.IsOpen;

        public static List<string> GetConversationList
        {
            get
            {
                if (SelectString.IsOpen)
                {
                    return SelectString.Lines();
                }

                if (SelectIconString.IsOpen)
                {
                    return SelectIconString.Lines();
                }

                return new List<string>();
            }
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