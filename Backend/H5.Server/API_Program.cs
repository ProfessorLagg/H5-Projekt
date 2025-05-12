using H5.Http;
using H5.Lib;

using Microsoft.Extensions.Logging;
namespace H5.API;
internal class API_Program {
    static void Main(string[] args) {
        ApiSettings.Load();
        ApiSettings.Validate();

        using ILoggerFactory factory = LoggerFactory.Create(builder => {
            builder = builder.SetMinimumLevel(ApiSettings.Logging.MinimumLoggingLevel);
            if (ApiSettings.Logging.LogToConsole) { builder = builder.AddConsole(); }
            if (ApiSettings.Logging.LogToFile) { builder = builder.AddFileLogger(ApiSettings.Logging.LogDirPath); }
        });

        ApiController controller = new();

        HttpServer server = new(controller, null, factory.CreateLogger<HttpServer>());

        foreach (string hostName in ApiSettings.HTTP.HostNames) {
            if (ApiSettings.HTTP.EnableHttp) {
                string httpPrefix = "http://" + hostName + $":{ApiSettings.HTTP.PortHttp}/";
                server.AddPrefix(httpPrefix);
            }
            if (ApiSettings.HTTP.EnableHttps) {
                string httpsPrefix = "https://" + hostName + $":{ApiSettings.HTTP.PortHttps}/";
                server.AddPrefix(httpsPrefix);
            }
        }

        server.Run();
    }
}