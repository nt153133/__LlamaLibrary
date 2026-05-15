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
    /// <summary>
    /// Wraps <see cref="CraftingManager"/> with a memory-read fallback that reads crafting state directly
    /// from game memory when the managed API reports stale values.
    /// Also exposes helpers for crafted-recipe bitmask decoding and book-unlock checks.
    /// </summary>
    public static class CraftingHelper
    {
        private static readonly LLogger Log = new(nameof(CraftingHelper), Colors.Bisque);

        

        /// <summary>
        /// Gets the raw crafting status struct read directly from game memory,
        /// valid when <see cref="IsValid(CraftingStatus)"/> returns <see langword="true"/>.
        /// </summary>
        public static CraftingStatus Status => Core.Memory.Read<CraftingStatus>(CraftingHelperOffsets.DohLastAction);

        /// <summary>
        /// Gets a value indicating whether the crafting animation is currently locked
        /// (i.e., an action is playing and input is blocked).
        /// </summary>
        public static bool AnimationLocked => CraftingManager.AnimationLocked;

        /// <summary>
        /// Gets the raw byte array from game memory that encodes which recipes the player has crafted.
        /// Each bit in the array corresponds to a recipe ID; a set bit means that recipe has been crafted at least once.
        /// </summary>
        public static byte[] CraftedRecipeByteArray => Core.Memory.ReadBytes(CraftingHelperOffsets.QuestPointer + CraftingHelperOffsets.OffsetRecipes, CraftingHelperOffsets.LengthOfArray);

        /// <summary>
        /// Checks whether the player has ever crafted the recipe with the given ID using an injected game call.
        /// </summary>
        /// <param name="recipeId">The recipe ID to check.</param>
        /// <returns><see langword="true"/> if the player has crafted this recipe at least once.</returns>
        public static bool HasCraftedRecipe(ushort recipeId)
        {
            return Core.Memory.CallInjectedWraper<bool>(
                CraftingHelperOffsets.HasCraftedRecipe,
                recipeId);
        }

        /// <summary>
        /// Returns an array of recipe IDs the player has crafted at least once,
        /// decoded from the <see cref="CraftedRecipeByteArray"/> bitmask.
        /// </summary>
        /// <returns>Array of crafted recipe IDs (1-based indices).</returns>
        public static ushort[] CraftedRecipes()
        {
            var craftedList = GetCraftedRecipeStatusArray().Select((crafted, index) => (crafted, index)).Where(i => i.index > 0 && i.crafted).Select(i => (ushort)i.index);

            return craftedList.ToArray();
        }

        /// <summary>
        /// Returns an array of recipe IDs the player has <em>never</em> crafted,
        /// decoded from the <see cref="CraftedRecipeByteArray"/> bitmask.
        /// </summary>
        /// <returns>Array of un-crafted recipe IDs (1-based indices).</returns>
        public static ushort[] NotCraftedRecipes()
        {
            var craftedList = GetCraftedRecipeStatusArray().Select((crafted, index) => (crafted, index)).Where(i => i.index > 0 && !i.crafted).Select(i => (ushort)i.index);

            return craftedList.ToArray();
        }

        /// <summary>
        /// Decodes <see cref="CraftedRecipeByteArray"/> into a flat boolean array.
        /// Index <c>i</c> is <see langword="true"/> if recipe ID <c>i</c> has been crafted.
        /// Index 0 is always ignored (recipe IDs are 1-based).
        /// </summary>
        /// <returns>Boolean array of length <c>CraftedRecipeByteArray.Length × 8</c>.</returns>
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

        /// <summary>
        /// Checks whether the Secret Recipe Book identified by <paramref name="key"/> is unlocked
        /// for the local player, using an injected game function call against the player state.
        /// </summary>
        /// <param name="key">The numeric key for the Secret Recipe Book entry in game data.</param>
        /// <returns><see langword="true"/> if the book is unlocked.</returns>
        public static bool IsSecretRecipeBookUnlocked(uint key)
        {
            return Core.Memory.CallInjectedWraper<byte>(Offsets.IsSecretRecipeBookUnlocked, Offsets.PlayerState, key) == 1;
        }

        /// <summary>
        /// Checks whether the Secret Recipe Book associated with the given item ID is unlocked.
        /// Looks up the book key in <see cref="BookConstants.SecretRecipeBooks"/> then delegates to <see cref="IsSecretRecipeBookUnlocked"/>.
        /// </summary>
        /// <param name="key">The item ID of the Secret Recipe Book item.</param>
        /// <returns><see langword="true"/> if the corresponding recipe book is unlocked; <see langword="false"/> if unknown or locked.</returns>
        public static bool IsSecretRecipeBookUnlockedItem(uint key)
        {
            if (BookConstants.SecretRecipeBooks.TryGetValue(key, out var book))
            {
                return IsSecretRecipeBookUnlocked(book);
            }

            return false;
        }

        /// <summary>
        /// Checks whether the Folklore Book identified by <paramref name="key"/> is unlocked
        /// for the local player, using an injected game function call against the player state.
        /// </summary>
        /// <param name="key">The numeric key for the Folklore Book entry in game data.</param>
        /// <returns><see langword="true"/> if the book is unlocked.</returns>
        public static bool IsFolkloreBookUnlocked(uint key)
        {
            return Core.Memory.CallInjectedWraper<byte>(Offsets.IsFolkloreBookUnlocked, Offsets.PlayerState, key) == 1;
        }

        /// <summary>
        /// Checks whether the Folklore Book associated with the given item ID is unlocked.
        /// Looks up the book key in <see cref="BookConstants.FolkloreBooks"/> then delegates to <see cref="IsFolkloreBookUnlocked"/>.
        /// </summary>
        /// <param name="key">The item ID of the Folklore Book item.</param>
        /// <returns><see langword="true"/> if the corresponding folklore book is unlocked; <see langword="false"/> if unknown or locked.</returns>
        public static bool IsFolkloreBookUnlockedItem(uint key)
        {
            if (BookConstants.FolkloreBooks.TryGetValue(key, out var book))
            {
                return IsFolkloreBookUnlocked(book);
            }

            return false;
        }

        /// <summary>
        /// Returns <see langword="true"/> when the given <see cref="CraftingStatus"/> was read from a valid
        /// crafting session (stage 9 = normal synthesis, stage 10 = expert synthesis).
        /// </summary>
        /// <param name="status">The crafting status struct to validate.</param>
        /// <returns><see langword="true"/> if the status is from an active synthesis.</returns>
        public static bool IsValid(CraftingStatus status)
        {
            if (status.Stage is 9 or 10)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the current crafting quality. Reads from game memory when a valid crafting status is available,
        /// falling back to <see cref="CraftingManager.Quality"/> otherwise.
        /// </summary>
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

        /// <summary>
        /// Gets the current synthesis step number. Reads from game memory when a valid crafting status is available,
        /// falling back to <see cref="CraftingManager.Step"/> otherwise.
        /// </summary>
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

        /// <summary>
        /// Gets the current HQ percentage (0–100). Reads from game memory when a valid crafting status is available,
        /// falling back to <see cref="CraftingManager.HQPercent"/> otherwise.
        /// </summary>
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

        /// <summary>
        /// Gets the current durability. Reads from game memory when a valid crafting status is available,
        /// falling back to <see cref="CraftingManager.Durability"/> otherwise.
        /// </summary>
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

        /// <summary>
        /// Gets the current synthesis progress. Reads from game memory when a valid crafting status is available,
        /// falling back to <see cref="CraftingManager.Progress"/> otherwise.
        /// </summary>
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

        /// <summary>
        /// Gets the action ID of the last crafting action used. Reads from game memory when a valid crafting
        /// status is available, falling back to <see cref="CraftingManager.LastActionId"/> otherwise.
        /// </summary>
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

        /// <summary>Gets the synthesis progress required to complete the current recipe.</summary>
        public static int ProgressRequired => CraftingManager.ProgressRequired;
        /// <summary>Gets the maximum durability for the current recipe.</summary>
        public static int DurabilityCap => (int)Synthesis.GetProperty("DurabilityCap");
        /// <summary>Gets the maximum quality achievable for the current recipe.</summary>
        public static int QualityCap => (int)Synthesis.GetProperty("QualityCap");
        /// <summary>Gets the icon ID of the item being synthesised.</summary>
        public static int IconId => (int)Synthesis.GetProperty("IconId");
        /// <summary>Gets a value indicating whether the player is actively engaged in a synthesis.</summary>
        public static bool IsCrafting => CraftingManager.IsCrafting;
        /// <summary>Gets a value indicating whether the player can start a new synthesis.</summary>
        public static bool CanCraft => CraftingManager.CanCraft;
        /// <summary>Gets the recipe data for the item currently being synthesised.</summary>
        public static RecipeData CurrentRecipe => CraftingManager.CurrentRecipe;
    }
}