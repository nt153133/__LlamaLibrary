using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers;
using LlamaLibrary.Helpers.NPC;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Retainers;
using Newtonsoft.Json;

namespace LlamaLibrary.Utilities
{
    public static class RetainerHire
    {
        private static readonly LLogger Log = new(nameof(RetainerHire), Colors.MediumSeaGreen);

        private static readonly Queue<string> Names = new();

        private static class Offsets
        {
            [Offset("Search 0F B6 15 ? ? ? ? EB ? Add 3 TraceRelative")]
            internal static IntPtr MaxRetainers;
        }

        public static List<Npc> Vocates = new()
        {
            //Frydwyb (Retainer Vocate)
            //Limsa Lominsa Lower Decks (Limsa Lominsa Aetheryte Plaza)
            new Npc(1003275, 129, new Vector3(-146.0263f, 18.21201f, 17.21003f)),

            //Parnell (Retainer Vocate)
            //Old Gridania (Leatherworkers' Guild & Shaded Bower)
            new Npc(1000233, 133, new Vector3(166.5145f, 15.5f, -95.87581f)),

            //Chachabi (Retainer Vocate)
            //Ul'dah - Steps of Thal (Sapphire Avenue Exchange)
            new Npc(1001963, 131, new Vector3(107.7402f, 4.19947f, -73.10289f)),

            //Prunilla (Retainer Vocate)
            //The Pillars (The Jeweled Crozier)
            new Npc(1011198, 419, new Vector3(-151.797f, -12.53491f, -15.14709f)),

            //Kazashi (Retainer Vocate)
            //Kugane (Kogane Dori Markets)
            new Npc(1018983, 628, new Vector3(19.36232f, 4.05f, 44.95885f)),

            //Misfrith (Retainer Vocate)
            //The Crystarium (Musica Universalis Markets)
            new Npc(1027994, 819, new Vector3(-38.96539f, -7.65f, 97.18719f)),

            //Tanine (Retainer Vocate)
            //Old Sharlayan (Old Sharlayan Aetheryte Plaza)
            new Npc(1037056, 962, new Vector3(66.39442f, 5.15f, -22.304f)),
        };

        public static async Task<bool> HireAllRetainers(bool onlyUseLimsa = false)
        {
            Npc[] vocates;
            await RefillNames();
            if (Vocates[3].CanGetTo && !onlyUseLimsa)
            {
                vocates = NpcHelper.OrderByDistance(Vocates.Skip(3)).ToArray();
            }
            else if (onlyUseLimsa)
            {
                vocates = NpcHelper.OrderByDistance(Vocates.Take(1)).ToArray();
            }
            else
            {
                vocates = NpcHelper.OrderByDistance(Vocates).ToArray();
            }

            foreach (var npc in vocates)
            {
                Log.Debug(npc.ToString());
            }

            var counts = new Dictionary<Npc, int>();

            int maxHires = await GetMaxNumRetainerHires();
            var currentHires = await GetNumRetainerHires();
            var numberOfRetainersToHire = maxHires - currentHires;

            for (var i = 0; i < numberOfRetainersToHire; i++)
            {
                var npc = vocates[i % vocates.Length];
                if (!counts.ContainsKey(npc))
                {
                    counts.Add(npc, 0);
                }

                counts[npc]++;
            }

            Log.Information($"Need to hire {numberOfRetainersToHire} retainers");

            foreach (var pair in counts)
            {
                Log.Information($"Going to hire {pair.Value} retainers from {pair.Key.Location.ClosestAetherytePrimaryResult.CurrentLocaleAethernetName}");

                for (var i = 0; i < pair.Value; i++)
                {
                    if (!await HireRetainer(pair.Key))
                    {
                        Log.Error($"Issue hiring retainer");
                        break;
                    }
                }
            }

            return await GetNumRetainerHires() == await GetMaxNumRetainerHires();
        }

        public static async Task<byte> GetMaxNumRetainerHires()
        {
            Core.Memory.Write(Offsets.MaxRetainers, (byte)0);
            await HelperFunctions.ForceGetRetainerData();
            await Coroutine.Wait(5000, () => Core.Memory.Read<byte>(Offsets.MaxRetainers) > 0);
            return Core.Memory.Read<byte>(Offsets.MaxRetainers);
        }

        public static async Task<int> GetNumRetainerHires()
        {
            var array = await HelperFunctions.GetRetainerArray(true);

            return array.Length;
        }

