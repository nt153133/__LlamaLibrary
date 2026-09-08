using System.Threading.Tasks;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>Provides access to the selected familiar's detail page in the Master's Bestiary.</summary>
    /// <remarks>
    /// FFXIVClientStructs bb007e6f names this addon XBMMonsterNotebookDetail. The reported
    /// XBMMonsterBookDetail spelling remains unverified; capture live addon names before adding an alias.
    /// Selection and assignment callback arguments are intentionally left unmapped until observed.
    /// </remarks>
    public class XBMMonsterNotebookDetail : RemoteWindow<XBMMonsterNotebookDetail>
    {
        /// <summary>Creates a wrapper for the client-owned selected-familiar detail page.</summary>
        public XBMMonsterNotebookDetail() : base("XBMMonsterNotebookDetail")
        {
        }

        /// <summary>Checks whether the detail page has already been opened by selecting a familiar.</summary>
        /// <returns>A completed task containing true when visible, or false when selection is still required.</returns>
        /// <remarks>
        /// This page cannot be opened independently with the evidence currently available.
        /// Avoid the base implementation's null agent toggle or inventing a creature-selection callback.
        /// </remarks>
        public override Task<bool> Open() => Task.FromResult(IsOpen);
    }
}