namespace TPLTask2.Logging;

internal interface ILogger
{
    void WriteLine(int threadId, object message);
}