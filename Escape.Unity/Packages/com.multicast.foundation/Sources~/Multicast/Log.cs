namespace Multicast {
    using System;

    public static class MulticastLog {
        public static bool IsDebugLogEnabled = true;
        public static bool IsErrorLogEnabled = true;

        public static LogCallback DebugLogCallback = (tag, message) => Console.WriteLine($"[{tag}] DEBUG: {message}");
        public static LogCallback ErrorLogCallback = (tag, message) => Console.WriteLine($"[{tag}] ERROR: {message}");

        public static Logger? Debug => IsDebugLogEnabled ? new Logger { callback = DebugLogCallback } : default(Logger?);
        public static Logger? Error => IsErrorLogEnabled ? new Logger { callback = ErrorLogCallback } : default(Logger?);

        public struct Logger {
            public LogCallback callback;

            public void Log(string tag, string message) => callback?.Invoke(tag, message);
        }

        public delegate void LogCallback(string tag, string message);
    }
}
