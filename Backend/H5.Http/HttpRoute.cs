using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5.Http;
public sealed record class HttpRoute {
    public ParsedUri Uri { get; private init; }

    public HttpRoute(ParsedUri uri) {
        this.Uri = uri;
        this.Uri.Scheme = string.Empty;
        this.Uri.UserInfo = string.Empty;
        this.Uri.Host = string.Empty;
        this.Uri.Port = string.Empty;
        this.Uri.Path = string.Empty;
        this.Uri.Query = string.Empty;
        this.Uri.Fragment = string.Empty;
    }
    public HttpRoute(string url) : this(ParsedUri.Parse(url)) { }


    public bool Match(string url) {
        // TODO
        throw new NotImplementedException();
    }

    public override string ToString() {
        // TODO
        throw new NotImplementedException();
    }
}

