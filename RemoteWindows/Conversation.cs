using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.RemoteWindows;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    /// Static helper for interacting with various NPC dialogue selection windows.
    /// Unified interface for <see cref="SelectString"/>, <see cref="SelectIconString"/>, and <see cref="CutSceneSelectString"/>.
    /// </summary>
    public static class Conversation
    {
        /// <summary>
        /// Gets a value indicating whether any of the supported dialogue selection windows are currently open.
        /// </summary>
        public static bool IsOpen => SelectString.IsOpen || SelectIconString.IsOpen || CutSceneSelectString.IsOpen;

        /// <summary>
        /// List of control character bytes (0x01, 0x02, 0x03, 0x16) often found in game strings that should be stripped for clean display/matching.
        /// </summary>
        private static readonly byte[] badbytes = { 02, 0x16, 01, 03 };

        /// <summary>
        /// Gets the list of dialogue options from the currently open selection window.
        /// For German and French clients, automatically cleans the strings by removing control characters.
        /// </summary>
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

        /// <summary>
        /// Extension method to remove non-printable control characters (e.g., 0x01, 0x02) from a string.
        /// </summary>
        /// <param name="line">The string to clean.</param>
        /// <returns>The cleaned string with control characters removed.</returns>
        public static string StripHypen(this string line)
        {
            var bytes = Encoding.UTF8.GetBytes(line).Where(i=> !badbytes.Contains(i)).ToArray();
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Selects the dialogue option at the specified index in the currently open selection window.
        /// </summary>
        /// <param name="line">The zero-based index of the line to select.</param>
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

        /// <summary>
        /// Searches the current dialogue options for a line containing the specified text and selects it if found.
        /// </summary>
        /// <param name="line">The text to search for.</param>
        /// <returns><see langword="true"/> if a matching line was found and selected; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Attempts to select the 'Quit' or 'Exit' option, which is typically the last item in the list.
        /// </summary>
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