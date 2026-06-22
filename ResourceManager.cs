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
    /// Manages the loading, caching, and access to embedded game resources and static data.
    /// Utilizes <see cref="Lazy{T}"/> for deferred initialization of JSON-backed data sets.
    /// </summary>
    public static class ResourceManager
    {
        /// <summary>Gets a lazily-loaded dictionary of materia items, indexed by their tier or type.</summary>
        public static readonly Lazy<Dictionary<int, List<MateriaItem>>> MateriaList;

        /// <summary>Gets a lazily-loaded sorted dictionary of daily hunt locations, indexed by their internal identifier.</summary>
        public static readonly Lazy<SortedDictionary<int, StoredHuntLocation>> DailyHunts;

        /// <summary>Gets a lazily-loaded list of retainer task (venture) data.</summary>
        public static readonly Lazy<List<RetainerTaskData>> VentureData;

        /// <summary>Gets a lazily-loaded list of custom delivery NPCs and their requirements.</summary>
        public static readonly Lazy<List<CustomDeliveryNpc>> CustomDeliveryNpcs;

        /// <summary>
        /// Gets a registry of housing plot data, partitioned by <see cref="HousingZone"/> and indexed by plot number.
        /// Each zone's plot dictionary is lazily initialized upon first access.
        /// </summary>
        public static readonly Dictionary<HousingZone, Lazy<Dictionary<int, RecordedPlot>>> HousingPlots;

        /// <summary>Gets a dictionary of Grand Company shop items, partitioned by <see cref="GrandCompany"/>.</summary>
        public static readonly Dictionary<GrandCompany, List<GCShopItemStored>> GCShopItems;

        /// <summary>Gets a dictionary of stored recipes for the Custom Delivery NPC Anden, indexed by the resulting item ID.</summary>
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
        /// Deserializes a JSON string into a specific data type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON into.</typeparam>
        /// <param name="text">The JSON string content, typically retrieved from <see cref="Resources"/>.</param>
        /// <returns>The deserialized object of type <typeparamref name="T"/>.</returns>
        public static T LoadResource<T>(string text)
        {
            return JsonConvert.DeserializeObject<T>(text);
        }
    }
}