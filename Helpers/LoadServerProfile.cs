using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Directors;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.NeoProfiles;
using ff14bot.RemoteAgents;
using ff14bot.RemoteWindows;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.ScriptConditions;
using LlamaLibrary.Structs;
using Newtonsoft.Json;

namespace LlamaLibrary.Helpers;

/// <summary>
/// Loads server-hosted profiles and cooperatively drives RebornBuddy's duty queue state machine.
/// </summary>
/// <remarks>
/// Queue operations use <see cref="Coroutine"/> waits and therefore must run from an active
/// RebornBuddy coroutine (for example, an OrderBot profile behavior). The workflow deliberately
/// resumes an existing queue instead of attempting to register again because
/// <see cref="DutyManager.CanQueue(InstanceContentResult[])"/> reports an active queue as an error.
/// </remarks>
public class LoadServerProfile
{
    /// <summary>
    /// Selects the game system used to enter a duty.
    /// </summary>
    public enum QueueType
    {
        /// <summary>Queues a synchronized live-player party through Duty Finder.</summary>
        Standard = 0,

        /// <summary>Queues the current party unrestricted through Duty Finder.</summary>
        Undersized = 1,

        /// <summary>Enters with scenario NPCs through Duty Support.</summary>
        DutySupport = 2,

        /// <summary>Enters with selected avatars through the Trust interface.</summary>
        Trust = 3
    }

    /// <summary>
    /// Describes the eligibility restrictions returned by the native Contents Finder check.
    /// The game combines these values as a bit mask, so callers must decode every set flag
    /// instead of treating the result as a sequential error number. The meanings mirror the
    /// current client's own Duty Finder validation and explanatory UI text; unknown future
    /// bits are deliberately preserved by <see cref="DescribeDutyQueueRestrictions"/>.
    /// </summary>
    [Flags]
    private enum DutyQueueRestriction : ulong
    {
        /// <summary>The selected duty has not been unlocked.</summary>
        DutyNotUnlocked = 1UL << 0,

        /// <summary>The account does not have the expansion required by the duty.</summary>
        RequiredExpansionUnavailable = 1UL << 1,

        /// <summary>The current class or job is below the duty's required level.</summary>
        ClassJobLevelTooLow = 1UL << 2,

        /// <summary>The current class or job is not eligible for the selected duty.</summary>
        ClassJobNotEligible = 1UL << 3,

        /// <summary>The equipped average item level is below the duty's requirement.</summary>
        AverageItemLevelTooLow = 1UL << 4,

        /// <summary>The duty does not support the Unrestricted Party option.</summary>
        UnrestrictedPartyUnavailable = 1UL << 5,

        /// <summary>The duty does not support the Minimum Item Level option.</summary>
        MinimumItemLevelUnavailable = 1UL << 6,

        /// <summary>The duty does not support the selected additional loot rules.</summary>
        AdditionalLootRulesUnavailable = 1UL << 7,

        /// <summary>The current party does not meet the duty's preformed-party size requirement.</summary>
        PartySizeRequirementNotMet = 1UL << 8,

        /// <summary>The duty requires a battle mentor with a current certification.</summary>
        BattleMentorCertificationRequired = 1UL << 9,

        /// <summary>The current party's jobs, roles, or composition are not eligible.</summary>
        PartyCompositionNotEligible = 1UL << 10,

        /// <summary>A Duty Finder withdrawal or abandonment penalty is active.</summary>
        DutyFinderPenaltyActive = 1UL << 11,

        /// <summary>The duty only accepts members of a registered PvP team.</summary>
        PvpTeamMembersRequired = 1UL << 12,

        /// <summary>The duty is temporarily unavailable.</summary>
        DutyTemporarilyUnavailable = 1UL << 13,

        /// <summary>The scheduled or rotating duty is not currently available.</summary>
        ScheduledDutyUnavailable = 1UL << 14,

        /// <summary>Registration must be performed from the character's Home World.</summary>
        HomeWorldRegistrationRequired = 1UL << 15,

        /// <summary>The client reported a restriction whose specific UI label is currently unknown.</summary>
        UnidentifiedGameRestriction = 1UL << 16,

        /// <summary>The current limited job is not permitted in the selected duty.</summary>
        LimitedJobUnavailable = 1UL << 17,

        /// <summary>The first duty-specific registration condition has not been met.</summary>
        DutySpecificConditionOneNotMet = 1UL << 18,

        /// <summary>The second duty-specific registration condition has not been met.</summary>
        DutySpecificConditionTwoNotMet = 1UL << 19,

        /// <summary>The third duty-specific registration condition has not been met.</summary>
        DutySpecificConditionThreeNotMet = 1UL << 20,

        /// <summary>The duty cannot be entered while New Game+ is active.</summary>
        NewGamePlusUnavailable = 1UL << 21,

        /// <summary>The duty does not support the Silence Echo option.</summary>
        SilenceEchoUnavailable = 1UL << 22,

        /// <summary>The duty does not support Explorer Mode.</summary>
        ExplorerModeUnavailable = 1UL << 23,

        /// <summary>Explorer Mode requires the duty to have been completed previously.</summary>
        ExplorerModeCompletionRequired = 1UL << 24,

        /// <summary>A duty-defined unlock criterion has not been met.</summary>
        DutyUnlockCriterionNotMet = 1UL << 25,

        /// <summary>Registration must be performed from the character's Home World physical data center.</summary>
        HomePhysicalDataCenterRequired = 1UL << 26,

        /// <summary>Registration must be performed from the data center hosting the duty.</summary>
        HostingDataCenterRequired = 1UL << 27,

        /// <summary>The game could not resolve eligibility data for the supplied duty entry.</summary>
        DutyEntryResolutionFailed = 1UL << 28
    }

    internal static readonly string NameValue = "DomesticHelper";
    private static readonly LLogger Log = new(NameValue, Colors.MediumPurple);

    // Dungeon IDs organized by expansion
    private static class DungeonIds
    {
        // Shadowbringers
        public const int HolminsterSwitch = 676;
        public const int DohnMheg = 649;
        public const int QitanaRavel = 651;
        public const int MalikahsWell = 656;
        public const int MtGulg = 659;
        public const int Amaurot = 652;
        public const int GrandCosmos = 692;
        public const int AnamnesisAnyder = 714;
        public const int HeroesGauntlet = 737;
        public const int MatoyasRelict = 746;
        public const int Paglthan = 777;

