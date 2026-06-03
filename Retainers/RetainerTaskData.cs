using ff14bot.Managers;
using Newtonsoft.Json;

namespace LlamaLibrary.Retainers
{
    /// <summary>
    /// Represents static data for a retainer venture task, typically loaded from an external JSON source or game data.
    /// Includes requirements for assignment and details about the expected reward.
    /// </summary>
    //TODO This needs to be somewhere else, Maybe a DataClass folder/namespace
    public class RetainerTaskData
    {
        /// <summary>
        /// Gets or sets the unique identifier for the venture task.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the category of class or job allowed to perform this venture.
        /// </summary>
        public byte ClassJobCategory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the venture reward is random (e.g., Quick Exploration).
        /// </summary>
        public bool IsRandom { get; set; }

        /// <summary>
        /// Gets or sets the minimum level the retainer must be to perform this venture.
        /// </summary>
        public int RetainerLevel { get; set; }

        /// <summary>
        /// Gets or sets the number of Venture scripts required to assign this task.
        /// </summary>
        public int VentureCost { get; set; }

        /// <summary>
        /// Gets or sets the maximum time (in minutes) the venture takes to complete.
        /// </summary>
        public int MaxTime { get; set; }

        /// <summary>
        /// Gets or sets the amount of experience points the retainer earns upon completion.
        /// </summary>
        public int Experience { get; set; }

        /// <summary>
        /// Gets or sets the minimum item level required for the retainer to perform this venture.
        /// </summary>
        public int RequiredItemLevel { get; set; }

        /// <summary>
        /// Gets or sets the minimum gathering stat required for the retainer to perform this venture (for gathering jobs).
        /// </summary>
        public int RequiredGathering { get; set; }

        /// <summary>
        /// Gets or sets the raw name of the venture task as defined in the source data.
        /// </summary>
        public string NameRaw { get; set; }

        /// <summary>
        /// Gets or sets the ID of the item rewarded by this venture (for non-random ventures).
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// Gets the display name of the venture.
        /// For random ventures, returns <see cref="NameRaw"/>; for fixed ventures, resolves the localized name of the reward item.
        /// </summary>
        [JsonIgnore]
        public string Name
        {
            get
            {
                if (IsRandom)
                {
                    return NameRaw;
                }

                return DataManager.GetItem((uint)ItemId).CurrentLocaleName;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetainerTaskData"/> class.
        /// </summary>
        /// <param name="id">The venture task ID.</param>
        /// <param name="classJobCategory">The allowed class/job category.</param>
        /// <param name="isRandom">Whether the reward is random.</param>
        /// <param name="retainerLevel">Minimum retainer level.</param>
        /// <param name="ventureCost">Cost in Venture currency.</param>
        /// <param name="maxTime">Completion time in minutes.</param>
        /// <param name="experience">XP reward.</param>
        /// <param name="requiredItemLevel">Minimum iLvl requirement.</param>
        /// <param name="requiredGathering">Minimum gathering requirement.</param>
        /// <param name="nameRaw">Raw name string.</param>
        /// <param name="itemId">Reward item ID.</param>
        [JsonConstructor]
        public RetainerTaskData(int id, byte classJobCategory, bool isRandom, int retainerLevel, int ventureCost, int maxTime, int experience, int requiredItemLevel, int requiredGathering, string nameRaw, int itemId)
        {
            Id = id;
            ClassJobCategory = classJobCategory;
            IsRandom = isRandom;
            RetainerLevel = retainerLevel;
            VentureCost = ventureCost;
            MaxTime = maxTime;
            Experience = experience;
            RequiredItemLevel = requiredItemLevel;
            RequiredGathering = requiredGathering;
            NameRaw = nameRaw;
            ItemId = itemId;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Id} - {IsRandom} {NameRaw}";
        }
    }
}
