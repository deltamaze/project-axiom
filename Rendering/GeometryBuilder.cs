

namespace project_axiom.Rendering;

  /// <summary>
  /// Static utility class for creating 3D geometry vertices and indices
  /// </summary>
  public static class GeometryBuilder
  {
      // Environment constants
      public const float GROUND_SIZE = 50f;
      public const float WALL_HEIGHT = 5f;
      public const float WALL_THICKNESS = 1f;
      public const float GROUND_Y = 0f;
      public const float WALL_FOUNDATION_DEPTH = 0.05f;

      /// <summary>
      /// Creates vertices and indices for a player cube with class-specific colors
      /// </summary>
      public static (VertexPositionColor[] vertices, short[] indices) CreatePlayerCube(CharacterClass characterClass)
      {
          Color primaryColor = GetClassColor(characterClass);
          Color secondaryColor = Color.White;

          var vertices = new VertexPositionColor[8];

          // Front face vertices
          vertices[0] = new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0.5f), primaryColor);
          vertices[1] = new VertexPositionColor(new Vector3(0.5f, -0.5f, 0.5f), secondaryColor);
          vertices[2] = new VertexPositionColor(new Vector3(0.5f, 0.5f, 0.5f), primaryColor);
          vertices[3] = new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0.5f), secondaryColor);

          // Back face vertices
          vertices[4] = new VertexPositionColor(new Vector3(-0.5f, -0.5f, -0.5f), secondaryColor);
          vertices[5] = new VertexPositionColor(new Vector3(0.5f, -0.5f, -0.5f), primaryColor);
          vertices[6] = new VertexPositionColor(new Vector3(0.5f, 0.5f, -0.5f), secondaryColor);
          vertices[7] = new VertexPositionColor(new Vector3(-0.5f, 0.5f, -0.5f), primaryColor);

          // Define the indices for the cube faces
          var indices = new short[]
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

          return (vertices, indices);
      }

      /// <summary>
      /// Creates vertices and indices for the ground plane
      /// </summary>
      public static (VertexPositionColor[] vertices, short[] indices) CreateGroundPlane()
      {
          Color groundColor = new Color(50, 100, 50); // Dark green
          Color groundAccent = new Color(60, 120, 60); // Slightly lighter green

          float halfSize = GROUND_SIZE / 2f;

          var vertices = new VertexPositionColor[4];
          vertices[0] = new VertexPositionColor(new Vector3(-halfSize, GROUND_Y, -halfSize), groundColor);
          vertices[1] = new VertexPositionColor(new Vector3(halfSize, GROUND_Y, -halfSize), groundAccent);
          vertices[2] = new VertexPositionColor(new Vector3(halfSize, GROUND_Y, halfSize), groundColor);
          vertices[3] = new VertexPositionColor(new Vector3(-halfSize, GROUND_Y, halfSize), groundAccent);

          // Two triangles to form a quad
          var indices = new short[]
          {
              0, 1, 2, // First triangle
              0, 2, 3  // Second triangle
          };

          return (vertices, indices);
      }

      /// <summary>
      /// Creates vertices and indices for boundary walls around the training area
      /// </summary>
      public static (VertexPositionColor[] vertices, short[] indices) CreateBoundaryWalls()
      {
          Color wallColor = new Color(100, 100, 120); // Gray-blue walls
          float halfSize = GROUND_SIZE / 2f;
          float halfThickness = WALL_THICKNESS / 2f;

          // We'll create 4 walls: North, South, East, West
          // Each wall will be a box with 8 vertices
          var vertices = new VertexPositionColor[32]; // 8 vertices per wall * 4 walls
          
          int vertexIndex = 0;

          // North Wall (positive Z)
          CreateWallVertices(
              new Vector3(-halfSize - halfThickness, GROUND_Y - WALL_FOUNDATION_DEPTH, halfSize - halfThickness),
              new Vector3(halfSize + halfThickness, GROUND_Y - WALL_FOUNDATION_DEPTH, halfSize + halfThickness),
              WALL_HEIGHT + WALL_FOUNDATION_DEPTH, wallColor, vertices, ref vertexIndex);

          // South Wall (negative Z)
          CreateWallVertices(
              new Vector3(-halfSize - halfThickness, GROUND_Y - WALL_FOUNDATION_DEPTH, -halfSize - halfThickness),
              new Vector3(halfSize + halfThickness, GROUND_Y - WALL_FOUNDATION_DEPTH, -halfSize + halfThickness),
              WALL_HEIGHT + WALL_FOUNDATION_DEPTH, wallColor, vertices, ref vertexIndex);

          // East Wall (positive X)
          CreateWallVertices(
              new Vector3(halfSize - halfThickness, GROUND_Y - WALL_FOUNDATION_DEPTH, -halfSize - halfThickness),
              new Vector3(halfSize + halfThickness, GROUND_Y - WALL_FOUNDATION_DEPTH, halfSize + halfThickness),
              WALL_HEIGHT + WALL_FOUNDATION_DEPTH, wallColor, vertices, ref vertexIndex);

          // West Wall (negative X)
          CreateWallVertices(
              new Vector3(-halfSize - halfThickness, GROUND_Y - WALL_FOUNDATION_DEPTH, -halfSize - halfThickness),
              new Vector3(-halfSize + halfThickness, GROUND_Y - WALL_FOUNDATION_DEPTH, halfSize + halfThickness),
              WALL_HEIGHT + WALL_FOUNDATION_DEPTH, wallColor, vertices, ref vertexIndex);

          // Create indices for all 4 walls (each wall uses the standard cube indices pattern)
          var indices = new short[144]; // 36 indices per wall * 4 walls
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
                  indices[indexOffset + i] = (short)(wallCubeIndices[i] + vertexOffset);
              }
              indexOffset += wallCubeIndices.Length;
          }

          return (vertices, indices);
      }

      /// <summary>
      /// Helper method to create vertices for a single wall
      /// </summary>
      private static void CreateWallVertices(Vector3 min, Vector3 max, float height, Color color, 
          VertexPositionColor[] vertices, ref int startIndex)
      {
          // Create a box from min to max with given height
          vertices[startIndex + 0] = new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), color);
          vertices[startIndex + 1] = new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), color);
          vertices[startIndex + 2] = new VertexPositionColor(new Vector3(max.X, min.Y + height, max.Z), color);
          vertices[startIndex + 3] = new VertexPositionColor(new Vector3(min.X, min.Y + height, max.Z), color);

          vertices[startIndex + 4] = new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), color);
          vertices[startIndex + 5] = new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), color);
          vertices[startIndex + 6] = new VertexPositionColor(new Vector3(max.X, min.Y + height, min.Z), color);
          vertices[startIndex + 7] = new VertexPositionColor(new Vector3(min.X, min.Y + height, min.Z), color);

          startIndex += 8;
      }

      /// <summary>
      /// Get the primary color for a character class
      /// </summary>
      private static Color GetClassColor(CharacterClass characterClass)
      {
          switch (characterClass)
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
  }