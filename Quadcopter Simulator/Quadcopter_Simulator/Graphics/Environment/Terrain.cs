using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quadcopter_Simulator.Forms;

namespace TripleM.Quadcopter.Graphics
{

    public class Terrain
    {
        public ushort terrainWidth;
        public ushort terrainLength;
        public float[,] heightData, collisionMatrix;
        public ushort[] indices;

        public Texture2D grassTexture;
        public Texture2D sandTexture;
        public Texture2D rockTexture;
        public Texture2D snowTexture;

        public static VertexMultitextured[] vertices;

        public VertexBuffer terrainVertexBuffer;
        public IndexBuffer terrainIndexBuffer;

        public Terrain(Texture2D grass, Texture2D sand, Texture2D rock, Texture2D snow)
        {
            grassTexture = grass;
            sandTexture = sand;
            rockTexture = rock;
            snowTexture = snow;
            if (environment_select_form.selectedEnv)
                grassTexture = sandTexture = rockTexture = snowTexture = grass;

        }
        public VertexMultitextured[] SetUpTerrainVertices()
        {
            VertexMultitextured[] terrainVertices = new VertexMultitextured[terrainWidth * terrainLength];
            vertices = new VertexMultitextured[terrainWidth * terrainLength];

            for (int x = 0; x < terrainWidth; x++)
            {
                for (int y = 0; y < terrainLength; y++)
                {
                    terrainVertices[x + y * terrainWidth].Position = new Vector3(x, heightData[x, y], -y);
                    terrainVertices[x + y * terrainWidth].TextureCoordinate.X = (float)x / 30.0f;
                    terrainVertices[x + y * terrainWidth].TextureCoordinate.Y = (float)y / 30.0f;

                    terrainVertices[x + y * terrainWidth].TexWeights.X = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 0) / 8.0f, 0, 1);
                    terrainVertices[x + y * terrainWidth].TexWeights.Y = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 12) / 6.0f, 0, 1);
                    terrainVertices[x + y * terrainWidth].TexWeights.Z = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 20) / 6.0f, 0, 1);
                    terrainVertices[x + y * terrainWidth].TexWeights.W = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 30) / 6.0f, 0, 1);


                    float total = terrainVertices[x + y * terrainWidth].TexWeights.X;
                    total += terrainVertices[x + y * terrainWidth].TexWeights.Y;
                    total += terrainVertices[x + y * terrainWidth].TexWeights.Z;
                    total += terrainVertices[x + y * terrainWidth].TexWeights.W;

                    terrainVertices[x + y * terrainWidth].TexWeights.X /= total;
                    terrainVertices[x + y * terrainWidth].TexWeights.Y /= total;
                    terrainVertices[x + y * terrainWidth].TexWeights.Z /= total;
                    terrainVertices[x + y * terrainWidth].TexWeights.W /= total;
                }
            }
            return terrainVertices;
        }
        public ushort[] SetUpTerrainIndices()
        {
            int counter = 0;
            indices = new ushort[(terrainWidth - 1) * (terrainLength - 1) * 6];

            for (int y = 0; y < terrainLength - 1; y++)
            {
                for (int x = 0; x < terrainWidth - 1; x++)
                {
                    int lowerLeft = x + y * terrainWidth;
                    int lowerRight = (x + 1) + y * terrainWidth;
                    int topLeft = x + (y + 1) * terrainWidth;
                    int topRight = (x + 1) + (y + 1) * terrainWidth;

                    indices[counter++] = (ushort)topLeft;
                    indices[counter++] = (ushort)lowerRight;
                    indices[counter++] = (ushort)lowerLeft;

                    indices[counter++] = (ushort)topLeft;
                    indices[counter++] = (ushort)topRight;
                    indices[counter++] = (ushort)lowerRight;
                }
            }

            return indices;
        }
        public void CopyToTerrainBuffers(VertexMultitextured[] vertices, ushort[] indices)
        {

            VertexDeclaration vertexDeclaration = new VertexDeclaration(VertexMultitextured.VertexElements);

            terrainVertexBuffer = new VertexBuffer(Game1.device, vertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            terrainVertexBuffer.SetData(vertices.ToArray());

            terrainIndexBuffer = new IndexBuffer(Game1.device, typeof(ushort), indices.Length, BufferUsage.WriteOnly);
            terrainIndexBuffer.SetData(indices);
        }


        public void LoadCollisionData(Texture2D heightMap)
        {
            float minimumHeight = float.MaxValue;
            float maximumHeight = float.MinValue;

            ushort terrainWidth = (ushort)heightMap.Width;
            ushort terrainLength = (ushort)heightMap.Height;

            Color[] heightMapColors = new Color[terrainWidth * terrainLength];
            heightMap.GetData(heightMapColors);

            collisionMatrix = new float[terrainWidth, terrainLength];
            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainLength; y++)
                {
                    collisionMatrix[x, y] = heightMapColors[x + y * terrainWidth].R;
                    if (collisionMatrix[x, y] < minimumHeight) minimumHeight = collisionMatrix[x, y];
                    if (collisionMatrix[x, y] > maximumHeight) maximumHeight = collisionMatrix[x, y];
                }
            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainLength; y++)
                    collisionMatrix[x, y] = (collisionMatrix[x, y] - minimumHeight) / (maximumHeight - minimumHeight) * 120.0f;
        }
        public void LoadHeightData(Texture2D heightMap)
        {
            float minimumHeight = float.MaxValue;
            float maximumHeight = float.MinValue;

            terrainWidth = (ushort)heightMap.Width;
            terrainLength = (ushort)heightMap.Height;

            Color[] heightMapColors = new Color[terrainWidth * terrainLength];
            heightMap.GetData(heightMapColors);

            heightData = new float[terrainWidth, terrainLength];
            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainLength; y++)
                {
                    heightData[x, y] = heightMapColors[x + y * terrainWidth].R;
                    if (heightData[x, y] < minimumHeight) minimumHeight = heightData[x, y];
                    if (heightData[x, y] > maximumHeight) maximumHeight = heightData[x, y];
                }

            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainLength; y++)
                    heightData[x, y] = (heightData[x, y] - minimumHeight) / (maximumHeight - minimumHeight) * 30.0f;
        }
        public VertexMultitextured[] CalculateNormals(VertexMultitextured[] vertices, ushort[] indices)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = new Vector3(0, 0, 0);

            for (int i = 0; i < indices.Length / 3; i++)
            {
                int index1 = indices[i * 3];
                int index2 = indices[i * 3 + 1];
                int index3 = indices[i * 3 + 2];

                Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
                Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
                Vector3 normal = Vector3.Cross(side1, side2);

                vertices[index1].Normal += normal;
                vertices[index2].Normal += normal;
                vertices[index3].Normal += normal;
            }

            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal.Normalize();

            return vertices;
        }
        public void DrawTerrain(Matrix currentViewMatrix)
        {
            Game1.effect.CurrentTechnique = Game1.effect.Techniques["MultiTextured"];
            Game1.effect.Parameters["xTexture0"].SetValue(sandTexture);
            Game1.effect.Parameters["xTexture1"].SetValue(grassTexture);
            Game1.effect.Parameters["xTexture2"].SetValue(rockTexture);
            Game1.effect.Parameters["xTexture3"].SetValue(snowTexture);

            Matrix worldMatrix = Matrix.Identity;
            Game1.effect.Parameters["xWorld"].SetValue(worldMatrix * Matrix.CreateScale(4f, 4, 4f));
            Game1.effect.Parameters["xView"].SetValue(currentViewMatrix);
            Game1.effect.Parameters["xProjection"].SetValue(Game1.input.camera.Projection);

            Game1.effect.Parameters["xEnableLighting"].SetValue(true);
            Game1.effect.Parameters["xAmbient"].SetValue(0.4f);
            Game1.effect.Parameters["xLightDirection"].SetValue(new Vector3(1.0f, -0.03f, 1.0f));//1.5 1 1.5
            foreach (EffectPass pass in Game1.effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                Game1.device.Indices = terrainIndexBuffer;
                Game1.device.SetVertexBuffer(terrainVertexBuffer);

                Game1.device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Length, 0, indices.Length / 3);

            }
        }
    }
}
