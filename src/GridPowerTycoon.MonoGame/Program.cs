using GridPowerTycoon.MonoGame;

try
{
    using var game = new Game1();
    game.Run();
}
catch (Exception ex)
{
    Console.Error.WriteLine("GridPowerTycoon failed to start.");
    Console.Error.WriteLine(ex);

    try
    {
        var logPath = Path.Combine(AppContext.BaseDirectory, "startup-error.log");
        File.WriteAllText(logPath, ex.ToString());
        Console.Error.WriteLine($"Startup error written to: {logPath}");
    }
    catch
    {
        // Ignore logging failures: the original exception is already printed to stderr.
    }

    Environment.ExitCode = 1;
}
