using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5.Utils.StreamUtils;
public static partial class Utils {
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
}
