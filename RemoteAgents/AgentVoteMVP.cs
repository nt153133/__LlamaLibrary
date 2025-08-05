using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.RemoteWindows;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentVoteMVP : AgentInterface<AgentVoteMVP>, IAgent
    {
        private static readonly LLogger Log = new(nameof(AgentVoteMVP), Colors.Gold);
        public IntPtr RegisteredVtable => Offsets.VTable;

        public static AtkAddonControl? NotificationWindow => RaptureAtkUnitManager.GetWindowByName("_NotificationIcMvp", true);

        private static class Offsets
        {
            [Offset("Search 48 8D 05 ? ? ? ? 48 89 03 33 C0 48 89 43 ? 48 89 43 ? 48 8B C3 48 83 C4 ? 5B C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 40 53 Add 3 TraceRelative")]
            // pre 6.3 [OffsetCN("Search 48 8D 05 ? ? ? ? 48 89 03 33 C0 48 89 43 ? 89 43 ? 48 8B C3 48 83 C4 ? 5B C3 ? ? ? ? ? ? 40 53 Add 3 TraceRelative")]
            internal static IntPtr VTable;

            //7.3
            [Offset("Search 8B 7B ? 44 3B F7 Add 2 Read8")]
            [OffsetCN("Search 8B 5E ? 44 3B F3 Add 2 Read8")]
            internal static int PlayerCount;

            //7.3
            [Offset("Search 48 03 4B ? E8 ? ? ? ? BA ? ? ? ? 48 8B CE 48 8B F8 E8 ? ? ? ? 48 89 7E ? 41 FF C6 Add 3 Read8")]
            [OffsetCN("Search 48 03 4E ? E8 ? ? ? ? 41 8D 56 ? Add 3 Read8")]
            internal static int ArrayStart;
        }

        protected AgentVoteMVP(IntPtr pointer) : base(pointer)
        {
        }

        public int PlayerCount => Core.Memory.NoCacheRead<int>(Pointer + Offsets.PlayerCount);
        public IntPtr ArrayStart => Core.Memory.NoCacheRead<IntPtr>(Pointer + Offsets.ArrayStart);

        public VoteOption[] VoteOptions => Core.Memory.ReadArray<VoteOption>(ArrayStart, PlayerCount);

        public bool CanToggle => NotificationWindow is { IsVisible: true };

        public void OpenVoteWindow()
        {
            if (VoteMvp.Instance.IsOpen)
            {
                return;
            }

            if (CanToggle)
            {
                Toggle();
            }
        }

        public async Task<bool> MakeSureVoteOpen()
        {
            if (VoteMvp.Instance.IsOpen)
            {
                return true;
            }

            if (!CanToggle)
            {
                return false;
            }

            Toggle();
            if (await Coroutine.Wait(10000, () => VoteMvp.Instance.IsOpen))
            {
                await Coroutine.Wait(10000, () => PlayerCount > 0);
            }

            return VoteMvp.Instance.IsOpen && PlayerCount > 0;
        }

        public async Task<bool> OpenAndVote(int index = 0)
        {
            if (!await MakeSureVoteOpen())
            {
                return false;
            }

            VoteMvp.Instance.Vote(Math.Min(index, PlayerCount - 1));

            return await Coroutine.Wait(10000, () => !VoteMvp.Instance.IsOpen);
        }

        public async Task<string> OpenAndVoteName(int index = 0)
        {
            if (!await MakeSureVoteOpen())
            {
                return string.Empty;
            }

            var selection = Math.Min(index, PlayerCount - 1);
            var name = Instance.VoteOptions[selection].Name;
            VoteMvp.Instance.Vote(selection);

            await Coroutine.Wait(10000, () => !VoteMvp.Instance.IsOpen);
            return name;
        }

        public async Task<string> HandleMvpVote(IEnumerable<string> possibleNames)
        {
            var enumerable = possibleNames.ToList();
            Log.Information("Handling MVP Vote with possible names: " + string.Join(", ", enumerable));
            if (Instance.CanToggle)
            {
                await Instance.MakeSureVoteOpen();

                if (Instance.PlayerCount < 1)
                {
                    Log.Error("No players in the list");
                    return string.Empty;
                }

                var options = Instance.VoteOptions;

                if (enumerable.Count == 0)
                {
                    await Instance.OpenAndVote();
                    return options[0].Name;
                }

                for (var index = 0; index < options.Length; index++)
                {
                    var option = options[index];
                    //Log.Information(option.ToString());
                    if (enumerable.Any(i => option.Name.Contains(i, StringComparison.InvariantCultureIgnoreCase)))
                    {
#if RB_CN
Log.Information($"点赞开始 {option.Name} ({index})");

#else
                        Log.Information($"Voting for {option.Name} ({index})");
#endif

                        if (await Instance.OpenAndVote(index))
                        {
#if RB_CN
Log.Information($"点赞结束");

#else
                            Log.Information("Voted");
#endif
                            return option.Name;
                        }
                    }
                }
            }

#if RB_CN
 Log.Error($@"点赞异常");

#else
            Log.Error("Could not open vote window");
#endif
            return string.Empty;
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x78)]
    public struct VoteOption
    {
        [FieldOffset(0x0)]
        public IntPtr NamePtr;

        [FieldOffset(0x10)]
        public int NameLength;

        [FieldOffset(0x70)]
        public ClassJobType Job;

        public string Name => Core.Memory.ReadString(NamePtr, Encoding.UTF8, NameLength);

        public override string ToString()
        {
            return $"Job: {Job}, Name: {Name}";
        }
    }
}