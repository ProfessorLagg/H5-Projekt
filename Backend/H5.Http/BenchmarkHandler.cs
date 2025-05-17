using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace H5.Http;
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Handle(HttpListenerContext context) {
		context.WriteResponse(Payload);
	}
}
