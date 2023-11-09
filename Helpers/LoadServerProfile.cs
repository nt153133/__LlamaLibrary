using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.NeoProfiles;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers;
using LlamaLibrary.Helpers.NPC;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;
using Newtonsoft.Json;

namespace LlamaLibrary.Helpers;

public class LoadServerProfile
{
    internal static readonly string NameValue = "DomesticHelper";
    private static readonly LLogger Log = new(NameValue, Colors.MediumPurple);

    private static int TheTowerofZot = 783;
    private static int TheStigmaDreamscape = 784;
    private static int TheTowerofBabil = 785;
    private static int TheAitiascope = 786;
    private static int KtisisHyperboreia = 787;
    private static int Vanaspati = 789;
    private static int TheDeadEnds = 792;
    private static int Smileton = 794;
    private static int TheAetherfont = 822;
    private static int LunarSubterrane = 823;
    private static int AlzadaalsLegacy = 844;
    private static int TheFellCourofTroia = 869;
    private static int LapisManalis = 896;

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
        TheAetherfont,
        LunarSubterrane,
        AlzadaalsLegacy,
        TheFellCourofTroia,
        LapisManalis
    };

    public static async Task LoadProfile(string profileName, int QueueType, bool GoToBarracks)
    {
        Log.Information("Loading Profile");

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
        var dungeonLevel = profile.Level;
        var dutyType = profile.DutyType;
        var unlockQuest = profile.UnlockQuest;
        var reqItemLevel = profile.ItemLevel;

        if (profileType == ProfileType.Quest)
        {
            await LoadQuestProfile(profileName, profileUrl);
            return;
        }

        if (profileType == ProfileType.Duty)
        {
            {
                await RunDutyTask(dutyType, profileUrl, dungeonDutyId, dungeonZoneId, QueueType, unlockQuest, GoToBarracks, reqItemLevel, dungeonLevel);
                return;
            }
        }

        return;
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

    internal static async Task RunDutyTask(DutyType dutyType, string profileUrl, int dungeonDutyId, int dungeonZoneId, int QueueType, int UnlockQuest, bool GoToBarracks, int ItemLevel, int dungeonLevel)
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

        if (!CanQueue(dungeonDutyId, dungeonZoneId, QueueType, UnlockQuest, ItemLevel, dungeonLevel))
        {
            return;
        }

        while (WorldManager.ZoneId != dungeonZoneId)
        {
            while (DutyManager.QueueState == QueueState.None)
            {
                if (GoToBarracks && (WorldManager.ZoneId != 534 && WorldManager.ZoneId != 535 && WorldManager.ZoneId != 536))
                {
                    await LlamaLibrary.Helpers.GrandCompanyHelper.GetToGCBarracks();
                }

                if (QueueType == 2 || EndwalkerDungeons.Contains(dungeonDutyId))
                {
                    if (EndwalkerDungeons.Contains(dungeonDutyId) && QueueType != 2)
                    {
                        Log.Information($"Endwalker dungeon automatically queueing for {DataManager.InstanceContentResults[(uint)dungeonDutyId].CurrentLocaleName} with Duty Support");
                    }
                    else
                    {
                        Log.Information($"Queuing for {DataManager.InstanceContentResults[(uint)dungeonDutyId].CurrentLocaleName} with Duty Support");
                    }

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
                        Log.Information("Queued for Dungeon");
                    }
                    else if (DutyManager.QueueState == QueueState.None)
                    {
                        Log.Error("Something went wrong, queueing again...");
                    }
                }
                else
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
                            Log.Information("Queued for Dungeon");
                        }
                        else if (DutyManager.QueueState == QueueState.None)
                        {
                            Log.Error("Something went wrong, queuing again...");
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
            ConditionParser.Initialize();
            NeoProfileManager.Load(profileUrl, false);
            NeoProfileManager.UpdateCurrentProfileBehavior();
        }

        return;
    }

    internal static bool CanQueue(int dungeonDutyId, int dungeonZoneId, int QueueType, int UnlockQuest, int ItemLevel, int dungeonLevel)
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
            string message = $"You must be on a DoW or DoM class to do a duty..";
            Core.OverlayManager.AddToast(() => $"{message}", TimeSpan.FromMilliseconds(25000), System.Windows.Media.Color.FromRgb(147, 112, 219), System.Windows.Media.Color.FromRgb(13, 106, 175), new System.Windows.Media.FontFamily("Gautami"));
            Log.Error($"{message}");
            TreeRoot.Stop($"{message}");
            return false;
        }

        if (dungeonLevel != 0)
        {
            if (Core.Me.ClassLevel < dungeonLevel)
            {
                string message = $"{CurrentLocalizedZoneNameById(dungeonZoneId)} requires level {dungeonLevel}. Your level is {Core.Me.ClassLevel}. Please swap to a job that is at least level {dungeonLevel}.";
                Core.OverlayManager.AddToast(() => $"{message}", TimeSpan.FromMilliseconds(25000), System.Windows.Media.Color.FromRgb(147, 112, 219), System.Windows.Media.Color.FromRgb(13, 106, 175), new System.Windows.Media.FontFamily("Gautami"));
                Log.Error($"{message}");
                TreeRoot.Stop($"{message}");
                return false;
            }
        }

        if (ItemLevel != 0)
        {
            if (LlamaLibrary.ScriptConditions.Helpers.CurrentItemLevel() < ItemLevel)
            {
                string message = $"{CurrentLocalizedZoneNameById(dungeonZoneId)} requires minimum Item Level of {ItemLevel}. Your Item Level is {LlamaLibrary.ScriptConditions.Helpers.CurrentItemLevel()}. Please upgrade your gear.";
                Core.OverlayManager.AddToast(() => $"{message}", TimeSpan.FromMilliseconds(25000), System.Windows.Media.Color.FromRgb(147, 112, 219), System.Windows.Media.Color.FromRgb(13, 106, 175), new System.Windows.Media.FontFamily("Gautami"));
                Log.Error($"{message}.");
                TreeRoot.Stop($"Please upgrade your gear");
                return false;
            }
        }

        /*
        if (QueueType == 2 && !DutySupportDuties.Contains((uint)dungeonDutyId))
        {
            string message = $"{CurrentLocalizedZoneNameById(dungeonZoneId)} is not a Duty Support dungeon.";
            Core.OverlayManager.AddToast(() => $"{message}", TimeSpan.FromMilliseconds(25000), System.Windows.Media.Color.FromRgb(147, 112, 219), System.Windows.Media.Color.FromRgb(13, 106, 175), new System.Windows.Media.FontFamily("Gautami"));
            Log.Error($"{message}");
            TreeRoot.Stop($"{message}");
            return false;
        }
        */

        return true;
    }

    private static List<uint>? _dutySupportDuties;

    public static List<uint> DutySupportDuties
    {
        get { return _dutySupportDuties ??= GetDutySupportDuties(); }
    }

    private static List<uint> GetDutySupportDuties()
    {
        var rowCount = GeneralFunctions.GetDawnContentRowCount();

        var list = new List<uint>();

        for (uint i = 0; i < rowCount; i++)
        {
            var row = GeneralFunctions.GetDawnContentRow(i);

            if (row == IntPtr.Zero)
            {
                row = GeneralFunctions.GetDawnContentRow(i + 200 - 32);
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