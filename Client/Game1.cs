﻿
namespace project_axiom;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private GameState _currentState;
    private GameState _nextState;

    public void ChangeState(GameState state)
    {
        _nextState = state;
    }

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // Set initial screen size (optional)
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges();

        base.Initialize();
    }    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Check if player is already authenticated, if not start with authentication
        if (PlayerAuthenticationManager.IsAuthenticated)
        {
            // Player is already authenticated, go to main menu
            _currentState = new MainMenuState(this, _graphics.GraphicsDevice, Content);
        }
        else
        {
            // Player needs to authenticate first
            _currentState = new AuthenticationState(this, _graphics.GraphicsDevice, Content);
        }
        
        _currentState.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        // Handle state transitioning
        if (_nextState != null)
        {
            _currentState = _nextState;
            _currentState.LoadContent(); // Load content for the new state
            _nextState = null; // Clear the next state
        }

        // Update the current state
        _currentState.Update(gameTime);

        // Call PostUpdate for any logic that needs to run after the main update (e.g., state changes)
        _currentState.PostUpdate(gameTime);

        // Global escape condition (optional, can be handled within states if preferred)
        // if (Keyboard.GetState().IsKeyDown(Keys.Escape))
        //    Exit();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // The current state is responsible for clearing the screen and drawing
        _currentState.Draw(gameTime, _spriteBatch);

        base.Draw(gameTime);
    }
}