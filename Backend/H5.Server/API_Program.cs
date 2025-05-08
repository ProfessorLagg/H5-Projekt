using H5.Http;

using Microsoft.Extensions.Logging;
namespace H5.API;
internal class API_Program {
    static void Main(string[] args) {
        // TODO this should be a setting
        LogLevel minLogLevel = LogLevel.Information;
#if DEBUG
        minLogLevel = LogLevel.Debug;
#endif
        using ILoggerFactory factory = LoggerFactory.Create(builder => 
            builder
            .AddConsole()
            .SetMinimumLevel(minLogLevel)

        );

        HttpServer server = new(factory.CreateLogger<HttpServer>());
        server.AddHandler("/", new HelloWorldHandler());

        server.Run();
    }
}