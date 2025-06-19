using project_axiom.Shared;

namespace project_axiom.GameStates;

public class CharacterSelectionState : GameState
{
    private SpriteFont _font;
    private SpriteFont _titleFont;
    private List<Button> _buttons;
    private Button _createNewButton;
    private Button _playButton;
    private Button _deleteButton;
    private Button _backButton;
    private Character _loadedCharacter;
    private bool _isLoading = true;
    private string _statusMessage = "Loading character data...";
    private bool _hasError = false;

    public CharacterSelectionState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
        : base(game, graphicsDevice, content)
    {
    }

    public override void LoadContent()
    {
        _font = _content.Load<SpriteFont>("Fonts/DefaultFont");
        _titleFont = _font;

        _buttons = new List<Button>();        // Create New Character button
        _createNewButton = new Button(_font, "Create New Character")
        {
            Position = new Vector2((_graphicsDevice.Viewport.Width / 2f) - 100, 350),
            PenColour = Color.White,
            BackgroundColour = new Color(50, 80, 50),
            BackgroundHoverColour = new Color(70, 100, 70)
        };
        _createNewButton.Click += CreateNewButton_Click;
        _buttons.Add(_createNewButton);

        // Play with existing character button (initially hidden)
        _playButton = new Button(_font, "Enter Training Grounds")
        {
            Position = new Vector2((_graphicsDevice.Viewport.Width / 2f) - 100, 300),
            PenColour = Color.White,
            BackgroundColour = new Color(50, 50, 80),
            BackgroundHoverColour = new Color(70, 70, 100),
            IsVisible = false
        };
        _playButton.Click += PlayButton_Click;
        _buttons.Add(_playButton);

        // Delete character button (initially hidden)
        _deleteButton = new Button(_font, "Delete Character")
        {
            Position = new Vector2((_graphicsDevice.Viewport.Width / 2f) - 100, 400),
            PenColour = Color.White,
            BackgroundColour = new Color(80, 50, 50),
            BackgroundHoverColour = new Color(100, 70, 70),
            IsVisible = false
        };
        _deleteButton.Click += DeleteButton_Click;
        _buttons.Add(_deleteButton);

        // Back button
        _backButton = new Button(_font, "Back to Main Menu")
        {
            Position = new Vector2((_graphicsDevice.Viewport.Width / 2f) - 100, 450),
            PenColour = Color.White,
            BackgroundColour = new Color(60, 60, 80),
            BackgroundHoverColour = new Color(80, 80, 100)
        };
        _backButton.Click += BackButton_Click;
        _buttons.Add(_backButton);

        // Ensure button rectangles are initialized
        foreach (var button in _buttons)
        {
            button.Position = button.Position;
        }

        // Load character data from PlayFab
        LoadCharacterData();
    }

    private void LoadCharacterData()
    {
        _isLoading = true;
        _statusMessage = "Loading character data...";
        
        PlayerAuthenticationManager.LoadCharacter(
            character =>
            {
                _isLoading = false;
                _loadedCharacter = character;
                
                if (character != null)
                {
                    _statusMessage = $"Welcome back, {character.Name}!";
                    _playButton.IsVisible = true;
                    _deleteButton.IsVisible = true;
                    _createNewButton.Text = "Create New Character";
                }
                else
                {
                    _statusMessage = "No character found. Create your first character!";
                    _playButton.IsVisible = false;
                    _deleteButton.IsVisible = false;
                    _createNewButton.Text = "Create Character";
                }
                _hasError = false;
            },
            error =>
            {
                _isLoading = false;
                _statusMessage = $"Error loading character: {error}";
                _hasError = true;
                _playButton.IsVisible = false;
                _deleteButton.IsVisible = false;
            }
        );
    }

    private void CreateNewButton_Click(object sender, System.EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Create New Character Clicked!");
        _game.ChangeState(new CharacterCreationState(_game, _graphicsDevice, _content));
    }

