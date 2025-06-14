using Microsoft.Extensions.Logging;
using PlayFab;
using PlayFab.MultiplayerModels;
using System.Net;
using System.Net.Sockets;

namespace project_axiom_server;

public class GameServer
{
    private readonly ILogger<GameServer> _logger;
    private readonly PlayFabServerManager _playfabManager;
    private readonly GameSessionManager _gameSessionManager;
    private UdpClient? _udpServer;
    private bool _isRunning;
    private readonly CancellationTokenSource _cancellationTokenSource = new();    public GameServer(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<GameServer>();
        _playfabManager = new PlayFabServerManager(loggerFactory);
        _gameSessionManager = new GameSessionManager(loggerFactory);
    }

    public async Task StartAsync()
    {
        try
        {
            _logger.LogInformation("Initializing PlayFab Game Server SDK...");
            await _playfabManager.InitializeAsync();

            _logger.LogInformation("Starting UDP server...");
            await StartUdpServerAsync();

            _logger.LogInformation("Starting game session manager...");
            await _gameSessionManager.StartAsync(_cancellationTokenSource.Token);

            _isRunning = true;
            _logger.LogInformation("Game server started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start game server");
            throw;
        }
    }    private Task StartUdpServerAsync()
    {
        // Default port for game traffic - this will be configurable via PlayFab
        int gamePort = 7777;
        
        // Check if we're running under PlayFab (environment variables set by agent)
        var portString = Environment.GetEnvironmentVariable("GamePort");
        if (!string.IsNullOrEmpty(portString) && int.TryParse(portString, out var envPort))
        {
            gamePort = envPort;
        }

        _udpServer = new UdpClient(gamePort);
        _logger.LogInformation($"UDP server listening on port {gamePort}");

        // Start listening for client connections in background
        _ = Task.Run(async () => await HandleClientConnections(), _cancellationTokenSource.Token);
        
        return Task.CompletedTask;
    }

    private async Task HandleClientConnections()
    {
        if (_udpServer == null) return;

        _logger.LogInformation("Starting to listen for client connections...");

        try
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                var result = await _udpServer.ReceiveAsync();
                var clientEndpoint = result.RemoteEndPoint;
                var data = result.Buffer;

                _logger.LogDebug($"Received {data.Length} bytes from {clientEndpoint}");
                
                // For now, just echo back a simple response
                // This will be expanded to handle actual game protocol
                await _gameSessionManager.HandleClientMessageAsync(clientEndpoint, data);
            }
        }
        catch (ObjectDisposedException)
        {
            // Expected when shutting down
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling client connections");
        }
    }

    public async Task StopAsync()
    {
        _logger.LogInformation("Stopping game server...");
        _isRunning = false;
        
        _cancellationTokenSource.Cancel();
        
        _udpServer?.Close();
        _udpServer?.Dispose();
        
        await _playfabManager.ShutdownAsync();
        
        _logger.LogInformation("Game server stopped");
    }
}
