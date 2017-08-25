using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;

namespace TripleM.Quadcopter.Physics
{
    class config
    {

        //======================================================================
        //===================== OUR FRAMES ====================================
        //======================================================================
        /*
                                      GRAPHICS FRAME            

                        Y
                        |
                        |
                        |
                        |
                        |_________________________ Z
                       /
                      /
                     / 
                    /     
                   X


                                     PHYSICS FRAME

                        Z
                        |
                        |
                        |
                        |
                        |_________________________ X
                       /
                      /
                     / 
                    /     
                   Y

           */






        //======================================================================
        //===================== LOOP TIMING ====================================
        //======================================================================

        // global simulation times in nanoseconds
        public static long QS_SIMULATION_END = 200000000000;  // Max play time 
        public static long QS_TIME_DELTA = 500000;            // Timestep
        public static long QS_USER_INPUT_PERIOD = 20000000;   // Time period for get from reciever
        public static long QS_SENSOR_INPUT_PERIOD = 5000000;  // Time period for get from sensors 















        //======================================================================
        //===================== PHYSICAL PARAMETERS ============================
        //======================================================================

        // physical constants
        public static float GRAVITY = 9.81f;                          // m.sec^-2
        public static float DENSITY = 1.22f;
        public static ENVIROMENT_DENSITY ENVIROMENT = ENVIROMENT_DENSITY.AIR;
        public static float TEMPERATURE = 299.15f;                    // Kelvin where T(K) = T(°C) + 273.15
        public static float TEMPERATURE_SEA_LEVEL = 288.15f;
        public static float RELATIVE_HUMIDITY = 0.0f;                 // 0.0% <--> 100.0%

        // magnetic field in central europe according to wikipedia
        public static float MAGFIELD_EARTH_X = 0.2f;     // Gauss
        public static float MAGFIELD_EARTH_Y = 0.0f;
        public static float MAGFIELD_EARTH_Z = -0.44f;
        















        //======================================================================
        //===================== INITIAL SIMULATION CONDITIONS ==================
        //======================================================================

        // velocity, acceleration, angular rotation and angular acceleration are set to 0.0 at the beginning

        // position and attitude initialization
        public static float Y_START = 54;            // meter
        public static float X_START = -75;
        public static float Z_START = 7;
        public static float ROLL_START = 0.0f;       // degrees
        public static float PITCH_START = 0.0f;
        public static float YAW_START = 90.0f;




















        //======================================================================
        //===================== QUADCOPTER MECHANICS ===========================
        //======================================================================

        /*
        *************** X versus Plus (+) Configuration *************** (according to www.aeroquad.com)
        This refers to the configuration of the frame.
        In the X configuration, the quad has two motors on each of the four sides, while still having 4 motors total. 
        This simply means that the front of the quad is between two of the front motors, the rear between two of the rear motors, and so on. 
        In the plus configuration, the location of the front is simply the front motor, the rear is the rear motor, and so on. 
        The plus configuration is generally the more common for beginners. 
        The X configuration is more useful for aerial photography where the camera needs to be positioned as close to the center of the frame as possible while still having an open view. 
        The same applies of course for Hexa and Octo X / Plus - configurations. 
        */

        // frame mode (plus or X) -> only X frame is implemented, but plus can be added easily
        public static QS_FRAME_MODE QS_FRAME_MODE_DEFAULT = QS_FRAME_MODE.QS_FRAME_MODE_XH;

        // arm length
        public static float LENGTH_ARM = 0.225f;   // meter

        // Inertia matrix and mass.
        // source: http://www8.informatik.uni-wuerzburg.de/fileadmin/10030800/user_upload/quadcopter/Abschlussarbeiten/6DOF_Control_Lebedev_MA.pdf

        public static float CENTRAL_MASS = 0.4f;             // kg
        public static float CENTRAL_MASS_RADIUS = 0.5f;      // meter
        public static float DRAG_COEFFICIENT = 0.5f;         // Cd
        public static float Propeller_RADIUS = 0.127f;       // meter
        public static float MOTOR_MASS = 0.075f;             // kg
        public static float CENTRAL_MASS_HIGH = 0.05f;       // meter
        public static float MASS = (CENTRAL_MASS + 4.0f * MOTOR_MASS);

