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
    /// Provides thread-safe, deferred access to static game data resources stored as JSON.
    /// Manages information for Materia, Hunts, Ventures, Custom Deliveries, Housing, and Grand Company shops.
    /// </summary>
    public static class ResourceManager
    {
        /// <summary>
        /// Gets the collection of Materia items, indexed by their tier or type.
        /// </summary>
        public static readonly Lazy<Dictionary<int, List<MateriaItem>>> MateriaList;

        /// <summary>
        /// Gets the collection of daily hunt locations, sorted by their identifier.
        /// </summary>
        public static readonly Lazy<SortedDictionary<int, StoredHuntLocation>> DailyHunts;

        /// <summary>
        /// Gets the list of available retainer venture tasks and their associated requirements/rewards.
        /// </summary>
        public static readonly Lazy<List<RetainerTaskData>> VentureData;

        /// <summary>
        /// Gets the list of NPCs that participate in the Custom Delivery system.
        /// </summary>
        public static readonly Lazy<List<CustomDeliveryNpc>> CustomDeliveryNpcs;

        /// <summary>
        /// Gets a mapping of housing zones to their respective plot data, including location coordinates.
        /// </summary>
        public static readonly Dictionary<HousingZone, Lazy<Dictionary<int, RecordedPlot>>> HousingPlots;

        /// <summary>
        /// Gets the collection of items available for purchase from Grand Company quartermasters, indexed by company.
        /// </summary>
        public static readonly Dictionary<GrandCompany, List<GCShopItemStored>> GCShopItems;

        /// <summary>
        /// Gets the collection of crafting recipes associated with Anden's custom deliveries.
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
        /// Deserializes a JSON resource string into the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="text">The JSON string content to deserialize.</param>
        /// <returns>The deserialized object of type <typeparamref name="T"/>.</returns>
        public static T LoadResource<T>(string text)
        {
            return JsonConvert.DeserializeObject<T>(text);
        }
    }
}