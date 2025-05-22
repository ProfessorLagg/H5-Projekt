
using H5.Lib.Logging;
using H5.Lib.Utils;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;

namespace H5.Http;
/// <summary><see cref="IRequestHandler"/> than returns static files from a specified directory and url path root.</summary>
public sealed class FileServer : IRequestHandler {
	#region Static Data
	private static LogScope Logger = new LogScope(typeof(FileServer).FullName);
	#endregion

	#region Instance Data
	public readonly DirectoryInfo RootDirectory;
	public readonly HttpRoute Route;
	public readonly CacheSettings CacheConfig;
	#endregion

	#region Constructors
	/// <summary>Creates a new FileServer request handler</summary>
	/// <param name="rootDirectory">The root directory where the file server looks for files</param>
	/// <param name="rootRoute">The root route that the file server is handling</param>
	/// <param name="cacheConfig">File content cache settings. If <see langword="null"/> uses <see cref="CacheSettings.Default"/></param>
	public FileServer(DirectoryInfo rootDirectory, HttpRoute? rootRoute = null, CacheSettings? cacheConfig = null) {
		if (!rootDirectory.ValidatePath()) { throw new ArgumentException($"\"{rootDirectory.FullName}\" is not a valid directory path"); }
		if (!rootDirectory.Exists) { throw new ArgumentException($"Could not find or access directory at \"{rootDirectory.FullName}\""); }
		this.RootDirectory = rootDirectory;
		this.Route = rootRoute ?? HttpRoute.Root;
		this.CacheConfig = cacheConfig ?? CacheSettings.Default;
		if (this.CacheConfig.EnableCache) { this.CleanCacheAction = this.CleanCache; }
	}
	/// <summary>Creates a new FileServer request handler</summary>
	/// <param name="rootDirectoryPath">The root directory where the file server looks for files</param>
	public FileServer(string rootDirectoryPath) : this(new DirectoryInfo(rootDirectoryPath)) { }
	/// <summary>Creates a new FileServer request handler</summary>
	/// <param name="rootDirectoryPath">The root directory where the file server looks for files</param>
	/// <param name="httpRoute">The root route that the file server is handling</param>
	public FileServer(string rootDirectoryPath, string httpRoute) : this(new DirectoryInfo(rootDirectoryPath), new HttpRoute(httpRoute)) { }
	/// <summary>Creates a new FileServer request handler</summary>
	/// <param name="rootDirectoryPath">The root directory where the file server looks for files</param>
	/// <param name="cacheConfig">File content cache settings</param>
	public FileServer(string rootDirectoryPath, CacheSettings cacheConfig) : this(new DirectoryInfo(rootDirectoryPath), null, cacheConfig) { }
	/// <summary>Creates a new FileServer request handler</summary>
	/// <param name="rootDirectoryPath">The root directory where the file server looks for files</param>
	/// <param name="httpRoute">The root route that the file server is handling</param>
	/// <param name="cacheConfig">File content cache settings</param>
	public FileServer(string rootDirectoryPath, string httpRoute, CacheSettings cacheConfig) : this(new DirectoryInfo(rootDirectoryPath), new HttpRoute(httpRoute), cacheConfig) { }
	#endregion

	#region Caching
	public record class CacheSettings {
		public static CacheSettings Default = new();
		/// <summary>If the file server should cache file content</summary>
		public bool EnableCache = true;
		/// <summary>The maximum size of cached files in bytes</summary>
		public int MaxFileSize = 4096;
		/// <summary>The maximum time a file lives in the cache without being touched</summary>
		public TimeSpan CacheLifetime = TimeSpan.FromMinutes(10);
	}
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
	private readonly ConcurrentDictionary<string, DateTime> LastTouchedCache = new();
	private readonly ConcurrentDictionary<string, FileCacheValue> FileContentCache = new();
	private DateTime NextClean = DateTime.MinValue;

	/// <summary>Cleans the FileContentCache if enough time has passed since the previous clean</summary>
	private void CleanCache() {
		if (this.NextClean <= DateTime.Now) { return; }
		this.NextClean = DateTime.Now + this.CacheConfig.CacheLifetime;
		List<string> removeKeys = new();
		foreach (KeyValuePair<string, DateTime> kvp in this.LastTouchedCache) {
			if ((DateTime.Now - kvp.Value) > this.CacheConfig.CacheLifetime) {
				removeKeys.Add(kvp.Key);
			}
			else if (kvp.Value < this.NextClean) {
				this.NextClean = kvp.Value;
			}
		}
		foreach (string k in CollectionsMarshal.AsSpan(removeKeys)) {
			_ = this.LastTouchedCache.Remove(k, out _);
			_ = this.FileContentCache.Remove(k, out _);
			Logger.Debug($"Removed {k} from file cache");
		}
	}
	private readonly Action CleanCacheAction = () => { };
	private byte[]? ReadFileCached(FileInfo file) {
		if (file.Length > this.CacheConfig.MaxFileSize) { return null; }
		FileCacheValue? cacheValue;
		if (this.FileContentCache.TryGetValue(file.FullName, out cacheValue) && cacheValue is not null) {
			cacheValue.UpdateContentIfNeed(file);
		}
		else {
			cacheValue = new FileCacheValue(file);
			this.FileContentCache[file.FullName] = cacheValue;
		}
		Debug.Assert(cacheValue is not null);
		this.LastTouchedCache[file.FullName] = DateTime.Now;
		this.CleanCacheAction.Invoke();
		return cacheValue.Content;
	}
	#endregion

	#region Request Handling
	private void WriteResponse(HttpListenerContext context, FileInfo file) {
		byte[]? cacheContent = this.ReadFileCached(file);
		if (cacheContent is not null) {
			context.WriteResponse(cacheContent, HttpUtils.GetMimeType(file.Extension), HttpStatusCode.OK);
		}
		else {
			using FileStream fileStream = file.OpenRead();
			context.WriteResponse(fileStream, HttpUtils.GetMimeType(file.Extension), HttpStatusCode.OK);
		}
	}
	public void Handle(HttpListenerContext context) {
		ArgumentNullException.ThrowIfNullOrWhiteSpace(context.Request.RawUrl);
		if (context.Request.Url is null) { throw new RouteNotFoundException(context); }
		string routeString = context.Request.Url.LocalPath;
		string requestFilePath = this.Route.GetSubPath(routeString);
		requestFilePath = requestFilePath.Replace('/', Path.DirectorySeparatorChar);
		requestFilePath = Path.Join(this.RootDirectory.FullName, requestFilePath);
		FileInfo requestFile = new(requestFilePath);
		requestFile.AssertValidPath();
		//requestFile.AssertExists();

		this.WriteResponse(context, requestFile);
	}
	#endregion
}
