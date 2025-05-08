using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace H5.Http;
public interface IRequestErrorHandler {
    void Handle(HttpListenerContext context, HttpStatusCode statusCode);
}
