using System.Net;

namespace H5.Http;
/// <inheritdoc/>
public class DefaultErrorHandler : IRequestErrorHandler {

	private void Empty(HttpListenerContext context, HttpStatusCode statusCode) {
		context.Response.SetStatus(statusCode);
	}

	private void Html(HttpListenerContext context, HttpStatusCode statusCode) {
		context.WriteHtml($"<h3>Error: {((int)statusCode).ToString("0")} - {statusCode.ToString()}</h3>", statusCode);
	}

	/// <inheritdoc/>
	public void Handle(HttpListenerContext context, HttpStatusCode statusCode) {
		switch (context.Request.ContentType) {
			case "text/html": this.Html(context, statusCode); break;
			default: this.Empty(context, statusCode); break;
		}
	}
}