        public static void SET_MASS(float mass)
        {
            MASS = mass;
    
            MOTOR_THRUST_MIN = MASS * GRAVITY / 4.0f * 0.0f;
            MOTOR_THRUST_MAX = MASS * GRAVITY / 4.0f * 2.0f;
            MOTOR_THRUST_EQU = MASS * GRAVITY / 4.0f;
            MOTOR_RPM_MIN = (float)Math.Pow(1.0 / MOTOR_CONSTANT * MOTOR_THRUST_MIN, 1.0 / MOTOR_EXPONENT_Q);
            MOTOR_RPM_MAX = (float)Math.Pow(1.0 / MOTOR_CONSTANT * MOTOR_THRUST_MAX, 1.0 / MOTOR_EXPONENT_Q);
            MOTOR_RPM_EQU = (float)Math.Pow(1.0 / MOTOR_CONSTANT * MOTOR_THRUST_EQU, 1.0 / MOTOR_EXPONENT_Q);
        }

        public static float INERTIA_X = 5e-3f;
        public static float INERTIA_Y = 5e-3f;
        public static float INERTIA_Z = 10e-3f;


        // quadcopter specific air resistance
        public static float DRAG_CONSTANT = (float)(0.5 * DENSITY * DRAG_COEFFICIENT * Math.PI * Math.Pow(CENTRAL_MASS_RADIUS,2));

        public static void setDragConst(int factor)
        {
            switch (factor)
            {
                case 0:
                    DENSITY = 0;
                    ENVIROMENT = ENVIROMENT_DENSITY.VACUUM;
                    break;
                case 1:
                    DENSITY = 1.22f;
                    ENVIROMENT = ENVIROMENT_DENSITY.AIR;
                    break;
                case 2:
                    DENSITY = 70f;
                    ENVIROMENT = ENVIROMENT_DENSITY.LIQUID_HYDROGEN;
                    break;
                case 3:
                    DENSITY = 820f;
                    ENVIROMENT = ENVIROMENT_DENSITY.OIL;
                    break;
                case 4:
                    DENSITY = 1000f;
                    ENVIROMENT = ENVIROMENT_DENSITY.WATER;
                    break;
                default:
                    break;

            }
            DRAG_CONSTANT = (float)(0.5 * DENSITY * DRAG_COEFFICIENT * Math.PI * Math.Pow(CENTRAL_MASS_RADIUS, 2));
        }












        //======================================================================
        //===================== MOTOR ELECTRONICS ==============================
        //======================================================================


        /* There are two possible relations between pwm duty cycle at the esc-input and rpm/thrust.
         * 
         *		duty cycle to rpm:    linear     ---->    duty cycle to thrust: NON-linear
         *
         *		duty cycle to thrust: linear     ---->    duty cycle to rpm:    NON-linear
         *
         *      esc : Electronic Speed Control
         *      pwm : Pulse Width Modulation ... pwm allows us to controll speed the speed of motors 
         */

        public static float MOTOR_ESC_PWM_MIN = 1000.0f;     // pwm at min, motors not spinning
        public static float MOTOR_ESC_PWM_STA = 1100.0f;     // pwm at min rpm and min thrust (motor start spinning)
        public static float MOTOR_ESC_PWM_MAX = 2000.0f;     // pwm at max rpm and max thrust

        public static bool POWER_OFF = false;

        public static QS_ESC_DUTY_CYCLE_THRUST_RELATION QS_ESC_DUTY_CYCLE_THRUST_RELATION_DEFAULT = QS_ESC_DUTY_CYCLE_THRUST_RELATION.QS_ESC_DUTY_CYCLE_2_THRUST_LINEAR;

