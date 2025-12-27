using System;
using System.Linq;
using System.Windows.Media;
using ff14bot;
using ff14bot.Managers;
using ff14bot.RemoteWindows;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers
{
    public static class CraftingHelper
    {
        private static readonly LLogger Log = new(nameof(CraftingHelper), Colors.Bisque);

        

        public static CraftingStatus Status => Core.Memory.Read<CraftingStatus>(CraftingHelperOffsets.DohLastAction);

        public static bool AnimationLocked => CraftingManager.AnimationLocked;

        public static byte[] CraftedRecipeByteArray => Core.Memory.ReadBytes(CraftingHelperOffsets.QuestPointer + CraftingHelperOffsets.OffsetRecipes, CraftingHelperOffsets.LengthOfArray);

        public static bool HasCraftedRecipe(ushort recipeId)
        {
            return Core.Memory.CallInjectedWraper<bool>(
                CraftingHelperOffsets.HasCraftedRecipe,
                recipeId);
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
            return Core.Memory.CallInjectedWraper<byte>(Offsets.IsSecretRecipeBookUnlocked, Offsets.PlayerState, key) == 1;
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
            return Core.Memory.CallInjectedWraper<byte>(Offsets.IsFolkloreBookUnlocked, Offsets.PlayerState, key) == 1;
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

                return CraftingManager.Quality;
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

                return CraftingManager.Step;
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

                return CraftingManager.HQPercent;
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

                return CraftingManager.Durability;
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

                return CraftingManager.Progress;
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

                return CraftingManager.LastActionId;
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