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
			if (line is not null) {
				yield return line!;
			}
		}
	}
}
