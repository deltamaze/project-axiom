using PlayFab;
using PlayFab.ClientModels;
using System.Threading.Tasks;

namespace project_axiom.GameStates;

/// <summary>
/// Handles player registration and login using PlayFab
/// </summary>
public class AuthenticationState : GameState
{
    private List<Button> _buttons;
    private List<TextInput> _textInputs;
    private SpriteFont _font;
    private SpriteFont _titleFont;
    
    private TextInput _emailInput;
    private TextInput _passwordInput;
    private AuthMessageDisplay _messageDisplay;
    
    private bool _isRegistrationMode = false; // false = login, true = register
    private bool _isProcessing = false;
    
    public AuthenticationState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
        : base(game, graphicsDevice, content)
    {
    }

    public override void LoadContent()
    {
        _font = _content.Load<SpriteFont>("Fonts/DefaultFont");
        _titleFont = _font; // Using same font for now
        
        // Initialize PlayFab
        PlayFabConfig.Initialize();
        
        // Create text inputs
        var centerX = _graphicsDevice.Viewport.Width / 2f;
        
        _emailInput = new TextInput(_font)
        {
            Position = new Vector2(centerX - 150, 250),
            Width = 300,
            Height = 40,
            PlaceholderText = "Email",
            BackgroundColor = new Color(40, 40, 40),
            TextColor = Color.White,
            PlaceholderColor = Color.Gray
        };
        
        _passwordInput = new TextInput(_font)
        {
            Position = new Vector2(centerX - 150, 310),
            Width = 300,
            Height = 40,
            PlaceholderText = "Password",
            IsPassword = true,
            BackgroundColor = new Color(40, 40, 40),
            TextColor = Color.White,
            PlaceholderColor = Color.Gray
        };
        
        _textInputs = new List<TextInput> { _emailInput, _passwordInput };
        
        // Create buttons
        var loginButton = new Button(_font, "Login")
        {
            Position = new Vector2(centerX - 200, 380),
            PenColour = Color.White,
            BackgroundColour = new Color(50, 100, 50),
            BackgroundHoverColour = new Color(70, 120, 70)
        };
        loginButton.Click += LoginButton_Click;
        
        var registerButton = new Button(_font, "Register")
        {
            Position = new Vector2(centerX - 50, 380),
            PenColour = Color.White,
            BackgroundColour = new Color(50, 50, 100),
            BackgroundHoverColour = new Color(70, 70, 120)
        };
        registerButton.Click += RegisterButton_Click;
        
        var toggleModeButton = new Button(_font, "Switch to Register")
        {
            Position = new Vector2(centerX + 100, 380),
            PenColour = Color.White,
            BackgroundColour = new Color(80, 80, 80),
            BackgroundHoverColour = new Color(100, 100, 100)
        };
        toggleModeButton.Click += ToggleModeButton_Click;
        
        _buttons = new List<Button> { loginButton, registerButton, toggleModeButton };
        
        // Initialize button rectangles
        foreach (var button in _buttons)
        {
            button.Position = button.Position;
        }
          // Create message display
        _messageDisplay = new AuthMessageDisplay(_font)
        {
            Position = new Vector2(centerX, 450),
            MaxWidth = 600,
            CenterAlign = true
        };
    }

    private void LoginButton_Click(object sender, EventArgs e)
    {
        if (_isProcessing) return;
        
        var email = _emailInput.Text.Trim();
        var password = _passwordInput.Text.Trim();
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            _messageDisplay.ShowMessage("Please enter both email and password.", Color.Red, 3000);
            return;
        }
        
