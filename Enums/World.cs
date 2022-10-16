namespace LlamaLibrary.Enums
{
    public enum World : ushort
    {
        SetMe = 0,
        Ravana = 21, //Materia
        Bismarck = 22, //Materia
        Asura = 23, //Mana
        Belias = 24, //Mana
        Pandaemonium = 28, //Mana
        Shinryu = 29, //Mana
        Unicorn = 30, //Elemental
        Yojimbo = 31, //Gaia
        Zeromus = 32, //Gaia
        Twintania = 33, //Light
        Brynhildr = 34, //Crystal
        Famfrit = 35, //Primal
        Lich = 36, //Light
        Mateus = 37, //Crystal
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
        Valefor = 52, //Gaia
        Exodus = 53, //Primal
        Faerie = 54, //Aether
        Lamia = 55, //Primal
        Phoenix = 56, //Light
        Siren = 57, //Aether
        Garuda = 58, //Elemental
        Ifrit = 59, //Gaia
        Ramuh = 60, //Elemental
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
        Mandragora = 82, //Mana
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
    }

    public static class Extensions
    {
        public static string WorldName(this World world)
        {
            return world.ToString();
        }
    }
}