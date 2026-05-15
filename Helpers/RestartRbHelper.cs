using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using ff14bot;
using ff14bot.Helpers;
using LlamaLibrary.Logging;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// Provides a helper for restarting the RebornBuddy process while keeping the FFXIV client running.
    /// Writes a batch script to the user's AppData folder that waits for RebornBuddy to close,
    /// then re-launches it attached to the same FFXIV process.
    /// </summary>
    public static class RestartRbHelper
    {
        private static readonly string TempFolderLocation = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        private static readonly string RebornbuddyExecutable = Path.Combine(Utils.AssemblyDirectory, "RebornBuddy.exe");

        private static readonly string BatchFileName = "restart_rebornbuddy.bat";

        private static readonly string BatchFileContent = @"@echo off
set RB_PID=%1
set FFXIV_PID=%2
set RB_EXECUTABLE=%3

title [RestartRbHelper] WaitForClose
echo [RestartRbHelper] waiting for Rebornbuddy (PID: %1, attached to FFXIV PID: %2) to close before restarting...

:whileRB
timeout /t 5 /nobreak
taskkill /PID %1 /F /FI ""status eq not responding"" >nul
TASKLIST /FI ""PID eq %1"" | FINDSTR /I ""RebornBuddy.exe"" >nul && goto :whileRB

start rebornbuddy.exe --processid=%2 -a

exit";

        private static readonly LLogger Log = new("RestartRbHelper", Colors.MediumPurple);

        /// <summary>
        /// Writes the restart batch script and launches it, then sends a close request to the current RebornBuddy process.
        /// The batch script waits for RB to exit, then re-launches it attached to the same FFXIV PID.
        /// </summary>
        public static void RestartRebornbuddy()
        {
            try
            {
                if (!Directory.Exists(TempFolderLocation))
                {
                    return;
                }

                // Update the batch file even if it already exists, it might need to be updated
                File.WriteAllText(Path.Combine(TempFolderLocation, BatchFileName), BatchFileContent);

                Log.Information("RestartRbHelper - Restarting Rebornbuddy");
                Log.Information($"RBExecutable location {RebornbuddyExecutable}");
                Process RBprocess = Process.GetCurrentProcess();
                Process.Start(Path.Combine(TempFolderLocation, BatchFileName), $"{RBprocess.Id} {Core.Memory.Process.Id} \"{RebornbuddyExecutable}\"");

                if (!RBprocess.CloseMainWindow())
                {
                    Log.Error("RestartRbHelper - Unable to shutdown RB.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"RestartRbHelper error: {ex}");
            }
        }
    }
}