using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using project_axiom.Shared.Networking;
using System.Collections.Concurrent;

namespace project_axiom_server.Game;

/// <summary>
/// Server-side player state and movement processing
/// </summary>
public class ServerPlayer
{
    public string PlayFabId { get; set; }
    public Vector3 Position { get; set; }
    public float RotationY { get; set; }
    public float RotationX { get; set; }
    public uint LastProcessedInput { get; set; }
    public DateTime LastUpdate { get; set; }
    
    // Game-specific properties
    public float Health { get; set; } = 100f;
    public string CharacterClass { get; set; } = "Brawler";
    
    public ServerPlayer(string playFabId, Vector3 startPosition)
    {
        PlayFabId = playFabId;
        Position = startPosition;
        LastUpdate = DateTime.UtcNow;
    }
}

/// <summary>
/// Handles server-side movement processing and game state management
/// </summary>
public class ServerMovementSystem
{
    private readonly ILogger<ServerMovementSystem> _logger;
    private readonly ConcurrentDictionary<string, ServerPlayer> _players = new();
    
    // Movement parameters (should match client)
    private readonly float _playerSpeed = 5.0f;
    private const float GROUND_SIZE = 50f; // Should match GeometryBuilder.GROUND_SIZE
    private const float GROUND_Y = 0f;
    private const float PLAYER_GROUND_OFFSET = 0.51f;
    
    public ServerMovementSystem(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ServerMovementSystem>();
    }
    
    /// <summary>
    /// Add a new player to the system
    /// </summary>
    public void AddPlayer(string playFabId, Vector3 startPosition, string characterClass = "Brawler")
    {
        var player = new ServerPlayer(playFabId, startPosition)
        {
            CharacterClass = characterClass
        };
        
        _players.TryAdd(playFabId, player);
        _logger.LogInformation($"Added player {playFabId} at position {startPosition}");
    }
    
    /// <summary>
    /// Remove a player from the system
    /// </summary>
    public void RemovePlayer(string playFabId)
    {
        if (_players.TryRemove(playFabId, out var player))
        {
            _logger.LogInformation($"Removed player {playFabId}");
        }
    }
    
    /// <summary>
    /// Process player input and update position
    /// </summary>
    public PlayerPositionMessage? ProcessPlayerInput(string playFabId, PlayerInputMessage input)
    {
        if (!_players.TryGetValue(playFabId, out var player))
        {
            _logger.LogWarning($"Received input for unknown player: {playFabId}");
            return null;
        }
        
        // Only process if this input is newer than the last processed
        if (input.SequenceNumber <= player.LastProcessedInput)
        {
            _logger.LogDebug($"Ignoring old input {input.SequenceNumber} for player {playFabId} (last: {player.LastProcessedInput})");
            return null;
        }
        
        // Apply rotation (always accept client rotation for responsiveness)
        player.RotationY = input.RotationY;
        player.RotationX = input.RotationX;
        
        // Calculate movement direction
        Vector3 moveDirection = Vector3.Zero;
        
        if (input.MoveForward) moveDirection += Vector3.Forward;
        if (input.MoveBackward) moveDirection += Vector3.Backward;
        if (input.MoveLeft) moveDirection += Vector3.Left;
        if (input.MoveRight) moveDirection += Vector3.Right;
        if (input.MoveUp) moveDirection += Vector3.Up;
        if (input.MoveDown) moveDirection += Vector3.Down;
        
        if (moveDirection.Length() > 0)
        {
            moveDirection.Normalize();
            
            // Apply rotation to movement direction
            Matrix rotationMatrix = Matrix.CreateRotationY(player.RotationY);
            moveDirection = Vector3.Transform(moveDirection, rotationMatrix);
            
            // Apply class-specific speed modifier
            float classSpeedModifier = GetClassSpeedModifier(player.CharacterClass);
            Vector3 newPosition = player.Position + moveDirection * _playerSpeed * classSpeedModifier * input.DeltaTime;
            
            // Apply boundary constraints and anti-cheat validation
            newPosition = ApplyBoundaryConstraints(newPosition);
            
            // Basic anti-cheat: ensure movement is reasonable
            float maxDistancePerFrame = _playerSpeed * classSpeedModifier * input.DeltaTime * 1.5f; // Allow 50% tolerance
            float actualDistance = Vector3.Distance(player.Position, newPosition);
            
            if (actualDistance <= maxDistancePerFrame)
            {
                player.Position = newPosition;
            }
            else
            {
                _logger.LogWarning($"Suspicious movement from player {playFabId}: {actualDistance:F2} > {maxDistancePerFrame:F2}");
                // Don't update position for suspicious movement
            }
        }
        
        player.LastProcessedInput = input.SequenceNumber;
        player.LastUpdate = DateTime.UtcNow;
        
        // Return position update
        return new PlayerPositionMessage
        {
            PlayFabId = playFabId,
            X = player.Position.X,
            Y = player.Position.Y,
            Z = player.Position.Z,
            RotationY = player.RotationY,
            RotationX = player.RotationX,
            InputSequence = input.SequenceNumber,
            Timestamp = (float)(DateTime.UtcNow - DateTime.Today).TotalSeconds
        };
    }
    
