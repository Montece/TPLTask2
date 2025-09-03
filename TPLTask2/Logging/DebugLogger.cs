using System.Diagnostics;

namespace TPLTask2.Logging;

internal sealed class DebugLogger : ILogger
{
    public void WriteLine(int threadId, object message)
    {
        Debug.WriteLine($"[THREAD {threadId}]: {message}");
    }
}