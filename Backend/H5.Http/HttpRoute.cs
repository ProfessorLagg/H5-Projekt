﻿using System.Runtime.CompilerServices;

namespace H5.Http;
public sealed record class HttpRoute : IComparable<HttpRoute> {
	public static readonly HttpRoute Root = new HttpRoute("/");

	public HttpStdMethod Method;
	public string Path;
	public ParsedUri Uri => new ParsedUri(this.Path);
	private static string NormalizePath(string path) {
		string r = path.ToLowerInvariant().Trim().TrimEnd('/');
		if (r.Length == 0 || r[0] != '/') {
			r = '/' + r;
		}

		return r;
	}
	public HttpRoute(ParsedUri uri, HttpStdMethod method = HttpStdMethodExt.ANY) {
		this.Method = method;
		this.Path = NormalizePath(uri.Path);
	}
	public HttpRoute(string url, HttpStdMethod method = HttpStdMethodExt.ANY) : this(new ParsedUri(url), method) { }

	private static bool PathMatch(HttpRoute a, HttpRoute b) {
		if (a.Path.Length != b.Path.Length) {
			return false;
		}

		return a.Path.Equals(b.Path, StringComparison.InvariantCultureIgnoreCase);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool MethodMatch(HttpRoute a, HttpRoute b) { return a.Method.SharesFlag(b.Method); }
	public bool Match(HttpRoute other) {
		if (ReferenceEquals(this, other)) { throw new ArgumentException("Cannot match a route with itself"); }
		return PathMatch(this, other) && MethodMatch(this, other);
	}
	public bool Match(string url, HttpStdMethod method = HttpStdMethodExt.ANY) { return this.Match(new HttpRoute(url, method)); }

	public override string ToString() {
		return $"[{this.Method.ToString()}]{this.Path}";
	}

	public int CompareTo(HttpRoute? other) {
		if (other is null) { return -1; }
		unchecked {
			int cmpMethod = Math.Sign(this.Method.Compare(other.Method));
			int cmpPath = Math.Sign(StringComparer.InvariantCultureIgnoreCase.Compare(this.Path, other.Path)) * 10;
			int cmp = cmpMethod + cmpPath;
			return Math.Sign(cmp);
		}
	}

	/// <summary>Removes this <see cref="HttpRoute.Path"/> from the input path</summary>
	public string GetSubPath(string path) {
		ReadOnlySpan<char> result = NormalizePath(path);
		int sliceAt = int.Max(0, result.IndexOf(this.Path));
		return new string(result.Slice(sliceAt));
	}
}

