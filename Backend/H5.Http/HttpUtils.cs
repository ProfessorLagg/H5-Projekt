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


	public static string GetHTTPVersionString(this HttpListenerResponse response) {
		Version version = response.ProtocolVersion;
		return $"HTTP/{version.Major}.{version.Minor}";
	}
	public static string GetHTTPVersionString(this HttpListenerRequest request) {
		Version version = request.ProtocolVersion;
		return $"HTTP/{version.Major}.{version.Minor}";
	}

	/// <summary>Gets the raw bytes of a request</summary>
	public static byte[] RawBytes(this HttpListenerRequest request) {
		Stream rs = request.InputStream;
		_ = rs.Seek(0, SeekOrigin.Begin);
		byte[] bytes = new byte[rs.Length];
		_ = rs.Read(bytes, 0, bytes.Length);
		return bytes;
	}
	/// <summary>Gets the raw bytes of a request as a string</summary>
	public static string RawString(this HttpListenerRequest request, Encoding encoding) {
		ReadOnlySpan<byte> bytes = request.RawBytes();
		return encoding.GetString(bytes);
	}
	/// <summary>Gets the raw bytes of a request as a string. Assumes UTF8 encoding</summary>
	public static string RawString(this HttpListenerRequest request) { return request.RawString(Encoding.UTF8); }


	// https://developer.mozilla.org/en-US/docs/Web/HTTP/Guides/MIME_types/Common_types
	private static StringDictionary ExtentionMimeTypeBinding = ParseBaseMimeTypes().ToStringDictionary();
	private static IEnumerable<KeyValuePair<string, string>> ParseBaseMimeTypes() {
		const string ResourceName = "H5.Http.BaseMimeTypes.csv";
		const char SplitChar = ';';
		Assembly asm = typeof(HttpUtils).Assembly;
		_ = asm.GetManifestResourceNames();
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
		if (request.Url is not null) {
			return new ParsedUri(request.Url.ToString());
		}

		if (request.RawUrl is not null) {
			return new ParsedUri(request.RawUrl);
		}

		return ParsedUri.Empty;
	}

	#region Response Shortcuts
	/// <summary>Responds with the content of a file</summary>
	public static void WriteFile(this HttpListenerContext context, FileInfo file, HttpStatusCode statusCode = HttpStatusCode.OK) {
		using FileStream readStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
		context.WriteResponse(readStream, HttpUtils.GetMimeType(file.Extension), statusCode);
	}
	/// <summary>Writes an HTML response</summary>
	public static void WriteHtml(this HttpListenerContext context, string html, HttpStatusCode statusCode = HttpStatusCode.OK) {
		context.WriteResponse(Encoding.UTF8.GetBytes(html), @"text/html", statusCode);
	}
	/// <summary>Writes bytes to the response. All content writing must go through this</summary>
	public static void WriteResponse(this HttpListenerContext context, Stream stream, string contentType = @"application/octet-stream", HttpStatusCode statusCode = HttpStatusCode.OK) {
		context.Response.SendChunked = false;
		context.Response.ContentType = contentType;
		context.Response.SetStatus(statusCode);
		if (context.Request.HttpMethod.Equals("HEAD")) { return; }

		context.Response.ContentLength64 += stream.Length;
		stream.CopyTo(context.Response.OutputStream);
	}
	/// <summary>Writes bytes to the response</summary>
	public static void WriteResponse(this HttpListenerContext context, byte[] bytes, string contentType = @"application/octet-stream", HttpStatusCode statusCode = HttpStatusCode.OK) {
		MemoryStream stream = new(bytes);
		context.WriteResponse(stream, contentType, statusCode);
	}
	#endregion
}
