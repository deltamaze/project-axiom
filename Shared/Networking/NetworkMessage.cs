using System.Text.Json;

namespace project_axiom.Shared.Networking;

/// <summary>
/// Base class for all network messages between client and server
/// </summary>
public abstract class NetworkMessage
{
    public abstract string MessageType { get; }
    public uint SequenceNumber { get; set; }
    public float Timestamp { get; set; }

    public virtual byte[] Serialize()
    {
        var json = JsonSerializer.Serialize(this, this.GetType());
        return System.Text.Encoding.UTF8.GetBytes(json);
    }

    public static T? Deserialize<T>(byte[] data) where T : NetworkMessage
    {
        var json = System.Text.Encoding.UTF8.GetString(data);
        return JsonSerializer.Deserialize<T>(json);
    }
}

/// <summary>
/// Client input message sent to server
/// </summary>
public class PlayerInputMessage : NetworkMessage
{
    public override string MessageType => "PlayerInput";
    
    public bool MoveForward { get; set; }
    public bool MoveBackward { get; set; }
    public bool MoveLeft { get; set; }
    public bool MoveRight { get; set; }
    public bool MoveUp { get; set; }
    public bool MoveDown { get; set; }
    
    public float RotationY { get; set; }
    public float RotationX { get; set; }
    
    public float DeltaTime { get; set; }
}

/// <summary>
/// Server position update sent to client
/// </summary>
public class PlayerPositionMessage : NetworkMessage
{
    public override string MessageType => "PlayerPosition";
    
    public required string PlayFabId { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float RotationY { get; set; }
    public float RotationX { get; set; }
    public uint InputSequence { get; set; } // Last input sequence processed
}

/// <summary>
/// Game state update from server to all clients
/// </summary>
public class GameStateUpdateMessage : NetworkMessage
{
    public override string MessageType => "GameStateUpdate";
    
    public List<PlayerPositionMessage> PlayerPositions { get; set; } = new();
    public float ServerTime { get; set; }
}
