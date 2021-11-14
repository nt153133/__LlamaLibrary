using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Pathing.Service_Navigation;
using ff14bot.RemoteWindows;
using LlamaLibrary.Enums;
using LlamaLibrary.Helpers;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;
using TreeSharp;

namespace LlamaBotBases.OutOnALimb
{
    public class OutOnALimbBase : BotBase
    {
        private static readonly LLogger Log = new LLogger("Out On A Limb", Colors.Aquamarine);

        private static readonly Random _random = new Random();
        private static readonly int BaseDelay = 500; //600
        private static readonly int MaxDelay = 800; //800

        private Composite _root;

        private static MiniGameResult hitResult = MiniGameResult.None;

        private readonly List<Vector3> playLocations = new List<Vector3>
        {
            new Vector3(36.15812f, 0.00596046f, 28.72554f),
            new Vector3(32.90435f, 0.006565332f, 26.9241f),
            new Vector3(30.45348f, 0.007183194f, 27.53075f),
            new Vector3(26.95518f, 0.007746458f, 26.11792f),
            new Vector3(25.93522f, 0.008450985f, 21.96296f),
            new Vector3(23.81929f, 0.008272052f, 20.41006f)
        };

        private int threshold;

        private static int totalMGP;
#if RB_CN
        public override string Name => "孤树无援";
#else
        public override string Name => "Out On A Limb";
#endif
        public override PulseFlags PulseFlags => PulseFlags.All;

        public override bool IsAutonomous => true;
        public override bool RequiresProfile => false;

        public override Composite Root => _root;

        public override bool WantButton { get; } = true;

        private async Task<bool> Run()
        {
            var lang = (Language)typeof(DataManager).GetFields(BindingFlags.Static | BindingFlags.NonPublic)
                .First(i => i.FieldType == typeof(Language)).GetValue(null);

            if (lang != Language.Eng && lang != Language.Chn)
            {
                TreeRoot.Stop("Only works on English and Chinese Clients for now");
            }

            await StartOutOnLimb();

            Log.Verbose("Start Done");
            await Coroutine.Wait(5000, () => AgentOutOnLimb.Instance.IsReadyBotanist);

            Log.Verbose("Ready");
            if (await PlayBotanist())
            {
                Log.Information("First win");
                do
                {
                    Log.Verbose("Loop");
                    if (!SelectYesno.IsOpen)
                    {
                        Log.Debug("Waiting on window");
                        await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
                        await Coroutine.Sleep(_random.Next(300, 500));
                    }

                    if (SelectYesno.IsOpen && GetDoubleDownReward() == 0)
                    {
                        SelectYesno.No();
                        Log.Information($"Won Nothing Reward: {GetDoubleDownReward()} total so far {totalMGP}");
                        await Coroutine.Sleep(_random.Next(300, 500));

                        //await Coroutine.Sleep(_random.Next(4000,5000));
                        break;
                    }

                    if (SelectYesno.IsOpen && (GetDoubleDownInfo().Key <= 2 || GetDoubleDownInfo().Value < 15))
                    {
                        SelectYesno.No();
                        Log.Information($"Click No Reward: {GetDoubleDownReward()}");
                        await Coroutine.Sleep(_random.Next(300, 500));
                        break;
                    }

                    if (SelectYesno.IsOpen && GetDoubleDownInfo().Key > 1 && GetDoubleDownInfo().Value > 15)
                    {
                        Log.Information($"Click Yes Reward: {GetDoubleDownReward()}");
                        await Coroutine.Sleep(_random.Next(500, 1000));
                        SelectYesno.ClickYes();
                        await Coroutine.Wait(5000, () => AgentOutOnLimb.Instance.IsReadyBotanist);

                        //await PlayBotanist();
                    }
                    else if (SelectYesno.IsOpen)
                    {
                        Log.Information($"Click No Reward: {GetDoubleDownReward()}");
                        SelectYesno.No();
                        await Coroutine.Sleep(_random.Next(300, 500));
                        break;
                    }
                }
                while (await PlayBotanist());

                await Coroutine.Wait(5000, () => GoldSaucerReward.Instance.IsOpen);

                if (GoldSaucerReward.Instance.IsOpen)
                {
                    var gained = GoldSaucerReward.Instance.MGPReward;
                    totalMGP += gained;
                    Log.Information($"Won {gained} - Total {totalMGP}");

                    if (gained == 0)
                    {
                        Log.Information($"Won {gained}");
                        TreeRoot.Stop("Won zero...issue");
                    }
                }

                Log.Information("Starting over");
            }

            if (GoldSaucerReward.Instance.IsOpen)
            {
                GoldSaucerReward.Instance.Close();
            }

            await Coroutine.Wait(5000, () => !GoldSaucerReward.Instance.IsOpen);
            Log.Information("Done");
            await Coroutine.Sleep(_random.Next(1500, 3000));

            if (totalMGP > threshold)
            {
                var _target = playLocations[_random.Next(0, playLocations.Count - 1)];
                Log.Information($"Moving to {_target}");
                Navigator.PlayerMover.MoveTowards(_target);
                while (_target.Distance2D(Core.Me.Location) >= 4)
                {
                    Navigator.PlayerMover.MoveTowards(_target);
                    await Coroutine.Sleep(100);
                }

                Navigator.PlayerMover.MoveStop();
                threshold = _random.Next(totalMGP + 3000, totalMGP + 6000);
                Log.Information($"At location {Core.Me.Location}. Set new threshold: {threshold}");
            }

            return true;
        }

