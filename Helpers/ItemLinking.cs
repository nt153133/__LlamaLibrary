using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ff14bot.Managers;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Builds FFXIV chat-link byte payloads for items.
    /// These payloads can be embedded in chat messages (e.g., via ChatBroadcaster) to produce
    /// clickable item links with the correct rarity colour, glow, icon, and name.
    /// </summary>
    public static class ItemLinking
    {
        /// <summary>
        /// Builds the full chat-link byte payload for the given item, including rarity foreground/glow colours,
        /// the item payload, the item-link icon character (U+E10B), and the item name.
        /// </summary>
        /// <param name="item">The item to create a link for.</param>
        /// <returns>A byte array representing the complete FFXIV chat-link payload.</returns>
        public static byte[] ChatLink(this Item item)
        {
            var result = new List<byte>();
            result.AddRange(UIForegroundPayload((ushort)(0x223 + (item.Rarity * 2))));
            result.AddRange(UIGlowPayload((ushort)(0x224 + (item.Rarity * 2))));
            result.AddRange(ItemPayload(item));
            result.AddRange(UIForegroundPayload(500));
            result.AddRange(UIGlowPayload(501));
            result.AddRange(TextPayload($"{(char)57531}"));
            result.AddRange(UIForegroundPayload(0));
            result.AddRange(UIGlowPayload(0));
            result.AddRange(TextPayload(item.CurrentLocaleName + ""));
            result.AddRange(RawPayload(new byte[] { 0x02, 0x27, 0x07, 0xCF, 0x01, 0x01, 0x01, 0xFF, 0x01, 0x03 }));
            result.AddRange(RawPayload(new byte[] { 0x02, 0x13, 0x02, 0xEC, 0x03 }));

            return result.ToArray();
        }

        /// <summary>
        /// Builds a UI foreground colour payload (chunk type 0x48) that sets the text colour
        /// to the palette entry identified by <paramref name="colorKey"/>.
        /// Pass 0 to reset the colour to default.
        /// </summary>
        /// <param name="colorKey">UI colour palette index.</param>
        /// <returns>Encoded payload bytes.</returns>
        public static byte[] UIForegroundPayload(ushort colorKey)
        {
            var collection = MakeInteger(colorKey);
            var byteList = new List<byte>(new byte[]
            {
                2,
                72,
                (byte)(collection.Length + 1)
            });
            byteList.AddRange(collection);
            byteList.Add(3);
            return byteList.ToArray();
        }

        /// <summary>
        /// Encodes an unsigned integer into the compact FFXIV payload integer format.
        /// Values below 207 are stored as a single byte (<c>value + 1</c>).
        /// Larger values use a multi-byte big-endian encoding with a leading flag byte.
        /// </summary>
        /// <param name="value">The value to encode.</param>
        /// <returns>The encoded byte sequence.</returns>
        public static byte[] MakeInteger(uint value)
        {
            if (value < 207U)
            {
                return new[] { (byte)(value + 1U) };
            }

            var bytes = BitConverter.GetBytes(value);
            var byteList = new List<byte>
            {
                240
            };
            for (var index = 3; index >= 0; --index)
            {
                if (bytes[index] != 0)
                {
                    byteList.Add(bytes[index]);
                    byteList[0] |= (byte)(1 << index);
                }
            }

            --byteList[0];
            return byteList.ToArray();
        }

        /// <summary>
        /// Builds an item payload (chunk type 0x27) for the given item, embedding its ID, name,
        /// and an optional HQ marker (U+E03C sequence) when the item is high quality.
        /// </summary>
        /// <param name="slot">The item to encode.</param>
        /// <returns>Encoded payload bytes.</returns>
        public static byte[] ItemPayload(Item slot)
        {
            var collection = MakeInteger(slot.Id);
            var flag = !string.IsNullOrEmpty(slot.CurrentLocaleName);
            var num1 = collection.Length + 4;
            if (flag)
            {
                num1 += 2 + slot.CurrentLocaleName.Length;
                if (slot.IsHighQuality)
                {
                    num1 += 4;
                }
            }

            var byteList = new List<byte>
            {
                2,
                39,
                (byte)num1,
                3
            };
            byteList.AddRange(collection);
            byteList.AddRange(new byte[]
            {
                2,
                1
            });
            if (flag)
            {
                var num2 = slot.CurrentLocaleName.Length + 1;
                if (slot.IsHighQuality)
                {
                    num2 += 4;
                }

                byteList.AddRange(new[]
                {
                    byte.MaxValue,
                    (byte)num2
                });
                byteList.AddRange(Encoding.UTF8.GetBytes(slot.CurrentLocaleName));
                if (slot.IsHighQuality)
                {
                    byteList.AddRange(new byte[]
                    {
                        32,
                        238,
                        128,
                        188
                    });
                }
            }

            byteList.Add(3);
            return byteList.ToArray();
        }

        /// <summary>
        /// Builds a UI glow colour payload (chunk type 0x49) that sets the text glow/outline colour
        /// to the palette entry identified by <paramref name="colorKey"/>.
        /// Pass 0 to reset the glow to default.
        /// </summary>
        /// <param name="colorKey">UI glow colour palette index.</param>
        /// <returns>Encoded payload bytes.</returns>
        public static byte[] UIGlowPayload(ushort colorKey)
        {
            var collection = MakeInteger(colorKey);
            var byteList = new List<byte>(new byte[]
            {
                2,
                73,
                (byte)(collection.Length + 1)
            });
            byteList.AddRange(collection);
            byteList.Add(3);
            return byteList.ToArray();
        }

        /// <summary>
        /// Encodes <paramref name="text"/> as a raw UTF-8 byte sequence.
        /// Returns an empty array when <paramref name="text"/> is null or empty.
        /// </summary>
        /// <param name="text">The text to encode.</param>
        /// <returns>UTF-8 bytes, or an empty array.</returns>
        public static byte[] TextPayload(string text)
        {
            return string.IsNullOrEmpty(text) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(text);
        }

        /// <summary>
        /// Strips the 3-byte header and 1-byte trailer from a pre-encoded FFXIV payload chunk
        /// and re-wraps it with the correct length byte, returning the normalised chunk.
        /// </summary>
        /// <param name="data">A fully formed FFXIV payload chunk (starts with 0x02, ends with 0x03).</param>
        /// <returns>The re-wrapped payload bytes.</returns>
        public static byte[] RawPayload(byte[] data)
        {
            var chunkType = data[1];
            data = data.Skip(3).Take(data.Length - 4).ToArray();
            var byteList = new List<byte>
            {
                2,
                chunkType,
                (byte)(data.Length + 1)
            };
            byteList.AddRange(data);
            byteList.Add(3);
            return byteList.ToArray();
        }
    }
}