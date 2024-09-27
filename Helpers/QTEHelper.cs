using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Helpers;
using ff14bot.Managers;
using LlamaLibrary.Helpers.Keyboard;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Helpers;

public static class QTEHelper
{
    private static readonly LLogger Log = new(nameof(QTEHelper), Colors.MediumPurple);
    private static FrameCachedValue<bool> _windowOpen = new FrameCachedValue<bool>(() => RaptureAtkUnitManager.GetRawControls.Any(i => i.Name.Equals("QTE") && IsVisible(i)));
    public static bool WindowOpen => _windowOpen.Value;
    public static bool ShouldCheck => DutyManager.InInstance && RaptureAtkUnitManager.GetWindowByName("QTE", true) != null;

    public static bool QteOpen => ShouldCheck && WindowOpen;
    private static byte Flags(AtkAddonControl control) => Core.Memory.Read<byte>(control.Pointer + 0x192);

    private static Process _process;

    private static Key _key = new Key(Messaging.VKeys.KEY_SPACE);

    public static bool IsVisible(AtkAddonControl? control)
    {
        if (control == null)
        {
            return false;
        }

        return (Flags(control) & 0x20) == 0x20;
    }

    public static async Task HandleQte()
    {
        _process = Core.Memory.Process;
        if (!QteOpen)
        {
            Log.Information("QTE Window is not open");
            return;
        }

        Log.Information("QTE Window is open");

        var control = RaptureAtkUnitManager.GetRawControls.FirstOrDefault(i => i.Name.Equals("QTE") && IsVisible(i));

        if (control == null)
        {
            Log.Error("Control is null");
            return;
        }

        var handle = _process.MainWindowHandle;
        do
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            _key.PressBackground(handle);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            await Coroutine.Sleep(100);
        }
        while (IsVisible(control));
    }
}