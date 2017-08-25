using MathNet.Numerics.LinearAlgebra;
using static TripleM.Quadcopter.Physics.config;
using System.Collections.Generic;
using static System.Math;

namespace TripleM.Quadcopter.Physics
{
    public class trajectory_tracking
    {
        private Queue<Vector<float>> trajectory_points;
        private Vector<float> current_point, target_point;

        public trajectory_tracking(Queue<Vector<float>> trajectory_points, Vector<float> current_position)
        {
            this.trajectory_points = trajectory_points;
            if (trajectory_points.Count > 0)
            {
                current_point = current_position.Clone();
                target_point = this.trajectory_points.Dequeue();
            }
        }
        public bool claculation(ref float theta, ref float phi, float yaw, Vector<float> current_position)
        {
            if (InRange(current_position[0], target_point[0] - 3, target_point[0] + 3) && InRange(current_position[1], target_point[1] - 3, target_point[1] + 3))
            {
                System.Windows.Forms.MessageBox.Show("New point reached!");
                current_point = target_point;
                if (trajectory_points.Count != 0)
                {
                    target_point = this.trajectory_points.Dequeue();
                }
                else
                {
                    return true;
                }
            }
            Vector<float> distance = Vector<float>.Build.Dense(3, 0);
            Vector<float> time = Vector<float>.Build.Dense(3, 0);

            for (int i = 0; i < 3; ++i)
            {
                distance[i] = (target_point[i] - current_position[i]);
                time[i] = (int)Ceiling(Abs(distance[i] / MOVMENT_SPEED));
            }
            Vector<float> a = Vector<float>.Build.Dense(3, 0);
            Vector<float> v = Vector<float>.Build.Dense(3, 0);
            for (int i = 0; i < 3; ++i)
            {
                if (time[i] == 0)
                {
                    continue;
                }

                a[i] = 2 * (distance[i] / time[i]) / time[i];
                v[i] = 2 * (distance[i] / time[i]);
            }
            float dx = a[0] + (0.2f * v[0]) / MASS;
            float dy = a[1] + (0.2f * v[1]) / MASS;
            float dz = a[2] + (0.2f * v[2]) / MASS;

            phi = (float)Asin((dx * Sin(yaw) - dy * Cos(yaw)) / (dx * dx + dy * dy + (dz + GRAVITY) * (dz + GRAVITY)));
            theta = (float)Atan((dx * Cos(yaw) + dy * Sin(yaw)) / ((dz + GRAVITY) * (dz + GRAVITY)));
            return false;
        }

        public static void position_control(ref float theta, ref float phi, float yaw, Vector<float> current_position, Vector<float> stable_position)
        {
            Vector<float> distance = Vector<float>.Build.Dense(3, 0);
            Vector<float> time = Vector<float>.Build.Dense(3, 0);

            for (int i = 0; i < 3; ++i)
            {
                distance[i] = (stable_position[i] - current_position[i]);
                time[i] = (int)Ceiling(Abs(distance[i] / MOVMENT_SPEED));
            }
            Vector<float> a = Vector<float>.Build.Dense(3, 0);
            Vector<float> v = Vector<float>.Build.Dense(3, 0);
            for (int i = 0; i < 3; ++i)
            {
                if (time[i] == 0)
                {

                    continue;
                }

                a[i] = 2f * (distance[i] / time[i]) / time[i];
                v[i] = 2f * (distance[i] / time[i]);
            }
            float dx = a[0] + (0.2f * v[0]) / MASS;
            float dy = a[1] + (0.2f * v[1]) / MASS;
            float dz = a[2] + (0.2f * v[2]) / MASS;

            phi = (float)Asin((dx * Sin(yaw) - dy * Cos(yaw)) / (dx * dx + dy * dy + (dz + GRAVITY) * (dz + GRAVITY)));
            theta = (float)Atan((dx * Cos(yaw) + dy * Sin(yaw)) / ((dz + GRAVITY) * (dz + GRAVITY)));
        }
    }
}
