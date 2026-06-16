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
    /// <summary>
    /// Provides thread-safe, deferred loading of game resources and metadata from embedded JSON assets.
    /// Manages data for Materia, Hunts, Ventures, Custom Deliveries, Housing, and Grand Company shops.
    /// </summary>
    public static class ResourceManager
    {
        /// <summary>
        /// Gets a lazy-loaded dictionary of Materia items, indexed by their base item ID.
        /// Each entry contains a list of <see cref="MateriaItem"/> representing different tiers.
        /// </summary>
        public static readonly Lazy<Dictionary<int, List<MateriaItem>>> MateriaList;

        /// <summary>
        /// Gets a lazy-loaded dictionary of daily hunt locations, indexed by their mob ID.
        /// </summary>
        public static readonly Lazy<SortedDictionary<int, StoredHuntLocation>> DailyHunts;

        /// <summary>
        /// Gets a lazy-loaded list of data for all available retainer ventures.
        /// </summary>
        public static readonly Lazy<List<RetainerTaskData>> VentureData;

        /// <summary>
        /// Gets a lazy-loaded list of configuration data for Custom Delivery NPCs.
        /// </summary>
        public static readonly Lazy<List<CustomDeliveryNpc>> CustomDeliveryNpcs;

        /// <summary>
        /// Maps each <see cref="HousingZone"/> to a lazy-loaded dictionary of recorded plot data.
        /// </summary>
        public static readonly Dictionary<HousingZone, Lazy<Dictionary<int, RecordedPlot>>> HousingPlots;

        /// <summary>
        /// Gets a collection of shop items available for purchase with seals, grouped by <see cref="GrandCompany"/>.
        /// </summary>
        public static readonly Dictionary<GrandCompany, List<GCShopItemStored>> GCShopItems;

        /// <summary>
        /// Gets a collection of recipes related to Anden's custom deliveries, indexed by the resulting item ID.
        /// </summary>
        public static readonly Dictionary<uint, List<StoredRecipe>> Recipes_Anden;

        static ResourceManager()
        {
            VentureData = new Lazy<List<RetainerTaskData>>(() => LoadResource<List<RetainerTaskData>>(Resources.Ventures));
            MateriaList = new Lazy<Dictionary<int, List<MateriaItem>>>(() => LoadResource<Dictionary<int, List<MateriaItem>>>(Resources.Materia));
            DailyHunts = new Lazy<SortedDictionary<int, StoredHuntLocation>>(() => LoadResource<SortedDictionary<int, StoredHuntLocation>>(Resources.AllHunts));
            CustomDeliveryNpcs = new Lazy<List<CustomDeliveryNpc>>(() => LoadResource<List<CustomDeliveryNpc>>(Resources.CustomDeliveryNpcs));
            GCShopItems = new Lazy<Dictionary<GrandCompany, List<GCShopItemStored>>>(() => LoadResource<Dictionary<GrandCompany, List<GCShopItemStored>>>(Resources.GCShopItems)).Value;

            Recipes_Anden = new Lazy<Dictionary<uint, List<StoredRecipe>>>(() => LoadResource<Dictionary<uint, List<StoredRecipe>>>(Resources.Recipes_Anden)).Value;

            HousingPlots = new Dictionary<HousingZone, Lazy<Dictionary<int, RecordedPlot>>>
            {
                { HousingZone.Mist, new Lazy<Dictionary<int, RecordedPlot>>(() => LoadResource<Dictionary<int, RecordedPlot>>(Resources.MistPlots)) },
                { HousingZone.Goblet, new Lazy<Dictionary<int, RecordedPlot>>(() => LoadResource<Dictionary<int, RecordedPlot>>(Resources.GobletPlots)) },
                { HousingZone.LavenderBeds, new Lazy<Dictionary<int, RecordedPlot>>(() => LoadResource<Dictionary<int, RecordedPlot>>(Resources.LavenderBedsPlots)) },
                { HousingZone.Shirogane, new Lazy<Dictionary<int, RecordedPlot>>(() => LoadResource<Dictionary<int, RecordedPlot>>(Resources.ShiroganePlots)) },
                { HousingZone.Empyreum, new Lazy<Dictionary<int, RecordedPlot>>(() => LoadResource<Dictionary<int, RecordedPlot>>(Resources.EmpyreumPlots)) }
            };
        }

        /// <summary>
        /// Deserializes a JSON string into an object of type <typeparamref name="T"/>.
        /// Primarily used to load embedded resource strings.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize to.</typeparam>
        /// <param name="text">The JSON string to deserialize.</param>
        /// <returns>The deserialized object of type <typeparamref name="T"/>.</returns>
        public static T LoadResource<T>(string text)
        {
            return JsonConvert.DeserializeObject<T>(text);
        }
    }
}
