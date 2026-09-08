using System;
using System.Linq;
using System.Text;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.RemoteWindows;
using AtkValueType = LlamaLibrary.RemoteWindows.Atk.ValueType;

namespace LlamaLibrary.Helpers
{
    /// <summary>Captures read-only Beastmaster UI evidence needed to map battlehorn assignment.</summary>
    /// <remarks>
    /// Uses the existing RemoteWindow/TwoInt reader and signature-resolved ATK offsets.
    /// No XBM field offsets, agent IDs, or callback values are assumed. Output is materialized
    /// while the frame is held so callers do not retain live string pointers after the capture.
    /// </remarks>
    public static class BeastmasterDiagnostics
    {
        /// <summary>Describes visible XBM addons and the context menu without sending UI actions.</summary>
        /// <returns>A text snapshot of addon names, associated agent IDs, and typed ATK values; an explicit message if none are visible.</returns>
        /// <remarks>
        /// Call with RB attached and initialized, preferably with the bot stopped from RebornConsole.
        /// GetRawControls avoids reliance on another thread's cached window list. The frame lock
        /// is held only for synchronous reads; there are no waits or coroutine operations.
        /// Only string-tagged values are dereferenced; vectors and unknown types remain raw data.
        /// A value snapshot does not capture callbacks and cannot establish their parameter order.
        /// </remarks>
        public static string Capture()
        {
            var text = new StringBuilder();
            using (Core.Memory.AcquireFrame())
            using (Core.Memory.TemporaryCacheState(false))
            {
                var controls = RaptureAtkUnitManager.GetRawControls
                    .Where(control => control != null && control.IsVisible &&
                        (control.Name.StartsWith("XBM", StringComparison.Ordinal) || control.Name == "ContextMenu"))
                    .ToArray();

                foreach (var control in controls)
                {
                    var agent = control.TryFindAgentInterface();
                    text.AppendLine($"{control.Name}: agent={agent?.Id.ToString() ?? "unresolved"}");
                    WindowReader.AppendValues(text, control);
                }

                if (controls.Length == 0)
                {
                    text.AppendLine("No visible XBM addons or ContextMenu. Open the Master's Bestiary first.");
                }
            }

            return text.ToString();
        }

        // A private adapter exposes the established protected reader without expanding RemoteWindow's
        // public API or duplicating its client-specific ATK count/pointer extraction.
        private sealed class WindowReader : RemoteWindow
        {
            private WindowReader() : base(string.Empty)
            {
            }

            internal static void AppendValues(StringBuilder text, AtkAddonControl control)
            {
                var values = ReadElements(control);
                text.AppendLine($"  AtkValues={values.Length}");
                for (var index = 0; index < values.Length; index++)
                {
                    var value = values[index];
                    var type = (AtkValueType)value.Type;
                    var baseType = type & AtkValueType.TypeMask;
                    var description = $"  [{index}] type={type} data=0x{value.Data:X}";
                    if ((baseType == AtkValueType.String || baseType == AtkValueType.String8) && value.Data != 0)
                    {
                        // Bound diagnostic string reads to 1 KiB: labels are short, and an unexpected
                        // client layout must not turn a UI probe into an unbounded remote-memory scan.
                        var label = Core.Memory.ReadString((IntPtr)value.Data, Encoding.UTF8, 1024);
                        description += " text=" + label.Replace("\r", "\\r").Replace("\n", "\\n");
                    }

                    text.AppendLine(description);
                }
            }
        }
    }
}