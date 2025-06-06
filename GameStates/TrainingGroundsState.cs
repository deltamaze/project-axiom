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

    // Constants
    private const float PLAYER_GROUND_OFFSET = 0.51f;

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

        // Draw training dummies
        _trainingDummyRenderer.DrawDummies(_basicEffect, _trainingDummies);

        // Draw player cube
        _cubeRenderer.Draw(_basicEffect, _playerController.Position);

        // Draw UI
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

        // Character information
        spriteBatch.DrawString(_font,
            $"Character: {_character.Name} ({_character.Class})",
            new Vector2(10, 10),
            Color.White);

        spriteBatch.DrawString(_font,
            $"Health: {_character.MaxHealth} | {_character.ResourceType}: {_character.MaxResource}",
            new Vector2(10, 30),
            Color.LightBlue);

        // Draw the class-specific resource bar (Section 6.8 implementation)
        DrawResourceBar(spriteBatch, new Vector2(10, 50));

        // Training area info
        spriteBatch.DrawString(_font,
            $"Training Grounds ({GeometryBuilder.GROUND_SIZE}x{GeometryBuilder.GROUND_SIZE} area) - WASD to move, Mouse to look around",
            new Vector2(10, 80),
            Color.White);
        spriteBatch.DrawString(_font,
            "Space/Shift for up/down, M to toggle mouse capture, ESC to return to menu",
            new Vector2(10, 100),
            Color.White);

        // Position information
        Vector3 pos = _playerController.Position;
        spriteBatch.DrawString(_font,
            $"Position: X:{pos.X:F1} Y:{pos.Y:F1} Z:{pos.Z:F1}",
            new Vector2(10, 130),
            Color.Yellow);

        // Class-specific tip
        string classTip = _playerController.GetClassTip();
        spriteBatch.DrawString(_font,
            classTip,
            new Vector2(10, 150),
            Color.LightGreen);

        // Environment status
        spriteBatch.DrawString(_font,
            "Environment: Ground plane and boundary walls active (z-fighting resolved)",
            new Vector2(10, 170),
            Color.LightGreen);

        // Training dummy information
        spriteBatch.DrawString(_font,
            $"Training Dummies: {_trainingDummies.Count} placed - Look around to see them!",
            new Vector2(10, 190),
            Color.Orange);

        spriteBatch.DrawString(_font,
            "Orange/brown cubes are training dummies for future combat practice",
            new Vector2(10, 210),
            Color.Orange);

        spriteBatch.End();

        // Draw training dummy health bars (3D to 2D projection)
        spriteBatch.Begin();
        _trainingDummyRenderer.DrawDummyHealthBars(spriteBatch, _trainingDummies, _cameraController.View, _cameraController.Projection);
        spriteBatch.End();
    }

    /// <summary>
    /// Draw the class-specific resource bar (Section 6.8 implementation)
    /// </summary>
    private void DrawResourceBar(SpriteBatch spriteBatch, Vector2 position)
    {
        // Resource bar dimensions
        int barWidth = 200;
        int barHeight = 20;
        int borderWidth = 2;

        // Create temporary texture for drawing rectangles
        Texture2D pixelTexture = new Texture2D(_graphicsDevice, 1, 1);
        pixelTexture.SetData(new[] { Color.White });

        // Calculate resource fill percentage and width
        float resourcePercentage = _character.GetResourcePercentage();
        int innerWidth = barWidth - borderWidth * 2;
        int innerHeight = barHeight - borderWidth * 2;
        int fillWidth = Math.Max(0, (int)(innerWidth * resourcePercentage));

        // Debug output to console (remove this later)
        // System.Diagnostics.Debug.WriteLine($"Resource Bar Debug - Percentage: {resourcePercentage:F2}, FillWidth: {fillWidth}, Current: {_character.CurrentResource:F1}, Max: {_character.MaxResource}");

        // Draw resource bar background (dark gray)
        Rectangle backgroundRect = new Rectangle((int)position.X, (int)position.Y, barWidth, barHeight);
        spriteBatch.Draw(pixelTexture, backgroundRect, Color.DarkGray);

        // Draw inner background (black)
        Rectangle innerBackgroundRect = new Rectangle(
            (int)position.X + borderWidth, 
            (int)position.Y + borderWidth, 
            innerWidth, 
            innerHeight);
        spriteBatch.Draw(pixelTexture, innerBackgroundRect, Color.Black);

        // Draw resource fill (class-specific color) - ALWAYS draw something even if very small
        if (resourcePercentage > 0)
        {
            // Ensure minimum 1 pixel width if there's any resource
            int actualFillWidth = Math.Max(1, fillWidth);
            Rectangle fillRect = new Rectangle(
                (int)position.X + borderWidth,
                (int)position.Y + borderWidth,
                actualFillWidth,
                innerHeight
            );
            spriteBatch.Draw(pixelTexture, fillRect, _character.GetResourceColor());
        }

        // Draw resource bar border (white)
        // Top border
        spriteBatch.Draw(pixelTexture, new Rectangle((int)position.X, (int)position.Y, barWidth, borderWidth), Color.White);
        // Bottom border  
        spriteBatch.Draw(pixelTexture, new Rectangle((int)position.X, (int)position.Y + barHeight - borderWidth, barWidth, borderWidth), Color.White);
        // Left border
        spriteBatch.Draw(pixelTexture, new Rectangle((int)position.X, (int)position.Y, borderWidth, barHeight), Color.White);
        // Right border
        spriteBatch.Draw(pixelTexture, new Rectangle((int)position.X + barWidth - borderWidth, (int)position.Y, borderWidth, barHeight), Color.White);

        // Dispose the temporary texture
        pixelTexture.Dispose();

        // Draw resource text label
        string resourceText = $"{_character.ResourceType}: {_character.CurrentResource:F0}/{_character.MaxResource}";
        Vector2 textPosition = new Vector2(position.X + barWidth + 10, position.Y + (barHeight / 2) - (_font.MeasureString(resourceText).Y / 2));
        spriteBatch.DrawString(_font, resourceText, textPosition, _character.GetResourceColor());

        // Draw resource percentage
        string percentageText = $"({resourcePercentage * 100:F0}%)";
        Vector2 percentagePosition = new Vector2(textPosition.X + _font.MeasureString(resourceText).X + 5, textPosition.Y);
        spriteBatch.DrawString(_font, percentageText, percentagePosition, Color.LightGray);
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
    }
}