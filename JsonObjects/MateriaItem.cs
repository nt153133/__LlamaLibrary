using ff14bot.Managers;

namespace LlamaLibrary.JsonObjects
{
    /// <summary>
    /// Represents data for a specific materia item, typically loaded from embedded resources.
    /// </summary>
    public class MateriaItem
    {
        /// <summary>The numeric identifier (Item ID) of the materia.</summary>
        public int Key;

        /// <summary>The tier of the materia (e.g., I=1, X=10).</summary>
        public int Tier;

        /// <summary>The value of the stat bonus provided by the materia.</summary>
        public int Value;

        /// <summary>Gets the <see cref="Item"/> data for this materia from the game's DataManager.</summary>
        internal Item Item => DataManager.GetItem((uint)Key);

        /// <summary>Gets the localized name of the materia item.</summary>
        public string ItemName => Item.CurrentLocaleName;

        /// <summary>The name of the stat this materia enhances (e.g., "Critical Hit").</summary>
        public string Stat;

        /// <summary>
        /// Initializes a new instance of the <see cref="MateriaItem"/> class.
        /// </summary>
        /// <param name="key">The Item ID.</param>
        /// <param name="tier">The materia tier.</param>
        /// <param name="value">The stat bonus value.</param>
        /// <param name="stat">The name of the enhanced stat.</param>
        public MateriaItem(int key, int tier, int value, string stat)
        {
            Key = key;
            Tier = tier;
            Value = value;
            Stat = stat;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{DataManager.GetItem((uint)Key).CurrentLocaleName} {Tier} {Value}";
        }
    }
}
