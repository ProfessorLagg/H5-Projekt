using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5.Http;
public sealed class RouteMatcher {
    public record class RouteNode {
        public string RouteFragment = string.Empty;
        public IRequestHandler? Handler { get; private set; } = null;
        public readonly List<RouteNode> ChildNodes = new();
    }

    private RouteNode RootNode = new();

}
