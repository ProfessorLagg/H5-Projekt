using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5.Utils.AsyncUtils;
public static partial class Utils {
    public static T Sync<T>(this Task<T> task) {
        task.Wait();
        return task.Result;
    }
}
