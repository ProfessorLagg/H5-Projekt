using H5.Lib;
using H5.Lib.Logging;
using H5.Lib.Utils;

using System.Collections;



namespace H5.API;
/// <summary>Handles settings for the API</summary>
public static class ApiSettings {
	/// <inheritdoc/>
	public class InvalidSettingException : Exception {
		/// <inheritdoc cref = "Exception(string?)" />
		public InvalidSettingException(string? message) : base(message) { }
		/// <summary>Throws a new <see cref="InvalidSettingException"/> if true</summary>
		/// <param name="v">Value indicating wether to throw or not</param>
		/// <param name="message"><inheritdoc cref="InvalidSettingException(string?)"/></param>
		public static void ThrowIf(bool v, string? message = null) { if (v) { throw new InvalidSettingException(message); } }
	}
	#region Settings File
	private static string SettingsFilePath = Path.Join(PathUtils.ExeDirectory.FullName, "ApiSettings.ini");
	private static FileInfo SettingsFileInfo = new(SettingsFilePath);
	private static IniDocument SettingsFile = null!;

	/// <summary>Validates the current state of <see cref="ApiSettings"/>. Throws errors for invalid settings.</summary>
	public static void Validate() {
		HTTP.Validate();
		Logging.Validate();
	}
	private static bool LoadCalled = false;
	/// <summary>Clones current <see cref="ApiSettings"/> to dictionary</summary>
	/// <returns>Clone of current <see cref="ApiSettings"/> in the format &lt;section, &lt;key, value&gt;&gt;</returns>
	public static IDictionary<string, IDictionary<string, string>> ToInstance() {
		Dictionary<string, IDictionary<string, string>> result = new();
		result[HTTPSettings.SectionName] = HTTP.ToDictionary();
		result[LoggingSettings.SectionName] = Logging.ToDictionary();
		return result;
	}
	/// <summary>Overwrites all values in <see cref="ApiSettings"/> with defaults</summary>
	public static void LoadDefault() {
		_HTTP = new();
		_Logging = new();
		LoadCalled = true;
	}
	/// <summary>Loads <see cref="ApiSettings"/> from file <see cref="ApiSettings.SettingsFileInfo"/></summary>
	public static void Load() {
		SettingsFile = IniDocument.Load(SettingsFileInfo);
		LoadCalled = true;
	}
	/// <summary>Clones current <see cref="ApiSettings"/> to <see cref="IniDocument"/></summary>
	public static IniDocument ToIniFile() {
		IniDocument result = new();
		result.SetSection(HTTPSettings.SectionName, HTTP);
		result.SetSection(LoggingSettings.SectionName, Logging);
		return result;
	}
	/// <summary>Saves current <see cref="ApiSettings"/> to file <see cref="ApiSettings.SettingsFileInfo"/></summary>
	public static void Save() {
		SettingsFile = ToIniFile();
		SettingsFile.Save(SettingsFilePath);
	}
	#endregion

	#region HTTP
	/// <summary>HTTP settings section</summary>
	public sealed class HTTPSettings : IEnumerable<KeyValuePair<string, string>> {
		/// <summary><see cref="IniDocument"/> section header for <see cref="HTTPSettings"/></summary>
		public const string SectionName = "HTTP";

		/// <summary>Should the API server respond to http:// requests</summary>
		public bool EnableHttp = true;
		/// <summary>The port the API server listsens for http:// requests on</summary>
		public ushort PortHttp = 80;

		/// <summary>Should the API server respond to https:// requests</summary>
		public bool EnableHttps = true;
		/// <summary>The port the API server listsens for https:// requests on</summary>
		public ushort PortHttps = 443;

		/// <summary>Hostnames the API server listens for requests on ex. localhost, casa-blanca.com, *</summary>
		public string[] HostNames = new string[] { "*" };

		/// <summary>
		/// Parses the input key/value pairs to a new instance of <see cref="HTTPSettings"/>.
		/// Only parses known keys, and does not check for unkown keys.
		/// Missing keys will be set to default values
		/// </summary>
		/// <param name="kvps">key/value pairs to parse</param>
		public static HTTPSettings Parse(in IDictionary<string, string> kvps) {
			HTTPSettings r = new();
			if (kvps.TryGetValue("EnableHttp", out string? sEnableHttp)) { r.EnableHttp = bool.Parse(sEnableHttp); }
			if (kvps.TryGetValue("PortHttp", out string? sPortHttp)) { r.PortHttp = ushort.Parse(sPortHttp); }
			if (kvps.TryGetValue("EnableHttps", out string? sEnableHttps)) { r.EnableHttps = bool.Parse(sEnableHttps); }
			if (kvps.TryGetValue("PortHttps", out string? sPortHttps)) { r.PortHttps = ushort.Parse(sPortHttps); }
			if (kvps.TryGetValue("HostNames", out string? sHostNames)) {
				r.HostNames = sHostNames.Split(',');
				for (int i = 0; i < r.HostNames.Length; i++) { r.HostNames[i] = r.HostNames[i].Trim().ToLowerInvariant(); }
			}


			return r;
		}

