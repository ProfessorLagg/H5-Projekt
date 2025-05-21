using System.Collections.Specialized;

namespace H5.Lib.Utils;
/// <summary>Utilities and extentions for collections</summary>
public static class CollectionUtils {
	/// <summary>
	/// Creates a <see cref="StringDictionary"/> from an <c>IEnumerable&lt;KeyValuePair&lt;string, string&gt;&gt;</c> according to the default comparer for the key type.
	/// </summary>
	/// <returns>A <see cref="StringDictionary"/> that contains keys and values from <paramref name="keyValuePairs"/> and uses default comparer for the key type.</returns>
	public static StringDictionary ToStringDictionary(this IEnumerable<KeyValuePair<string, string>> keyValuePairs) {
		StringDictionary result = new();
		foreach (KeyValuePair<string, string> kvp in keyValuePairs) {
			result[kvp.Key] = kvp.Value;
		}
		return result;
	}
}
