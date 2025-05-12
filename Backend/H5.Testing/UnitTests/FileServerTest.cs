using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using H5.Http;
using H5.Utils.StreamUtils;
using H5.Utils.AsyncUtils;


namespace H5.Testing.UnitTests;
internal class FileServer : IUnitTest {
    public string GetName() { return typeof(Http.FileServer).FullName; }

    private static FileInfo[] CreateTestFiles(DirectoryInfo directory) {
        const int FileCount = 2;
        const int MinFileSize = 1;
        const int MaxFileSize = 64 * 1024;
        FileInfo[] result = new FileInfo[FileCount];
        Random rand = new();
        for (int i = 0; i < FileCount; i++) {
            string fileName = DateTime.Now.Ticks.ToString("X") + ".txt";
            string filePath = Path.Join(directory.FullName, fileName);
            int fileSize = rand.Next(MinFileSize, MaxFileSize);
            byte[] fileContent = new byte[fileSize];
            rand.NextBytes(fileContent);
            File.WriteAllBytes(filePath, fileContent);
            result[i] = new FileInfo(filePath);
        }
        return result;
    }
    private DirectoryInfo CreateTestDirectory() {
        string tempDirectoryPath = Path.Join(Path.GetTempPath(), DateTime.Now.Ticks.ToString("X"));
        DirectoryInfo tempDirectory = new DirectoryInfo(tempDirectoryPath);
        tempDirectory.Create();

        return tempDirectory;
    }

    public void Run() {
        DirectoryInfo testdir = CreateTestDirectory();
        HttpServer httpServer = new();
        Task httpServerTask = new Task(() => httpServer.Run());
        try {
            FileInfo[] files = CreateTestFiles(testdir);
            Http.FileServer fileServer = new(testdir);
            httpServer.AddHandler("/", Http.HttpStdMethod.GET, fileServer);
            httpServerTask.Start();

            using (HttpClient client = new()) {
                client.DefaultRequestVersion = Version.Parse("1.1");
                foreach (FileInfo file in files) {
                    string uri = "http://localhost:4728/" + file.Name;
                    using HttpResponseMessage resp = client.GetAsync(uri).Sync();
                    TestHelpers.ExpectEqual(HttpStatusCode.OK, resp.StatusCode);

                    ReadOnlySpan<byte> fileBytes = File.ReadAllBytes(file.FullName);
                    ReadOnlySpan<byte> respBytes = resp.Content.ReadAsByteArrayAsync().Sync();
                    TestHelpers.ExpectEqualSpans(ref fileBytes, ref respBytes);
                }
            }
        }
        finally {
            httpServer.Stop();
            testdir.Delete(true);
            httpServerTask.Wait();
        }
    }
}
