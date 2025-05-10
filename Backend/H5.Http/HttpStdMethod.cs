using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace H5.Http;

/// <summary>
/// Standard HTTP Methods.
/// HTTP Actually allows you to write whatever you want in the method field
/// </summary>
[Flags]
public enum HttpStdMethod : ushort {
    GET = 0x0001,
    HEAD = 0x0002,
    OPTIONS = 0x0004,
    TRACE = 0x0008,
    PUT = 0x0010,
    DELETE = 0x0020,
    POST = 0x0040,
    PATCH = 0x0080,
    CONNECT = 0x0100,
}

public static class HttpStdMethodExt {
    public const HttpStdMethod ANY = HttpStdMethod.GET | HttpStdMethod.HEAD | HttpStdMethod.OPTIONS | HttpStdMethod.TRACE | HttpStdMethod.PUT | HttpStdMethod.DELETE | HttpStdMethod.POST | HttpStdMethod.PATCH | HttpStdMethod.CONNECT;
    /// <summary>Checks if 2 <see cref="HttpStdMethod"/> enums shares atleast 1 flag</summary>
    public static bool SharesFlag(this HttpStdMethod @this, HttpStdMethod other) {
        return ((ushort)@this & (ushort)other) > 0;
    }

    /// <summary>Returns the singluar <see cref="HttpStdMethod"/>s that make up this <see cref="HttpStdMethod"/></summary>
    public static HttpStdMethod[] Components(this HttpStdMethod @this) {
        ushort x = (ushort)@this;
        int popcount = BitOperations.PopCount(x);
        HttpStdMethod[] result = new HttpStdMethod[popcount];
        int i = 0;
        foreach (HttpStdMethod method in Enum.GetValues<HttpStdMethod>()) {
            if (@this.HasFlag(method)) {
                result[i++] = method;
            }
        }
        return result;
    }

    public static int Compare(this HttpStdMethod @this, HttpStdMethod other) {
        int a = (int)((ushort)@this);
        int b = (int)((ushort)other);
        return int.Sign(unchecked(a - b));
    }

    public static HttpStdMethod Parse(string str) {
        str = str.ToUpperInvariant();
        foreach (HttpStdMethod method in Enum.GetValues<HttpStdMethod>()) {
            if (method.ToString().Equals(str)) return method;
        }
        throw new Exception($"\"{str}\" could not be parsed as a standard HTTP method");
    }
}
