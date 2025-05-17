using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace H5.Lib.Utils;
public static class EnumUtils {
	private static ConcurrentDictionary<Enum, string> ToStringCache = new();
	private static string ToStringFactory(Enum e) { return e.ToString(); }
	public static string ToStringCached<T>(this T @this) where T : Enum {
		return ToStringCache.GetOrAdd(@this, ToStringFactory);
	}
}