        /*
         * We assume a NON-linear relation beetween rpm and thrust:
         *
         *		Thrust = motor_constant * rpm ^ Q	-> 'motor_constant' and 'Q' should be aligned to measured point in equilibrium.
         *											->  Single motor equation.
         *											->  The unit is Newton.
         *											->  Example: If the quadcopter weights 0.5kg you would need a thrust of 0.5*9.81 = 4.905 Newton (all 4 motors) to hover.
         *
         * We assume a non-linear relation beetween rpm and torque:
         *
         *		Torque = torque_yaw_constant * rpm ^ QQ	-> 'torque_yaw_constant' can't be measuered easily, but estimated.
         *												->  Single motor equation.
         */

        public static float MOTOR_CONSTANT = 9.6549e-8f; // "k" constant
        public static float MOTOR_EXPONENT_Q = 2.1f;
        public static float TORQUE_YAW_CONSTANT = 4.8274e-9f; // "b" constant
        public static float TORQUE_YAW_EXPONENT_QQ = 2.0f;

        // A quadcopter should be able to generate a thrust to lift twice it's own weight.
        // With the motor equation which is defined above the required rpm's can be calculated.
        public static float MOTOR_THRUST_MIN = MASS * GRAVITY / 4.0f * 0.0f;
        public static float MOTOR_THRUST_MAX = MASS * GRAVITY / 4.0f * 2.0f;
        public static float MOTOR_THRUST_EQU = MASS * GRAVITY / 4.0f;
        public static float MOTOR_RPM_MIN = (float)Math.Pow(1.0 / MOTOR_CONSTANT * MOTOR_THRUST_MIN, 1.0 / MOTOR_EXPONENT_Q);
        public static float MOTOR_RPM_MAX = (float)Math.Pow(1.0 / MOTOR_CONSTANT * MOTOR_THRUST_MAX, 1.0 / MOTOR_EXPONENT_Q);
        public static float MOTOR_RPM_EQU = (float)Math.Pow(1.0 / MOTOR_CONSTANT * MOTOR_THRUST_EQU, 1.0 / MOTOR_EXPONENT_Q);

        // the hoovering pwm has to be known approximately, here an offset can be defined
        public static float MOTOR_PWM_EQU_OFFSET = 10.0f;

        public static float getPWMinPointOfEquilibirum()
        {
            float pwm_equib = 0;
            float scale = (MOTOR_ESC_PWM_MAX - MOTOR_ESC_PWM_MIN);

            if (QS_ESC_DUTY_CYCLE_THRUST_RELATION_DEFAULT == QS_ESC_DUTY_CYCLE_THRUST_RELATION.QS_ESC_DUTY_CYCLE_2_RPM_LINEAR)
                pwm_equib = (MOTOR_RPM_EQU - MOTOR_RPM_MIN) * scale / (MOTOR_RPM_MAX - MOTOR_RPM_MIN) + MOTOR_ESC_PWM_MIN;

            if (QS_ESC_DUTY_CYCLE_THRUST_RELATION_DEFAULT == QS_ESC_DUTY_CYCLE_THRUST_RELATION.QS_ESC_DUTY_CYCLE_2_THRUST_LINEAR)
                pwm_equib = (MOTOR_THRUST_EQU - MOTOR_THRUST_MIN) * scale / (MOTOR_THRUST_MAX - MOTOR_THRUST_MIN) + MOTOR_ESC_PWM_MIN;

            return pwm_equib + MOTOR_PWM_EQU_OFFSET;
        }



















        //======================================================================
        //===================== STABLIZER ======================================
        //======================================================================

        public static int STAB = 0;         // Outer Control Loop 
        public static int RATE = 1;         // Inner Control Loop

        public static int HEIGHT_M = 0;
        public static int HEIGHTDOT_M_S = 1;
        
        public static float MAX_TILT_ANGLE = 30.0f;    // degrees


        public static void set_max_tilt_angle(float angle)
        {
            MAX_TILT_ANGLE = angle;
        }

        public static Vector<float> STAB_POSITION = Vector<float>.Build.Dense(3, 0);

        public static QS_CONTROLLER_TYPE CONTROLLER_TYPE = QS_CONTROLLER_TYPE.QS_CONTROLLER_TYPE_PID;

        public static stable_flight_mode flight_mode = stable_flight_mode.STABILIZE_HEIGHT;



