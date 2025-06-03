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
        private VertexPositionColor[] _cubeVertices;
        private short[] _cubeIndices;
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;

        // Player movement properties
        private Vector3 _playerPosition = Vector3.Zero;
        private float _playerSpeed = 5.0f;
        private float _playerRotationY = 0f; // Y-axis rotation for looking left/right
        private float _playerRotationX = 0f; // X-axis rotation for looking up/down
        private float _mouseSensitivity = 0.003f;

        // Camera properties (now based on player position and rotation)
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

        public TrainingGroundsState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
            : base(game, graphicsDevice, content)
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

            // Create cube vertices
            CreateCube();

            // Create vertex and index buffers
            _vertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionColor), _cubeVertices.Length, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(_cubeVertices);

            _indexBuffer = new IndexBuffer(_graphicsDevice, typeof(short), _cubeIndices.Length, BufferUsage.WriteOnly);
            _indexBuffer.SetData(_cubeIndices);

            // Initialize input states
            _previousKeyboardState = Keyboard.GetState();
            _previousMouseState = Mouse.GetState();

            // Center the mouse cursor initially
            Vector2 screenCenter = new Vector2(_graphicsDevice.Viewport.Width / 2f, _graphicsDevice.Viewport.Height / 2f);
            Mouse.SetPosition((int)screenCenter.X, (int)screenCenter.Y);
        }

        private void CreateCube()
        {
            // Define the 8 vertices of a cube
            _cubeVertices = new VertexPositionColor[8];

            // Front face vertices
            _cubeVertices[0] = new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0.5f), Color.Red);
            _cubeVertices[1] = new VertexPositionColor(new Vector3(0.5f, -0.5f, 0.5f), Color.Green);
            _cubeVertices[2] = new VertexPositionColor(new Vector3(0.5f, 0.5f, 0.5f), Color.Blue);
            _cubeVertices[3] = new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0.5f), Color.Yellow);

            // Back face vertices
            _cubeVertices[4] = new VertexPositionColor(new Vector3(-0.5f, -0.5f, -0.5f), Color.Purple);
            _cubeVertices[5] = new VertexPositionColor(new Vector3(0.5f, -0.5f, -0.5f), Color.Orange);
            _cubeVertices[6] = new VertexPositionColor(new Vector3(0.5f, 0.5f, -0.5f), Color.Cyan);
            _cubeVertices[7] = new VertexPositionColor(new Vector3(-0.5f, 0.5f, -0.5f), Color.White);

            // Define the indices for the cube faces (2 triangles per face)
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

        public override void Update(GameTime gameTime)
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();
            MouseState currentMouseState = Mouse.GetState();

            // Handle escape key to return to main menu
            if (currentKeyboardState.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape))
            {
                _game.ChangeState(new MainMenuState(_game, _graphicsDevice, _content));
                return;
            }

            // Handle mouse capture toggle (for debugging - press M to toggle)
            if (currentKeyboardState.IsKeyDown(Keys.M) && !_previousKeyboardState.IsKeyDown(Keys.M))
            {
                _isMouseCaptured = !_isMouseCaptured;
            }

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Handle mouse look (only if mouse is captured)
            if (_isMouseCaptured)
            {
                Vector2 screenCenter = new Vector2(_graphicsDevice.Viewport.Width / 2f, _graphicsDevice.Viewport.Height / 2f);
                Vector2 mouseDelta = new Vector2(currentMouseState.X - screenCenter.X, currentMouseState.Y - screenCenter.Y);

                // Apply mouse sensitivity and update rotation
                _playerRotationY -= mouseDelta.X * _mouseSensitivity; // Horizontal mouse movement rotates around Y-axis
                _playerRotationX -= mouseDelta.Y * _mouseSensitivity; // Vertical mouse movement rotates around X-axis

                // Clamp vertical rotation to prevent over-rotation
                _playerRotationX = MathHelper.Clamp(_playerRotationX, -MathHelper.PiOver2 + 0.1f, MathHelper.PiOver2 - 0.1f);

                // Reset mouse to center of screen
                Mouse.SetPosition((int)screenCenter.X, (int)screenCenter.Y);
            }

            // Handle WASD movement
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

            // Normalize movement direction to prevent faster diagonal movement
            if (moveDirection.Length() > 0)
            {
                moveDirection.Normalize();
                
                // Transform movement direction based on player's Y rotation (horizontal looking)
                Matrix rotationMatrix = Matrix.CreateRotationY(_playerRotationY);
                moveDirection = Vector3.Transform(moveDirection, rotationMatrix);
                
                // Apply movement
                _playerPosition += moveDirection * _playerSpeed * deltaTime;
            }

            // Update camera based on player position and rotation
            UpdateCamera();

            // Store previous input states
            _previousKeyboardState = currentKeyboardState;
            _previousMouseState = currentMouseState;
        }

        private void UpdateCamera()
        {
            // Create rotation matrix from player's rotation
            Matrix rotationMatrix = Matrix.CreateRotationX(_playerRotationX) * Matrix.CreateRotationY(_playerRotationY);
            
            // Calculate camera position (offset from player position for third-person view)
            // For first-person, you'd set _cameraPosition = _playerPosition
            Vector3 cameraOffset = Vector3.Transform(new Vector3(0, 1, 3), rotationMatrix);
            _cameraPosition = _playerPosition + cameraOffset;
            
            // Calculate what the camera is looking at
            Vector3 forwardDirection = Vector3.Transform(Vector3.Forward, rotationMatrix);
            _cameraTarget = _playerPosition + forwardDirection;

            // Update view matrix
            _view = Matrix.CreateLookAt(_cameraPosition, _cameraTarget, _cameraUp);
        }

        public override void PostUpdate(GameTime gameTime)
        {
            // No state transitions needed in PostUpdate for now
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Clear the screen with a dark blue color
            _graphicsDevice.Clear(Color.CornflowerBlue);

            // Set up the graphics device for 3D rendering
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            // Set vertex and index buffers
            _graphicsDevice.SetVertexBuffer(_vertexBuffer);
            _graphicsDevice.Indices = _indexBuffer;

            // Position the cube at the player's position
            _world = Matrix.CreateTranslation(_playerPosition);

            // Configure the BasicEffect
            _basicEffect.World = _world;
            _basicEffect.View = _view;
            _basicEffect.Projection = _projection;

            // Draw the cube
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

            // Draw UI text (instructions)
            spriteBatch.Begin();
            spriteBatch.DrawString(_content.Load<SpriteFont>("Fonts/DefaultFont"), 
                "Training Grounds - WASD to move, Mouse to look around", 
                new Vector2(10, 10), 
                Color.White);
            spriteBatch.DrawString(_content.Load<SpriteFont>("Fonts/DefaultFont"), 
                "Space/Shift for up/down, M to toggle mouse capture, ESC to return to menu", 
                new Vector2(10, 30), 
                Color.White);
            spriteBatch.DrawString(_content.Load<SpriteFont>("Fonts/DefaultFont"), 
                $"Position: X:{_playerPosition.X:F1} Y:{_playerPosition.Y:F1} Z:{_playerPosition.Z:F1}", 
                new Vector2(10, 60), 
                Color.Yellow);
            spriteBatch.End();

            // Reset graphics device state after SpriteBatch
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
    }
}