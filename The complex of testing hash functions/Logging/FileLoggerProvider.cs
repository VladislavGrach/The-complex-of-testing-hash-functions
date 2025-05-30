namespace The_complex_of_testing_hash_functions.Logging
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _filePath;
        private readonly Dictionary<string, FileLogger> _loggers = new();

        public FileLoggerProvider(string filePath)
        {
            _filePath = filePath;
            // Записываем маркер старта сессии при создании провайдера
            LogStartupMarker();
        }

        public ILogger CreateLogger(string categoryName)
        {
            if (!_loggers.TryGetValue(categoryName, out var logger))
            {
                logger = new FileLogger(categoryName, _filePath);
                _loggers[categoryName] = logger;
            }

            return logger;
        }

        private void LogStartupMarker()
        {
            var startupLogger = new FileLogger("Startup", _filePath);
            startupLogger.LogSessionStart();
        }

        public void Dispose() { }
    }
}