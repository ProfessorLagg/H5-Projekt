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
		if (this.ShouldRun) { throw new InvalidOperationException("Cannot edit middleware while server is running"); }
		lock (DefitionLock) {
			this.IncomingMiddleware.Append(middleware);
		}
	}

	// TODO Summary
	public void AddOutgoingMiddleWare(IMiddleware middleware) {
		if (this.ShouldRun) { throw new InvalidOperationException("Cannot edit middleware while server is running"); }
		lock (DefitionLock) {
			this.OutgoingMiddleware.Append(middleware);
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
		Logger.Write(LogLevel.Info, $"Recieved request on {request.RawUrl ?? "unkown route"}");
	}
	private void LogResponse(HttpListenerResponse response) {
		const string spacer = "  ";
		LogLevel level = LogLevel.Info;
		if (response.StatusCode < 200 || response.StatusCode > 299) {
			level = LogLevel.Error;
		}
		StringBuilder msgbuilder = new();
		msgbuilder.AppendLine("HTTP Response:");
		msgbuilder.Append(spacer);
		msgbuilder.AppendLine($"{response.GetHTTPVersionString()} {((int)response.StatusCode).ToString()} {response.StatusDescription}");
		for (int i = 0; i < response.Headers.Count; i++) {
			msgbuilder.Append(spacer);
			msgbuilder.Append(response.Headers.Keys[i] ?? "");
			msgbuilder.Append(": ");
			msgbuilder.AppendLine(response.Headers[i] ?? "");
		}
		Logger.Write(level, msgbuilder.ToString());
	}
	private void HandleException(HttpListenerContext context, Exception e) {
		if (e is FileNotFoundException || e is DirectoryNotFoundException) {
			Logger.Warn($"Could not match file to url: {context.Request.Url}");
			if (context is not null) {
				this.ErrorHandler.Handle(context, HttpStatusCode.NotFound);
			}
			return;
		}

		Logger.Error($"Exception: {e} trying to handle request: {context.Request.Url}");
		if (context is not null) {
			this.ErrorHandler.Handle(context, HttpStatusCode.InternalServerError);
		}
	}
	private void HandleRequest(HttpListenerContext context) {
		try {
			LogRequest(context.Request);
			for (int i = 0; i < this.IncomingMiddleware.Count; i++) {
				// Returning here still runs the finally block
				if (!this.IncomingMiddleware[i].Handle(context)) return;
			}

			IRequestHandler? mapResult = this.RouteMatcher.MatchRoute(context.Request);
			if (mapResult is null) { this.ErrorHandler.Handle(context, HttpStatusCode.NotFound); } else { mapResult.Handle(context); }

			for (int i = 0; i < this.OutgoingMiddleware.Count; i++) {
				// Returning here still runs the finally block
				if (!this.OutgoingMiddleware[i].Handle(context)) return;
			}
		}
		catch (Exception e) {
			HandleException(context, e);
		}
		finally {
			if (context is not null) {
				LogResponse(context.Response);
				context.Response.OutputStream.Flush();
				context.Response.Close();
			}
		}
	}
	public void AddPrefix(string uriPrefix) {
		this.Listener.Prefixes.Add(uriPrefix);
	}

	/// <summary>Blocks the calling thread to run the WebServer</summary>
	public void Run() {
		lock (DefitionLock) {
			this.ShouldRun = true;
			this.Listener.Start();
			string listening_msg = "Listening on:";
			foreach (var prefix in this.Listener.Prefixes) { listening_msg += "\n\t" + prefix; }
			Logger.Info(listening_msg);

			while (Listener.IsListening && this.ShouldRun) {
				HttpListenerContext context = Listener.GetContext();
				ThreadPool.QueueUserWorkItem<HttpListenerContext>(this.HandleRequest, context, false);
			}
			if (Listener.IsListening) { Listener.Stop(); }
			Listener.Close();
		}
	}

	public void Stop() {
		this.ShouldRun = false;
		this.Listener.Stop();
	}
}
