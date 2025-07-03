using project_axiom.Shared;
using System.Threading.Tasks;

namespace project_axiom.GameStates;

/// <summary>
/// Game state that handles server allocation and shows loading progress
/// </summary>
public class ServerAllocationState : GameState
{
    private readonly Character _character;
    private SpriteFont _font;
    private SpriteFont _titleFont;
    private ServerAllocationManager _serverManager;
    private bool _isAllocating = true;
    private string _statusMessage = "Requesting server...";
    private Color _statusColor = Color.Yellow;
    private Button _retryButton;
    private Button _backButton;
    private float _loadingDots = 0f;
    private bool _hasError = false;

    public ServerAllocationState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content, Character character)
        : base(game, graphicsDevice, content)
    {
        _character = character ?? throw new ArgumentNullException(nameof(character));
        _serverManager = new ServerAllocationManager();
    }

    public override void LoadContent()
    {
        _font = _content.Load<SpriteFont>("Fonts/DefaultFont");
        _titleFont = _font;

        // Retry button (initially hidden)
        _retryButton = new Button(_font, "Retry Connection")
        {
            Position = new Vector2((_graphicsDevice.Viewport.Width / 2f) - 100, 350),
            PenColour = Color.White,
            BackgroundColour = new Color(80, 50, 50),
            BackgroundHoverColour = new Color(100, 70, 70),
            IsVisible = false
        };
        _retryButton.Click += RetryButton_Click;

        // Back button
        _backButton = new Button(_font, "Back to Character Selection")
        {
            Position = new Vector2((_graphicsDevice.Viewport.Width / 2f) - 120, 400),
            PenColour = Color.White,
            BackgroundColour = new Color(50, 50, 50),
            BackgroundHoverColour = new Color(70, 70, 70),
            IsVisible = false
        };
        _backButton.Click += BackButton_Click;

        // Start server allocation immediately
        _ = Task.Run(AllocateServerAsync);
    }

    private async Task AllocateServerAsync()
    {
        try
        {
            _statusMessage = "Requesting training server...";
            _statusColor = Color.Yellow;
            
            // Add a small delay to show the loading state
            await Task.Delay(1000);

            bool success = await _serverManager.RequestTrainingGroundsServerAsync();

            if (success)
            {
                _statusMessage = "Server allocated! Connecting...";
                _statusColor = Color.LightGreen;
                
                await Task.Delay(500); // Brief pause to show success message
                
                // Transition to Training Grounds with server connection
                var trainingGroundsState = new TrainingGroundsState(_game, _graphicsDevice, _content, _character, _serverManager);
                _game.ChangeState(trainingGroundsState);
            }
            else
            {
                _statusMessage = $"Failed to allocate server: {_serverManager.LastError ?? "Unknown error"}";
                _statusColor = Color.Red;
                _hasError = true;
                _isAllocating = false;
                _retryButton.IsVisible = true;
                _backButton.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            _statusMessage = $"Allocation error: {ex.Message}";
            _statusColor = Color.Red;
            _hasError = true;
            _isAllocating = false;
            _retryButton.IsVisible = true;
            _backButton.IsVisible = true;
        }
    }

    private void RetryButton_Click(object sender, System.EventArgs e)
    {
        _isAllocating = true;
        _hasError = false;
        _statusMessage = "Retrying server allocation...";
        _statusColor = Color.Yellow;
        _retryButton.IsVisible = false;
        _backButton.IsVisible = false;
        
        _ = Task.Run(AllocateServerAsync);
    }

    private void BackButton_Click(object sender, System.EventArgs e)
    {
        _serverManager?.Dispose();
        _game.ChangeState(new CharacterSelectionState(_game, _graphicsDevice, _content));
    }

    public override void Update(GameTime gameTime)
    {
        // Update loading animation
        if (_isAllocating)
        {
            _loadingDots += (float)gameTime.ElapsedGameTime.TotalSeconds * 2f;
            if (_loadingDots > 3f)
                _loadingDots = 0f;
        }

        // Update buttons
        if (_retryButton.IsVisible)
            _retryButton.Update(gameTime);
        if (_backButton.IsVisible)
            _backButton.Update(gameTime);

        // Handle ESC key to go back
        var keyState = Keyboard.GetState();
        if (keyState.IsKeyDown(Keys.Escape) && !_previousKeyState.IsKeyDown(Keys.Escape))
        {
            BackButton_Click(this, EventArgs.Empty);
        }
        _previousKeyState = keyState;
    }

    private KeyboardState _previousKeyState;

    public override void PostUpdate(GameTime gameTime)
    {
        // No post-update logic needed
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        _graphicsDevice.Clear(new Color(25, 25, 45));

        spriteBatch.Begin();

        // Draw title
        string title = "Connecting to Server";
        var titleSize = _titleFont.MeasureString(title);
        var titlePosition = new Vector2((_graphicsDevice.Viewport.Width / 2f) - (titleSize.X / 2f), 80);
        spriteBatch.DrawString(_titleFont, title, titlePosition, Color.White);

        // Draw character info
        string characterInfo = $"Character: {_character.Name} ({_character.Class})";
        var charSize = _font.MeasureString(characterInfo);
        var charPosition = new Vector2((_graphicsDevice.Viewport.Width / 2f) - (charSize.X / 2f), 150);
        spriteBatch.DrawString(_font, characterInfo, charPosition, _character.GetClassColor());

        // Draw status message with loading animation
        string displayMessage = _statusMessage;
        if (_isAllocating)
        {
            int dots = (int)_loadingDots;
            displayMessage += new string('.', dots + 1);
        }

        var statusSize = _font.MeasureString(displayMessage);
        var statusPosition = new Vector2((_graphicsDevice.Viewport.Width / 2f) - (statusSize.X / 2f), 220);
        spriteBatch.DrawString(_font, displayMessage, statusPosition, _statusColor);

        // Draw server info if available
        if (_serverManager.IsConnected)
        {
            string serverInfo = $"Server: {_serverManager.ServerIP}:{_serverManager.ServerPort}";
            var serverSize = _font.MeasureString(serverInfo);
            var serverPosition = new Vector2((_graphicsDevice.Viewport.Width / 2f) - (serverSize.X / 2f), 260);
            spriteBatch.DrawString(_font, serverInfo, serverPosition, Color.LightBlue);
        }

        // Draw buttons if visible
        if (_retryButton.IsVisible)
            _retryButton.Draw(spriteBatch);
        if (_backButton.IsVisible)
            _backButton.Draw(spriteBatch);

        // Draw instructions
        if (!_isAllocating)
        {
            string instructions = "ESC to go back";
            spriteBatch.DrawString(_font, instructions, new Vector2(10, _graphicsDevice.Viewport.Height - 30), Color.Gray);
        }

        spriteBatch.End();
    }
}
