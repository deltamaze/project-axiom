using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using project_axiom.UI;
using System.Collections.Generic;

namespace project_axiom.GameStates
{
    public class CharacterCreationState : GameState
    {
        private SpriteFont _font;
        private SpriteFont _titleFont;
        private TextInput _nameInput;
        private List<Button> _classButtons;
        private Button _createButton;
        private Button _backButton;
        private Character _character;
        private CharacterClass _selectedClass = CharacterClass.Brawler;

        public CharacterCreationState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
            : base(game, graphicsDevice, content)
        {
            _character = new Character();
        }

        public override void LoadContent()
        {
            _font = _content.Load<SpriteFont>("Fonts/DefaultFont");
            _titleFont = _font; // Using same font for now, can be different later

            // Create name input
            _nameInput = new TextInput(_font, "Enter character name...", 20)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width / 2f) - 100, 250),
                BackgroundColor = new Color(40, 40, 60),
                BorderColor = Color.White,
                FocusedBorderColor = Color.LightBlue,
                TextColor = Color.White,
                PlaceholderColor = Color.Gray
            };

            // Create class selection buttons
            _classButtons = new List<Button>();

            var brawlerButton = new Button(_font, "Brawler")
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width / 2f) - 250, 350),
                PenColour = Color.White,
                BackgroundColour = _selectedClass == CharacterClass.Brawler ? new Color(100, 50, 50) : new Color(60, 60, 80),
                BackgroundHoverColour = new Color(120, 70, 70)
            };
            brawlerButton.Click += (s, e) => SelectClass(CharacterClass.Brawler);

            var rangerButton = new Button(_font, "Ranger")
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width / 2f) - 75, 350),
                PenColour = Color.White,
                BackgroundColour = _selectedClass == CharacterClass.Ranger ? new Color(50, 100, 50) : new Color(60, 60, 80),
                BackgroundHoverColour = new Color(70, 120, 70)
            };
            rangerButton.Click += (s, e) => SelectClass(CharacterClass.Ranger);

            var spellcasterButton = new Button(_font, "Spellcaster")
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width / 2f) + 100, 350),
                PenColour = Color.White,
                BackgroundColour = _selectedClass == CharacterClass.Spellcaster ? new Color(50, 50, 100) : new Color(60, 60, 80),
                BackgroundHoverColour = new Color(70, 70, 120)
            };
            spellcasterButton.Click += (s, e) => SelectClass(CharacterClass.Spellcaster);

            _classButtons.Add(brawlerButton);
            _classButtons.Add(rangerButton);
            _classButtons.Add(spellcasterButton);

            // Create character button
            _createButton = new Button(_font, "Create Character")
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width / 2f) - 75, 500),
                PenColour = Color.White,
                BackgroundColour = new Color(50, 80, 50),
                BackgroundHoverColour = new Color(70, 100, 70)
            };
            _createButton.Click += CreateCharacter_Click;

            // Back button
            _backButton = new Button(_font, "Back")
            {
                Position = new Vector2(50, 50),
                PenColour = Color.White,
                BackgroundColour = new Color(80, 50, 50),
                BackgroundHoverColour = new Color(100, 70, 70)
            };
            _backButton.Click += BackButton_Click;

            // Ensure button rectangles are initialized
            foreach (var button in _classButtons)
            {
                button.Position = button.Position;
            }
            _createButton.Position = _createButton.Position;
            _backButton.Position = _backButton.Position;
        }

        private void SelectClass(CharacterClass characterClass)
        {
            _selectedClass = characterClass;
            _character.Class = characterClass;
            
            // Update button colors to show selection
            UpdateClassButtonColors();
        }

        private void UpdateClassButtonColors()
        {
            for (int i = 0; i < _classButtons.Count; i++)
            {
                var button = _classButtons[i];
                CharacterClass buttonClass = (CharacterClass)i;
                
                if (buttonClass == _selectedClass)
                {
                    switch (buttonClass)
                    {
                        case CharacterClass.Brawler:
                            button.BackgroundColour = new Color(100, 50, 50);
                            break;
                        case CharacterClass.Ranger:
                            button.BackgroundColour = new Color(50, 100, 50);
                            break;
                        case CharacterClass.Spellcaster:
                            button.BackgroundColour = new Color(50, 50, 100);
                            break;
                    }
                }
                else
                {
                    button.BackgroundColour = new Color(60, 60, 80);
                }
            }
        }

        private void CreateCharacter_Click(object sender, System.EventArgs e)
        {
            // Validate character name
            string characterName = _nameInput.Text.Trim();
            if (string.IsNullOrEmpty(characterName))
            {
                System.Diagnostics.Debug.WriteLine("Character name cannot be empty!");
                return;
            }

            // Create the character with the entered name and selected class
            _character.Name = characterName;
            _character.Class = _selectedClass;

            System.Diagnostics.Debug.WriteLine($"Character created: {_character.Name} - {_character.Class}");
            System.Diagnostics.Debug.WriteLine($"Stats: Health={_character.MaxHealth}, {_character.ResourceType}={_character.MaxResource}");

            // Transition to Training Grounds with the created character
            _game.ChangeState(new TrainingGroundsState(_game, _graphicsDevice, _content, _character));
        }

        private void BackButton_Click(object sender, System.EventArgs e)
        {
            _game.ChangeState(new MainMenuState(_game, _graphicsDevice, _content));
        }

        public override void Update(GameTime gameTime)
        {
            _nameInput.Update(gameTime);
            
            foreach (var button in _classButtons)
                button.Update(gameTime);
                
            _createButton.Update(gameTime);
            _backButton.Update(gameTime);

            // Handle escape key to go back
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                _game.ChangeState(new MainMenuState(_game, _graphicsDevice, _content));
            }
        }

        public override void PostUpdate(GameTime gameTime)
        {
            // No post-update logic needed for character creation
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _graphicsDevice.Clear(new Color(25, 25, 45)); // Slightly different background

            spriteBatch.Begin();

            // Draw title
            string title = "Create Your Character";
            var titleSize = _titleFont.MeasureString(title);
            var titlePosition = new Vector2((_graphicsDevice.Viewport.Width / 2f) - (titleSize.X / 2f), 100);
            spriteBatch.DrawString(_titleFont, title, titlePosition, Color.White);

            // Draw name input label
            string nameLabel = "Character Name:";
            var nameLabelPosition = new Vector2((_graphicsDevice.Viewport.Width / 2f) - 100, 220);
            spriteBatch.DrawString(_font, nameLabel, nameLabelPosition, Color.White);

            // Draw name input
            _nameInput.Draw(spriteBatch);

            // Draw class selection label
            string classLabel = "Choose Your Class:";
            var classLabelSize = _font.MeasureString(classLabel);
            var classLabelPosition = new Vector2((_graphicsDevice.Viewport.Width / 2f) - (classLabelSize.X / 2f), 320);
            spriteBatch.DrawString(_font, classLabel, classLabelPosition, Color.White);

            // Draw class buttons
            foreach (var button in _classButtons)
                button.Draw(spriteBatch);

            // Draw class description
            string description = GetSelectedClassDescription();
            var descSize = _font.MeasureString(description);
            var descPosition = new Vector2((_graphicsDevice.Viewport.Width / 2f) - (descSize.X / 2f), 420);
            spriteBatch.DrawString(_font, description, descPosition, Color.LightGray);

            // Draw create and back buttons
            _createButton.Draw(spriteBatch);
            _backButton.Draw(spriteBatch);

            // Draw instructions
            string instructions = "ESC to go back";
            spriteBatch.DrawString(_font, instructions, new Vector2(10, _graphicsDevice.Viewport.Height - 30), Color.Gray);

            spriteBatch.End();
        }

        private string GetSelectedClassDescription()
        {
            var tempCharacter = new Character("", _selectedClass);
            return tempCharacter.GetClassDescription();
        }
    }
}