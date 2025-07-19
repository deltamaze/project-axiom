using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using project_axiom.Shared.Networking;
using project_axiom_server.Game;
using Microsoft.Xna.Framework;

namespace project_axiom_server;

public class GameSessionManager
{
    private readonly ILogger<GameSessionManager> _logger;
    private readonly PlayFabServerManager _playfabManager;
    private readonly ConcurrentDictionary<string, ConnectedPlayer> _connectedPlayers = new(); // Key: PlayFabId
    private readonly ConcurrentDictionary<string, string> _endpointToPlayFabId = new(); // Key: "IP:Port", Value: PlayFabId
    private readonly ServerMovementSystem _movementSystem;
    private readonly object _gameStateLock = new();
    private bool _isRunning;
    private UdpClient? _udpServer; // Add UDP server reference
    
    public GameSessionManager(ILoggerFactory loggerFactory, PlayFabServerManager playfabManager)
    {
        _logger = loggerFactory.CreateLogger<GameSessionManager>();
        _playfabManager = playfabManager;
        _movementSystem = new ServerMovementSystem(loggerFactory);
    }

    /// <summary>
    /// Set the UDP server reference for sending messages to clients
    /// </summary>
    public void SetUdpServer(UdpClient udpServer)
    {
        _udpServer = udpServer;
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

            // Check if this is a JSON network message
            if (message.StartsWith("{"))
            {
                await HandleNetworkMessage(clientEndpoint, data);
                return;
            }

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

    private async Task HandleNetworkMessage(IPEndPoint clientEndpoint, byte[] data)
    {
        try
        {
            var json = Encoding.UTF8.GetString(data);
            using var document = JsonDocument.Parse(json);
            var messageType = document.RootElement.GetProperty("MessageType").GetString();

            // Look up player by endpoint, and update endpoint if needed
            var endpointKey = $"{clientEndpoint.Address}:{clientEndpoint.Port}";
            
            if (!_endpointToPlayFabId.TryGetValue(endpointKey, out var playFabId))
            {
                _logger.LogWarning($"Received network message from unknown client: {clientEndpoint}");
                return;
            }

            if (!_connectedPlayers.TryGetValue(playFabId, out var connectedPlayer))
            {
                _logger.LogWarning($"Received network message for unknown player: {playFabId}");
                return;
            }

            // Update the player's endpoint in case it changed
            connectedPlayer.EndPoint = clientEndpoint;
            connectedPlayer.LastHeartbeat = DateTime.UtcNow;

            switch (messageType)
            {
                case "PlayerInput":
                    var inputMessage = NetworkMessage.Deserialize<PlayerInputMessage>(data);
                    if (inputMessage != null)
                    {
                        var positionUpdate = _movementSystem.ProcessPlayerInput(connectedPlayer.PlayFabId, inputMessage);
                        
                        if (positionUpdate != null)
                        {
                            // Send position update back to the client
                            await SendMessageToClient(clientEndpoint, positionUpdate.Serialize());
                        }
                    }
                    break;
                    
                default:
                    _logger.LogDebug($"Unknown network message type: {messageType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling network message from {clientEndpoint}");
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
            var endpointKey = $"{clientEndpoint.Address}:{clientEndpoint.Port}";

            var player = new ConnectedPlayer
            {
                PlayFabId = playFabId,
                EndPoint = clientEndpoint,
                ConnectedAt = DateTime.UtcNow,
                LastHeartbeat = DateTime.UtcNow
            };

            // Store player by PlayFabId and create endpoint mapping
            _connectedPlayers.TryAdd(playFabId, player);
            _endpointToPlayFabId.TryAdd(endpointKey, playFabId);
            
            // Add player to movement system with default starting position
            var startPosition = new Vector3(0, 0.51f, 0); // Default spawn position
            _movementSystem.AddPlayer(playFabId, startPosition);

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
    }    private async Task HandlePlayerDisconnect(IPEndPoint clientEndpoint, string message)
    {
        try
        {
            var playerKey = $"{clientEndpoint.Address}:{clientEndpoint.Port}";
              if (_connectedPlayers.TryRemove(playerKey, out var player))
            {
                _logger.LogInformation($"Player disconnected: {player.PlayFabId} from {clientEndpoint}");
                
                // Remove from movement system
                _movementSystem.RemovePlayer(player.PlayFabId);
                
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
            if (_udpServer != null)
            {
                var data = Encoding.UTF8.GetBytes(message);
                await _udpServer.SendAsync(data, data.Length, clientEndpoint);
                _logger.LogDebug($"Sent to {clientEndpoint}: {message}");
            }
            else
            {
                _logger.LogWarning($"Cannot send message - UDP server not set: {message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send message to {clientEndpoint}");
        }
    }

    private async Task SendMessageToClient(IPEndPoint clientEndpoint, byte[] data)
    {
        try
        {
            if (_udpServer != null)
            {
                await _udpServer.SendAsync(data, data.Length, clientEndpoint);
                _logger.LogDebug($"Sent {data.Length} bytes to {clientEndpoint}");
            }
            else
            {
                _logger.LogWarning($"Cannot send data - UDP server not set");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send data to {clientEndpoint}");
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
    }    private async Task CleanupDisconnectedPlayers()
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
            if (_connectedPlayers.TryRemove(playerKey, out var player))
            {
                // Remove from movement system
                _movementSystem.RemovePlayer(player.PlayFabId);
            }
        }
        
        // Clean up movement system inactive players
        _movementSystem.CleanupInactivePlayers(TimeSpan.FromSeconds(30));
        
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
        if (_connectedPlayers.Count == 0)
            return;

        // Get current game state from movement system
        var gameStateUpdate = _movementSystem.GetGameStateUpdate();
        var updateData = gameStateUpdate.Serialize();

        // Broadcast to all connected clients
        var tasks = new List<Task>();
        foreach (var kvp in _connectedPlayers)
        {
            var player = kvp.Value;
            tasks.Add(SendMessageToClient(player.EndPoint, updateData));
        }

        await Task.WhenAll(tasks);
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