        public static async Task<bool> RunHomeMGP()
        {
            var lang = (Language)typeof(DataManager).GetFields(BindingFlags.Static | BindingFlags.NonPublic)
                .First(i => i.FieldType == typeof(Language)).GetValue(null);

            if (lang != Language.Eng && lang != Language.Chn)
            {
                TreeRoot.Stop("Only works on English and Chinese Clients for now");
            }

            while (true)
            {
                await StartOutOnLimbHome();

                Log.Verbose("Start Done");
                await Coroutine.Wait(5000, () => AgentOutOnLimb.Instance.IsReadyBotanist);

                Log.Verbose("Ready");
                if (await PlayBotanist())
                {
                    Log.Information("First win");
                    do
                    {
                        Log.Verbose("Loop");
                        if (!SelectYesno.IsOpen)
                        {
                            Log.Debug("Waiting on window");
                            await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
                            await Coroutine.Sleep(_random.Next(300, 500));
                        }

                        if (SelectYesno.IsOpen && GetDoubleDownReward() == 0)
                        {
                            SelectYesno.No();
                            Log.Information($"Won Nothing Reward: {GetDoubleDownReward()} total so far {totalMGP}");
                            await Coroutine.Sleep(_random.Next(300, 500));

                            //await Coroutine.Sleep(_random.Next(4000,5000));
                            break;
                        }

                        if (SelectYesno.IsOpen && (GetDoubleDownInfo().Key <= 2 || GetDoubleDownInfo().Value < 15))
                        {
                            SelectYesno.No();
                            Log.Information($"Click No Reward: {GetDoubleDownReward()} TimeLeft: {GetDoubleDownInfo().Value}");
                            await Coroutine.Sleep(_random.Next(300, 500));
                            break;
                        }

                        if (SelectYesno.IsOpen && GetDoubleDownInfo().Key > 1 && GetDoubleDownInfo().Value > 15)
                        {
                            Log.Information($"Click Yes Reward: {GetDoubleDownReward()}");
                            await Coroutine.Sleep(_random.Next(300, 500));
                            SelectYesno.ClickYes();
                            await Coroutine.Wait(5000, () => AgentOutOnLimb.Instance.IsReadyBotanist);

                            //await PlayBotanist();
                        }
                        else if (SelectYesno.IsOpen)
                        {
                            Log.Information($"Click No Reward: {GetDoubleDownReward()} TimeLeft: {GetDoubleDownInfo().Value}");
                            SelectYesno.No();
                            await Coroutine.Sleep(_random.Next(300, 500));
                            break;
                        }
                    }
                    while (await PlayBotanist());

                    await Coroutine.Wait(5000, () => GoldSaucerReward.Instance.IsOpen);

                    if (GoldSaucerReward.Instance.IsOpen)
                    {
                        var gained = GoldSaucerReward.Instance.MGPReward;
                        totalMGP += gained;
                        Log.Information($"Won {gained} - Total {totalMGP}");

                        if (gained == 0)
                        {
                            Log.Information($"Won {gained}");

                            //TreeRoot.Stop("Won zero...issue");
                            break;
                        }
                    }

                    Log.Information("Starting over");
                }

                if (GoldSaucerReward.Instance.IsOpen)
                {
                    GoldSaucerReward.Instance.Close();
                }

                await Coroutine.Wait(5000, () => !GoldSaucerReward.Instance.IsOpen);
                Log.Information("Done");
                await Coroutine.Sleep(_random.Next(8000, 11500));
            }

            GamelogManager.MessageRecevied -= GamelogManagerOnMessageRecevied;

            //TreeRoot.Stop("Stop Requested");
            return true;
        }

