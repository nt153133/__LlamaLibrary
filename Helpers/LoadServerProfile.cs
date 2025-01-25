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
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.NeoProfiles;
using ff14bot.RemoteWindows;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Structs;
using Newtonsoft.Json;

namespace LlamaLibrary.Helpers;

public class LoadServerProfile
{
    internal static readonly string NameValue = "DomesticHelper";
    private static readonly LLogger Log = new(NameValue, Colors.MediumPurple);

    private static int HolminsterSwitch = 676;
    private static int DohnMheg = 649;
    private static int QitanaRavel = 651;
    private static int MalikahsWell = 656;
    private static int MtGulg = 659;
    private static int Amaurot = 652;
    private static int GrandCosmos = 692;
    private static int AnamnesisAnyder = 714;
    private static int HeroesGauntlet = 737;
    private static int MatoyasRelict = 746;
    private static int Paglthan = 777;
    private static int TheTowerofZot = 783;
    private static int TheTowerofBabil = 785;
    private static int Vanaspati = 789;
    private static int KtisisHyperboreia = 787;
    private static int TheAitiascope = 786;
    private static int TheDeadEnds = 792;
    private static int AlzadaasLegacy = 844;
    private static int Smileton = 794;
    private static int TheFellCourofTroia = 869;
    private static int LapisManalis = 896;
    private static int Aetherfont = 822;
    private static int LunarSubterrane = 823;

    private static int TheStigmaDreamscape = 784;

    private static int Ihuykatumu = 826;
    private static int WorqorZormor = 824;
    private static int SkydeepCenote = 829;
    private static int Vanguard = 831;
    private static int Origenics = 825;
    private static int Alexandria = 827;
    private static int Yuweyawata = 1008;

    private static int WorqorLarDor = 832;
    private static int Everkeep = 995;

    private static List<int> EndwalkerDungeons = new()
    {
        TheTowerofZot,
        TheStigmaDreamscape,
        TheTowerofBabil,
        TheAitiascope,
        KtisisHyperboreia,
        Vanaspati,
        TheDeadEnds,
        Smileton,
        Aetherfont,
        LunarSubterrane,
        AlzadaasLegacy,
        TheFellCourofTroia,
        LapisManalis
    };

    private static List<int> DawntrailDungeons = new()
    {
        Ihuykatumu,
        WorqorZormor,
        SkydeepCenote,
        Vanguard,
        Origenics,
        Alexandria,
        WorqorLarDor,
        Everkeep,
        Yuweyawata
    };

    private static List<int> TrustDungeons = new()
    {
        HolminsterSwitch,
        DohnMheg,
        QitanaRavel,
        MalikahsWell,
        MtGulg,
        Amaurot,
        GrandCosmos,
        AnamnesisAnyder,
        HeroesGauntlet,
        MatoyasRelict,
        Paglthan,
        TheTowerofZot,
        TheTowerofBabil,
        Vanaspati,
        KtisisHyperboreia,
        TheAitiascope,
        TheDeadEnds,
        AlzadaasLegacy,
        TheFellCourofTroia,
        LapisManalis,
        Aetherfont,
        LunarSubterrane,
        Ihuykatumu,
        WorqorZormor,
        SkydeepCenote,
        Vanguard,
        Origenics,
        Alexandria,
        WorqorLarDor,
        Everkeep,
        Yuweyawata
    };

    private static readonly string[] Greetings = new string[]
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

    private static readonly ShuffleCircularQueue<string> _greetingQueue = new ShuffleCircularQueue<string>(Greetings);

    //private static ShuffleCircularQueue<string> _greetingQueueCustom;

    public static ChatBroadcaster PartyBroadcaster = new ChatBroadcaster(MessageType.Party);
    public static ChatBroadcaster EmoteBroadcaster = new ChatBroadcaster(MessageType.StandardEmotes);

