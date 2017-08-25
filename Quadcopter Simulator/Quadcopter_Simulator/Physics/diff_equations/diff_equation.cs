using static TripleM.Quadcopter.Physics.config;
using static System.Math;
using MathNet.Numerics.LinearAlgebra;

namespace TripleM.Quadcopter.Physics
{
    class diff_equation
    {
        // Inertis Matrix is same in (+) and (x) mode
        public static void calc_inertia_matrix(ref Matrix<float> Inertia, float central_mass, float central_rad, float motor_mass, float L)
        {
            float xx = 0.0f;
            float yy = 0.0f;
            float zz = 0.0f;

            float fac_central_mass = 2.0f / 5.0f * central_mass * central_rad * central_rad;

            xx += fac_central_mass;
            yy += fac_central_mass;
            zz += fac_central_mass;

            xx += 2 * L * L * motor_mass;
            yy += 2 * L * L * motor_mass;
            zz += 4 * L * L * motor_mass;

            // quadcopter symmetrical, so values other than zero only on diagonal
            Inertia.Clear();
            Inertia[0, 0] = xx;
            Inertia[1, 1] = yy;
            Inertia[2, 2] = zz;
        }

        // linear acceleration
        public static void acceleration(ref Vector<float> a, Vector<float> speeds, Vector<float> angles, Vector<float> vels, float m, float g, float k, float kd, float height, Vector<float> xdot)
        {
            // Gravity force
            Vector<float> gravity = Vector<float>.Build.Dense(3, 0);
            gravity[2] = g;

            // Thrust force
            Vector<float> T_body = Vector<float>.Build.Dense(3, 0);
            thrust(ref T_body, speeds, k, height);

            Matrix<float> R = Matrix<float>.Build.Dense(3, 3, 0);
            rotation(ref R, angles);

            // Convert Thrust from body frame to inertial frame
            Vector<float> T_inertial = R * T_body;

            // Drag force
            float[] fd = { kd * vels[0], kd * vels[1], kd * vels[2] };
            Vector<float> Fd = Vector<float>.Build.DenseOfArray(fd);

            // Calc Acceleration using The Second Newton Law
            a = -gravity + (T_inertial - Fd) / m;
        }

        // Angular Acceleration
        public static void angular_acceleration(ref Vector<float> omegadot, Vector<float> speeds, Vector<float> omega, Matrix<float> I, float L, float b, float k, QS_FRAME_MODE frame_mode)
        {
            Vector<float> tau = Vector<float>.Build.Dense(3, 0);

            // The torques are NOT the same in (+) and (×) mode
            if (frame_mode == QS_FRAME_MODE.QS_FRAME_MODE_PL)
                torques_plus(ref tau, speeds, L, b, k);
            else if (frame_mode == QS_FRAME_MODE.QS_FRAME_MODE_XH)
                torques_xh(ref tau, speeds, L, b, k);

            omegadot = I.Inverse() * (tau - Cross(omega, I * omega));
        }


        // convert from INERTIAL frame to BODY frame
        public static void thetadot2omega(ref Vector<float> omega, Vector<float> thetadot, Vector<float> angles)
        {
            float phi = angles[0];
            float theta = angles[1];

            float[,] w = {  { 1 ,       0           , -1* (float) Sin(theta)          },
                            { 0 , (float)Cos(phi)   , (float) (Cos(theta) * Sin(phi)) },
                            { 0 , (float)-Sin(phi)  , (float) (Cos(theta) * Cos(phi)) }
                          };
            Matrix<float> W = Matrix<float>.Build.DenseOfArray(w);
            omega = W * thetadot;
        }

        // convert from BODY frame to INERTIAL frame
        public static void omega2thetadot(ref Vector<float> thetadot, Vector<float> omega, Vector<float> angles)
        {
            float phi = angles[0];
            float theta = angles[1];

            // Option A
            /*
            float[,] w = { { 1 , 0          , -1* Sin(theta)        },
                            { 0 , Cos(phi)   , Cos(theta) * Sin(phi) },
                            { 0 ,-Sin(phi)   , Cos(theta) * Cos(phi) }
                          };
            Matrix<float> W = Matrix<float>.Build.DenseOfArray(w);
            thetadot = W.Inverse() * omega;
            */
            // Option B
            float[,] w = {  { 1 , (float)(Sin(phi) * Tan(theta)) ,(float)( Cos(phi) * Tan(theta)) } ,
                            { 0 , (float)Cos(phi)                ,          (float)  -Sin(phi)    } ,
                            { 0 , (float)(Sin(phi) / Cos(theta)) ,(float)( Cos(phi) / Cos(theta)) }
            };
            Matrix<float> W_invers = Matrix<float>.Build.DenseOfArray(w);

            thetadot = W_invers * omega;
        }

