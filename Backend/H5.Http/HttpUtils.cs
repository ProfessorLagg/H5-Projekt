using H5.Http;
using H5.Lib.Utils;

using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using System.Text;

namespace H5.Lib.HttpUtils;
public static class HttpUtils {
	/// <summary>Sets both the <see cref="HttpListenerResponse.StatusCode"/> and <see cref="HttpListenerResponse.StatusDescription"/> fields</summary>
	public static void SetStatus(this HttpListenerResponse response, HttpStatusCode statusCode) {
		response.StatusCode = (int)statusCode;
		response.StatusDescription = statusCode.ToStringCached();
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
		rs.Read(bytes, 0, bytes.Length);
		return bytes;
	}
	public static string RawString(this HttpListenerRequest request) {
		ReadOnlySpan<byte> bytes = request.RawBytes();
		return Encoding.UTF8.GetString(bytes);
	}

	public static void File(this HttpListenerResponse response, FileInfo file) {
		using FileStream readStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
		response.ContentLength64 = readStream.Length;
		response.SendChunked = false;
		response.ContentType = HttpUtils.GetMimeType(file.Extension);
		readStream.CopyTo(response.OutputStream);
		response.SetStatus(HttpStatusCode.OK);
	}

	// https://developer.mozilla.org/en-US/docs/Web/HTTP/Guides/MIME_types/Common_types
	private static StringDictionary ExtentionMimeTypeBinding = ParseBaseMimeTypes().ToStringDictionary();
	private static IEnumerable<KeyValuePair<string, string>> ParseBaseMimeTypes() {
		const string ResourceName = "H5.Http.BaseMimeTypes.csv";
		const char SplitChar = ';';
		Assembly asm = typeof(HttpUtils).Assembly;
		string[] resourceNames = asm.GetManifestResourceNames();
		using Stream resourceStream = asm.GetManifestResourceStream(ResourceName) ?? throw new Exception($"Could not find or open resource \"{ResourceName}\"");
		using StreamReader sr = new(resourceStream, Encoding.UTF8, true);
		foreach (string line in sr.ReadLines()) {
			int splitIndex = line.IndexOf(SplitChar);
			if (splitIndex >= 0) {
				yield return new KeyValuePair<string, string>(line.Substring(0, splitIndex), line.Substring(splitIndex + 1));
			}
		}
	}
	public static string GetMimeType(string fileExtention) {
		const string DefaultMimeType = "text/plain";
		fileExtention = fileExtention.ToLowerInvariant();
		return ExtentionMimeTypeBinding[fileExtention] ?? DefaultMimeType;
	}

	public static ParsedUri ParseUri(this HttpListenerRequest request) {
		if (request.Url is not null) return new ParsedUri(request.Url.ToString());
		if (request.RawUrl is not null) return new ParsedUri(request.RawUrl);
		return ParsedUri.Empty;
	}
}
