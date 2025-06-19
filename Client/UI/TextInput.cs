namespace project_axiom.UI;

  public class TextInput
  {
      private SpriteFont _font;
      private string _text;
      private string _placeholder;
      private Vector2 _position;
      private Rectangle _rectangle;
      private bool _isActive;
      private bool _isFocused;
      private KeyboardState _previousKeyboard;
      private KeyboardState _currentKeyboard;
      private int _maxLength;
      private bool _isPassword;

      public string Text 
      { 
          get { return _text; } 
          set { SetText(value); }
      }
      public bool IsActive { get { return _isActive; } set { _isActive = value; } }
      public bool IsPassword { get { return _isPassword; } set { _isPassword = value; } }
      public string PlaceholderText 
      { 
          get { return _placeholder; } 
          set { _placeholder = value; }
      }
      
      // Add Width and Height properties
      public int Width { get; set; } = 400;
      public int Height { get; set; } = 40;
      
      public Color TextColor { get; set; } = Color.White;
      public Color PlaceholderColor { get; set; } = Color.Gray;
      public Color BackgroundColor { get; set; } = Color.DarkSlateGray;
      public Color BorderColor { get; set; } = Color.White;
      public Color FocusedBorderColor { get; set; } = Color.LightBlue;

      public Vector2 Position
      {
          get { return _position; }
          set
          {
              _position = value;
              UpdateRectangle();
          }
      }

      public TextInput(SpriteFont font, string placeholder = "", int maxLength = 50)
      {
          _font = font;
          _placeholder = placeholder;
          _text = "";
          _maxLength = maxLength;
          _isActive = true;
          _isPassword = false;
          UpdateRectangle();
      }

      public void UpdateRectangle()
      {
          _rectangle = new Rectangle((int)_position.X, (int)_position.Y, Width, Height);
      }

      public void Update(GameTime gameTime)
      {
          if (!_isActive) return;

          _previousKeyboard = _currentKeyboard;
          _currentKeyboard = Keyboard.GetState();

          // Check if clicked to focus/unfocus
          var mouseState = Mouse.GetState();
          var mouseRectangle = new Rectangle(mouseState.X, mouseState.Y, 1, 1);
          
          if (mouseState.LeftButton == ButtonState.Pressed && !Mouse.GetState().Equals(_previousKeyboard))
          {
              _isFocused = mouseRectangle.Intersects(_rectangle);
          }

          if (_isFocused)
          {
              HandleTextInput();
          }
      }

      private void HandleTextInput()
      {
          var pressedKeys = _currentKeyboard.GetPressedKeys()
              .Where(key => !_previousKeyboard.IsKeyDown(key))
              .ToArray();

          foreach (var key in pressedKeys)
          {              if (key == Keys.Back && _text.Length > 0)
              {
                  _text = _text.Substring(0, _text.Length - 1);
                  // Don't call UpdateRectangle() here - size should stay constant
              }
              else if (key == Keys.Space && _text.Length < _maxLength)
              {
                  _text += " ";
                  // Don't call UpdateRectangle() here - size should stay constant
              }
              else if (_text.Length < _maxLength)
              {
                  string keyString = GetKeyString(key);
                  if (!string.IsNullOrEmpty(keyString))
                  {
                      _text += keyString;
                      // Don't call UpdateRectangle() here - size should stay constant
                  }
              }
          }
      }

      private string GetKeyString(Keys key)
      {
          bool isShiftPressed = _currentKeyboard.IsKeyDown(Keys.LeftShift) || _currentKeyboard.IsKeyDown(Keys.RightShift);
          
          // Handle letters
          if (key >= Keys.A && key <= Keys.Z)
          {
              char letter = (char)('a' + (key - Keys.A));
              return isShiftPressed ? letter.ToString().ToUpper() : letter.ToString();
          }
          
          // Handle numbers
          if (key >= Keys.D0 && key <= Keys.D9)
          {
              char[] shiftedNumbers = { ')', '!', '@', '#', '$', '%', '^', '&', '*', '(' };
              if (isShiftPressed)
              {
                  return shiftedNumbers[key - Keys.D0].ToString();
              }
              return ((int)(key - Keys.D0)).ToString();
          }
          
          // Handle numpad numbers
          if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
          {
              return ((int)(key - Keys.NumPad0)).ToString();
          }

          // Handle common symbols for email/password
          switch (key)
          {
              case Keys.OemPeriod:
                  return isShiftPressed ? ">" : ".";
              case Keys.OemComma:
                  return isShiftPressed ? "<" : ",";
              case Keys.OemMinus:
                  return isShiftPressed ? "_" : "-";
              case Keys.OemPlus:
                  return isShiftPressed ? "+" : "=";
              case Keys.OemQuestion:
                  return isShiftPressed ? "?" : "/";
              case Keys.OemSemicolon:
                  return isShiftPressed ? ":" : ";";
              case Keys.OemQuotes:
                  return isShiftPressed ? "\"" : "'";              case Keys.OemOpenBrackets:
                  return isShiftPressed ? "{" : "[";
              case Keys.OemCloseBrackets:
                  return isShiftPressed ? "}" : "]";
              case Keys.OemPipe:
                  return isShiftPressed ? "|" : "\\";
              case Keys.OemTilde:
                  return isShiftPressed ? "~" : "`";
          }

          return null;
      }

      public void Draw(SpriteBatch spriteBatch)
      {
          if (!_isActive) return;

          // Create temporary texture for background
          Texture2D tempTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
          tempTexture.SetData(new[] { BackgroundColor });
          
          // Draw background
          spriteBatch.Draw(tempTexture, _rectangle, Color.White);
          
          // Draw border
          Color borderColor = _isFocused ? FocusedBorderColor : BorderColor;
          
          // Draw border lines (top, bottom, left, right)
          spriteBatch.Draw(tempTexture, new Rectangle(_rectangle.X, _rectangle.Y, _rectangle.Width, 2), borderColor);
          spriteBatch.Draw(tempTexture, new Rectangle(_rectangle.X, _rectangle.Bottom - 2, _rectangle.Width, 2), borderColor);
          spriteBatch.Draw(tempTexture, new Rectangle(_rectangle.X, _rectangle.Y, 2, _rectangle.Height), borderColor);
          spriteBatch.Draw(tempTexture, new Rectangle(_rectangle.Right - 2, _rectangle.Y, 2, _rectangle.Height), borderColor);
          
          tempTexture.Dispose();

          // Draw text or placeholder
          string displayText;
          if (string.IsNullOrEmpty(_text))
          {
              displayText = _placeholder;
          }
          else if (_isPassword)
          {
              displayText = new string('*', _text.Length);
          }
          else
          {
              displayText = _text;
          }
          
          Color textColor = string.IsNullOrEmpty(_text) ? PlaceholderColor : TextColor;
          
          if (!string.IsNullOrEmpty(displayText))
          {
              var textPosition = new Vector2(
                  _rectangle.X + 10,
                  _rectangle.Y + (_rectangle.Height / 2f) - (_font.MeasureString(displayText).Y / 2f)
              );
              spriteBatch.DrawString(_font, displayText, textPosition, textColor);
          }

          // Draw cursor if focused
          if (_isFocused && _text.Length < _maxLength)
          {
              var cursorX = _rectangle.X + 10 + (string.IsNullOrEmpty(_text) ? 0 : _font.MeasureString(displayText).X);
              var cursorY = _rectangle.Y + 5;
              var cursorHeight = _rectangle.Height - 10;
              
              tempTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
              tempTexture.SetData(new[] { TextColor });
              spriteBatch.Draw(tempTexture, new Rectangle((int)cursorX, (int)cursorY, 2, cursorHeight), Color.White);
              tempTexture.Dispose();
          }
      }      public void SetText(string text)
      {
          _text = text ?? "";
          if (_text.Length > _maxLength)
              _text = _text.Substring(0, _maxLength);
          // Don't call UpdateRectangle() here - size should stay constant
      }      public void Clear()
      {
          _text = "";
          // Don't call UpdateRectangle() here - size should stay constant
      }
  }