using H5.Http;

using System.Net;
namespace H5.API;
public sealed class ApiController : IRouteMatcher {
	public readonly FileServer FileHandler = new(ApiSettings.FileServer.ContentRoot, "/");
	public readonly RedirectionHandler RedirectToIndexHandler = new RedirectionHandler("/index.html");
	public readonly BenchmarkHandler BenchmarkHandler = new BenchmarkHandler();


	public IRequestHandler? MatchRoute(HttpListenerRequest request) {
		if (request.RawUrl is null) { return null; }
		if (request.RawUrl == @"/") { return RedirectToIndexHandler; }
		if (request.RawUrl.Equals(@"/benchmark", StringComparison.InvariantCultureIgnoreCase)) { return BenchmarkHandler; }

		return this.FileHandler;
	}
}
