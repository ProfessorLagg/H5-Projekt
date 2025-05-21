using System.Net;
using System.Runtime.CompilerServices;

namespace H5.Http;

/// <summary><inheritdoc cref="BenchmarkHandler.Handle(HttpListenerContext)"/></summary>
/// <inheritdoc cref="IRequestHandler"/>
public sealed class BenchmarkHandler : IRequestHandler {
	const int KB = 1024;
	const int MB = 1024 * KB;
	const int GB = 1024 * MB;
	const int PayloadSize = 1 * KB;
	const int Seed = 2025_05_17;
	private static readonly byte[] Payload = BuildPayload();
	private static byte[] BuildPayload() {
		byte[] result = new byte[PayloadSize];
		Random rand = new(Seed);
		rand.NextBytes(result);
		return result;
	}

	/// <summary>Responds with static sized random bytes intented for performance testing</summary>
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Handle(HttpListenerContext context) {
		context.WriteResponse(Payload);
	}
}
