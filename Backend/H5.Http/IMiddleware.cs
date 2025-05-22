using System.Net;

namespace H5.Http;
/// <summary>
/// HTTP Middleware.
/// <see cref="HttpServer"/> can run these either before the request is handled or after
/// </summary>
public interface IMiddleware {
	/// <summary>
	/// Runs this <see cref="IMiddleware"/> on a <see cref="HttpListenerContext"/>
	/// </summary>
	/// <returns><see langword="true"/> if handling of the request should continue, otherwise <see langword="false"/></returns>
	bool Handle(HttpListenerContext context);
}
