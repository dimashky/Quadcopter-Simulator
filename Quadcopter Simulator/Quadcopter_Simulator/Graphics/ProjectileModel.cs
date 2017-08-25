using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TripleM.Quadcopter.Graphics
{
    public class ProjectileModel
    {
        public static Model model, trajectoryModel;
        List<Vector3> trajectory;
        Vector3 lastPosition;
        bool trajectoryLock;
        Vector2 rotationangles;
        Vector3 rotaion_value;
        public ProjectileModel(Vector3 firePosition,Vector3 rotation, bool trajectoryLock)
        {
            trajectory = new List<Vector3>();
            lastPosition = firePosition;
            this.trajectoryLock = trajectoryLock;
            rotaion_value = rotation;
            rotationangles = Vector2.Zero;
        }

        private double distance(Vector3 a,Vector3 b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2d) + Math.Pow(a.Y - b.Y, 2d) + Math.Pow(a.Z - b.Z, 2d));
        }
        private void drawTrajectory(Matrix View ,Matrix Projection)
        {

            Matrix[] Transform = new Matrix[trajectoryModel.Bones.Count];
            trajectoryModel.CopyAbsoluteBoneTransformsTo(Transform);
            foreach (Vector3 Pos in trajectory)
            {
                foreach (ModelMesh mesh in trajectoryModel.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.Projection = Projection;
                        effect.View = View;
                        effect.World = Transform[mesh.ParentBone.Index] * Matrix.CreateScale(0.01f) * Matrix.CreateFromYawPitchRoll(-MathHelper.PiOver2, MathHelper.PiOver2, 0) * Matrix.CreateTranslation(Pos.X, Pos.Y, Pos.Z);
                    }
                    mesh.Draw();
                }
            }
        }
        public void Draw(Matrix View, Matrix Projection, Vector3 Pos)
        {
            
            rotationangles.X += rotaion_value.X/10F;
            rotationangles.Y += rotaion_value.Y/10F;
            

            Matrix[] Transform = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(Transform);
            if (!trajectoryLock)
                if (distance(lastPosition, Pos) >= .2f)
                {
                    trajectory.Add(Pos);
                    lastPosition = Pos;
                }
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.Projection = Projection;
                    effect.View = View;
                    effect.World = Transform[mesh.ParentBone.Index] * Matrix.CreateScale(.0015f) * Matrix.CreateRotationX(rotationangles.X) * Matrix.CreateRotationY(rotationangles.Y) *  Matrix.CreateTranslation(Pos.X, Pos.Y, Pos.Z);
                }
                mesh.Draw();
            }
            if (!trajectoryLock)
                drawTrajectory(View, Projection);
        }
    }
}
