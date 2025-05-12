using H5.Http;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using H5.Lib.Utils;
namespace H5.API;
public sealed class ApiController : IRouteMatcher {
    public readonly FileServer FileHandler = new(ApiSettings.HTTP.ContentRoot, "/");


    public IRequestHandler? MatchRoute(HttpListenerRequest request) {
        if (request.RawUrl is null) { return null; }
        if (request.RawUrl == @"/") { return new RedirectionHandler("/index.html"); }

        return this.FileHandler;
    }
}
