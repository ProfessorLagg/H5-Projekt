using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5.Lib.Utils;
public static class CollectionUtils {
    public static StringDictionary ToStringDictionary(this IEnumerable<KeyValuePair<string, string>> keyValuePairs) {
        StringDictionary result = new();
        foreach (KeyValuePair<string, string> kvp in keyValuePairs) {
            result[kvp.Key] = kvp.Value;
        }
        return result;
    }

#nullable enable
    public static bool TryGetValue(this StringDictionary @this, string key, out string value) {
        string? v = @this[key];
        if (v is null) {
            value = "";
            return false;
        }
        value = v!;
        return true;
    }
#nullable disable
}
