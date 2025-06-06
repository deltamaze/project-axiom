namespace project_axiom.Input;

/// <summary>
/// Handles player input, movement, and position management
/// </summary>
public class PlayerController
{
  // Player position and rotation
  public Vector3 Position { get; private set; }
  public float RotationY { get; private set; }
  public float RotationX { get; private set; }

  // Movement properties
  private float _playerSpeed = 5.0f;
  private float _mouseSensitivity = 0.003f;
  private Character _character;

  // Input state tracking
  private KeyboardState _previousKeyboardState;
  private MouseState _previousMouseState;
  private bool _isMouseCaptured = true;

  // Graphics device for mouse positioning
  private GraphicsDevice _graphicsDevice;

  // Constants
  private const float PLAYER_GROUND_OFFSET = 0.51f;

  public bool IsMouseCaptured => _isMouseCaptured;

  public PlayerController(GraphicsDevice graphicsDevice, Character character, Vector3 startPosition)
  {
    _graphicsDevice = graphicsDevice;
    _character = character;
    Position = startPosition;

    // Initialize input states
    _previousKeyboardState = Keyboard.GetState();
    _previousMouseState = Mouse.GetState();

    // Center the mouse cursor initially
    CenterMouse();
  }

  /// <summary>
  /// Update player input and position
  /// </summary>
  public void Update(GameTime gameTime)
  {
    KeyboardState currentKeyboardState = Keyboard.GetState();
    MouseState currentMouseState = Mouse.GetState();

    // Toggle mouse capture
    if (currentKeyboardState.IsKeyDown(Keys.M) && !_previousKeyboardState.IsKeyDown(Keys.M))
    {
      _isMouseCaptured = !_isMouseCaptured;
    }

    float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

    // Handle mouse look
    if (_isMouseCaptured)
    {
      HandleMouseLook(currentMouseState);
    }

    // Handle movement
    HandleMovement(currentKeyboardState, deltaTime);

    // Handle resource testing (Section 6.8 - for demonstration purposes)
    HandleResourceTesting(currentKeyboardState);

    // Update previous states
    _previousKeyboardState = currentKeyboardState;
    _previousMouseState = currentMouseState;
  }

  /// <summary>
  /// Handle resource testing keys for demonstration (Section 6.8)
  /// </summary>
  private void HandleResourceTesting(KeyboardState keyboardState)
  {
    // Test resource consumption with number keys 1-3
    if (keyboardState.IsKeyDown(Keys.D1) && !_previousKeyboardState.IsKeyDown(Keys.D1))
    {
      // Test light resource consumption
      _character.TryConsumeResource(10f);
      System.Diagnostics.Debug.WriteLine($"Light ability used - {_character.ResourceType}: {_character.CurrentResource:F1}/{_character.MaxResource}");
    }
    
    if (keyboardState.IsKeyDown(Keys.D2) && !_previousKeyboardState.IsKeyDown(Keys.D2))
    {
      // Test medium resource consumption
      _character.TryConsumeResource(25f);
      System.Diagnostics.Debug.WriteLine($"Medium ability used - {_character.ResourceType}: {_character.CurrentResource:F1}/{_character.MaxResource}");
    }
    
    if (keyboardState.IsKeyDown(Keys.D3) && !_previousKeyboardState.IsKeyDown(Keys.D3))
    {
      // Test heavy resource consumption
      _character.TryConsumeResource(50f);
      System.Diagnostics.Debug.WriteLine($"Heavy ability used - {_character.ResourceType}: {_character.CurrentResource:F1}/{_character.MaxResource}");
    }

    // Test resource restore (for testing purposes)
    if (keyboardState.IsKeyDown(Keys.R) && !_previousKeyboardState.IsKeyDown(Keys.R))
    {
      _character.CurrentResource = _character.MaxResource;
      System.Diagnostics.Debug.WriteLine($"Resource restored - {_character.ResourceType}: {_character.CurrentResource:F1}/{_character.MaxResource}");
    }
  }

  /// <summary>
  /// Handle mouse look for camera rotation
  /// </summary>
  private void HandleMouseLook(MouseState currentMouseState)
  {
    Vector2 screenCenter = new Vector2(_graphicsDevice.Viewport.Width / 2f, _graphicsDevice.Viewport.Height / 2f);
    Vector2 mouseDelta = new Vector2(currentMouseState.X - screenCenter.X, currentMouseState.Y - screenCenter.Y);

    RotationY -= mouseDelta.X * _mouseSensitivity;
    RotationX -= mouseDelta.Y * _mouseSensitivity;
    RotationX = MathHelper.Clamp(RotationX, -MathHelper.PiOver2 + 0.1f, MathHelper.PiOver2 - 0.1f);

    CenterMouse();
  }

