using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using LlamaBotBases.AutoRetainerSort.Classes;
using LlamaLibrary.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LlamaBotBases.AutoRetainerSort
{
    public static class LisbethRuleGenerator
    {
        private static readonly LLogger Log = new LLogger(Strings.LogPrefix, Colors.Orange);

        public static string GetSettingsPath()
        {
            var botType = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(i => i.FullName.Contains("Lisbeth.Reborn"))?.DefinedTypes.FirstOrDefault(i => i.Name == "Directories")
                               ?? AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(i => i.FullName.Contains("Lisbeth"))?.DefinedTypes.FirstOrDefault(i => i.Name == "Directories");

            if (botType == null)
            {
                Log.Error($"Couldn't find our Lisbeth install, but we're supposed to generate rules for it...?");
                return string.Empty;
            }

            Log.Debug($"Lisbeth Type {botType.FullName}");

            var assemblyLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location ?? string.Empty);
            var settingsPath = botType.GetProperty("SettingsPath")?.GetValue(null) as string ?? string.Empty;

            if (string.IsNullOrEmpty(assemblyLocation) || string.IsNullOrEmpty(settingsPath))
            {
                return string.Empty;
            }

            var settingsFilePath = Path.Combine(assemblyLocation, settingsPath);
            return settingsFilePath;
        }

        private static JObject GetJObject(string settingsFilePath)
        {
            using (var reader = File.OpenText(settingsFilePath))
            {
                return (JObject)JToken.ReadFrom(new JsonTextReader(reader));
            }
        }

        public static void PopulateSettings(string settingsPath)
        {
            if (string.IsNullOrEmpty(settingsPath))
            {
                Log.Error("Provided Lisbeth settings path is invalid!");
                return;
            }

            var settingsJObject = GetJObject(settingsPath);
            var knownRules = new LisbethRetainerRules(settingsJObject);

            foreach (var indexInfoPair in AutoRetainerSortSettings.Instance.InventoryOptions)
            {
                if (!knownRules.RulesByIndex.ContainsKey(indexInfoPair.Key))
                {
                    continue;
                }

                var ruleList = knownRules.RulesByIndex[indexInfoPair.Key];
                foreach (var itemId in indexInfoPair.Value.SpecificItems.Select(x => x.RawItemId).Distinct())
                {
                    ruleList.Add(new LisbethRetainerRules.ItemRule(itemId));
                }
            }

            foreach (var cachedInventory in ItemSortStatus.GetAllInventories())
            {
                if (!knownRules.RulesByIndex.ContainsKey(cachedInventory.Index))
                {
                    continue;
                }

                var ruleList = knownRules.RulesByIndex[cachedInventory.Index];
                foreach (var sortInfo in cachedInventory.ItemCounts.Select(x => ItemSortStatus.GetSortInfo(x.Key)).Distinct())
                {
                    if (sortInfo.ItemInfo.Unique || sortInfo.ItemInfo.StackSize <= 1)
                    {
                        continue;
                    }

                    ruleList.Add(new LisbethRetainerRules.ItemRule(sortInfo.RawItemId));
                }
            }

            SetRules(settingsJObject, knownRules);

            using (var outputFile = new StreamWriter(settingsPath, false))
            {
                outputFile.Write(JsonConvert.SerializeObject(settingsJObject, Formatting.None));
            }
        }

        private static void SetRules(JObject settingsObject, LisbethRetainerRules rules)
        {
            foreach (var retainerToken in settingsObject["Retainers"])
            {
                var retainerIndex = retainerToken["Index"]?.ToObject<int>() ?? 0;
                retainerToken["Rules"] = JToken.FromObject(rules.RulesByIndex[retainerIndex]);
            }
        }
    }

    public class LisbethRetainerRules
    {
        private static readonly LLogger Log = new LLogger(Strings.LogPrefix, Colors.Orange);

        public Dictionary<int, HashSet<ItemRule>> RulesByIndex;

        [JsonObject(MemberSerialization.OptIn)]
        public class ItemRule : IEquatable<ItemRule>
        {
            [JsonProperty("Item")]
            public readonly uint ItemId;

            [JsonProperty("LowerQuality")]
            public readonly bool LowerQuality;

            public bool Equals(ItemRule other)
            {
                if (other is null)
                {
                    return false;
                }

                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                return ItemId == other.ItemId;
            }

            public override bool Equals(object obj)
            {
                if (obj is null)
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj.GetType() != GetType())
                {
                    return false;
                }

                return Equals((ItemRule)obj);
            }

            public override int GetHashCode()
            {
                return (int)ItemId;
            }

            public ItemRule(uint itemId, bool lowerQuality = false)
            {
                ItemId = itemId;
                LowerQuality = lowerQuality;
            }
        }

        public LisbethRetainerRules(JObject lisbethSettings)
        {
            RulesByIndex = new Dictionary<int, HashSet<ItemRule>>();
            var retainerSettings = lisbethSettings["Retainers"];
            if (retainerSettings == null)
            {
                Log.Warning("No retainers found in Lisbeth's settings!");
                return;
            }

            foreach (var retainerInfo in retainerSettings)
            {
                var index = retainerInfo["Index"]?.ToObject<int>() ?? 0;
                RulesByIndex.Add(index, new HashSet<ItemRule>());
                var ruleSet = RulesByIndex[index];
                var currentRules = retainerInfo["Rules"];
                if (currentRules == null)
                {
                    Log.Warning("RetainerInfo didn't contain any rules array!");
                    return;
                }

                foreach (var rule in currentRules)
                {
                    var itemRule = rule.ToObject<ItemRule>();
                    if (itemRule == null)
                    {
                        var itemIdToken = rule["Item"];
                        Log.Error(itemIdToken == null ? "Couldn't parse rule! ID is null?" : $"Couldn't parse rule for ID {itemIdToken.ToObject<uint>()}");
                        continue;
                    }

                    ruleSet.Add(itemRule);
                }
            }
        }
    }
}