using System.Globalization;
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
	public static string ToLargestUnitString(this TimeSpan timeSpan) => timeSpan.ToLargestUnitString(CultureInfo.InvariantCulture);
	public static string ToLargestUnitString(this TimeSpan timeSpan, IFormatProvider format) {
		double value = timeSpan.Ticks;
		string unit_str = " ticks";

		if (timeSpan.Ticks >= TimeSpan.TicksPerDay) {
			value = timeSpan.TotalDays;
			unit_str = " days";
		}
		else if (timeSpan.Ticks >= TimeSpan.TicksPerHour) {
			value = timeSpan.TotalHours;
			unit_str = " hours";
		}
		else if (timeSpan.Ticks >= TimeSpan.TicksPerSecond) {
			value = timeSpan.TotalSeconds;
			unit_str = "s";
		}
		else if (timeSpan.Ticks >= TimeSpan.TicksPerMillisecond) {
			value = timeSpan.TotalMilliseconds;
			unit_str = "ms";
		}
		else if (timeSpan.Ticks >= TimeSpan.TicksPerMicrosecond) {
			value = timeSpan.TotalMicroseconds;
			unit_str = "us";
		}
		else {
			value = timeSpan.TotalNanoseconds;
			unit_str = "ns";
		}

		return value.ToString("n", format) + unit_str;
	}
}
