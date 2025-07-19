using Microsoft.Extensions.Logging;
using PlayFab;
using project_axiom_server;

// Handle debug mode for LocalMultiplayerAgent
if (args.Contains("--debug"))
{
    Console.WriteLine("🐛 Debug mode enabled - Waiting for debugger to attach...");
    Console.WriteLine($"📍 Process ID: {System.Diagnostics.Process.GetCurrentProcess().Id}");
    Console.WriteLine("💡 In VS Code: Ctrl+Shift+P → 'Debug: Attach to Process' → Select 'project-axiom-server.exe'");
    
    while (!System.Diagnostics.Debugger.IsAttached)
    {
        await Task.Delay(1000);
        Console.Write(".");
    }
    
    Console.WriteLine("\n✅ Debugger attached! Starting server...");
    System.Diagnostics.Debugger.Break();
}

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole().SetMinimumLevel(LogLevel.Information);
});

var logger = loggerFactory.CreateLogger<Program>();

try
{
    logger.LogInformation("Starting Project Axiom Dedicated Server...");
    
    var server = new GameServer(loggerFactory);
    await server.StartAsync();
    
    logger.LogInformation("Server started successfully. Press Ctrl+C to exit.");
    
    // Keep the server running
    var cancellationToken = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>
    {
        e.Cancel = true;
        cancellationToken.Cancel();
    };
    
    await Task.Delay(Timeout.Infinite, cancellationToken.Token);
}
catch (OperationCanceledException)
{
    logger.LogInformation("Server shutdown requested.");
}
catch (Exception ex)
{
    logger.LogError(ex, "Fatal error occurred in server");
}
finally
{
    logger.LogInformation("Project Axiom Dedicated Server shutting down...");
}
