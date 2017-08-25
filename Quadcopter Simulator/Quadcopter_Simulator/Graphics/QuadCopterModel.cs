using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MathNet.Numerics.LinearAlgebra;

namespace TripleM.Quadcopter.Graphics
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class QuadCopterModel : Microsoft.Xna.Framework.GameComponent
    {
        private Model model;
        Vector<float> plates;
        public QuadCopterModel(Game game, Model m)
            : base(game)
        {
            plates = Vector<float>.Build.Dense(4, 0);
            model = m;
        }
        public override void Initialize()
        {
            base.Initialize();
        }

        public void PladesRotation(Vector<float>speedPerMin)
        {
            plates += speedPerMin;
            if (speedPerMin[0].Equals(0))
                plates = Vector<float>.Build.Dense(4, 0);

            //BackwordLeft
            model.Bones[2].Transform = model.Bones[2].Transform * Matrix.CreateTranslation(149.429947f, 0, -145.349991f) * Matrix.CreateRotationY(plates[0]/10000) * Matrix.CreateTranslation(-149.429947f, 0, 145.349991f);

            //BacwordRight
            model.Bones[3].Transform = model.Bones[3].Transform * Matrix.CreateTranslation(-149.429947f, 0, -145.349991f) * Matrix.CreateRotationY(plates[1] / 10000) * Matrix.CreateTranslation(149.429947f, 0, 145.349991f);

            //ForwordLeft
            model.Bones[4].Transform = model.Bones[4].Transform * Matrix.CreateTranslation(-149.429947f, 0, 145.349991f) * Matrix.CreateRotationY(plates[2] / 10000) * Matrix.CreateTranslation(149.429947f, 0, -145.349991f);

            //ForwordRight
            model.Bones[1].Transform = model.Bones[1].Transform * Matrix.CreateTranslation(149.429947f, 0, 145.349991f) * Matrix.CreateRotationY(plates[3] / 10000) * Matrix.CreateTranslation(-149.429947f, 0, -145.349991f);

        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public void Draw(Matrix View, Matrix Projection, Vector3 Pos, float Yaw, float Pitch, float Roll)
        {
            Matrix[] Transform = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(Transform);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.Projection = Projection;
                    effect.View = View;
                    effect.World = Transform[mesh.ParentBone.Index] * Matrix.CreateScale(.0002f) * Matrix.CreateFromYawPitchRoll(Yaw, Pitch, Roll) * Matrix.CreateTranslation(Pos);
                    effect.LightingEnabled = true; // turn on the lighting subsystem.

                    
                    effect.DirectionalLight0.DiffuseColor = new Vector3(0.0f, 0.0f, 0.0f); 
                    effect.DirectionalLight0.Direction = new Vector3(1.0f, .2f, 1.0f);
                    effect.DirectionalLight0.SpecularColor = new Vector3(0.5f, 0.0f, 0.0f); // with red highlights

                    effect.DirectionalLight1.DiffuseColor = new Vector3(0.0f, 0.0f, 0.0f);
                    effect.DirectionalLight1.Direction = new Vector3(1.0f, .2f, 1.0f);
                    effect.DirectionalLight1.SpecularColor = new Vector3(0.5f, 0.0f, 0.0f); // with red highlights

                    effect.DirectionalLight2.DiffuseColor = new Vector3(0.0f, 0.0f, 0.0f);
                    effect.DirectionalLight2.Direction = new Vector3(1.0f, .2f, 1.0f);
                    effect.DirectionalLight2.SpecularColor = new Vector3(0.5f, 0.0f, 0.0f); // with red highlights



                }
                mesh.Draw();
            }
        }
    }
}