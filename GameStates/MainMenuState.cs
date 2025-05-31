using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using project_axiom.UI;
using System.Collections.Generic;

namespace project_axiom.GameStates
{
    public class MainMenuState : GameState
    {
        private List<Button> _buttons;
        private SpriteFont _buttonFont;
        // private Texture2D _buttonTexture; // Optional: if you create a button background image

        public MainMenuState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
          : base(game, graphicsDevice, content)
        {
        }

        public override void LoadContent()
        {
            _buttonFont = _content.Load<SpriteFont>("Fonts/DefaultFont");
            // _buttonTexture = _content.Load<Texture2D>("Path/To/Your/ButtonTexture"); // If you have one

            var loadGameButton = new Button(_buttonFont, "Load Game" /*, _buttonTexture (optional) */)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width / 2f) - 100, 200),
                PenColour = Color.AntiqueWhite, // Example text color
                BackgroundColour = new Color(50,50,100), // Example button bg color
                BackgroundHoverColour = new Color(80,80,150) // Example button bg hover color
            };
            loadGameButton.Click += LoadGameButton_Click;

            var quitButton = new Button(_buttonFont, "Quit" /*, _buttonTexture (optional) */)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width / 2f) - 100, 280),
                PenColour = Color.AntiqueWhite,
                BackgroundColour = new Color(100,50,50),
                BackgroundHoverColour = new Color(150,80,80)
            };
            quitButton.Click += QuitButton_Click;

            _buttons = new List<Button>()
            {
                loadGameButton,
                quitButton,
            };

             // Ensure button rectangles are initialized after setting position
            foreach(var button in _buttons)
            {
                 button.Position = button.Position; // This calls the setter which updates the rectangle
            }
        }

        private void LoadGameButton_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Load Game Clicked!"); // Use System.Diagnostics.Debug for output
            // TODO: Transition to Load Game screen or Character Creation (as per README 6.5)
            // As a placeholder, for now, this doesn't change state.
            // Example: _game.ChangeState(new CharacterCreationState(_game, _graphicsDevice, _content));
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
            foreach (var button in _buttons)
                button.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}