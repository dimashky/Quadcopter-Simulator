using System;
using Microsoft.Xna.Framework;


namespace TripleM.Quadcopter.Graphics
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Camera
    {
        public Matrix View { get; protected set; }
        public Matrix Projection { get; protected set; }
        private Vector3 target, cameraPosition;

        public Camera(Game game,Vector3 position, Vector3 target,Vector3 up)
        {
            View = Matrix.CreateLookAt(position, target, up);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)game.Window.ClientBounds.Width / (float)game.Window.ClientBounds.Height, .01f, 3000f);
        }
        public Vector3 getCameraPosition()
        {
            return cameraPosition;
        }
        public void updateTarget(Vector3 target, float Yaw, float hieght, float zoom)
        {
            this.target = target;
            updateCameraPosition(Yaw, hieght, zoom);
            View = Matrix.CreateLookAt(cameraPosition, target, Vector3.Transform(Vector3.Up, Matrix.CreateRotationY(Yaw)));
        }

        private void updateCameraPosition(float Yaw, float hieght, float zoom)
        {
            cameraPosition.X = target.X + (float)Math.Sin(Yaw) * (0.4f + zoom);
            cameraPosition.Y = target.Y + .1f + hieght;
            cameraPosition.Z = target.Z + (float)Math.Cos(Yaw) * (0.4f + zoom);
        }
    }
}
