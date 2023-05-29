using System;
using System.Collections.Generic;
using System.Text;
using ff14bot;

namespace LlamaLibrary.RemoteWindows
{
    public class RelicSphereScroll : RemoteWindow<RelicSphereScroll>
    {
        public RelicSphereScroll() : base("RelicSphereScroll")
        {
        }

        public static readonly Dictionary<string, int> Properties = new()
        {
            {
                "ItemId",
                3
            },
            {
                "CurrentInfuse",
                10
            },
            {
                "MaxInfuse",
                11
            },
            {
                "NameStart",
                31
            },
            {
                "StatStart",
                59
            },
            {
                "CountStart",
                87
            },
            {
                "HighlightStart",
                115
            },
        };

        const int MateriaCount = 28;

        public int MaxInfuse => Elements[Properties["MaxInfuse"]].TrimmedData;

        public int CurrentInfuse => Elements[Properties["CurrentInfuse"]].TrimmedData;

        public int ItemId => Elements[Properties["ItemId"]].TrimmedData;

        public void SelectMateria(int index)
        {
            SendAction(2, 3, 0, 3, (ulong)index);
        }

        public void Infuse()
        {
            SendAction(1, 3, 2);
        }

        public MateriaOption[] GetMateriaOptions()
        {
            var materiaOptions = new MateriaOption[MateriaCount];
            for (var i = 0; i < MateriaCount; i++)
            {
                materiaOptions[i] = new MateriaOption(i,
                                                      Core.Memory.ReadString((IntPtr)Elements[Properties["NameStart"] + i].Data, Encoding.UTF8),
                                                      Core.Memory.ReadString((IntPtr)Elements[Properties["StatStart"] + i].Data, Encoding.UTF8),
                                                      Elements[Properties["CountStart"] + i].TrimmedData,
                                                      Elements[Properties["HighlightStart"] + i].TrimmedData == 1);
            }

            return materiaOptions;
        }
    }

    public record MateriaOption(int Index, string Name, string Stat, int Count, bool Highlighted)
    {
        public string Name { get; } = Name;
        public string Stat { get; } = Stat;
        public int Count { get; } = Count;
        public bool Highlighted { get; } = Highlighted;
        public int Index { get; } = Index;
    }
}