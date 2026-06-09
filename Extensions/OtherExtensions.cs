using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Clio.Utilities;
using ff14bot.ServiceClient;
using LlamaLibrary.JsonObjects.Lisbeth;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LlamaLibrary.Extensions
{
    /// <summary>
    /// Provides miscellaneous extension methods for common types like <see cref="Enum"/>, <see cref="IEnumerable{T}"/>, and <see cref="BoundingCircle"/>.
    /// </summary>
    public static class OtherExtensions
    {
        private static readonly Random Rng = new();

        /// <summary>
        /// Converts an enum value to a string and adds spaces before each uppercase letter (except the first).
        /// Useful for display purposes in a UI.
        /// </summary>
        /// <param name="toText">The enum value to format.</param>
        /// <returns>A string representation of the enum with spaces added.</returns>
        public static string AddSpacesToEnum(this Enum toText)
        {
            var text = toText.ToString();
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }

            var newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (var i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                {
                    newText.Append(' ');
                }

                newText.Append(text[i]);
            }

            return newText.ToString();
        }

        /// <summary>
        /// Randomly shuffles the elements of an <see cref="IEnumerable{T}"/> using the default random number generator.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The collection to shuffle.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> with shuffled elements.</returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.Shuffle(Rng);
        }

        /// <summary>
        /// Randomly shuffles the elements of an <see cref="IEnumerable{T}"/> using a specified random number generator.
        /// Uses the Fisher-Yates shuffle algorithm.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The collection to shuffle.</param>
        /// <param name="rng">The random number generator to use.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> with shuffled elements.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="rng"/> is null.</exception>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (rng == null)
            {
                throw new ArgumentNullException(nameof(rng));
            }

            return source.ShuffleIterator(rng);
        }

        private static IEnumerable<T> ShuffleIterator<T>(
            this IEnumerable<T> source, Random rng)
        {
            var buffer = source.ToList();
            for (var i = 0; i < buffer.Count; i++)
            {
                var j = rng.Next(i, buffer.Count);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }

        /// <summary>
        /// Serializes a collection of Lisbeth <see cref="Order"/> objects into a JSON string.
        /// Configured to ignore null and default values and use string representations for enums.
        /// </summary>
        /// <param name="orders">The collection of orders to serialize.</param>
        /// <returns>A formatted JSON string representing the orders, sorted by their type.</returns>
        public static string GetOrderJson(this IEnumerable<Order> orders)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
            settings.Converters.Add(new StringEnumConverter());
            return JsonConvert.SerializeObject(orders.OrderBy(i => i.Type), Formatting.Indented, settings);
        }

        /// <summary>
        /// Determines whether a string contains a specified substring using the specified comparison rules.
        /// </summary>
        /// <param name="source">The source string to search.</param>
        /// <param name="toCheck">The substring to look for.</param>
        /// <param name="comp">One of the enumeration values that specifies the rules for the search.</param>
        /// <returns><see langword="true"/> if the substring is found; otherwise <see langword="false"/>.</returns>
        public static bool Contains(this string? source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        /// <summary>
        /// Determines whether a 2D point is within the bounds of a <see cref="BoundingCircle"/>.
        /// </summary>
        /// <param name="source">The bounding circle.</param>
        /// <param name="toCheck">The 2D coordinates to check.</param>
        /// <returns><see langword="true"/> if the point is inside or on the boundary of the circle; otherwise <see langword="false"/>.</returns>
        public static bool Contains(this BoundingCircle source, Vector2 toCheck)
        {
            return source.Center.ToVector2().Distance(toCheck) <= source.Radius;
        }

        /// <summary>
        /// Determines whether a 3D point is within the 2D bounds of a <see cref="BoundingCircle"/>, ignoring the Z-axis.
        /// </summary>
        /// <param name="source">The bounding circle.</param>
        /// <param name="toCheck">The 3D coordinates to check.</param>
        /// <returns><see langword="true"/> if the projected 2D point is inside or on the boundary of the circle; otherwise <see langword="false"/>.</returns>
        public static bool ContainsIgnoreZ(this BoundingCircle source, Vector3 toCheck)
        {
            return source.Center.ToVector2().Distance(toCheck.ToVector2()) <= source.Radius;
        }
    }
}