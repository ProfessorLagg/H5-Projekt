using System.Net;

namespace H5.Http;
public interface IRequestErrorHandler {
	void Handle(HttpListenerContext context, HttpStatusCode statusCode);
}
