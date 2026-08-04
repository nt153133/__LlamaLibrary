using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteWindows
{
    public class GuildLeve : RemoteWindow<GuildLeve>
    {
        public static readonly Dictionary<string, int> Properties = new(StringComparer.Ordinal)
        {
            { "LeveWindowType", 6 },
            { "LeveNamesBase", 628 },
        };

        public GuildLeve() : base("GuildLeve")
        {
        }

        public LeveWindow Window => (LeveWindow) Elements[Properties["LeveWindowType"]].Int;

        /*
        private static readonly Type LeveManagerType =
            Assembly.GetEntryAssembly()
                .GetTypes()
                .FirstOrDefault(t =>
                                    t.GetProperties(BindingFlags.Static | BindingFlags.Public).Count(f => f.PropertyType == typeof(LeveWork[])) == 1);

        private static readonly PropertyInfo LevesPropertyInfo =
            LeveManagerType.GetProperties(BindingFlags.Static | BindingFlags.Public)
                .FirstOrDefault(f => f.PropertyType == typeof(LeveWork[]));
                */

        public static LeveWork[] ActiveLeves => LeveManager.Leves; // LevesPropertyInfo.GetValue(null) as LeveWork[] ?? Array.Empty<LeveWork>();

        public static int Allowances => Core.Memory.NoCacheRead<int>(GuildLeveOffsets.AllowancesPtr);

        public void AcceptLeve(uint guildLeveId)
        {
            SendAction(2, 3, 3, 4, guildLeveId);
        }

        public static bool HasLeve(uint leveId)
        {
            var activeLeves = ActiveLeves;

            return activeLeves.Any(leve => leve.GlobalId == leveId);
        }

        public static bool HasLeves(uint[]? leveIds)
        {
            if (leveIds == null)
            {
                return false;
            }

            var activeLeves = ActiveLeves;

            return leveIds.All(leveId => activeLeves.Any(leve => leve.GlobalId == leveId));
        }

        public string PrintWindow()
        {
            var sb = new StringBuilder();

            sb.AppendLine(Window.ToString());

            for (var i = 0; i < 5; i++)
            {
                var leveBlock = GetLeveGroup(i);

                //sb.AppendLine("Block " + i);

                foreach (var leve in leveBlock)
                {
                    if (!leve.Contains("Level "))
                    {
                        sb.AppendLine(leve);
                    }
                }
            }

            return sb.ToString();
        }

        public string[] GetLeveGroup(int index)
        {
            var names = new string[3];
            var baseOffset = (index * 8) + Properties["LeveNamesBase"];

            names[0] = Core.Memory.ReadString((IntPtr) Elements[baseOffset].Data, Encoding.UTF8);
            names[1] = Core.Memory.ReadString((IntPtr) Elements[baseOffset + 2].Data, Encoding.UTF8);
            names[2] = Core.Memory.ReadString((IntPtr) Elements[baseOffset + 4].Data, Encoding.UTF8);

            return names;
        }

        public void SwitchType(int index)
        {
            SendAction(3, 3, 9, 3, (ulong)index, 3, 0);
        }

        public void SwitchClass(int index)
        {
            SendAction(2, 3, 0xB, 3, (ulong)index);
        }
    }

    public enum LeveWindow
    {
        Battle = 0,
        Gathering = 3,
        Crafting = 8
    }
}