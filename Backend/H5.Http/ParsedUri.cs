using System.Security;

namespace H5.Http;

/// <summary>Represents a uri in the standard format scheme ":" ["//" authority] path ["?" query] ["#" fragment]</summary>
public sealed record class ParsedUri : IEquatable<ParsedUri>, IEquatable<string> {
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
        get { return _Scheme; }
        set { _Scheme = value.ToLowerInvariant(); }
    }
    /// <summary>The UserInfo </summary>
    public string UserInfo {
        get { return $"{_UserName}:{_UserPassword}"; }
        set {
            string[] split = value.ToLowerInvariant().Split(':');
            switch (split.Length) {
                case 1:
                    _UserName = split[0];
                    _UserPassword = string.Empty;
                    break;
                case 2:
                    _UserName = split[0];
                    _UserPassword = string.Empty;
                    break;
                default:
                    throw new ArgumentException("Invalid UserInfo component string");
            }
        }
    }
    public string Host {
        get { return _Host; }
        set { _Host = value.ToLowerInvariant(); }
    }
    public string Port {
        get { return _Port; }
        set { _Port = value.ToLowerInvariant(); }
    }
    public string Path {
        get { return _Path; }
        set { _Path = value.ToLowerInvariant(); }
    }
    public string Query {
        get { return _Query; }
        set { _Query = value.ToLowerInvariant(); }
    }
    public string Fragment {
        get { return _Fragment; }
        set { _Fragment = value.ToLowerInvariant(); }
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

    public static ParsedUri Parse(string url) {
        ParsedUri result = new ParsedUri();

        int scheme_end = url.IndexOf(':');
        if (scheme_end >= 0) {
            result.Scheme = url.Substring(0, scheme_end);
            url = url.Substring(scheme_end + 1);
        }

        /// Authority component
        if (url.StartsWith("//")) {
            url = url.Substring(2);
            int authority_end = url.IndexOf('/');
            string authority_string = url.Substring(0, authority_end);
            url = url.Substring(authority_end);
            result.ParseAuthorityString(authority_string);
        }

        int fragment_start = url.IndexOf('#');
        if (fragment_start >= 0) {
            result.Fragment = url.Substring(fragment_start + 1);
            url = url.Substring(0, fragment_start);
        }

        int query_start = url.IndexOf('?');
        if (query_start >= 0) {
            result.Query = url.Substring(query_start + 1);
            url = url.Substring(0, query_start);
        }

        result.Path = url.Trim('/');

        return result;
    }

    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        // Scheme
        if (this._Scheme.Length > 0) {
            sb.Append(this.Scheme);
            sb.Append("://");
        }
        // UserInfo
        if (this._UserInfo.Length > 0) {
            sb.Append(this.UserInfo);
            sb.Append('@');
        }
        // Host
        sb.Append(this._Host);
        // Port
        if (this._Port.Length > 0) {
            sb.Append(':');
            sb.Append(this.Port);
        }
        // Path
        if (this._Path.Length > 0) {
            sb.Append('/');
            sb.Append(this.Path);
        }
        // Query
        if (this._Query.Length > 0) {
            sb.Append("?");
            sb.Append(this.Query);
        }
        // Fragment
        if (this._Fragment.Length > 0) {
            sb.Append('#');
            sb.Append(this._Fragment);
        }
        return sb.ToString();
    }

    public bool Equals(string? other) {
        if (other == null) { return false; }
        // TODO
        throw new NotImplementedException();
    }
}