        public static async Task<bool> HireRetainer(Npc vocate)
        {
            Log.Information($"Going to {vocate}");
            if (!await Navigation.GetToInteractNpcSelectString(vocate, 0))
            {
                Log.Error($"Can't get to {vocate}");
                return false;
            }

            await Coroutine.Wait(5000, () => Talk.DialogOpen);
            while (Talk.DialogOpen)
            {
                Talk.Next();
                await Coroutine.Wait(500, () => !Talk.DialogOpen);
                await Coroutine.Wait(500, () => Talk.DialogOpen);
                await Coroutine.Yield();
            }

            await Coroutine.Wait(5000, () => SelectYesno.IsOpen);

            if (!SelectYesno.IsOpen)
            {
                Log.Error("Character has max retainers");
                return false;
            }

            if (SelectYesno.IsOpen)
            {
                Log.Debug("First Select yes/no");
                SelectYesno.Yes();
            }

            await Coroutine.Wait(5000, () => Talk.DialogOpen);

            while (Talk.DialogOpen)
            {
                Talk.Next();
                await Coroutine.Wait(500, () => !Talk.DialogOpen);
                await Coroutine.Wait(500, () => Talk.DialogOpen);
                await Coroutine.Yield();
            }

            await Coroutine.Wait(10000, () => SelectYesno.IsOpen);

            if (SelectYesno.IsOpen)
            {
                Log.Debug("Second Select yes/no");
                SelectYesno.Yes();
            }

            await Coroutine.Wait(10000, () => CharaMakeDataImport.Instance.IsOpen);

            if (CharaMakeDataImport.Instance.IsOpen)
            {
                Log.Debug("Should be on import appearance");
                CharaMakeDataImport.Instance.SelectAppearanceSave(0);
            }

            await Coroutine.Wait(10000, () => _CharaMakeFeature.Instance.IsOpen);

            if (_CharaMakeFeature.Instance.IsOpen)
            {
                Log.Debug("Should be on confirm appearance");
                _CharaMakeFeature.Instance.ConfirmAppearance();
            }

            await Coroutine.Wait(10000, () => SelectYesno.IsOpen);

            if (SelectYesno.IsOpen)
            {
                Log.Debug("Save Appearance Yes/No");
                SelectYesno.No();
                await Coroutine.Wait(500, () => !SelectYesno.IsOpen);
            }

            await Coroutine.Wait(10000, () => SelectYesno.IsOpen);

            if (SelectYesno.IsOpen)
            {
                Log.Debug("Accept Appearance");
                SelectYesno.Yes();
            }

            await Coroutine.Wait(10000, () => Talk.DialogOpen || SelectYesno.IsOpen);

            if (SelectYesno.IsOpen)
            {
                Log.Debug("Accept Appearance 2");
                SelectYesno.Yes();
                await Coroutine.Wait(5000, () => Talk.DialogOpen);
            }

            while (Talk.DialogOpen)
            {
                Talk.Next();
                await Coroutine.Wait(500, () => !Talk.DialogOpen);
                await Coroutine.Wait(500, () => Talk.DialogOpen);
                await Coroutine.Yield();
            }

            Log.Debug("Wait till conversation open");
            await Coroutine.Wait(5000, () => Conversation.IsOpen);
            Log.Debug("Past wait till conversation open");
            if (Conversation.IsOpen)
            {
                Log.Debug("Its open");
                Conversation.SelectLine(0);
            }

            Log.Debug("Wait till talk");
            await Coroutine.Wait(10000, () => Talk.DialogOpen);
            while (Talk.DialogOpen)
            {
                Talk.Next();
                await Coroutine.Wait(500, () => !Talk.DialogOpen);
                await Coroutine.Wait(500, () => Talk.DialogOpen);
                await Coroutine.Yield();
            }

            await Coroutine.Wait(10000, () => SelectYesno.IsOpen);

            if (SelectYesno.IsOpen)
            {
                Log.Debug("Hire Yes/No");
                SelectYesno.Yes();
            }

            await Coroutine.Wait(10000, () => Talk.DialogOpen);
            while (Talk.DialogOpen)
            {
                Talk.Next();
                await Coroutine.Wait(500, () => !Talk.DialogOpen);
                await Coroutine.Wait(500, () => Talk.DialogOpen);
                await Coroutine.Yield();
            }

            await Coroutine.Wait(10000, () => InputString.Instance.IsOpen);

            if (InputString.Instance.IsOpen)
            {
                var name = await GetName(); //ApiName(4, 12);
                bool result;
                do
                {
                    Log.Information($"Using name {name}");
                    result = await SetRetainerName(name);
                    if (result)
                    {
                        Log.Information("Retainer Created");
                    }
                    else
                    {
                        //Get Name
                        Log.Error($"Retainer failed to create retainer named {name}");
                        Log.Information("Calling API for new name");
                        name = await GetName(); //ApiName(4, 12);
                        Log.Information($"Got name: {name}");
                    }
                }
                while (!result);
            }

            return true;
        }