    /// <summary>
    /// Get all current player positions for broadcast
    /// </summary>
    public GameStateUpdateMessage GetGameStateUpdate()
    {
        var playerPositions = new List<PlayerPositionMessage>();
        
        foreach (var kvp in _players)
        {
            var player = kvp.Value;
            playerPositions.Add(new PlayerPositionMessage
            {
                PlayFabId = player.PlayFabId,
                X = player.Position.X,
                Y = player.Position.Y,
                Z = player.Position.Z,
                RotationY = player.RotationY,
                RotationX = player.RotationX,
                InputSequence = player.LastProcessedInput,
                Timestamp = (float)(DateTime.UtcNow - DateTime.Today).TotalSeconds
            });
        }
        
        return new GameStateUpdateMessage
        {
            PlayerPositions = playerPositions,
            ServerTime = (float)(DateTime.UtcNow - DateTime.Today).TotalSeconds,
            Timestamp = (float)(DateTime.UtcNow - DateTime.Today).TotalSeconds
        };
    }
    
    /// <summary>
    /// Get speed modifier based on character class
    /// </summary>
    private float GetClassSpeedModifier(string characterClass)
    {
        return characterClass switch
        {
            "Brawler" => 0.8f,
            "Ranger" => 1.2f,
            "Spellcaster" => 1.0f,
            _ => 1.0f
        };
    }
    
    /// <summary>
    /// Apply boundary constraints to keep player within the training area
    /// </summary>
    private Vector3 ApplyBoundaryConstraints(Vector3 newPosition)
    {
        Vector3 originalPosition = newPosition;
        
        float halfSize = GROUND_SIZE / 2f;
        float playerRadius = 0.5f; // Half the size of player cube
        
        float minBound = -halfSize + playerRadius;
        float maxBound = halfSize - playerRadius;

        // Constrain to ground boundaries
        newPosition.X = MathHelper.Clamp(newPosition.X, minBound, maxBound);
        newPosition.Z = MathHelper.Clamp(newPosition.Z, minBound, maxBound);

        // Keep player above ground level (with small offset to prevent z-fighting)
        newPosition.Y = Math.Max(newPosition.Y, GROUND_Y + PLAYER_GROUND_OFFSET);

        // Log when boundaries are hit
        if (originalPosition != newPosition)
        {
            _logger.LogInformation($"[SERVER] Boundary constraint applied:");
            _logger.LogInformation($"  Original: {originalPosition}");
            _logger.LogInformation($"  Constrained: {newPosition}");
            _logger.LogInformation($"  Bounds: X/Z ∈ [{minBound:F2}, {maxBound:F2}], Ground size: {GROUND_SIZE}");
        }

        return newPosition;
    }
    
    /// <summary>
    /// Get a specific player by PlayFab ID
    /// </summary>
    public ServerPlayer? GetPlayer(string playFabId)
    {
        _players.TryGetValue(playFabId, out var player);
        return player;
    }
    
    /// <summary>
    /// Get all connected players
    /// </summary>
    public IEnumerable<ServerPlayer> GetAllPlayers()
    {
        return _players.Values;
    }
    
    /// <summary>
    /// Clean up inactive players
    /// </summary>
    public void CleanupInactivePlayers(TimeSpan inactiveThreshold)
    {
        var cutoffTime = DateTime.UtcNow - inactiveThreshold;
        var inactivePlayers = _players.Where(kvp => kvp.Value.LastUpdate < cutoffTime).ToList();
        
        foreach (var kvp in inactivePlayers)
        {
            if (_players.TryRemove(kvp.Key, out var player))
            {
                _logger.LogInformation($"Removed inactive player {kvp.Key}");
            }
        }
    }
}
