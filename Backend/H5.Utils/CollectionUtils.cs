using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5.Utils.CollectionUtils;
public static partial class Utils {
    public static StringDictionary ToStringDictionary(this IEnumerable<KeyValuePair<string, string>> keyValuePairs) {
        StringDictionary result = new();
        foreach (KeyValuePair<string, string> kvp in keyValuePairs) {
            result.Add(kvp.Key, kvp.Value);
        }
        return result;
    }
}
