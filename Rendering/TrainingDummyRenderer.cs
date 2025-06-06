namespace project_axiom.Rendering;

/// <summary>
/// Handles rendering of training dummy cubes in the training grounds
/// </summary>
public class TrainingDummyRenderer
{
    private GraphicsDevice _graphicsDevice;
    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    private VertexPositionColor[] _vertices;
    private short[] _indices;
    private SpriteFont _font;

    public TrainingDummyRenderer(GraphicsDevice graphicsDevice, SpriteFont font)
    {
        _graphicsDevice = graphicsDevice;
        _font = font;
        InitializeGeometry();
    }

    /// <summary>
    /// Initialize the training dummy geometry and buffers
    /// </summary>
    private void InitializeGeometry()
    {
        // Create a standard training dummy with default colors
        Color primaryColor = new Color(200, 100, 50);   // Orange-brown
        Color secondaryColor = new Color(150, 75, 25);  // Darker brown
        
        // Get geometry from GeometryBuilder
        (_vertices, _indices) = GeometryBuilder.CreateTrainingDummy(primaryColor, secondaryColor, 1.2f);

        // Create vertex and index buffers
        _vertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionColor), _vertices.Length, BufferUsage.WriteOnly);
        _vertexBuffer.SetData(_vertices);

        _indexBuffer = new IndexBuffer(_graphicsDevice, typeof(short), _indices.Length, BufferUsage.WriteOnly);
        _indexBuffer.SetData(_indices);
    }

    /// <summary>
    /// Draw a single training dummy at the specified position
    /// </summary>
    public void DrawDummy(BasicEffect basicEffect, Vector3 position)
    {
        // Set up the graphics device
        _graphicsDevice.SetVertexBuffer(_vertexBuffer);
        _graphicsDevice.Indices = _indexBuffer;

        // Create world matrix with dummy position
        Matrix world = Matrix.CreateTranslation(position);
        basicEffect.World = world;

        // Draw the dummy
        foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphicsDevice.DrawIndexedPrimitives(
                PrimitiveType.TriangleList,
                0,
                0,
                _indices.Length / 3);
        }
    }

    /// <summary>
    /// Draw with highlight (e.g., yellow overlay)
    /// </summary>
    private void DrawDummyWithHighlight(BasicEffect basicEffect, Vector3 position)
    {
        // Draw normal dummy
        DrawDummy(basicEffect, position);
        // Draw highlight overlay (e.g., slightly larger, yellow)
        var prevColor = basicEffect.DiffuseColor;
        basicEffect.DiffuseColor = Color.Yellow.ToVector3();
        Matrix world = Matrix.CreateScale(1.1f) * Matrix.CreateTranslation(position);
        basicEffect.World = world;
        foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _indices.Length / 3);
        }
        basicEffect.DiffuseColor = prevColor;
    }

    /// <summary>
    /// Draw multiple training dummies at their positions
    /// </summary>
    public void DrawDummies(BasicEffect basicEffect, List<TrainingDummy> dummies, TrainingDummy targetedDummy = null)
    {
        foreach (var dummy in dummies)
        {
            if (dummy.IsAlive)
            {
                if (dummy == targetedDummy)
                {
                    // Draw with highlight (e.g., yellow overlay)
                    DrawDummyWithHighlight(basicEffect, dummy.Position);
                }
                else
                {
                    DrawDummy(basicEffect, dummy.Position);
                }
            }
        }
    }

    /// <summary>
    /// Draw health bars above training dummies (called during 2D UI rendering)
    /// </summary>
    public void DrawDummyHealthBars(SpriteBatch spriteBatch, List<TrainingDummy> dummies, Matrix view, Matrix projection, TrainingDummy targetedDummy = null)
    {
        foreach (var dummy in dummies)
        {
            if (dummy.IsAlive)
            {
                bool highlight = (dummy == targetedDummy);
                DrawHealthBar(spriteBatch, dummy, view, projection, highlight);
            }
        }
    }

    /// <summary>
    /// Draw a health bar above a specific training dummy
    /// </summary>
    private void DrawHealthBar(SpriteBatch spriteBatch, TrainingDummy dummy, Matrix view, Matrix projection, bool highlight = false)
    {
        // Calculate screen position of the dummy
        Vector3 worldPosition = dummy.Position + new Vector3(0, 1.0f, 0); // Offset above dummy
        Vector3 screenPosition = _graphicsDevice.Viewport.Project(worldPosition, projection, view, Matrix.Identity);

        // Only draw if the dummy is visible on screen
        if (screenPosition.Z > 0 && screenPosition.Z < 1)
        {
            Vector2 healthBarPosition = new Vector2(screenPosition.X - 30, screenPosition.Y - 20);
            
            // Draw health bar background
            Texture2D healthBarTexture = CreateSolidColorTexture(_graphicsDevice, Color.Black);
            Rectangle backgroundRect = new Rectangle((int)healthBarPosition.X, (int)healthBarPosition.Y, 60, 8);
            spriteBatch.Draw(healthBarTexture, backgroundRect, highlight ? Color.Yellow : Color.Black);

            // Draw health bar fill
            float healthPercentage = dummy.GetHealthPercentage();
            Color healthColor = healthPercentage > 0.6f ? Color.Green : healthPercentage > 0.3f ? Color.Yellow : Color.Red;
            Rectangle healthRect = new Rectangle((int)healthBarPosition.X + 1, (int)healthBarPosition.Y + 1, (int)((60 - 2) * healthPercentage), 6);
            spriteBatch.Draw(healthBarTexture, healthRect, healthColor);

            // Draw dummy name
            Vector2 namePosition = new Vector2(healthBarPosition.X, healthBarPosition.Y - 20);
            Vector2 nameSize = _font.MeasureString(dummy.Name);
            namePosition.X -= (nameSize.X - 60) / 2; // Center the name over the health bar
            spriteBatch.DrawString(_font, dummy.Name, namePosition, highlight ? Color.Yellow : Color.White);

            healthBarTexture.Dispose();
        }
    }

    /// <summary>
    /// Helper method to create a solid color texture
    /// </summary>
    private Texture2D CreateSolidColorTexture(GraphicsDevice graphicsDevice, Color color)
    {
        Texture2D texture = new Texture2D(graphicsDevice, 1, 1);
        texture.SetData(new[] { color });
        return texture;
    }

    /// <summary>
    /// Update the renderer with custom colors for dummies (if needed for variety)
    /// </summary>
    public void UpdateDummyColors(Color primaryColor, Color secondaryColor, float scale = 1.2f)
    {
        // Dispose old buffers
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();

        // Recreate with new colors
        (_vertices, _indices) = GeometryBuilder.CreateTrainingDummy(primaryColor, secondaryColor, scale);

        _vertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionColor), _vertices.Length, BufferUsage.WriteOnly);
        _vertexBuffer.SetData(_vertices);

        _indexBuffer = new IndexBuffer(_graphicsDevice, typeof(short), _indices.Length, BufferUsage.WriteOnly);
        _indexBuffer.SetData(_indices);
    }

    /// <summary>
    /// Dispose of graphics resources
    /// </summary>
    public void Dispose()
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
    }
}