using System;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteWindows
{
    public class HWDLottery : RemoteWindow<HWDLottery>
    {
        internal static class Offsets
        {

            [Offset("Search E8 ? ? ? ? 32 C0 48 8B 5C 24 ? 48 8B 74 24 ? 48 83 C4 ? 5F C3 48 8B CB E8 ? ? ? ? 32 C0 TraceCall")]
            internal static IntPtr KupoFunction;
        }

        public override void Close()
        {
            SendAction(1, 3, 2);
        }

        public HWDLottery() : base("HWDLottery")
        {
        }

        public async Task ClickSpot(int slot)
        {
            //var patternFinder = new GreyMagic.PatternFinder(Core.Memory);
            //IntPtr KupoClick = patternFinder.Find("E8 ? ? ? ? 32 C0 48 8B 5C 24 ? 48 8B 74 24 ? 48 83 C4 ? 5F C3 48 8B 03 48 8B CB FF 50 ? TraceCall");

            if (IsOpen)
            {
                var agent = WindowByName?.TryFindAgentInterface();

                if (agent != null)
                {
                    Core.Memory.CallInjected64<uint>(Offsets.KupoFunction, new object[2]
                    {
                        agent.Pointer,
                        1U
                    });

                    await Coroutine.Sleep(2000);
                }
            }
        }
    }
}