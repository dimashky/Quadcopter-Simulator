using Quadcopter_Simulator;
using System;
using Microsoft.Xna.Framework;
using TripleM.Quadcopter.Graphics.Particle_System;
using System.Diagnostics;
using static TripleM.Quadcopter.Physics.config;

namespace TripleM.Quadcopter.Physics
{
    public class Projectile
    {
        private Vector3 position;           // Current projectile's position in Graphics frame
        private Vector3 velocity;           // Current projectile's velocity in Graphics frame
        private Vector3 firing_angle;       // Euler angles values of our quadcopter when fired the projectile
        private Vector3 spinning_speed;     // Spinning speed to show magnus effect

        private float mass;                 // The mass of projectile
        private float radius;               // The radius of projectile
        private float cross_sectional_area; // 'A' constant
        private float drag_coefficient;     // 'Cd' constant

        private float dt;                   // Timestep for numerical solution

        public Graphics.ProjectileModel model;           // To show projectile on our environment
        private bool simulation_running;    // To check if projectile is running or not
        public bool startTimer = false;     
        public static long trajectoryTime = 5000;
        Stopwatch timer;





        public Projectile(Vector3 initial_position, Vector3 initial_velocity, Vector3 firing_angles, Vector3 rotation_speed, bool trajectoryLock)
        {

            position = initial_position;
            velocity = initial_velocity;

            firing_angle = firing_angles;

            firing_angle.Y -= (float)Math.PI;

            
            velocity.Z += (float)(PROJECTILE_VELOCITY * Math.Cos(firing_angle.Z));
            velocity.Y += (float)(PROJECTILE_VELOCITY * Math.Sin(firing_angle.Z));

            mass = PROJECTILE_MASS;
            radius = PROJECTILE_DIAMETER;

            cross_sectional_area = (float)(Math.PI * Math.Pow(radius, 2));
            drag_coefficient = 0.5f;


            dt = PROJECTILE_TIMESTEP;


            spinning_speed = rotation_speed;

            model = new Graphics.ProjectileModel(initial_position, rotation_speed, trajectoryLock);


            simulation_running = true;
            timer = new Stopwatch();
        }

        public bool isRunning()
        {
            return simulation_running;
        }
        public bool stop_projectitle()
        {
            return true;
        }
        public Vector3 get_position()
        {
            return position;
        }
        public float get_theta()
        {
            return firing_angle.Z;
        }
        public long getTime()
        {
            return timer.ElapsedMilliseconds;
        }
        public float getYaw()
        {
            return firing_angle.Y;
        }
        public void start()
        {
            startTimer = true;
            timer.Start();
        }




        public void update()
        {
            float D = 0.5f * cross_sectional_area * drag_coefficient * DENSITY;  // Drag constant
            float L = 0.5f * 0.25f * DENSITY * cross_sectional_area;             // Magnus constant

            float v;

            // while projectile is running!
            if (((Graphics.Game1.input.InTerrain(position.X, position.Z) == 0) && (!Graphics.Game1.input.collisionDetection(position.X, position.Y, position.Z))))
            {
                // wind velocity vector 
                Vector3 wind = new Vector3( WIND_OFFSET_Y, WIND_OFFSET_Z, WIND_OFFSET_X) * 10f;
                
                // current velocity vector
                v = (float)Math.Sqrt(Math.Pow(velocity.X ,2) + Math.Pow(velocity.Y, 2) + Math.Pow(velocity.Z, 2));

                // for X axis
                float ax = -((D / mass) * v * (velocity.X)) + (((L * v * v) / mass) * (spinning_speed.X * (velocity.Z) - spinning_speed.Z * (velocity.X)));
                ax = constrainn(ax, -100f, +100f);
                velocity.X = velocity.X + ax * dt;

                // for Y axis
                float ay = -(GRAVITY) - (D / mass * v * (velocity.Y)) + (((L * v * v) / mass) * (spinning_speed.Y * (velocity.Z) - spinning_speed.X * (velocity.Y)));
                ay = constrainn(ay, -100f, +100f);
                velocity.Y = velocity.Y + ay * dt;

                // for Z axis
                float az = -(D / mass * v * (velocity.Z)) + (((L * v * v) / mass) * (spinning_speed.X * (velocity.Y) - spinning_speed.Y * (velocity.X)));
                az = constrainn(az, -100f, +100f);
                velocity.Z = velocity.Z + az * dt;


                position += SIMULATION_SPEED * (Vector3.Transform(new Vector3(velocity.X, velocity.Y, velocity.Z), Matrix.CreateRotationY(firing_angle.Y)) + wind) * dt;


                if (velocity.Z == 0)
                {
                    firing_angle.Z = (float)Math.Acos(v / velocity.Z);
                }
                else
                {
                    firing_angle.Z = (float)Math.PI;
                }
            }
            else
            {
                if ((Graphics.Game1.input.InTerrain(position.X, position.Z) == 0) && Graphics.Game1.input.collisionDetection(position.X, position.Y, position.Z))
                    if (Graphics.Game1.terrain.collisionMatrix[(int)position.X, -(int)position.Z] > 20.0f)
                        Graphics.Input.firePosition.Add(new ParticleSystemTimer(get_position()));
                simulation_running = false;
            }

        }
    }
}
