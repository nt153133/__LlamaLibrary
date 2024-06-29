using System;
using System.Linq;
using System.Windows.Media;
using ff14bot;
using ff14bot.Managers;
using ff14bot.RemoteWindows;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;

namespace LlamaLibrary.Helpers
{
    public static class CraftingHelper
    {
        private static readonly LLogger Log = new(nameof(CraftingHelper), Colors.Bisque);

        private static class Offsets
        {
            [Offset("Search 4C 8D 0D ?? ?? ?? ?? 4D 8B 13 49 8B CB Add 3 TraceRelative")]
            [OffsetDawntrail("Search 4C 8D 0D ? ? ? ? 4C 8B 11 44 0F B7 41 ?  Add 3 TraceRelative")]
            internal static IntPtr DohLastAction;

            [Offset("Search 40 53 48 83 EC ? 8B D9 81 F9 ? ? ? ?")]
            internal static IntPtr HasCraftedRecipe;

            [Offset("Search 4C 8D 1D ? ? ? ? 88 44 24 ? Add 3 TraceRelative")]
            [OffsetDawntrail("Search 48 8D 0D ? ? ? ? E8 ? ? ? ? C6 44 24 ? ?  Add 3 TraceRelative")]
            internal static IntPtr QuestPointer;

            [Offset("Search 81 F9 ? ? ? ? 72 ? 32 C0 48 83 C4 ? Add 2 Read32")]
            [OffsetDawntrail("Search 81 F9 ? ? ? ? 73 38 Add 2 Read32")]
            internal static int NumberOfRecipes;

            [Offset("Search 81 F9 ? ? ? ? 73 ? 44 0F B6 84 01 ? ? ? ?  Add 2 Read32")]
            internal static int LengthOfArray;

            [Offset("Search 44 0F B6 84 01 ? ? ? ? 0F B6 C3 Add 5 Read32")]
            [OffsetDawntrail("Search 44 0F B6 84 01 ? ? ? ? 0F B6 CB  Add 5 Read32")]
            internal static int OffsetRecipes;
        }

        public static CraftingStatus Status => Core.Memory.Read<CraftingStatus>(Offsets.DohLastAction);

        public static bool AnimationLocked => CraftingManager.AnimationLocked;

        public static byte[] CraftedRecipeByteArray => Core.Memory.ReadBytes(Offsets.QuestPointer + Offsets.OffsetRecipes, Offsets.LengthOfArray);

        public static bool HasCraftedRecipe(ushort recipeId)
        {
            using (Core.Memory.TemporaryCacheState(false))
            {
                return Core.Memory.CallInjected64<bool>(
                    Offsets.HasCraftedRecipe,
                    recipeId);
            }
        }

        public static ushort[] CraftedRecipes()
        {
            var craftedList = GetCraftedRecipeStatusArray().Select((crafted, index) => (crafted, index)).Where(i => i.index > 0 && i.crafted).Select(i => (ushort)i.index);

            return craftedList.ToArray();
        }

        public static ushort[] NotCraftedRecipes()
        {
            var craftedList = GetCraftedRecipeStatusArray().Select((crafted, index) => (crafted, index)).Where(i => i.index > 0 && !i.crafted).Select(i => (ushort)i.index);

            return craftedList.ToArray();
        }

        public static bool[] GetCraftedRecipeStatusArray()
        {
            var byteArray = CraftedRecipeByteArray;
            var isCrafted = new bool[byteArray.Length * 8];

            for (var i = 0; i < byteArray.Length; i++)
            {
                var b = byteArray[i];
                for (var j = 0; j < 8; j++)
                {
                    isCrafted[(i * 8) + j] = (128 >> j & b) != 0;
                }
            }

            return isCrafted;
        }

        public static bool IsSecretRecipeBookUnlocked(uint key)
        {
            return Core.Memory.CallInjected64<byte>(Memory.Offsets.IsSecretRecipeBookUnlocked, Memory.Offsets.PlayerState, key) == 1;
        }

        public static bool IsSecretRecipeBookUnlockedItem(uint key)
        {
            if (BookConstants.SecretRecipeBooks.TryGetValue(key, out var book))
            {
                return IsSecretRecipeBookUnlocked(book);
            }

            return false;
        }

        public static bool IsFolkloreBookUnlocked(uint key)
        {
            return Core.Memory.CallInjected64<byte>(Memory.Offsets.IsFolkloreBookUnlocked, Memory.Offsets.PlayerState, key) == 1;
        }

        public static bool IsFolkloreBookUnlockedItem(uint key)
        {
            if (BookConstants.FolkloreBooks.TryGetValue(key, out var book))
            {
                return IsFolkloreBookUnlocked(book);
            }

            return false;
        }

        public static bool IsValid(CraftingStatus status)
        {
            if (status.Stage is 9 or 10)
            {
                return true;
            }

            return false;
        }

        public static int Quality
        {
            get
            {
                var status = Status;
                if (IsValid(status))
                {
                    return (int)status.Quality;
                }
                else
                {
                    return CraftingManager.Quality;
                }
            }
        }

        public static int Step
        {
            get
            {
                var status = Status;
                if (IsValid(status))
                {
                    return (int)status.Step;
                }
                else
                {
                    return CraftingManager.Step;
                }
            }
        }

        public static int HQPercent
        {
            get
            {
                var status = Status;
                if (IsValid(status))
                {
                    return (int)status.HQ;
                }
                else
                {
                    return CraftingManager.HQPercent;
                }
            }
        }

        public static int Durability
        {
            get
            {
                var status = Status;
                if (IsValid(status))
                {
                    return (int)status.Durability;
                }
                else
                {
                    return CraftingManager.Durability;
                }
            }
        }

        public static int Progress
        {
            get
            {
                var status = Status;
                if (IsValid(status))
                {
                    return (int)status.Progress;
                }
                else
                {
                    return CraftingManager.Progress;
                }
            }
        }

        public static uint LastActionId
        {
            get
            {
                var status = Status;
                if (IsValid(status))
                {
                    return status.LastAction;
                }
                else
                {
                    return CraftingManager.LastActionId;
                }
            }
        }

        public static int ProgressRequired => CraftingManager.ProgressRequired;
        public static int DurabilityCap => (int)Synthesis.GetProperty("DurabilityCap");
        public static int QualityCap => (int)Synthesis.GetProperty("QualityCap");
        public static int IconId => (int)Synthesis.GetProperty("IconId");
        public static bool IsCrafting => CraftingManager.IsCrafting;
        public static bool CanCraft => CraftingManager.CanCraft;
        public static RecipeData CurrentRecipe => CraftingManager.CurrentRecipe;
    }
}