        _isProcessing = true;
        _messageDisplay.ShowMessage("Logging in...", Color.Yellow, 0);
          var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password
        };
        
        PlayFabClientAPI.LoginWithEmailAddress (request, OnLoginSuccess, OnPlayFabError);
    }
    
    private void RegisterButton_Click(object sender, EventArgs e)
    {
        if (_isProcessing) return;
        
        var email = _emailInput.Text.Trim();
        var password = _passwordInput.Text.Trim();
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            _messageDisplay.ShowMessage("Please enter both email and password.", Color.Red, 3000);
            return;
        }
        
        if (password.Length < 6)
        {
            _messageDisplay.ShowMessage("Password must be at least 6 characters long.", Color.Red, 3000);
            return;
        }
        
        _isProcessing = true;
        _messageDisplay.ShowMessage("Creating account...", Color.Yellow, 0);
        
        var request = new RegisterPlayFabUserRequest
        {
            Email = email,
            Password = password,
            RequireBothUsernameAndEmail = false
        };
        
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnPlayFabError);
    }
      private void ToggleModeButton_Click(object sender, EventArgs e)
    {
        _isRegistrationMode = !_isRegistrationMode;
        var toggleButton = _buttons[2]; // Toggle mode button
        toggleButton.SetText(_isRegistrationMode ? "Switch to Login" : "Switch to Register");
        
        _emailInput.Text = "";
        _passwordInput.Text = "";
        _messageDisplay.Clear();
    }

    private void OnLoginSuccess(LoginResult result)
    {
        _isProcessing = false;
        _messageDisplay.ShowMessage($"Login successful! Welcome back, {result.PlayFabId}", Color.Green, 2000);
        
        // Store the PlayFab ID for later use
        PlayFabSettings.staticPlayer.PlayFabId = result.PlayFabId;
        
        // Transition to the main menu after a short delay
        Task.Delay(2000).ContinueWith(_ =>
        {
            // This will be executed on a background thread, so we need to be careful
            // For now, we'll just set a flag and handle the transition in Update
            _game.ChangeState(new MainMenuState(_game, _graphicsDevice, _content));
        });
    }
    
    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        _isProcessing = false;
        _messageDisplay.ShowMessage($"Registration successful! Welcome, {result.PlayFabId}", Color.Green, 2000);
        
        // Store the PlayFab ID for later use
        PlayFabSettings.staticPlayer.PlayFabId = result.PlayFabId;
        
        // Transition to the main menu after a short delay
        Task.Delay(2000).ContinueWith(_ =>
        {
            _game.ChangeState(new MainMenuState(_game, _graphicsDevice, _content));
        });
    }
    
    private void OnPlayFabError(PlayFabError error)
    {
        _isProcessing = false;
        
        string errorMessage = error.ErrorMessage ?? "An unknown error occurred.";
        
        // Handle specific error codes with user-friendly messages
        switch (error.Error)
        {
            case PlayFabErrorCode.InvalidEmailAddress:
                errorMessage = "Please enter a valid email address.";
                break;
            case PlayFabErrorCode.InvalidPassword:
                errorMessage = "Invalid password. Password must be at least 6 characters.";
                break;
            case PlayFabErrorCode.EmailAddressNotAvailable:
                errorMessage = "An account with this email already exists. Try logging in instead.";
                break;
            case PlayFabErrorCode.AccountNotFound:
                errorMessage = "Account not found. Please check your email or register a new account.";
                break;
            case PlayFabErrorCode.InvalidEmailOrPassword:
                errorMessage = "Invalid email or password. Please try again.";
                break;
        }
        
        _messageDisplay.ShowMessage($"Error: {errorMessage}", Color.Red, 5000);
    }

    public override void Update(GameTime gameTime)
    {
        if (!_isProcessing)
        {
            foreach (var textInput in _textInputs)
                textInput.Update(gameTime);
                
            foreach (var button in _buttons)
                button.Update(gameTime);
        }
        
        _messageDisplay.Update(gameTime);
        
        // Handle Enter key for quick login/register
        var keyboardState = Keyboard.GetState();
        if (keyboardState.IsKeyDown(Keys.Enter))
        {
            if (_isRegistrationMode)
                RegisterButton_Click(this, EventArgs.Empty);
            else
                LoginButton_Click(this, EventArgs.Empty);
        }
    }

    public override void PostUpdate(GameTime gameTime)
    {
        // Any post-update logic if needed
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        _graphicsDevice.Clear(new Color(15, 15, 25)); // Dark background
        
        spriteBatch.Begin();
        
        var centerX = _graphicsDevice.Viewport.Width / 2f;
        
        // Draw title
        string title = "Block Brawlers";
        var titleSize = _titleFont.MeasureString(title);
        var titlePosition = new Vector2(centerX - (titleSize.X / 2f), 80);
        spriteBatch.DrawString(_titleFont, title, titlePosition, Color.White);
        
        // Draw subtitle
        string subtitle = _isRegistrationMode ? "Create New Account" : "Login to Continue";
        var subtitleSize = _font.MeasureString(subtitle);
        var subtitlePosition = new Vector2(centerX - (subtitleSize.X / 2f), 140);
        spriteBatch.DrawString(_font, subtitle, subtitlePosition, Color.LightGray);
        
        // Draw labels
        spriteBatch.DrawString(_font, "Email:", new Vector2(_emailInput.Position.X, _emailInput.Position.Y - 25), Color.White);
        spriteBatch.DrawString(_font, "Password:", new Vector2(_passwordInput.Position.X, _passwordInput.Position.Y - 25), Color.White);
        
        // Draw text inputs
        foreach (var textInput in _textInputs)
            textInput.Draw(spriteBatch);
        
        // Draw buttons (only show relevant ones based on mode)
        if (!_isProcessing)
        {
            foreach (var button in _buttons)
                button.Draw(spriteBatch);
        }
        
        // Draw message
        _messageDisplay.Draw(spriteBatch);
        
        // Draw instructions
        if (!_isProcessing)
        {
            string instructions = "Press Enter to submit, or use the buttons above";
            var instructionsSize = _font.MeasureString(instructions);
            var instructionsPosition = new Vector2(centerX - (instructionsSize.X / 2f), _graphicsDevice.Viewport.Height - 60);
            spriteBatch.DrawString(_font, instructions, instructionsPosition, Color.Gray);
        }
        
        spriteBatch.End();
    }
}
