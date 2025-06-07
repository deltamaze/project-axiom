namespace project_axiom.GameStates;

public class TrainingGroundsState : GameState
{
    // Rendering components
    private BasicEffect _basicEffect;
    private CubeRenderer _cubeRenderer;
    private EnvironmentRenderer _environmentRenderer;
    private TrainingDummyRenderer _trainingDummyRenderer;

    // Input and camera components
    private PlayerController _playerController;
    private CameraController _cameraController;

    // Character information
    private Character _character;

    // Training dummies
    private List<TrainingDummy> _trainingDummies;

    // UI font
    private SpriteFont _font;

    // Persistent 1x1 white texture for UI rectangles
    private Texture2D _whiteTexture;

    // Constants
    private const float PLAYER_GROUND_OFFSET = 0.51f;

    // Targeting system
    private TrainingDummy _targetedDummy;
    private MouseState _previousMouseState;

    // Add player health fields
    private float _playerMaxHealth = 100f;
    private float _playerCurrentHealth = 100f;

    public TrainingGroundsState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content, Character character)
        : base(game, graphicsDevice, content)
    {
        _character = character ?? new Character("Default", CharacterClass.Brawler);
    }

    public TrainingGroundsState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
        : this(game, graphicsDevice, content, new Character("Default", CharacterClass.Brawler))
    {
    }

    public override void LoadContent()
    {
        // Load font for UI
        _font = _content.Load<SpriteFont>("Fonts/DefaultFont");

        // Initialize rendering components
        InitializeRendering();

        // Initialize input and camera components
        InitializeInputAndCamera();

        // Initialize training dummies
        InitializeTrainingDummies();

        // Create persistent 1x1 white texture for UI
        _whiteTexture = new Texture2D(_graphicsDevice, 1, 1);
        _whiteTexture.SetData(new[] { Color.White });

        System.Diagnostics.Debug.WriteLine($"Character {_character.Name} ({_character.Class}) entered Training Grounds");
        System.Diagnostics.Debug.WriteLine($"Training area: {GeometryBuilder.GROUND_SIZE}x{GeometryBuilder.GROUND_SIZE} units, Wall height: {GeometryBuilder.WALL_HEIGHT} units");
        System.Diagnostics.Debug.WriteLine($"Training dummies placed: {_trainingDummies.Count}");
    }

    /// <summary>
    /// Initialize all rendering components and effects
    /// </summary>
    private void InitializeRendering()
    {
        // Initialize BasicEffect for 3D rendering
        _basicEffect = new BasicEffect(_graphicsDevice);
        _basicEffect.VertexColorEnabled = true;
        _basicEffect.LightingEnabled = false;

        // Initialize renderers
        _cubeRenderer = new CubeRenderer(_graphicsDevice, _character.Class);
        _environmentRenderer = new EnvironmentRenderer(_graphicsDevice);
        _trainingDummyRenderer = new TrainingDummyRenderer(_graphicsDevice, _font);
    }

    /// <summary>
    /// Initialize input handling and camera systems
    /// </summary>
    private void InitializeInputAndCamera()
    {
        // Initialize camera controller
        _cameraController = new CameraController(_graphicsDevice);

        // Initialize player controller with starting position
        Vector3 startPosition = new Vector3(0, GeometryBuilder.GROUND_Y + PLAYER_GROUND_OFFSET, 0);
        _playerController = new PlayerController(_graphicsDevice, _character, startPosition);
    }

    /// <summary>
    /// Initialize training dummies at predefined positions
    /// </summary>
    private void InitializeTrainingDummies()
    {
        _trainingDummies = new List<TrainingDummy>();

        // Get predefined positions from GeometryBuilder
        Vector3[] dummyPositions = GeometryBuilder.GetTrainingDummyPositions();

        // Create training dummies at each position
        for (int i = 0; i < dummyPositions.Length; i++)
        {
            var dummy = new TrainingDummy(dummyPositions[i], $"Dummy {i + 1}");
            _trainingDummies.Add(dummy);
        }

        System.Diagnostics.Debug.WriteLine($"Initialized {_trainingDummies.Count} training dummies");
        foreach (var dummy in _trainingDummies)
        {
            System.Diagnostics.Debug.WriteLine($"  {dummy.Name} at position {dummy.Position}");
        }
    }

    public override void Update(GameTime gameTime)
    {
        // Check for escape key to return to main menu
        if (_playerController.IsEscapePressed())
        {
            _game.ChangeState(new MainMenuState(_game, _graphicsDevice, _content));
            return;
        }

        // Update player input and movement
        _playerController.Update(gameTime);

        // Update camera based on player position and rotation
        _cameraController.Update(
            _playerController.Position,
            _playerController.RotationX,
            _playerController.RotationY);

        // Update BasicEffect matrices
        _basicEffect.View = _cameraController.View;
        _basicEffect.Projection = _cameraController.Projection;

        // Update character resource regeneration
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _character.RegenerateResource(deltaTime);

        // Update training dummies (placeholder for future logic)
        UpdateTrainingDummies(gameTime);

        // Mouse picking for click-to-target
        MouseState mouseState = Mouse.GetState();
        if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
        {
            _targetedDummy = GetDummyUnderMouse(mouseState.Position);
        }
        _previousMouseState = mouseState;
    }

    /// <summary>
    /// Update training dummy logic (placeholder for future expansion)
    /// </summary>
    private void UpdateTrainingDummies(GameTime gameTime)
    {
        // For now, training dummies are static
        // Future: Add respawn logic, animations, or other behaviors

        // Example: Auto-reset dummies after being "defeated" for a certain time
        foreach (var dummy in _trainingDummies)
        {
            if (!dummy.IsAlive)
            {
                // Could add a respawn timer here in the future
                // For now, keep them "alive" for testing
                dummy.Reset();
            }
        }
    }

    public override void PostUpdate(GameTime gameTime)
    {
        // No state transitions needed in PostUpdate for now
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        // Clear screen with sky blue background
        _graphicsDevice.Clear(new Color(135, 206, 235));

        // Set 3D rendering states
        _graphicsDevice.BlendState = BlendState.Opaque;
        _graphicsDevice.DepthStencilState = DepthStencilState.Default;
        _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

        // Draw 3D environment
        _environmentRenderer.DrawEnvironment(_basicEffect);

        // Draw training dummies (pass targeted dummy)
        _trainingDummyRenderer.DrawDummies(_basicEffect, _trainingDummies, _targetedDummy);

        // Draw player cube
        _cubeRenderer.Draw(_basicEffect, _playerController.Position);

        // Draw UI (show targeted dummy info)
        DrawUI(spriteBatch);

        // Reset render states
        _graphicsDevice.BlendState = BlendState.Opaque;
        _graphicsDevice.DepthStencilState = DepthStencilState.Default;
    }

    /// <summary>
    /// Draw the user interface elements
    /// </summary>
    private void DrawUI(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin();

        // Draw player health and resource bars at top left
        DrawPlayerHealthAndResourceBars(spriteBatch);

        // Show targeted dummy info (centered at top)
        if (_targetedDummy != null && _targetedDummy.IsAlive)
        {
            string info = $"Target: {_targetedDummy.Name}  HP: {(int)_targetedDummy.CurrentHealth} / {(int)_targetedDummy.MaxHealth}";
            Vector2 size = _font.MeasureString(info);
            spriteBatch.DrawString(_font, info, new Vector2((_graphicsDevice.Viewport.Width - size.X) / 2, 10), Color.Yellow);
        }

        // Draw placeholder spell bar UI (8 slots)
        DrawSpellBar(spriteBatch);

        spriteBatch.End();

        // Draw training dummy health bars (3D to 2D projection)
        spriteBatch.Begin();
        _trainingDummyRenderer.DrawDummyHealthBars(spriteBatch, _trainingDummies, _cameraController.View, _cameraController.Projection, _targetedDummy);
        spriteBatch.End();
    }

    // Draws the player health and resource bars at the top left
    private void DrawPlayerHealthAndResourceBars(SpriteBatch spriteBatch)
    {
        // Bar settings
        int barWidth = 280;
        int healthBarHeight = 28;
        int resourceBarHeight = 16;
        int x = 24;
        int y = 24;
        int spacing = 4;
        float healthPercent = _playerCurrentHealth / _playerMaxHealth;
        Color healthColor;
        Color borderColor = Color.White;
        Color textColor = Color.White;
        // Class color for health bar
        switch (_character.Class)
        {
            case CharacterClass.Brawler:
                healthColor = new Color(255, 140, 0); // Orange
                break;
            case CharacterClass.Ranger:
                healthColor = new Color(0, 200, 80); // Green
                break;
            case CharacterClass.Spellcaster:
                healthColor = new Color(0, 200, 220); // Teal/Light Blue
                break;
            default:
                healthColor = Color.Gray;
                break;
        }
        // Draw health bar background
        spriteBatch.Draw(_whiteTexture, new Rectangle(x, y, barWidth, healthBarHeight), Color.Black);
        // Draw health fill
        int fillWidth = (int)(barWidth * healthPercent);
        if (fillWidth > 0)
            spriteBatch.Draw(_whiteTexture, new Rectangle(x, y, fillWidth, healthBarHeight), healthColor);
        // Draw border
        int border = 2;
        spriteBatch.Draw(_whiteTexture, new Rectangle(x, y, barWidth, border), borderColor); // Top
        spriteBatch.Draw(_whiteTexture, new Rectangle(x, y + healthBarHeight - border, barWidth, border), borderColor); // Bottom
        spriteBatch.Draw(_whiteTexture, new Rectangle(x, y, border, healthBarHeight), borderColor); // Left
        spriteBatch.Draw(_whiteTexture, new Rectangle(x + barWidth - border, y, border, healthBarHeight), borderColor); // Right
        // Draw player name (left)
        string name = _character.Name;
        Vector2 nameSize = _font.MeasureString(name);
        spriteBatch.DrawString(_font, name, new Vector2(x + 8, y + (healthBarHeight - nameSize.Y) / 2), textColor);
        // Draw health value (right)
        string healthLabel = $"{(int)_playerCurrentHealth} / {(int)_playerMaxHealth}";
        Vector2 healthSize = _font.MeasureString(healthLabel);
        spriteBatch.DrawString(_font, healthLabel, new Vector2(x + barWidth - healthSize.X - 8, y + (healthBarHeight - healthSize.Y) / 2), textColor);

        // Resource bar directly under health bar
        float resourcePercent = _character.GetResourcePercentage();
        int resourceY = y + healthBarHeight + spacing;
        Color resourceColor;
        string classLabel;
        switch (_character.Class)
        {
            case CharacterClass.Brawler:
                resourceColor = new Color(255, 140, 0); // Orange for Rage
                classLabel = "Brawler";
                break;
            case CharacterClass.Ranger:
                resourceColor = new Color(0, 200, 80); // Green for Energy
                classLabel = "Ranger";
                break;
            case CharacterClass.Spellcaster:
                resourceColor = new Color(0, 200, 220); // Teal/Light Blue for Mana
                classLabel = "Spellcaster";
                break;
            default:
                resourceColor = Color.Gray;
                classLabel = _character.Class.ToString();
                break;
        }
        // Draw resource bar background
        spriteBatch.Draw(_whiteTexture, new Rectangle(x, resourceY, barWidth, resourceBarHeight), Color.Black);
        // Draw resource fill
        int resourceFillWidth = (int)(barWidth * resourcePercent);
        if (resourceFillWidth > 0)
            spriteBatch.Draw(_whiteTexture, new Rectangle(x, resourceY, resourceFillWidth, resourceBarHeight), resourceColor);
        // Draw border
        spriteBatch.Draw(_whiteTexture, new Rectangle(x, resourceY, barWidth, border), borderColor); // Top
        spriteBatch.Draw(_whiteTexture, new Rectangle(x, resourceY + resourceBarHeight - border, barWidth, border), borderColor); // Bottom
        spriteBatch.Draw(_whiteTexture, new Rectangle(x, resourceY, border, resourceBarHeight), borderColor); // Left
        spriteBatch.Draw(_whiteTexture, new Rectangle(x + barWidth - border, resourceY, border, resourceBarHeight), borderColor); // Right
        // Draw class label (left)
        Vector2 classSize = _font.MeasureString(classLabel);
        spriteBatch.DrawString(_font, classLabel, new Vector2(x + 8, resourceY + (resourceBarHeight - classSize.Y) / 2), textColor);
        // Draw resource value (right)
        string resourceLabel = $"{(int)_character.CurrentResource} / {_character.MaxResource}";
        Vector2 resourceValueSize = _font.MeasureString(resourceLabel);
        spriteBatch.DrawString(_font, resourceLabel, new Vector2(x + barWidth - resourceValueSize.X - 8, resourceY + (resourceBarHeight - resourceValueSize.Y) / 2), textColor);
    }

    /// <summary>
    /// Draw the placeholder spell bar UI with 8 empty slots
    /// </summary>
    private void DrawSpellBar(SpriteBatch spriteBatch)
    {
        int slotCount = 8;
        int slotWidth = 48;
        int slotHeight = 48;
        int slotSpacing = 12;
        int totalWidth = slotCount * slotWidth + (slotCount - 1) * slotSpacing;
        int startX = (_graphicsDevice.Viewport.Width - totalWidth) / 2;
        int y = _graphicsDevice.Viewport.Height - slotHeight - 32;
        for (int i = 0; i < slotCount; i++)
        {
            int x = startX + i * (slotWidth + slotSpacing);
            Rectangle rect = new Rectangle(x, y, slotWidth, slotHeight);
            spriteBatch.Draw(_whiteTexture, rect, Color.DarkSlateGray);
            // Draw border
            int border = 2;
            spriteBatch.Draw(_whiteTexture, new Rectangle(x, y, slotWidth, border), Color.White); // Top
            spriteBatch.Draw(_whiteTexture, new Rectangle(x, y + slotHeight - border, slotWidth, border), Color.White); // Bottom
            spriteBatch.Draw(_whiteTexture, new Rectangle(x, y, border, slotHeight), Color.White); // Left
            spriteBatch.Draw(_whiteTexture, new Rectangle(x + slotWidth - border, y, border, slotHeight), Color.White); // Right
            // Draw slot number
            string num = (i + 1).ToString();
            Vector2 numSize = _font.MeasureString(num);
            Vector2 numPos = new Vector2(x + (slotWidth - numSize.X) / 2, y + slotHeight - numSize.Y - 4);
            spriteBatch.DrawString(_font, num, numPos, Color.LightGray);
        }
    }

    /// <summary>
    /// Get the display color for the character class
    /// </summary>
    private Color GetClassColor()
    {
        switch (_character.Class)
        {
            case CharacterClass.Brawler:
                return Color.Red;
            case CharacterClass.Ranger:
                return Color.Green;
            case CharacterClass.Spellcaster:
                return Color.Blue;
            default:
                return Color.Gray;
        }
    }

    /// <summary>
    /// Get training dummies (useful for future combat system)
    /// </summary>
    public List<TrainingDummy> GetTrainingDummies()
    {
        return _trainingDummies;
    }

    /// <summary>
    /// Dispose of resources when state is destroyed
    /// </summary>
    public void Dispose()
    {
        _cubeRenderer?.Dispose();
        _environmentRenderer?.Dispose();
        _trainingDummyRenderer?.Dispose();
        _basicEffect?.Dispose();
        _whiteTexture?.Dispose();
    }

    private TrainingDummy GetDummyUnderMouse(Point mousePosition)
    {
        // Convert mouse to world ray
        Vector3 nearPoint = _graphicsDevice.Viewport.Unproject(new Vector3(mousePosition.X, mousePosition.Y, 0), _cameraController.Projection, _cameraController.View, Matrix.Identity);
        Vector3 farPoint = _graphicsDevice.Viewport.Unproject(new Vector3(mousePosition.X, mousePosition.Y, 1), _cameraController.Projection, _cameraController.View, Matrix.Identity);
        Vector3 direction = Vector3.Normalize(farPoint - nearPoint);
        float closestDist = float.MaxValue;
        TrainingDummy closestDummy = null;
        foreach (var dummy in _trainingDummies)
        {
            if (!dummy.IsAlive) continue;
            BoundingBox box = new BoundingBox(dummy.Position - new Vector3(dummy.Scale/2), dummy.Position + new Vector3(dummy.Scale/2));
            float? dist = box.Intersects(new Ray(nearPoint, direction));
            if (dist.HasValue && dist.Value < closestDist)
            {
                closestDist = dist.Value;
                closestDummy = dummy;
            }
        }
        return closestDummy;
    }
}