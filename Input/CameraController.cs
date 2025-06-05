
namespace project_axiom.Input
{
    /// <summary>
    /// Handles camera positioning, rotation, and view matrix management
    /// </summary>
    public class CameraController
    {
        // Camera properties
        public Vector3 Position { get; private set; }
        public Vector3 Target { get; private set; }
        public Vector3 Up { get; private set; } = Vector3.Up;
        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }

        public CameraController(GraphicsDevice graphicsDevice)
        {
            // Set up projection matrix
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45f),
                graphicsDevice.Viewport.AspectRatio,
                0.1f,
                100f);
        }

        /// <summary>
        /// Update camera position and view matrix based on player position and rotation
        /// </summary>
        public void Update(Vector3 playerPosition, float playerRotationX, float playerRotationY)
        {
            // Create rotation matrix from player rotation
            Matrix rotationMatrix = Matrix.CreateRotationX(playerRotationX) * Matrix.CreateRotationY(playerRotationY);
            
            // Calculate camera offset from player (third-person style camera behind player)
            Vector3 cameraOffset = Vector3.Transform(new Vector3(0, 1, 3), rotationMatrix);
            Position = playerPosition + cameraOffset;
            
            // Calculate where the camera should look (forward direction from player)
            Vector3 forwardDirection = Vector3.Transform(Vector3.Forward, rotationMatrix);
            Target = playerPosition + forwardDirection;

            // Update view matrix
            View = Matrix.CreateLookAt(Position, Target, Up);
        }

        /// <summary>
        /// Update the projection matrix (useful for window resizing)
        /// </summary>
        public void UpdateProjection(GraphicsDevice graphicsDevice)
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45f),
                graphicsDevice.Viewport.AspectRatio,
                0.1f,
                100f);
        }

        /// <summary>
        /// Get the forward direction vector of the camera
        /// </summary>
        public Vector3 GetForwardDirection()
        {
            return Vector3.Normalize(Target - Position);
        }

        /// <summary>
        /// Get the right direction vector of the camera
        /// </summary>
        public Vector3 GetRightDirection()
        {
            return Vector3.Normalize(Vector3.Cross(GetForwardDirection(), Up));
        }
    }
}