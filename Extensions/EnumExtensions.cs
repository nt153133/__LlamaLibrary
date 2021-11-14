using System;
using ff14bot.Enums;
using LlamaLibrary.Enums;

namespace LlamaLibrary.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Determines if character class is a Disciple of War (combat).
        /// </summary>
        /// <param name="type">The <see cref="ClassJobType"/> to be categorized.</param>
        /// <returns><see langword="true"/> if class is a Disciple of War.</returns>
        public static bool IsDow(this ClassJobType type)
        {
            return !IsDoh(type) && !IsDol(type);
        }

        /// <summary>
        /// Determines if character class is a Disciple of the Hand (crafter).
        /// </summary>
        /// <param name="type">The <see cref="ClassJobType"/> to be categorized.</param>
        /// <returns><see langword="true"/> if class is a Disciple of the Hand.</returns>
        public static bool IsDoh(this ClassJobType type)
        {
            return type == ClassJobType.Carpenter ||
                   type == ClassJobType.Blacksmith ||
                   type == ClassJobType.Armorer ||
                   type == ClassJobType.Goldsmith ||
                   type == ClassJobType.Leatherworker ||
                   type == ClassJobType.Weaver ||
                   type == ClassJobType.Alchemist ||
                   type == ClassJobType.Culinarian;
        }

        /// <summary>
        /// Determines if character class is a Disciple of the Land (gatherer).
        /// </summary>
        /// <param name="type">The <see cref="ClassJobType"/> to be categorized.</param>
        /// <returns><see langword="true"/> if class is a Disciple of the Land.</returns>
        public static bool IsDol(this ClassJobType type)
        {
            return type == ClassJobType.Miner ||
                   type == ClassJobType.Botanist ||
                   type == ClassJobType.Fisher;
        }

        public static ClassJobType ClassJob(this RetainerRole type)
        {
            return (ClassJobType)Enum.Parse(typeof(ClassJobType), type.ToString());
        }

        public static ClassJobCategory ClassJobCategory(this RetainerRole type)
        {
            if (type.ClassJob().IsDow())
            {
                return Enums.ClassJobCategory.DOW;
            }

            switch (type.ClassJob())
            {
                case ClassJobType.Miner:
                    return Enums.ClassJobCategory.MIN;
                case ClassJobType.Fisher:
                    return Enums.ClassJobCategory.FSH;
                case ClassJobType.Botanist:
                    return Enums.ClassJobCategory.BOT;
                default:
                    return Enums.ClassJobCategory.ANY;
            }
        }
    }
}