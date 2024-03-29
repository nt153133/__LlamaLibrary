﻿using ff14bot.Managers;

namespace LlamaLibrary.JsonObjects
{
    public class MateriaItem
    {
        public int Key;
        public int Tier;
        public int Value;
        internal Item Item => DataManager.GetItem((uint)Key);
        public string ItemName => Item.CurrentLocaleName;

        public string Stat;

        public MateriaItem(int key, int tier, int value, string stat)
        {
            Key = key;
            Tier = tier;
            Value = value;
            Stat = stat;
        }

        public override string ToString()
        {
            return $"{DataManager.GetItem((uint)Key).CurrentLocaleName} {Tier} {Value}";
        }
    }
}