using System;
using System.Runtime.InteropServices;
using Clio.Utilities;
using ff14bot.Enums;
using LlamaLibrary.Enums;

namespace LlamaLibrary.Structs
{
    /// <summary>
    /// Represents raw memory data for a retainer, as defined by the game's internal structure.
    /// Includes metadata such as the retainer's unique ID, level, job, gil, and market status.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 0x48)]
    public struct RetainerInfo
    {
        /// <summary>
        /// Gets the unique identifier (Content ID) of the retainer.
        /// </summary>
        //0x0
        public ulong Unique;

        /// <summary>
        /// Gets the raw byte array containing the retainer's UTF-8 encoded name.
        /// </summary>
        //0x8
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
        public byte[] name_bytes;

        /// <summary>
        /// Gets a value indicating whether the retainer is enabled (1) or disabled (0).
        /// </summary>
        //0x28
        public byte enabled;

        /// <summary>
        /// Gets the current class or job of the retainer.
        /// </summary>
        //0x29
        public ClassJobType Job;

        /// <summary>
        /// Gets the current level of the retainer.
        /// </summary>
        //0x2A
        public byte Level;

        /// <summary>
        /// Gets the number of inventory slots currently occupied in the retainer's bags.
        /// </summary>
        //0x2B
        public byte FilledInventorySlots;

        /// <summary>
        /// Gets the amount of gil currently held by the retainer.
        /// </summary>
        //0x2C
        public int Gil;

        /// <summary>
        /// Gets the city or market zone where the retainer is currently registered.
        /// </summary>
        //0x30
        public RetainerCity MarketZone;

        /// <summary>
        /// Gets the number of items the retainer currently has listed on the market board.
        /// </summary>
        //0x31
        public byte NumberOfMbItems;

        //0x32
        private readonly byte unknown1;

        //0x33
        private readonly byte unknown2;

        /// <summary>
        /// Gets the Unix timestamp indicating when the retainer's market board listing will time out.
        /// </summary>
        //0x34
        public int MBTimeOutTimestamp;

        /// <summary>
        /// Gets the ID of the venture task the retainer is currently performing.
        /// </summary>
        //0x38
        public int VentureTask;

        /// <summary>
        /// Gets the Unix timestamp indicating when the retainer's current venture will be completed.
        /// </summary>
        //0x3C
        public int VentureEndTimestamp;

        //0x40
        private readonly int unknown3;

        //0x44
        private readonly int unknown4;

        /// <summary>
        /// Gets a value indicating whether the retainer is active and available for interaction.
        /// </summary>
        public bool Active => enabled == 1;

        /// <summary>
        /// Gets the name of the retainer as a UTF-8 string.
        /// </summary>
        public string Name => name_bytes.ToUTF8String();

        /// <summary>
        /// Gets the display name of the retainer.
        /// </summary>
        public string DisplayName => Name;

        /// <summary>
        /// Gets the content identifier (Content ID) of the retainer.
        /// </summary>
        public ulong ContentId => Unique;

        /// <summary>
        /// Returns a formatted string representing the retainer's current status and metadata.
        /// </summary>
        /// <returns>A string containing name, level, gil, and venture status.</returns>
        public override string ToString()
        {
            return $"{Name} ({(enabled == 1 ? "enabled" : "disabled")}) - {Job} ({Level}) Gil: {Gil} Selling: {NumberOfMbItems} Venture: {VentureTask} VentureEnd: {UnixTimeStampToDateTime(VentureEndTimestamp)} {Unique}";
        }

        /// <summary>
        /// Converts a Unix timestamp (seconds since epoch) to a <see cref="DateTime"/> object in local time.
        /// </summary>
        /// <param name="unixTimeStamp">The Unix timestamp to convert.</param>
        /// <returns>A <see cref="DateTime"/> representation of the timestamp.</returns>
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}