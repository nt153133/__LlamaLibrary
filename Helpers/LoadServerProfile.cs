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

    public static async Task LoadProfile(string profileName, int QueueType, bool GoToBarracks)
    {
        if (GoToBarracks && (WorldManager.ZoneId != 534 && WorldManager.ZoneId != 535 && WorldManager.ZoneId != 536))
        {
            await LlamaLibrary.Helpers.GrandCompanyHelper.GetToGCBarracks();
        }

        Log.Information("Loading Profile");

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
            return;
        }

        var profile = shortList.First();
        var profileUrl = profile.URL;
        var profileType = profile.Type;
        var dungeonDutyId = profile.DutyId;
        var dungeonZoneId = profile.ZoneId;
        var dutyType = profile.DutyType;

        if (profileType == ProfileType.Quest)
        {
            await LoadQuestProfile(profileName, profileUrl);
            return;
        }

        if (profileType == ProfileType.Duty)
        {
            await RunDutyTask(dutyType, profileUrl, dungeonDutyId, dungeonZoneId, QueueType);
            return;
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

    internal static async Task RunDutyTask(DutyType dutyType, string profileUrl, int dungeonDutyId, int dungeonZoneId, int QueueType)
    {
        await GeneralFunctions.StopBusy(false);

        while (WorldManager.ZoneId != dungeonZoneId)
        {
            while (DutyManager.QueueState == QueueState.None)
            {
                if (QueueType == 2)
                {
                    Log.Information("Queuing for " + DataManager.InstanceContentResults[(uint)dungeonDutyId].CurrentLocaleName);
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
}