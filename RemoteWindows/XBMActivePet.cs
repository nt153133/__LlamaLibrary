using System.Threading.Tasks;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>Provides access to the Beastmaster active-pet addon identified by FFXIVClientStructs.</summary>
    /// <remarks>
    /// The name is mapped in FFXIVClientStructs bb007e6f, but its relationship to Battlehorn Settings
    /// and its slot fields still require live verification. Do not interpret this window as Crucible
    /// team composition or assume its values are summon action IDs. See Docs/Beastmaster.md.
    /// </remarks>
    public class XBMActivePet : RemoteWindow<XBMActivePet>
    {
        /// <summary>Creates a wrapper that looks up the active-pet addon without retaining its native pointer.</summary>
        public XBMActivePet() : base("XBMActivePet")
        {
        }

        /// <summary>Checks whether the game has already opened the active-pet addon.</summary>
        /// <returns>A completed task containing true when visible, or false when the UI must first open it.</returns>
        /// <remarks>No opening callback is verified yet, so this does not toggle an unregistered agent.</remarks>
        public override Task<bool> Open() => Task.FromResult(IsOpen);
    }
}