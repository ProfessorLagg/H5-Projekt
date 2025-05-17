using System.Net;

namespace H5.Http;
public sealed class HelloWorldHandler : IRequestHandler {
	public void Handle(HttpListenerContext context) {
		context.WriteHtml("<span>Hello World!<span>");
	}
}
