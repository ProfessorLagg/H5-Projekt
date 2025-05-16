using System.Text;

namespace H5.Lib.Utils;
public static class StringUtils {
	private readonly struct EscapeSequence {
		public readonly string Find;
		public readonly string Replace;
		public EscapeSequence(string find, string replace) {
			this.Find = find;
			this.Replace = replace;
		}
	}
	private static readonly EscapeSequence[] WhitespaceEscapes = new EscapeSequence[] {
		new EscapeSequence("\r", @"\r"),
		new EscapeSequence("\n", @"\n"),
		new EscapeSequence("\t", @"\t"),
		new EscapeSequence("\f", @"\f"),
		new EscapeSequence("\v", @"\v"),
	};

	public static string EscapeWhitespace(this string str) {
		StringBuilder sb = new(str);
		sb.EscapeWhitespace();
		return sb.ToString();
	}
	public static void EscapeWhitespace(this StringBuilder sb) {
		foreach (EscapeSequence e in WhitespaceEscapes) {
			sb.Replace(e.Find, e.Replace);
		}
	}
}
