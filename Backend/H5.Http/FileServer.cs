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

    public FileServer(DirectoryInfo rootDirectory) {
        if (!rootDirectory.ValidatePath()) throw new ArgumentException($"\"{rootDirectory.FullName}\" is not a valid directory path");
        if (!rootDirectory.Exists) throw new ArgumentException($"Could not find or access directory at \"{rootDirectory.FullName}\"");
        this.RootDirectory = rootDirectory;
    }
    public FileServer(string rootDirectoryPath) : this(new DirectoryInfo(rootDirectoryPath)) { }


    public void Handle(HttpListenerContext context, HttpRoute? route) {
        string requestFilePath = (route ?? HttpRoute.Root).GetSubPath(route is null ? "/" : route.Path);
        requestFilePath = requestFilePath.Replace('/', Path.DirectorySeparatorChar);
        requestFilePath = Path.Join(this.RootDirectory.FullName, requestFilePath);
        FileInfo requestFile = new(requestFilePath);
        requestFile.AssertValidPath();
        requestFile.AssertExists();

        context.Response.File(requestFile);
    }
}
