using System.IO.Abstractions;
using TPLTask2.Logging;

namespace TPLTask2;

internal sealed class CopyManager
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger? _logger;

    private long _count;

    public CopyManager(IFileSystem? fileSystem = null, ILogger? logger = null)
    {
        _fileSystem = fileSystem ?? new FileSystem();
        _logger = logger;
    }

    public void Copy(DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory)
    {
        Copy(_fileSystem.DirectoryInfo.New(sourceDirectory.FullName), _fileSystem.DirectoryInfo.New(destinationDirectory.FullName));
    }

    public void Copy(IDirectoryInfo sourceDirectory, IDirectoryInfo destinationDirectory)
    {
        Interlocked.Increment(ref _count);
        ProcessCopy(sourceDirectory, destinationDirectory);
        Interlocked.Decrement(ref _count);

        while (_count != 0)
        {
        }
    }

    private void ProcessCopy(IDirectoryInfo sourceRootDirectory, IDirectoryInfo destinationRootDirectory)
    {
        try
        {
            if (IsLinkLike(sourceRootDirectory))
            {
                WriteToLog($"Symlink '{sourceRootDirectory}' ignored.");
                return;
            }

            if (!sourceRootDirectory.Exists)
            {
                WriteToLog($"Source directory '{sourceRootDirectory}' doesn't exist.");
                return;
            }

            if (!destinationRootDirectory.Exists)
            {
                CreateDirectory(destinationRootDirectory);
            }

            var directories = sourceRootDirectory.GetDirectories();
            var files = sourceRootDirectory.GetFiles();

            foreach (var directory in directories)
            {
                var localSourceRootDirectory = directory;
                var localDestinationRootDirectory = _fileSystem.DirectoryInfo.New(_fileSystem.Path.Combine(destinationRootDirectory.FullName, directory.Name));

                var createThread = new Thread(parameters =>
                {
                    var (_localSourceRootDirectory, _localDestinationRootDirectory) = (ValueTuple<IDirectoryInfo, IDirectoryInfo>)parameters!;
                    ProcessCopy(_localSourceRootDirectory, _localDestinationRootDirectory);
                    Interlocked.Decrement(ref _count);
                });

                Interlocked.Increment(ref _count);
                createThread.Start((localSourceRootDirectory, localDestinationRootDirectory));
            }

            foreach (var file in files)
            {
                var localSourceFilePath = file;
                var localDestinationFilePath = _fileSystem.FileInfo.New(_fileSystem.Path.Combine(destinationRootDirectory.FullName, file.Name));

                var copyThread = new Thread(parameters =>
                {
                    var (_localSourceFilePath, _localDestinationFilePath) = (ValueTuple<IFileInfo, IFileInfo>)parameters!;
                    CopyFile(_localSourceFilePath, _localDestinationFilePath);
                    Interlocked.Decrement(ref _count);
                });

                Interlocked.Increment(ref _count);
                copyThread.Start((localSourceFilePath, localDestinationFilePath));
            }
        }
        catch (Exception ex)
        {
            WriteToLog($"Error to process copy from '{sourceRootDirectory.FullName}' to '{destinationRootDirectory.FullName}'. Reason: {ex.Message}");
        }
    }

    private void CopyFile(IFileInfo sourceFilePath, IFileInfo destinationFilePath)
    {
        try
        {
            sourceFilePath.CopyTo(destinationFilePath.FullName);
            WriteToLog($"'{sourceFilePath.Name}' file copied successfully.");
        }
        catch (Exception ex)
        {
            WriteToLog($"'{sourceFilePath.FullName}' copied with error. Reason: {ex.Message}");
        }
    }

    private void CreateDirectory(IDirectoryInfo destinationDirectoryPath)
    {
        try
        {
            destinationDirectoryPath.Create();
            WriteToLog($"'{destinationDirectoryPath.Name}' folder copied successfully.");
        }
        catch (Exception ex)
        { 
            WriteToLog($"Error to create directory '{destinationDirectoryPath.FullName}'. Reason: {ex.Message}");
        }
    }

    private void WriteToLog(object message)
    {
        _logger?.WriteLine(Thread.CurrentThread.ManagedThreadId, message);
    }

    private static bool IsReparsePoint(IFileSystemInfo info)
    {
        return info.Attributes.HasFlag(FileAttributes.ReparsePoint);
    }

    private static bool IsLinkLike(IFileSystemInfo info)
    {
        try
        {
            return info.LinkTarget is not null || info.ResolveLinkTarget(returnFinalTarget: false) is not null || IsReparsePoint(info);
        }
        catch (PlatformNotSupportedException)
        {
            return IsReparsePoint(info);
        }
        catch (IOException)
        {
            return IsReparsePoint(info);
        }
    }
}