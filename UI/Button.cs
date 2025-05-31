using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace project_axiom.UI
{
    public class Button
    {
        private Texture2D _texture; // Optional: for button background image
        private SpriteFont _font;
        private string _text;
        private Vector2 _position;
        private Rectangle _rectangle;
        private bool _isHovering;
        private MouseState _previousMouse;
        private MouseState _currentMouse;

        public event EventHandler Click;
        public bool Clicked { get; private set; }
        public Color PenColour { get; set; } = Color.White; // Text color
        public Color HoverColour { get; set; } = Color.LightGray; // Text color on hover
        public Color BackgroundColour { get; set; } = Color.DarkSlateGray; // Default button background
        public Color BackgroundHoverColour { get; set; } = Color.SlateGray; // Button background on hover
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                UpdateRectangle();
            }
        }
        public string Text { get { return _text; } }

        public Button(SpriteFont font, string text, Texture2D texture = null)
        {
            _font = font;
            _text = text;
            _texture = texture; // Can be null if using solid color background
        }

        private void UpdateRectangle()
        {
            var textSize = _font.MeasureString(_text);
            int width = _texture?.Width ?? (int)textSize.X + 20; // Add padding if no texture
            int height = _texture?.Height ?? (int)textSize.Y + 10; // Add padding if no texture
            _rectangle = new Rectangle((int)_position.X, (int)_position.Y, width, height);
        }

        public void Update(GameTime gameTime)
        {
            _previousMouse = _currentMouse;
            _currentMouse = Mouse.GetState();
            var mouseRectangle = new Rectangle(_currentMouse.X, _currentMouse.Y, 1, 1);

            _isHovering = false;
            Clicked = false;

            if (mouseRectangle.Intersects(_rectangle))
            {
                _isHovering = true;
                if (_currentMouse.LeftButton == ButtonState.Released && _previousMouse.LeftButton == ButtonState.Pressed)
                {
                    Click?.Invoke(this, EventArgs.Empty);
                    Clicked = true;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw background
            Color currentBgColor = _isHovering ? BackgroundHoverColour : BackgroundColour;
            if (_texture != null)
            {
                spriteBatch.Draw(_texture, _rectangle, _isHovering ? Color.LightGray : Color.White); // Modulate texture color on hover
            }
            else
            {
                Texture2D tempTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                tempTexture.SetData(new[] { currentBgColor });
                spriteBatch.Draw(tempTexture, _rectangle, Color.White);
                tempTexture.Dispose(); // Dispose of the temporary texture
            }

            // Draw text
            if (!string.IsNullOrEmpty(_text))
            {
                var x = (_rectangle.X + (_rectangle.Width / 2f)) - (_font.MeasureString(_text).X / 2f);
                var y = (_rectangle.Y + (_rectangle.Height / 2f)) - (_font.MeasureString(_text).Y / 2f);
                spriteBatch.DrawString(_font, _text, new Vector2(x, y), _isHovering ? HoverColour : PenColour);
            }
        }
    }
}