    private void PlayButton_Click(object sender, System.EventArgs e)
    {
        if (_loadedCharacter != null)
        {
            System.Diagnostics.Debug.WriteLine($"Playing with character: {_loadedCharacter.Name}");
            _game.ChangeState(new TrainingGroundsState(_game, _graphicsDevice, _content, _loadedCharacter));
        }
    }    private void DeleteButton_Click(object sender, System.EventArgs e)
    {
        if (_loadedCharacter != null)
        {
            System.Diagnostics.Debug.WriteLine($"Deleting character: {_loadedCharacter.Name}");
            
            // Delete character data from PlayFab
            PlayerAuthenticationManager.DeleteCharacter(
                successMessage =>
                {
                    System.Diagnostics.Debug.WriteLine("Character deleted successfully");
                    LoadCharacterData(); // Reload to update UI
                },
                errorMessage =>
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting character: {errorMessage}");
                    _statusMessage = $"Error deleting character: {errorMessage}";
                    _hasError = true;
                }
            );
        }
    }

    private void BackButton_Click(object sender, System.EventArgs e)
    {
        _game.ChangeState(new MainMenuState(_game, _graphicsDevice, _content));
    }

    public override void Update(GameTime gameTime)
    {
        foreach (var button in _buttons)
        {
            button.Update(gameTime);
        }

        // Handle escape key to go back
        var keyboardState = Keyboard.GetState();
        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            _game.ChangeState(new MainMenuState(_game, _graphicsDevice, _content));
        }
    }

    public override void PostUpdate(GameTime gameTime)
    {
        // No post-update logic needed
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        _graphicsDevice.Clear(new Color(25, 25, 45));

        spriteBatch.Begin();        // Draw title
        string title = "Character Selection";
        var titleSize = _titleFont.MeasureString(title);
        var titlePosition = new Vector2((_graphicsDevice.Viewport.Width / 2f) - (titleSize.X / 2f), 80);
        spriteBatch.DrawString(_titleFont, title, titlePosition, Color.White);

        // Draw status message
        var statusColor = _hasError ? Color.Red : (_isLoading ? Color.Yellow : Color.LightGray);
        var statusSize = _font.MeasureString(_statusMessage);
        var statusPosition = new Vector2((_graphicsDevice.Viewport.Width / 2f) - (statusSize.X / 2f), 150);
        spriteBatch.DrawString(_font, _statusMessage, statusPosition, statusColor);

        // Draw character info if loaded
        if (!_isLoading && _loadedCharacter != null && !_hasError)
        {
            string characterInfo = $"Class: {_loadedCharacter.Class}";
            string healthInfo = $"Health: {_loadedCharacter.MaxHealth}";
            string resourceInfo = $"{_loadedCharacter.ResourceType}: {_loadedCharacter.MaxResource}";

            var charInfoPosition = new Vector2((_graphicsDevice.Viewport.Width / 2f) - 100, 200);
            spriteBatch.DrawString(_font, characterInfo, charInfoPosition, _loadedCharacter.GetClassColor());
            
            var healthInfoPosition = new Vector2((_graphicsDevice.Viewport.Width / 2f) - 100, 230);
            spriteBatch.DrawString(_font, healthInfo, healthInfoPosition, Color.LightGreen);
            
            var resourceInfoPosition = new Vector2((_graphicsDevice.Viewport.Width / 2f) - 100, 260);
            spriteBatch.DrawString(_font, resourceInfo, resourceInfoPosition, _loadedCharacter.GetResourceColor());
        }

        // Draw buttons
        foreach (var button in _buttons)
        {
            button.Draw(spriteBatch);
        }

        // Draw instructions
        string instructions = "ESC to go back";
        spriteBatch.DrawString(_font, instructions, new Vector2(10, _graphicsDevice.Viewport.Height - 30), Color.Gray);

        spriteBatch.End();
    }
}
