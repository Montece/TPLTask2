namespace TPLTask2.Logging;

internal sealed class ConsoleLogger : ILogger
{
    public void WriteLine(int threadId, object message)
    {
        Console.WriteLine($"[THREAD {threadId}]: {message}");
    }
}