        // from BODY frame to INERTIAL frame
        public static void rotation(ref Matrix<float> R, Vector<float> angles)
        {
            float phi = angles[0];     // roll
            float theta = angles[1];   // pitch
            float psi = angles[2];     // yaw

            float[,] r = {  { (float)(Cos(psi) * Cos(theta)) ,(float)( Cos(psi) * Sin(theta) * Sin(phi) - Sin(psi) * Cos(phi))    , (float)( Cos(psi) * Sin(theta) * Cos(phi) + Sin(psi) * Sin(phi))},
                            { (float)(Sin(psi) * Cos(theta)) ,(float)(Sin(psi) * Sin(theta) * Sin(phi) + Cos(psi) * Cos(phi))     , (float)( Sin(psi) * Sin(theta) * Cos(phi) - Cos(psi) * Sin(phi))},
                            { (float)-Sin(theta)             ,(float)( Cos(theta) * Sin(phi))                                     , (float)(Cos(theta) * Cos(phi))                                  }
            };

            R = Matrix<float>.Build.DenseOfArray(r);
        }

        // Thrust force
        public static void thrust(ref Vector<float> T_body, Vector<float> speeds, float k, float height)
        {
            Vector<float> speeds_sqr_thrust = Vector<float>.Build.Dense(4, 0);

            for (int i = 0; i < 4; i++)
                speeds_sqr_thrust[i] = (float)Pow(speeds[i], MOTOR_EXPONENT_Q);

            T_body[0] = 0;
            T_body[1] = 0;
            T_body[2] = k * speeds_sqr_thrust.Sum();
        }

        // Drag force 
        public static void calc_drag_constant(float height)
        {
            calc_medium_properties(height);
            DRAG_CONSTANT = (float)(0.5 * DENSITY * DRAG_COEFFICIENT * PI * Pow(CENTRAL_MASS_RADIUS, 2));
        }

        // Torques in (+) mode
        public static void torques_plus(ref Vector<float> tau, Vector<float> speeds, float L, float b, float k)
        {
            Vector<float> speeds_sqr_thrust = Vector<float>.Build.Dense(4, 0);
            Vector<float> speeds_sqr_torque = Vector<float>.Build.Dense(4, 0);

            for (int i = 0; i < 4; i++)
            {
                speeds_sqr_thrust[i] = (float)Pow(speeds[i], MOTOR_EXPONENT_Q);
                speeds_sqr_torque[i] = (float)Pow(speeds[i], TORQUE_YAW_EXPONENT_QQ);
            }

            tau[0] = L * k * (speeds_sqr_thrust[3] - speeds_sqr_thrust[1]);
            tau[1] = L * k * (speeds_sqr_thrust[2] - speeds_sqr_thrust[0]);

            //tau[2] = b*(speeds_sqr_torque[1] + speeds_sqr_torque[3] - speeds_sqr_torque[0] - speeds_sqr_torque[2]);
            tau[2] = b * (-speeds_sqr_torque[1] - speeds_sqr_torque[3] + speeds_sqr_torque[0] + speeds_sqr_torque[2]);
        }

        // Torques in (×) mode
        public static void torques_xh(ref Vector<float> tau, Vector<float> speeds, float L, float b, float k)
        {
            Vector<float> speeds_sqr_thrust = Vector<float>.Build.Dense(4, 0);
            Vector<float> speeds_sqr_torque = Vector<float>.Build.Dense(4, 0);

            for (int i = 0; i < 4; i++)
            {
                speeds_sqr_thrust[i] = (float)Pow(speeds[i], MOTOR_EXPONENT_Q);
                speeds_sqr_torque[i] = (float)Pow(speeds[i], TORQUE_YAW_EXPONENT_QQ);
            }

            tau[0] = (float)(L / Sqrt(2) * k * (speeds_sqr_thrust[0] + speeds_sqr_thrust[3] - speeds_sqr_thrust[1] - speeds_sqr_thrust[2]));
            tau[1] = (float)(L / Sqrt(2) * k * (speeds_sqr_thrust[3] + speeds_sqr_thrust[2] - speeds_sqr_thrust[0] - speeds_sqr_thrust[1]));

            //(*tau)(2) = b*(speeds_sqr_torque(1) + speeds_sqr_torque(3) - speeds_sqr_torque(0) - speeds_sqr_torque(2));
            tau[2] = b * (-speeds_sqr_torque[1] - speeds_sqr_torque[3] + speeds_sqr_torque[0] + speeds_sqr_torque[2]);
        }
        
        /** OPTION function: update enviroment parameters **/
        private static void calc_medium_properties(float height)
        {
            height = height - 25f;
            TEMPERATURE = TEMPERATURE_SEA_LEVEL - 0.0065f * height;
            GRAVITY = (9.806f * (6371000.0f / (6371000.0f + height)));
            float absolute_pressure = (float)Pow(101.325, -(GRAVITY * 0.0289644 * height) / (8.31447 * TEMPERATURE)); //* (float)Pow((1 - (0.0065f * height) / TEMPERATURE), (GRAVITY * 0.028964f) / (287.058f * 0.0065f));
            float vapor_pressure = RELATIVE_HUMIDITY * 6.1078f * (float)Pow(10, (7.5 * TEMPERATURE) / (TEMPERATURE + 237.3));
            //DENSITY = 1000 * (absolute_pressure - vapor_pressure) / (287.058f * TEMPERATURE) + (vapor_pressure) / (461.495f * TEMPERATURE);
            DENSITY =  100000f * absolute_pressure / (287.058f * TEMPERATURE);
        }
    }
}
