using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace LlamaLibrary.Helpers.Ping
{
    public static class PingChecker
    {
        private static AddressGetter _addressGetter = new AddressGetter();
        private static ushort lastWorld;

        public static int CurrentPing => _currentPing;

        public static async Task<int> GetCurrentPing()
        {
            if (WorldHelper.CurrentWorldId != lastWorld)
            {
                await UpdatePing();
            }

            return _currentPing;
        }

        private static int _currentPing;

        static PingChecker()
        {
            lastWorld = 0;
        }

        public static async Task UpdatePing()
        {
            var ip = _addressGetter.GetAddress();
            _currentPing = (int)await PingTimeAverage(ip.ToString(), 3);
            lastWorld = WorldHelper.CurrentWorldId;
        }

        public static async Task<double> PingTimeAverage(string host, int echoNum)
        {
            long totalTime = 0;
            int timeout = 1000;
            System.Net.NetworkInformation.Ping pingSender = new System.Net.NetworkInformation.Ping();

            for (int i = 0; i < echoNum; i++)
            {
                PingReply reply = await pingSender.SendPingAsync(host, timeout);
                if (reply.Status == IPStatus.Success)
                {
                    totalTime += reply.RoundtripTime;
                }
            }

            return totalTime / echoNum;
        }
    }
}