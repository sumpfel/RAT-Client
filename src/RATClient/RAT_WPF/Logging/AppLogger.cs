using System;
using System.IO;
using System.Linq;

namespace RAT_WPF.Logging
{
    //KI start (Claude Opus 4.8, prompt 22): simple rolling file logger.
    // - logs go into a "logs" folder next to the executable
    // - every program start opens a NEW file named rat_<yyyy-MM-dd_HH-mm-ss>.tail
    // - only the 3 most recent log files are kept (older ones are deleted on start)
    public static class AppLogger
    {
        private static readonly object _lock = new object();
        private static string? _logFile;
        private const int KeepFiles = 3;

        public static string? LogFilePath => _logFile;

        //KI start (Claude Opus 4.8, prompt 28): the logs directory (created on Start), used by the "Open log folder"
        // button in Settings.
        public static string LogDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

        /// <summary>Opens the logs folder in Explorer (best-effort). Returns false if it couldn't be opened.</summary>
        public static bool OpenLogFolder()
        {
            try
            {
                Directory.CreateDirectory(LogDirectory);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = LogDirectory,
                    UseShellExecute = true
                });
                return true;
            }
            catch (Exception ex)
            {
                Error("Could not open the log folder", ex);
                return false;
            }
        }
        //KI end

        /// <summary>Creates this run's log file, prunes old ones to the newest 3, and writes a start line.</summary>
        public static void Start()
        {
            try
            {
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                Directory.CreateDirectory(dir);

                string name = $"rat_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.tail";
                _logFile = Path.Combine(dir, name);

                // keep only the newest (KeepFiles - 1) existing logs, since we are about to add one more
                FileInfo[] existing = new DirectoryInfo(dir)
                    .GetFiles("rat_*.tail")
                    .OrderByDescending(f => f.LastWriteTimeUtc)
                    .ToArray();
                foreach (FileInfo old in existing.Skip(KeepFiles - 1))
                {
                    try { old.Delete(); } catch { /* a locked/old file shouldn't stop logging */ }
                }

                Info("=== RAT client started ===");
            }
            catch
            {
                // logging must never crash the app
                _logFile = null;
            }
        }

        public static void Debug(string message) => Write("DEBUG", message); // KI (prompt 28)
        public static void Info(string message) => Write("INFO", message);
        public static void Warn(string message) => Write("WARN", message);
        public static void Error(string message) => Write("ERROR", message);

        /// <summary>Logs an exception with its message + stack trace.</summary>
        public static void Error(string message, Exception ex) =>
            Write("ERROR", $"{message} :: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");

        private static void Write(string level, string message)
        {
            if (_logFile == null) { return; }
            string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {level} {message}";
            try
            {
                lock (_lock)
                {
                    File.AppendAllText(_logFile, line + Environment.NewLine);
                }
            }
            catch
            {
                // swallow — a logging failure must not affect the app
            }
        }
    }
    //KI end
}
