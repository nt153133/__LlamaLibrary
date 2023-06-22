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

        private static readonly string BatchFileName = "restart_rebornbuddy.bat";

        private static readonly string BatchFileContent = @$"@echo off
set RB_PID=%1
set FFXIV_PID=%2

title [RestartRbHelper] WaitForClose
echo [RestartRbHelper] waiting for Rebornbuddy (PID: %RB_PID%, attached to FFXIV PID: %FFXIV_PID%) to close before restarting...

:whileRB
timeout /t 5 /nobreak
taskkill /PID %RB_PID% /F /FI ""status eq not responding"" >nul
TASKLIST /FI ""PID eq %RB_PID%"" | FINDSTR /I ""RebornBuddy.exe"" >nul && goto :whileRB

start rebornbuddy.exe --processid=%FFXIV_PID% --autologin --autostart= 5000

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
                Process RBprocess = Process.GetCurrentProcess();
                Process.Start(Path.Combine(TempFolderLocation, BatchFileName), $"{RBprocess.Id} {ff14bot.Core.Memory.Process.Id}");

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