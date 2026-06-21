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
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    /// <summary>
    /// Remote agent for the Player MVP voting system (typically seen at the end of duties).
    /// Manages the list of eligible players and facilitates the voting process.
    /// </summary>
    public class AgentVoteMVP : AgentInterface<AgentVoteMVP>, IAgent
    {
        private static readonly LLogger Log = new(nameof(AgentVoteMVP), Colors.Gold);

        /// <inheritdoc/>
        public IntPtr RegisteredVtable => AgentVoteMVPOffsets.VTable;

        /// <summary>
        /// Gets the window control for the MVP notification popup.
        /// </summary>
        public static AtkAddonControl? NotificationWindow => RaptureAtkUnitManager.GetWindowByName("_NotificationIcMvp", true);

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentVoteMVP"/> class.
        /// </summary>
        /// <param name="pointer">The memory address of the agent.</param>
        protected AgentVoteMVP(IntPtr pointer) : base(pointer)
        {
        }

        /// <summary>
        /// Gets the number of players eligible to receive an MVP vote.
        /// </summary>
        public int PlayerCount => Core.Memory.NoCacheRead<int>(Pointer + AgentVoteMVPOffsets.PlayerCount);

        /// <summary>
        /// Gets the memory pointer to the start of the <see cref="VoteOption"/> array.
        /// </summary>
        public IntPtr ArrayStart => Core.Memory.NoCacheRead<IntPtr>(Pointer + AgentVoteMVPOffsets.ArrayStart);

        /// <summary>
        /// Gets an array of <see cref="VoteOption"/> objects representing the players available for voting.
        /// </summary>
        public VoteOption[] VoteOptions => Core.Memory.ReadArray<VoteOption>(ArrayStart, PlayerCount);

        /// <summary>
        /// Gets a value indicating whether the vote window can currently be toggled (i.e., the notification popup is visible).
        /// </summary>
        public bool CanToggle => NotificationWindow is { IsVisible: true };

        /// <summary>
        /// Attempts to open the MVP voting window if it is not already open.
        /// </summary>
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

        /// <summary>
        /// Ensures that the MVP voting window is open and contains at least one player.
        /// Toggles the window if necessary and waits for the UI to populate.
        /// </summary>
        /// <returns><see langword="true"/> if the window is open and ready; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Ensures the vote window is open and casts a vote for the player at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the player to vote for. Will be clamped to the available player count.</param>
        /// <returns><see langword="true"/> if the vote was successfully cast and the window closed; otherwise <see langword="false"/>.</returns>
        public async Task<bool> OpenAndVote(int index = 0)
        {
            if (!await MakeSureVoteOpen())
            {
                return false;
            }

            VoteMvp.Instance.Vote(Math.Min(index, PlayerCount - 1));

            return await Coroutine.Wait(10000, () => !VoteMvp.Instance.IsOpen);
        }

        /// <summary>
        /// Ensures the vote window is open, casts a vote for the player at the specified index, and returns their name.
        /// </summary>
        /// <param name="index">The zero-based index of the player to vote for.</param>
        /// <returns>The name of the player who received the vote, or an empty string if the operation failed.</returns>
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

        /// <summary>
        /// Attempts to cast an MVP vote based on a list of preferred names.
        /// Votes for the first player whose name matches (case-insensitive) any entry in <paramref name="possibleNames"/>.
        /// If no matches are found or the list is empty, votes for the first available player.
        /// </summary>
        /// <param name="possibleNames">A collection of names to prioritize for voting.</param>
        /// <returns>The name of the player who received the vote, or an empty string if no vote was cast.</returns>
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

    /// <summary>
    /// Represents a player option in the MVP voting window.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 0x78)]
    public struct VoteOption
    {
        /// <summary>
        /// The memory pointer to the player's name string.
        /// </summary>
        [FieldOffset(0x0)]
        public IntPtr NamePtr;

        /// <summary>
        /// The length of the player's name string.
        /// </summary>
        [FieldOffset(0x10)]
        public int NameLength;

        /// <summary>
        /// The player's current <see cref="ClassJobType"/>.
        /// </summary>
        [FieldOffset(0x70)]
        public ClassJobType Job;

        /// <summary>
        /// Gets the player's name by reading from <see cref="NamePtr"/>.
        /// </summary>
        public string Name => Core.Memory.ReadString(NamePtr, Encoding.UTF8, NameLength);

        /// <summary>
        /// Returns a string representation of the vote option.
        /// </summary>
        /// <returns>A string containing the job and name.</returns>
        public override string ToString()
        {
            return $"Job: {Job}, Name: {Name}";
        }
    }
}