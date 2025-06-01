using System.Collections.Generic;

namespace LlamaLibrary.Helpers
{
    public static class BookConstants
    {
        //The ID of the book to check for. Can be retrieved from the SecretRecipeBook sheet.
        public static IReadOnlyDictionary<uint, uint> SecretRecipeBooks = new Dictionary<uint, uint>
        {
            { 7778, 1 }, //Master Carpenter I
            { 7779, 2 }, //Master Blacksmith I
            { 7780, 3 }, //Master Armorer I
            { 7781, 4 }, //Master Goldsmith I
            { 7782, 5 }, //Master Leatherworker I
            { 7783, 6 }, //Master Weaver I
            { 7784, 7 }, //Master Alchemist I
            { 7785, 8 }, //Master Culinarian I
            { 7786, 9 }, //Master Carpenter: Glamours
            { 7787, 10 }, //Master Blacksmith: Glamours
            { 7788, 11 }, //Master Armorer: Glamours
            { 7789, 12 }, //Master Goldsmith: Glamours
            { 7790, 13 }, //Master Leatherworker: Glamours
            { 7791, 14 }, //Master Weaver: Glamours
            { 7792, 15 }, //Master Alchemist: Glamours
            { 8135, 17 }, //Master Carpenter: Demimateria
            { 8136, 18 }, //Master Blacksmith: Demimateria
            { 8137, 19 }, //Master Armorer: Demimateria
            { 8138, 20 }, //Master Goldsmith: Demimateria
            { 8139, 21 }, //Master Leatherworker: Demimateria
            { 8140, 22 }, //Master Weaver: Demimateria
            { 8141, 23 }, //Master Alchemist: Demimateria
            { 9336, 24 }, //Master Carpenter II
            { 9337, 25 }, //Master Blacksmith II
            { 9338, 26 }, //Master Armorer II
            { 9339, 27 }, //Master Goldsmith II
            { 9340, 28 }, //Master Leatherworker II
            { 9341, 29 }, //Master Weaver II
            { 9342, 30 }, //Master Alchemist II
            { 9343, 31 }, //Master Culinarian II
            { 12244, 32 }, //Master Carpenter III
            { 12245, 33 }, //Master Blacksmith III
            { 12246, 34 }, //Master Armorer III
            { 12247, 35 }, //Master Goldsmith III
            { 12248, 36 }, //Master Leatherworker III
            { 12249, 37 }, //Master Weaver III
            { 12250, 38 }, //Master Alchemist III
            { 12251, 39 }, //Master Culinarian III
            { 14126, 40 }, //Master Carpenter IV
            { 14127, 41 }, //Master Blacksmith IV
            { 14128, 42 }, //Master Armorer IV
            { 14129, 43 }, //Master Goldsmith IV
            { 14130, 44 }, //Master Leatherworker IV
            { 14131, 45 }, //Master Weaver IV
            { 14132, 46 }, //Master Alchemist IV
            { 14133, 47 }, //Master Culinarian IV
            { 17869, 48 }, //Master Carpenter V
            { 17870, 49 }, //Master Blacksmith V
            { 17871, 50 }, //Master Armorer V
            { 17872, 51 }, //Master Goldsmith V
            { 17873, 52 }, //Master Leatherworker V
            { 17874, 53 }, //Master Weaver V
            { 17875, 54 }, //Master Alchemist V
            { 17876, 55 }, //Master Culinarian V
            { 22309, 56 }, //Master Carpenter VI
            { 22310, 57 }, //Master Blacksmith VI
            { 22311, 58 }, //Master Armorer VI
            { 22312, 59 }, //Master Goldsmith VI
            { 22313, 60 }, //Master Leatherworker VI
            { 22314, 61 }, //Master Weaver VI
            { 22315, 62 }, //Master Alchemist VI
            { 22316, 63 }, //Master Culinarian VI
            { 24266, 64 }, //Master Carpenter VII
            { 24267, 65 }, //Master Blacksmith VII
            { 24268, 66 }, //Master Armorer VII
            { 24269, 67 }, //Master Goldsmith VII
            { 24270, 68 }, //Master Leatherworker VII
            { 24271, 69 }, //Master Weaver VII
            { 24272, 70 }, //Master Alchemist VII
            { 24273, 71 }, //Master Culinarian VII
            { 29484, 72 }, //Master Carpenter VIII
            { 29485, 73 }, //Master Blacksmith VIII
            { 29486, 74 }, //Master Armorer VIII
            { 29487, 75 }, //Master Goldsmith VIII
            { 29488, 76 }, //Master Leatherworker VIII
            { 29489, 77 }, //Master Weaver VIII
            { 29490, 78 }, //Master Alchemist VIII
            { 29491, 79 }, //Master Culinarian VIII
            { 35618, 80 }, //Master Carpenter IX
            { 35619, 81 }, //Master Blacksmith IX
            { 35620, 82 }, //Master Armorer IX
            { 35621, 83 }, //Master Goldsmith IX
            { 35622, 84 }, //Master Leatherworker IX
            { 35623, 85 }, //Master Weaver IX
            { 35624, 86 }, //Master Alchemist IX
            { 35625, 87 }, //Master Culinarian IX
            { 37734, 88 }, //Master Carpenter X
            { 37735, 89 }, //Master Blacksmith X
            { 37736, 90 }, //Master Armorer X
            { 37737, 91 }, //Master Goldsmith X
            { 37738, 92 }, //Master Leatherworker X
            { 37739, 93 }, //Master Weaver X
            { 37740, 94 }, //Master Alchemist X
            { 37741, 95 }, //Master Culinarian X
        };

