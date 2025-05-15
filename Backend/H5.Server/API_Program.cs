using H5.Http;
using H5.Lib;
namespace H5.API;
internal class API_Program {
    static void Main(string[] args) {
        ApiSettings.Load();
        Console.Write($"Loaded settings:\n{ApiSettings.ToIniFile()}\n");
        ApiSettings.Validate();

        ApiController controller = new();
        HttpServer server = new(controller, null);

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