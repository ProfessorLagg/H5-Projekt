using System.Net;

namespace H5.Http;
public interface IMiddleware {
	/// <summary>
	/// HTTP Middleware
	/// </summary>
	/// <returns>True if handling of the request should continue, otherwise false</returns>
	bool Handle(HttpListenerContext context);
}
