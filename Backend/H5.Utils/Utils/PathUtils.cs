using System.IO;

namespace H5.Lib.Utils;

public static class PathUtils {
    private static char[] InvalidPathChars = Path.GetInvalidPathChars();
    private static char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

    public static FileInfo ExeFile => new FileInfo(System.Environment.ProcessPath);
    public static DirectoryInfo ExeDirectory => ExeFile.Directory!;
    public static bool ValidateDirectoryPath(ReadOnlySpan<char> path) {
        foreach (char c in path) {
            foreach (char invalidChar in InvalidPathChars) {
                if (c == invalidChar) return false;
            }
        }
        return true;
    }
    public static bool ValidateFileName(ReadOnlySpan<char> fileName) {
        foreach (char c in fileName) {
            foreach (char invalidChar in InvalidFileNameChars) {
                if (c == invalidChar) return false;
            }
        }
        return true;
    }
    public static bool ValidateFilePath(string filePath) {
        ReadOnlySpan<char> fullFilePath = Path.GetFullPath(filePath);
        ReadOnlySpan<char> fileName = Path.GetFileName(filePath);
        ReadOnlySpan<char> directoryPath = Path.GetDirectoryName(filePath) ?? "";

        return ValidateDirectoryPath(directoryPath) && ValidateFileName(fileName);
    }
    public static bool ValidatePath(this FileInfo file) { return ValidateFileName(file.Name) && file.Directory!.ValidatePath(); }
    public static bool ValidatePath(this DirectoryInfo directory) { return ValidateDirectoryPath(directory.FullName); }
    public static void AssertValidPath(this FileInfo file) {
        if (!file.ValidatePath()) throw new Exception($"\"{file.FullName}\" is not a valid file path");
    }
    public static void AssertValidPath(this DirectoryInfo directory) {
        if (!directory.ValidatePath()) throw new Exception($"\"{directory.FullName}\" is not a valid directory path");
    }
    public static void AssertExists(this FileInfo file) {
        if (!file.Exists) throw new Exception($"file at \"{file.FullName}\" did not exist, is not accisible or is not a file");
    }
    public static void AssertExists(this DirectoryInfo directory) {
        if (!directory.Exists) throw new Exception($"directory at \"{directory.FullName}\" did not exist, is not accisible or is not a directory");
    }
    public static void EnsureExists(this DirectoryInfo @this) {
        if (!@this.Exists) { @this.Create(); }
        if (!@this.Exists) { throw new DirectoryNotFoundException($"Could not create directory \"{@this.FullName}\""); }
    }
}
