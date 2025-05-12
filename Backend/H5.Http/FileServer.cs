using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

//using System.Net.Mime;
using H5.Utils.PathUtils;

namespace H5.Http;
public sealed class FileServer : IRequestHandler {
    public readonly DirectoryInfo RootDirectory;
    public readonly HttpRoute Route;

    public FileServer(DirectoryInfo rootDirectory, HttpRoute? matchedRoute = null) {
        if (!rootDirectory.ValidatePath()) throw new ArgumentException($"\"{rootDirectory.FullName}\" is not a valid directory path");
        if (!rootDirectory.Exists) throw new ArgumentException($"Could not find or access directory at \"{rootDirectory.FullName}\"");
        this.RootDirectory = rootDirectory;
        this.Route = matchedRoute ?? HttpRoute.Root;
    }
    public FileServer(string rootDirectoryPath) : this(new DirectoryInfo(rootDirectoryPath)) { }
    public FileServer(string rootDirectoryPath, string httpRoute) : this(new DirectoryInfo(rootDirectoryPath), new HttpRoute(httpRoute)) { }


    public void Handle(HttpListenerContext context) {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(context.Request.RawUrl);
        string routeString = context.Request.RawUrl;
        string requestFilePath = this.Route.GetSubPath(routeString);
        requestFilePath = requestFilePath.Replace('/', Path.DirectorySeparatorChar);
        requestFilePath = Path.Join(this.RootDirectory.FullName, requestFilePath);
        FileInfo requestFile = new(requestFilePath);
        requestFile.AssertValidPath();
        requestFile.AssertExists();

        context.Response.File(requestFile);
    }
}
