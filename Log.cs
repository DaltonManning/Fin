using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Fin
{
    internal static class Log
    {
        private static readonly string LogFile = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            $"Fin_{DateTime.Now:yyyyMMdd_HHmmss}.log");

        public static void Info(string message)  => Write("INFO ", message);
        public static void Warn(string message)  => Write("WARN ", message);
        public static void Error(string message) => Write("ERROR", message);

        public static void Error(string message, Exception ex)
        {
            Write("ERROR", $"{message}: {ex.Message}");
            if (ex is COMException com)
                Write("ERROR", $"  HRESULT: 0x{com.ErrorCode:X}");
            if (!string.IsNullOrEmpty(ex.StackTrace))
                Write("ERROR", $"  {ex.StackTrace}");
        }

        private static void Write(string level, string message)
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {level} {message}";
            Console.WriteLine(line);
            try { File.AppendAllText(LogFile, line + Environment.NewLine); }
            catch { /* logging must never throw */ }
        }
    }
}
