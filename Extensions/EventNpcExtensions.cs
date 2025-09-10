using System;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Objects;
using LlamaLibrary.Memory;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.Extensions
{
    public static class EventNpcExtensions
    {


        public static uint IconId(this GameObject eventNpc)
        {
            return eventNpc.Type == GameObjectType.EventNpc ? Core.Memory.Read<uint>(eventNpc.Pointer + EventNpcExtensionsOffsets.IconID) : 0U;
        }

        public static int OpenTradeWindow(this BattleCharacter otherPlayer)
        {
            return otherPlayer.Type == GameObjectType.Pc ? Core.Memory.CallInjectedWraper<IntPtr>(Offsets.OpenTradeWindow, Offsets.g_InventoryManager, otherPlayer.ObjectId).ToInt32() : -1;
        }
    }
}