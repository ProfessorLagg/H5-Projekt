using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace H5.Http;
public class HttpException : Exception {
	public HttpException(string? message) : base(message) { }
}
public class RouteNotFoundException : HttpException {
	public RouteNotFoundException(string? message) : base(message) { }
	public RouteNotFoundException(Uri? uri) : this($"Could not find route: {(uri is null ? "null" : uri.AbsolutePath)}") { }
	public RouteNotFoundException(HttpListenerRequest request) : this(request.Url) { }
	public RouteNotFoundException(HttpListenerContext context) : this(context.Request) { }
}