    // Queue Type - 0 for standard, 1 for Undersized, 2 for Duty Support, 3 for Trust
    public static async Task LoadProfile(string profileName, int QueueType, bool GoToBarracks, bool sayHello = false, bool sayHelloCustom = false, string sayHelloMessages = "")
    {
        var loadingMessage = "";

        switch (QueueType)
        {
            case 0:
                loadingMessage = $"Loading {profileName} with Standard Live Party";
                break;
            case 1:
                loadingMessage = $"Loading {profileName} in unsynced party";
                break;
            case 2:
                loadingMessage = $"Loading {profileName} with Duty Support";
                break;
            case 3:
                loadingMessage = $"Loading {profileName} with Trust";
                break;
        }

        Log.Information(loadingMessage);

        if (DutyManager.QueueState == QueueState.InQueue)
        {
            Log.Information("Already in queue");
        }

        if (DutyManager.QueueState == QueueState.InDungeon)
        {
            Log.Information("Already in dungeon");
        }

        await GeneralFunctions.StopBusy(false);

        var profileList = await GetProfileList("https://sts.llamamagic.net/profiles.json");
        if (profileList == null)
        {
            Log.Error("Profile List is null");
            return;
        }

        if (!profileList.Any())
        {
            Log.Error("Profile List is empty");
            return;
        }

        var shortList = profileList.Where(i => i != null && i.Name != null && i.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase)).ToList();

        if (shortList == null || !shortList.Any())
        {
            Log.Error($"Profile {profileName} not found on server.");
            TreeRoot.Stop($"Profile {profileName} not found on server.");
            return;
        }

        var profile = shortList.First();
        var profileUrl = profile.URL;
        var profileType = profile.Type;
        var dungeonDutyId = profile.DutyId;
        var dungeonZoneId = profile.ZoneId;
        var dutyType = profile.DutyType;
        var unlockQuest = profile.UnlockQuest;
        var trustId = profile.TrustId;

        if (profileType == ProfileType.Quest)
        {
            await LoadQuestProfile(profileName, profileUrl);
            return;
        }

        if (profileType == ProfileType.Duty)
        {
            {
                await RunDutyTask(dutyType, profileUrl, dungeonDutyId, dungeonZoneId, QueueType, unlockQuest, GoToBarracks, sayHello, sayHelloCustom, sayHelloMessages, trustId);
                return;
            }
        }

