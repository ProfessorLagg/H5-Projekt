using H5.Lib.Logging;

using System.Net;
using System.Text;

namespace H5.Http;
public sealed class HttpServer {
	private static LogScope Logger = new LogScope(typeof(HttpServer).FullName);

	private readonly List<IMiddleware> IncomingMiddleware = new();
	private readonly List<IMiddleware> OutgoingMiddleware = new();
	private readonly IRouteMatcher RouteMatcher;
	private readonly IRequestErrorHandler ErrorHandler;
	private readonly HttpListener Listener = new();


	private object DefitionLock = new();
	public bool ShouldRun { get; private set; } = false;

	private const string LogScope = "HttpServer";

	public HttpServer(IRouteMatcher routeMatcher, IRequestErrorHandler? errorHandler = null) {
		this.ErrorHandler = errorHandler ?? new DefaultErrorHandler();
		this.RouteMatcher = routeMatcher;
	}

	// TODO Summary
	public void AddIncomingMiddleWare(IMiddleware middleware) {
		if (this.ShouldRun || this.Listener.IsListening) { throw new InvalidOperationException("Cannot edit middleware while server is running"); }
		lock (this.DefitionLock) {
			this.IncomingMiddleware.Add(middleware);
		}
	}

	// TODO Summary
	public void AddOutgoingMiddleWare(IMiddleware middleware) {
		if (this.ShouldRun || this.Listener.IsListening) { throw new InvalidOperationException("Cannot edit middleware while server is running"); }
		lock (this.DefitionLock) {
			this.OutgoingMiddleware.Add(middleware);
		}
	}

	private sealed record class MapRouteResult {
		public readonly HttpRoute Route;
		public readonly IRequestHandler Handler;
		public MapRouteResult(HttpRoute route, IRequestHandler handler) {
			this.Route = route;
			this.Handler = handler;
		}
		public void Handle(HttpListenerContext context) { this.Handler.Handle(context); }
	};

	private void LogRequest(HttpListenerRequest request) {
		const string spacer = "  ";
		StringBuilder msgbuilder = new();
		_ = msgbuilder.AppendLine("HTTP Request:");
		_ = msgbuilder.AppendLine($"{spacer}{request.HttpMethod} {request.RawUrl} {request.GetHTTPVersionString()}");
		for (int i = 0; i < request.Headers.Count; i++) {
			_ = msgbuilder.Append(spacer);
			_ = msgbuilder.Append(request.Headers.Keys[i] ?? "");
			_ = msgbuilder.Append(": ");
			_ = msgbuilder.AppendLine(request.Headers[i] ?? "");
		}
		Logger.Write(LogLevel.Info, msgbuilder.ToString());
	}
	private void LogResponse(HttpListenerResponse response) {
		const string spacer = "  ";
		LogLevel level = LogLevel.Info;
		if (response.StatusCode < 200 || response.StatusCode > 299) {
			level = LogLevel.Error;
		}
		StringBuilder msgbuilder = new();
		_ = msgbuilder.AppendLine("HTTP Response:");
		_ = msgbuilder.Append(spacer);
		_ = msgbuilder.AppendLine($"{response.GetHTTPVersionString()} {response.StatusCode.ToString()} {response.StatusDescription}");
		for (int i = 0; i < response.Headers.Count; i++) {
			_ = msgbuilder.Append(spacer);
			_ = msgbuilder.Append(response.Headers.Keys[i] ?? "");
			_ = msgbuilder.Append(": ");
			_ = msgbuilder.AppendLine(response.Headers[i] ?? "");
		}
		Logger.Write(level, msgbuilder.ToString());
	}
	private void HandleException(HttpListenerContext context, Exception e, HttpStatusCode? errorCode = null) {
		if (e is FileNotFoundException || e is DirectoryNotFoundException) {
			errorCode = HttpStatusCode.NotFound;
			Logger.Warn($"Could not match file to url: {context.Request.Url}");
			goto Respond;
		}
		// DefaultMessage
		Logger.Error($"Exception: {e} trying to handle request: {context.Request.Url}");
	Respond:
		if (context is null) { return; }
		try {
			this.ErrorHandler.Handle(context, errorCode ?? HttpStatusCode.InternalServerError);
		}
		catch (ObjectDisposedException) {
			Logger.Error("context was already disposed!");
		}
	}
	private void HandleRequest(HttpListenerContext context) {
		try {
			this.LogRequest(context.Request);
			for (int i = 0; i < this.IncomingMiddleware.Count; i++) {
				// Returning here still runs the finally block
				if (!this.IncomingMiddleware[i].Handle(context)) {
					return;
				}
			}

			IRequestHandler? mapResult = this.RouteMatcher.MatchRoute(context.Request);
			if (mapResult is null) {
				this.HandleException(context, new RouteNotFoundException(context));
				this.ErrorHandler.Handle(context, HttpStatusCode.NotFound);
			}
			else {
				mapResult.Handle(context);
			}

			for (int i = 0; i < this.OutgoingMiddleware.Count; i++) {
				// Returning here still runs the finally block
				if (!this.OutgoingMiddleware[i].Handle(context)) { return; }
			}
		}
		catch (Exception e) {
			this.HandleException(context, e);
#if DEBUG
			throw;
#endif
		}
		finally {
			if (context is not null) {
				this.LogResponse(context.Response);
				context.Response.OutputStream.Flush();
				context.Response.Close();
			}
		}
	}
	private void ScheduleRequest(HttpListenerContext context) {
		context.Response.SendChunked = false;
		context.Response.Headers.Add(@"Server", @""); // This is the only way to remove the Server field
		_ = ThreadPool.QueueUserWorkItem<HttpListenerContext>(this.HandleRequest, context, false);
	}
	public void AddPrefix(string uriPrefix) {
		this.Listener.Prefixes.Add(uriPrefix);
	}

	/// <summary>Blocks the calling thread to run the WebServer</summary>
	public void Run() {
		lock (this.DefitionLock) {
			this.ShouldRun = true;
			this.Listener.Start();
			string listening_msg = "Listening on:";
			foreach (string prefix in this.Listener.Prefixes) { listening_msg += "\n\t" + prefix; }
			Logger.Info(listening_msg);

			while (this.Listener.IsListening && this.ShouldRun) {
				HttpListenerContext context = this.Listener.GetContext();
				this.ScheduleRequest(context);
			}
			if (this.Listener.IsListening) { this.Listener.Stop(); }
			this.Listener.Close();
		}
	}

	public void Stop() {
		this.ShouldRun = false;
		this.Listener.Stop();
	}
}
