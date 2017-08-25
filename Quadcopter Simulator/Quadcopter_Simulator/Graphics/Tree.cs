using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Quadcopter_Simulator.Forms;
using MathNet.Numerics.LinearAlgebra;
using Quadcopter_Simulator.Graphics.Particle_System;
using System.Diagnostics;
namespace Quadcopter_Simulator
{
    class Tree : Microsoft.Xna.Framework.GameComponent
    {
        private Vector3 position;

        public Tree(Game1 game, Vector3 position) : base(game)
        {
            this.position = position;
        }

        public Vector3 getPosition()
        {
            return position;
        }

        public void Draw(Matrix View,Matrix Projection)
        {
            Matrix[] Transform = new Matrix[treeModel.Bones.Count];
            treeModel.CopyAbsoluteBoneTransformsTo(Transform);

            foreach (ModelMesh mesh in treeModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.Projection = Projection;
                    effect.View = View;
                    effect.World = Transform[mesh.ParentBone.Index] * Matrix.CreateScale(0.007f) * Matrix.CreateTranslation(position);
                    
                    effect.LightingEnabled = true; // turn on the lighting subsystem.

                    effect.DirectionalLight0.DiffuseColor = new Vector3(0.5f, 0.5f, 0); // a red light
                    effect.DirectionalLight0.Direction = new Vector3(1.0f, -.01f, .8f);
                    effect.DirectionalLight0.SpecularColor = new Vector3(0, 1, 0); // with green highlights
                }
                mesh.Draw();
            }
        }

    }
}
