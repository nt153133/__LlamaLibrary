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
/// Handles loading server profiles and managing duty queue operations.
/// </summary>
public class LoadServerProfile
{
    /// <summary>
    /// Queue type enumeration for type-safe queue selection.
    /// </summary>
    public enum QueueType
    {
        Standard = 0,
        Undersized = 1,
        DutySupport = 2,
        Trust = 3
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
    private const int QueueTimeout = 10000;
    private const int TrustWindowTimeout = 8000;
    private const int DutyRecommenceDelay = 5000;
    private const int LoadContentTimeout = 1000;

    private static readonly Color ToastHeaderColor = Color.FromRgb(147, 112, 219);
    private static readonly Color ToastTextColor = Color.FromRgb(13, 106, 175);
    private static readonly FontFamily ToastFont = new("Gautami");

    private static readonly TimeSpan BarrierCheckInterval = TimeSpan.FromMilliseconds(500);
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
                LoadProfileDirect(profile.DutyId, profile.URL);
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

        using (var client = new HttpClient { Timeout = new TimeSpan(0, 0, 10) })
        {
            var response = (await Coroutine.ExternalTask(client.GetAsync(uri), 10_000)).Result;
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ServerProfile?>>(content);
            }
        }

        Log.Error("Failed to get profile list from server");
        return new List<ServerProfile>();
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

    internal static async Task RunDutyTask(ServerProfile profile, bool goToBarracks, bool sayHello, bool sayHelloCustom, string sayHelloMessages, int queueType)
    {
        while (DutyManager.QueueState != QueueState.InDungeon)
        {
            await GeneralFunctions.StopBusy(false);

            if (!await ValidateAndPrepareForQueue(profile, queueType))
            {
                return;
            }

            while (DutyManager.QueueState == QueueState.None)
            {
                if (goToBarracks && !IsInGCBarracks())
                {
                    await GrandCompanyHelper.GetToGCBarracks();
                }

                if (DutyManager.QueueState == QueueState.None)
                {
                    if (!await QueueForDuty(profile, queueType))
                    {
                        return;
                    }
                }
            }

            if (!await WaitForDutyPopAndCommence(profile))
            {
                return;
            }

            await HandleCutscene();

            if (!await WaitForBarrierAndSayHello(profile.DutyType, sayHello, sayHelloCustom, sayHelloMessages))
            {
                return;
            }

            Log.Information("Should be ready");
        }

        await LoadDutyProfile(profile);
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

    private static void LoadProfileDirect(uint dungeonDutyId, string profileUrl)
    {
        Log.Information($"Loading {DataManager.InstanceContentResults[dungeonDutyId].CurrentLocaleName} profile.");
        ConditionParser.Initialize();
        NeoProfileManager.Load(profileUrl, false);
    }

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

    private static async Task<bool> ValidateAndPrepareForQueue(ServerProfile profile, int queueType)
    {
        // Check unlock quest
        if (profile.UnlockQuest != 0 && !QuestLogManager.IsQuestCompleted((uint)profile.UnlockQuest))
        {
            Log.Information($"Unlock quest {DataManager.GetLocalizedQuestName(profile.UnlockQuest)} is not complete. Loading profile to complete quest.");
            ConditionParser.Initialize();
            NeoProfileManager.Load(profile.URL, false);
            NeoProfileManager.UpdateCurrentProfileBehavior();
            return false;
        }

        // Use DutyManager.CanQueue for validation
        var instanceContent = DataManager.InstanceContentResults[(uint)profile.DutyId];
        if (instanceContent == null)
        {
            ShowErrorToast($"Instance content data not found for duty ID {profile.DutyId}");
            return false;
        }

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
            _ => HandleGameSpecificError(result, instanceContent)
        };
    }