        public static async Task<bool> SetRetainerName(string name)
        {
            if (InputString.Instance.IsOpen)
            {
                Log.Debug("Waiting a sec to enter name");
                await Coroutine.Sleep(1000);
                InputString.Instance.Confirm(name);
            }

            await Coroutine.Wait(10000, () => SelectYesno.IsOpen);

            if (SelectYesno.IsOpen)
            {
                Log.Debug($"Hire {name} Yes/No");
                SelectYesno.Yes();
            }

            await Coroutine.Wait(10000, () => Talk.DialogOpen);
            while (Talk.DialogOpen)
            {
                Talk.Next();
                await Coroutine.Wait(500, () => !Talk.DialogOpen);
                await Coroutine.Wait(500, () => Talk.DialogOpen);
                await Coroutine.Yield();
            }

            await Coroutine.Wait(5000, () => Talk.DialogOpen);
            while (Talk.DialogOpen)
            {
                Talk.Next();
                await Coroutine.Wait(500, () => !Talk.DialogOpen);
                await Coroutine.Wait(500, () => Talk.DialogOpen);
                await Coroutine.Yield();
            }

            await Coroutine.Wait(5000, () => InputString.Instance.IsOpen || Talk.DialogOpen);

            if (InputString.Instance.IsOpen)
            {
                Log.Error("Failed to set name");
                return false;
            }

            if (Talk.DialogOpen)
            {
                while (Talk.DialogOpen)
                {
                    Talk.Next();
                    await Coroutine.Wait(500, () => !Talk.DialogOpen);
                    await Coroutine.Wait(500, () => Talk.DialogOpen);
                    await Coroutine.Yield();
                }

                Log.Information("Done");
            }

            return true;
        }

        public static async Task<string> GetName()
        {
            if (Names.Count == 0)
            {
                await RefillNames();
            }

            return Names.Dequeue();
        }

        public static async Task RefillNames()
        {
            var newNames = await GetNamesFromApi();
            foreach (var name in newNames.Where(i => i.Length is > 3 and < 15))
            {
                Names.Enqueue(name);
            }
        }

        public static async Task<List<string>> GetNamesFromApi()
        {
            var request = GenerateRequest(new Uri("https://namey.muffinlabs.com/"), "name.json");
            return await SendRequest<List<string>>(request);
        }

        public static string ApiName(int min, int max)
        {
            var client = new WebClient();

            client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            var response = client.UploadValues(
                "https://uzby.com/api.php",
                new NameValueCollection
                {
                    { "min", $"{min}" },
                    { "max", $"{max}" },
                });

            var name = Encoding.UTF8.GetString(response);
            return name.Substring(0, Math.Min(name.Length, max));
        }

        public static string GenerateRequest(Uri uri, string destination)
        {
            var builder = new UriBuilder(uri);
            builder.Path += destination;
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["count"] = "10";
            query["type"] = "surname";
            query["frequency"] = "rare";
            builder.Query = query.ToString();
            return builder.ToString();
        }

        public static async Task<T> SendRequest<T>(string url)
        {
            var cancellationToken = new CancellationTokenSource();
            using var httpClient = new HttpClient();
            var request = await Coroutine.ExternalTask(httpClient.GetAsync(url, cancellationToken.Token));

            //var request = await httpClient.GetAsync(url, cancellationToken.Token);
            cancellationToken.Token.ThrowIfCancellationRequested();

            var response = await Coroutine.ExternalTask(request.Content.ReadAsStringAsync());

            //var response = await request.Content.ReadAsStringAsync();
            cancellationToken.Token.ThrowIfCancellationRequested();

            return JsonConvert.DeserializeObject<T>(response);
        }
    }
}