using System.Collections.Concurrent;

namespace H5.Lib.Utils;
/// <summary>Utilities and extentions for <see cref="Enum"/></summary>
public static class EnumUtils {
	private static ConcurrentDictionary<Enum, string> ToStringCache = new();
	private static string ToStringFactory(Enum e) { return e.ToString(); }
	/// <summary>
	/// <inheritdoc cref="Enum.ToString()"/>
	/// Caches results to minimize usage of reflection
	/// </summary>
	/// <inheritdoc cref="Enum.ToString()"/>
	public static string ToStringCached<T>(this T @this) where T : Enum {
		return ToStringCache.GetOrAdd(@this, ToStringFactory);
	}
}
