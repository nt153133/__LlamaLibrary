using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ff14bot.Managers;

namespace LlamaLibrary.Helpers
{
    public static class ItemLinking
    {
        public static byte[] ChatLink(this Item item)
        {
            var result = new List<byte>();
            result.AddRange(UIForegroundPayload((ushort)(0x223 + item.Rarity * 2)));
            result.AddRange(UIGlowPayload((ushort)(0x224 + item.Rarity * 2)));
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

        public static byte[] UIForegroundPayload(ushort colorKey)
        {
            var collection = MakeInteger(colorKey);
            var byteList = new List<byte>(new byte[3]
            {
                2,
                72,
                (byte)(collection.Length + 1)
            });
            byteList.AddRange(collection);
            byteList.Add(3);
            return byteList.ToArray();
        }

        public static byte[] MakeInteger(uint value)
        {
            if (value < 207U)
            {
                return new byte[1] { (byte)(value + 1U) };
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
            byteList.AddRange(new byte[2]
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

                byteList.AddRange(new byte[2]
                {
                    byte.MaxValue,
                    (byte)num2
                });
                byteList.AddRange(Encoding.UTF8.GetBytes(slot.CurrentLocaleName));
                if (slot.IsHighQuality)
                {
                    byteList.AddRange(new byte[4]
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

        public static byte[] UIGlowPayload(ushort colorKey)
        {
            var collection = MakeInteger(colorKey);
            var byteList = new List<byte>(new byte[3]
            {
                2,
                73,
                (byte)(collection.Length + 1)
            });
            byteList.AddRange(collection);
            byteList.Add(3);
            return byteList.ToArray();
        }

        public static byte[] TextPayload(string text)
        {
            return string.IsNullOrEmpty(text) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(text);
        }

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