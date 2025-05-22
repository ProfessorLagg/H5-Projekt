using System.Net;
namespace H5.Http;

/// <summary>
/// <inheritdoc cref="IRequestHandler.Handle(HttpListenerContext)"/>
/// </summary>
public interface IRequestHandler {
	/// <summary>
	/// Handles <see cref="HttpListenerRequest"/>'s
	/// </summary>
	/// <param name="context"><see cref="HttpListenerContext"/> to handle request for</param>
	void Handle(HttpListenerContext context);
}
