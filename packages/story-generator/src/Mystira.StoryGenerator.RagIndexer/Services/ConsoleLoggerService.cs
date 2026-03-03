using Mystira.StoryGenerator.RagIndexer.Interfaces;

namespace Mystira.StoryGenerator.RagIndexer.Services;

public class ConsoleLoggerService : ILoggerService
{
    public void LogInfo(string message)
    {
        Console.WriteLine(message);
    }

    public void LogError(string message, Exception? exception = null)
    {
        Console.WriteLine($"Error: {message}");
        if (exception != null)
        {
            Console.WriteLine($"Exception details: {exception.Message}");
        }
    }

    public void LogWarning(string message)
    {
        Console.WriteLine($"Warning: {message}");
    }
}