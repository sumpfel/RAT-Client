using System;
using System.IO;

namespace RAT_WPF.Setup
{
    //KI start (Claude Opus 4.8, prompt 25): first-run setup. The app, on first start, offers to create a desktop
    // shortcut and install nmap. Choices are remembered in a marker file next to the exe so the prompt only shows
    // once. NmapDeclined is read by the topology to grey out the Discover button.
    public static class SetupService
    {
        private static readonly string MarkerFile =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".rat_setup_done");

        private static readonly string DeclineFile =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".rat_nmap_declined");

        /// <summary>True if first-run setup already ran.</summary>
        public static bool HasRunSetup => File.Exists(MarkerFile);

        /// <summary>True if the user declined installing nmap (persisted) — used to grey out discovery.</summary>
        public static bool NmapDeclined
        {
            get => File.Exists(DeclineFile);
            set
            {
                try
                {
                    if (value) { File.WriteAllText(DeclineFile, DateTime.Now.ToString("o")); }
                    else if (File.Exists(DeclineFile)) { File.Delete(DeclineFile); }
                }
                catch { /* preference persistence is best-effort */ }
            }
        }

        public static void MarkSetupDone()
        {
            try { File.WriteAllText(MarkerFile, DateTime.Now.ToString("o")); } catch { }
        }

        /// <summary>Creates a "RAT.lnk" desktop shortcut pointing at the running exe (via Windows Script Host).</summary>
        public static bool CreateDesktopShortcut()
        {
            try
            {
                string exePath = Environment.ProcessPath
                                 ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RAT_WPF.exe");
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string lnk = Path.Combine(desktop, "RAT.lnk");

                Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) { return false; }
                dynamic? shell = Activator.CreateInstance(shellType);
                if (shell == null) { return false; }

                dynamic shortcut = shell.CreateShortcut(lnk);
                shortcut.TargetPath = exePath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
                shortcut.IconLocation = exePath + ",0";
                shortcut.Description = "RAT — Remote Access Topologie";
                shortcut.Save();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    //KI end
}
