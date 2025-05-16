using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using H5.Lib.Utils;

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

    #region Caching
    /// <summary>Size in bytes of the largest file the FileServer will cache in memory</summary>
    private static readonly int MaxCachedFileSize = Environment.SystemPageSize;
    private record class FileCacheValue {
        public DateTime LastWriteTimeUtc = DateTime.MinValue;
        public byte[] Content = Array.Empty<byte>();

        public FileCacheValue(FileInfo file) {
            this.OverwriteContent(file, true);

        }

        private void OverwriteContent(FileInfo file, bool resize) {
            if (resize) { this.Content = new byte[file.Length]; }
            this.LastWriteTimeUtc = file.LastWriteTimeUtc;
            using FileStream fs = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            int readCount = fs.Read(this.Content);
            Debug.Assert(readCount == this.Content.Length, "Number of bytes read did not match size of buffer");
        }
        public void UpdateContentIfNeed(FileInfo file) {
            if (this.Content.Length != file.Length) {
                this.OverwriteContent(file, true);
            }
            else if (this.LastWriteTimeUtc != file.LastWriteTimeUtc) {
                this.OverwriteContent(file, false);
            }
        }
    }
    private readonly ConcurrentDictionary<string, FileCacheValue> FileContentCache = new();
    private byte[] ReadFileCached(FileInfo file) {
        if (file.Length > MaxCachedFileSize) { return file.ReadAllBytes(); }
        FileCacheValue? cacheValue = null;
        if (FileContentCache.TryGetValue(file.FullName, out cacheValue) && cacheValue is not null) {
            cacheValue.UpdateContentIfNeed(file);
        }
        else {
            cacheValue = new(file);
            FileContentCache[file.FullName] = cacheValue;
        }
        Debug.Assert(cacheValue is not null);
        return cacheValue.Content;
    }
    #endregion
    private void WriteResponse(HttpListenerResponse response, FileInfo file) {
        byte[] fileContent = this.ReadFileCached(file);
        response.ContentLength64 = fileContent.Length;
        response.SendChunked = false;
        response.ContentType = HttpUtils.GetMimeType(file.Extension);
        response.OutputStream.Write(fileContent);
        response.SetStatus(HttpStatusCode.OK);
    }
    public void Handle(HttpListenerContext context) {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(context.Request.RawUrl);
        string routeString = context.Request.RawUrl;
        string requestFilePath = this.Route.GetSubPath(routeString);
        requestFilePath = requestFilePath.Replace('/', Path.DirectorySeparatorChar);
        requestFilePath = Path.Join(this.RootDirectory.FullName, requestFilePath);
        FileInfo requestFile = new(requestFilePath);
        requestFile.AssertValidPath();
        requestFile.AssertExists();

        this.WriteResponse(context.Response, requestFile);
        //HttpUtils.File(context.Response, requestFile);
    }
}
