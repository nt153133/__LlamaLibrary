using LlamaLibrary.Enums;
using LlamaLibrary.Helpers.NPC;

namespace LlamaLibrary.Helpers.WorldTravel
{
    public class WorldLocation
    {
        public World World { get; set; }
        public Location Location { get; set; }

        public WorldLocation(World world, Location location)
        {
            World = world;
            Location = location;
        }

        public WorldLocation()
        {
        }

        public override string ToString()
        {
            return $"World: {World}, Zone: {Location.ZoneName} Location: {Location.Coordinates}";
        }
    }
}