using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace H5.Http {
    public class DefaultErrorHandler : IRequestErrorHandler {
        public void Handle(HttpListenerContext context, HttpStatusCode statusCode) {
            // TODO
            throw new NotImplementedException();
        }
    }
}
