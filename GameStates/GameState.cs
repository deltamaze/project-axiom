namespace project_axiom.GameStates;

public abstract class GameState
{
    protected ContentManager _content;
    protected GraphicsDevice _graphicsDevice;
    protected Game1 _game;

    public GameState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
    {
        _game = game;
        _graphicsDevice = graphicsDevice;
        _content = content;
    }

    public abstract void LoadContent();
    public abstract void Update(GameTime gameTime);
    public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
    public abstract void PostUpdate(GameTime gameTime); // For handling state transitions after updates
}
