using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot.Enums;

namespace LlamaLibrary.Helpers.Ping
{
    /// <summary>
    /// Measures and caches the round-trip ping latency to the FFXIV game server.
    /// The server address is resolved automatically via <see cref="AddressGetter"/>.
    /// When the active world changes, the ping is refreshed against the new server address.
    /// </summary>
    public static class PingChecker
    {
        private static readonly AddressGetter _addressGetter = new();
        private static ushort lastWorld;

        /// <summary>
        /// Gets the most recently measured round-trip latency to the FFXIV game server in milliseconds.
        /// </summary>
        /// <value>
        /// The average ping in milliseconds as of the last call to <see cref="UpdatePing"/>.
        /// Defaults to <c>100</c> ms when the server address cannot be determined.
        /// </value>
        public static int CurrentPing { get; private set; }

        /// <summary>
        /// Returns the current ping, refreshing it first if the active world has changed since
        /// the last measurement.
        /// </summary>
        /// <returns>The current round-trip latency in milliseconds.</returns>
        public static async Task<int> GetCurrentPing()
        {
            if (WorldHelper.CurrentWorldId != lastWorld)
            {
                await UpdatePing();
            }

            return CurrentPing;
        }

        static PingChecker()
        {
            lastWorld = 0;
        }

        /// <summary>
        /// Re-measures the ping to the current FFXIV game server.
        /// If the server address cannot be resolved, a fallback address is selected from a
        /// hard-coded data-center map. Sets <see cref="CurrentPing"/> to the measured average.
        /// </summary>
        public static async Task UpdatePing()
        {
            var ip = _addressGetter.GetAddress();
            //ff14bot.Helpers.Logging.Write($"PingChecker: Updating ping");
            if (ip.Equals(IPAddress.Loopback))
            {
                if (Translator.Language == Language.Chn)
                {
                    CurrentPing = 100;
                    lastWorld = WorldHelper.CurrentWorldId;
                    return;
                }

                ip = WorldHelper.DataCenterId switch
                {
                    // I just copied these from https://is.xivup.com/adv
                    1 => IPAddress.Parse("124.150.157.23"), // Elemental
                    2 => IPAddress.Parse("124.150.157.36"), // Gaia
                    3 => IPAddress.Parse("124.150.157.49"), // Mana
                    4 => IPAddress.Parse("204.2.229.84"),   // Aether
                    5 => IPAddress.Parse("204.2.229.95"),   // Primal
                    6 => IPAddress.Parse("195.82.50.46"),   // Chaos
                    7 => IPAddress.Parse("195.82.50.55"),   // Light
                    8 => IPAddress.Parse("204.2.229.106"),  // Crystal
                    9 => IPAddress.Parse("153.254.80.75"),  // Materia

                    // If you have CN/KR DC IDs and IP addresses, feel free to PR them.
                    // World server IP address are fine too, since worlds are hosted
                    // alongside the lobby servers.

                    _ => IPAddress.Loopback,
                };
                ff14bot.Helpers.Logging.Write("PingChecker: Unable to get server address, using defaults.");
            }

            if (ip.Equals(IPAddress.Loopback))
            {
                CurrentPing = 100;
                lastWorld = WorldHelper.CurrentWorldId;
                return;
            }

            CurrentPing = (int)await PingTimeAverage(ip.ToString(), 3);
            lastWorld = WorldHelper.CurrentWorldId;
        }

        /// <summary>
        /// Sends <paramref name="echoNum"/> ICMP echo requests to <paramref name="host"/> and
        /// returns the average round-trip time in milliseconds across successful replies.
        /// </summary>
        /// <param name="host">The hostname or IP address string to ping.</param>
        /// <param name="echoNum">The number of ICMP echo requests to send.</param>
        /// <returns>
        /// The average round-trip time in milliseconds, or <c>100</c> if an exception occurs.
        /// </returns>
        public static async Task<double> PingTimeAverage(string host, int echoNum)
        {
            try
            {
                long totalTime = 0;
                var timeout = 2000;
                var pingSender = new System.Net.NetworkInformation.Ping();

                for (var i = 0; i < echoNum; i++)
                {
                    var reply = await Coroutine.ExternalTask(pingSender.SendPingAsync(host, timeout));
                    if (reply.Status == IPStatus.Success)
                    {
                        totalTime += reply.RoundtripTime;
                    }
                }

                return totalTime / echoNum;
            }
            catch (Exception e)
            {
                ff14bot.Helpers.Logging.WriteException(e);
                return 100;
            }
        }
    }
}