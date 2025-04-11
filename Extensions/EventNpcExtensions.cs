using System;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Objects;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.Extensions
{
    public static class EventNpcExtensions
    {
        public static class Offsets
        {
            [Offset("Search 44 89 BF ? ? ? ? 83 BF ? ? ? ? ? Add 3 Read32")]
            [OffsetDawntrail("Search 44 0F 47 F3 44 89 B7 ? ? ? ? Add 7 Read32")]
            internal static int IconID;
        }

        public static uint IconId(this GameObject eventNpc)
        {
            return eventNpc.Type == GameObjectType.EventNpc ? Core.Memory.Read<uint>(eventNpc.Pointer + Offsets.IconID) : 0U;
        }

        public static int OpenTradeWindow(this BattleCharacter otherPlayer)
        {
            return otherPlayer.Type == GameObjectType.Pc ? Core.Memory.CallInjectedWraper<IntPtr>(Memory.Offsets.OpenTradeWindow, Memory.Offsets.g_InventoryManager, otherPlayer.ObjectId).ToInt32() : -1;
        }
    }
}