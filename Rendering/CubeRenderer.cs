

namespace project_axiom.Rendering;

/// <summary>
/// Handles rendering of the player's cube character
/// </summary>
public class CubeRenderer
{
  private GraphicsDevice _graphicsDevice;
  private VertexBuffer _vertexBuffer;
  private IndexBuffer _indexBuffer;
  private VertexPositionColor[] _vertices;
  private short[] _indices;

  public CubeRenderer(GraphicsDevice graphicsDevice, CharacterClass characterClass)
  {
    _graphicsDevice = graphicsDevice;
    InitializeCube(characterClass);
  }

  /// <summary>
  /// Initialize the cube geometry and buffers
  /// </summary>
  private void InitializeCube(CharacterClass characterClass)
  {
    // Get geometry from GeometryBuilder
    (_vertices, _indices) = GeometryBuilder.CreatePlayerCube(characterClass);

    // Create vertex and index buffers
    _vertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionColor), _vertices.Length, BufferUsage.WriteOnly);
    _vertexBuffer.SetData(_vertices);

    _indexBuffer = new IndexBuffer(_graphicsDevice, typeof(short), _indices.Length, BufferUsage.WriteOnly);
    _indexBuffer.SetData(_indices);
  }

  /// <summary>
  /// Draw the player cube at the specified position
  /// </summary>
  public void Draw(BasicEffect basicEffect, Vector3 position)
  {
    // Set up the graphics device
    _graphicsDevice.SetVertexBuffer(_vertexBuffer);
    _graphicsDevice.Indices = _indexBuffer;

    // Create world matrix with player position
    Matrix world = Matrix.CreateTranslation(position);
    basicEffect.World = world;

    // Draw the cube
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
  /// Update the cube for a different character class (if needed)
  /// </summary>
  public void UpdateCharacterClass(CharacterClass characterClass)
  {
    // Dispose old buffers
    _vertexBuffer?.Dispose();
    _indexBuffer?.Dispose();

    // Recreate with new class
    InitializeCube(characterClass);
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
