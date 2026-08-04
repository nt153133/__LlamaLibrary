using System;
using System.Collections.Generic;
using System.Text;
using ff14bot;
using ff14bot.Enums;
using LlamaLibrary.Helpers;

namespace LlamaLibrary.RemoteWindows
{
    public class HWDSupply : RemoteWindow<HWDSupply>
    {
        public static readonly Dictionary<string, int> Properties = new(System.StringComparer.Ordinal)
        {
#if RB_CN
            { "ClassSelected", 29 },
            { "KupoVoucherData", 0 },
#else
            { "ClassSelected", 62 },
            { "KupoVoucherData", 3 },
#endif
            { "ScoreBase", 17 },
        };

        public HWDSupply() : base("HWDSupply")
        {
        }

        public int CurrentClassSelected()
        {
            return Elements[Properties["ClassSelected"]].Int;
        }

        public int GetAccumulatedScore()
        {
            return Elements[Properties["ScoreBase"] + CurrentClassSelected()].Int;
        }

        public int GetKupoVoucherCount()
        {
            var index = Properties["KupoVoucherData"];
            if (index == 0)
            {
                return 0;
            }

            var data = Core.Memory.ReadString((IntPtr)Elements[index].Data, Encoding.UTF8).Split('/');
            return data.Length < 2 ? 0 : int.Parse(data[0].Trim());
        }

        public int ClassSelected
        {
            get => CurrentClassSelected();
            set
            {
                if (WindowByName != null && CurrentClassSelected() != value)
                {
                    SendAction(2, 0, 1, 1, (ulong)value);
                }
            }
        }

        public void ClickItem(int index)
        {
            SendAction(2, 3, 1, 3, (ulong)index);
        }

        public override void Close()
        {
            SendAction(1, 3, ulong.MaxValue);
        }
    }
}