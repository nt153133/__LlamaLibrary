using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using LlamaLibrary.Enums;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.Helpers
{
    public static class WorldHelper
    {
        private static class Offsets
        {
            [Offset("Search 48 8D 4B ? 4C 8B 47 ? E8 ? ? ? ? Add 3 Read8")]
            //[OffsetCN("Search 48 8D 4B ? E8 ? ? ? ? 84 C0 74 ? 48 8B 74 24 ? Add 3 Read8")]
            internal static int Offset1;

            [Offset("Search 41 89 9F ? ? ? ? 48 83 7D ? ? Add 3 Read32")]
            internal static int DCOffset;

            [Offset("Search 0F B7 98 ? ? ? ? 66 85 FF Add 3 Read32")]
            internal static int CurrentWorld;

            [Offset("Search 0F B7 81 ? ? ? ? 66 89 44 24 ? 48 8D 4C 24 ? Add 3 Read32")]
            internal static int HomeWorld;

            [Offset("Search E8 ? ? ? ? 48 85 C0 74 ? 0F B7 70 14 EB ? BE ? ? ? ? 0F B7 CD E8 ? ? ? ? 48 85 C0 74 ? 0F B7 40 14 EB ? B8 ? ? ? ? 66 3B F0 72 ? 0F B7 0B Add 1 TraceRelative")]
            internal static IntPtr GetPlaceName;
        }

        private static readonly IntPtr DcOffsetLocation;

        static WorldHelper()
        {
            var agentPointer = AgentModule.AgentPointers[0];
            var offset1 = agentPointer + Offsets.Offset1;
            DcOffsetLocation = offset1 + Offsets.DCOffset;
        }

        public static string CurrentPlaceName
        {
            get
            {
                var ptr = Core.Memory.CallInjected64<IntPtr>(Offsets.GetPlaceName, WorldManager.SubZoneId);
                return Core.Memory.ReadStringUTF8(ptr + 24);
            }
        }

        public static readonly Dictionary<WorldDCGroupType, World[]> WorldMap = new()
        {
            { WorldDCGroupType.Materia, new World[] { World.Ravana, World.Bismarck, World.Sephirot, World.Sophia, World.Zurvan } },
            { WorldDCGroupType.Mana, new World[] { World.Asura, World.Pandaemonium, World.Anima, World.Hades, World.Ixion, World.Titan, World.Chocobo, World.Masamune } },
            { WorldDCGroupType.Meteor, new World[] { World.Belias, World.Shinryu, World.Unicorn, World.Yojimbo, World.Zeromus, World.Valefor, World.Ramuh, World.Mandragora } },
            { WorldDCGroupType.Light, new World[] { World.Twintania, World.Lich, World.Zodiark, World.Phoenix, World.Odin, World.Shiva, World.Alpha, World.Raiden } },
            { WorldDCGroupType.Crystal, new World[] { World.Brynhildr, World.Mateus, World.Zalera, World.Diabolos, World.Coeurl, World.Malboro, World.Goblin, World.Balmung } },
            { WorldDCGroupType.Primal, new World[] { World.Famfrit, World.Exodus, World.Lamia, World.Leviathan, World.Ultros, World.Behemoth, World.Excalibur, World.Hyperion } },
            { WorldDCGroupType.Chaos, new World[] { World.Omega, World.Moogle, World.Cerberus, World.Louisoix, World.Spriggan, World.Ragnarok, World.Sagittarius, World.Phantom } },
            { WorldDCGroupType.Aether, new World[] { World.Jenova, World.Faerie, World.Siren, World.Gilgamesh, World.Midgardsormr, World.Adamantoise, World.Cactuar, World.Sargatanas } },
            { WorldDCGroupType.Gaia, new World[] { World.Alexander, World.Fenrir, World.Ultima, World.Ifrit, World.Bahamut, World.Tiamat, World.Durandal, World.Ridill } },
            { WorldDCGroupType.Elemental, new World[] { World.Carbuncle, World.Kujata, World.Typhon, World.Garuda, World.Atomos, World.Tonberry, World.Aegis, World.Gungnir } },
            { WorldDCGroupType.Dynamis, new World[] { World.Marilith, World.Seraph, World.Halicarnassus, World.Maduin } },

            //Chinese
            { WorldDCGroupType.Chocobo, new World[] { World.LaNuoXiYa, World.HuanYingQunDao, World.MengYaChi, World.ShenYiZhiDi, World.HongYuHai, World.YuZhouHeYin, World.WoXianXiRan, World.ChenXiWangZuo } },
            { WorldDCGroupType.FatCat, new World[] { World.ZiShuiZhanQiao, World.MoDuNa, World.JingYuZhuangYuan, World.YanXia, World.HaiMaoChaWu, World.RouFengHaiWan, World.HuPoYuan } },
            { WorldDCGroupType.Mameshiba, new World[] { World.TaiyangHaiAn, World.YinLeiHu, World.HongChaChuan, World.YiXiuJiaDe, World.ShuiJingTa, World.TaiYangHaiAn2, World.YinLeiHu2, World.YiXiuJiaDe2, World.ShuiJingTa2, World.HongChaChuan2 } },
            { WorldDCGroupType.Moogle, new World[] { World.BaiJinHuanXiang, World.LvRenZhanQiao, World.FuXiaoZhiJian, World.Longchaoshendian, World.ChaoFengTing, World.ShenQuanHen, World.BaiYinXiang, World.MengYuBaoJing } },
        };

        public static World[] CurrentWorldList
        {
            get
            {
                if (!CheckDC(CurrentWorld))
                {
                    return Array.Empty<World>();
                }

                return WorldMap[DataCenter];
            }
        }

        public static bool IsOnHomeWorld => CurrentWorldId == HomeWorldId;

        public static byte DataCenterId
        {
            get
            {
                #if RB_CN
                var dc = WorldMap.Where(x => x.Value.Any(y => y == CurrentWorld));
                if (!dc.Any())
                {
                    return 0;
                }

                return (byte)dc.First().Key;
                #endif
                return Core.Memory.Read<byte>(DcOffsetLocation);
            }
        }

        public static bool CheckDC(World world)
        {
            if (Translator.Language == Language.Chn)
            {
                return true;
            }

            return WorldMap.ContainsKey(DataCenter) && WorldMap[DataCenter].Contains(world);
        }

        public static WorldDCGroupType DataCenter => (WorldDCGroupType)DataCenterId;

        [Obsolete("Use Enum instead")]
        public static readonly Dictionary<byte, string> DataCenterNamesDictionary = new()
        {
            { 0, "INVALID" },
            { 1, "Elemental" },
            { 2, "Gaia" },
            { 3, "Mana" },
            { 4, "Aether" },
            { 5, "Primal" },
            { 6, "Chaos" },
            { 7, "Light" },
            { 8, "Crystal" },
            { 9, "Materia" },
            { 10, "Meteor" },
            { 11, "Dynamis" },
            { 99, "Beta" },
        };

        public static ushort CurrentWorldId => Core.Memory.NoCacheRead<ushort>(Core.Me.Pointer + Offsets.CurrentWorld);

        public static ushort HomeWorldId => Core.Memory.NoCacheRead<ushort>(Core.Me.Pointer + Offsets.HomeWorld);

        public static World CurrentWorld => (World)CurrentWorldId;

        public static World HomeWorld => (World)HomeWorldId;

        [Obsolete("Use Enum instead")]
        public static readonly Dictionary<ushort, string> WorldNamesDictionary = new()
        {
            { 0, "INVALID" },
            { 21, "Ravana" },
            { 22, "Bismarck" },
            { 23, "Asura" },
            { 24, "Belias" },
            { 25, "Chaos" },
            { 26, "Hecatoncheir" },
            { 27, "Moomba" },
            { 28, "Pandaemonium" },
            { 29, "Shinryu" },
            { 30, "Unicorn" },
            { 31, "Yojimbo" },
            { 32, "Zeromus" },
            { 33, "Twintania" },
            { 34, "Brynhildr" },
            { 35, "Famfrit" },
            { 36, "Lich" },
            { 37, "Mateus" },
            { 38, "Shemhazai" },
            { 39, "Omega" },
            { 40, "Jenova" },
            { 41, "Zalera" },
            { 42, "Zodiark" },
            { 43, "Alexander" },
            { 44, "Anima" },
            { 45, "Carbuncle" },
            { 46, "Fenrir" },
            { 47, "Hades" },
            { 48, "Ixion" },
            { 49, "Kujata" },
            { 50, "Typhon" },
            { 51, "Ultima" },
            { 52, "Valefor" },
            { 53, "Exodus" },
            { 54, "Faerie" },
            { 55, "Lamia" },
            { 56, "Phoenix" },
            { 57, "Siren" },
            { 58, "Garuda" },
            { 59, "Ifrit" },
            { 60, "Ramuh" },
            { 61, "Titan" },
            { 62, "Diabolos" },
            { 63, "Gilgamesh" },
            { 64, "Leviathan" },
            { 65, "Midgardsormr" },
            { 66, "Odin" },
            { 67, "Shiva" },
            { 68, "Atomos" },
            { 69, "Bahamut" },
            { 70, "Chocobo" },
            { 71, "Moogle" },
            { 72, "Tonberry" },
            { 73, "Adamantoise" },
            { 74, "Coeurl" },
            { 75, "Malboro" },
            { 76, "Tiamat" },
            { 77, "Ultros" },
            { 78, "Behemoth" },
            { 79, "Cactuar" },
            { 80, "Cerberus" },
            { 81, "Goblin" },
            { 82, "Mandragora" },
            { 83, "Louisoix" },
            { 84, "Syldra" },
            { 85, "Spriggan" },
            { 86, "Sephirot" },
            { 87, "Sophia" },
            { 88, "Zurvan" },
            { 90, "Aegis" },
            { 91, "Balmung" },
            { 92, "Durandal" },
            { 93, "Excalibur" },
            { 94, "Gungnir" },
            { 95, "Hyperion" },
            { 96, "Masamune" },
            { 97, "Ragnarok" },
            { 98, "Ridill" },
            { 99, "Sargatanas" },
            { 400, "Sagittarius" },
            { 401, "Phantom" },
            { 402, "Alpha" },
            { 403, "Raiden" },
            { 404, "Marilith" },
            { 405, "Seraph" },
            { 406, "Halicarnassus" },
            { 407, "Maduin" },
        };

        public static string DataCenterName => DataCenter.DCName();

        public static string HomeWorldName => HomeWorld.WorldName();
    }
}