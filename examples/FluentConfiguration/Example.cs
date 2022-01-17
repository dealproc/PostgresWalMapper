namespace FluentConfiguration {
    using System;
    using System.Text.Json;

    using Microsoft.Extensions.Logging;

    using PGWalMapper;

    public class Example {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private IDisposable _listener;

        public Example(ILoggerFactory loggerFactory) {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<Example>();
        }

        public void Start() {
            var options = new JsonSerializerOptions { IncludeFields = true };
            
            _listener = new WalConfigurationBuilder("Host=127.0.0.1;Port=5432;Database=postgres;username=postgres;password=mysecretpassword;", _loggerFactory.CreateLogger<WalListener>())
                .ForPublication("films_pub").UsingSlot("films_slot")
                .OnInsert(o => _logger.LogInformation("'Object' OnInsert called."))
                .OnUpdate(o => _logger.LogWarning("'Object' OnUpdate called."))
                .OnDelete(o => _logger.LogCritical("'Object' OnDelete called."))
                
                .Map<AggregateMsgs.Films>().ToTable("films").InSchema("public")
                .Column("code").ToProperty(prop => prop.Code)
                .Column("title").ToProperty(prop => prop.Title)
                .Column("did").ToProperty(prop => prop.Did)
                .Column("date_prod").ToProperty(prop => prop.DateProduced)
                .Column("kind").ToProperty(prop => prop.Kind)
                .OnInsert(o => _logger.LogInformation($"Insert: {JsonSerializer.Serialize(o, options)}"))
                .OnUpdate(o => _logger.LogWarning($"Update: {JsonSerializer.Serialize(o, options)}"))
                .OnDelete(o => _logger.LogCritical($"Delete: {JsonSerializer.Serialize(o, options)}"))
                
                .Map<AggregateMsgs.Distributors>().ToTable("distributors").InSchema("public")
                .Column("did").ToProperty(prop => prop.Did)
                .Column("name").ToProperty(prop=> prop.Name)
                .OnInsert(o=> _logger.LogInformation($"Insert: {JsonSerializer.Serialize(o, options)}"))
                .OnUpdate(o => _logger.LogWarning($"Update: {JsonSerializer.Serialize(o, options)}"))
                .OnDelete(o => _logger.LogCritical($"Delete: {JsonSerializer.Serialize(o, options)}"))
             
                .Build().Connect();
            
            _logger.LogInformation("Listener waiting for events");
        }

        public void Stop() {
            _listener?.Dispose();
            _listener = null;
        }

        public void HandleError(Exception exc) {
            _logger.LogError(exc, "Application-wide failure occurred");
        }
    }
}