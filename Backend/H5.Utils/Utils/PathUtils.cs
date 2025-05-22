namespace H5.Lib.Utils;

/// <summary>Utilities and extentions for file system pathing</summary>
public static class PathUtils {
	private static char[] InvalidPathChars = Path.GetInvalidPathChars();
	private static char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

	/// <summary>Exception that is thrown when an invalid path was found</summary>
	public sealed class InvalidPathException : Exception {
#nullable enable
		/// <summary>Initializes a new instance of the <see cref="InvalidPathException"/> with it's message set to <paramref name="message"/></summary>
		public InvalidPathException(string? message) : base(message) { }
#nullable disable
	}

	/// <summary>Returns <see cref="FileInfo"/> of the executable that started the currently executing process. Throws when not available.</summary>
	/// <remarks>
	/// <see cref="FileInfo"/> version of <see cref="System.Environment.ProcessPath"/>.
	/// Does not check that the file actually exists
	/// </remarks>
	public static FileInfo ExeFile {
		get {
#nullable enable
			string? processPath = Environment.ProcessPath;
			ArgumentNullException.ThrowIfNull(processPath);
			return new FileInfo(processPath);
#nullable disable
		}
	}

	/// <summary>
	/// Returns <see cref="DirectoryInfo"/> of the directory of the executable that started the currently executing process.
	/// Throws when not available.
	/// </summary>
	/// <remarks>
	/// Uses <see cref="PathUtils.ExeFile"/> to find the directory.
	/// Does not check that the directory actually exists.
	/// </remarks>
	public static DirectoryInfo ExeDirectory { get { return ExeFile.Directory!; } }

	/// <summary>Validates that a directory path does not contain invalid characters</summary>
	/// <remarks>The list of invalid characters is sourced from <see cref="System.IO.Path.GetInvalidPathChars()"/></remarks>
	/// <param name="path">The path to check</param>
	/// <returns><see langword="false"/> if <paramref name="path"/> contains invalid characters, otherwise <see langword="true"/></returns>
	public static bool ValidateDirectoryPath(ReadOnlySpan<char> path) {
		foreach (char c in path) {
			foreach (char invalidChar in InvalidPathChars) {
				if (c == invalidChar) return false;
			}
		}
		return true;
	}

	/// <summary>Validates that a file name does not contain invalid characters</summary>
	/// <remarks>The list of invalid characters is sourced from <see cref="System.IO.Path.GetInvalidFileNameChars()"/></remarks>
	/// <param name="fileName">The file name to check</param>
	/// <returns><see langword="false"/> if <paramref name="fileName"/> contains invalid characters, otherwise <see langword="true"/></returns>
	public static bool ValidateFileName(ReadOnlySpan<char> fileName) {
		foreach (char c in fileName) {
			foreach (char invalidChar in InvalidFileNameChars) {
				if (c == invalidChar) return false;
			}
		}
		return true;
	}

	/// <summary>Validates that a file path does not contain invalid characters</summary>
	/// <param name="filePath">The path to check</param>
	/// <returns><see langword="false"/> if <paramref name="filePath"/> contains invalid characters, otherwise <see langword="true"/></returns>
	public static bool ValidateFilePath(string filePath) { return new FileInfo(filePath).ValidatePath(); }

	/// <summary>Validates that the path of this <see cref="FileInfo"/> does not contain invalid characters</summary>
	/// <param name="file"><see cref="FileInfo"/> to check</param>
	/// <returns><see langword="false"/> if path of <paramref name="file"/> contains invalid characters, otherwise <see langword="true"/></returns>
	public static bool ValidatePath(this FileInfo file) { return ValidateFileName(file.Name) && file.Directory!.ValidatePath(); }

	/// <summary>Validates that the path of this <see cref="DirectoryInfo"/> does not contain invalid characters</summary>
	/// <param name="directory"><see cref="DirectoryInfo"/> to check</param>
	/// <returns><see langword="false"/> if path of <paramref name="directory"/> contains invalid characters, otherwise <see langword="true"/></returns>
	public static bool ValidatePath(this DirectoryInfo directory) { return ValidateDirectoryPath(directory.FullName); }

	/// <summary>Throws if <see cref="ValidatePath(FileInfo)"/> returns false</summary>
	/// <param name="file"><inheritdoc cref="ValidatePath(FileInfo)"/></param>
	/// <exception cref="InvalidPathException"></exception>
	public static void AssertValidPath(this FileInfo file) {
		if (!file.ValidatePath()) { throw new InvalidPathException($"\"{file.FullName}\" is not a valid file path"); }
	}

	/// <summary>Throws if <see cref="ValidatePath(DirectoryInfo)"/> returns false</summary>
	/// <param name="directory"><inheritdoc cref="ValidatePath(DirectoryInfo)"/></param>
	/// <exception cref="InvalidPathException"></exception>
	public static void AssertValidPath(this DirectoryInfo directory) {
		if (!directory.ValidatePath()) { throw new InvalidPathException($"\"{directory.FullName}\" is not a valid directory path"); }
	}

	/// <summary>Throws if <see cref="FileInfo.Exists"/> is <see langword="false"/></summary>
	/// <param name="file">The <see cref="FileInfo"/> to check</param>
	/// <exception cref="FileNotFoundException"></exception>
	public static void AssertExists(this FileInfo file) {
		if (!file.Exists) { throw new FileNotFoundException($"file at \"{file.FullName}\" did not exist, is not accisible or is not a file"); }
	}

	/// <summary>Throws if <see cref="DirectoryInfo.Exists"/> is <see langword="false"/></summary>
	/// <param name="directory">The <see cref="DirectoryInfo"/> to check</param>
	/// <exception cref="DirectoryNotFoundException"></exception>
	public static void AssertExists(this DirectoryInfo directory) {
		if (!directory.Exists) throw new DirectoryNotFoundException($"directory at \"{directory.FullName}\" did not exist, is not accisible or is not a directory");
	}

	/// <summary>
	/// Ensures that <paramref name="directory"/> exists by creating it if it doesn't.
	/// Throws if <paramref name="directory"/> could not be created
	/// </summary>
	/// <param name="directory">The <see cref="DirectoryInfo"/> to create</param>
	/// <exception cref="InvalidPathException"><inheritdoc cref="AssertValidPath(DirectoryInfo)"/></exception>
	/// <exception cref="DirectoryNotFoundException">Thrown if <paramref name="directory"/> still does not exist, even after attempted creation</exception>
	public static void EnsureExists(this DirectoryInfo directory) {
		directory.AssertValidPath();
		if (!directory.Exists) { directory.Create(); }
		directory.AssertExists();
	}
}