  /// <summary>
  /// Handle WASD movement with boundary checking
  /// </summary>
  private void HandleMovement(KeyboardState keyboardState, float deltaTime)
  {
    Vector3 moveDirection = Vector3.Zero;

    if (keyboardState.IsKeyDown(Keys.W))
      moveDirection += Vector3.Forward;
    if (keyboardState.IsKeyDown(Keys.S))
      moveDirection += Vector3.Backward;
    if (keyboardState.IsKeyDown(Keys.A))
      moveDirection += Vector3.Left;
    if (keyboardState.IsKeyDown(Keys.D))
      moveDirection += Vector3.Right;
    if (keyboardState.IsKeyDown(Keys.Space))
      moveDirection += Vector3.Up;
    if (keyboardState.IsKeyDown(Keys.LeftShift))
      moveDirection += Vector3.Down;

    if (moveDirection.Length() > 0)
    {
      moveDirection.Normalize();

      // Apply rotation to movement direction
      Matrix rotationMatrix = Matrix.CreateRotationY(RotationY);
      moveDirection = Vector3.Transform(moveDirection, rotationMatrix);

      // Apply class-specific speed modifier
      float classSpeedModifier = GetClassSpeedModifier();
      Vector3 newPosition = Position + moveDirection * _playerSpeed * classSpeedModifier * deltaTime;

      // Apply boundary constraints
      newPosition = ApplyBoundaryConstraints(newPosition);

      Position = newPosition;
    }
  }

  /// <summary>
  /// Apply boundary constraints to keep player within the training area
  /// </summary>
  private Vector3 ApplyBoundaryConstraints(Vector3 newPosition)
  {
    float halfSize = GeometryBuilder.GROUND_SIZE / 2f;
    float playerRadius = 0.5f; // Half the size of player cube

    // Constrain to ground boundaries
    newPosition.X = MathHelper.Clamp(newPosition.X, -halfSize + playerRadius, halfSize - playerRadius);
    newPosition.Z = MathHelper.Clamp(newPosition.Z, -halfSize + playerRadius, halfSize - playerRadius);

    // Keep player above ground level (with small offset to prevent z-fighting)
    newPosition.Y = Math.Max(newPosition.Y, GeometryBuilder.GROUND_Y + PLAYER_GROUND_OFFSET);

    return newPosition;
  }

  /// <summary>
  /// Get speed modifier based on character class
  /// </summary>
  private float GetClassSpeedModifier()
  {
    switch (_character.Class)
    {
      case CharacterClass.Brawler:
        return 0.8f;
      case CharacterClass.Ranger:
        return 1.2f;
      case CharacterClass.Spellcaster:
        return 1.0f;
      default:
        return 1.0f;
    }
  }

  /// <summary>
  /// Check if escape key was pressed for state transitions
  /// </summary>
  public bool IsEscapePressed()
  {
    KeyboardState currentKeyboardState = Keyboard.GetState();
    return currentKeyboardState.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape);
  }

  /// <summary>
  /// Center the mouse cursor on screen
  /// </summary>
  private void CenterMouse()
  {
    Vector2 screenCenter = new Vector2(_graphicsDevice.Viewport.Width / 2f, _graphicsDevice.Viewport.Height / 2f);
    Mouse.SetPosition((int)screenCenter.X, (int)screenCenter.Y);
  }

  /// <summary>
  /// Set the player's position (useful for respawning or teleporting)
  /// </summary>
  public void SetPosition(Vector3 position)
  {
    Position = position;
  }

  /// <summary>
  /// Get class-specific tip for UI display
  /// </summary>
  public string GetClassTip()
  {
    switch (_character.Class)
    {
      case CharacterClass.Brawler:
        return "Tip: Brawlers are tough but slower. Get close to enemies! (Press 1/2/3 to test Rage consumption, R to restore)";
      case CharacterClass.Ranger:
        return "Tip: Rangers are fast and agile. Keep your distance! (Press 1/2/3 to test Energy consumption, R to restore)";
      case CharacterClass.Spellcaster:
        return "Tip: Spellcasters have powerful magic. Manage your mana wisely! (Press 1/2/3 to test Mana consumption, R to restore)";
      default:
        return "";
    }
  }
}