        return;
    }

    public static async Task LoadProfileByZone(int ZoneId)
    {
        Log.Information("Loading Profile by Zone ID");

        await GeneralFunctions.StopBusy(false);

        var profileList = await GetProfileList("https://sts.llamamagic.net/profiles.json");
        if (profileList == null)
        {
            Log.Error("Profile List is null");
            return;
        }

        if (!profileList.Any())
        {
            Log.Error("Profile List is empty");
            return;
        }

        var shortList = profileList.Where(i => i != null && i.ZoneId != null && i.ZoneId == ZoneId).ToList();

        if (shortList == null || !shortList.Any())
        {
            Log.Error($"Profile with ID {ZoneId} not found on server.");
            TreeRoot.Stop($"Profile with ID {ZoneId} not found on server.");
            return;
        }

        var profile = shortList.First();
        var profileUrl = profile.URL;
        var profileType = profile.Type;
        var dungeonDutyId = profile.DutyId;
        var dungeonZoneId = profile.ZoneId;
        var dutyType = profile.DutyType;
        var unlockQuest = profile.UnlockQuest;
        var trustId = profile.TrustId;

        if (profileType == ProfileType.Duty)
        {
            {
                await RunDutyTask(dutyType, profileUrl, dungeonDutyId, dungeonZoneId, 1, unlockQuest, false, false, false, "hi/welcome", trustId);
                return;
            }
        }

        return;
    }

    public static async Task LoadTrust()
    {
        if (LlamaLibrary.RemoteWindows.Dawn.Instance.IsOpen)
        {
            ff14bot.Helpers.Logging.WriteDiagnostic("Closing Dawn window");
            LlamaLibrary.RemoteAgents.AgentDawn.Instance.Toggle();
        }

        LlamaLibrary.RemoteAgents.AgentDawn.Instance.TrustId = 27;

        if (!LlamaLibrary.RemoteWindows.Dawn.Instance.IsOpen)
        {
            ff14bot.Helpers.Logging.WriteDiagnostic("Openning Dawn window");
            LlamaLibrary.RemoteAgents.AgentDawn.Instance.Toggle();
            await Coroutine.Wait(8000, () => LlamaLibrary.RemoteWindows.Dawn.Instance.IsOpen);
        }

        ff14bot.Helpers.Logging.WriteDiagnostic("Clicking Register");
        LlamaLibrary.RemoteWindows.Dawn.Instance.Register();
    }

    private static async Task<List<ServerProfile>> GetProfileList(string uri)
    {
        var profileUri = new Uri(uri);

        using (var client = new HttpClient() { Timeout = new TimeSpan(0, 0, 10) })
        {
            var response = (await Coroutine.ExternalTask(client.GetAsync(uri), 10_000)).Result;
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ServerProfile>>(content);
            }
        }

        Log.Error("Failed to get profile list from server");
        return new List<ServerProfile>();
    }

    public static string CurrentLocalizedZoneNameById(int zoneId)
    {
        ZoneNameResult zoneNameResult;
        return !DataManager.ZoneNameResults.TryGetValue((uint)zoneId, out zoneNameResult) ? (string)null : zoneNameResult.CurrentLocaleName;
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
            return;
        }

        return;
    }

    internal static bool TryLoad(string profile)
    {
        NeoProfileManager.CurrentProfile.Name = "Loading Profile";
        NeoProfileManager.Load(profile, false);
        NeoProfileManager.UpdateCurrentProfileBehavior();
        return NeoProfileManager.CurrentProfile != null && NeoProfileManager.CurrentProfile.Name != "Loading Profile";
    }

    internal static async Task RunDutyTask(DutyType dutyType, string profileUrl, int dungeonDutyId, int dungeonZoneId, int QueueType, int UnlockQuest, bool GoToBarracks, bool sayHello, bool sayHelloCustom, string SayHelloMessages, int trustId)
    {
        if (WorldManager.ZoneId == dungeonZoneId)
        {
            ConditionParser.Initialize();
            NeoProfileManager.Load(profileUrl, false);
        }

        while (WorldManager.ZoneId != dungeonZoneId)
        {
            await GeneralFunctions.StopBusy(false);

            if (UnlockQuest != 0)
            {
                if (!QuestLogManager.IsQuestCompleted((uint)UnlockQuest))
                {
                    Log.Information($"Unlock quest {DataManager.GetLocalizedQuestName(UnlockQuest)} is not complete. Loading profile to complete quest.");
                    ConditionParser.Initialize();
                    NeoProfileManager.Load(profileUrl, false);
                    NeoProfileManager.UpdateCurrentProfileBehavior();
                    return;
                }
            }

            if (!CanQueue(dungeonDutyId, dungeonZoneId, QueueType, UnlockQuest))
            {
                return;
            }

            while (DutyManager.QueueState == QueueState.None)
            {
                if (GoToBarracks && (WorldManager.ZoneId != 534 && WorldManager.ZoneId != 535 && WorldManager.ZoneId != 536))
                {
                    await LlamaLibrary.Helpers.GrandCompanyHelper.GetToGCBarracks();
                }

                while (DutyManager.QueueState == QueueState.None)
                {
                    if (QueueType == 3)
                    {
                        if (!TrustDungeons.Contains(dungeonDutyId))
                        {
#if RB_CN
                                string message = $"{DataManager.InstanceContentResults[(uint)dungeonDutyId].CurrentLocaleName} 不是亲信副本。\\n请选择其他队列类型或副本";
#else
                            string message = $"{DataManager.InstanceContentResults[(uint)dungeonDutyId].CurrentLocaleName} is not a Trust dungeon.\nPlease select a different Queue Type or dungeon.";

#endif
                            Core.OverlayManager.AddToast(() => $"{message}", TimeSpan.FromMilliseconds(25000), System.Windows.Media.Color.FromRgb(147, 112, 219), System.Windows.Media.Color.FromRgb(13, 106, 175), new System.Windows.Media.FontFamily("Gautami"));
                            Log.Error($"{message}");
                            TreeRoot.Stop($"{message}");
                            break;
                        }

                        Log.Information($"Queuing for {DataManager.InstanceContentResults[(uint)dungeonDutyId].CurrentLocaleName} with Trust");

                        if (LlamaLibrary.RemoteWindows.Dawn.Instance.IsOpen && AgentDawn.Instance.TrustId != trustId)
                        {
                            Log.Information("Closing Trust window");
                            AgentDawn.Instance.Toggle();
                        }

                        if (AgentDawn.Instance.TrustId != trustId)
                        {
                            Log.Information($"Setting Trust dungeon to {DataManager.InstanceContentResults[(uint)dungeonDutyId].CurrentLocaleName}");
                            LlamaLibrary.RemoteAgents.AgentDawn.Instance.TrustId = trustId;
                            await Coroutine.Wait(5000, () => AgentDawn.Instance.TrustId == trustId);
                            if (AgentDawn.Instance.TrustId != trustId)
                            {
#if RB_CN
                                string message = $"无法将 {DataManager.InstanceContentResults[(uint)dungeonDutyId].CurrentLocaleName} 选择为亲信副本。";
#else
                                string message = $"Something went wrong when attempting to select {DataManager.InstanceContentResults[(uint)dungeonDutyId].CurrentLocaleName} as Trust dungeon.";

#endif
                                Core.OverlayManager.AddToast(() => $"{message}", TimeSpan.FromMilliseconds(25000), System.Windows.Media.Color.FromRgb(147, 112, 219), System.Windows.Media.Color.FromRgb(13, 106, 175), new System.Windows.Media.FontFamily("Gautami"));
                                Log.Error($"{message}");
                                TreeRoot.Stop($"{message}");
                                break;
                            }
                        }

                        if (!Dawn.Instance.IsOpen && AgentDawn.Instance.TrustId == trustId)
                        {
                            Log.Information("Opening Trust window");
                            AgentDawn.Instance.Toggle();
                            await Coroutine.Wait(8000, () => Dawn.Instance.IsOpen);
                        }

                        if (Dawn.Instance.IsOpen && AgentDawn.Instance.TrustId == trustId)
                        {
                            Log.Information("Clicking Register");
                            Dawn.Instance.Register();
                            await Coroutine.Wait(8000, () => !Dawn.Instance.IsOpen);
                        }

                        await Coroutine.Wait(10000, () => DutyManager.QueueState == QueueState.CommenceAvailable || DutyManager.QueueState == QueueState.JoiningInstance);
                        if (DutyManager.QueueState != QueueState.None)
                        {
                            Log.Information("Queued for Trust Dungeon");
                        }
                        else if (DutyManager.QueueState == QueueState.None)
                        {
                            Log.Error("Something went wrong attempting to queue for Trust, queueing again...");
                        }
                    }

                    if (QueueType == 2)
                    {
                        Log.Information($"Queuing for {DataManager.InstanceContentResults[(uint)dungeonDutyId].CurrentLocaleName} with Duty Support");

                        if (!DawnStory.Instance.IsOpen)
                        {
                            AgentDawnStory.Instance.Toggle();
                        }

                        if (await Coroutine.Wait(8000, () => DawnStory.Instance.IsOpen))
                        {
                            if (await DawnStory.Instance.SelectDuty(dungeonDutyId))
                            {
                                DawnStory.Instance.Commence();
                            }
                        }

                        await Coroutine.Wait(10000, () => DutyManager.QueueState == QueueState.CommenceAvailable || DutyManager.QueueState == QueueState.JoiningInstance);
                        if (DutyManager.QueueState != QueueState.None)
                        {
                            Log.Information("Queued for Duty Support Dungeon");
                        }
                        else if (DutyManager.QueueState == QueueState.None)
                        {
                            Log.Error("Something went wrong attempting to queue for Duty Support, queueing again...");
                        }
                    }

                    if (QueueType == 0 || QueueType == 1)
                    {
                        if (!PartyManager.IsInParty || (PartyManager.IsInParty && PartyManager.IsPartyLeader))
                        {
                            if (QueueType == 0)
                            {
                                Log.Information($"Queuing for {DataManager.InstanceContentResults[(uint)dungeonDutyId].CurrentLocaleName} as normal group.");
                                GameSettingsManager.JoinWithUndersizedParty = false;
                            }

                            if (QueueType == 1)
                            {
                                Log.Information($"Queuing for {DataManager.InstanceContentResults[(uint)dungeonDutyId].CurrentLocaleName} as undersized party.");
                                GameSettingsManager.JoinWithUndersizedParty = true;
                            }

                            DutyManager.Queue(DataManager.InstanceContentResults[(uint)dungeonDutyId]);
                            await Coroutine.Wait(10000, () => DutyManager.QueueState == QueueState.CommenceAvailable || DutyManager.QueueState == QueueState.JoiningInstance);

                            if (DutyManager.QueueState != QueueState.None)
                            {
                                Log.Information("Queued for regular Dungeon");
                            }
                            else if (DutyManager.QueueState == QueueState.None)
                            {
                                Log.Error("Something went wrong attempting to queue regular dungeon, queuing again...");
                            }
                        }
                        else
                        {
                            Log.Information("In a party, waiting for dungeon queue.");
                            await Coroutine.Wait(-1, () => DutyManager.QueueState == QueueState.CommenceAvailable || DutyManager.QueueState == QueueState.JoiningInstance);
                            Log.Information("Queued for Dungeon");
                        }
                    }
                }
            }

            while (DutyManager.QueueState != QueueState.None || DutyManager.QueueState != QueueState.InDungeon || CommonBehaviors.IsLoading)
            {
                if (DutyManager.QueueState == QueueState.CommenceAvailable)
                {
                    Log.Information("Waiting for queue pop.");
                    await Coroutine.Wait(-1,
                                         () => DutyManager.QueueState == QueueState.JoiningInstance ||
                                               DutyManager.QueueState == QueueState.None);
                }

                if (DutyManager.QueueState == QueueState.JoiningInstance)
                {
                    var rnd = new Random();
                    var waitTime = rnd.Next(1000, 10000);

                    Log.Information($"Dungeon popped, commencing in {waitTime / 1000} seconds.");
                    await Coroutine.Sleep(waitTime);
                    DutyManager.Commence();
                    await Coroutine.Wait(-1,
                                         () => DutyManager.QueueState == QueueState.LoadingContent ||
                                               DutyManager.QueueState == QueueState.CommenceAvailable);
                }

                if (DutyManager.QueueState == QueueState.LoadingContent)
                {
                    Log.Information("Waiting for everyone to accept queue.");
                    await Coroutine.Wait(-1, () => CommonBehaviors.IsLoading || DutyManager.QueueState == QueueState.CommenceAvailable);
                    await Coroutine.Sleep(1000);
                }

                if (CommonBehaviors.IsLoading)
                {
                    break;
                }

                await Coroutine.Sleep(500);
            }

            if (DutyManager.QueueState == QueueState.None)
            {
                return;
            }

            await Coroutine.Sleep(500);
            if (CommonBehaviors.IsLoading)
            {
                await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
            }

            if (QuestLogManager.InCutscene)
            {
                TreeRoot.StatusText = "InCutscene";
                if (ff14bot.RemoteAgents.AgentCutScene.Instance != null)
                {
                    ff14bot.RemoteAgents.AgentCutScene.Instance.PromptSkip();
                    await Coroutine.Wait(2000, () => SelectString.IsOpen || SelectYesno.IsOpen);

                    if (SelectString.IsOpen)
                    {
                        SelectString.ClickSlot(0);
                    }

                    if (SelectYesno.IsOpen)
                    {
                        SelectYesno.Yes();
                    }
                }
            }

            Log.Information("Should be in duty");

            if (DirectorManager.ActiveDirector is ff14bot.Directors.InstanceContentDirector director)
            {
                var time = new TimeSpan(1, 29, 59);
                if (dutyType == DutyType.Raid)
                {
                    time = new TimeSpan(1, 59, 59);
                }

                if (dutyType == DutyType.Trial)
                {
                    time = new TimeSpan(0, 59, 59);
                }

                if (dutyType == DutyType.Guildhest)
                {
                    time = new TimeSpan(0, 29, 59);
                }

                if (director.TimeLeftInDungeon >= time.Add(new TimeSpan(0, 0, 1)))
                {
                    Log.Information("Barrier up");
                    if (sayHello && !sayHelloCustom)
                    {
                        var sentgreeting = _greetingQueue.Dequeue();
                        Log.Information($"Saying '{sentgreeting}' the group");
                        await PartyBroadcaster.Send(sentgreeting);
                    }

                    if (sayHelloCustom && sayHello)
                    {
                        ShuffleCircularQueue<string> greetingQueueCustomNew = new ShuffleCircularQueue<string>(SayHelloMessages.Split('/'));
                        if (greetingQueueCustomNew.Any)
                        {
                            var sentcustomgreeting = greetingQueueCustomNew.Dequeue();
                            Log.Information($"Saying '{sentcustomgreeting}' the group");
                            await PartyBroadcaster.Send(sentcustomgreeting);
                        }
                    }

                    await Coroutine.Wait(-1, () => director.TimeLeftInDungeon < time);
                }
            }
            else
            {
                Log.Error("Director is null");
            }

            Log.Information("Should be ready");
        }

        if (WorldManager.ZoneId == dungeonZoneId)
        {
            Log.Information($"Loading {DataManager.InstanceContentResults[(uint)dungeonDutyId].CurrentLocaleName} profile.");
            ConditionParser.Initialize();
            NeoProfileManager.Load(profileUrl, false);
        }
        else
        {
            Log.Error($"Something went wrong, we're in a duty but the Zone Id isn't the expected ID.");
            TreeRoot.Stop("Something went wrong, we're in a duty but the Zone Id isn't the expected ID");
            return;
        }

        return;
    }

    internal static bool CanQueue(int dungeonDutyId, int dungeonZoneId, int QueueType, int UnlockQuest)
    {
        /*
        if (!LlamaLibrary.Helpers.GeneralFunctions.IsDutyUnlocked((uint)dungeonDutyId))
        {
            string message = $"{CurrentLocalizedZoneNameById(dungeonZoneId)} is not unlocked. Have you done the unlock quest?";
            Core.OverlayManager.AddToast(() => $"{message}", TimeSpan.FromMilliseconds(25000), System.Windows.Media.Color.FromRgb(147, 112, 219), System.Windows.Media.Color.FromRgb(13, 106, 175), new System.Windows.Media.FontFamily("Gautami"));
            Log.Error($"{message}");
            TreeRoot.Stop($"{message}");
            return false;
        }
        */

        if (!LlamaLibrary.ScriptConditions.Extras.IsDiscipleofWarClass() && !LlamaLibrary.ScriptConditions.Extras.IsDiscipleofMagicClass())
        {
#if RB_CN
            string message = $"执行任务需要您使用战斗职业 (DoW) 或魔法职业 (DoM)";
#else
            string message = $"You must be on a DoW or DoM class to do a duty..";

#endif
            Core.OverlayManager.AddToast(() => $"{message}", TimeSpan.FromMilliseconds(25000), System.Windows.Media.Color.FromRgb(147, 112, 219), System.Windows.Media.Color.FromRgb(13, 106, 175), new System.Windows.Media.FontFamily("Gautami"));
            Log.Error($"{message}");
            TreeRoot.Stop($"{message}");
            return false;
        }

        if (DataManager.InstanceContentResults[(uint)dungeonDutyId].RequiredClassJobLevel != 0)
        {
            if (Core.Me.ClassLevel < DataManager.InstanceContentResults[(uint)dungeonDutyId].RequiredClassJobLevel)
            {
#if RB_CN
                string message = $"{DataManager.InstanceContentResults[(uint)dungeonDutyId].CurrentLocaleName} 需要 {DataManager.InstanceContentResults[(uint)dungeonDutyId].RequiredClassJobLevel} 级。您的等级为 {Core.Me.ClassLevel} 级。请切换到至少 {DataManager.InstanceContentResults[(uint)dungeonDutyId].RequiredClassJobLevel} 级的职业.";
#else
                string message = $"{DataManager.InstanceContentResults[(uint)dungeonDutyId].CurrentLocaleName} requires level {DataManager.InstanceContentResults[(uint)dungeonDutyId].RequiredClassJobLevel}. Your level is {Core.Me.ClassLevel}. Please swap to a job that is at least level {DataManager.InstanceContentResults[(uint)dungeonDutyId].RequiredClassJobLevel}.";

#endif

                Core.OverlayManager.AddToast(() => $"{message}", TimeSpan.FromMilliseconds(25000), System.Windows.Media.Color.FromRgb(147, 112, 219), System.Windows.Media.Color.FromRgb(13, 106, 175), new System.Windows.Media.FontFamily("Gautami"));
                Log.Error($"{message}");
                TreeRoot.Stop($"{message}");
                return false;
            }
        }

        if (DataManager.InstanceContentResults[(uint)dungeonDutyId].RequiredItemLevel != 0)
        {
            if (LlamaLibrary.ScriptConditions.Helpers.CurrentItemLevel() < DataManager.InstanceContentResults[(uint)dungeonDutyId].RequiredItemLevel)
            {
#if RB_CN
                string message = $"{DataManager.InstanceContentResults[(uint)dungeonDutyId].CurrentLocaleName} 需要最低物品等级 {DataManager.InstanceContentResults[(uint)dungeonDutyId].RequiredItemLevel}。您的装备等级为 {LlamaLibrary.ScriptConditions.Helpers.CurrentItemLevel()}。请升级您的装备品级。";
#else
                string message = $"{DataManager.InstanceContentResults[(uint)dungeonDutyId].CurrentLocaleName} requires minimum Item Level of {DataManager.InstanceContentResults[(uint)dungeonDutyId].RequiredItemLevel}. Your Item Level is {LlamaLibrary.ScriptConditions.Helpers.CurrentItemLevel()}. Please upgrade your gear.";
#endif
                Core.OverlayManager.AddToast(() => $"{message}", TimeSpan.FromMilliseconds(25000), System.Windows.Media.Color.FromRgb(147, 112, 219), System.Windows.Media.Color.FromRgb(13, 106, 175), new System.Windows.Media.FontFamily("Gautami"));
                Log.Error($"{message}.");
                TreeRoot.Stop($"Please upgrade your gear");
                return false;
            }
        }

        if (QueueType == 2 && !DutySupportDuties.Contains((uint)dungeonDutyId))
        {
#if RB_CN
            string message = $"{DataManager.InstanceContentResults[(uint)dungeonDutyId].CurrentLocaleName} 不是亲信支持副本";
#else
            string message = $"{DataManager.InstanceContentResults[(uint)dungeonDutyId].CurrentLocaleName} is not a Duty Support dungeon.";
#endif
            Core.OverlayManager.AddToast(() => $"{message}", TimeSpan.FromMilliseconds(25000), System.Windows.Media.Color.FromRgb(147, 112, 219), System.Windows.Media.Color.FromRgb(13, 106, 175), new System.Windows.Media.FontFamily("Gautami"));
            Log.Error($"{message}");
            TreeRoot.Stop($"{message}");
            return false;
        }

        return true;
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
#if RB_DT
                row = GeneralFunctions.GetDawnContentRow(i + 200 - 33);
#else
                row = GeneralFunctions.GetDawnContentRow(i + 200 - 24);
#endif
            }

            if (row == IntPtr.Zero)
            {
                continue;
            }

            var content = Core.Memory.Read<uint>(row);

            if (content == 0)
            {
                continue;
            }

            list.Add(content);
        }

        return list;
    }
}