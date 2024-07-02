using ff14bot.Managers;

namespace LlamaLibrary.Extensions
{
    public static class RetainerTaskAskExtensions
    {
        public static bool CanAssign()
        {
            var windowByName = RaptureAtkUnitManager.GetWindowByName("RetainerTaskAsk");

            var remoteButton = windowByName?.FindButton(40);
            return remoteButton is { Clickable: true };
        }

        public static string GetErrorReason()
        {
            var windowByName = RaptureAtkUnitManager.GetWindowByName("RetainerTaskAsk");
            return windowByName?.FindLabel(39) == null ? "" : windowByName.FindLabel(39).Text;
        }
    }
}