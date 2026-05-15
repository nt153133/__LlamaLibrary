using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using LlamaLibrary.Enums;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Static helper that exposes world (server) and data-centre information for the local player.
    /// </summary>
    /// <remarks>
    /// World and data-centre values are read directly from game memory or resolved through
    /// <see cref="WorldMap"/>. Prefer the <see cref="World"/> and <see cref="WorldDCGroupType"/>
    /// enums over the deprecated string dictionaries.
    /// </remarks>
    public static class WorldHelper
    {
        private static readonly IntPtr DcOffsetLocation;

        /// <summary>
        /// Initialises the static helper by resolving the data-centre offset location from game memory.
        /// </summary>
        static WorldHelper()
        {
            var agentPointer = AgentModule.AgentPointers[0];
            var offset1 = agentPointer + WorldHelperOffsets.Offset1;
            DcOffsetLocation = offset1 + WorldHelperOffsets.DCOffset;
        }

        /// <summary>
        /// Gets the UTF-8 display name of the player's current sub-zone (place-name).
        /// </summary>
        /// <value>The localised sub-zone name string, resolved via an injected game call against the current sub-zone ID.</value>
        public static string CurrentPlaceName
        {
            get
            {
                var ptr = Core.Memory.CallInjectedWraper<IntPtr>(WorldHelperOffsets.GetPlaceName, WorldManager.SubZoneId);
                return Core.Memory.ReadStringUTF8(ptr + 24);
            }
        }

        /// <summary>
        /// Maps each <see cref="WorldDCGroupType"/> data centre to the array of <see cref="World"/>
        /// servers it contains, covering NA, EU, JP, OCE, CN, and KR regions.
        /// </summary>
        public static readonly Dictionary<WorldDCGroupType, World[]> WorldMap = new()
        {
            { WorldDCGroupType.Materia, new[] { World.Ravana, World.Bismarck, World.Sephirot, World.Sophia, World.Zurvan } },
            { WorldDCGroupType.Mana, new[] { World.Asura, World.Pandaemonium, World.Anima, World.Hades, World.Ixion, World.Titan, World.Chocobo, World.Masamune } },
            { WorldDCGroupType.Meteor, new[] { World.Belias, World.Shinryu, World.Unicorn, World.Yojimbo, World.Zeromus, World.Valefor, World.Ramuh, World.Mandragora } },
            { WorldDCGroupType.Light, new[] { World.Twintania, World.Lich, World.Zodiark, World.Phoenix, World.Odin, World.Shiva, World.Alpha, World.Raiden } },
            { WorldDCGroupType.Crystal, new[] { World.Brynhildr, World.Mateus, World.Zalera, World.Diabolos, World.Coeurl, World.Malboro, World.Goblin, World.Balmung } },
            { WorldDCGroupType.Primal, new[] { World.Famfrit, World.Exodus, World.Lamia, World.Leviathan, World.Ultros, World.Behemoth, World.Excalibur, World.Hyperion } },
            { WorldDCGroupType.Chaos, new[] { World.Omega, World.Moogle, World.Cerberus, World.Louisoix, World.Spriggan, World.Ragnarok, World.Sagittarius, World.Phantom } },
            { WorldDCGroupType.Aether, new[] { World.Jenova, World.Faerie, World.Siren, World.Gilgamesh, World.Midgardsormr, World.Adamantoise, World.Cactuar, World.Sargatanas } },
            { WorldDCGroupType.Gaia, new[] { World.Alexander, World.Fenrir, World.Ultima, World.Ifrit, World.Bahamut, World.Tiamat, World.Durandal, World.Ridill } },
            { WorldDCGroupType.Elemental, new[] { World.Carbuncle, World.Kujata, World.Typhon, World.Garuda, World.Atomos, World.Tonberry, World.Aegis, World.Gungnir } },
            { WorldDCGroupType.Dynamis, new[] { World.Marilith, World.Seraph, World.Halicarnassus, World.Maduin, World.Cuchulainn, World.Golem, World.Kraken, World.Rafflesia } },

            //Chinese
            { WorldDCGroupType.Chocobo, new[] { World.LaNuoXiYa, World.HuanYingQunDao, World.MengYaChi, World.ShenYiZhiDi, World.HongYuHai, World.YuZhouHeYin, World.WoXianXiRan, World.ChenXiWangZuo } },
            { WorldDCGroupType.FatCat, new[] { World.ZiShuiZhanQiao, World.MoDuNa, World.JingYuZhuangYuan, World.YanXia, World.HaiMaoChaWu, World.RouFengHaiWan, World.HuPoYuan } },
            { WorldDCGroupType.Mameshiba, new[] { World.TaiYangHaiAn2, World.YinLeiHu2, World.YiXiuJiaDe2, World.ShuiJingTa2, World.HongChaChuan2 } },
            { WorldDCGroupType.Moogle, new[] { World.BaiJinHuanXiang, World.LvRenZhanQiao, World.FuXiaoZhiJian, World.Longchaoshendian, World.ChaoFengTing, World.ShenQuanHen, World.BaiYinXiang, World.MengYuBaoJing } },

            //Tc worlds
            { WorldDCGroupType.TCRegion1, new[] { World.TcBahamut, World.TcGaruda, World.TcPhoenix, World.TcIfrit, World.TcTitan, World.TcRamuh, World.TcLeviathan, World.TcOdin } },
        };

        /// <summary>
        /// Gets all worlds on the same data centre as the player's current world.
        /// </summary>
        /// <value>
        /// An array of <see cref="World"/> values for every server on the current DC,
        /// or an empty array when the current world is not found in <see cref="WorldMap"/>.
        /// </value>
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

        /// <summary>
        /// Gets a value indicating whether the player is currently on their home world.
        /// </summary>
        /// <value>
        /// <see langword="true"/> when <see cref="CurrentWorldId"/> equals <see cref="HomeWorldId"/>.
        /// </value>
        public static bool IsOnHomeWorld => CurrentWorldId == HomeWorldId;

        /// <summary>
        /// Gets the raw numeric identifier of the player's current data centre.
        /// </summary>
        /// <remarks>
        /// On Traditional Chinese builds the value is always <c>TCRegion1</c>.
        /// On CN builds the value is derived by searching <see cref="WorldMap"/>.
        /// On all other builds it is read directly from game memory.
        /// </remarks>
        public static byte DataCenterId
        {
            get
            {

                #if RB_TC
                    return (byte)WorldDCGroupType.TCRegion1;
                #endif

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

        /// <summary>
        /// Determines whether the specified world belongs to the same data centre as the current player.
        /// </summary>
        /// <param name="world">The <see cref="World"/> server to check.</param>
        /// <returns>
        /// <see langword="true"/> when <paramref name="world"/> is listed under the current DC in
        /// <see cref="WorldMap"/>, or always <see langword="true"/> on Traditional Chinese builds.
        /// </returns>
        /// <example>
        /// <code>
        /// if (WorldHelper.CheckDC(World.Balmung))
        /// {
        ///     // Balmung is on the same DC — world travel is possible
        /// }
        /// </code>
        /// </example>
        public static bool CheckDC(World world)
        {
            if (OffsetManager.ActiveRegion == ClientRegion.TraditionalChinese)
            {
                return true;
            }

            return WorldMap.ContainsKey(DataCenter) && WorldMap[DataCenter].Contains(world);
        }

        /// <summary>
        /// Gets the player's current data centre as a <see cref="WorldDCGroupType"/> enum value.
        /// </summary>
        public static WorldDCGroupType DataCenter => (WorldDCGroupType)DataCenterId;

        /// <summary>
        /// Maps legacy numeric data-centre identifiers to their string names.
        /// </summary>
        /// <remarks>
        /// This member is preserved for backward compatibility only.
        /// Use <see cref="DataCenter"/> and the <see cref="WorldDCGroupType"/> enum instead.
        /// </remarks>
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

        /// <summary>
        /// Gets the raw numeric identifier of the world the player is currently visiting.
        /// </summary>
        public static ushort CurrentWorldId => Core.Memory.NoCacheRead<ushort>(Core.Me.Pointer + WorldHelperOffsets.CurrentWorld);

        /// <summary>
        /// Gets the raw numeric identifier of the player's home world
        /// (the server on which the character was created).
        /// </summary>
        public static ushort HomeWorldId => Core.Memory.NoCacheRead<ushort>(Core.Me.Pointer + WorldHelperOffsets.HomeWorld);

        /// <summary>
        /// Gets the world the player is currently visiting as a <see cref="World"/> enum value.
        /// </summary>
        public static World CurrentWorld => (World)CurrentWorldId;

        /// <summary>
        /// Gets the player's home world as a <see cref="World"/> enum value.
        /// </summary>
        public static World HomeWorld => (World)HomeWorldId;

        /// <summary>
        /// Maps legacy numeric world identifiers to their string names.
        /// </summary>
        /// <remarks>
        /// This member is preserved for backward compatibility only.
        /// Use <see cref="CurrentWorld"/> / <see cref="HomeWorld"/> and the <see cref="World"/> enum instead.
        /// </remarks>
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

        /// <summary>
        /// Gets the localised display name of the player's current data centre.
        /// </summary>
        public static string DataCenterName => DataCenter.DCName();

        /// <summary>
        /// Gets the localised display name of the player's home world.
        /// </summary>
        public static string HomeWorldName => HomeWorld.WorldName();
    }
}