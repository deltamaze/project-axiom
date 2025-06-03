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

        // Camera properties
        private Vector3 _cameraPosition = new Vector3(0f, 2f, 5f);
        private Vector3 _cameraTarget = Vector3.Zero;
        private Vector3 _cameraUp = Vector3.Up;
        private Matrix _world = Matrix.Identity;
        private Matrix _view;
        private Matrix _projection;

        // Input handling
        private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;

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
        }

        private void CreateCube()
        {
            // Define the 8 vertices of a cube
            _cubeVertices = new VertexPositionColor[8];

            // Front face vertices
            _cubeVertices[0] = new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0.5f), Color.White);
            _cubeVertices[1] = new VertexPositionColor(new Vector3(0.5f, -0.5f, 0.5f), Color.White);
            _cubeVertices[2] = new VertexPositionColor(new Vector3(0.5f, 0.5f, 0.5f), Color.White);
            _cubeVertices[3] = new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0.5f), Color.White);

            // Back face vertices
            _cubeVertices[4] = new VertexPositionColor(new Vector3(-0.5f, -0.5f, -0.5f), Color.White);
            _cubeVertices[5] = new VertexPositionColor(new Vector3(0.5f, -0.5f, -0.5f), Color.White);
            _cubeVertices[6] = new VertexPositionColor(new Vector3(0.5f, 0.5f, -0.5f), Color.White);
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

            // Basic camera rotation around the cube for now
            float time = (float)gameTime.TotalGameTime.TotalSeconds;
            _cameraPosition = new Vector3(
                (float)Math.Sin(time * 0.5f) * 5f,
                2f,
                (float)Math.Cos(time * 0.5f) * 5f);

            // Update view matrix
            _view = Matrix.CreateLookAt(_cameraPosition, _cameraTarget, _cameraUp);

            // Store previous input states
            _previousKeyboardState = currentKeyboardState;
            _previousMouseState = currentMouseState;
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
                "Training Grounds - Press ESC to return to Main Menu", 
                new Vector2(10, 10), 
                Color.White);
            spriteBatch.End();

            // Reset graphics device state after SpriteBatch
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
    }
}