        // Endwalker
        public const int TheTowerofZot = 783;
        public const int TheTowerofBabil = 785;
        public const int Vanaspati = 789;
        public const int KtisisHyperboreia = 787;
        public const int TheAitiascope = 786;
        public const int TheDeadEnds = 792;
        public const int AlzadaasLegacy = 844;
        public const int Smileton = 794;
        public const int TheFellCourofTroia = 869;
        public const int LapisManalis = 896;
        public const int Aetherfont = 822;
        public const int LunarSubterrane = 823;
        public const int TheStigmaDreamscape = 784;

        // Dawntrail
        public const int Ihuykatumu = 826;
        public const int WorqorZormor = 824;
        public const int SkydeepCenote = 829;
        public const int Vanguard = 831;
        public const int Origenics = 825;
        public const int Alexandria = 827;
        public const int Yuweyawata = 1008;
        public const int Underkeep = 1027;
        public const int MesoTerminal = 1028;
        public const int WorqorLarDor = 832;
        public const int Everkeep = 995;
    }

    private static readonly HashSet<int> TrustDungeons = new()
    {
        DungeonIds.HolminsterSwitch,
        DungeonIds.DohnMheg,
        DungeonIds.QitanaRavel,
        DungeonIds.MalikahsWell,
        DungeonIds.MtGulg,
        DungeonIds.Amaurot,
        DungeonIds.GrandCosmos,
        DungeonIds.AnamnesisAnyder,
        DungeonIds.HeroesGauntlet,
        DungeonIds.MatoyasRelict,
        DungeonIds.Paglthan,
        DungeonIds.TheTowerofZot,
        DungeonIds.TheTowerofBabil,
        DungeonIds.Vanaspati,
        DungeonIds.KtisisHyperboreia,
        DungeonIds.TheAitiascope,
        DungeonIds.TheDeadEnds,
        DungeonIds.AlzadaasLegacy,
        DungeonIds.TheFellCourofTroia,
        DungeonIds.LapisManalis,
        DungeonIds.Aetherfont,
        DungeonIds.LunarSubterrane,
        DungeonIds.Ihuykatumu,
        DungeonIds.WorqorZormor,
        DungeonIds.SkydeepCenote,
        DungeonIds.Vanguard,
        DungeonIds.Origenics,
        DungeonIds.Alexandria,
        DungeonIds.WorqorLarDor,
        DungeonIds.Everkeep,
        DungeonIds.Yuweyawata,
        DungeonIds.Underkeep,
        DungeonIds.MesoTerminal,
    };

    private static readonly string[] Greetings =
    {
        "Hola",
        "Bonjour",
        "Hallo",
        "Ciao",
        "Konnichiwa",
        "What’s kicking, little chicken?",
        "Hello, governor!",
        "Whaddup bro?",
        "Bonjour monsieur!",
        "Ciao babydoll!",
        "Bing bing! How’s it going?",
        "Good day guys",
        "Oooo la la. This guy again",
        "Welcome to the club guys",
        "What’s sizzling?",
        "Whazzup?",
        "Ni hao ma?",
        "What’s up, buttercup?",
        "Hello!",
        "Hey",
        "Heyo",
        "Hihi",
        "Hello new friends!",
        "Hi new friends",
        "Heya",
        "Ello! o/",
        "hello!",
        "Hi, I just met you, and yes, this is crazy. Here’s my number – can we kill this guy, maybe?",
        "Hi guys",
        "What’s smokin’?",
        "How is life sailing?",
        "Hiya",
        "Hi",
        "Hey friends!",
        "Yo",
        "I come in peace. Okay, yeah maybe not.",
        "Hello, my name is Inigo Montoya.",
        "I'm Batman",
        "‘Ello, mates",
        "How you doin'?",
        "What's cookin', good lookin'?",
        "Aloha",
        "Hey you, yeah you. I like your face.",
        "Why, hello there!",
        "This fight may be recorded for training purposes.",
        "GOOOOOD MORNING, VIETNAM!",
        "‘Sup, homeslice?",
        "What’s crackin’?",
        "Here's Johnny!",
        "Whaddup",
        "o/",
        "o7",
        "Greetings and salutations!",
        "Top of the mornin’ to ya!",
        "Howdy partners.",
        "Ahoy there, matey.",
        "Anyone else have chicken too?",
        "Hey guys, glad to be here. Let's go have some fun.",
        "Oh yeah, love fighting this guy"
    };

    private static readonly ShuffleCircularQueue<string> _greetingQueue = new(Greetings);

    public static ChatBroadcaster PartyBroadcaster = new(MessageType.Party);
    public static ChatBroadcaster EmoteBroadcaster = new(MessageType.StandardEmotes);

    private const string ProfileServerUrl = "https://sts.llamamagic.net/profiles.json";
    private const int ToastDurationMs = 25000;
    private const int HttpTimeoutSeconds = 10;
    // Registration normally changes QueueState within a few frames. A bounded timeout and retry
    // protect against dropped UI actions without turning a permanent eligibility failure into a loop.
    private const int QueueRegistrationTimeout = 10000;
    private const int QueueRegistrationAttempts = 3;
    private const int QueueRegistrationRetryDelay = 1000;
    private const int TrustWindowTimeout = 8000;
    private const int DutyRecommenceDelay = 5000;
    private const int LoadContentTimeout = 1000;
    private const int DirectorInitializationTimeout = 10000;

    private static readonly Color ToastHeaderColor = Color.FromRgb(147, 112, 219);
    private static readonly Color ToastTextColor = Color.FromRgb(13, 106, 175);
    private static readonly FontFamily ToastFont = new("Gautami");

    private static readonly TimeSpan CutsceneCheckTimeout = TimeSpan.FromMilliseconds(2000);

    // GrandCompany Barracks Zone IDs
    private static readonly HashSet<uint> GcBarracksZones = new() { 534, 535, 536 };

