using System;
using System.Linq;
using System.Reflection;
using System.Text;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteWindows
{
    //TODO Move element numbers to dictionary
    public class GuildLeve : RemoteWindow<GuildLeve>
    {
        public GuildLeve() : base("GuildLeve")
        {
        }

        private static class Offsets
        {
            [Offset("Search 88 05 ? ? ? ? 0F B7 41 06 Add 2 TraceRelative")]
            [OffsetDawntrail("Search 88 05 ? ? ? ? E8 ? ? ? ? 48 8B C8 48 83 C4 ? Add 2 TraceRelative")]
            public static IntPtr AllowancesPtr;
        }

        public LeveWindow Window => (LeveWindow) Elements[6].TrimmedData;

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

        public static int Allowances => Core.Memory.NoCacheRead<int>(Offsets.AllowancesPtr);

        public void AcceptLeve(uint guildLeveId)
        {
            SendAction(2, 3, 3, 4, guildLeveId);
        }

        public static bool HasLeve(uint leveId)
        {
            var activeLeves = GuildLeve.ActiveLeves;

            return activeLeves.Any(leve => leve.GlobalId == leveId);
        }

        public static bool HasLeves(uint[]? leveIds)
        {
            if (leveIds == null)
            {
                return false;
            }

            var activeLeves = GuildLeve.ActiveLeves;

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

            names[0] = Core.Memory.ReadString((IntPtr) Elements[(index * 8) + 628].Data, Encoding.UTF8);
            names[1] = Core.Memory.ReadString((IntPtr) Elements[((index * 8) + 628) + 2].Data, Encoding.UTF8);
            names[2] = Core.Memory.ReadString((IntPtr) Elements[((index * 8) + 628) + 4].Data, Encoding.UTF8);

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