using System.Collections.Concurrent;
using TPLTask2.Logging;

namespace TPLTask2.Tests;

internal sealed class MockLogger : ILogger
{
    public ConcurrentDictionary<string, int> FilesAndTheirThread { get; } = new();

    public void WriteLine(int threadId, object message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var messageString = message.ToString()!;

        if (!messageString.Contains("file copied"))
        {
            return;
        }

        var filename = messageString.Substring(messageString.IndexOf('\'') + 1, messageString.LastIndexOf('\'') - 1);

        FilesAndTheirThread[filename] = threadId;
    }
}