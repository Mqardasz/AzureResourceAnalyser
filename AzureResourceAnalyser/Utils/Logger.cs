namespace AzureResourceAnalyser.Utils;

public static class Logger
{
    public static void LogInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("[INFO] ");
        Console.ResetColor();
        Console.WriteLine(message);
    }

    public static void LogWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("[WARNING] ");
        Console.ResetColor();
        Console.WriteLine(message);
    }

    public static void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("[ERROR] ");
        Console.ResetColor();
        Console.WriteLine(message);
    }

    public static void LogSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("[SUCCESS] ");
        Console.ResetColor();
        Console.WriteLine(message);
    }

    public static void LogDebug(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("[DEBUG] ");
        Console.ResetColor();
        Console.WriteLine(message);
    }
}