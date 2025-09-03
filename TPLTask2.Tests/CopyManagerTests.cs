using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace TPLTask2.Tests;

public class CopyManagerTests
{
    [Fact]
    public void Copy_VirtualFileSystem_CorrectAllFilesCopies()
    {
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/src/root.txt", new MockFileData("root") },
            { "/src/dirA/a1.txt", new MockFileData("a1") },
            { "/src/dirA/a2.txt", new MockFileData("a2") },
            { "/src/dirB/sub/b.txt", new MockFileData("b") }
        });

        var mockLogger = new MockLogger();
        var copyManager = new CopyManager(mockFileSystem, mockLogger);

        var src = mockFileSystem.DirectoryInfo.New("/src");
        var dst = mockFileSystem.DirectoryInfo.New("/dst");

        copyManager.Copy(src, dst);

        string[] expectedFiles =
        [
            mockFileSystem.Path.Combine("/dst", "root.txt"),
            mockFileSystem.Path.Combine("/dst", "dirA", "a1.txt"),
            mockFileSystem.Path.Combine("/dst", "dirA", "a2.txt"),
            mockFileSystem.Path.Combine("/dst", "dirB", "sub", "b.txt")
        ];

        Assert.True(mockFileSystem.Directory.Exists(mockFileSystem.Path.Combine("/dst", "dirA")));
        Assert.True(mockFileSystem.Directory.Exists(mockFileSystem.Path.Combine("/dst", "dirB")));
        Assert.True(mockFileSystem.Directory.Exists(mockFileSystem.Path.Combine("/dst", "dirB", "sub")));

        foreach (var dstFile in expectedFiles)
        {
            var srcFile = dstFile.Replace("/dst", "/src");
            Assert.Equal(mockFileSystem.File.ReadAllText(srcFile), mockFileSystem.File.ReadAllText(dstFile));
        }

        Assert.Equal(expectedFiles.Length, mockLogger.FilesAndTheirThread.Count);
        Assert.Equal(expectedFiles.Length, mockLogger.FilesAndTheirThread.Values.Distinct().Count());
    }
}