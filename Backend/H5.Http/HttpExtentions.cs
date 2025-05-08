using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace H5.Http;
public static class HttpExtentions {
    public static void AppendContent(this HttpListenerResponse response, byte[] bytes) {
        ReadOnlySpan<byte> bytesSpan = bytes;
        response.ContentLength64 += bytesSpan.Length;
        response.OutputStream.Write(bytesSpan);
    }
    public static void Html(this HttpListenerResponse response, string html) {
        response.StatusCode = (int)HttpStatusCode.OK;
        response.ContentType = "text/html";
        response.AppendContent(Encoding.UTF8.GetBytes(html));
    }

    public static byte[] RawBytes(this HttpListenerRequest request) {
        Stream rs = request.InputStream;
        rs.Seek(0, SeekOrigin.Begin);
        byte[] bytes = new byte[rs.Length];
        rs.Read(bytes.AsSpan());
        return bytes;
    }
    public static string RawString(this HttpListenerRequest request) {
        ReadOnlySpan<byte> bytes = request.RawBytes();
        return Encoding.UTF8.GetString(bytes);
    }
}
