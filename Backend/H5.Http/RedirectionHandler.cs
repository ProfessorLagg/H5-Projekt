using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace H5.Http;
public sealed class RedirectionHandler : IRequestHandler {
    public readonly string RedirectTo;
    public RedirectionHandler(string redirectTo) { this.RedirectTo = redirectTo; }
    public void Handle(HttpListenerContext context) {
        context.Response.Redirect(RedirectTo);
    }
}