    private static bool HandleAlreadyInQueue(InstanceContentResult instanceContent)
    {
        Log.Warning($"Already in queue for {instanceContent.CurrentLocaleName}");
        return false;
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

    private static bool HandleGameSpecificError(long result, InstanceContentResult instanceContent)
    {
        Log.Error($"Cannot queue for {instanceContent.CurrentLocaleName}: Game returned error code {result}");

        var errorMessage = result switch
        {
            1 => $"{instanceContent.CurrentLocaleName} requires a higher item level",
            2 => $"{instanceContent.CurrentLocaleName} requires a higher class level",
            3 => $"You are not eligible to queue for {instanceContent.CurrentLocaleName}",
            _ => $"Cannot queue for {instanceContent.CurrentLocaleName} (Error {result})"
        };

        ShowErrorToast(errorMessage);
        return false;
    }

    private static async Task<bool> QueueForDuty(ServerProfile profile, int queueType)
    {
        var instanceName = DataManager.InstanceContentResults[(uint)profile.DutyId].CurrentLocaleName;

        return queueType switch
        {
            (int)QueueType.Trust                                 => await QueueForTrust(profile, instanceName),
            (int)QueueType.DutySupport                           => await QueueForDutySupport(profile, instanceName),
            (int)QueueType.Standard or (int)QueueType.Undersized => await QueueForParty(profile, instanceName, queueType),
            _                                                    => false
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
            await Coroutine.Wait(TrustWindowTimeout, () => !Dawn.Instance.IsOpen);
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
        await Coroutine.Wait(TrustWindowTimeout, () => !Dawn.Instance.IsOpen);
        await Coroutine.Wait(QueueTimeout, () => DutyManager.QueueState == QueueState.CommenceAvailable || DutyManager.QueueState == QueueState.JoiningInstance);

        return DutyManager.QueueState != QueueState.None;
    }

    private static async Task<bool> QueueForDutySupport(ServerProfile profile, string instanceName)
    {
        Log.Information($"Queuing for {instanceName} with Duty Support");

        if (!DawnStory.Instance.IsOpen)
        {
            AgentDawnStory.Instance.Toggle();
        }

        if (await Coroutine.Wait(TrustWindowTimeout, () => DawnStory.Instance.IsOpen))
        {
            if (await DawnStory.Instance.SelectDuty(profile.DutyId))
            {
                DawnStory.Instance.Commence();
            }
        }

        await Coroutine.Wait(QueueTimeout, () => DutyManager.QueueState == QueueState.CommenceAvailable || DutyManager.QueueState == QueueState.JoiningInstance);
        return DutyManager.QueueState != QueueState.None;
    }

    private static async Task<bool> QueueForParty(ServerProfile profile, string instanceName, int queueType)
    {
        if (PartyManager.IsInParty && !PartyManager.IsPartyLeader)
        {
            Log.Information("In a party but not leader, waiting for queue...");
            await Coroutine.Wait(-1, () => DutyManager.QueueState == QueueState.CommenceAvailable || DutyManager.QueueState == QueueState.JoiningInstance);
            return true;
        }

        bool isUndersized = queueType == (int)QueueType.Undersized;
        Log.Information($"Queuing for {instanceName} as {(isUndersized ? "undersized" : "normal")} group.");

        GameSettingsManager.JoinWithUndersizedParty = isUndersized;
        DutyManager.Queue(DataManager.InstanceContentResults[(uint)profile.DutyId]);

        await Coroutine.Wait(QueueTimeout, () => DutyManager.QueueState == QueueState.CommenceAvailable || DutyManager.QueueState == QueueState.JoiningInstance);
        return DutyManager.QueueState != QueueState.None;
    }

    private static async Task<bool> WaitForDutyPopAndCommence(ServerProfile profile)
    {
        while (DutyManager.QueueState != QueueState.InDungeon && !CommonBehaviors.IsLoading)
        {
            switch (DutyManager.QueueState)
            {
                case QueueState.CommenceAvailable:
                    Log.Information("Waiting for queue pop.");
                    await Coroutine.Wait(-1, () => DutyManager.QueueState == QueueState.JoiningInstance || DutyManager.QueueState == QueueState.None);
                    break;

                case QueueState.JoiningInstance:
                    var randomDelay = new Random().Next(1000, 10000);
                    Log.Information($"Dungeon popped, commencing in {randomDelay / 1000} seconds.");
                    await Coroutine.Sleep(randomDelay);
                    DutyManager.Commence();
                    await Coroutine.Wait(-1, () => DutyManager.QueueState == QueueState.LoadingContent || DutyManager.QueueState == QueueState.CommenceAvailable);
                    break;

                case QueueState.LoadingContent:
                    Log.Information("Waiting for everyone to accept queue.");
                    await Coroutine.Wait(-1, () => CommonBehaviors.IsLoading || DutyManager.QueueState == QueueState.CommenceAvailable);
                    await Coroutine.Sleep(LoadContentTimeout);
                    break;

                case QueueState.None:
                    return false;

                default:
                    await Coroutine.Sleep(500);
                    break;
            }
        }

        return DutyManager.QueueState != QueueState.None;
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

        if (DirectorManager.ActiveDirector is not InstanceContentDirector director)
        {
            Log.Error("Director is null");
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

    private static async Task LoadDutyProfile(ServerProfile profile)
    {
        if (WorldManager.ZoneId == profile.ZoneId)
        {
            Log.Information($"Loading {DataManager.InstanceContentResults[(uint)profile.DutyId].CurrentLocaleName} profile.");
            ConditionParser.Initialize();
            NeoProfileManager.Load(profile.URL, false);
        }
        else
        {
            Log.Information("Zone mismatch, attempting to find profile by current Zone ID");
            await LoadProfileByZoneId(profile);
        }
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
