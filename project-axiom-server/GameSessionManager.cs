using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;

namespace project_axiom_server;

public class GameSessionManager
{
    private readonly ILogger<GameSessionManager> _logger;
    private readonly PlayFabServerManager _playfabManager;
    private readonly ConcurrentDictionary<string, ConnectedPlayer> _connectedPlayers = new();
    private readonly object _gameStateLock = new();
    private bool _isRunning;
      public GameSessionManager(ILoggerFactory loggerFactory, PlayFabServerManager playfabManager)
    {
        _logger = loggerFactory.CreateLogger<GameSessionManager>();
        _playfabManager = playfabManager;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Game Session Manager...");
        _isRunning = true;
        
        // Start the game loop
        _ = Task.Run(async () => await GameLoopAsync(cancellationToken), cancellationToken);
        
        await Task.CompletedTask;
    }

    public async Task HandleClientMessageAsync(IPEndPoint clientEndpoint, byte[] data)
    {
        try
        {
            var message = Encoding.UTF8.GetString(data);
            _logger.LogDebug($"Received message from {clientEndpoint}: {message}");

            // Basic message handling - this will be expanded
            if (message.StartsWith("CONNECT:"))
            {
                await HandlePlayerConnect(clientEndpoint, message);
            }
            else if (message.StartsWith("DISCONNECT:"))
            {
                await HandlePlayerDisconnect(clientEndpoint, message);
            }
            else if (message.StartsWith("HEARTBEAT"))
            {
                await HandleHeartbeat(clientEndpoint);
            }
            else
            {
                _logger.LogDebug($"Unknown message type from {clientEndpoint}: {message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling message from {clientEndpoint}");
        }
    }

    private async Task HandlePlayerConnect(IPEndPoint clientEndpoint, string message)
    {
        try
        {
            // Expected format: "CONNECT:PlayFabId"
            var parts = message.Split(':');
            if (parts.Length < 2)
            {
                _logger.LogWarning($"Invalid connect message from {clientEndpoint}: {message}");
                return;
            }

            var playFabId = parts[1];
            var playerKey = $"{clientEndpoint.Address}:{clientEndpoint.Port}";

            var player = new ConnectedPlayer
            {
                PlayFabId = playFabId,
                EndPoint = clientEndpoint,
                ConnectedAt = DateTime.UtcNow,
                LastHeartbeat = DateTime.UtcNow
            };            _connectedPlayers.TryAdd(playerKey, player);
            
            _logger.LogInformation($"Player connected: {playFabId} from {clientEndpoint}");
            
            // Notify PlayFab about the new player connection
            await _playfabManager.NotifyPlayerConnected(playFabId);
            
            // Send connection acknowledgment
            var response = "CONNECT_ACK:SUCCESS";
            await SendMessageToClient(clientEndpoint, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling player connect from {clientEndpoint}");
        }
    }

    private async Task HandlePlayerDisconnect(IPEndPoint clientEndpoint, string message)
    {
        try
        {
            var playerKey = $"{clientEndpoint.Address}:{clientEndpoint.Port}";
              if (_connectedPlayers.TryRemove(playerKey, out var player))
            {
                _logger.LogInformation($"Player disconnected: {player.PlayFabId} from {clientEndpoint}");
                
                // Notify PlayFab about the player disconnection
                await _playfabManager.NotifyPlayerDisconnected(player.PlayFabId);
            }
            else
            {
                _logger.LogWarning($"Disconnect request from unknown client: {clientEndpoint}");
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling player disconnect from {clientEndpoint}");
        }
    }

    private async Task HandleHeartbeat(IPEndPoint clientEndpoint)
    {
        try
        {
            var playerKey = $"{clientEndpoint.Address}:{clientEndpoint.Port}";
            
            if (_connectedPlayers.TryGetValue(playerKey, out var player))
            {
                player.LastHeartbeat = DateTime.UtcNow;
                _logger.LogDebug($"Heartbeat received from {player.PlayFabId}");
                
                // Send heartbeat response
                await SendMessageToClient(clientEndpoint, "HEARTBEAT_ACK");
            }
            else
            {
                _logger.LogWarning($"Heartbeat from unknown client: {clientEndpoint}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling heartbeat from {clientEndpoint}");
        }
    }

    private async Task SendMessageToClient(IPEndPoint clientEndpoint, string message)
    {
        try
        {
            // This would be implemented with the actual UDP socket
            // For now, just log what we would send
            _logger.LogDebug($"Sending to {clientEndpoint}: {message}");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send message to {clientEndpoint}");
        }
    }

    private async Task GameLoopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting game loop...");
        
        var targetFrameTime = TimeSpan.FromMilliseconds(1000.0 / 60.0); // 60 FPS
        
        while (!cancellationToken.IsCancellationRequested && _isRunning)
        {
            var frameStart = DateTime.UtcNow;
            
            try
            {
                // Clean up disconnected players
                await CleanupDisconnectedPlayers();
                
                // Update game state
                await UpdateGameState();
                
                // Send updates to clients
                await SendGameStateUpdates();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in game loop");
            }
            
            // Maintain target frame rate
            var frameTime = DateTime.UtcNow - frameStart;
            var sleepTime = targetFrameTime - frameTime;
            
            if (sleepTime > TimeSpan.Zero)
            {
                await Task.Delay(sleepTime, cancellationToken);
            }
        }
        
        _logger.LogInformation("Game loop stopped");
    }

    private async Task CleanupDisconnectedPlayers()
    {
        var cutoffTime = DateTime.UtcNow.AddSeconds(-30); // 30 second timeout
        var playersToRemove = new List<string>();
        
        foreach (var kvp in _connectedPlayers)
        {
            if (kvp.Value.LastHeartbeat < cutoffTime)
            {
                playersToRemove.Add(kvp.Key);
                _logger.LogInformation($"Removing inactive player: {kvp.Value.PlayFabId}");
            }
        }
        
        foreach (var playerKey in playersToRemove)
        {
            _connectedPlayers.TryRemove(playerKey, out _);
        }
        
        await Task.CompletedTask;
    }

    private async Task UpdateGameState()
    {
        // Basic game state update - this will be expanded with actual game logic
        lock (_gameStateLock)
        {
            // Update positions, health, cooldowns, etc.
            // For now, this is just a placeholder
        }
        
        await Task.CompletedTask;
    }

    private async Task SendGameStateUpdates()
    {
        // Send periodic game state updates to all connected clients
        // This will be expanded with actual game state serialization
        await Task.CompletedTask;
    }

    public int ConnectedPlayerCount => _connectedPlayers.Count;
}

public class ConnectedPlayer
{
    public required string PlayFabId { get; set; }
    public required IPEndPoint EndPoint { get; set; }
    public DateTime ConnectedAt { get; set; }
    public DateTime LastHeartbeat { get; set; }
    
    // Game-specific properties will be added here
    // public Vector3 Position { get; set; }
    // public float Health { get; set; }
    // public CharacterClass Class { get; set; }
}
