using H5.Http;

using System.Net;
namespace H5.API;
public sealed class ApiController : IRouteMatcher {
	public readonly FileServer FileHandler = new(ApiSettings.FileServer.ContentRoot, "/");


	public IRequestHandler? MatchRoute(HttpListenerRequest request) {
		if (request.RawUrl is null) { return null; }
		if (request.RawUrl == @"/") { return new RedirectionHandler("/index.html"); }

		return this.FileHandler;
	}
}