        public override void Start()
        {
            // GamelogManager.MessageRecevied += GamelogManagerOnMessageRecevied;
            totalMGP = 0;
            threshold = 5000;
            _root = new ActionRunCoroutine(r => Run());
        }

        public override void Stop()
        {
            GamelogManager.MessageRecevied -= GamelogManagerOnMessageRecevied;
            _root = null;
        }

        public async Task<bool> StartOutOnLimb()
        {
            Navigator.NavigationProvider = new ServiceNavigationProvider();
            Navigator.PlayerMover = new SlideMover();

            uint npcid = 2005423; //197371

            if (GameObjectManager.GetObjectByNPCId(npcid) == null)
            {
                Log.Information("Not Near Machine");
                if (WorldManager.ZoneId != 388)
                {
                    await GetToMinionSquare();

                    var _target = new Vector3(32.61445f, 0.0005990267f, 18.66965f);
                    Navigator.PlayerMover.MoveTowards(_target);
                    while (_target.Distance2D(Core.Me.Location) >= _random.Next(1, 6))
                    {
                        Navigator.PlayerMover.MoveTowards(_target);
                        await Coroutine.Sleep(100);
                    }

                    Navigator.PlayerMover.MoveStop();

                    _target = playLocations[_random.Next(0, playLocations.Count - 1)];
                    Navigator.PlayerMover.MoveTowards(_target);
                    while (_target.Distance2D(Core.Me.Location) >= 4)
                    {
                        Navigator.PlayerMover.MoveTowards(_target);
                        await Coroutine.Sleep(100);
                    }

                    Navigator.PlayerMover.MoveStop();
                }
            }

            var station = GameObjectManager.GameObjects.Where(i => i.NpcId == 2005423).OrderBy(r => r.Distance()).First();

            if (!station.IsWithinInteractRange)
            {
                var _target = station.Location;
                Navigator.PlayerMover.MoveTowards(_target);
                while (_target.Distance2D(Core.Me.Location) >= 4)
                {
                    Navigator.PlayerMover.MoveTowards(_target);
                    await Coroutine.Sleep(100);
                }

                Navigator.PlayerMover.MoveStop();
            }

            station.Interact();

            await Coroutine.Wait(5000, () => SelectString.IsOpen);

            SelectString.ClickSlot(0);

            await Coroutine.Wait(5000, () => MiniGameAimg.Instance.IsOpen);

            await Coroutine.Sleep(1000);

            AgentOutOnLimb.Instance.Refresh();

            await Coroutine.Wait(5000, () => AgentOutOnLimb.Instance.IsReadyAimg);

            await Coroutine.Sleep(_random.Next(400, 800));

            MiniGameAimg.Instance.PressButton();

            await Coroutine.Wait(5000, () => MiniGameBotanist.Instance.IsOpen);

            return MiniGameBotanist.Instance.IsOpen;
        }

        public static async Task<bool> StartOutOnLimbHome()
        {
            Navigator.NavigationProvider = new ServiceNavigationProvider();
            Navigator.PlayerMover = new SlideMover();

            uint npcid = 197371; //197371

            if (GameObjectManager.GetObjectByNPCId(npcid) == null)
            {
                Log.Information("Not Near Machine");
                await GeneralFunctions.GoHome();
            }

            var station = GameObjectManager.GameObjects.Where(i => i.NpcId == 197371).OrderBy(r => r.Distance()).First();

            if (!station.IsWithinInteractRange)
            {
                var _target = station.Location;
                await Navigation.FlightorMove(_target);
            }

            station.Interact();

            await Coroutine.Wait(5000, () => SelectString.IsOpen);

            SelectString.ClickSlot(0);

            await Coroutine.Wait(5000, () => MiniGameAimg.Instance.IsOpen);

            await Coroutine.Sleep(1000);

            AgentOutOnLimb.Instance.Refresh();

            await Coroutine.Wait(5000, () => AgentOutOnLimb.Instance.IsReadyAimg);

            await Coroutine.Sleep(_random.Next(400, 800));

            MiniGameAimg.Instance.PressButton();

            await Coroutine.Wait(5000, () => MiniGameBotanist.Instance.IsOpen);

            return MiniGameBotanist.Instance.IsOpen;
        }

