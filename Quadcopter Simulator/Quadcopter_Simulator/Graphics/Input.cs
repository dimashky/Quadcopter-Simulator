using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MathNet.Numerics.LinearAlgebra;
using TripleM.Quadcopter.Physics;
using TripleM.Quadcopter.Graphics.Particle_System;

namespace TripleM.Quadcopter.Graphics
{
    public class Input
    {
        public Physics.Quadcopter quadcopter;

        public static bool Restart;
        public float Yaw , Pitch = 0, Roll = 0, PhyisicalYaw;
        static float minZ, maxZ, minX, maxX, maxY;
        public float flagRotation = 0f;

        bool pause;

        public Vector<float> rpm;
        public Vector3 position, freeCameraPosition;
        KeyboardState key1, key2;
        private Vector<float> quadcopter_position, quadcopter_velocity , quadcopter_angles;
        public static List<Projectile> Projectiles;
        public static List<ParticleSystemTimer> firePosition;

        public Camera camera;
        float rotateCamera = 0, hieghtCamera = 0, zoomCamera = 0;

        bool fireLock = false;
        
        public Input(Game game)
        {
            minZ = -1024;
            maxZ = minX = 5f;
            maxX = 1024;
            maxY = 2000;
            Restart = false;
            pause = false;

            position = new Vector3(950, 45.199989f, -450);
            Yaw = 1.0f;
            config.Y_START = 950;//236;
            config.X_START = -490;
            config.Z_START = 45.199989f;

            rpm = Vector<float>.Build.Dense(4, 0);
            quadcopter_position = Vector<float>.Build.Dense(3, 0);
            quadcopter_velocity = Vector<float>.Build.Dense(3, 0);
            quadcopter_angles = Vector<float>.Build.Dense(3, 0);

            quadcopter = new Physics.Quadcopter();
            quadcopter.startSimulation();

            Projectiles = new List<Projectile>();
            firePosition = new List<ParticleSystemTimer>();

            camera = new Camera(game, Vector3.Zero, Vector3.Zero, Vector3.Up);

        }

