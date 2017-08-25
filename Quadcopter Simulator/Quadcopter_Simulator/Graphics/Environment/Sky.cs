using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace TripleM.Quadcopter.Graphics
{
    public class Sky
    {

        Effect skyEffect;

        Matrix View, Projection;

        Vector3 originalView = new Vector3(0, 0, 10);
        Vector3 position = Vector3.Zero;

        IndexBuffer indices;
        VertexBuffer vertices;

        public Sky(Effect skyEffect ,TextureCube skyTexture)
        {
            this.skyEffect = skyEffect;

            skyEffect.Parameters["tex"].SetValue(skyTexture);

            View = Matrix.CreateLookAt(position, originalView, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Game1.device.Viewport.AspectRatio, 1, 20);

            CreateCubeVertexBuffer();
            CreateCubeIndexBuffer();

        }



        void CreateCubeVertexBuffer()
        {
            Vector3[] cubeVertices = new Vector3[8];

            cubeVertices[0] = new Vector3(-1, -1, -1);
            cubeVertices[1] = new Vector3(-1, -1, 1);
            cubeVertices[2] = new Vector3(1, -1, 1);
            cubeVertices[3] = new Vector3(1, -1, -1);
            cubeVertices[4] = new Vector3(-1, 1, -1);
            cubeVertices[5] = new Vector3(-1, 1, 1);
            cubeVertices[6] = new Vector3(1, 1, 1);
            cubeVertices[7] = new Vector3(1, 1, -1);

            VertexDeclaration VertexPositionDeclaration = new VertexDeclaration
                (
                    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0)
                );

            vertices = new VertexBuffer(Game1.device, VertexPositionDeclaration, 8, BufferUsage.WriteOnly);
            vertices.SetData<Vector3>(cubeVertices);
        }
        
        
        
        void CreateCubeIndexBuffer()
        {
            UInt16[] cubeIndices = new UInt16[36];

            //bottom face
            cubeIndices[0] = 0;
            cubeIndices[1] = 2;
            cubeIndices[2] = 3;
            cubeIndices[3] = 0;
            cubeIndices[4] = 1;
            cubeIndices[5] = 2;

            //top face
            cubeIndices[6] = 4;
            cubeIndices[7] = 6;
            cubeIndices[8] = 5;
            cubeIndices[9] = 4;
            cubeIndices[10] = 7;
            cubeIndices[11] = 6;

            //front face
            cubeIndices[12] = 5;
            cubeIndices[13] = 2;
            cubeIndices[14] = 1;
            cubeIndices[15] = 5;
            cubeIndices[16] = 6;
            cubeIndices[17] = 2;

            //back face
            cubeIndices[18] = 0;
            cubeIndices[19] = 7;
            cubeIndices[20] = 4;
            cubeIndices[21] = 0;
            cubeIndices[22] = 3;
            cubeIndices[23] = 7;

            //left face
            cubeIndices[24] = 0;
            cubeIndices[25] = 4;
            cubeIndices[26] = 1;
            cubeIndices[27] = 1;
            cubeIndices[28] = 4;
            cubeIndices[29] = 5;

            //right face
            cubeIndices[30] = 2;
            cubeIndices[31] = 6;
            cubeIndices[32] = 3;
            cubeIndices[33] = 3;
            cubeIndices[34] = 6;
            cubeIndices[35] = 7;

            indices = new IndexBuffer(Game1.device, IndexElementSize.SixteenBits, 36, BufferUsage.WriteOnly);
            indices.SetData<UInt16>(cubeIndices);

        }

        void UpdateView(int dir)
        {
            Vector3 tempView = Vector3.Transform(originalView, Matrix.CreateRotationX(0.0f));
            tempView = Vector3.Transform(tempView, Matrix.CreateRotationY(Game1.input.Yaw));
            Vector3 tempUp = Vector3.Transform(Vector3.Up, Matrix.CreateRotationX(0.0f));
            tempUp = Vector3.Transform(tempUp, Matrix.CreateRotationY(Game1.input.Yaw));
            position += dir * Vector3.Normalize(tempView) / 10;
            View = Matrix.CreateLookAt(Vector3.Zero + position, tempView + position, tempUp);
        }

        public void DrawSkyEffect(bool Sky)
        {
            Game1.device.Clear(Color.CornflowerBlue);

            Game1.device.SetVertexBuffer(vertices);
            Game1.device.Indices = indices;
            if (!Sky)
            {
                UpdateView(0);
                skyEffect.Parameters["WVP"].SetValue(Matrix.Identity * View * Projection);
            }
            else
                    skyEffect.Parameters["WVP"].SetValue(Matrix.Identity * Matrix.CreateScale(1000 * 4) * Game1.input.camera.View * Game1.input.camera.Projection);
                skyEffect.CurrentTechnique.Passes[0].Apply();

            Game1.device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 8, 0, 36 / 3);
        }
    }
}
