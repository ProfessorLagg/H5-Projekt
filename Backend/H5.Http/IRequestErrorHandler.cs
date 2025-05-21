using System.Net;

namespace H5.Http;
/// <summary>
/// Request handler for Errors
/// </summary>
public interface IRequestErrorHandler {
	/// <summary>
	/// Writes error response to the input <see cref="HttpListenerContext"/>
	/// </summary>
	/// <param name="context"><see cref="HttpListenerContext"/> to write error to</param>
	/// <param name="statusCode"><see cref="HttpStatusCode"/> to write</param>
	void Handle(HttpListenerContext context, HttpStatusCode statusCode);
}
