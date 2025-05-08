using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace H5.Http;
public interface IMiddleware {
    /// <summary>
    /// HTTP Middleware
    /// </summary>
    /// <returns>True if handling of the request should continue, otherwise false</returns>
    bool Handle(HttpListenerContext context);
}
