using H5.Http;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace H5.API;
public sealed class ApiController : IRouteMatcher {
    public readonly FileServer FileHandler;

    public ApiController() {
        // TODO Get the directory location of wwwroot from settings
        string fileServerRootPath = System.Environment.ProcessPath ?? Path.GetFullPath(Path.Join(".", "wwwroot"));
        this.FileHandler = new(fileServerRootPath, "/");
    }

    public IRequestHandler? MatchRoute(HttpListenerRequest request) {


        return this.FileHandler;
    }
}