        private async Task<bool> GetToMinionSquare()
        {
            if (!WorldManager.TeleportById(62))
            {
                //Log.Error($"We can't get to {Constants.EntranceZone.CurrentLocaleAethernetName}. something is very wrong...");
                //TreeRoot.Stop();
                return false;
            }

            await Coroutine.Sleep(1000);

            await Coroutine.Wait(10000, () => !Core.Me.IsCasting);

            await Coroutine.Sleep(1000);

            if (CommonBehaviors.IsLoading)
            {
                await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
            }

            await Coroutine.Wait(10000, () => WorldManager.ZoneId == 144);

            await Coroutine.Wait(5000, () => GameObjectManager.GetObjectByNPCId(62) != null);

            var unit = GameObjectManager.GetObjectByNPCId(62);

            if (!unit.IsWithinInteractRange)
            {
                var _target = unit.Location;
                Navigator.PlayerMover.MoveTowards(_target);
                while (_target.Distance2D(Core.Me.Location) >= 4)
                {
                    Navigator.PlayerMover.MoveTowards(_target);
                    await Coroutine.Sleep(100);
                }

                Navigator.PlayerMover.MoveStop();
            }

            unit.Target();
            unit.Interact();
            await Coroutine.Wait(5000, () => SelectString.IsOpen);
#if RB_CN

            if (SelectString.IsOpen)
                SelectString.ClickLineContains("都市传送网");

            await Coroutine.Sleep(500);

            await Coroutine.Wait(5000, () => SelectString.IsOpen);
            if (SelectString.IsOpen)
                SelectString.ClickLineContains("宠物广场");

#else

            if (SelectString.IsOpen)
            {
                SelectString.ClickLineContains("Aethernet");
            }

            await Coroutine.Sleep(500);

            await Coroutine.Wait(5000, () => SelectString.IsOpen);
            if (SelectString.IsOpen)
            {
                SelectString.ClickLineContains("Minion");
            }

#endif

            await Coroutine.Sleep(1000);

            if (CommonBehaviors.IsLoading)
            {
                await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
            }

            await Coroutine.Sleep(3000);

            return true;
        }

