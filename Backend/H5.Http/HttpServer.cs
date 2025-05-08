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
    private readonly Dictionary<HttpRoute, IRequestHandler> Handlers = new();
    private readonly IRequestErrorHandler ErrorHandler;
    private object DefitionLock = new();
    private ILogger Logger;
    public bool Running { get; private set; } = false;


    public HttpServer(IRequestErrorHandler errorHandler, ILogger? logger) {
        this.ErrorHandler = errorHandler;

        this.Logger = logger ?? NullLogger.Instance;
    }
    public HttpServer() : this(new DefaultErrorHandler(), null) { }
    public HttpServer(ILogger logger) : this(new DefaultErrorHandler(), logger) { }

    /// <summary>
    /// Adds a request handler to the <see cref="HttpServer"/>, and binds it to a specific route.
    /// Fails if the route is already mapped, or if the server is running
    /// </summary>
    /// <param name="route">The route to handle</param>
    /// <param name="handler">The handler to add</param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public void AddHandler(HttpRoute route, IRequestHandler handler) {
        if (this.Running) { throw new InvalidOperationException("Cannot edit handlers while server is running"); }
        lock (DefitionLock) {
            if (!Handlers.TryAdd(route, handler)) {
                throw new ArgumentException($"Cannot add duplicate of route {route}");
            }
        }
    }
    /// <summary>
    /// Adds a request handler to the <see cref="HttpServer"/>, and binds it to a specific route.
    /// Fails if the route is already mapped, or if the server is running
    /// </summary>
    /// <param name="route">The route to handle</param>
    /// <param name="handler">The handler to add</param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public void AddHandler(string route, IRequestHandler handler) { this.AddHandler(new HttpRoute(route), handler); }

    // TODO Summary
    public void AddIncomingMiddleWare(IMiddleware middleware) {
        if (this.Running) { throw new InvalidOperationException("Cannot edit middleware while server is running"); }
        lock (DefitionLock) {
            this.IncomingMiddleware.Append(middleware);
        }
    }

    // TODO Summary
    public void AddOutgoingMiddleWare(IMiddleware middleware) {
        if (this.Running) { throw new InvalidOperationException("Cannot edit middleware while server is running"); }
        lock (DefitionLock) {
            this.OutgoingMiddleware.Append(middleware);
        }
    }


    private IRequestHandler? MapRoute(HttpListenerRequest request) {
        foreach (HttpRoute route in this.Handlers.Keys) {
            if (route.Match(request.RawUrl ?? "")) {
                return this.Handlers[route];
            }
        }

        return null;
    }
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

        IRequestHandler? handler = this.MapRoute(context.Request);
        if (handler is null) {
            this.ErrorHandler.Handle(context, HttpStatusCode.NotFound);
        }
        else {
            handler.Handle(context);
        }

        for (int i = 0; i < this.OutgoingMiddleware.Count; i++) {
            if (!this.OutgoingMiddleware[i].Handle(context)) return;
        }
    }


    /// <summary>
    /// Blocks the calling thread to run the WebServer
    /// </summary>
    public void Run() {
        this.Running = true;
        lock (DefitionLock) {
            HttpListener listener = new();
            listener.Prefixes.Add("http://localhost:80/");
            listener.Start();
            while (listener.IsListening) {
                HttpListenerContext context = listener.GetContext();
                try {
                    HandleRequest(context);
                }
                catch (Exception e) {
                    this.Logger.LogWarning(e.ToString());
                    this.ErrorHandler.Handle(context, HttpStatusCode.InternalServerError);
                }
                LogResponse(context.Response);
                context.Response.Close();
            }
            if (listener.IsListening) { listener.Stop(); }
            listener.Close();
        }
    }
}
