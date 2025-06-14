using Microsoft.Extensions.Logging;
using PlayFab;
using project_axiom_server;

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
