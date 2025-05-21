using System.Net;

namespace H5.Http;

public interface IRouteMatcher {
	/// <summary>Matches <see cref="HttpListenerRequest"/> to <see cref="IRequestHandler"/></summary>
	/// <param name="request">The <see cref="HttpListenerRequest"/> to match a handler for</param>
	/// <returns>A <see cref="IRequestHandler"/> if one is found, otherwise null</returns>
	IRequestHandler? MatchRoute(HttpListenerRequest request);
}