        private static void GamelogManagerOnMessageRecevied(object sender, ChatEventArgs e)
        {
            if (e.ChatLogEntry.MessageType == MessageType.SystemMessages)
            {
#if RB_CN
                if (e.ChatLogEntry.FullLine.IndexOf("寻找目标位置", StringComparison.OrdinalIgnoreCase) >= 0)
#else
                if (e.ChatLogEntry.FullLine.IndexOf("hatchet", StringComparison.OrdinalIgnoreCase) >= 0)
#endif
                {
                    Log.Information("Ready");
                    Log.Information(e.ChatLogEntry.FullLine);
                }
            }

            //Hatchet Ready
#if RB_CN
            if (e.ChatLogEntry.MessageType == (MessageType) 2105)
            {
                if (e.ChatLogEntry.FullLine.IndexOf("手感", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Log.Verbose("Not Close");
                    HitResult = MiniGameResult.NotClose;
                    GamelogManager.MessageRecevied -= GamelogManagerOnMessageRecevied;
                }
                else if (e.ChatLogEntry.FullLine.IndexOf("什么东西", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Log.Verbose("Close");
                    HitResult = MiniGameResult.Close;
                    GamelogManager.MessageRecevied -= GamelogManagerOnMessageRecevied;
                }
                else if (e.ChatLogEntry.FullLine.IndexOf("相当接近", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Log.Verbose("Very Close");
                    HitResult = MiniGameResult.VeryClose;
                    GamelogManager.MessageRecevied -= GamelogManagerOnMessageRecevied;
                }
                else if (e.ChatLogEntry.FullLine.IndexOf("正中目标", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Log.Verbose("On Top");
                    HitResult = MiniGameResult.OnTop;
                    GamelogManager.MessageRecevied -= GamelogManagerOnMessageRecevied;
                }
            }
#else
            if (e.ChatLogEntry.MessageType == (MessageType)2105)
            {
                if (e.ChatLogEntry.FullLine.IndexOf("nothing", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Log.Verbose("Not Close");
                    hitResult = MiniGameResult.NotClose;
                    GamelogManager.MessageRecevied -= GamelogManagerOnMessageRecevied;
                }
                else if (e.ChatLogEntry.FullLine.IndexOf("something close", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Log.Verbose("Close");
                    hitResult = MiniGameResult.Close;
                    GamelogManager.MessageRecevied -= GamelogManagerOnMessageRecevied;
                }
                else if (e.ChatLogEntry.FullLine.IndexOf("very", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Log.Verbose("Very Close");
                    hitResult = MiniGameResult.VeryClose;
                    GamelogManager.MessageRecevied -= GamelogManagerOnMessageRecevied;
                }
                else if (e.ChatLogEntry.FullLine.IndexOf("right on", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Log.Verbose("On Top");
                    hitResult = MiniGameResult.OnTop;
                    GamelogManager.MessageRecevied -= GamelogManagerOnMessageRecevied;
                }
            }
#endif
        }

        private static async Task<bool> PlayBotanist()
        {
            if (!MiniGameBotanist.Instance.IsOpen)
            {
                return false;
            }

            AgentOutOnLimb.Instance.Refresh();

            await Coroutine.Wait(5000, () => AgentOutOnLimb.Instance.IsReadyBotanist);

            var lastVeryCloseLocation = -1;
            var lastCloseLocation = -1;
            var lastLocation = -1;

            Log.Verbose($"Progress {MiniGameBotanist.Instance.GetProgressLeft}");
            var stops1 = new List<int> { 20, 60, 40, 80 };
            Shuffle(stops1);
            foreach (var stopLoc in stops1)
            {
                if (MiniGameBotanist.Instance.IsOpen && MiniGameBotanist.Instance.GetNumberOfTriesLeft < 1)
                {
                    return false;
                }

                if (SelectYesno.IsOpen)
                {
                    return true;
                }

                Log.Information($"Pointer Loc: {AgentOutOnLimb.Instance.addressLocation.ToString("X")} AgentPointer: {AgentOutOnLimb.Instance.Pointer.ToString("X")}");
                var result = await StopAtLocation(_random.Next(stopLoc - 1, stopLoc + 1));
                var stop = false;
                switch (result)
                {
                    case MiniGameResult.Error:
                        Log.Error("Error");
                        return false;
                    case MiniGameResult.OnTop:
                        return true;
                    case MiniGameResult.DoubleDown:
                        return true;
                    case MiniGameResult.VeryClose:
                        stop = true;
                        lastVeryCloseLocation = stopLoc;
                        break;
                    case MiniGameResult.Close:
                        lastCloseLocation = stopLoc;
                        stop = true;
                        break;
                }

                lastLocation = stopLoc;

                Log.Verbose($"Progress {MiniGameBotanist.Instance.GetProgressLeft}");
                await Coroutine.Wait(5000, () => AgentOutOnLimb.Instance.IsReadyBotanist);
                await Coroutine.Sleep(_random.Next(BaseDelay, MaxDelay));
                if (stop)
                {
                    break;
                }
            }

            Log.Verbose("endFor");
            if (lastCloseLocation > 1 && lastVeryCloseLocation < 1)
            {
                var stops = new List<int> { lastCloseLocation - 12, lastCloseLocation + 12, lastCloseLocation - 7, lastCloseLocation + 17 };

                foreach (var stopLoc in stops)
                {
                    if (MiniGameBotanist.Instance.IsOpen && MiniGameBotanist.Instance.GetNumberOfTriesLeft < 1)
                    {
                        return false;
                    }

                    if (SelectYesno.IsOpen)
                    {
                        return true;
                    }

                    var result = await StopAtLocation(_random.Next(stopLoc - 1, stopLoc + 1));
                    var stop = false;
                    switch (result)
                    {
                        case MiniGameResult.Error:
                            Log.Error("Error");
                            return false;
                        case MiniGameResult.OnTop:
                            return true;
                        case MiniGameResult.DoubleDown:
                            return true;
                        case MiniGameResult.VeryClose:
                            stop = true;
                            lastVeryCloseLocation = stopLoc;
                            break;
                        case MiniGameResult.Close:
                            lastCloseLocation = stopLoc;
                            break;
                    }

                    lastLocation = stopLoc;

                    Log.Verbose($"Progress {MiniGameBotanist.Instance.GetProgressLeft} stop {stopLoc}");
                    await Coroutine.Wait(5000, () => AgentOutOnLimb.Instance.IsReadyBotanist);
                    await Coroutine.Sleep(_random.Next(BaseDelay, MaxDelay));
                    if (stop)
                    {
                        break;
                    }
                }
            }

            if (lastCloseLocation < 1 && lastVeryCloseLocation < 1)
            {
                var stops = new List<int> { 5, 95, 50, 10, 70, 30, 0 };
                Shuffle(stops);
                foreach (var stopLoc in stops)
                {
                    if (MiniGameBotanist.Instance.IsOpen && MiniGameBotanist.Instance.GetNumberOfTriesLeft < 1)
                    {
                        return false;
                    }

                    if (SelectYesno.IsOpen)
                    {
                        return true;
                    }

                    var result = await StopAtLocation(_random.Next(stopLoc - 1, stopLoc + 1));
                    var stop = false;
                    switch (result)
                    {
                        case MiniGameResult.Error:
                            Log.Error("Error");
                            return false;
                        case MiniGameResult.OnTop:
                            return true;
                        case MiniGameResult.DoubleDown:
                            return true;
                        case MiniGameResult.VeryClose:
                            stop = true;
                            lastVeryCloseLocation = stopLoc;
                            break;
                        case MiniGameResult.Close:
                            lastCloseLocation = stopLoc;
                            break;
                    }

                    lastLocation = stopLoc;

                    Log.Verbose($"Progress {MiniGameBotanist.Instance.GetProgressLeft}");
                    if (MiniGameBotanist.Instance.GetProgressLeft == 0)
                    {
                        return true;
                    }

                    await Coroutine.Wait(5000, () => AgentOutOnLimb.Instance.IsReadyBotanist || SelectYesno.IsOpen);
                    await Coroutine.Sleep(_random.Next(BaseDelay, MaxDelay));
                    if (stop)
                    {
                        break;
                    }
                }
            }

            Log.Verbose($"Last Location {lastLocation}");
            Log.Verbose($"Last Close Location {lastCloseLocation}");

            if (lastVeryCloseLocation > 1)
            {
                Log.Verbose($"\tVery Close set location {lastVeryCloseLocation}");
                var locations = new List<int>
                {
                    lastVeryCloseLocation - 7,
                    lastVeryCloseLocation + 7,
                    lastVeryCloseLocation,
                    lastVeryCloseLocation + 5
                };
                Shuffle(locations);
                var i = 0;
                while (MiniGameBotanist.Instance.GetProgressLeft > 0 || !SelectYesno.IsOpen || i >= (locations.Count - 1))
                {
                    //var location = _random.Next(lastVeryCloseLocation - 5, lastVeryCloseLocation + 5);
                    Log.Verbose($"Very Close location {locations[i]}");
                    var result = await StopAtLocation(locations[i]);

                    Log.Verbose($"Progress {MiniGameBotanist.Instance.GetProgressLeft} result {result}");
                    if (MiniGameBotanist.Instance.GetProgressLeft == 0)
                    {
                        return true;
                    }

                    await Coroutine.Wait(5000, () => AgentOutOnLimb.Instance.IsReadyBotanist || SelectYesno.IsOpen);

                    Log.Verbose($"IsReady {AgentOutOnLimb.Instance.IsReadyBotanist}");
                    if (!AgentOutOnLimb.Instance.IsReadyBotanist)
                    {
                        break;
                    }

                    await Coroutine.Sleep(_random.Next(BaseDelay, MaxDelay));
                    i++;
                }

                Log.Verbose($"Done very close");
            }

            if (MiniGameBotanist.Instance.GetProgressLeft == 0)
            {
                //await Coroutine.Sleep(_random.Next(100,200));
                await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
            }

            return SelectYesno.IsOpen;
        }

        private static async Task<MiniGameResult> StopAtLocation(int location)
        {
            if (!AgentOutOnLimb.Instance.IsReadyBotanist)
            {
                await Coroutine.Wait(5000, () => AgentOutOnLimb.Instance.IsReadyBotanist);
            }

            if (!MiniGameBotanist.Instance.IsOpen || !AgentOutOnLimb.Instance.IsReadyBotanist || MiniGameBotanist.Instance.GetNumberOfTriesLeft < 1)
            {
                return MiniGameResult.Error;
            }

            if (!AgentOutOnLimb.Instance.CursorLocked)
            {
                Log.Verbose("Lock Cursor");
                MiniGameBotanist.Instance.PauseCursor();
                await Coroutine.Sleep(200);
            }

            AgentOutOnLimb.Instance.CursorLocation = location;

            GamelogManager.MessageRecevied += GamelogManagerOnMessageRecevied;
            hitResult = MiniGameResult.None;
            await Coroutine.Sleep(400);
            MiniGameBotanist.Instance.PressButton();
            var timeleft = MiniGameBotanist.Instance.GetTimeLeft * 1000;
            await Coroutine.Wait(timeleft, () => hitResult != MiniGameResult.None || SelectYesno.IsOpen);

            return SelectYesno.IsOpen ? MiniGameResult.DoubleDown : hitResult;
        }

        public static KeyValuePair<int, int> GetDoubleDownInfo()
        {
#if RB_CN
            var OpportunitiesRegex = new Regex(@"还能够挑战(\d)次", RegexOptions.Compiled);

            var TimeRegex = new Regex(@"剩余时间：(\d):(\d+).*", RegexOptions.Compiled);
#else
            var OpportunitiesRegex = new Regex(@".* opportunities remaining: (\d)", RegexOptions.Compiled);

            var TimeRegex = new Regex(@"Time Remaining: (\d):(\d+).*", RegexOptions.Compiled);
#endif

            var offset0 = 458;
            var offset2 = 352;
            var count = 0;
            var sec = 0;

            var windowByName = RaptureAtkUnitManager.GetWindowByName("SelectYesno");
            if (windowByName != null)
            {
                var elementCount = Core.Memory.Read<ushort>(windowByName.Pointer + offset0);

                var addr = Core.Memory.Read<IntPtr>(windowByName.Pointer + offset2);
                var elements = Core.Memory.ReadArray<TwoInt>(addr, elementCount);

                var data = Core.Memory.ReadString((IntPtr)elements[0].Data, Encoding.UTF8);

#if RB_CN
               foreach (var line in data.Split('\n').Skip(3))
#else
                foreach (var line in data.Split('\n').Skip(5))
#endif
                {
                    if (OpportunitiesRegex.IsMatch(line))
                    {
                        count = int.Parse(OpportunitiesRegex.Match(line).Groups[1].Value.Trim());
                    }
                    else if (TimeRegex.IsMatch(line))
                    {
                        sec = int.Parse(TimeRegex.Match(line).Groups[2].Value.Trim());
                    }
                }
            }

            return new KeyValuePair<int, int>(count, sec);
        }

        public static int GetDoubleDownReward()
        {
#if RB_CN
			var RewardRegex = new Regex(@"(\d+) ⇒", RegexOptions.Compiled);
#else
            var RewardRegex = new Regex(@".*Current payout: .*[^\d](\d+)[^\d].* MGP", RegexOptions.Compiled);
#endif

            //Regex TimeRegex = new Regex(@"Time Remaining: (\d):(\d+).*", RegexOptions.Compiled);

            var offset0 = 458;
            var offset2 = 352;
            var count = 0;

            var windowByName = RaptureAtkUnitManager.GetWindowByName("SelectYesno");

            if (windowByName != null)
            {
                var elementCount = Core.Memory.Read<ushort>(windowByName.Pointer + offset0);

                var addr = Core.Memory.Read<IntPtr>(windowByName.Pointer + offset2);
                var elements = Core.Memory.ReadArray<TwoInt>(addr, elementCount);

                var data = Core.Memory.ReadString((IntPtr)elements[0].Data, Encoding.UTF8);
#if RB_CN
                foreach (var line in data.Split('\n').Skip(5))
                {
                    if (RewardRegex.IsMatch(line)) count = int.Parse(RewardRegex.Match(line).Groups[1].Value.Trim());
                }
#else
                foreach (var line in data.Split('\n').Skip(2))
                {
                    if (RewardRegex.IsMatch(line))
                    {
                        count = int.Parse(RewardRegex.Match(line).Groups[1].Value.Trim());
                    }
                }
#endif

            }

            return count;
        }


        private static readonly Random rng = new Random();

        private static void Shuffle<T>(IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}