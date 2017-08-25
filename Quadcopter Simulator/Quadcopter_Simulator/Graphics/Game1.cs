using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TripleM.Quadcopter.Graphics.Particle_System;
using TripleM.Quadcopter.Physics;

namespace TripleM.Quadcopter.Graphics
{
    public struct VertexPositionNormalTexture
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;

        public static int SizeInBytes = 7 * 4;
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
              (
                  new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                  new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                  new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
              );
    }
    public struct VertexMultitextured
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector4 TextureCoordinate;
        public Vector4 TexWeights;

        public static int SizeInBytes = (3 + 3 + 4 + 4) * sizeof(float);
        public static VertexElement[] VertexElements = new VertexElement[]
 {
         new VertexElement(  0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
         new VertexElement(  sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0 ),
         new VertexElement(  sizeof(float) * 6, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0 ),
         new VertexElement(  sizeof(float) * 10, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1 ),
 };
    }
    public partial class Game1 : Microsoft.Xna.Framework.Game
    {
        #region innerstructs

        #endregion

        public static GraphicsDeviceManager graphics;
        public static GraphicsDevice device;
        public static Matrix reflectionViewMatrix;

        public static Effect effect;
        public static Effect bbEffect;

        public static QuadCopterModel quadCopterModel;
        public static Terrain terrain;
        public static Water water;
        public static Sky sky;
        public static Input input;

        public static SpriteBatch spriteBatch;
        public static SpriteFont font;

        Model flag;

        

        ParticleSystem ps;
        ParticleSystem smoke;

        Random r = new Random();

        // Returns a random Vector3 between min and max
        Vector3 randVec3(Vector3 min, Vector3 max)
        {
            return new Vector3(
                min.X + (float)r.NextDouble() * (max.X - min.X),
                min.Y + (float)r.NextDouble() * (max.Y - min.Y),
                min.Z + (float)r.NextDouble() * (max.Z - min.Z));
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        protected override void OnExiting(Object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            input.quadcopter.stopSimulation();
            for (int i = 0; i < Input.Projectiles.Count; ++i)
            {
                Input.Projectiles[i].stop_projectitle();
            }
        }
        protected override void Initialize()
        {
            IsMouseVisible = true;
            Window.AllowUserResizing = true;

            graphics.PreferredBackBufferWidth = 1244;
            graphics.PreferredBackBufferHeight = 700;

            graphics.ApplyChanges();
            Window.Title = "Quadcopter Simulator";
            base.Initialize();

        }
        protected override void LoadContent()
        {
            device = GraphicsDevice;

            flag = Content.Load<Model>("flag1");
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("myFont");
            effect = Content.Load<Effect>("effects");
            bbEffect = Content.Load<Effect>("bbEffect");
            quadCopterModel = new QuadCopterModel(this, Content.Load<Model>("drone"));
            sky = new Sky(Content.Load<Effect>("SkyEffect"), Content.Load<TextureCube>("SkyBoxTex"));
            terrain = new Terrain(Content.Load<Texture2D>("grass"), Content.Load<Texture2D>("sand"), Content.Load<Texture2D>("rock"), Content.Load<Texture2D>("snow"));

            ProjectileModel.model = Content.Load<Model>("red-ball");
            ProjectileModel.trajectoryModel = Content.Load<Model>("baseball");
            

            terrain.LoadHeightData(Content.Load<Texture2D>("TerrainHeightmap"));
            terrain.LoadCollisionData(Content.Load<Texture2D>("BigTerrainHeightmap"));
            input = new Input(this);
            water = new Water(Content.Load<Texture2D>("waterbump"));
            input.UpdateViewMatrix(water);
            Components.Add(quadCopterModel);

            VertexMultitextured[] terrainVertices = terrain.SetUpTerrainVertices();
            ushort[] terrainIndices = terrain.SetUpTerrainIndices();
            terrainVertices = terrain.CalculateNormals(terrainVertices, terrainIndices);
            terrain.CopyToTerrainBuffers(terrainVertices, terrainIndices);
            water.SetUpWaterVertices(this, terrain);

            ps = new ParticleSystem(GraphicsDevice, Content, Content.Load<Texture2D>("fire"),400, new Vector2(400), 1, Vector3.Zero, 0.5f);
            smoke = new ParticleSystem(GraphicsDevice, Content, Content.Load<Texture2D>("smoke"),400, new Vector2(800), 6, new Vector3(500, 0, 0), 5f);

        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            input.ProcessInput(this, water, terrain);

            // Generate a direction within 15 degrees of (0, 1, 0)
            Vector3 offset = new Vector3(MathHelper.ToRadians(10.0f));
            Vector3 randAngle = Vector3.Up + randVec3(-offset, offset);

            // Generate a position between (-400, 0, -400) and (400, 0, 400)
            Vector3 randPosition = randVec3(new Vector3(-400), new Vector3(400));

            // Generate a speed between 600 and 900
            float randSpeed = (float)r.NextDouble() * 300 + 600;

            ps.AddParticle(randPosition, randAngle, randSpeed);
            ps.Update();

            smoke.AddParticle(randPosition + new Vector3(0, 1200, 0), randAngle, randSpeed);
            smoke.Update();


            base.Update(gameTime);
        }

        private void DrawFlag()
        {
            Matrix[] Transform = new Matrix[flag.Bones.Count];
            flag.CopyAbsoluteBoneTransformsTo(Transform);

            foreach (ModelMesh mesh in flag.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.Projection = input.camera.Projection;
                    effect.View = input.camera.View;
                    effect.World = Transform[mesh.ParentBone.Index] * Matrix.CreateScale(20) * Matrix.CreateRotationY(input.flagRotation) * Matrix.CreateTranslation(new Vector3(223,35,-364));
                    if (mesh.Name.Equals("Mesh3"))
                    {
                        effect.LightingEnabled = true; // turn on the lighting subsystem.

                        effect.DirectionalLight0.DiffuseColor = new Vector3(1f, 1f, 1f);
                        effect.DirectionalLight0.Direction = new Vector3(1.0f, .1f, .1f);
                        effect.DirectionalLight0.SpecularColor = new Vector3(1, 1, 1); 
                    }

                }
                mesh.Draw();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            water.DrawRefractionMap(terrain);
            water.DrawReflectionMap(terrain, sky, this, input);

            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            sky.DrawSkyEffect(true);

            terrain.DrawTerrain(input.camera.View);

            water.DrawWater(time);

            DrawFlag();

            quadCopterModel.Draw(input.camera.View, input.camera.Projection, input.position, input.PhyisicalYaw, input.Pitch, input.Roll);

            foreach (Projectile projectile in Input.Projectiles)
                if (!projectile.startTimer)
                    projectile.model.Draw(input.camera.View, input.camera.Projection, projectile.get_position());

            for (int i = 0; i < Input.firePosition.Count; i++)
            {
                ParticleSystemTimer show = Input.firePosition.ElementAt(i);

                if (show.getTime() < ParticleSystemTimer.FireTime)
                {
                    ps.Draw(input.camera.View, input.camera.Projection, Input.firePosition.ElementAt(i).getPosition(),
                         Vector3.Transform(Vector3.Up, Matrix.CreateFromYawPitchRoll(input.Yaw, input.Pitch, input.Roll)), Vector3.Transform(Vector3.Right, Matrix.CreateFromYawPitchRoll(input.Yaw, input.Pitch, input.Roll)));

                    smoke.Draw(input.camera.View, input.camera.Projection, Input.firePosition.ElementAt(i).getPosition(),
                        Vector3.Transform(Vector3.Up, Matrix.CreateFromYawPitchRoll(input.Yaw, input.Pitch, input.Roll)), Vector3.Transform(Vector3.Right, Matrix.CreateFromYawPitchRoll(input.Yaw, input.Pitch, input.Roll)));
                }
                else
                {
                    Input.firePosition.RemoveAt(i);
                    i--;
                }
            }

            spriteBatch.Begin();
            input.drawInformation();
            spriteBatch.End();

            base.Draw(gameTime);
            
        }

    }

}
