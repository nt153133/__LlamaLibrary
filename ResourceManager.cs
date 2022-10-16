using System;
using System.Collections.Generic;
using ff14bot.Enums;
using LlamaLibrary.Helpers.Housing;
using LlamaLibrary.JsonObjects;
using LlamaLibrary.Properties;
using LlamaLibrary.Retainers;
using Newtonsoft.Json;

namespace LlamaLibrary
{
    public static class ResourceManager
    {
        public static readonly Lazy<Dictionary<int, List<MateriaItem>>> MateriaList;
        public static readonly Lazy<SortedDictionary<int, StoredHuntLocation>> DailyHunts;
        public static readonly Lazy<List<RetainerTaskData>> VentureData;
        public static readonly Lazy<List<CustomDeliveryNpc>> CustomDeliveryNpcs;
        public static readonly Dictionary<HousingZone, Lazy<Dictionary<int, RecordedPlot>>> HousingPlots;
        public static readonly Dictionary<GrandCompany, List<GCShopItemStored>> GCShopItems;

        static ResourceManager()
        {
            VentureData = new Lazy<List<RetainerTaskData>>(() => LoadResource<List<RetainerTaskData>>(Resources.Ventures));
            MateriaList = new Lazy<Dictionary<int, List<MateriaItem>>>(() => LoadResource<Dictionary<int, List<MateriaItem>>>(Resources.Materia));
            DailyHunts = new Lazy<SortedDictionary<int, StoredHuntLocation>>(() => LoadResource<SortedDictionary<int, StoredHuntLocation>>(Resources.AllHunts));
            CustomDeliveryNpcs = new Lazy<List<CustomDeliveryNpc>>(() => LoadResource<List<CustomDeliveryNpc>>(Resources.CustomDeliveryNpcs));
            GCShopItems = new Lazy<Dictionary<GrandCompany, List<GCShopItemStored>>>(() => LoadResource<Dictionary<GrandCompany, List<GCShopItemStored>>>(Resources.GCShopItems)).Value;

            HousingPlots = new Dictionary<HousingZone, Lazy<Dictionary<int, RecordedPlot>>>
            {
                { HousingZone.Mist, new Lazy<Dictionary<int, RecordedPlot>>(() => LoadResource<Dictionary<int, RecordedPlot>>(Resources.MistPlots)) },
                { HousingZone.Goblet, new Lazy<Dictionary<int, RecordedPlot>>(() => LoadResource<Dictionary<int, RecordedPlot>>(Resources.GobletPlots)) },
                { HousingZone.LavenderBeds, new Lazy<Dictionary<int, RecordedPlot>>(() => LoadResource<Dictionary<int, RecordedPlot>>(Resources.LavenderBedsPlots)) },
                { HousingZone.Shirogane, new Lazy<Dictionary<int, RecordedPlot>>(() => LoadResource<Dictionary<int, RecordedPlot>>(Resources.ShiroganePlots)) },
                { HousingZone.Empyreum, new Lazy<Dictionary<int, RecordedPlot>>(() => LoadResource<Dictionary<int, RecordedPlot>>(Resources.EmpyreumPlots)) }
            };
        }

        public static T LoadResource<T>(string text)
        {
            return JsonConvert.DeserializeObject<T>(text);
        }
    }
}