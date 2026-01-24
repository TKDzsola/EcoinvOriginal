using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ecoinv.Common
{
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        // --- ÚJ, PROFI METÓDUSOK ---

        public static void LogInfo(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            WriteLog("INFO", message, null, caller, file, line);
        }

        public static void LogError(Exception ex, string message = "", [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            WriteLog("ERROR", message, ex, caller, file, line);
        }

        // --- RÉGI KÓD KOMPATIBILITÁS (Ez javítja a hibákat!) ---
        // Ha valahol a régi kód azt hívja, hogy Logger.Log("üzenet"), 
        // ez elkapja, és átirányítja az új LogInfo-ra.
        public static void Log(string message, string level = "INFO")
        {
            if (level == "ERROR")
                WriteLog("ERROR", message, null, "LegacyCall", "Unknown", 0);
            else
                WriteLog("INFO", message, null, "LegacyCall", "Unknown", 0);
        }
        // -------------------------------------------------------

        private static void WriteLog(string level, string message, Exception ex, string caller, string file, int line)
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                    Directory.CreateDirectory(LogDirectory);

                string fileName = $"log_{DateTime.Now:yyyyMMdd}.txt";
                string fullPath = Path.Combine(LogDirectory, fileName);

                string shortFileName = Path.GetFileName(file);

                var logLine = new StringBuilder();
                logLine.Append($"[{DateTime.Now:HH:mm:ss}] ");
                logLine.Append($"[{level}] ");

                // Ha legacy hívás, máshogy formázzuk
                if (caller == "LegacyCall")
                    logLine.Append("[RégiKód] ");
                else
                    logLine.Append($"[{shortFileName}::{caller}:{line}] ");

                if (!string.IsNullOrEmpty(message))
                    logLine.Append($"- {message} ");

                if (ex != null)
                {
                    logLine.AppendLine();
                    logLine.Append($"   >>> HIBA: {ex.Message}");
                    logLine.AppendLine();
                    logLine.Append($"   >>> STACK: {ex.StackTrace}");
                }

                lock (_lock)
                {
                    File.AppendAllText(fullPath, logLine.ToString() + Environment.NewLine);
                }
            }
            catch { }
        }
    }
}