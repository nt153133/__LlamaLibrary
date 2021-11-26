using Clio.Utilities;
using Newtonsoft.Json;

namespace LlamaLibrary.JsonObjects
{
    public class CustomDeliveryNpc
    {
        public uint npcId;
        public uint Zone;
        public Vector3 Location;
        public string Name;
        public int RequiredQuest;
        public uint Index;

        [JsonConstructor]
        public CustomDeliveryNpc()
        {
        }

        public CustomDeliveryNpc(uint npcId, uint zone, Vector3 location, string name, int requiredQuest, uint index)
        {
            this.npcId = npcId;
            Zone = zone;
            Location = location;
            Name = name;
            RequiredQuest = requiredQuest;
            Index = index;
        }
    }
}