using Microsoft.Xna.Framework;
using project_axiom.Shared.Networking;
using System.Collections.Concurrent;

namespace project_axiom.Client.Networking;

/// <summary>
/// Handles client-side movement prediction and server reconciliation
/// </summary>
public class ClientMovementSystem
{
    private readonly ServerAllocationManager _serverManager;
    private readonly Queue<PlayerInputMessage> _unacknowledgedInputs = new();
    private uint _currentSequenceNumber = 0;
    private float _gameTime = 0f;
    
    // Client-side prediction state
    private Vector3 _predictedPosition;
    private float _predictedRotationY;
    private float _predictedRotationX;
    
    // Server authoritative state
    private Vector3 _serverPosition;
    private float _serverRotationY;
    private float _serverRotationX;
    
    public Vector3 PredictedPosition => _predictedPosition;
    public float PredictedRotationY => _predictedRotationY;
    public float PredictedRotationX => _predictedRotationX;
    
    public Vector3 ServerPosition => _serverPosition;
    public float ServerRotationY => _serverRotationY;
    public float ServerRotationX => _serverRotationX;
    
    // Movement parameters
    private readonly float _playerSpeed = 5.0f;
    private readonly float _mouseSensitivity = 0.003f;
    
    public ClientMovementSystem(ServerAllocationManager serverManager, Vector3 startPosition)
    {
        _serverManager = serverManager;
        _predictedPosition = startPosition;
        _serverPosition = startPosition;
    }
    
    /// <summary>
    /// Process client input and send to server with prediction
    /// </summary>
    public void ProcessInput(bool moveForward, bool moveBackward, bool moveLeft, bool moveRight, 
                           bool moveUp, bool moveDown, float rotationY, float rotationX, float deltaTime)
    {
        _gameTime += deltaTime;
        _currentSequenceNumber++;
        
        // Create input message
        var inputMessage = new PlayerInputMessage
        {
            SequenceNumber = _currentSequenceNumber,
            Timestamp = _gameTime,
            MoveForward = moveForward,
            MoveBackward = moveBackward,
            MoveLeft = moveLeft,
            MoveRight = moveRight,
            MoveUp = moveUp,
            MoveDown = moveDown,
            RotationY = rotationY,
            RotationX = rotationX,
            DeltaTime = deltaTime
        };
        
        // Store for reconciliation
        _unacknowledgedInputs.Enqueue(inputMessage);
        
        // Apply input locally for prediction
        ApplyInput(inputMessage, ref _predictedPosition, ref _predictedRotationY, ref _predictedRotationX);
        
        // Send to server
        if (_serverManager?.IsConnected == true)
        {
            _ = System.Threading.Tasks.Task.Run(async () => await _serverManager.SendMessageAsync(inputMessage.Serialize()));
        }
    }
    
    /// <summary>
    /// Receive server position update and reconcile with client prediction
    /// </summary>
    public void ReceiveServerUpdate(PlayerPositionMessage serverUpdate)
    {
        // Update server authoritative state
        _serverPosition = new Vector3(serverUpdate.X, serverUpdate.Y, serverUpdate.Z);
        _serverRotationY = serverUpdate.RotationY;
        _serverRotationX = serverUpdate.RotationX;
        
        // Remove acknowledged inputs
        while (_unacknowledgedInputs.Count > 0 && 
               _unacknowledgedInputs.Peek().SequenceNumber <= serverUpdate.InputSequence)
        {
            _unacknowledgedInputs.Dequeue();
        }
        
        // Reconcile: start from server position and re-apply unacknowledged inputs
        _predictedPosition = _serverPosition;
        _predictedRotationY = _serverRotationY;
        _predictedRotationX = _serverRotationX;
        
        foreach (var input in _unacknowledgedInputs)
        {
            ApplyInput(input, ref _predictedPosition, ref _predictedRotationY, ref _predictedRotationX);
        }
    }
    
    /// <summary>
    /// Apply movement input to position and rotation
    /// </summary>
    private void ApplyInput(PlayerInputMessage input, ref Vector3 position, ref float rotationY, ref float rotationX)
    {
        // Apply rotation
        rotationY = input.RotationY;
        rotationX = input.RotationX;
        
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
            Matrix rotationMatrix = Matrix.CreateRotationY(rotationY);
            moveDirection = Vector3.Transform(moveDirection, rotationMatrix);
            
            // Apply movement
            Vector3 newPosition = position + moveDirection * _playerSpeed * input.DeltaTime;
            
            // Apply boundary constraints (same as in PlayerController)
            position = ApplyBoundaryConstraints(newPosition);
        }
    }
    
    /// <summary>
    /// Apply boundary constraints to keep player within the training area
    /// </summary>
    private Vector3 ApplyBoundaryConstraints(Vector3 newPosition)
    {
        Vector3 originalPosition = newPosition;
        
        // Import the correct values from GeometryBuilder
        float halfSize = project_axiom.Rendering.GeometryBuilder.GROUND_SIZE / 2f;
        float playerRadius = 0.5f; // Half the size of player cube
        
        float minBound = -halfSize + playerRadius;
        float maxBound = halfSize - playerRadius;

        // Constrain to ground boundaries
        newPosition.X = MathHelper.Clamp(newPosition.X, minBound, maxBound);
        newPosition.Z = MathHelper.Clamp(newPosition.Z, minBound, maxBound);

        // Keep player above ground level (with small offset to prevent z-fighting)
        newPosition.Y = Math.Max(newPosition.Y, project_axiom.Rendering.GeometryBuilder.GROUND_Y + 0.51f);

        // Log when boundaries are hit
        if (originalPosition != newPosition)
        {
            System.Diagnostics.Debug.WriteLine($"[CLIENT] Boundary constraint applied:");
            System.Diagnostics.Debug.WriteLine($"  Original: {originalPosition}");
            System.Diagnostics.Debug.WriteLine($"  Constrained: {newPosition}");
            System.Diagnostics.Debug.WriteLine($"  Bounds: X/Z ∈ [{minBound:F2}, {maxBound:F2}], Ground size: {project_axiom.Rendering.GeometryBuilder.GROUND_SIZE}");
        }

        return newPosition;
    }
    
    /// <summary>
    /// Get the display position (interpolated between server and predicted)
    /// </summary>
    public Vector3 GetDisplayPosition(float interpolationFactor = 0.1f)
    {
        // Simple interpolation between server and predicted position
        // In a more complex system, this could use lag compensation
        return Vector3.Lerp(ServerPosition, PredictedPosition, interpolationFactor);
    }
    
    /// <summary>
    /// Get current rotation values for display
    /// </summary>
    public (float rotationY, float rotationX) GetDisplayRotation()
    {
        return (PredictedRotationY, PredictedRotationX);
    }
}
