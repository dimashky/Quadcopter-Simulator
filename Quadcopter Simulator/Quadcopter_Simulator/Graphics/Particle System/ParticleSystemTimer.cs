using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace TripleM.Quadcopter.Graphics.Particle_System
{
    public class ParticleSystemTimer
    {
        public static long FireTime = 5500;
        Vector3 position;
        Stopwatch timer;
        public ParticleSystemTimer(Vector3 position)
        {
            this.position = position;
            timer = new Stopwatch();
            timer.Start();
        }
        public Vector3 getPosition()
        {
            return position;
        }
        public long getTime()
        {
            return timer.ElapsedMilliseconds;
        }
    }
}