        public static float P_ROLL = 4.5f;
        public static float P_PITCH = 4.5f;
        public static float P_YAW = 1.5f;
        public static float P_HEIGHT = 1.0f;


        public static bool RESTART = false;

















        //======================================================================
        //===================== RECEIVER =======================================
        //======================================================================

        // I assume a linear relation between stick position at the transmitter and pwm duty cycle at the receiver.
        // If a stick is in the lowest/leftist position		-> RECEIVER_PWM_MIN is received.
        // If a stick is in the middle position				-> REICEVER_PWM_ZERO_SIGNAL is received.
        // If a stick is in the highest/rightest position	-> RECEIVER_PWM_MAX is received.


        public static float RECEIVER_PWM_MIN = 1000.0f;
        public static float RECEIVER_PWM_MAX = 2000.0f;
        public static float RECEIVER_PWM_ZERO_SIGNAL = (RECEIVER_PWM_MAX + RECEIVER_PWM_MIN) / 2.0f;

        // A keyboard is non-analog input, so a key can represent only a specific pwm value.
        // RECEIVER_PWM_ZERO_SIGNAL is added in all cases except for throttle.
        public static float RECEIVER_ROLL_KEY_PWM = 300.0f;
        public static float RECEIVER_PITCH_KEY_PWM = 300.0f;
        public static float RECEIVER_YAW_KEY_PWM = 250.0f;
        public static float RECEIVER_THROTTLE_KEY_PWM = 100.0f;
























        //======================================================================
        //===================== SENSORS ========================================
        //======================================================================

        // sensor noise and offset parameters
        public static float BAROMETER_OFFSET = 0.0f;           // meters
        public static float BAROMETER_STANDEV = 0.0f;
        public static float MAGNETOMETER_OFFSET_X = 0.0f;      // Gauss (see definition of magnetic earth field at top)
        public static float MAGNETOMETER_OFFSET_Y = 0.0f;
        public static float MAGNETOMETER_OFFSET_Z = 0.0f;
        public static float MAGNETOMETER_STANDEV_X = 0.0f;
        public static float MAGNETOMETER_STANDEV_Y = 0.0f;
        public static float MAGNETOMETER_STANDEV_Z = 0.0f;
        public static float GYROSCOPE_OFFSET_R = 0.0f;          // degrees/s
        public static float GYROSCOPE_OFFSET_P = 0.0f;
        public static float GYROSCOPE_OFFSET_Y = 0.0f;
        public static float GYROSCOPE_STANDEV_R = 0.0f;
        public static float GYROSCOPE_STANDEV_P = 0.0f;
        public static float GYROSCOPE_STANDEV_Y = 0.0f;
        public static float ACCELEROMETER_OFFSET_X = 0.0f;       // m/s^2
        public static float ACCELEROMETER_OFFSET_Y = 0.0f;
        public static float ACCELEROMETER_OFFSET_Z = 0.0f;
        public static float ACCELEROMETER_STANDEV_X = 0.0f;
        public static float ACCELEROMETER_STANDEV_Y = 0.0f;
        public static float ACCELEROMETER_STANDEV_Z = 0.0f;

        public static float CHIP_OFFSET_R = 0.0f;   // degrees
        public static float CHIP_OFFSET_P = 0.0f;
        public static float CHIP_OFFSET_Y = 0.0f;

