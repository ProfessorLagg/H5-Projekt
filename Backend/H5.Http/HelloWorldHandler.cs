using System.Net;

namespace H5.Http;
public sealed class HelloWorldHandler : IRequestHandler {
	public void Handle(HttpListenerContext context) {
		HttpListenerResponse response = context.Response;
		response.ContentType = "text/html";
		response.Html("<span>Hello World!<span>");
	}
}
