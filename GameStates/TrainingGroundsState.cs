using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace project_axiom.GameStates
{
    public class TrainingGroundsState : GameState
    {
        private BasicEffect _basicEffect;
        
        // Player cube rendering
        private VertexPositionColor[] _cubeVertices;
        private short[] _cubeIndices;
        private VertexBuffer _cubeVertexBuffer;
        private IndexBuffer _cubeIndexBuffer;

        // Ground plane rendering
        private VertexPositionColor[] _groundVertices;
        private short[] _groundIndices;
        private VertexBuffer _groundVertexBuffer;
        private IndexBuffer _groundIndexBuffer;

        // Boundary walls rendering
        private VertexPositionColor[] _wallVertices;
        private short[] _wallIndices;
        private VertexBuffer _wallVertexBuffer;
        private IndexBuffer _wallIndexBuffer;

        // Character information
        private Character _character;

        // Environment constants
        private const float GROUND_SIZE = 50f; // 50x50 unit ground plane
        private const float WALL_HEIGHT = 5f;
        private const float WALL_THICKNESS = 1f;
        private const float GROUND_Y = 0f; // Ground level

        // Player movement properties
        private Vector3 _playerPosition = new Vector3(0, GROUND_Y + 0.5f, 0); // Start on ground
        private float _playerSpeed = 5.0f;
        private float _playerRotationY = 0f;
        private float _playerRotationX = 0f;
        private float _mouseSensitivity = 0.003f;

        // Camera properties
        private Vector3 _cameraPosition;
        private Vector3 _cameraTarget;
        private Vector3 _cameraUp = Vector3.Up;
        private Matrix _world = Matrix.Identity;
        private Matrix _view;
        private Matrix _projection;

        // Input handling
        private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;
        private bool _isMouseCaptured = true;

        public TrainingGroundsState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content, Character character)
            : base(game, graphicsDevice, content)
        {
            _character = character ?? new Character("Default", CharacterClass.Brawler);
        }

        public TrainingGroundsState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
            : this(game, graphicsDevice, content, new Character("Default", CharacterClass.Brawler))
        {
        }

        public override void LoadContent()
        {
            // Initialize BasicEffect for 3D rendering
            _basicEffect = new BasicEffect(_graphicsDevice);
            _basicEffect.VertexColorEnabled = true;
            _basicEffect.LightingEnabled = false;

            // Set up projection matrix
            _projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45f),
                _graphicsDevice.Viewport.AspectRatio,
                0.1f,
                100f);

            // Create all geometry
            CreatePlayerCube();
            CreateGroundPlane();
            CreateBoundaryWalls();

            // Create vertex and index buffers for player cube
            _cubeVertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionColor), _cubeVertices.Length, BufferUsage.WriteOnly);
            _cubeVertexBuffer.SetData(_cubeVertices);
            _cubeIndexBuffer = new IndexBuffer(_graphicsDevice, typeof(short), _cubeIndices.Length, BufferUsage.WriteOnly);
            _cubeIndexBuffer.SetData(_cubeIndices);

            // Create vertex and index buffers for ground plane
            _groundVertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionColor), _groundVertices.Length, BufferUsage.WriteOnly);
            _groundVertexBuffer.SetData(_groundVertices);
            _groundIndexBuffer = new IndexBuffer(_graphicsDevice, typeof(short), _groundIndices.Length, BufferUsage.WriteOnly);
            _groundIndexBuffer.SetData(_groundIndices);

            // Create vertex and index buffers for walls
            _wallVertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionColor), _wallVertices.Length, BufferUsage.WriteOnly);
            _wallVertexBuffer.SetData(_wallVertices);
            _wallIndexBuffer = new IndexBuffer(_graphicsDevice, typeof(short), _wallIndices.Length, BufferUsage.WriteOnly);
            _wallIndexBuffer.SetData(_wallIndices);

            // Initialize input states
            _previousKeyboardState = Keyboard.GetState();
            _previousMouseState = Mouse.GetState();

            // Center the mouse cursor initially
            Vector2 screenCenter = new Vector2(_graphicsDevice.Viewport.Width / 2f, _graphicsDevice.Viewport.Height / 2f);
            Mouse.SetPosition((int)screenCenter.X, (int)screenCenter.Y);

            System.Diagnostics.Debug.WriteLine($"Character {_character.Name} ({_character.Class}) entered Training Grounds");
            System.Diagnostics.Debug.WriteLine($"Training area: {GROUND_SIZE}x{GROUND_SIZE} units, Wall height: {WALL_HEIGHT} units");
        }

        private void CreatePlayerCube()
        {
            Color primaryColor = GetClassColor();
            Color secondaryColor = Color.White;

            _cubeVertices = new VertexPositionColor[8];

            // Front face vertices
            _cubeVertices[0] = new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0.5f), primaryColor);
            _cubeVertices[1] = new VertexPositionColor(new Vector3(0.5f, -0.5f, 0.5f), secondaryColor);
            _cubeVertices[2] = new VertexPositionColor(new Vector3(0.5f, 0.5f, 0.5f), primaryColor);
            _cubeVertices[3] = new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0.5f), secondaryColor);

            // Back face vertices
            _cubeVertices[4] = new VertexPositionColor(new Vector3(-0.5f, -0.5f, -0.5f), secondaryColor);
            _cubeVertices[5] = new VertexPositionColor(new Vector3(0.5f, -0.5f, -0.5f), primaryColor);
            _cubeVertices[6] = new VertexPositionColor(new Vector3(0.5f, 0.5f, -0.5f), secondaryColor);
            _cubeVertices[7] = new VertexPositionColor(new Vector3(-0.5f, 0.5f, -0.5f), primaryColor);

            // Define the indices for the cube faces
            _cubeIndices = new short[]
            {
                // Front face
                0, 1, 2, 0, 2, 3,
                // Back face
                4, 6, 5, 4, 7, 6,
                // Left face
                4, 0, 3, 4, 3, 7,
                // Right face
                1, 5, 6, 1, 6, 2,
                // Top face
                3, 2, 6, 3, 6, 7,
                // Bottom face
                4, 5, 1, 4, 1, 0
            };
        }

        private void CreateGroundPlane()
        {
            // Create a simple ground plane
            Color groundColor = new Color(50, 100, 50); // Dark green
            Color groundAccent = new Color(60, 120, 60); // Slightly lighter green

            float halfSize = GROUND_SIZE / 2f;

            _groundVertices = new VertexPositionColor[4];
            _groundVertices[0] = new VertexPositionColor(new Vector3(-halfSize, GROUND_Y, -halfSize), groundColor);
            _groundVertices[1] = new VertexPositionColor(new Vector3(halfSize, GROUND_Y, -halfSize), groundAccent);
            _groundVertices[2] = new VertexPositionColor(new Vector3(halfSize, GROUND_Y, halfSize), groundColor);
            _groundVertices[3] = new VertexPositionColor(new Vector3(-halfSize, GROUND_Y, halfSize), groundAccent);

            // Two triangles to form a quad
            _groundIndices = new short[]
            {
                0, 1, 2, // First triangle
                0, 2, 3  // Second triangle
            };
        }

        private void CreateBoundaryWalls()
        {
            Color wallColor = new Color(100, 100, 120); // Gray-blue walls
            float halfSize = GROUND_SIZE / 2f;
            float halfThickness = WALL_THICKNESS / 2f;

            // We'll create 4 walls: North, South, East, West
            // Each wall will be a box with 8 vertices
            _wallVertices = new VertexPositionColor[32]; // 8 vertices per wall * 4 walls
            
            int vertexIndex = 0;

            // North Wall (positive Z)
            CreateWallVertices(
                new Vector3(-halfSize - halfThickness, GROUND_Y, halfSize - halfThickness),
                new Vector3(halfSize + halfThickness, GROUND_Y, halfSize + halfThickness),
                WALL_HEIGHT, wallColor, ref vertexIndex);

            // South Wall (negative Z)
            CreateWallVertices(
                new Vector3(-halfSize - halfThickness, GROUND_Y, -halfSize - halfThickness),
                new Vector3(halfSize + halfThickness, GROUND_Y, -halfSize + halfThickness),
                WALL_HEIGHT, wallColor, ref vertexIndex);

            // East Wall (positive X)
            CreateWallVertices(
                new Vector3(halfSize - halfThickness, GROUND_Y, -halfSize - halfThickness),
                new Vector3(halfSize + halfThickness, GROUND_Y, halfSize + halfThickness),
                WALL_HEIGHT, wallColor, ref vertexIndex);

            // West Wall (negative X)
            CreateWallVertices(
                new Vector3(-halfSize - halfThickness, GROUND_Y, -halfSize - halfThickness),
                new Vector3(-halfSize + halfThickness, GROUND_Y, halfSize + halfThickness),
                WALL_HEIGHT, wallColor, ref vertexIndex);

            // Create indices for all 4 walls (each wall uses the standard cube indices pattern)
            _wallIndices = new short[144]; // 36 indices per wall * 4 walls
            int indexOffset = 0;
            
            for (int wall = 0; wall < 4; wall++)
            {
                int vertexOffset = wall * 8;
                short[] wallCubeIndices = new short[]
                {
                    // Front face
                    0, 1, 2, 0, 2, 3,
                    // Back face
                    4, 6, 5, 4, 7, 6,
                    // Left face
                    4, 0, 3, 4, 3, 7,
                    // Right face
                    1, 5, 6, 1, 6, 2,
                    // Top face
                    3, 2, 6, 3, 6, 7,
                    // Bottom face
                    4, 5, 1, 4, 1, 0
                };

                for (int i = 0; i < wallCubeIndices.Length; i++)
                {
                    _wallIndices[indexOffset + i] = (short)(wallCubeIndices[i] + vertexOffset);
                }
                indexOffset += wallCubeIndices.Length;
            }
        }

        private void CreateWallVertices(Vector3 min, Vector3 max, float height, Color color, ref int startIndex)
        {
            // Create a box from min to max with given height
            _wallVertices[startIndex + 0] = new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), color);
            _wallVertices[startIndex + 1] = new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), color);
            _wallVertices[startIndex + 2] = new VertexPositionColor(new Vector3(max.X, min.Y + height, max.Z), color);
            _wallVertices[startIndex + 3] = new VertexPositionColor(new Vector3(min.X, min.Y + height, max.Z), color);

            _wallVertices[startIndex + 4] = new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), color);
            _wallVertices[startIndex + 5] = new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), color);
            _wallVertices[startIndex + 6] = new VertexPositionColor(new Vector3(max.X, min.Y + height, min.Z), color);
            _wallVertices[startIndex + 7] = new VertexPositionColor(new Vector3(min.X, min.Y + height, min.Z), color);

            startIndex += 8;
        }

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

        public override void Update(GameTime gameTime)
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();
            MouseState currentMouseState = Mouse.GetState();

            if (currentKeyboardState.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape))
            {
                _game.ChangeState(new MainMenuState(_game, _graphicsDevice, _content));
                return;
            }

            if (currentKeyboardState.IsKeyDown(Keys.M) && !_previousKeyboardState.IsKeyDown(Keys.M))
            {
                _isMouseCaptured = !_isMouseCaptured;
            }

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Handle mouse look
            if (_isMouseCaptured)
            {
                Vector2 screenCenter = new Vector2(_graphicsDevice.Viewport.Width / 2f, _graphicsDevice.Viewport.Height / 2f);
                Vector2 mouseDelta = new Vector2(currentMouseState.X - screenCenter.X, currentMouseState.Y - screenCenter.Y);

                _playerRotationY -= mouseDelta.X * _mouseSensitivity;
                _playerRotationX -= mouseDelta.Y * _mouseSensitivity;
                _playerRotationX = MathHelper.Clamp(_playerRotationX, -MathHelper.PiOver2 + 0.1f, MathHelper.PiOver2 - 0.1f);

                Mouse.SetPosition((int)screenCenter.X, (int)screenCenter.Y);
            }

            // Handle WASD movement with boundary checking
            Vector3 moveDirection = Vector3.Zero;

            if (currentKeyboardState.IsKeyDown(Keys.W))
                moveDirection += Vector3.Forward;
            if (currentKeyboardState.IsKeyDown(Keys.S))
                moveDirection += Vector3.Backward;
            if (currentKeyboardState.IsKeyDown(Keys.A))
                moveDirection += Vector3.Left;
            if (currentKeyboardState.IsKeyDown(Keys.D))
                moveDirection += Vector3.Right;
            if (currentKeyboardState.IsKeyDown(Keys.Space))
                moveDirection += Vector3.Up;
            if (currentKeyboardState.IsKeyDown(Keys.LeftShift))
                moveDirection += Vector3.Down;

            if (moveDirection.Length() > 0)
            {
                moveDirection.Normalize();
                
                Matrix rotationMatrix = Matrix.CreateRotationY(_playerRotationY);
                moveDirection = Vector3.Transform(moveDirection, rotationMatrix);
                
                float classSpeedModifier = GetClassSpeedModifier();
                Vector3 newPosition = _playerPosition + moveDirection * _playerSpeed * classSpeedModifier * deltaTime;

                // Apply boundary constraints and ground constraint
                newPosition = ApplyBoundaryConstraints(newPosition);
                
                _playerPosition = newPosition;
            }

            UpdateCamera();

            _previousKeyboardState = currentKeyboardState;
            _previousMouseState = currentMouseState;
        }

        private Vector3 ApplyBoundaryConstraints(Vector3 newPosition)
        {
            float halfSize = GROUND_SIZE / 2f;
            float playerRadius = 0.5f; // Half the size of player cube

            // Constrain to ground boundaries
            newPosition.X = MathHelper.Clamp(newPosition.X, -halfSize + playerRadius, halfSize - playerRadius);
            newPosition.Z = MathHelper.Clamp(newPosition.Z, -halfSize + playerRadius, halfSize - playerRadius);
            
            // Keep player above ground level
            newPosition.Y = Math.Max(newPosition.Y, GROUND_Y + playerRadius);

            return newPosition;
        }

        private float GetClassSpeedModifier()
        {
            switch (_character.Class)
            {
                case CharacterClass.Brawler:
                    return 0.8f;
                case CharacterClass.Ranger:
                    return 1.2f;
                case CharacterClass.Spellcaster:
                    return 1.0f;
                default:
                    return 1.0f;
            }
        }

        private void UpdateCamera()
        {
            Matrix rotationMatrix = Matrix.CreateRotationX(_playerRotationX) * Matrix.CreateRotationY(_playerRotationY);
            
            Vector3 cameraOffset = Vector3.Transform(new Vector3(0, 1, 3), rotationMatrix);
            _cameraPosition = _playerPosition + cameraOffset;
            
            Vector3 forwardDirection = Vector3.Transform(Vector3.Forward, rotationMatrix);
            _cameraTarget = _playerPosition + forwardDirection;

            _view = Matrix.CreateLookAt(_cameraPosition, _cameraTarget, _cameraUp);
        }

        public override void PostUpdate(GameTime gameTime)
        {
            // No state transitions needed in PostUpdate for now
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _graphicsDevice.Clear(new Color(135, 206, 235)); // Sky blue background

            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            _basicEffect.View = _view;
            _basicEffect.Projection = _projection;

            // Draw ground plane
            _graphicsDevice.SetVertexBuffer(_groundVertexBuffer);
            _graphicsDevice.Indices = _groundIndexBuffer;
            _basicEffect.World = Matrix.Identity;
            
            foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    0,
                    _groundVertices.Length,
                    0,
                    _groundIndices.Length / 3);
            }

            // Draw boundary walls
            _graphicsDevice.SetVertexBuffer(_wallVertexBuffer);
            _graphicsDevice.Indices = _wallIndexBuffer;
            _basicEffect.World = Matrix.Identity;
            
            foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    0,
                    _wallVertices.Length,
                    0,
                    _wallIndices.Length / 3);
            }

            // Draw player cube
            _graphicsDevice.SetVertexBuffer(_cubeVertexBuffer);
            _graphicsDevice.Indices = _cubeIndexBuffer;
            _world = Matrix.CreateTranslation(_playerPosition);
            _basicEffect.World = _world;

            foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    0,
                    _cubeVertices.Length,
                    0,
                    _cubeIndices.Length / 3);
            }

            // Draw UI
            spriteBatch.Begin();
            
            // Character information
            spriteBatch.DrawString(_content.Load<SpriteFont>("Fonts/DefaultFont"), 
                $"Character: {_character.Name} ({_character.Class})", 
                new Vector2(10, 10), 
                Color.White);
            
            spriteBatch.DrawString(_content.Load<SpriteFont>("Fonts/DefaultFont"), 
                $"Health: {_character.MaxHealth} | {_character.ResourceType}: {_character.MaxResource}", 
                new Vector2(10, 30), 
                Color.LightBlue);
            
            // Training area info
            spriteBatch.DrawString(_content.Load<SpriteFont>("Fonts/DefaultFont"), 
                $"Training Grounds ({GROUND_SIZE}x{GROUND_SIZE} area) - WASD to move, Mouse to look around", 
                new Vector2(10, 60), 
                Color.White);
            spriteBatch.DrawString(_content.Load<SpriteFont>("Fonts/DefaultFont"), 
                "Space/Shift for up/down, M to toggle mouse capture, ESC to return to menu", 
                new Vector2(10, 80), 
                Color.White);
                
            // Position information
            spriteBatch.DrawString(_content.Load<SpriteFont>("Fonts/DefaultFont"), 
                $"Position: X:{_playerPosition.X:F1} Y:{_playerPosition.Y:F1} Z:{_playerPosition.Z:F1}", 
                new Vector2(10, 110), 
                Color.Yellow);
                
            // Class-specific tip
            string classTip = GetClassTip();
            spriteBatch.DrawString(_content.Load<SpriteFont>("Fonts/DefaultFont"), 
                classTip, 
                new Vector2(10, 130), 
                GetClassColor());

            // Environment status
            spriteBatch.DrawString(_content.Load<SpriteFont>("Fonts/DefaultFont"), 
                "Environment: Ground plane and boundary walls active", 
                new Vector2(10, 150), 
                Color.LightGreen);
                
            spriteBatch.End();

            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        private string GetClassTip()
        {
            switch (_character.Class)
            {
                case CharacterClass.Brawler:
                    return "Tip: Brawlers are tough but slower. Get close to enemies!";
                case CharacterClass.Ranger:
                    return "Tip: Rangers are fast and agile. Keep your distance!";
                case CharacterClass.Spellcaster:
                    return "Tip: Spellcasters have powerful magic. Manage your mana wisely!";
                default:
                    return "";
            }
        }
    }
}