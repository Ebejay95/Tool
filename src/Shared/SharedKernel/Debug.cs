namespace SharedKernel;

public class Debug
{
    public static void Log(string message)
    {
        var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"DEBUG | {message}");
        Console.ForegroundColor = oldColor;
    }

    public static void LogObject(object obj)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(obj, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
        Log(json);
    }
}
