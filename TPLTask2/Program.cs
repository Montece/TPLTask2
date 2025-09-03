using System.Diagnostics;
using TPLTask2;
using TPLTask2.Logging;

var copyManager = new CopyManager(logger: new ConsoleLogger());

if (args.Length != 3)
{
    Console.WriteLine("Usage: <source_directory_path> <destination_directory_path>");
    return;
}

var sourceDirectoryPath = args[1];
var destinationDirectoryPath = args[2];

var stopwatch = new Stopwatch();
stopwatch.Start();
copyManager.Copy(new(sourceDirectoryPath), new DirectoryInfo(destinationDirectoryPath));
stopwatch.Stop();

Console.WriteLine($"Done. Time: {stopwatch.ElapsedMilliseconds} ms");