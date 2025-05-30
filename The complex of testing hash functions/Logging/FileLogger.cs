using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace The_complex_of_testing_hash_functions.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _filePath;
        private static readonly object _lock = new();

        public FileLogger(string categoryName, string filePath)
        {
            _categoryName = categoryName;
            _filePath = filePath;
        }

        public IDisposable BeginScope<TState>(TState state) => null!;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {_categoryName}: {formatter(state, exception)}";

            if (exception != null)
                logEntry += Environment.NewLine + $"EXCEPTION: {exception}";

            lock (_lock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
                File.AppendAllText(_filePath, logEntry + Environment.NewLine);
            }
        }

        public void LogSessionStart()
        {
            string separator = new string('=', 80);
            string startEntry = $"\n{separator}\n=== СТАРТ НОВОЙ СЕССИИ: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n{separator}\n";

            lock (_lock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
                File.AppendAllText(_filePath, startEntry);
            }
        }
    }
}
