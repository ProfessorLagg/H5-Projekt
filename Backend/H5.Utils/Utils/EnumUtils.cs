using System.Collections.Concurrent;

namespace H5.Lib.Utils;
public static class EnumUtils {
	private static ConcurrentDictionary<Enum, string> ToStringCache = new();
	private static string ToStringFactory(Enum e) { return e.ToString(); }
	public static string ToStringCached<T>(this T @this) where T : Enum {
		return ToStringCache.GetOrAdd(@this, ToStringFactory);
	}
}
