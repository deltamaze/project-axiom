
namespace project_axiom.GameStates;

public class TrainingGroundsState : GameState
{
    // Rendering components
    private BasicEffect _basicEffect;
    private CubeRenderer _cubeRenderer;
    private EnvironmentRenderer _environmentRenderer;

    // Input and camera components
    private PlayerController _playerController;
    private CameraController _cameraController;

    // Character information
    private Character _character;

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

        System.Diagnostics.Debug.WriteLine($"Character {_character.Name} ({_character.Class}) entered Training Grounds");
        System.Diagnostics.Debug.WriteLine($"Training area: {GeometryBuilder.GROUND_SIZE}x{GeometryBuilder.GROUND_SIZE} units, Wall height: {GeometryBuilder.WALL_HEIGHT} units");
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

        // Training area info
        spriteBatch.DrawString(_font,
            $"Training Grounds ({GeometryBuilder.GROUND_SIZE}x{GeometryBuilder.GROUND_SIZE} area) - WASD to move, Mouse to look around",
            new Vector2(10, 60),
            Color.White);
        spriteBatch.DrawString(_font,
            "Space/Shift for up/down, M to toggle mouse capture, ESC to return to menu",
            new Vector2(10, 80),
            Color.White);

        // Position information
        Vector3 pos = _playerController.Position;
        spriteBatch.DrawString(_font,
            $"Position: X:{pos.X:F1} Y:{pos.Y:F1} Z:{pos.Z:F1}",
            new Vector2(10, 110),
            Color.Yellow);

        // Class-specific tip
        string classTip = _playerController.GetClassTip();
        spriteBatch.DrawString(_font,
            classTip,
            new Vector2(10, 130),
            GetClassColor());

        // Environment status
        spriteBatch.DrawString(_font,
            "Environment: Ground plane and boundary walls active (z-fighting resolved)",
            new Vector2(10, 150),
            Color.LightGreen);

        spriteBatch.End();
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
    /// Dispose of resources when state is destroyed
    /// </summary>
    public void Dispose()
    {
        _cubeRenderer?.Dispose();
        _environmentRenderer?.Dispose();
        _basicEffect?.Dispose();
    }
}
