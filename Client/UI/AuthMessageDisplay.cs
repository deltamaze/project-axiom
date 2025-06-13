namespace project_axiom.UI;

/// <summary>
/// Simple message display for authentication screens
/// </summary>
public class AuthMessageDisplay
{
    private SpriteFont _font;
    private string _currentMessage = "";
    private Color _currentColor = Color.White;
    private float _messageTimer = 0f;
    
    public Vector2 Position { get; set; }
    public int MaxWidth { get; set; } = 400;
    public bool CenterAlign { get; set; } = false;

    public AuthMessageDisplay(SpriteFont font)
    {
        _font = font;
    }
    
    /// <summary>
    /// Show a message with specified color and duration
    /// </summary>
    public void ShowMessage(string message, Color color, int durationMs)
    {
        _currentMessage = message;
        _currentColor = color;
        _messageTimer = durationMs / 1000f; // Convert to seconds
    }
    
    /// <summary>
    /// Clear the current message
    /// </summary>
    public void Clear()
    {
        _currentMessage = "";
        _messageTimer = 0f;
    }
    
    /// <summary>
    /// Update the message timer
    /// </summary>
    public void Update(GameTime gameTime)
    {
        if (_messageTimer > 0f)
        {
            _messageTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_messageTimer <= 0f)
            {
                _currentMessage = "";
            }
        }
    }
    
    /// <summary>
    /// Draw the message
    /// </summary>
    public void Draw(SpriteBatch spriteBatch)
    {
        if (!string.IsNullOrEmpty(_currentMessage))
        {
            var messageSize = _font.MeasureString(_currentMessage);
            Vector2 drawPosition;
            
            if (CenterAlign)
            {
                drawPosition = new Vector2(Position.X - (messageSize.X / 2f), Position.Y);
            }
            else
            {
                drawPosition = Position;
            }
            
            spriteBatch.DrawString(_font, _currentMessage, drawPosition, _currentColor);
        }
    }
}