    public static async Task LoadProfile(string profileName, QueueType queueType, bool goToBarracks, bool sayHello = false, bool sayHelloCustom = false, string sayHelloMessages = "")
    {
        Log.Information(GetLoadingMessage(profileName, queueType));

        if (DutyManager.QueueState == QueueState.InQueue)
        {
            Log.Information("Already in queue");
        }

        await GeneralFunctions.StopBusy(false);

        var profile = await FindProfileByName(profileName);
        if (profile == null)
        {
            return;
        }

        if (profile.Type == ProfileType.Quest)
        {
            await LoadQuestProfile(profileName, profile.URL);
            return;
        }

        if (profile.Type == ProfileType.Duty)
        {
            if (DutyManager.QueueState == QueueState.InDungeon)
            {
                Log.Information("Already in dungeon");
                await LoadDutyProfile(profile, null);
            }
            else
            {
                await RunDutyTask(profile, goToBarracks, sayHello, sayHelloCustom, sayHelloMessages, (int)queueType);
            }
        }
    }

    /// <summary>
    /// Overload for backward compatibility with int queue type parameter.
    /// </summary>
    public static async Task LoadProfile(string profileName, int queueType, bool goToBarracks, bool sayHello = false, bool sayHelloCustom = false, string sayHelloMessages = "")
    {
        await LoadProfile(profileName, (QueueType)queueType, goToBarracks, sayHello, sayHelloCustom, sayHelloMessages);
    }

    /// <summary>
    /// Loads a profile supplied by a plugin-owned source while retaining the standard duty queue workflow.
    /// </summary>
    public static async Task LoadProfile(string profileName, QueueType queueType, bool goToBarracks, IServerProfileSource profileSource, bool sayHello = false, bool sayHelloCustom = false, string sayHelloMessages = "")
    {
        ArgumentNullException.ThrowIfNull(profileSource);
        Log.Information(GetLoadingMessage(profileName, queueType));

        if (DutyManager.QueueState == QueueState.InQueue)
        {
            Log.Information("Already in queue");
        }

        await GeneralFunctions.StopBusy(false);

        var profile = await FindProfileByName(profileName, profileSource);
        if (profile == null)
        {
            return;
        }

        if (profile.Type == ProfileType.Quest)
        {
            await LoadMaterializedProfile(profile, profileSource);
            return;
        }

        if (profile.Type != ProfileType.Duty)
        {
            return;
        }

        if (DutyManager.QueueState == QueueState.InDungeon)
        {
            Log.Information("Already in dungeon");
            await LoadDutyProfile(profile, profileSource);
            return;
        }

        await RunDutyTask(profile, goToBarracks, sayHello, sayHelloCustom, sayHelloMessages, (int)queueType, profileSource);
    }

    /// <summary>
    /// Backward-compatible integer queue type overload for plugin-owned profile sources.
    /// </summary>
    public static Task LoadProfile(string profileName, int queueType, bool goToBarracks, IServerProfileSource profileSource, bool sayHello = false, bool sayHelloCustom = false, string sayHelloMessages = "")
        => LoadProfile(profileName, (QueueType)queueType, goToBarracks, profileSource, sayHello, sayHelloCustom, sayHelloMessages);

    public static async Task LoadProfileByZone(int zoneId)
    {
        Log.Information("Loading Profile by Zone ID");

        await GeneralFunctions.StopBusy(false);

        var profile = await FindProfileByZone(zoneId);
        if (profile?.Type == ProfileType.Duty)
        {
            await RunDutyTask(profile, goToBarracks: false, sayHello: false, sayHelloCustom: false, sayHelloMessages: "hi/welcome", (int)QueueType.Undersized);
        }
    }

    /// <summary>
    /// Loads a profile by zone through a plugin-owned profile source.
    /// </summary>
    public static async Task LoadProfileByZone(int zoneId, IServerProfileSource profileSource)
    {
        ArgumentNullException.ThrowIfNull(profileSource);
        Log.Information("Loading Profile by Zone ID");

        await GeneralFunctions.StopBusy(false);

        var profile = await FindProfileByZone(zoneId, profileSource);
        if (profile?.Type == ProfileType.Duty)
        {
            await RunDutyTask(profile, goToBarracks: false, sayHello: false, sayHelloCustom: false, sayHelloMessages: "hi/welcome", (int)QueueType.Undersized, profileSource);
        }
    }

    public static async Task LoadTrust()
    {
        if (Dawn.Instance.IsOpen)
        {
            ff14bot.Helpers.Logging.WriteDiagnostic("Closing Dawn window");
            AgentDawn.Instance.Toggle();
        }

        AgentDawn.Instance.TrustId = 27;

        if (!Dawn.Instance.IsOpen)
        {
            ff14bot.Helpers.Logging.WriteDiagnostic("Openning Dawn window");
            AgentDawn.Instance.Toggle();
            await Coroutine.Wait(8000, () => Dawn.Instance.IsOpen);
        }

        ff14bot.Helpers.Logging.WriteDiagnostic("Clicking Register");
        Dawn.Instance.Register();
    }

    private static async Task<List<ServerProfile?>> GetProfileList(string uri)
    {
        var profileUri = new Uri(uri);

        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(HttpTimeoutSeconds) };
            var responseWait = await Coroutine.ExternalTask(client.GetAsync(profileUri), HttpTimeoutSeconds * 1000);
            if (!responseWait.Completed)
            {
                Log.Error($"Timed out fetching the profile list from {profileUri.Host}.");
                return new List<ServerProfile?>();
            }

            using var response = responseWait.Result;
            if (!response.IsSuccessStatusCode)
            {
                Log.Error($"Profile server returned HTTP {(int)response.StatusCode} ({response.StatusCode}).");
                return new List<ServerProfile?>();
            }