		/// <inheritdoc/>
		public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
			yield return new KeyValuePair<string, string>("EnableHttp", this.EnableHttp.ToString().ToLowerInvariant());
			yield return new KeyValuePair<string, string>("PortHttp", this.PortHttp.ToString());
			yield return new KeyValuePair<string, string>("EnableHttps", this.EnableHttps.ToString().ToLowerInvariant());
			yield return new KeyValuePair<string, string>("PortHttps", this.PortHttps.ToString());
			yield return new KeyValuePair<string, string>("HostNames", string.Join(',', this.HostNames));
		}
		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }

		/// <summary>Validates the state of this <see cref="HTTPSettings"/>. Throws <see cref="InvalidSettingException"/> for invalid values.</summary>
		public void Validate() {
			InvalidSettingException.ThrowIf(!(this.EnableHttp || this.EnableHttps), "Atleast one of HTTP.EnableHttp or HTTP.EnableHttps must be enabled");
			InvalidSettingException.ThrowIf(this.HostNames.Length == 0, "You must define atleast 1 host name in HTTP.HostNames");
			InvalidSettingException.ThrowIf(this.EnableHttp && this.EnableHttps && this.PortHttp == this.PortHttps, $"http:// and https:// cannot both use port {this.PortHttp}");
		}
	}

	private static HTTPSettings? _HTTP = null;
	/// <summary><inheritdoc cref="HTTPSettings"/></summary>
	public static HTTPSettings HTTP {
		get {
			if (!LoadCalled) { throw new Exception("Load has not been called"); }
			_HTTP ??= HTTPSettings.Parse(SettingsFile.GetSection(HTTPSettings.SectionName));
			return _HTTP;
		}
	}
	#endregion

	#region Logging
	/// <summary>Logging settings section</summary>
	public sealed record class LoggingSettings : IEnumerable<KeyValuePair<string, string>> {
		/// <summary><see cref="IniDocument"/> section header for <see cref="LoggingSettings"/></summary>
		public const string SectionName = "Logging";

		///<summary>Lowest level of logs that get printed to console and generic log file</summary>
		public LogLevel MaxLogLevel = LogLevel.Info;
		/// <summary>Enables/Disables writing generic log to console</summary>
		public bool LogToConsole = true;
		/// <summary>Enables/Disables writing generic log to file</summary>
		public bool LogToFile = false;
		/// <summary>Directory logfiles are placed in when <see cref="LogToFile"/> is enabled</summary>
		public string LogDirPath = Path.Join(PathUtils.ExeDirectory.FullName, "Log");
		/// <summary>Enables/Disables writing W3 Extended Log File Format to file</summary>
		public bool LogW3 = false;
		/// <summary>Directory logfiles are placed in when <see cref="LogW3"/> is enabled</summary>
		public string LogW3DirPath = Path.Join(PathUtils.ExeDirectory.FullName, "LogW3");
		/// <summary>
		/// Parses the input key/value pairs to a new instance of <see cref="LoggingSettings"/>.
		/// Only parses known keys, and does not check for unkown keys.
		/// Missing keys will be set to default values
		/// </summary>
		/// <param name="kvps">key/value pairs to parse</param>
		public static LoggingSettings Parse(IDictionary<string, string> kvps) {
			LoggingSettings r = new();
			if (kvps.TryGetValue("MaxLogLevel", out string? sMinimumLoggingLevel)) { r.MaxLogLevel = Enum.Parse<LogLevel>(sMinimumLoggingLevel); }
			if (kvps.TryGetValue("LogToConsole", out string? sLogToConsole)) { r.LogToConsole = bool.Parse(sLogToConsole); }
			if (kvps.TryGetValue("LogToFile", out string? sLogToFile)) { r.LogToFile = bool.Parse(sLogToFile); }
			if (kvps.TryGetValue("LogDirPath", out string? sLogDirPath)) { r.LogDirPath = string.IsNullOrWhiteSpace(sLogDirPath) ? r.LogDirPath : sLogDirPath; }
			if (kvps.TryGetValue("LogW3", out string? sLogW3)) { r.LogW3 = bool.Parse(sLogW3); }
			if (kvps.TryGetValue("LogW3DirPath", out string? sLogW3DirPath)) { r.LogW3DirPath = string.IsNullOrWhiteSpace(sLogW3DirPath) ? r.LogW3DirPath : sLogW3DirPath; }

			return r;
		}
		/// <inheritdoc/>
		public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
			yield return new KeyValuePair<string, string>("MaxLogLevel", this.MaxLogLevel.ToString());
			yield return new KeyValuePair<string, string>("LogToFile", this.LogToFile.ToString().ToLowerInvariant());
			yield return new KeyValuePair<string, string>("LogDirPath", this.LogDirPath.ToString());
			yield return new KeyValuePair<string, string>("LogW3", this.LogW3.ToString().ToLowerInvariant());
			yield return new KeyValuePair<string, string>("LogW3DirPath", this.LogW3DirPath.ToString());
		}
		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
		/// <summary>Validates the state of this <see cref="LoggingSettings"/>.
		/// Throws <see cref="InvalidSettingException"/> for invalid values.</summary>
		public void Validate() {
			DirectoryInfo LogDir = new(this.LogDirPath);
			InvalidSettingException.ThrowIf(this.LogToFile && !LogDir.ValidatePath(), "Could not find or access Logging.LogDirPath");

			DirectoryInfo LogW3Dir = new(this.LogW3DirPath);
			InvalidSettingException.ThrowIf(this.LogW3 && !LogW3Dir.ValidatePath(), "Could not find or access Logging.LogW3DirPath");
		}
	}
	private static LoggingSettings? _Logging = null;
	/// <summary><inheritdoc cref="LoggingSettings"/></summary>
	public static LoggingSettings Logging {
		get {
			if (!LoadCalled) { throw new Exception("Load has not been called"); }
			_Logging ??= LoggingSettings.Parse(SettingsFile.GetSection("Logging"));

			return _Logging;
		}
	}
	#endregion

	#region FileServer
	/// <summary>FileServer settings section</summary>
	public sealed class FileServerSettings : IEnumerable<KeyValuePair<string, string>> {
		/// <summary><see cref="IniDocument"/> section header for <see cref="FileServerSettings"/></summary>
		public const string SectionName = "FileServer";
		/// <summary>Directory path to static content.Defaults to {exe directory}\wwwroot if empty or missing</summary>
		public string ContentRoot = Path.Join(PathUtils.ExeDirectory.FullName, "wwwroot");
		/// <summary>Toggles the file server in-memory cache. Defaults to true if not specified</summary>
		public bool EnableCaching = true;
		/// <summary>Maximum size of cached files in bytes. Defaults to 4096 if not specified</summary>
		public uint MaxCacheFileSize = 4096;
		/// <summary>Seconds a file can go untouched in the cache before it gets cleaned up. Defaults to 600 if not specified</summary>
		public uint CacheLifetimeSeconds = 600;

		/// <summary>
		/// Parses the input key/value pairs to a new instance of <see cref="FileServerSettings"/>.
		/// Only parses known keys, and does not check for unkown keys.
		/// Missing keys will be set to default values
		/// </summary>
		/// <param name="kvps">key/value pairs to parse</param>
		public static FileServerSettings Parse(IDictionary<string, string> kvps) {
			FileServerSettings r = new();
			if (kvps.TryGetValue("ContentRoot", out string? sContentRoot)) { r.ContentRoot = string.IsNullOrWhiteSpace(sContentRoot) ? r.ContentRoot : sContentRoot; }
			if (kvps.TryGetValue("EnableCaching", out string? sEnableCaching)) { r.EnableCaching = string.IsNullOrWhiteSpace(sEnableCaching) ? r.EnableCaching : bool.Parse(sEnableCaching); }
			if (kvps.TryGetValue("MaxCacheFileSize", out string? sMaxCacheFileSize)) { r.MaxCacheFileSize = string.IsNullOrWhiteSpace(sMaxCacheFileSize) ? r.MaxCacheFileSize : uint.Parse(sMaxCacheFileSize); }
			if (kvps.TryGetValue("CacheLifetimeSeconds", out string? sCacheLifetimeSeconds)) { r.CacheLifetimeSeconds = string.IsNullOrWhiteSpace(sCacheLifetimeSeconds) ? r.CacheLifetimeSeconds : uint.Parse(sCacheLifetimeSeconds); }
			return r;
		}
		/// <summary>Validates the state of this <see cref="FileServerSettings"/>.
		/// Throws <see cref="InvalidSettingException"/> for invalid values.</summary>
		public void Validate() {
			InvalidSettingException.ThrowIf(!Directory.Exists(this.ContentRoot), $"Could not find or access HTTP.ContentRoot directory \"{this.ContentRoot}\"");
		}
		/// <inheritdoc/>
		public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
			yield return new KeyValuePair<string, string>("ContentRoot", this.ContentRoot.ToString());
		}
		IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
	}
	private static FileServerSettings? _FileServer = null;
	/// <inheritdoc cref="FileServerSettings"/>
	public static FileServerSettings FileServer {
		get {
			if (!LoadCalled) { throw new Exception("Load has not been called"); }
			_FileServer ??= FileServerSettings.Parse(SettingsFile.GetSection(FileServerSettings.SectionName));
			return _FileServer;
		}
	}
	#endregion
}
