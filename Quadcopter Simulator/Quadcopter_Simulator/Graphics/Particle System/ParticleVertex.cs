using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TripleM.Quadcopter.Graphics.Particle_System
{
    struct ParticleVertex : IVertexType
    {
        Vector3 startPosition;
        Vector2 uv;
        Vector3 direction;
        float speed;
        float startTime;

        // Starting position of that particle (t = 0)
        public Vector3 StartPosition
        {
            get { return startPosition; }
            set { startPosition = value; }
        }

        // UV coordinate, used for texturing and to offset vertex in shader
        public Vector2 UV
        {
            get { return uv; }
            set { uv = value; }
        }

        // Movement direction of the particle
        public Vector3 Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        // Speed of the particle in units/second
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        // The time since the particle system was created that this
        // particle came into use
        public float StartTime
        {
            get { return startTime; }
            set { startTime = value; }
        }

        public ParticleVertex(Vector3 StartPosition, Vector2 UV, Vector3 Direction,
            float Speed, float StartTime)
        {
            this.startPosition = StartPosition;
            this.uv = UV;
            this.direction = Direction;
            this.speed = Speed;
            this.startTime = StartTime;
        }

        // Vertex declaration
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, // Start position
                VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector2, // UV coordinates
                VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(20, VertexElementFormat.Vector3, // Movement direction
                VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(32, VertexElementFormat.Single, // Movement speed
                VertexElementUsage.TextureCoordinate, 2),
            new VertexElement(36, VertexElementFormat.Single, // Start time
                VertexElementUsage.TextureCoordinate, 3)
        );

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    }
}
