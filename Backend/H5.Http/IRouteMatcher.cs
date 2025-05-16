using System.Net;

namespace H5.Http;
public interface IRouteMatcher {
	IRequestHandler? MatchRoute(HttpListenerRequest request);
}
