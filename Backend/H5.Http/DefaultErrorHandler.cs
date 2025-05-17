using System.Net;

namespace H5.Http;
public class DefaultErrorHandler : IRequestErrorHandler {

	public DefaultErrorHandler() { }

	private void Empty(HttpListenerContext context, HttpStatusCode statusCode) {
		context.Response.SetStatus(statusCode);
	}

	private void Html(HttpListenerContext context, HttpStatusCode statusCode) {
		context.WriteHtml($"<h3>Error: {((int)statusCode).ToString("0")} - {statusCode.ToString()}</h3>", statusCode);
	}

	public void Handle(HttpListenerContext context, HttpStatusCode statusCode) {
		switch (context.Request.ContentType) {
			case "text/html": Html(context, statusCode); break;
			default: Empty(context, statusCode); break;
		}
	}
}