            // Response content is another ordinary .NET task. It must also be bridged through
            // Coroutine.ExternalTask or RebornBuddy faults the active profile coroutine.
            var content = await Coroutine.ExternalTask(response.Content.ReadAsStringAsync());
            return JsonConvert.DeserializeObject<List<ServerProfile?>>(content) ?? new List<ServerProfile?>();
        }
        catch (HttpRequestException ex)
        {
            Log.Error($"Failed to fetch the profile list: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            Log.Error($"Profile list request was canceled or timed out: {ex.Message}");
        }
        catch (JsonException ex)
        {
            Log.Error($"Profile server returned invalid JSON: {ex.Message}");
        }

        return new List<ServerProfile?>();
    }

    public static string CurrentLocalizedZoneNameById(int zoneId)
    {
        ZoneNameResult? zoneNameResult;
        return (!DataManager.ZoneNameResults.TryGetValue((uint)zoneId, out zoneNameResult) ? null : zoneNameResult.CurrentLocaleName) ?? $"Unknown Zone ID: {zoneId}";
    }

    internal static async Task LoadQuestProfile(string profileName, string profileUrl)
    {
        Log.Information($"Loading quest {profileName}.");
        ConditionParser.Initialize();
        var newurl = new Uri(profileUrl);

        if (TryLoad(newurl.ToString()))
            return;

        Log.Error($"Failed to load profile from server {newurl}");

        try
        {
            var profile = NeoProfile.Load(XElement.Parse(new WebClient().DownloadString(newurl), LoadOptions.SetLineInfo));
            Log.Information($"Loaded quest {profile.Name}. But have to load it one more time");
        }
        catch (Exception ex)
        {
            Log.Error("Failed to load profile from server attempt 1");
            Log.Error(ex.ToString());
            return;
        }

        var client = new WebClient();
        var newFile = Path.GetTempFileName();
        try
        {
            client.DownloadFile(newurl, newFile);
            if (!TryLoad(newFile))
            {
                Log.Error("Failed to load profile from server attempt 2");
                return;
            }

            File.Delete(newFile);
        }
        catch (Exception ex)
        {
            Log.Error("Failed to download profile from server attempt 2");
            Log.Error(ex.ToString());
        }
    }

    internal static bool TryLoad(string profile)
    {
        NeoProfileManager.CurrentProfile.Name = "Loading Profile";
        NeoProfileManager.Load(profile, false);
        NeoProfileManager.UpdateCurrentProfileBehavior();
        return NeoProfileManager.CurrentProfile != null && NeoProfileManager.CurrentProfile.Name != "Loading Profile";
    }

    internal static Task RunDutyTask(ServerProfile profile, bool goToBarracks, bool sayHello, bool sayHelloCustom, string sayHelloMessages, int queueType)
    {
        if (!TryParseQueueType(queueType, out var parsedQueueType))
        {
            return Task.CompletedTask;
        }

        return RunDutyTask(profile, goToBarracks, sayHello, sayHelloCustom, sayHelloMessages, parsedQueueType, null);
    }

    private static Task RunDutyTask(ServerProfile profile, bool goToBarracks, bool sayHello, bool sayHelloCustom, string sayHelloMessages, int queueType, IServerProfileSource? profileSource)
    {
        if (!TryParseQueueType(queueType, out var parsedQueueType))
        {
            return Task.CompletedTask;
        }

        return RunDutyTask(profile, goToBarracks, sayHello, sayHelloCustom, sayHelloMessages, parsedQueueType, profileSource);
    }

    /// <summary>
    /// Registers for a duty when necessary, follows queue-pop transitions, and loads the matching
    /// profile only after the client has entered content.
    /// </summary>
    private static async Task RunDutyTask(ServerProfile profile, bool goToBarracks, bool sayHello, bool sayHelloCustom, string sayHelloMessages, QueueType queueType, IServerProfileSource? profileSource)
    {
        if (!TryGetInstanceContent(profile.DutyId, out var instanceContent))
        {
            return;
        }

        if (!await ValidateUnlockQuest(profile, profileSource))
        {
            return;
        }

        if (DutyManager.QueueState == QueueState.None)
        {
            await GeneralFunctions.StopBusy(false);

            if (goToBarracks && !IsInGCBarracks())
            {
                await GrandCompanyHelper.GetToGCBarracks();
            }

            if (!ValidateCanQueue(instanceContent))
            {
                return;
            }

            // The party leader can register between CanQueue and this point, so seed the result
            // from live state before deciding whether this client should issue a queue action.
            var registered = DutyManager.QueueState != QueueState.None;
            for (var attempt = 1; attempt <= QueueRegistrationAttempts && DutyManager.QueueState == QueueState.None; attempt++)
            {
                registered = await QueueForDuty(profile, instanceContent, queueType);
                if (registered)
                {
                    break;
                }

                Log.Warning($"Queue registration for {instanceContent.CurrentLocaleName} did not start (attempt {attempt}/{QueueRegistrationAttempts}).");
                if (attempt < QueueRegistrationAttempts)
                {
                    await Coroutine.Sleep(QueueRegistrationRetryDelay);
                }
            }

            if (!registered)
            {
                ShowErrorToast($"Unable to queue for {instanceContent.CurrentLocaleName} after {QueueRegistrationAttempts} attempts.");
                return;
            }
        }
        else
        {
            // CanQueue returns -1 for every active queue and cannot validate which duty is selected.
            // Preserve the caller's profile choice and resume monitoring instead of abandoning the pop.
            Log.Information($"Resuming existing duty queue in state {DutyManager.QueueState}.");
        }

        if (!await WaitForDutyPopAndCommence())
        {
            Log.Warning($"Queue for {instanceContent.CurrentLocaleName} ended before the duty loaded.");
            return;
        }

        await HandleCutscene();

        if (!await WaitForBarrierAndSayHello(profile.DutyType, sayHello, sayHelloCustom, sayHelloMessages))
        {
            return;
        }

        Log.Information($"Entered {instanceContent.CurrentLocaleName}; loading its profile.");
        await LoadDutyProfile(profile, profileSource);
    }

    private static List<uint>? _dutySupportDuties;

    public static List<uint> DutySupportDuties
    {
        get { return _dutySupportDuties ??= GetDutySupportDuties(); }
    }

    public static List<uint> GetDutySupportDuties()
    {
        var rowCount = GeneralFunctions.GetDawnContentRowCount();
        var list = new List<uint>();

        for (uint i = 0; i < rowCount; i++)
        {
            var row = GeneralFunctions.GetDawnContentRow(i);

            if (row == IntPtr.Zero)
            {
                row = GeneralFunctions.GetDawnContentRow(i + 200 - 38);
            }

            if (row == IntPtr.Zero)
            {
                continue;
            }

            var content = Core.Memory.Read<uint>(row);
            if (content != 0)
            {
                list.Add(content);
            }
        }

        return list;
    }

    #region Private Helper Methods

    private static string GetLoadingMessage(string profileName, QueueType queueType) => queueType switch
    {
        QueueType.Standard    => $"Loading {profileName} with Standard Live Party",
        QueueType.Undersized  => $"Loading {profileName} in unsynced party",
        QueueType.DutySupport => $"Loading {profileName} with Duty Support",
        QueueType.Trust       => $"Loading {profileName} with Trust",
        _                     => $"Loading {profileName}"
    };

    private static bool IsInGCBarracks() => GcBarracksZones.Contains(WorldManager.ZoneId);

    private static async Task<ServerProfile?> FindProfileByName(string profileName)
    {
        var profileList = await GetProfileList(ProfileServerUrl);
        if (profileList == null || profileList.Count == 0)
        {
            Log.Error("Profile List is null or empty");
            return null;
        }

        var profile = profileList.FirstOrDefault(p => p?.Name != null && p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
        if (profile == null)
        {
            Log.Error($"Profile {profileName} not found on server.");
            TreeRoot.Stop($"Profile {profileName} not found on server.");
        }

        return profile;
    }

    private static async Task<ServerProfile?> FindProfileByName(string profileName, IServerProfileSource profileSource)
    {
        var profileList = await profileSource.GetProfilesAsync();
        var profile = profileList.FirstOrDefault(p => p.Name != null && p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
        if (profile == null)
        {
            Log.Error($"Profile {profileName} not found in the supplied profile source.");
            TreeRoot.Stop($"Profile {profileName} not found in the supplied profile source.");
        }

        return profile;
    }

    private static async Task<ServerProfile?> FindProfileByZone(int zoneId)
    {
        var profileList = await GetProfileList(ProfileServerUrl);
        if (profileList == null || profileList.Count == 0)
        {
            Log.Error("Profile List is null or empty");
            return null;
        }

        var profile = profileList.FirstOrDefault(p => p?.ZoneId == zoneId);
        if (profile == null)
        {
            Log.Error($"Profile with ID {zoneId} not found on server.");
            TreeRoot.Stop($"Profile with ID {zoneId} not found on server.");
        }

        return profile;
    }

    private static async Task<ServerProfile?> FindProfileByZone(int zoneId, IServerProfileSource profileSource)
    {
        var profileList = await profileSource.GetProfilesAsync();
        var profile = profileList.FirstOrDefault(p => p.ZoneId == zoneId);
        if (profile == null)
        {
            Log.Error($"Profile with ID {zoneId} not found in the supplied profile source.");
            TreeRoot.Stop($"Profile with ID {zoneId} not found in the supplied profile source.");
        }

        return profile;
    }

    private static bool TryParseQueueType(int value, out QueueType queueType)
    {
        if (Enum.IsDefined(typeof(QueueType), value))
        {
            queueType = (QueueType)value;
            return true;
        }

        queueType = default;
        ShowErrorToast($"Unknown queue type {value}. Expected a value from 0 through 3.");
        return false;
    }

    private static bool TryGetInstanceContent(uint dutyId, out InstanceContentResult instanceContent)
    {
        if (DataManager.InstanceContentResults.TryGetValue(dutyId, out var result) && result != null)
        {
            instanceContent = result;
            return true;
        }

        instanceContent = null!;
        ShowErrorToast($"Instance content data not found for duty ID {dutyId}.");
        return false;
    }

    private static async Task<bool> ValidateUnlockQuest(ServerProfile profile, IServerProfileSource? profileSource)
    {
        if (profile.UnlockQuest != 0 && !QuestLogManager.IsQuestCompleted((uint)profile.UnlockQuest))
        {
            Log.Information($"Unlock quest {DataManager.GetLocalizedQuestName(profile.UnlockQuest)} is not complete. Loading profile to complete quest.");
            if (profileSource == null)
            {
                ConditionParser.Initialize();
                NeoProfileManager.Load(profile.URL, false);
                NeoProfileManager.UpdateCurrentProfileBehavior();
            }
            else
            {
                await LoadMaterializedProfile(profile, profileSource, true);
            }
            return false;
        }

        return true;
    }

    private static bool ValidateCanQueue(InstanceContentResult instanceContent)
    {
        // This check is intentionally limited to QueueState.None. RebornBuddy returns -1 for an
        // existing queue, which is a valid resumable state rather than a registration failure.
        var canQueueResult = DutyManager.CanQueue(instanceContent);
        return HandleCanQueueResult(canQueueResult, instanceContent);
    }

    private static bool HandleCanQueueResult(long result, InstanceContentResult instanceContent)
    {
        return result switch
        {
            0 => true, // Can queue
            -1 => HandleAlreadyInQueue(instanceContent),
            -2 => HandleInvalidInstanceCount(instanceContent),
            -3 => HandleMixedRouletteSoloOnly(instanceContent),
            -4 => HandleInstanceNotAvailable(instanceContent),
            -5 => HandleNullInstance(instanceContent),
            -6 => HandleQueueSubsystemUnavailable("the queue data pointer is unavailable"),
            -7 => HandleQueueSubsystemUnavailable("the Contents Finder agent is unavailable"),
            _ => HandleGameSpecificError(result, instanceContent)
        };
    }

    private static bool HandleAlreadyInQueue(InstanceContentResult instanceContent)
    {
        // Queue state may change between the caller's pre-check and CanQueue. Treat that race as
        // success so the state machine resumes monitoring instead of trying to register twice.
        Log.Information($"A duty queue became active while validating {instanceContent.CurrentLocaleName}.");
        return true;
    }

    private static bool HandleInvalidInstanceCount(InstanceContentResult instanceContent)
    {
        Log.Error("Invalid number of instances provided to CanQueue (must be 1-5)");
        ShowErrorToast("Internal error: Invalid instance count");
        return false;
    }

    private static bool HandleMixedRouletteSoloOnly(InstanceContentResult instanceContent)
    {
        Log.Error($"Cannot queue for {instanceContent.CurrentLocaleName}: Mixing roulette or solo-only duties with other duties is not allowed");
        ShowErrorToast($"{instanceContent.CurrentLocaleName} cannot be mixed with other duty types");
        return false;
    }

    private static bool HandleInstanceNotAvailable(InstanceContentResult instanceContent)
    {
        Log.Error($"Instance {instanceContent.CurrentLocaleName} is not available in the duty finder");
        ShowErrorToast($"{instanceContent.CurrentLocaleName} is not available in the duty finder");
        return false;
    }

    private static bool HandleNullInstance(InstanceContentResult instanceContent)
    {
        Log.Error("Instance content is null");
        ShowErrorToast("Instance content data is invalid");
        return false;
    }

    private static bool HandleQueueSubsystemUnavailable(string reason)
    {
        Log.Error($"Cannot inspect duty eligibility because {reason}.");
        ShowErrorToast("Duty Finder is not ready. Close its windows, wait a moment, and try again.");
        return false;
    }

    private static bool HandleGameSpecificError(long result, InstanceContentResult instanceContent)
    {
        var restrictions = DescribeDutyQueueRestrictions(unchecked((ulong)result));
        var errorMessage = $"Cannot queue for {instanceContent.CurrentLocaleName}: {restrictions}";

        // Retain the raw value in diagnostics because Square Enix can introduce new flags
        // before LlamaLibrary has a semantic label for them, while keeping the user-facing
        // toast focused on actionable explanations rather than an opaque number.
        Log.Error($"{errorMessage} (Game restriction flags: {result})");
        ShowErrorToast(errorMessage);
        return false;
    }

    /// <summary>
    /// Converts the game's combinable Contents Finder restriction flags into actionable text.
    /// </summary>
    /// <param name="rawResult">The positive bit mask returned by <see cref="DutyManager.CanQueue(InstanceContentResult[])"/>.</param>
    /// <returns>A semicolon-separated description of every known restriction plus any unknown bits.</returns>
    private static string DescribeDutyQueueRestrictions(ulong rawResult)
    {
        var reasons = new List<string>();

        AddReason(DutyQueueRestriction.DutyNotUnlocked, "the duty has not been unlocked");
        AddReason(DutyQueueRestriction.RequiredExpansionUnavailable, "the required expansion is not available on this account");
        AddReason(DutyQueueRestriction.ClassJobLevelTooLow, "the current class or job level is too low");
        AddReason(DutyQueueRestriction.ClassJobNotEligible, "the current class or job cannot enter this duty");
        AddReason(DutyQueueRestriction.AverageItemLevelTooLow, "the equipped average item level is too low");
        AddReason(DutyQueueRestriction.UnrestrictedPartyUnavailable, "Unrestricted Party is not allowed");
        AddReason(DutyQueueRestriction.MinimumItemLevelUnavailable, "Minimum Item Level is not available");
        AddReason(DutyQueueRestriction.AdditionalLootRulesUnavailable, "the selected additional loot rules are not available");
        AddReason(DutyQueueRestriction.PartySizeRequirementNotMet, "the preformed-party or party-size requirement is not met");
        AddReason(DutyQueueRestriction.BattleMentorCertificationRequired, "a battle mentor with current certification is required");
        AddReason(DutyQueueRestriction.PartyCompositionNotEligible, "the current job, role, or party composition is not eligible");
        AddReason(DutyQueueRestriction.DutyFinderPenaltyActive, "a Duty Finder penalty is active");
        AddReason(DutyQueueRestriction.PvpTeamMembersRequired, "only PvP Team members may register");
        AddReason(DutyQueueRestriction.DutyTemporarilyUnavailable, "the duty is temporarily unavailable");
        AddReason(DutyQueueRestriction.ScheduledDutyUnavailable, "the scheduled or rotating duty is not currently available");
        AddReason(DutyQueueRestriction.HomeWorldRegistrationRequired, "registration must be performed from the Home World");
        AddReason(DutyQueueRestriction.UnidentifiedGameRestriction, "the game reported an unidentified Duty Finder restriction");
        AddReason(DutyQueueRestriction.LimitedJobUnavailable, "the current limited job is not allowed");
        AddReason(DutyQueueRestriction.DutySpecificConditionOneNotMet, "a duty-specific registration condition is not met");
        AddReason(DutyQueueRestriction.DutySpecificConditionTwoNotMet, "a second duty-specific registration condition is not met");
        AddReason(DutyQueueRestriction.DutySpecificConditionThreeNotMet, "a third duty-specific registration condition is not met");
        AddReason(DutyQueueRestriction.NewGamePlusUnavailable, "the duty is not available during New Game+");
        AddReason(DutyQueueRestriction.SilenceEchoUnavailable, "the Silence Echo option is not available");
        AddReason(DutyQueueRestriction.ExplorerModeUnavailable, "Explorer Mode is not available");
        AddReason(DutyQueueRestriction.ExplorerModeCompletionRequired, "Explorer Mode requires prior duty completion");
        AddReason(DutyQueueRestriction.DutyUnlockCriterionNotMet, "a duty-specific unlock criterion is not met");
        AddReason(DutyQueueRestriction.HomePhysicalDataCenterRequired, "registration must be performed from the Home World physical data center");
        AddReason(DutyQueueRestriction.HostingDataCenterRequired, "registration must be performed from the data center hosting the duty");
        AddReason(DutyQueueRestriction.DutyEntryResolutionFailed, "the game could not resolve the supplied duty entry");

        const ulong knownFlags = (1UL << 29) - 1;
        var unknownFlags = rawResult & ~knownFlags;
        if (unknownFlags != 0)
        {
            reasons.Add($"the game reported unknown restriction flags 0x{unknownFlags:X}");
        }

        return reasons.Count > 0
            ? string.Join("; ", reasons)
            : $"the game returned an unrecognized restriction value ({rawResult})";

        void AddReason(DutyQueueRestriction restriction, string reason)
        {
            if ((rawResult & (ulong)restriction) != 0)
            {
                reasons.Add(reason);
            }
        }
    }

    private static async Task<bool> QueueForDuty(ServerProfile profile, InstanceContentResult instanceContent, QueueType queueType)
    {
        var instanceName = instanceContent.CurrentLocaleName;

        return queueType switch
        {
            QueueType.Trust                                 => await QueueForTrust(profile, instanceName),
            QueueType.DutySupport                           => await QueueForDutySupport(profile, instanceName),
            QueueType.Standard or QueueType.Undersized      => await QueueForParty(instanceContent, queueType),
            _                                               => false
        };
    }

    private static async Task<bool> QueueForTrust(ServerProfile profile, string instanceName)
    {
        if (!TrustDungeons.Contains(profile.DutyId))
        {
            ShowErrorToast($"{instanceName} is not a Trust dungeon.\nPlease select a different Queue Type or dungeon.");
            return false;
        }

        Log.Information($"Queuing for {instanceName} with Trust");

        // Handle Trust window setup
        if (Dawn.Instance.IsOpen && AgentDawn.Instance.TrustId != profile.TrustId)
        {
            AgentDawn.Instance.Toggle();
            if (!await Coroutine.Wait(TrustWindowTimeout, () => !Dawn.Instance.IsOpen))
            {
                Log.Error("Trust window failed to close before changing the selected duty.");
                return false;
            }
        }

        if (AgentDawn.Instance.TrustId != profile.TrustId)
        {
            Log.Information($"Setting Trust dungeon to {instanceName}");
            AgentDawn.Instance.TrustId = profile.TrustId;
            if (!await Coroutine.Wait(5000, () => AgentDawn.Instance.TrustId == profile.TrustId))
            {
                ShowErrorToast($"Could not set {instanceName} as Trust dungeon.");
                return false;
            }
        }

        if (!Dawn.Instance.IsOpen)
        {
            AgentDawn.Instance.Toggle();
            if (!await Coroutine.Wait(TrustWindowTimeout, () => Dawn.Instance.IsOpen))
            {
                Log.Error("Trust window failed to open");
                return false;
            }
        }

        Dawn.Instance.Register();
        return await WaitForQueueRegistration("Trust");
    }

    private static async Task<bool> QueueForDutySupport(ServerProfile profile, string instanceName)
    {
        Log.Information($"Queuing for {instanceName} with Duty Support");

        if (!DawnStory.Instance.IsOpen)
        {
            AgentDawnStory.Instance.Toggle();
        }

        if (!await Coroutine.Wait(TrustWindowTimeout, () => DawnStory.Instance.IsOpen))
        {
            Log.Error("Duty Support window failed to open.");
            return false;
        }

        if (!await DawnStory.Instance.SelectDuty(profile.DutyId))
        {
            Log.Error($"Duty Support could not select {instanceName}.");
            return false;
        }

        DawnStory.Instance.Commence();
        return await WaitForQueueRegistration("Duty Support");
    }

    private static async Task<bool> QueueForParty(InstanceContentResult instanceContent, QueueType queueType)
    {
        if (PartyManager.IsInParty && !PartyManager.IsPartyLeader)
        {
            Log.Information("Waiting for the party leader to register for the duty.");
            await Coroutine.Wait(-1, () => DutyManager.QueueState != QueueState.None);
            return DutyManager.QueueState != QueueState.None;
        }

        var isUndersized = queueType == QueueType.Undersized;
        Log.Information($"Queuing for {instanceContent.CurrentLocaleName} as {(isUndersized ? "undersized" : "normal")} group.");

        GameSettingsManager.JoinWithUndersizedParty = isUndersized;
        if (!DutyManager.Queue(instanceContent))
        {
            Log.Error($"DutyManager rejected the queue request for {instanceContent.CurrentLocaleName}.");
            return false;
        }

        return await WaitForQueueRegistration("Duty Finder");
    }

    private static async Task<bool> WaitForQueueRegistration(string queueName)
    {
        if (await Coroutine.Wait(QueueRegistrationTimeout, () => DutyManager.QueueState != QueueState.None))
        {
            Log.Information($"{queueName} registered the duty queue ({DutyManager.QueueState}).");
            return true;
        }

        Log.Error($"{queueName} did not register a duty queue within {QueueRegistrationTimeout / 1000} seconds.");
        return false;
    }

    private static async Task<bool> WaitForDutyPopAndCommence()
    {
        while (DutyManager.QueueState != QueueState.InDungeon && !CommonBehaviors.IsLoading)
        {
            switch (DutyManager.QueueState)
            {
                case QueueState.InQueue:
                    await Coroutine.Wait(-1, () => DutyManager.QueueState != QueueState.InQueue);
                    break;

                case QueueState.CommenceAvailable:
                    Log.Information("Waiting for queue pop.");
                    await Coroutine.Wait(-1, () => DutyManager.QueueState == QueueState.JoiningInstance || DutyManager.QueueState == QueueState.None);
                    break;

                case QueueState.JoiningInstance:
                    var randomDelay = Random.Shared.Next(1000, 10000);
                    Log.Information($"Dungeon popped, commencing in {randomDelay / 1000} seconds.");
                    await Coroutine.Sleep(randomDelay);

                    // A party member can withdraw during the humanized delay. Re-checking prevents
                    // a stale click against a closed confirmation window and lets the next pop proceed.
                    if (DutyManager.QueueState != QueueState.JoiningInstance)
                    {
                        break;
                    }

                    DutyManager.Commence();
                    await Coroutine.Wait(-1,
                                         () => DutyManager.QueueState == QueueState.LoadingContent ||
                                               DutyManager.QueueState == QueueState.CommenceAvailable ||
                                               DutyManager.QueueState == QueueState.None ||
                                               CommonBehaviors.IsLoading);
                    break;

                case QueueState.LoadingContent:
                    Log.Information("Waiting for everyone to accept queue.");
                    await Coroutine.Wait(-1,
                                         () => CommonBehaviors.IsLoading ||
                                               DutyManager.QueueState == QueueState.CommenceAvailable ||
                                               DutyManager.QueueState == QueueState.None);

                    if (DutyManager.QueueState == QueueState.CommenceAvailable)
                    {
                        // The client reuses CommenceAvailable after another player withdraws. Give
                        // Duty Finder time to settle before monitoring for the replacement pop.
                        await Coroutine.Sleep(DutyRecommenceDelay);
                    }

                    await Coroutine.Sleep(LoadContentTimeout);
                    break;

                case QueueState.None:
                    return false;

                default:
                    await Coroutine.Sleep(500);
                    break;
            }
        }

        return CommonBehaviors.IsLoading || DutyManager.QueueState == QueueState.InDungeon;
    }

    private static async Task HandleCutscene()
    {
        await Coroutine.Sleep(500);
        if (CommonBehaviors.IsLoading)
        {
            await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
        }

        if (!QuestLogManager.InCutscene || AgentCutScene.Instance == null)
        {
            return;
        }

        TreeRoot.StatusText = "InCutscene";
        AgentCutScene.Instance.PromptSkip();
        await Coroutine.Wait(CutsceneCheckTimeout, () => SelectString.IsOpen || SelectYesno.IsOpen);

        if (SelectString.IsOpen)
        {
            SelectString.ClickSlot(0);
        }
        else if (SelectYesno.IsOpen)
        {
            SelectYesno.Yes();
        }
    }

    private static async Task<bool> WaitForBarrierAndSayHello(DutyType dutyType, bool sayHello, bool sayHelloCustom, string sayHelloMessages)
    {
        Log.Information("Should be in duty");

        if (!await Coroutine.Wait(DirectorInitializationTimeout, () => DirectorManager.ActiveDirector is InstanceContentDirector) ||
            DirectorManager.ActiveDirector is not InstanceContentDirector director)
        {
            Log.Error("The instance content director was not initialized after zoning into the duty.");
            return false;
        }

        var barrierTime = GetBarrierTime(dutyType);
        if (director.TimeLeftInDungeon >= barrierTime.Add(TimeSpan.FromSeconds(1)))
        {
            Log.Information("Barrier up");
            await SendGreeting(sayHello, sayHelloCustom, sayHelloMessages);
            await Coroutine.Wait(-1, () => director.TimeLeftInDungeon < barrierTime);
        }

        return true;
    }

    private static async Task SendGreeting(bool sayHello, bool sayHelloCustom, string sayHelloMessages)
    {
        if (sayHello && !sayHelloCustom)
        {
            var greeting = _greetingQueue.Dequeue();
            Log.Information($"Saying '{greeting}' to the group");
            await PartyBroadcaster.Send(greeting);
        }
        else if (sayHelloCustom && sayHello)
        {
            var customGreetings = new ShuffleCircularQueue<string>(sayHelloMessages.Split('/'));
            if (customGreetings.Any)
            {
                var greeting = customGreetings.Dequeue();
                Log.Information($"Saying '{greeting}' to the group");
                await PartyBroadcaster.Send(greeting);
            }
        }
    }

    private static TimeSpan GetBarrierTime(DutyType dutyType) => dutyType switch
    {
        DutyType.Raid      => new TimeSpan(1, 59, 59),
        DutyType.Trial     => new TimeSpan(0, 59, 59),
        DutyType.Guildhest => new TimeSpan(0, 29, 59),
        _                  => new TimeSpan(1, 29, 59)
    };

    private static async Task LoadDutyProfile(ServerProfile profile, IServerProfileSource? profileSource)
    {
        if (WorldManager.ZoneId == profile.ZoneId)
        {
            Log.Information($"Loading {DataManager.InstanceContentResults[(uint)profile.DutyId].CurrentLocaleName} profile.");
            if (profileSource == null)
            {
                ConditionParser.Initialize();
                NeoProfileManager.Load(profile.URL, false);
            }
            else
            {
                await LoadMaterializedProfile(profile, profileSource);
            }
        }
        else if (profileSource != null)
        {
            Log.Information("Zone mismatch, attempting to find profile by current Zone ID");
            await LoadProfileByZoneId(profile, profileSource);
        }
        else
        {
            Log.Information("Zone mismatch, attempting to find profile by current Zone ID");
            await LoadProfileByZoneId(profile);
        }
    }

    private static async Task LoadProfileByZoneId(ServerProfile expectedProfile, IServerProfileSource profileSource)
    {
        var profileList = await profileSource.GetProfilesAsync();
        var zoneProfile = profileList.FirstOrDefault(p => p.ZoneId == WorldManager.ZoneId);
        if (zoneProfile?.Type != ProfileType.Duty)
        {
            Log.Error($"Profile with Zone ID {WorldManager.ZoneId} not found in the supplied profile source.");
            Log.Error($"Expected: {DataManager.InstanceContentResults[(uint)expectedProfile.DutyId].CurrentLocaleName} (Zone {expectedProfile.ZoneId})");
            Log.Error($"Current: {CurrentLocalizedZoneNameById(WorldManager.ZoneId)} (Zone {WorldManager.ZoneId})");
            TreeRoot.Stop($"Profile with ID {WorldManager.ZoneId} not found in the supplied profile source.");
            return;
        }

        await LoadMaterializedProfile(zoneProfile, profileSource);
    }

    private static async Task<bool> LoadMaterializedProfile(ServerProfile profile, IServerProfileSource profileSource, bool updateBehavior = false)
    {
        var profilePath = await profileSource.MaterializeProfileAsync(profile);
        if (string.IsNullOrWhiteSpace(profilePath))
        {
            ShowErrorToast($"Unable to materialize profile {profile.Name ?? profile.DutyId.ToString()}.");
            return false;
        }

        ConditionParser.Initialize();
        NeoProfileManager.Load(profilePath, false);
        if (updateBehavior)
        {
            NeoProfileManager.UpdateCurrentProfileBehavior();
        }

        return true;
    }

    private static async Task LoadProfileByZoneId(ServerProfile expectedProfile)
    {
        var profileList = await GetProfileList(ProfileServerUrl);
        if (profileList == null || profileList.Count == 0)
        {
            Log.Error("Profile List is null or empty");
            return;
        }

        var zoneProfile = profileList.FirstOrDefault(p => p?.ZoneId == WorldManager.ZoneId);
        if (zoneProfile?.Type != ProfileType.Duty)
        {
            Log.Error($"Profile with Zone ID {WorldManager.ZoneId} not found on server.");
            Log.Error($"Expected: {DataManager.InstanceContentResults[(uint)expectedProfile.DutyId].CurrentLocaleName} (Zone {expectedProfile.ZoneId})");
            Log.Error($"Current: {CurrentLocalizedZoneNameById(WorldManager.ZoneId)} (Zone {WorldManager.ZoneId})");
            TreeRoot.Stop($"Profile with ID {WorldManager.ZoneId} not found on server.");
            return;
        }

        NeoProfileManager.Load(zoneProfile.URL, false);
    }

    private static void ShowErrorToast(string message)
    {
        Core.OverlayManager.AddToast(() => message, TimeSpan.FromMilliseconds(ToastDurationMs), ToastHeaderColor, ToastTextColor, ToastFont);
        Log.Error(message);
        TreeRoot.Stop(message);
    }

    #endregion
}
