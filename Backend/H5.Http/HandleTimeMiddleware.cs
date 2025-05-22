using H5.Lib.Logging;
using H5.Lib.Utils;

using System.Net;

namespace H5.Http;
public sealed class HandleTimeServer {
	private const string HandleTimeHeaderName = @"X-HandleTime";
	private const string ReceivedHeaderName = @"X-Received";
	private static readonly TimeSpan MaxNormalTime = TimeSpan.FromSeconds(1);
	private static LogScope Logger = new LogScope("HandleTimeServer");
	public sealed class IncommingHandler : IMiddleware {
		public bool Handle(HttpListenerContext context) {
			context.Request.Headers[ReceivedHeaderName] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffffzzz");
			return true;
		}
	}

	public sealed class OutgoingHandler : IMiddleware {
		public bool Handle(HttpListenerContext context) {
			DateTime recieved = DateTime.Parse(context.Request.Headers[ReceivedHeaderName]!).ToUniversalTime();
			TimeSpan handleTime = DateTime.UtcNow - recieved;
			context.Response.Headers[HandleTimeHeaderName] = handleTime.ToLargestUnitString();
			if (handleTime > MaxNormalTime) {
				Logger.Warn($"HandleTime exceeded {MaxNormalTime.ToLargestUnitString()}");
			}
			return true;
		}
	}

	public readonly IncommingHandler InHandler = new();
	public readonly OutgoingHandler OutHandler = new();


}
