using H5.Http;

using System.Net;
namespace H5.API;

/// <inheritdoc/>
public sealed class ApiController : IRouteMatcher {
	private readonly FileServer FileHandler = new(ApiSettings.FileServer.ContentRoot, "/");
	private readonly RedirectionHandler RedirectToIndexHandler = new RedirectionHandler("/index.html");
	private readonly BenchmarkHandler BenchmarkHandler = new BenchmarkHandler();

	/// <inheritdoc/>
	public IRequestHandler? MatchRoute(HttpListenerRequest request) {
		if (request.RawUrl is null) { return null; }
		if (request.RawUrl == @"/") { return this.RedirectToIndexHandler; }
		if (request.RawUrl.Equals(@"/benchmark", StringComparison.InvariantCultureIgnoreCase)) { return this.BenchmarkHandler; }

		return this.FileHandler;
	}
}
