using System.Diagnostics;

namespace H5.Lib.Utils;
/// <summary>Utilities and extentions for <see cref="System.IO.Stream"/></summary>
public static class StreamUtils {
#nullable enable
	/// <summary>Read lines iteratively from <paramref name="streamReader"/></summary>
	/// <param name="streamReader">The <see cref="StreamReader"/> to read lines from</param>
	/// <returns>Every non-null line returned from <see cref="StreamReader.ReadLine"/></returns>
	public static IEnumerable<string> ReadLines(this StreamReader streamReader) {
		while (!streamReader.EndOfStream) {
			string? line = streamReader.ReadLine();
			if (line is not null) yield return line!;
		}
	}
#nullable disable
	/// <summary>Check that 2 streams contain exactly the same bytes</summary>
	/// <remarks>Does not reset <see cref="Stream.Position"/> for <paramref name="streamA"/> and <paramref name="streamB"/></remarks>
	/// <param name="streamA"></param>
	/// <param name="streamB"></param>
	/// <returns><see langword="true"/> if <paramref name="streamA"/> and <paramref name="streamB"/> contain exactly the same bytes</returns>
	/// <exception cref="ArgumentException"></exception>
	public static bool EqualContent(this Stream streamA, Stream streamB) {
		if (streamA.Length != streamB.Length) return false;

		if (streamA.CanSeek && streamB.CanSeek) {
			streamA.Seek(0, SeekOrigin.Begin);
			streamB.Seek(0, SeekOrigin.Begin);
		}
		else if (streamA.Position != streamB.Position) {
			throw new ArgumentException("Streams must either be seekable or have the same starting position");
		}

		BufferedStream bufstreamA = new(streamA);
		BufferedStream bufstreamB = new(streamB);


		for (int i = 0; i < streamA.Length; i++) {
			if (bufstreamA.ReadByte() != bufstreamB.ReadByte()) return false;
		}
		return true;
	}
}
