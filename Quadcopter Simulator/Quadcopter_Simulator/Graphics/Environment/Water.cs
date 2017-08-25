using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TripleM.Quadcopter.Graphics
{
    public class Water
    {
        Texture2D refractionMap;
        Texture2D reflectionMap;
        RenderTarget2D reflectionRenderTarget;
        RenderTarget2D refractionRenderTarget;
        VertexBuffer waterVertexBuffer;
        Vector3 windDirection;

        Texture2D waterBumpMap;
        public float waterHeight;



        public Water(Texture2D water)
        {
            PresentationParameters pp = Game1.device.PresentationParameters;
            refractionRenderTarget = new RenderTarget2D(Game1.device, pp.BackBufferWidth, pp.BackBufferHeight, false, pp.BackBufferFormat, pp.DepthStencilFormat);
            reflectionRenderTarget = new RenderTarget2D(Game1.device, pp.BackBufferWidth, pp.BackBufferHeight, false, pp.BackBufferFormat, pp.DepthStencilFormat);
            waterBumpMap = water;
            waterHeight = 25f;
            windDirection = Vector3.Transform(new Vector3(1, 0, 0), Matrix.CreateRotationY(Game1.input.flagRotation - 3.1415f / 2));
        }
        public void SetUpWaterVertices(Game1 game, Terrain terrain)
        {
            VertexPositionTexture[] waterVertices = new VertexPositionTexture[6];

            waterVertices[0] = new VertexPositionTexture(new Vector3(0, waterHeight, 0), new Vector2(0, 1));
            waterVertices[2] = new VertexPositionTexture(new Vector3(terrain.terrainWidth * 4, waterHeight, -terrain.terrainLength * 4), new Vector2(1, 0));
            waterVertices[1] = new VertexPositionTexture(new Vector3(0, waterHeight, -terrain.terrainLength * 4), new Vector2(0, 0));

            waterVertices[3] = new VertexPositionTexture(new Vector3(0, waterHeight, 0), new Vector2(0, 1));
            waterVertices[5] = new VertexPositionTexture(new Vector3(terrain.terrainWidth * 4, waterHeight, 0), new Vector2(1, 1));
            waterVertices[4] = new VertexPositionTexture(new Vector3(terrain.terrainWidth * 4, waterHeight, -terrain.terrainLength * 4), new Vector2(1, 0));

            waterVertexBuffer = new VertexBuffer(game.GraphicsDevice, VertexPositionTexture.VertexDeclaration, waterVertices.Count(), BufferUsage.WriteOnly);
            waterVertexBuffer.SetData(waterVertices);
        }
        public Plane CreatePlane(float height, Vector3 planeNormalDirection, Matrix currentViewMatrix, bool clipSide)
        {
            planeNormalDirection.Normalize();
            Vector4 planeCoeffs = new Vector4(planeNormalDirection, height);
            if (clipSide) planeCoeffs *= -1;
            Plane finalPlane = new Plane(planeCoeffs);
            return finalPlane;
        }
        
        public void DrawRefractionMap(Terrain terrain)
        {
            Plane refractionPlane = CreatePlane(waterHeight + 1.5f, new Vector3(0, -1, 0), Game1.input.camera.View, false);

            Game1.effect.Parameters["ClipPlane0"].SetValue(new Vector4(refractionPlane.Normal, refractionPlane.D));
            Game1.effect.Parameters["Clipping"].SetValue(true);    // Allows the geometry to be clipped for the purpose of creating a refraction map
            Game1.device.SetRenderTarget(refractionRenderTarget);
            Game1.device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            terrain.DrawTerrain(Game1.input.camera.View);
            Game1.device.SetRenderTarget(null);
            Game1.effect.Parameters["Clipping"].SetValue(false);   // Make sure you turn it back off so the whole scene doesnt keep rendering as clipped
            refractionMap = refractionRenderTarget;

        }
      
        public void DrawReflectionMap(Terrain terrain, Sky sky, Game1 game, Input input)
        {
            Plane reflectionPlane = CreatePlane(waterHeight - 0.5f, new Vector3(0, -1, 0), Game1.reflectionViewMatrix, true);

            Game1.effect.Parameters["ClipPlane0"].SetValue(new Vector4(reflectionPlane.Normal, reflectionPlane.D));

            Game1.effect.Parameters["Clipping"].SetValue(true);    // Allows the geometry to be clipped for the purpose of creating a refraction map
            Game1.device.SetRenderTarget(reflectionRenderTarget);


            Game1.device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            sky.DrawSkyEffect(false);
            terrain.DrawTerrain(Game1.reflectionViewMatrix);
            // second part

            Game1.effect.Parameters["Clipping"].SetValue(false);

            Game1.device.SetRenderTarget(null);

            reflectionMap = reflectionRenderTarget;
        }
        

        public void DrawWater(float time)
        {
            Game1.effect.CurrentTechnique = Game1.effect.Techniques["Water"];
            Matrix worldMatrix = Matrix.Identity;
            Game1.effect.Parameters["xWorld"].SetValue(worldMatrix);
            Game1.effect.Parameters["xView"].SetValue(Game1.input.camera.View);
            Game1.effect.Parameters["xReflectionView"].SetValue(Game1.reflectionViewMatrix);
            Game1.effect.Parameters["xProjection"].SetValue(Game1.input.camera.Projection);
            Game1.effect.Parameters["xReflectionMap"].SetValue(reflectionMap);
            Game1.effect.Parameters["xRefractionMap"].SetValue(refractionMap);
            Game1.effect.Parameters["xWaterBumpMap"].SetValue(waterBumpMap);
            Game1.effect.Parameters["xWaterBumpMap"].SetValue(waterBumpMap);
            Game1.effect.Parameters["xWaveLength"].SetValue(0.1f);
            Game1.effect.Parameters["xWaveHeight"].SetValue(0.3f);
            Game1.effect.Parameters["xTime"].SetValue(time);
            Game1.effect.Parameters["xWindForce"].SetValue(0.002f);
            Game1.effect.Parameters["xWindDirection"].SetValue(windDirection);


            Game1.effect.CurrentTechnique.Passes[0].Apply();


            Game1.device.SetVertexBuffer(waterVertexBuffer);

            Game1.device.DrawPrimitives(PrimitiveType.TriangleList, 0, waterVertexBuffer.VertexCount / 3);
            windDirection = Vector3.Transform(new Vector3(1, 0, 0), Matrix.CreateRotationY(Game1.input.flagRotation - 3.1415f / 2));
        }
    }
}
