using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace H5.Http;
public static class HttpExtentions {
    /// <summary>Sets both the <see cref="HttpListenerResponse.StatusCode"/> and <see cref="HttpListenerResponse.StatusDescription"/> fields</summary>
    public static void SetStatus(this HttpListenerResponse response, HttpStatusCode statusCode) {
        response.StatusCode = (int)statusCode;
        response.StatusDescription = statusCode.ToString();
    }

    public static void AppendContent(this HttpListenerResponse response, byte[] bytes) {
        ReadOnlySpan<byte> bytesSpan = bytes;
        response.ContentLength64 += bytesSpan.Length;
        response.OutputStream.Write(bytesSpan);
    }
    public static void Html(this HttpListenerResponse response, string html, HttpStatusCode statusCode = HttpStatusCode.OK) {
        response.SetStatus(statusCode);
        response.ContentType = "text/html";
        response.AppendContent(Encoding.UTF8.GetBytes(html));
    }

    public static string GetHTTPVersionString(this HttpListenerResponse response) {
        Version version = response.ProtocolVersion;
        return $"HTTP/{version.Major}.{version.Minor}";
    }
    public static string GetHTTPVersionString(this HttpListenerRequest request) {
        Version version = request.ProtocolVersion;
        return $"HTTP/{version.Major}.{version.Minor}";
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
