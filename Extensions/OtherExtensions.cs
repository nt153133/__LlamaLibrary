using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LlamaLibrary.JsonObjects.Lisbeth;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LlamaLibrary.Extensions
{
    public static class OtherExtensions
    {
        private static readonly Random Rng = new();

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

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.Shuffle(Rng);
        }

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
    }
}