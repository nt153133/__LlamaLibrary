using System;
using System.Linq;
using System.Reflection;
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
            return type is ClassJobType.Carpenter or
                ClassJobType.Blacksmith or
                ClassJobType.Armorer or
                ClassJobType.Goldsmith or
                ClassJobType.Leatherworker or
                ClassJobType.Weaver or
                ClassJobType.Alchemist or
                ClassJobType.Culinarian;
        }

        /// <summary>
        /// Determines if character class is a Disciple of the Land (gatherer).
        /// </summary>
        /// <param name="type">The <see cref="ClassJobType"/> to be categorized.</param>
        /// <returns><see langword="true"/> if class is a Disciple of the Land.</returns>
        public static bool IsDol(this ClassJobType type)
        {
            return type is ClassJobType.Miner or
                ClassJobType.Botanist or
                ClassJobType.Fisher;
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

            return type.ClassJob() switch
            {
                ClassJobType.Miner    => Enums.ClassJobCategory.MIN,
                ClassJobType.Fisher   => Enums.ClassJobCategory.FSH,
                ClassJobType.Botanist => Enums.ClassJobCategory.BOT,
                _                     => Enums.ClassJobCategory.ANY,
            };
        }

        /// <summary>
        ///     A generic extension method that aids in reflecting
        ///     and retrieving any attribute that is applied to an `Enum`.
        /// </summary>
        public static TAttribute? GetAttribute<TAttribute>(this Enum enumValue)
            where TAttribute : Attribute
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .FirstOrDefault()?
                .GetCustomAttribute<TAttribute>();
        }
    }
}