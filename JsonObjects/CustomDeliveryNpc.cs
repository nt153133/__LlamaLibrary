using Clio.Utilities;
using Newtonsoft.Json;

namespace LlamaLibrary.JsonObjects
{
    /// <summary>
    /// Represents metadata for a Custom Delivery NPC, typically loaded from embedded resources.
    /// </summary>
    public class CustomDeliveryNpc
    {
        /// <summary>The ENpcResident identifier of the NPC.</summary>
        public uint npcId;

        /// <summary>The TerritoryType (zone) identifier where the NPC is located.</summary>
        public uint Zone;

        /// <summary>The world coordinates for the NPC's location.</summary>
        public Vector3 Location;

        /// <summary>The display name of the NPC.</summary>
        public string Name;

        /// <summary>The Quest identifier required to unlock this NPC's deliveries.</summary>
        public int RequiredQuest;

        /// <summary>The internal index or order of the NPC within the custom delivery system.</summary>
        public uint Index;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomDeliveryNpc"/> class for JSON deserialization.
        /// </summary>
        [JsonConstructor]
        public CustomDeliveryNpc()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomDeliveryNpc"/> class with specific data.
        /// </summary>
        /// <param name="npcId">The ENpcResident identifier.</param>
        /// <param name="zone">The zone identifier.</param>
        /// <param name="location">The world coordinates.</param>
        /// <param name="name">The NPC's name.</param>
        /// <param name="requiredQuest">The unlock quest ID.</param>
        /// <param name="index">The system index.</param>
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
