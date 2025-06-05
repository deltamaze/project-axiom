

namespace project_axiom.GameStates;

public class MainMenuState : GameState
{
    private List<Button> _buttons;
    private SpriteFont _buttonFont;
    private SpriteFont _titleFont;
    // private Texture2D _buttonTexture; // Optional: if you create a button background image

    public MainMenuState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
      : base(game, graphicsDevice, content)
    {
    }

    public override void LoadContent()
    {
        _buttonFont = _content.Load<SpriteFont>("Fonts/DefaultFont");
        _titleFont = _buttonFont; // Using same font for now, can be different later
                                  // _buttonTexture = _content.Load<Texture2D>("Path/To/Your/ButtonTexture"); // If you have one

        // Updated button - now goes to Character Creation instead of directly to Training Grounds
        var newGameButton = new Button(_buttonFont, "New Game" /*, _buttonTexture (optional) */)
        {
            Position = new Vector2((_graphicsDevice.Viewport.Width / 2f) - 100, 200),
            PenColour = Color.AntiqueWhite, // Example text color
            BackgroundColour = new Color(50, 80, 50), // Green for new game
            BackgroundHoverColour = new Color(70, 100, 70) // Brighter green on hover
        };
        newGameButton.Click += NewGameButton_Click;

        var quitButton = new Button(_buttonFont, "Quit" /*, _buttonTexture (optional) */)
        {
            Position = new Vector2((_graphicsDevice.Viewport.Width / 2f) - 100, 280),
            PenColour = Color.AntiqueWhite,
            BackgroundColour = new Color(100, 50, 50),
            BackgroundHoverColour = new Color(150, 80, 80)
        };
        quitButton.Click += QuitButton_Click;

        _buttons = new List<Button>()
            {
                newGameButton,
                quitButton,
            };

        // Ensure button rectangles are initialized after setting position
        foreach (var button in _buttons)
        {
            button.Position = button.Position; // This calls the setter which updates the rectangle
        }
    }

    private void NewGameButton_Click(object sender, System.EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("New Game Clicked!"); // Use System.Diagnostics.Debug for output
                                                                 // Transition to Character Creation instead of directly to Training Grounds
        _game.ChangeState(new CharacterCreationState(_game, _graphicsDevice, _content));
    }

    private void QuitButton_Click(object sender, System.EventArgs e)
    {
        _game.Exit();
    }

    public override void Update(GameTime gameTime)
    {
        foreach (var button in _buttons)
            button.Update(gameTime);
    }

    public override void PostUpdate(GameTime gameTime)
    {
        // Logic to transition to another state can be put here if needed
        // For example, if a button click sets a flag to change state.
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        _graphicsDevice.Clear(new Color(20, 20, 40)); // Dark blue background for main menu

        spriteBatch.Begin();

        // Draw game title
        string gameTitle = "Block Brawlers";
        var titleSize = _titleFont.MeasureString(gameTitle);
        var titlePosition = new Vector2((_graphicsDevice.Viewport.Width / 2f) - (titleSize.X / 2f), 100);
        spriteBatch.DrawString(_titleFont, gameTitle, titlePosition, Color.White);

        // Draw subtitle
        string subtitle = "Choose your class and enter the arena!";
        var subtitleSize = _buttonFont.MeasureString(subtitle);
        var subtitlePosition = new Vector2((_graphicsDevice.Viewport.Width / 2f) - (subtitleSize.X / 2f), 140);
        spriteBatch.DrawString(_buttonFont, subtitle, subtitlePosition, Color.LightGray);

        // Draw buttons
        foreach (var button in _buttons)
            button.Draw(spriteBatch);

        spriteBatch.End();
    }
}
