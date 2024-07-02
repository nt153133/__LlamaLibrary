using System.Collections.Generic;
using System.ComponentModel;
using LlamaLibrary.Extensions;

// ReSharper disable IdentifierTypo

namespace LlamaLibrary.Enums
{
    public enum World : ushort
    {
        [Description("Invalid")]
        SetMe = 0,
        Ravana = 21, //Materia
        Bismarck = 22, //Materia
        Asura = 23, //Mana
        Belias = 24, //Meteor
        Pandaemonium = 28, //Mana
        Shinryu = 29, //Meteor
        Unicorn = 30, //Meteor
        Yojimbo = 31, //Meteor
        Zeromus = 32, //Meteor
        Twintania = 33, //Light
        Brynhildr = 34, //Crystal
        Famfrit = 35, //Primal
        Lich = 36, //Light
        Mateus = 37, //Crystal
        Shemhazai = 38, //INVALID
        Omega = 39, //Chaos
        Jenova = 40, //Aether
        Zalera = 41, //Crystal
        Zodiark = 42, //Light
        Alexander = 43, //Gaia
        Anima = 44, //Mana
        Carbuncle = 45, //Elemental
        Fenrir = 46, //Gaia
        Hades = 47, //Mana
        Ixion = 48, //Mana
        Kujata = 49, //Elemental
        Typhon = 50, //Elemental
        Ultima = 51, //Gaia
        Valefor = 52, //Meteor
        Exodus = 53, //Primal
        Faerie = 54, //Aether
        Lamia = 55, //Primal
        Phoenix = 56, //Light
        Siren = 57, //Aether
        Garuda = 58, //Elemental
        Ifrit = 59, //Gaia
        Ramuh = 60, //Meteor
        Titan = 61, //Mana
        Diabolos = 62, //Crystal
        Gilgamesh = 63, //Aether
        Leviathan = 64, //Primal
        Midgardsormr = 65, //Aether
        Odin = 66, //Light
        Shiva = 67, //Light
        Atomos = 68, //Elemental
        Bahamut = 69, //Gaia
        Chocobo = 70, //Mana
        Moogle = 71, //Chaos
        Tonberry = 72, //Elemental
        Adamantoise = 73, //Aether
        Coeurl = 74, //Crystal
        Malboro = 75, //Crystal
        Tiamat = 76, //Gaia
        Ultros = 77, //Primal
        Behemoth = 78, //Primal
        Cactuar = 79, //Aether
        Cerberus = 80, //Chaos
        Goblin = 81, //Crystal
        Mandragora = 82, //Meteor
        Louisoix = 83, //Chaos
        Spriggan = 85, //Chaos
        Sephirot = 86, //Materia
        Sophia = 87, //Materia
        Zurvan = 88, //Materia
        Aegis = 90, //Elemental
        Balmung = 91, //Crystal
        Durandal = 92, //Gaia
        Excalibur = 93, //Primal
        Gungnir = 94, //Elemental
        Hyperion = 95, //Primal
        Masamune = 96, //Mana
        Ragnarok = 97, //Chaos
        Ridill = 98, //Gaia
        Sargatanas = 99, //Aether
        Sagittarius = 400, //Chaos
        Phantom = 401, //Chaos
        Alpha = 402, //Light
        Raiden = 403, //Light
        Marilith = 404, //Dynamis
        Seraph = 405, //Dynamis
        Halicarnassus = 406, //Dynamis
        Maduin = 407, //Dynamis

        // DC Group: 陆行鸟
        [Description("拉诺西亚")]
        LaNuoXiYa = 1042,

        [Description("幻影群岛")]
        HuanYingQunDao = 1044,

        [Description("萌芽池")]
        MengYaChi = 1060,

        [Description("神意之地")]
        ShenYiZhiDi = 1081,

        [Description("红玉海")]
        HongYuHai = 1167,

        [Description("宇宙和音")]
        YuZhouHeYin = 1173,

        [Description("沃仙曦染")]
        WoXianXiRan = 1174,

        [Description("晨曦王座")]
        ChenXiWangZuo = 1175,

        // DC Group: 猫小胖
        [Description("紫水栈桥")]
        ZiShuiZhanQiao = 1043,

        [Description("摩杜纳")]
        MoDuNa = 1045,

        [Description("静语庄园")]
        JingYuZhuangYuan = 1106,

        [Description("延夏")]
        YanXia = 1169,

        [Description("海猫茶屋")]
        HaiMaoChaWu = 1177,

        [Description("柔风海湾")]
        RouFengHaiWan = 1178,

        [Description("琥珀原")]
        HuPoYuan = 1179,

        // DC Group: 豆豆柴
        [Description("太阳海岸")]
        TaiyangHaiAn = 1048,

        [Description("银泪湖")]
        YinLeiHu = 1050,

        [Description("红茶川")]
        HongChaChuan = 1056,

        [Description("伊修加德")]
        YiXiuJiaDe = 1057,

        [Description("水晶塔")]
        ShuiJingTa = 1074,

        [Description("太阳海岸")]
        TaiYangHaiAn2 = 1180,

        [Description("银泪湖")]
        YinLeiHu2 = 1183,

        [Description("伊修加德")]
        YiXiuJiaDe2 = 1186,

        [Description("水晶塔")]
        ShuiJingTa2 = 1192,

        [Description("红茶川")]
        HongChaChuan2 = 1201,

        // DC Group: 莫古力
        [Description("白金幻象")]
        BaiJinHuanXiang = 1076,

        [Description("旅人栈桥")]
        LvRenZhanQiao = 1113,

        [Description("拂晓之间")]
        FuXiaoZhiJian = 1121,

        [Description("龙巢神殿")]
        Longchaoshendian = 1166,

        [Description("潮风亭")]
        ChaoFengTing = 1170,

        [Description("神拳痕")]
        ShenQuanHen = 1171,

        [Description("白银乡")]
        BaiYinXiang = 1172,

        [Description("梦羽宝境")]
        MengYuBaoJing = 1176,
    }

    public static class Extensions
    {
        private static readonly Dictionary<World, string> NameCache = new Dictionary<World, string>();
        private static readonly Dictionary<WorldDCGroupType, string> DCNameCache = new Dictionary<WorldDCGroupType, string>();

        public static string WorldName(this World world)
        {
            if (!NameCache.ContainsKey(world))
            {
                NameCache.Add(world, world.GetAttribute<DescriptionAttribute>()?.Description ?? world.ToString());
            }

            return NameCache[world];
        }

        public static string DCName(this WorldDCGroupType dcGroup)
        {
            if (!DCNameCache.ContainsKey(dcGroup))
            {
                DCNameCache.Add(dcGroup, dcGroup.GetAttribute<DescriptionAttribute>()?.Description ?? dcGroup.ToString());
            }

            return DCNameCache[dcGroup];
        }
    }
}