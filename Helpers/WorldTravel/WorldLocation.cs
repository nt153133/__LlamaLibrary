using LlamaLibrary.Enums;
using LlamaLibrary.Helpers.NPC;

namespace LlamaLibrary.Helpers.WorldTravel
{
    /// <summary>
    /// Represents a specific location on a particular world (server), pairing a target
    /// <see cref="World"/> with an in-game <see cref="Location"/> that contains the zone ID
    /// and 3-D coordinates.
    /// </summary>
    public class WorldLocation
    {
        /// <summary>Gets or sets the target world (server).</summary>
        public World World { get; set; }

        /// <summary>Gets or sets the in-game location, including zone ID and 3-D coordinates.</summary>
        public Location Location { get; set; }

        /// <summary>
        /// Initialises a new <see cref="WorldLocation"/> with an explicit world and location.
        /// </summary>
        /// <param name="world">The target <see cref="World"/> server.</param>
        /// <param name="location">The in-game <see cref="Location"/> within that world.</param>
        public WorldLocation(World world, Location location)
        {
            World = world;
            Location = location;
        }

        /// <summary>Initialises a new empty <see cref="WorldLocation"/>.</summary>
        public WorldLocation()
        {
        }

        /// <summary>Returns a human-readable summary of this world location.</summary>
        /// <returns>A string in the form <c>World: {world}, Zone: {name} Location: {coords}</c>.</returns>
        public override string ToString()
        {
            return $"World: {World}, Zone: {Location.ZoneName} Location: {Location.Coordinates}";
        }
    }
}