        public int InTerrain(float x, float z)
        {
            try
            {
                if (x >= maxX)
                    return 1;
                if (x <= minX)
                    return 2;
                if (z >= maxZ)
                    return 3;
                if (z <= minZ)
                    return 4;

                return 0;
            }
            catch
            {
                return 0;
            }
        }
        public bool collisionDetection(float X, float Y, float Z)
        {
            try
            {
                if (Y < 25f)
                    return true;
                if (Y - Game1.terrain.collisionMatrix[(int)X, -(int)Z] > 0.0 && Y < maxY)
                    return false;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool restart()
        {
            config.setDragConst(1);
            config.SET_MASS(0.4f);
            config.set_max_tilt_angle(30.0f);
            config.SET_WIND_OFFSET(0, 0);
            pause = false;
            Restart = false;
            quadcopter.stopSimulation();
            quadcopter_position = Vector<float>.Build.Dense(3, 0);
            quadcopter_velocity = Vector<float>.Build.Dense(3, 0);
            quadcopter_angles = Vector<float>.Build.Dense(3, 0);
            position = new Vector3(236, 9.199989f, -105);
            Yaw = 1.0f;
            config.Y_START = 950;
            config.X_START = -490;
            config.Z_START = 45.199989f;
            config.YAW_START = 90;
            config.SIMULATION_SPEED = 1;
            Pitch = 0; Roll = 0;
            quadcopter = new Physics.Quadcopter();
            quadcopter.startSimulation();
            config.flight_mode = stable_flight_mode.STABILIZE_HEIGHT;
            return true;
        }

        public bool resume()
        {
            quadcopter.resumeSimulation();
            pause = false;
            return true;
        }

        public void drawInformation()
        {
            rpm = quadcopter.get_motor_rpms();
            Game1.spriteBatch.DrawString(Game1.font, "Yaw    : " + MathHelper.ToDegrees(PhyisicalYaw) % 360, new Vector2(10, 10), Color.Red);
            Game1.spriteBatch.DrawString(Game1.font, "Pitch  : " + MathHelper.ToDegrees(Pitch), new Vector2(10, 30), Color.Red);
            Game1.spriteBatch.DrawString(Game1.font, "Roll   : " + MathHelper.ToDegrees(Roll), new Vector2(10, 50), Color.Red);
            Game1.spriteBatch.DrawString(Game1.font, "----------------", new Vector2(10, 70), Color.Red);
            Game1.spriteBatch.DrawString(Game1.font, "X      : " + position.X, new Vector2(10, 90), Color.Red);
            Game1.spriteBatch.DrawString(Game1.font, "Y      : " + position.Y, new Vector2(10, 110), Color.Red);
            Game1.spriteBatch.DrawString(Game1.font, "Z      : " + position.Z, new Vector2(10, 130), Color.Red);
            Game1.spriteBatch.DrawString(Game1.font, "----------------", new Vector2(10, 150), Color.Red);
            Game1.spriteBatch.DrawString(Game1.font, "RPM1   : " + rpm[0], new Vector2(10, 170), Color.Red);
            Game1.spriteBatch.DrawString(Game1.font, "RPM2   : " + rpm[1], new Vector2(10, 190), Color.Red);
            Game1.spriteBatch.DrawString(Game1.font, "RPM3   : " + rpm[2], new Vector2(10, 210), Color.Red);
            Game1.spriteBatch.DrawString(Game1.font, "RPM4   : " + rpm[3], new Vector2(10, 230), Color.Red);
            Game1.spriteBatch.DrawString(Game1.font, "----------------", new Vector2(10, 250), Color.Red);
            Game1.spriteBatch.DrawString(Game1.font, "Density: " + config.DENSITY, new Vector2(10, 270), Color.Red);
       }

        private void updateThirdPersonCamera()
        {
            if (key1.IsKeyDown(Keys.NumPad6))
                rotateCamera += 0.01f;

            if (key1.IsKeyDown(Keys.NumPad4))
                rotateCamera -= 0.01f;

            if (key1.IsKeyDown(Keys.NumPad8))
                hieghtCamera += 0.01f;

            if (key1.IsKeyDown(Keys.NumPad2))
                hieghtCamera -= 0.01f;

            if (key1.IsKeyDown(Keys.OemPlus))
                zoomCamera -= 0.01f;

            if (key1.IsKeyDown(Keys.OemMinus))
                zoomCamera += 0.01f;
        }
        public void ProcessInput(Game1 game, Water water, Terrain terrain)
        {
            key2 = key1;

            key1 = Keyboard.GetState();

            if (key1.IsKeyDown(Keys.R) && !key2.IsKeyDown(Keys.R))
                rotateCamera = zoomCamera = hieghtCamera = 0;


            if (Keyboard.GetState().IsKeyDown(Keys.Space) || Restart)
                restart();

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                if (!pause)
                {
                    quadcopter.pauseSimulation();
                    pause = true;
                    Quadcopter_Simulator.Forms.pausing_form pause_form = new Quadcopter_Simulator.Forms.pausing_form();
                    pause_form.Visible = true;
                }

            if(key1.IsKeyDown(Keys.F) && key2.IsKeyUp(Keys.F) && key1.IsKeyDown(Keys.LeftControl))
            {
                config.FIRING = true;
                float[] tmp = { quadcopter_velocity[0], (float)(quadcopter_velocity[1] + config.PROJECTILE_VELOCITY * Math.Cos(Pitch)), (float)(quadcopter_velocity[2] + config.PROJECTILE_VELOCITY * Math.Sin(Pitch)) };
                config.PROJ_VELOCITY = Vector<float>.Build.DenseOfArray(tmp);
                config.RECOIL_VELOCITY = -config.PROJECTILE_MASS / config.MASS * config.PROJ_VELOCITY;

                Vector3 tmpVec = new Vector3(config.RECOIL_VELOCITY[0], config.RECOIL_VELOCITY[1], config.RECOIL_VELOCITY[2]);
                tmpVec = Vector3.Transform(tmpVec, Matrix.CreateRotationZ(PhyisicalYaw));
                config.RECOIL_VELOCITY[0] = tmpVec.X;
                config.RECOIL_VELOCITY[1] = tmpVec.Y;
                config.RECOIL_VELOCITY[2] = tmpVec.Z;


                Projectiles.Add(new Projectile(position, new Vector3(quadcopter_velocity[1], quadcopter_velocity[2], quadcopter_velocity[0]), new Vector3(Roll, PhyisicalYaw, Pitch), new Vector3(config.ROTATION_SPEED[0], config.ROTATION_SPEED[1], config.ROTATION_SPEED[2]), true));
                fireLock = true;
            }
            else if (key1.IsKeyDown(Keys.F) && key2.IsKeyUp(Keys.F) && key1.IsKeyDown(Keys.LeftAlt) && !fireLock)
            {
                config.FIRING = true;
                float[] tmp = { quadcopter_velocity[0], (float)(quadcopter_velocity[1] + config.PROJECTILE_VELOCITY * Math.Cos(Pitch)), (float)(quadcopter_velocity[2] + config.PROJECTILE_VELOCITY * Math.Sin(Pitch)) };
                config.PROJ_VELOCITY = Vector<float>.Build.DenseOfArray(tmp);
                config.RECOIL_VELOCITY = -(config.PROJECTILE_MASS / config.MASS) * config.PROJ_VELOCITY;


                Vector3 tmpVec = new Vector3(config.RECOIL_VELOCITY[0], config.RECOIL_VELOCITY[1], config.RECOIL_VELOCITY[2]);
                tmpVec = Vector3.Transform(tmpVec, Matrix.CreateRotationZ(PhyisicalYaw));
                config.RECOIL_VELOCITY[0] = tmpVec.X;
                config.RECOIL_VELOCITY[1] = tmpVec.Y;
                config.RECOIL_VELOCITY[2] = tmpVec.Z;


                Projectiles.Add(new Projectile(position, new Vector3(quadcopter_velocity[1], quadcopter_velocity[2], quadcopter_velocity[0]), new Vector3(Roll, PhyisicalYaw, Pitch), new Vector3(config.ROTATION_SPEED[0], config.ROTATION_SPEED[1], config.ROTATION_SPEED[2]), false));
            }
            else if (key1.IsKeyDown(Keys.F) && key2.IsKeyUp(Keys.F) && !fireLock)
            {
                config.FIRING = true;
                float[] tmp = { quadcopter_velocity[0], (float)(quadcopter_velocity[1] + config.PROJECTILE_VELOCITY * Math.Cos(Pitch)), (float)(quadcopter_velocity[2] + config.PROJECTILE_VELOCITY * Math.Sin(Pitch)) };
                config.PROJ_VELOCITY = Vector<float>.Build.DenseOfArray(tmp);
                config.RECOIL_VELOCITY = -config.PROJECTILE_MASS / config.MASS * config.PROJ_VELOCITY;

                Vector3 tmpVec = new Vector3(config.RECOIL_VELOCITY[0], config.RECOIL_VELOCITY[1], config.RECOIL_VELOCITY[2]);
                tmpVec = Vector3.Transform(tmpVec, Matrix.CreateRotationZ(PhyisicalYaw));
                config.RECOIL_VELOCITY[0] = tmpVec.X;
                config.RECOIL_VELOCITY[1] = tmpVec.Y;
                config.RECOIL_VELOCITY[2] = tmpVec.Z;


                Projectiles.Add(new Projectile(position, new Vector3(quadcopter_velocity[1], quadcopter_velocity[2], quadcopter_velocity[0]), new Vector3(Roll, PhyisicalYaw, Pitch), new Vector3(config.ROTATION_SPEED[0], config.ROTATION_SPEED[1], config.ROTATION_SPEED[2]), true));
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Z))
            {
                config.POWER_OFF = true;
            }
            else
            {
                config.POWER_OFF = false;
            }

            if (config.RESTART)
            {
                config.RESTART = false;
                restart();
            }

            quadcopter_angles = quadcopter.get_attitude();
            PhyisicalYaw = quadcopter_angles[2];
            Pitch = quadcopter_angles[1];
            Roll = quadcopter_angles[0];

            quadcopter_position = quadcopter.get_position_vector();
            position.Z = quadcopter_position[0];
            position.Y = quadcopter_position[2];
            position.X = quadcopter_position[1];

            quadcopter_velocity = quadcopter.get_velocity_vector();
            Game1.quadCopterModel.PladesRotation(quadcopter.get_motor_rpms());
            flagRotation = MathHelper.ToRadians((config.WIND_DEGREE + 180) % 360);

            for (int i = 0; i < Projectiles.Count; i++)
            {
                if (!Projectiles.ElementAt(i).isRunning())
                {
                    if (!Projectiles.ElementAt(i).startTimer)
                        Projectiles.ElementAt(i).start();
                    else if (Projectiles.ElementAt(i).getTime() >= Projectile.trajectoryTime)
                    {
                        Projectiles.RemoveAt(i);
                        i--;
                    }
                }
                else
                    Projectiles.ElementAt(i).update();
            }

            Yaw = PhyisicalYaw + rotateCamera;

            if (!fireLock)
                updateThirdPersonCamera();
            if (Projectiles.Count.Equals(0) || Projectiles.Count.Equals(1) && Projectiles.ElementAt(0).startTimer)
                fireLock = false;

            UpdateViewMatrix(water);
        }

        public void UpdateViewMatrix(Water water)
        {
            Matrix cameraRotation = Matrix.CreateRotationY(Yaw);


            if (fireLock && !Projectiles.ElementAt(Projectiles.Count - 1).startTimer)
                camera.updateTarget(Projectiles.ElementAt(Projectiles.Count - 1).get_position(), Projectiles.ElementAt(Projectiles.Count - 1).getYaw() + MathHelper.Pi, 0, 0);
            else camera.updateTarget(position, Yaw, hieghtCamera, zoomCamera);


            Vector3 reflCameraPosition = camera.getCameraPosition();
            reflCameraPosition.Y = -camera.getCameraPosition().Y + water.waterHeight * 2;
            Vector3 reflTargetPos = position;
            reflTargetPos.Y = -position.Y + water.waterHeight * 2;

            Vector3 cameraRight = Vector3.Transform(new Vector3(1, 0, 0), cameraRotation);
            Vector3 invUpVector = Vector3.Cross(cameraRight, reflTargetPos - reflCameraPosition);

            Game1.reflectionViewMatrix = Matrix.CreateLookAt(reflCameraPosition, reflTargetPos, invUpVector);
        }
    }
}
