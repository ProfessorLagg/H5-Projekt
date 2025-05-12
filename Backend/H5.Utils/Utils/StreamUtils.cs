using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5.Lib.Utils;
public static class StreamUtils {
    public static IEnumerable<string> ReadLines(this StreamReader sr) {
        while (!sr.EndOfStream) {
            string? line = sr.ReadLine();
            if (line is not null) yield return line!;
        }
    }
    public static async Task<IList<string>> ReadLinesAsync(this StreamReader sr) {
        List<string> result = new();
        while (!sr.EndOfStream) {
            string? line = await sr.ReadLineAsync();
            if (line is not null) result.Add(line!);
        }

        return result;
    }
    /// <summary>
    /// Check that 2 streams contain exactly the same bytes.
    /// Does not reset positions
    /// </summary>
    public static bool SameContent(this Stream streamA, Stream streamB) {
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
