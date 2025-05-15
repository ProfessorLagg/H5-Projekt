using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Xml;

namespace H5.Http;
public sealed class HttpServer {
    private readonly List<IMiddleware> IncomingMiddleware = new();
    private readonly List<IMiddleware> OutgoingMiddleware = new();
    private readonly IRouteMatcher RouteMatcher;
    private readonly IRequestErrorHandler ErrorHandler;
    private readonly HttpListener Listener = new();
    private readonly ConcurrentQueue<HttpListenerContext> RequestQueue = new();

    private object DefitionLock = new();
    private Mutex DefinitionMutex = new();
    private ILogger Logger;
    public bool ShouldRun { get; private set; } = false;


    public HttpServer(IRouteMatcher routeMatcher, IRequestErrorHandler? errorHandler = null, ILogger? logger = null) {
        this.ErrorHandler = errorHandler ?? new DefaultErrorHandler();
        this.Logger = logger ?? NullLogger.Instance;
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
        // TODO Gotta have settings for this
        this.Logger.LogInformation($"Recieved request on {request.RawUrl ?? "unkown route"}");
    }
    private void LogResponse(HttpListenerResponse response) {
        const string spacer = "  ";
        LogLevel level = LogLevel.Information;
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
        this.Logger.Log(level, msgbuilder.ToString());
    }
    private void HandleRequest(HttpListenerContext context) {
        LogRequest(context.Request);
        for (int i = 0; i < this.IncomingMiddleware.Count; i++) {
            if (!this.IncomingMiddleware[i].Handle(context)) return;
        }

        IRequestHandler? mapResult = this.RouteMatcher.MatchRoute(context.Request);
        if (mapResult is null) {
            this.ErrorHandler.Handle(context, HttpStatusCode.NotFound);
        }
        else {
            mapResult.Handle(context);
        }

        for (int i = 0; i < this.OutgoingMiddleware.Count; i++) {
            if (!this.OutgoingMiddleware[i].Handle(context)) return;
        }
    }
    public void AddPrefix(string uriPrefix) {
        this.Listener.Prefixes.Add(uriPrefix);
    }

    private async Task Schedule() {
        while ((Listener.IsListening && this.ShouldRun) || this.RequestQueue.Count > 0) {
            if (this.RequestQueue.TryDequeue(out HttpListenerContext? context)) {
                if (context is null) {
                    this.Logger.LogWarning($"Found null context");
                    continue;
                }
                await Task.Run(() => {
                    try {
                        HandleRequest(context);
                    }
                    catch (Exception e) {
                        this.Logger.LogWarning(e.ToString());
                        if (context is not null) {
                            this.ErrorHandler.Handle(context, HttpStatusCode.InternalServerError);
                        }
                    }
                    finally {
                        if (context is not null) {
                            LogResponse(context.Response);
                            context.Response.OutputStream.Flush();
                            context.Response.Close();
                        }
                    }
                });

            }
        }
    }
    private async Task Listen() {
        this.ShouldRun = true;
        this.Listener.Start();
        string listening_msg = "Listening on:";
        foreach (var prefix in this.Listener.Prefixes) { listening_msg += "\n\t" + prefix; }
        this.Logger.LogInformation(listening_msg);

        while (Listener.IsListening && this.ShouldRun) {
            HttpListenerContext context = await Listener.GetContextAsync(); ;
            this.RequestQueue.Enqueue(context);


        }
        if (Listener.IsListening) { Listener.Stop(); }
        Listener.Close();

    }

    /// <summary>Blocks the calling thread to run the WebServer</summary>
    public void Run() {
        lock (DefitionLock) {
            var listenTask = this.Listen();
            var handleTask = this.Schedule();
            listenTask.Wait();
            handleTask.Wait();
        }
    }

    public void Stop() {
        this.ShouldRun = false;
        this.Listener.Stop();
    }
}
