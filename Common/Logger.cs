using System;
using System.IO;

namespace Ecoinv.Common
{
    public static class Logger
    {
        // A log fájl a program .exe fájlja mellett fog létrejönni
        private static string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Ecoinv_Events.log");

        public static void Log(string message, string level = "INFO")
        {
            try
            {
                // Formátum: [2026.01.19 08:30:15] [INFO] Üzenet szövege
                string logLine = $"[{DateTime.Now:yyyy.MM.dd HH:mm:ss}] [{level}] {message}";

                // Hozzáírja a fájlhoz, ha nem létezik, létrehozza
                File.AppendAllLines(logFilePath, new[] { logLine });
            }
            catch
            {
                // Ha a logolás hibázik, nem akarjuk, hogy a program leálljon
            }
        }

        public static void LogError(Exception ex, string context = "")
        {
            string message = string.IsNullOrEmpty(context) ? ex.Message : $"{context}: {ex.Message}";
            // Beírjuk a hibaüzenetet és a technikai részleteket (Stack Trace) is
            Log(message + Environment.NewLine + "Stack Trace: " + ex.StackTrace, "ERROR");
        }
    }
}