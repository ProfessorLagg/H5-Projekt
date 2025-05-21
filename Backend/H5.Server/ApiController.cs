using H5.Http;

using System.Net;
namespace H5.API;

/// <summary>Handles routing for the API</summary>
public sealed class ApiController : IRouteMatcher {
	private readonly FileServer FileHandler = new(ApiSettings.FileServer.ContentRoot, "/");
	private readonly RedirectionHandler RedirectToIndexHandler = new RedirectionHandler("/index.html");
	private readonly BenchmarkHandler BenchmarkHandler = new BenchmarkHandler();

	/// <summary>
	/// Matches request
	/// </summary>
	/// <param name="request"></param>
	/// <returns></returns>
	public IRequestHandler? MatchRoute(HttpListenerRequest request) {
		if (request.RawUrl is null) { return null; }
		if (request.RawUrl == @"/") { return RedirectToIndexHandler; }
		if (request.RawUrl.Equals(@"/benchmark", StringComparison.InvariantCultureIgnoreCase)) { return BenchmarkHandler; }

		return this.FileHandler;
	}
}
