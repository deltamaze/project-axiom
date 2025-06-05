
namespace project_axiom.Rendering
{
    /// <summary>
    /// Handles rendering of the training ground environment (ground plane and boundary walls)
    /// </summary>
    public class EnvironmentRenderer
    {
        private GraphicsDevice _graphicsDevice;
        
        // Ground rendering
        private VertexBuffer _groundVertexBuffer;
        private IndexBuffer _groundIndexBuffer;
        private VertexPositionColor[] _groundVertices;
        private short[] _groundIndices;

        // Wall rendering
        private VertexBuffer _wallVertexBuffer;
        private IndexBuffer _wallIndexBuffer;
        private VertexPositionColor[] _wallVertices;
        private short[] _wallIndices;

        public EnvironmentRenderer(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            InitializeEnvironment();
        }

        /// <summary>
        /// Initialize all environment geometry and buffers
        /// </summary>
        private void InitializeEnvironment()
        {
            InitializeGround();
            InitializeWalls();
        }

        /// <summary>
        /// Initialize the ground plane geometry and buffers
        /// </summary>
        private void InitializeGround()
        {
            // Get geometry from GeometryBuilder
            (_groundVertices, _groundIndices) = GeometryBuilder.CreateGroundPlane();

            // Create vertex and index buffers for ground
            _groundVertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionColor), _groundVertices.Length, BufferUsage.WriteOnly);
            _groundVertexBuffer.SetData(_groundVertices);

            _groundIndexBuffer = new IndexBuffer(_graphicsDevice, typeof(short), _groundIndices.Length, BufferUsage.WriteOnly);
            _groundIndexBuffer.SetData(_groundIndices);
        }

        /// <summary>
        /// Initialize the boundary walls geometry and buffers
        /// </summary>
        private void InitializeWalls()
        {
            // Get geometry from GeometryBuilder
            (_wallVertices, _wallIndices) = GeometryBuilder.CreateBoundaryWalls();

            // Create vertex and index buffers for walls
            _wallVertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionColor), _wallVertices.Length, BufferUsage.WriteOnly);
            _wallVertexBuffer.SetData(_wallVertices);

            _wallIndexBuffer = new IndexBuffer(_graphicsDevice, typeof(short), _wallIndices.Length, BufferUsage.WriteOnly);
            _wallIndexBuffer.SetData(_wallIndices);
        }

        /// <summary>
        /// Draw the ground plane
        /// </summary>
        public void DrawGround(BasicEffect basicEffect)
        {
            _graphicsDevice.SetVertexBuffer(_groundVertexBuffer);
            _graphicsDevice.Indices = _groundIndexBuffer;
            basicEffect.World = Matrix.Identity;
            
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
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
        }

        /// <summary>
        /// Draw the boundary walls
        /// </summary>
        public void DrawWalls(BasicEffect basicEffect)
        {
            _graphicsDevice.SetVertexBuffer(_wallVertexBuffer);
            _graphicsDevice.Indices = _wallIndexBuffer;
            basicEffect.World = Matrix.Identity;
            
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
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
        }

        /// <summary>
        /// Draw both ground and walls in one call
        /// </summary>
        public void DrawEnvironment(BasicEffect basicEffect)
        {
            DrawGround(basicEffect);
            DrawWalls(basicEffect);
        }

        /// <summary>
        /// Dispose of graphics resources
        /// </summary>
        public void Dispose()
        {
            _groundVertexBuffer?.Dispose();
            _groundIndexBuffer?.Dispose();
            _wallVertexBuffer?.Dispose();
            _wallIndexBuffer?.Dispose();
        }
    }
}