        public static float sensorsAccuracyfactor = 0.0f;
        public static void setSensorsAccuracy(float factor)
        {
            sensorsAccuracyfactor = factor;
            BAROMETER_OFFSET = factor * 10.0f;           
            BAROMETER_STANDEV = factor * 0.5f;
            MAGNETOMETER_OFFSET_X = factor * 0.01f;  
            MAGNETOMETER_OFFSET_Y = factor * 0.01f;
            MAGNETOMETER_OFFSET_Z = factor * 0.01f;
            MAGNETOMETER_STANDEV_X = factor * 0.003f;
            MAGNETOMETER_STANDEV_Y = factor * 0.003f;
            MAGNETOMETER_STANDEV_Z = factor * 0.003f;
            GYROSCOPE_OFFSET_R = factor * 4.0f;          
            GYROSCOPE_OFFSET_P = factor * 2.0f;
            GYROSCOPE_OFFSET_Y = factor * 3.0f;
            GYROSCOPE_STANDEV_R = factor * 1.4f;
            GYROSCOPE_STANDEV_P = factor * 2.5f;
            GYROSCOPE_STANDEV_Y = factor * 0.5f;
            ACCELEROMETER_OFFSET_X = factor * 0.2f;       
            ACCELEROMETER_OFFSET_Y = factor * 0.1f;
            ACCELEROMETER_OFFSET_Z = factor * 0.3f;
            ACCELEROMETER_STANDEV_X = factor * 0.2f;
            ACCELEROMETER_STANDEV_Y = factor * 0.25f;
            ACCELEROMETER_STANDEV_Z = factor * 0.23f;

            CHIP_OFFSET_R = factor * -3.0f;  
            CHIP_OFFSET_P = factor * 2.0f;
            CHIP_OFFSET_Y = factor * -3.5f;
        }


        // sensor calibration readings from horizontal position before simulation start
        public static float BAROMETER_CAL_READS = 1000;
        public static float MAGNETOMETER_CAL_READS = 1000;
        public static float GYROSCOPE_CAL_READS = 1000;
        public static float ACCELEROMETER_CAL_READS = 1000;


















        //======================================================================
        //===================== SENSOR FUSION ==================================
        //======================================================================

        public static int ROLL = 0;
        public static int PITCH = 1;
        public static int YAW = 2;
        public static int HEIGHTDOT = 3;

        public static int SFUS_SENSOR_TILT_DROPS = 200;
        public static int SFUS_SENSOR_TILT_READS = 600;

        public static int SFUS_INIT_ACCEL_DROPS = 50;
        public static int SFUS_INIT_ACCEL_READS = 300;
        public static int SFUS_INIT_GYROS_DROPS = 50;
        public static int SFUS_INIT_GYROS_READS = 300;
        public static int SFUS_INIT_MAGNE_DROPS = 50;
        public static int SFUS_INIT_MAGNE_READS = 300;
        public static int SFUS_INIT_BAROM_DROPS = 50;
        public static int SFUS_INIT_BAROM_READS = 300;

        public static int SFUS_ACCEL_BUF = 10;
        public static int SFUS_GYROS_BUF = 10;
        public static int SFUS_MAGNE_BUF = 10;
        public static int SFUS_BAROM_BUF = 10;

        // Roll and pitch are adjusted proportionately to the total tilt angle.
        // Sampling period is a privat class member.
        public static float SFUS_COMPF_RP_TAU_MIN = 2.5f;   //0.0001//0.999999//
        public static float SFUS_COMPF_RP_TAU_MAX = 20.0f;  //0.001//0.99999999//
        public static float SFUS_COMPF_RP_LIM_TILT = 10.0f;
        public static float SFUS_COMPF_YAW_A = 1.0f;
        public static float SFUS_COMPF_HEIGHTDOT_A = 0.99667777f;
        public static float SFUS_MA_HEIGHT = 20.0f;

        // This values are recalculated each time the simulation starts.
        public static float SFUS_SENSOR_TILT_R = 0.0f;
        public static float SFUS_SENSOR_TILT_P = 0.0f;
        public static float SFUS_SENSOR_TILT_Y = 0.0f;

        public static float COMPLEMENTARY_FILTER_TAU_UNDEFINED = -1.0f;


















        //======================================================================
        //===================== Wind Effect PARAMETERS =========================
        //======================================================================

        public static float WIND_OFFSET_X = 0.0f;       // m/s^2
        public static float WIND_OFFSET_Y = 0.0f;
        public static float WIND_OFFSET_Z = 0.0f;
        public static float WIND_SPEED = 0;
        public static int WIND_DEGREE = 0;
        public static void SET_WIND_OFFSET(int degree, float speed)
        {
            WIND_OFFSET_X = speed / 30f * (float)Math.Cos(DEG2RAD(degree));
            WIND_OFFSET_Y = speed / 30f * (float)Math.Sin(DEG2RAD(degree));
            WIND_OFFSET_Z = 0.0f;
            WIND_SPEED = speed;
            WIND_DEGREE = degree;
        }











