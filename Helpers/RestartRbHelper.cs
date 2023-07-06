namespace LlamaLibrary.Helpers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Media;
    using LlamaLibrary.Logging;

    public static class RestartRbHelper
    {
        private static readonly string TempFolderLocation = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        private static readonly string RebornbuddyExecutable = Path.Combine(ff14bot.Helpers.Utils.AssemblyDirectory, "RebornBuddy.exe");

        private static readonly string BatchFileName = "restart_rebornbuddy.bat";

        private static readonly string BatchFileContent = @$"@echo off
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
                Process.Start(Path.Combine(TempFolderLocation, BatchFileName), $"{RBprocess.Id} {ff14bot.Core.Memory.Process.Id} \"{RebornbuddyExecutable}\"");

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