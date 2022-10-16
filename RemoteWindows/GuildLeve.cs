using System;
using System.Text;
using ff14bot;

namespace LlamaLibrary.RemoteWindows
{
    //TODO Move element numbers to dictionary
    public class GuildLeve : RemoteWindow<GuildLeve>
    {
        public GuildLeve() : base("GuildLeve")
        {
        }

        public LeveWindow Window => (LeveWindow)Elements[6].TrimmedData;

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

            names[0] = Core.Memory.ReadString((IntPtr)Elements[(index * 8) + 628].Data, Encoding.UTF8);
            names[1] = Core.Memory.ReadString((IntPtr)Elements[((index * 8) + 628) + 2].Data, Encoding.UTF8);
            names[2] = Core.Memory.ReadString((IntPtr)Elements[((index * 8) + 628) + 4].Data, Encoding.UTF8);

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