        //======================================================================
        //===================== TRAJECTORY TRACKING ============================
        //======================================================================
        public static int MOVMENT_SPEED = 5;
        public static Queue<Vector<float>> CURRENT_POINTS = new Queue<Vector<float>>();






















        //======================================================================
        //==================== PROJECTILE PARAMETERS ===========================
        //======================================================================

        public static float PROJECTILE_MASS = 0.05f; //kg
        public static float PROJECTILE_VELOCITY = 20; //m/s
        public static float PROJECTILE_DIAMETER = 0.1f; //m
        public static float PROJECTILE_TIMESTEP = 0.01f; //s



        // RECOIL EFFECT
        public static bool FIRING = false;
        public static Vector<float> RECOIL_VELOCITY = Vector<float>.Build.Dense(3,0);
        public static Vector<float> PROJ_VELOCITY = Vector<float>.Build.Dense(3, 0);
        public static bool RECOIL_EFFECT = false;

        // MAGNUS EFFECT
        public static Vector<float> ROTATION_SPEED = Vector<float>.Build.Dense(3, 0);






        public static float SIMULATION_SPEED = 1f;






        //======================================================================
        //===================== Associate Function =============================
        //======================================================================
        public static float DEG2RAD(float degree)
        {
            return (float)(Math.PI / 180) * degree;
        }
        public static float RAD2DEG(float rad)
        {
            return (float)(180.0 / (float)Math.PI) * rad;
        }

        public static float mapp(float input, float in_min, float in_max, float out_min, float out_max)
        {
            return (((input - in_min) * ((out_max - out_min) / (in_max - in_min))) + out_min);
        }
        public static float constrainn(float current, float minimum, float maximum)
        {
            current = (current <= minimum) ? minimum : current;
            current = (current >= maximum) ? maximum : current;

            return current;
        }
        public static float wrap_180(float x)
        {
            return (x < -180 ? x + 360 : (x > 180 ? x - 360 : x));
        }
        public static float deadband(float value, float threshold)
        {
            // typecast of 0?
            if ((value > 0 && value < threshold) || (value < 0 && value > -threshold))
                value = 0;
            else if (value > 0)
                value -= threshold;
            else if (value < 0)
                value += threshold;

            return value;
        }

        public static float wrap_Pi(float x)
        {
            float Pi = 3.1415926535897f;
            float Pi2 = 6.28318530718f;
            return x < -Pi ? x + Pi2 : (x > Pi ? x - Pi2 : x);
        }
        public static bool HasValue(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
        public static Vector<float> Cross(Vector<float> u, Vector<float> v)
        {
            float[] x = {   u[1]*v[2] - u[2]*v[1] ,
                            u[2]*v[0] - u[0]*v[2] ,
                            u[0]*v[1] - u[1]*v[0]
            };
            return Vector<float>.Build.DenseOfArray(x);
        }
        public static bool InRange(float value, float min, float max)
        {
            return ((value >= min) && (value <= max));
        }
    }

    public enum QS_ESC_DUTY_CYCLE_THRUST_RELATION
    {
        QS_ESC_DUTY_CYCLE_2_RPM_LINEAR,
        QS_ESC_DUTY_CYCLE_2_THRUST_LINEAR
    };

    public enum QS_FRAME_MODE
    {
        QS_FRAME_MODE_PL = 0,
        QS_FRAME_MODE_XH = 1
    };

    public enum QS_CONTROLLER_TYPE
    {
        QS_CONTROLLER_TYPE_PID = 0,
        QS_CONTROLLER_TYPE_PD = 1,
        QS_CONTROLLER_TYPE_PI = 2,
        QS_CONTROLLER_TYPE_P = 3
    };

    public enum ENVIROMENT_DENSITY
    {
        VACUUM = 0,             // 0
        AIR = 1,                // 1.22
        LIQUID_HYDROGEN = 2,    // 70
        OIL = 3,                // 820 
        WATER = 4,              // 1000
    };

}
