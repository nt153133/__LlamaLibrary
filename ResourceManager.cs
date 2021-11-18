using System;
using System.Collections.Generic;
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

        static ResourceManager()
        {
            VentureData = new Lazy<List<RetainerTaskData>>(() => LoadResource<List<RetainerTaskData>>(Resources.Ventures));
            MateriaList = new Lazy<Dictionary<int, List<MateriaItem>>>(() => LoadResource<Dictionary<int, List<MateriaItem>>>(Resources.Materia));
            DailyHunts = new Lazy<SortedDictionary<int, StoredHuntLocation>>(() => LoadResource<SortedDictionary<int, StoredHuntLocation>>(Resources.AllHunts));
        }

        public static T LoadResource<T>(string text)
        {
            return JsonConvert.DeserializeObject<T>(text);
        }
    }
}