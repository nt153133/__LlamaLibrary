using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Buddy.Coroutines;

namespace LlamaLibrary.Helpers.Ping
{
    public static class PingChecker
    {
        private static readonly AddressGetter _addressGetter = new();
        private static ushort lastWorld;

        public static int CurrentPing { get; private set; }

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

        public static async Task UpdatePing()
        {
            var ip = _addressGetter.GetAddress();
            CurrentPing = (int)await PingTimeAverage(ip.ToString(), 3);
            lastWorld = WorldHelper.CurrentWorldId;
        }

        public static async Task<double> PingTimeAverage(string host, int echoNum)
        {
            long totalTime = 0;
            var timeout = 1000;
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
    }
}