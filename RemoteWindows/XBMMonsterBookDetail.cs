using System.Threading.Tasks;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>Provides access to the selected familiar's detail page in the Master's Bestiary.</summary>
    /// <remarks>
    /// The live 7.56 capture confirms XBMMonsterBookDetail, despite FFXIVClientStructs bb007e6f's
    /// XBMMonsterNotebookDetail symbol. Both book windows report agent 500 in that capture;
    /// this detail addon has no ATK values of its own, while the notebook carries the selected details.
    /// Selection and assignment callback arguments are intentionally left unmapped until observed.
    /// </remarks>
    public class XBMMonsterBookDetail : RemoteWindow<XBMMonsterBookDetail>
    {
        /// <summary>Creates a wrapper for the client-owned selected-familiar detail page.</summary>
        public XBMMonsterBookDetail() : base("XBMMonsterBookDetail")
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