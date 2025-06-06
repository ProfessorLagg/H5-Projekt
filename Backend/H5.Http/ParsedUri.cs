﻿using System.Text;

namespace H5.Http;

/// <summary>Represents a uri in the standard format scheme ":" ["//" authority] path ["?" query] ["#" fragment]</summary>
public sealed record class ParsedUri : IEquatable<ParsedUri>, IEquatable<string> {
	public static readonly ParsedUri Empty = new ParsedUri();

	// https://da.wikipedia.org/wiki/Uniform_Resource_Identifier
	private string _Scheme = string.Empty;
	private string _UserName = string.Empty;
	private string _UserPassword = string.Empty; // TODO This should be some form of secure string
	private string _Host = string.Empty;
	private string _Port = string.Empty;
	private string _Path = string.Empty;
	private string _Query = string.Empty;
	private string _Fragment = string.Empty;

	/// <summary>The scheme component of the URI. ex. 'http''</summary>
	public string Scheme {
		get => this._Scheme; set => this._Scheme = value.ToLowerInvariant();
	}
	/// <summary>The UserInfo </summary>
	public string UserInfo {
		get => $"{this._UserName}:{this._UserPassword}";
		set {
			string[] split = value.ToLowerInvariant().Split(':');
			switch (split.Length) {
				case 1:
					this._UserName = split[0];
					this._UserPassword = string.Empty;
					break;
				case 2:
					this._UserName = split[0];
					this._UserPassword = string.Empty;
					break;
				default:
					throw new ArgumentException("Invalid UserInfo component string");
			}
		}
	}
	public string Host {
		get => this._Host; set => this._Host = value.ToLowerInvariant();
	}
	public string Port {
		get => this._Port; set => this._Port = value.ToLowerInvariant();
	}
	public string Path {
		get => this._Path; set => this._Path = value.ToLowerInvariant();
	}
	public string Query {
		get => this._Query; set => this._Query = value.ToLowerInvariant();
	}
	public string Fragment {
		get => this._Fragment; set => this._Fragment = value.ToLowerInvariant();
	}

	public ParsedUri() { }
	public ParsedUri(string url) : this() {
		// TODO Handle URL Encoding
		int scheme_end = url.IndexOf(':');
		if (scheme_end >= 0) {
			this.Scheme = url.Substring(0, scheme_end);
			url = url.Substring(scheme_end + 1);
		}

		// Authority component
		if (url.StartsWith("//")) {
			url = url.Substring(2);
			int authority_end = url.IndexOf('/');
			string authority_string = url.Substring(0, authority_end);
			url = url.Substring(authority_end);
			this.ParseAuthorityString(authority_string);
		}

		int fragment_start = url.IndexOf('#');
		if (fragment_start >= 0) {
			this.Fragment = url.Substring(fragment_start + 1);
			url = url.Substring(0, fragment_start);
		}

		int query_start = url.IndexOf('?');
		if (query_start >= 0) {
			this.Query = url.Substring(query_start + 1);
			url = url.Substring(0, query_start);
		}

		this.Path = url.Trim('/');
	}

	private void ParseAuthorityString(string authstr) {
		authstr = authstr.TrimStart('/').TrimStart('/');
		int userinfo_end = authstr.IndexOf('@');
		if (userinfo_end >= 0) {
			this.UserInfo = authstr.Substring(0, userinfo_end);
			authstr = authstr.Substring(userinfo_end + 1);
		}

		string[] hostport_split = authstr.Split(':');
		this.Host = hostport_split[0];
		if (hostport_split.Length > 1) { this.Port = hostport_split[1]; }
	}

	public override string ToString() {
		StringBuilder sb = new StringBuilder();
		// Scheme
		if (this.Scheme.Length > 0) {
			_ = sb.Append(this.Scheme);
			_ = sb.Append("://");
		}
		// UserInfo
		if (this.UserInfo.Length > 0) {
			_ = sb.Append(this.UserInfo);
			_ = sb.Append('@');
		}
		// Host
		_ = sb.Append(this.Host);
		// Port
		if (this.Port.Length > 0) {
			_ = sb.Append(':');
			_ = sb.Append(this.Port);
		}
		// Path
		if (this.Path.Length > 0) {
			_ = sb.Append('/');
			_ = sb.Append(this.Path);
		}
		// Query
		if (this.Query.Length > 0) {
			_ = sb.Append("?");
			_ = sb.Append(this.Query);
		}
		// Fragment
		if (this.Fragment.Length > 0) {
			_ = sb.Append('#');
			_ = sb.Append(this.Fragment);
		}
		return sb.ToString();
	}
	public bool Equals(string? other) {
		if (other == null) { return false; }
		// TODO
		throw new NotImplementedException();
	}
}

