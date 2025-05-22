using System.Net;

namespace H5.Http;
public sealed class RedirectionHandler : IRequestHandler {
	public readonly string RedirectTo;
	public RedirectionHandler(string redirectTo) { this.RedirectTo = redirectTo; }
	public void Handle(HttpListenerContext context) {
		context.Response.Redirect(this.RedirectTo);
	}
}