        public static IReadOnlyDictionary<uint, uint> FolkloreBooks = new Dictionary<uint, uint>
        {
            { 12238, 2000 }, //Tome of Geological Folklore - Coerthas
            { 12698, 2002 }, //Tome of Botanical Folklore - Coerthas
            { 12239, 2004 }, //Tome of Geological Folklore - Dravania
            { 12699, 2006 }, //Tome of Botanical Folklore - Dravania
            { 12240, 2008 }, //Tome of Geological Folklore - Abalathia's Spine
            { 12700, 2010 }, //Tome of Botanical Folklore - Abalathia's Spine
            { 17838, 2012 }, //Tome of Geological Folklore - Gyr Abania
            { 17840, 2014 }, //Tome of Botanical Folklore - Gyr Abania
            { 17839, 2016 }, //Tome of Geological Folklore - Othard
            { 17841, 2018 }, //Tome of Botanical Folklore - Othard
            { 26808, 2020 }, //Tome of Geological Folklore - Norvrandt
            { 26809, 2022 }, //Tome of Botanical Folklore - Norvrandt
            { 36598, 2024 }, //Tome of Geological Folklore - Ilsabard and the Northern Empty
            { 36602, 2026 }, //Tome of Botanical Folklore - Ilsabard and the Northern Empty
            { 36600, 2028 }, //Tome of Geological Folklore - The Sea of Stars
            { 36604, 2030 }, //Tome of Botanical Folklore - The Sea of Stars
            { 36601, 2032 }, //Tome of Geological Folklore - The World Unsundered
            { 36605, 2034 }, //Tome of Botanical Folklore - The World Unsundered
            { 12701, 2500 }, //Tome of Ichthyological Folklore - Coerthas
            { 12702, 2501 }, //Tome of Ichthyological Folklore - Dravania
            { 12703, 2502 }, //Tome of Ichthyological Folklore - Abalathia's Spine
            { 17842, 2503 }, //Tome of Ichthyological Folklore - Gyr Abania
            { 17843, 2505 }, //Tome of Ichthyological Folklore - Othard
            { 26810, 2507 }, //Tome of Ichthyological Folklore - Norvrandt
            { 36606, 2509 }, //Tome of Ichthyological Folklore - Ilsabard and the Northern Empty
            { 36608, 2511 }, //Tome of Ichthyological Folklore - The Sea of Stars
            { 36609, 2513 }, //Tome of Ichthyological Folklore - The World Unsundered
        };
    }
}