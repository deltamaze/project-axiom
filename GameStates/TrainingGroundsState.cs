using project_axiom.UI;
using project_axiom.Spells;

namespace project_axiom.GameStates;

public class TrainingGroundsState : GameState
{
    // Rendering components
    private BasicEffect _basicEffect;
    private CubeRenderer _cubeRenderer;
    private EnvironmentRenderer _environmentRenderer;
    private TrainingDummyRenderer _trainingDummyRenderer;

    // Input and camera components
    private PlayerController _playerController;
    private CameraController _cameraController;

    // Character information
    private Character _character;

    // Training dummies
    private List<TrainingDummy> _trainingDummies;

    // UI font
    private SpriteFont _font;

    // Persistent 1x1 white texture for UI rectangles
    private Texture2D _whiteTexture;

    // Constants
    private const float PLAYER_GROUND_OFFSET = 0.51f;

    // Targeting system
    private TrainingDummy _targetedDummy;
    private MouseState _previousMouseState;

    // Add player health fields
    private float _playerMaxHealth = 100f;
    private float _playerCurrentHealth = 100f;

    // UI components for spell system
    private SpellBarState _spellBarState;
    private MessageDisplay _messageDisplay;

    public TrainingGroundsState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content, Character character)
        : base(game, graphicsDevice, content)
    {
        _character = character ?? new Character("Default", CharacterClass.Brawler);
        
        // Initialize UI systems
        _spellBarState = new SpellBarState();
        _messageDisplay = new MessageDisplay();
    }

    public TrainingGroundsState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
        : this(game, graphicsDevice, content, new Character("Default", CharacterClass.Brawler))
    {
    }

    public override void LoadContent()
    {
        // Load font for UI
        _font = _content.Load<SpriteFont>("Fonts/DefaultFont");

        // Initialize rendering components
        InitializeRendering();

        // Initialize input and camera components
        InitializeInputAndCamera();

        // Initialize training dummies
        InitializeTrainingDummies();

        // Create persistent 1x1 white texture for UI
        _whiteTexture = new Texture2D(_graphicsDevice, 1, 1);
        _whiteTexture.SetData(new[] { Color.White });

        System.Diagnostics.Debug.WriteLine($"Character {_character.Name} ({_character.Class}) entered Training Grounds");
        System.Diagnostics.Debug.WriteLine($"Training area: {GeometryBuilder.GROUND_SIZE}x{GeometryBuilder.GROUND_SIZE} units, Wall height: {GeometryBuilder.WALL_HEIGHT} units");
        System.Diagnostics.Debug.WriteLine($"Training dummies placed: {_trainingDummies.Count}");
    }

    /// <summary>
    /// Initialize all rendering components and effects
    /// </summary>
    private void InitializeRendering()
    {
        // Initialize BasicEffect for 3D rendering
        _basicEffect = new BasicEffect(_graphicsDevice);
        _basicEffect.VertexColorEnabled = true;
        _basicEffect.LightingEnabled = false;

        // Initialize renderers
        _cubeRenderer = new CubeRenderer(_graphicsDevice, _character.Class);
        _environmentRenderer = new EnvironmentRenderer(_graphicsDevice);
        _trainingDummyRenderer = new TrainingDummyRenderer(_graphicsDevice, _font);
    }

    /// <summary>
    /// Initialize input handling and camera systems
    /// </summary>
    private void InitializeInputAndCamera()
    {
        // Initialize camera controller
        _cameraController = new CameraController(_graphicsDevice);

        // Initialize player controller with starting position
        Vector3 startPosition = new Vector3(0, GeometryBuilder.GROUND_Y + PLAYER_GROUND_OFFSET, 0);
        _playerController = new PlayerController(_graphicsDevice, _character, startPosition);
        
        // Connect player controller to UI systems
        _playerController.MessageDisplay = _messageDisplay;
        _playerController.OnSpellCast += OnPlayerSpellCast;
    }

    /// <summary>
    /// Handle spell cast events from player
    /// </summary>
    private void OnPlayerSpellCast(int slotIndex, SpellData spell)
    {
        // Flash the spell bar slot
        _spellBarState.FlashSlot(slotIndex);
        
        System.Diagnostics.Debug.WriteLine($"Spell cast visual feedback for slot {slotIndex + 1}: {spell.Name}");
    }

    /// <summary>
    /// Initialize training dummies at predefined positions
    /// </summary>
    private void InitializeTrainingDummies()
    {
        _trainingDummies = new List<TrainingDummy>();

        // Get predefined positions from GeometryBuilder
        Vector3[] dummyPositions = GeometryBuilder.GetTrainingDummyPositions();

        // Create training dummies at each position
        for (int i = 0; i < dummyPositions.Length; i++)
        {
            var dummy = new TrainingDummy(dummyPositions[i], $"Dummy {i + 1}");
            _trainingDummies.Add(dummy);
        }

        System.Diagnostics.Debug.WriteLine($"Initialized {_trainingDummies.Count} training dummies");
        foreach (var dummy in _trainingDummies)
        {
            System.Diagnostics.Debug.WriteLine($"  {dummy.Name} at position {dummy.Position}");
        }
    }

    public override void Update(GameTime gameTime)
    {
        // Check for escape key to return to main menu
        if (_playerController.IsEscapePressed())
        {
            _game.ChangeState(new MainMenuState(_game, _graphicsDevice, _content));
            return;
        }

        // Update player input and movement
        _playerController.Update(gameTime);

        // Update camera based on player position and rotation
        _cameraController.Update(
            _playerController.Position,
            _playerController.RotationX,
            _playerController.RotationY);

        // Update BasicEffect matrices
        _basicEffect.View = _cameraController.View;
        _basicEffect.Projection = _cameraController.Projection;

        // Update character resource regeneration
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _character.RegenerateResource(deltaTime);

        // Update UI systems
        _spellBarState.Update(deltaTime);
        _messageDisplay.Update(deltaTime);

        // Update spell bar visual states based on cooldowns
        UpdateSpellBarVisuals();

        // Set current target for player controller
        _playerController.CurrentTarget = _targetedDummy;

        // Update training dummies (placeholder for future logic)
        UpdateTrainingDummies(gameTime);

        // Mouse picking for click-to-target
        MouseState mouseState = Mouse.GetState();
        if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
        {
            _targetedDummy = GetDummyUnderMouse(mouseState.Position);
        }
        _previousMouseState = mouseState;
    }

    /// <summary>
    /// Update training dummy logic (placeholder for future expansion)
    /// </summary>
    private void UpdateTrainingDummies(GameTime gameTime)
    {
        // For now, training dummies are static
        // Future: Add respawn logic, animations, or other behaviors

        // Example: Auto-reset dummies after being "defeated" for a certain time
        foreach (var dummy in _trainingDummies)
        {
            if (!dummy.IsAlive)
            {
                // Could add a respawn timer here in the future
                // For now, keep them "alive" for testing
                dummy.Reset();
            }
        }
    }

    /// <summary>
    /// Update spell bar visual states based on spell cooldowns
    /// </summary>
    private void UpdateSpellBarVisuals()
    {
        var spellSystem = _playerController.GetSpellCastingSystem();
        
        for (int i = 0; i < 8; i++)
        {
            if (spellSystem.IsSpellOnCooldown(i))
            {
                _spellBarState.SetSlotOnCooldown(i);
            }
            else if (_spellBarState.GetSlotState(i) == SlotState.OnCooldown)
            {
                _spellBarState.SetSlotReady(i);
            }
        }
    }

    public override void PostUpdate(GameTime gameTime)
    {
        // No state transitions needed in PostUpdate for now
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        // Clear screen with sky blue background
        _graphicsDevice.Clear(new Color(135, 206, 235));

        // Set 3D rendering states
        _graphicsDevice.BlendState = BlendState.Opaque;
        _graphicsDevice.DepthStencilState = DepthStencilState.Default;
        _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

        // Draw 3D environment
        _environmentRenderer.DrawEnvironment(_basicEffect);

        // Draw training dummies (pass targeted dummy)
        _trainingDummyRenderer.DrawDummies(_basicEffect, _trainingDummies, _targetedDummy);

        // Draw player cube
        _cubeRenderer.Draw(_basicEffect, _playerController.Position);

        // Draw UI (show targeted dummy info)
        DrawUI(spriteBatch);

        // Reset render states
        _graphicsDevice.BlendState = BlendState.Opaque;
        _graphicsDevice.DepthStencilState = DepthStencilState.Default;
    }    /// <summary>
    /// Draw the user interface elements
    /// </summary>
    private void DrawUI(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin();

        // Draw player health and resource bars at top left
        DrawPlayerHealthAndResourceBars(spriteBatch);

        // Show targeted dummy info (centered at top)
        if (_targetedDummy != null && _targetedDummy.IsAlive)
        {
            string info = $"Target: {_targetedDummy.Name}  HP: {(int)_targetedDummy.CurrentHealth} / {(int)_targetedDummy.MaxHealth}";
            Vector2 size = _font.MeasureString(info);
            spriteBatch.DrawString(_font, info, new Vector2((_graphicsDevice.Viewport.Width - size.X) / 2, 10), Color.Yellow);
        }

        // Draw spell bar UI with visual feedback
        DrawSpellBar(spriteBatch);

        // Draw temporary messages (like "Out of Range")
        DrawMessages(spriteBatch);

        spriteBatch.End();

        // Draw training dummy health bars (3D to 2D projection)
        spriteBatch.Begin();
        _trainingDummyRenderer.DrawDummyHealthBars(spriteBatch, _trainingDummies, _cameraController.View, _cameraController.Projection, _targetedDummy);
        spriteBatch.End();
    }

    // Draws the player health and resource bars at the top left
    private void DrawPlayerHealthAndResourceBars(SpriteBatch spriteBatch)
    {
        // Bar settings
        int baseBarWidth = 280;
        int healthBarWidth = (int)(baseBarWidth * 1.2f); // 20% wider
        int spellBarWidth = baseBarWidth; // Spell bar matches old health bar width
        int healthBarHeight = 36;
        int resourceBarHeight = 28;
        int x = 24;
        int y = 24;
        int spacing = 4;
        float healthPercent = _playerCurrentHealth / _playerMaxHealth;
        Color healthColor;
        Color borderColor = Color.White;
        Color textColor = Color.White;
        // Class color for health bar
        switch (_character.Class)
        {
            case CharacterClass.Brawler:
                healthColor = new Color(255, 140, 0); // Orange
                break;
            case CharacterClass.Ranger:
                healthColor = new Color(0, 200, 80); // Green
                break;
            case CharacterClass.Spellcaster:
                healthColor = new Color(0, 200, 220); // Teal/Light Blue
                break;
            default:
                healthColor = Color.Gray;
                break;
        }
        // Draw health bar background
        spriteBatch.Draw(_whiteTexture, new Rectangle(x, y, healthBarWidth, healthBarHeight), Color.Black);
        // Draw health fill
        int fillWidth = (int)(healthBarWidth * healthPercent);
        if (fillWidth > 0)
            spriteBatch.Draw(_whiteTexture, new Rectangle(x, y, fillWidth, healthBarHeight), healthColor);
        // Draw border
        int border = 2;
        spriteBatch.Draw(_whiteTexture, new Rectangle(x, y, healthBarWidth, border), borderColor); // Top
        spriteBatch.Draw(_whiteTexture, new Rectangle(x, y + healthBarHeight - border, healthBarWidth, border), borderColor); // Bottom
        spriteBatch.Draw(_whiteTexture, new Rectangle(x, y, border, healthBarHeight), borderColor); // Left
        spriteBatch.Draw(_whiteTexture, new Rectangle(x + healthBarWidth - border, y, border, healthBarHeight), borderColor); // Right
        // Draw player name (left)
        string name = _character.Name;
        Vector2 nameSize = _font.MeasureString(name);
        spriteBatch.DrawString(_font, name, new Vector2(x + 8, y + (healthBarHeight - nameSize.Y) / 2), textColor);
        // Draw health value (right)
        string healthLabel = $"{(int)_playerCurrentHealth} / {(int)_playerMaxHealth}";
        Vector2 healthSize = _font.MeasureString(healthLabel);
        spriteBatch.DrawString(_font, healthLabel, new Vector2(x + healthBarWidth - healthSize.X - 8, y + (healthBarHeight - healthSize.Y) / 2), textColor);

        // Resource bar directly under health bar
        float resourcePercent = _character.GetResourcePercentage();
        int resourceY = y + healthBarHeight + spacing;
        Color resourceColor;
        string classLabel;
        // Resource bar color by resource type
        switch (_character.ResourceType)
        {
            case ResourceTypeEnum.Frenzy:
                resourceColor = Color.Maroon;
                classLabel = "Brawler";
                break;
            case ResourceTypeEnum.Energy:
                resourceColor = Color.YellowGreen;
                classLabel = "Ranger";
                break;
            case ResourceTypeEnum.Mana:
                resourceColor = Color.DarkBlue;
                classLabel = "Spellcaster";
                break;
            default:
                resourceColor = Color.Gray;
                classLabel = _character.Class.ToString();
                break;
        }
        // Draw resource bar background
        spriteBatch.Draw(_whiteTexture, new Rectangle(x, resourceY, healthBarWidth, resourceBarHeight), Color.Black);
        // Draw resource fill
        int resourceFillWidth = (int)(healthBarWidth * resourcePercent);
        if (resourceFillWidth > 0)
            spriteBatch.Draw(_whiteTexture, new Rectangle(x, resourceY, resourceFillWidth, resourceBarHeight), resourceColor);
        // Draw border
        spriteBatch.Draw(_whiteTexture, new Rectangle(x, resourceY, healthBarWidth, border), borderColor); // Top
        spriteBatch.Draw(_whiteTexture, new Rectangle(x, resourceY + resourceBarHeight - border, healthBarWidth, border), borderColor); // Bottom
        spriteBatch.Draw(_whiteTexture, new Rectangle(x, resourceY, border, resourceBarHeight), borderColor); // Left
        spriteBatch.Draw(_whiteTexture, new Rectangle(x + healthBarWidth - border, resourceY, border, resourceBarHeight), borderColor); // Right
        // Draw class label (left)
        Vector2 classSize = _font.MeasureString(classLabel);
        spriteBatch.DrawString(_font, classLabel, new Vector2(x + 8, resourceY + (resourceBarHeight - classSize.Y) / 2), textColor);
        // Draw resource value (right)
        string resourceLabel = $"{(int)_character.CurrentResource} / {_character.MaxResource}";
        Vector2 resourceValueSize = _font.MeasureString(resourceLabel);
        spriteBatch.DrawString(_font, resourceLabel, new Vector2(x + healthBarWidth - resourceValueSize.X - 8, resourceY + (resourceBarHeight - resourceValueSize.Y) / 2), textColor);
    }    /// <summary>
    /// Draw the spell bar UI with visual feedback for cooldowns and flashing
    /// </summary>
    private void DrawSpellBar(SpriteBatch spriteBatch)
    {
        int slotCount = 8;
        int slotWidth = 48;
        int slotHeight = 48;
        int slotSpacing = 12;
        int totalWidth = slotCount * slotWidth + (slotCount - 1) * slotSpacing;
        int startX = (_graphicsDevice.Viewport.Width - totalWidth) / 2;
        int y = _graphicsDevice.Viewport.Height - slotHeight - 32;
        
        var spellSystem = _playerController.GetSpellCastingSystem();
        
        for (int i = 0; i < slotCount; i++)
        {
            int x = startX + i * (slotWidth + slotSpacing);
            Rectangle rect = new Rectangle(x, y, slotWidth, slotHeight);
            
            // Determine slot color based on state
            Color slotColor = Color.DarkSlateGray;
            Color borderColor = Color.White;
            
            SlotState state = _spellBarState.GetSlotState(i);
            switch (state)
            {
                case SlotState.Flashing:
                    // Flash between normal and bright colors
                    float flashIntensity = _spellBarState.GetFlashIntensity(i);
                    slotColor = Color.Lerp(Color.DarkSlateGray, Color.Gold, flashIntensity);
                    borderColor = Color.Lerp(Color.White, Color.Yellow, flashIntensity);
                    break;
                    
                case SlotState.OnCooldown:
                    slotColor = Color.DarkGray;
                    borderColor = Color.Gray;
                    break;
                    
                case SlotState.Ready:
                default:
                    // Check if we have a spell in this slot and show different color
                    var spell = spellSystem.GetEquippedSpell(i);
                    if (spell != null)
                    {
                        slotColor = Color.DarkSlateBlue; // Indicate spell is equipped
                    }
                    break;
            }
            
            // Draw slot background
            spriteBatch.Draw(_whiteTexture, rect, slotColor);
            
            // Draw cooldown overlay if applicable
            if (state == SlotState.OnCooldown)
            {
                float cooldownProgress = spellSystem.GetSpellCooldownProgress(i);
                if (cooldownProgress > 0)
                {
                    int cooldownHeight = (int)(slotHeight * cooldownProgress);
                    Rectangle cooldownRect = new Rectangle(x, y + slotHeight - cooldownHeight, slotWidth, cooldownHeight);
                    spriteBatch.Draw(_whiteTexture, cooldownRect, Color.Black * 0.7f);
                }
            }
            
            // Draw border
            int border = 2;
            spriteBatch.Draw(_whiteTexture, new Rectangle(x, y, slotWidth, border), borderColor); // Top
            spriteBatch.Draw(_whiteTexture, new Rectangle(x, y + slotHeight - border, slotWidth, border), borderColor); // Bottom
            spriteBatch.Draw(_whiteTexture, new Rectangle(x, y, border, slotHeight), borderColor); // Left
            spriteBatch.Draw(_whiteTexture, new Rectangle(x + slotWidth - border, y, border, slotHeight), borderColor); // Right
            
            // Draw slot number
            string num = (i + 1).ToString();
            Vector2 numSize = _font.MeasureString(num);
            Vector2 numPos = new Vector2(x + (slotWidth - numSize.X) / 2, y + slotHeight - numSize.Y - 4);
            spriteBatch.DrawString(_font, num, numPos, Color.LightGray);
            
            // Draw spell name if equipped
            var equippedSpell = spellSystem.GetEquippedSpell(i);
            if (equippedSpell != null)
            {
                Vector2 spellNameSize = _font.MeasureString(equippedSpell.Name);
                if (spellNameSize.X <= slotWidth - 4) // Only draw if it fits
                {
                    Vector2 spellNamePos = new Vector2(x + (slotWidth - spellNameSize.X) / 2, y + 4);
                    spriteBatch.DrawString(_font, equippedSpell.Name, spellNamePos, Color.White);
                }
            }
        }
    }

    /// <summary>
    /// Get the display color for the character class
    /// </summary>
    private Color GetClassColor()
    {
        switch (_character.Class)
        {
            case CharacterClass.Brawler:
                return Color.Red;
            case CharacterClass.Ranger:
                return Color.Green;
            case CharacterClass.Spellcaster:
                return Color.Blue;
            default:
                return Color.Gray;
        }
    }

    /// <summary>
    /// Get training dummies (useful for future combat system)
    /// </summary>
    public List<TrainingDummy> GetTrainingDummies()
    {
        return _trainingDummies;
    }

    /// <summary>
    /// Dispose of resources when state is destroyed
    /// </summary>
    public void Dispose()
    {
        _cubeRenderer?.Dispose();
        _environmentRenderer?.Dispose();
        _trainingDummyRenderer?.Dispose();
        _basicEffect?.Dispose();
        _whiteTexture?.Dispose();
    }

    // Helper to get display name for resource type
    private string GetResourceTypeDisplayName(ResourceTypeEnum type)
    {
        switch (type)
        {
            case ResourceTypeEnum.Mana:
                return "Mana";
            case ResourceTypeEnum.Energy:
                return "Energy";
            case ResourceTypeEnum.Frenzy:
                return "Frenzy";
            default:
                return type.ToString();
        }
    }

    private TrainingDummy GetDummyUnderMouse(Point mousePosition)
    {
        // Convert mouse to world ray
        Vector3 nearPoint = _graphicsDevice.Viewport.Unproject(new Vector3(mousePosition.X, mousePosition.Y, 0), _cameraController.Projection, _cameraController.View, Matrix.Identity);
        Vector3 farPoint = _graphicsDevice.Viewport.Unproject(new Vector3(mousePosition.X, mousePosition.Y, 1), _cameraController.Projection, _cameraController.View, Matrix.Identity);
        Vector3 direction = Vector3.Normalize(farPoint - nearPoint);
        float closestDist = float.MaxValue;
        TrainingDummy closestDummy = null;
        foreach (var dummy in _trainingDummies)
        {
            if (!dummy.IsAlive) continue;
            BoundingBox box = new BoundingBox(dummy.Position - new Vector3(dummy.Scale/2), dummy.Position + new Vector3(dummy.Scale/2));
            float? dist = box.Intersects(new Ray(nearPoint, direction));
            if (dist.HasValue && dist.Value < closestDist)
            {
                closestDist = dist.Value;
                closestDummy = dummy;
            }
        }
        return closestDummy;
    }    /// <summary>
    /// Draw temporary messages like "Out of Range"
    /// </summary>
    private void DrawMessages(SpriteBatch spriteBatch)
    {
        var messages = _messageDisplay.GetMessages();
        
        foreach (var message in messages)
        {
            Vector2 messageSize = _font.MeasureString(message.Text);
            Vector2 position;
            
            // Calculate position based on message position type
            switch (message.Position)
            {
                case MessagePosition.Center:
                    position = new Vector2(
                        (_graphicsDevice.Viewport.Width - messageSize.X) / 2,
                        (_graphicsDevice.Viewport.Height - messageSize.Y) / 2
                    );
                    break;
                    
                case MessagePosition.Top:
                    position = new Vector2(
                        (_graphicsDevice.Viewport.Width - messageSize.X) / 2,
                        50
                    );
                    break;
                    
                case MessagePosition.Bottom:
                    position = new Vector2(
                        (_graphicsDevice.Viewport.Width - messageSize.X) / 2,
                        _graphicsDevice.Viewport.Height - messageSize.Y - 100
                    );
                    break;

                case MessagePosition.PlayerRight:
                    // Get player position in screen coordinates
                    Vector3 playerScreenPos = _graphicsDevice.Viewport.Project(
                        _playerController.Position, 
                        _cameraController.Projection, 
                        _cameraController.View, 
                        Matrix.Identity);

                    // Animation parameters
                    float animProgress = message.GetAnimationProgress();
                    float screenHeight = _graphicsDevice.Viewport.Height;
                    
                    // Base position: to the right of player, in bottom half of screen
                    float baseX = playerScreenPos.X + 100; // 100 pixels to the right of player
                    float baseY = Math.Max(playerScreenPos.Y, screenHeight * 0.5f); // Ensure bottom half
                    
                    // Create scrolling animation: up and out, then up and in
                    float animX, animY;
                    
                    if (animProgress < 0.5f)
                    {
                        // First half: scroll up and out (to the right)
                        float t = animProgress * 2.0f; // 0 to 1 over first half
                        animX = baseX + (t * 50); // Move 50 pixels to the right
                        animY = baseY - (t * 40); // Move 40 pixels up
                    }
                    else
                    {
                        // Second half: continue up and in (back toward player)
                        float t = (animProgress - 0.5f) * 2.0f; // 0 to 1 over second half
                        animX = (baseX + 50) - (t * 25); // Move 25 pixels back toward player
                        animY = (baseY - 40) - (t * 30); // Continue moving up 30 more pixels
                    }
                    
                    position = new Vector2(animX, animY);
                    break;
                    
                default:
                    position = new Vector2(
                        (_graphicsDevice.Viewport.Width - messageSize.X) / 2,
                        (_graphicsDevice.Viewport.Height - messageSize.Y) / 2
                    );
                    break;
            }
            
            // Apply fade effect
            Color messageColor = message.Color * message.GetAlpha();
            
            spriteBatch.DrawString(_font, message.Text, position, messageColor);
        }
    }
}