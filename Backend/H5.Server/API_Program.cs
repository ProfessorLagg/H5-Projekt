using H5.Http;
using H5.Lib.Logging;
namespace H5.API;
internal class API_Program {
	static void Main(string[] args) {
		ApiSettings.Load();
		ApiSettings.Validate();
#if DEBUG
		ApiSettings.Logging.LogToConsole = true;
#endif
		Console.Write($"Loaded settings:\n{ApiSettings.ToIniFile()}\n");
		if (ApiSettings.Logging.LogToConsole) { Log.AddConsoleLog(); }
		if (ApiSettings.Logging.LogToFile) { Log.AddFileLog(ApiSettings.Logging.LogDirPath); }
		ApiController controller = new();
		HandleTimeServer handleTimeServer = new();
		HttpServer server = new(controller, null);
		server.AddIncomingMiddleWare(handleTimeServer.InHandler);
		server.AddOutgoingMiddleWare(handleTimeServer.OutHandler);

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