using Clio.Utilities;

namespace LlamaLibrary.JsonObjects
{
    public class StoredHuntLocation
    {
        public int BNpcNameKey;
        public Vector3 Location;
        public uint Map;

        public StoredHuntLocation(int name, uint mapId, Vector3 location)
        {
            Map = mapId;
            Location = location;
            BNpcNameKey = name;
        }
    }
}