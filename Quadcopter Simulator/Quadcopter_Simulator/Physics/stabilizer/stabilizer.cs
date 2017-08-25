using MathNet.Numerics.LinearAlgebra;
using static System.Math;
using static TripleM.Quadcopter.Physics.config;

namespace TripleM.Quadcopter.Physics
{
    public class stabilizer
    {
        /*
                                                      ************** Cascading Control System ************** 
            ->  In single-loop control, the controller’s set point is set by an operator, and its output drives a final control element (e.g valve). 
                For example: a level controller driving a control valve to keep the level at its set point.
            ->  In a cascade control arrangement, there are two (or more) controllers of which one controller’s output drives the set point of another controller. 
                For example: a level controller driving the set point of a flow controller to keep the level at its set point. The flow controller, in turn, drives a control valve to match the flow with the set point the level controller is requesting.
            ->  The controller driving the set point (the level controller in the example above) is called the primary, outer, or master controller.
                In quadcopter the outer loop determines the required angle for a given path. 
            ->  The controller receiving the set point (flow controller in the example) is called the secondary, inner or slave controller.
                In quadcopter the inner loop controles the corresponding attitude.
                The inner loop is more time critical and guarantees the stablity of the vehicle.  
            ->  For more: http://blog.opticontrols.com/archives/105
            ->  For more: https://www.quora.com/What-is-rate-and-stabilize-PID-in-quadcopter-control
        */

        // Two stage PID cascade
        private PID[] pidRoll;
        private PID[] pidPitch;

        // First PID for locked yaw and the second PID for yaw rotation
        private PID[] pidYaw;
        private float yawLock;

        // First PID for locked height and the second PID for hight value
        private PID[] pidHeight;
        private float heightLock;

        private float STABILIZER_HOVER_THRUST_PWM = getPWMinPointOfEquilibirum(); //1500.0f


        public stabilizer()
        {

            pidYaw = new PID[] { new PID(), new PID() };
            pidRoll = new PID[] { new PID(), new PID() };
            pidPitch = new PID[] { new PID(), new PID() };
            pidHeight = new PID[] { new PID(), new PID() };

            // all PID values are defined here:

            // roll
            pidRoll[STAB].setGains(P_ROLL, 0.0f, 0.0f);
            pidRoll[RATE].setGains(1.5f, 1.0f, 0.0f);
            pidRoll[RATE].setiTermLimit(50.0f);

            // pitch
            pidPitch[STAB].setGains(P_PITCH, 0.0f, 0.0f);
            pidPitch[RATE].setGains(1.5f, 1.0f, 0.0f);
            pidPitch[RATE].setiTermLimit(50.0f);

            // yaw
            pidYaw[STAB].setGains(P_YAW, 0.0f, 0.0f);
            pidYaw[RATE].setGains(2.7f, 0.0f, 0.0f);
            pidYaw[RATE].setiTermLimit(0.0f);
            yawLock = 0.0f;

            // height
            pidHeight[HEIGHT_M].setGains(P_HEIGHT, 0.1f, 0.0f);
            pidHeight[HEIGHT_M].setiTermLimit(0.5f);
            pidHeight[HEIGHTDOT_M_S].setGains(200.0f, 1.0f, 0.0f);
            pidHeight[HEIGHTDOT_M_S].setiTermLimit(100.0f);
            heightLock = Z_START;
        }
        public void setInitYawLock(float yawInitlock)
        {
            yawLock = RAD2DEG(yawInitlock);
        }
        public void setInitHeightLock(float heightInitlock)
        {
            heightLock = heightInitlock;
        }
        public void resetIntegrals()
        {
            pidRoll[STAB].zeroErrorIntegral();
            pidRoll[RATE].zeroErrorIntegral();
            pidPitch[STAB].zeroErrorIntegral();
            pidPitch[RATE].zeroErrorIntegral();
            pidYaw[STAB].zeroErrorIntegral();
            pidYaw[RATE].zeroErrorIntegral();
            pidHeight[HEIGHT_M].zeroErrorIntegral();
            pidHeight[HEIGHTDOT_M_S].zeroErrorIntegral();
        }
        public int getFlightMode()
        {
            return (flight_mode == stable_flight_mode.STABILIZE ? 0 : 1);
        }
        public void setFlightMode(int idx)
        {
            if (idx == 0)
            {
                flight_mode = stable_flight_mode.STABILIZE;
            }
            else
            {
                flight_mode = stable_flight_mode.STABILIZE_HEIGHT;
            }
        }
        public void compute_pwmDutyCycle(ref Vector<float> pwmDutyCycle, Vector<float> RPY_is, Vector<float> RPYDot_is, float height_is, float heightDot_is, float heightDotDot_is, float roll_rx, float pitch_rx, float yaw_rx, float thrust_rx, bool sp)
        {
            /* Control System Steps:
                  1- Convert target roll,pitch,yaw pwmDutyCycle to roll,pitch,yaw Angles.
                  2- Limit target roll and pitch.
                  3- Covert current roll,pitch,yaw Angels from radian to degrees.
                  4- Calculate first attitude control stage: getting values from PID controller function.
                  5- Calculate second attitude control stage with or without body rate conversion.
                  6- Height stabilization.
                  7- Translate into pwm signal.
                  8- Ensuring that during flight motors cannot be turned off.
            */

            // automatic controlling
            if (flight_mode == stable_flight_mode.TRAJECTORY_TRACKING || (flight_mode == stable_flight_mode.STABILIZE_POSITION && sp == true))
            {
                roll_rx = constrainn(RAD2DEG(roll_rx), -MAX_TILT_ANGLE, MAX_TILT_ANGLE);
                pitch_rx = constrainn(RAD2DEG(pitch_rx), -MAX_TILT_ANGLE, MAX_TILT_ANGLE);
                yaw_rx = mapp(yaw_rx, RECEIVER_PWM_MIN, RECEIVER_PWM_MAX, -180.0f, 180.0f);
                yaw_rx = -yaw_rx;
            }
            // user controlling 
            else
            {
                // map rx input to target angles
                roll_rx = mapp(roll_rx, RECEIVER_PWM_MIN, RECEIVER_PWM_MAX, -MAX_TILT_ANGLE, MAX_TILT_ANGLE);
                pitch_rx = mapp(pitch_rx, RECEIVER_PWM_MIN, RECEIVER_PWM_MAX, -MAX_TILT_ANGLE, MAX_TILT_ANGLE);
                yaw_rx = mapp(yaw_rx, RECEIVER_PWM_MIN, RECEIVER_PWM_MAX, -180.0f, 180.0f);
                yaw_rx = -yaw_rx;
            }
            


            // limit roll and pitch sum to maximum
            float tilt_uncorrected = RAD2DEG((float)Acos(Cos(DEG2RAD(roll_rx)) * Cos(DEG2RAD(pitch_rx))));
            if (tilt_uncorrected > MAX_TILT_ANGLE)
            {
                roll_rx  = roll_rx *  (float)MAX_TILT_ANGLE / tilt_uncorrected;
                pitch_rx = pitch_rx * (float)MAX_TILT_ANGLE / tilt_uncorrected;
            }

            // convert fused angles from Radian to Degrees
            Vector<float> RPY_is_deg = RPY_is.Clone();
            Vector<float> RPYDot_is_deg = RPYDot_is.Clone();
            for (int i = 0; i < 3; i++)
            {
                RPY_is_deg[i] = RAD2DEG(RPY_is_deg[i]);
                RPYDot_is_deg[i] = RAD2DEG(RPYDot_is_deg[i]);
            }

            // values to be determined
            float roll_out = 0.0f, pitch_out = 0.0f, yaw_out = 0.0f, thrust_out = 0.0f;

            // First attitude control stage (inner Loop)
            float roll_stab_output = constrainn(pidRoll[STAB].compute(roll_rx, RPY_is_deg[0]), -250.0f, 250.0f);
            float pitch_stab_output = constrainn(pidPitch[STAB].compute(pitch_rx, RPY_is_deg[1]), -250.0f, 250.0f);
            float yaw_error = wrap_180(yawLock - RPY_is_deg[2]);
            float yaw_stab_output = constrainn(pidYaw[STAB].compute(yaw_error, 0.0f), -360.0f, 360.0f);
            
            // if pilot is asking for yaw change feed signal directly to rate pid
            if (Abs(deadband(yaw_rx, 5.0f)) > 0.0f)
            {
                yaw_stab_output = yaw_rx;
                yawLock = RPY_is_deg[2];
            }

            // Second attitude control stage with or without body rate conversion (outer loop)
            
            // rates in body frame
            Vector<float> RPYDot_is_deg_body = getBodyRatesFromEulerRates(RPY_is, RPYDot_is);
            for (int i = 0; i < 3; i++)
                RPYDot_is_deg_body[i] = RAD2DEG(RPYDot_is_deg_body[i]);

            // target rates in body frame
            float[] tmp = { DEG2RAD(roll_stab_output), DEG2RAD(pitch_stab_output), DEG2RAD(yaw_stab_output)};
            Vector<float> target_body_rates = getBodyRatesFromEulerRates(RPY_is, Vector<float>.Build.DenseOfArray(tmp));
            roll_stab_output = RAD2DEG(target_body_rates[0]);
            pitch_stab_output = RAD2DEG(target_body_rates[1]);
            yaw_stab_output = RAD2DEG(target_body_rates[2]);

            
            roll_out = constrainn(pidRoll[RATE].compute(roll_stab_output, RPYDot_is_deg_body[0]), -500.0f, 500.0f);
            pitch_out = constrainn(pidPitch[RATE].compute(pitch_stab_output, RPYDot_is_deg_body[1]), -500.0f, 500.0f);
            yaw_out = constrainn(pidYaw[RATE].compute(yaw_stab_output, RPYDot_is_deg_body[2]), -500.0f, 500.0f);

    
            // height stabilization
            if (flight_mode == stable_flight_mode.STABILIZE)
            {
                thrust_out = mapp(thrust_rx, RECEIVER_PWM_MIN, RECEIVER_PWM_MAX, MOTOR_ESC_PWM_MIN, MOTOR_ESC_PWM_MAX);
                thrust_out = getBodyVerticalFromEarthVertical(RPY_is, thrust_out);
                thrust_out = constrainn(thrust_out, 0.0f, MOTOR_ESC_PWM_MAX - 200.0f);
            }
            else
            {
                thrust_out = STABILIZER_HOVER_THRUST_PWM;

                // map thrust from rx to zero centered signal (throttle stick must be moved into hover position)
                thrust_rx -= STABILIZER_HOVER_THRUST_PWM;

                // if pilot is asking for height change
                if (Abs(deadband(thrust_rx, 10.0f)) > 0.0f)
                {
                    thrust_out += thrust_rx;
                    heightLock = height_is;
                    pidHeight[HEIGHT_M].zeroErrorIntegral();
                    pidHeight[HEIGHTDOT_M_S].zeroErrorIntegral();
                }
                else
                {
                    // height controlled only in discrete levels
                    float height_step = 0.01f;
                    int height_level_difference = (int)((height_is - heightLock) / height_step);
                    float height_discrete = heightLock + ((float)height_level_difference) * height_step;

                    // first stage; controll height difference
                    float additional_thrust = constrainn(pidHeight[HEIGHT_M].compute(heightLock, height_discrete), -2.0f, 2.0f);

                    // second state: controll climb/sink rate
                    additional_thrust = pidHeight[HEIGHTDOT_M_S].compute(additional_thrust, heightDot_is);

                    thrust_out += additional_thrust;
                }

                thrust_out = getBodyVerticalFromEarthVertical(RPY_is, thrust_out);
                thrust_out = constrainn(thrust_out, STABILIZER_HOVER_THRUST_PWM - 300.0f, STABILIZER_HOVER_THRUST_PWM + 300.0f);
            }
          

            // translate into pwm signal (x mode)
            float pwm0 = thrust_out + roll_out - pitch_out + yaw_out;
            float pwm1 = thrust_out - roll_out - pitch_out - yaw_out;
            float pwm2 = thrust_out - roll_out + pitch_out + yaw_out;
            float pwm3 = thrust_out + roll_out + pitch_out - yaw_out;

            // during flight motors cannot be turned off
            pwmDutyCycle[0] = constrainn(pwm0, MOTOR_ESC_PWM_STA, MOTOR_ESC_PWM_MAX);
            pwmDutyCycle[1] = constrainn(pwm1, MOTOR_ESC_PWM_STA, MOTOR_ESC_PWM_MAX);
            pwmDutyCycle[2] = constrainn(pwm2, MOTOR_ESC_PWM_STA, MOTOR_ESC_PWM_MAX);
            pwmDutyCycle[3] = constrainn(pwm3, MOTOR_ESC_PWM_STA, MOTOR_ESC_PWM_MAX);   
        }

        // transformation function
        private Vector<float> getBodyRatesFromEulerRates(Vector<float> eulerAngles, Vector<float> eulerRatesDesired)
        {
            // Source: http://sal.aalto.fi/publications/pdf-files/eluu11_public.pdf
            float sin_phi = (float)Sin(eulerAngles[0]);
            float cos_phi = (float)Cos(eulerAngles[0]);
            float sin_theta = (float)Sin(eulerAngles[1]);
            float cos_theta = (float)Cos(eulerAngles[1]);

            Vector<float> BodyRates = Vector<float>.Build.Dense(3,0);

            BodyRates[0] = eulerRatesDesired[0]                                       -           sin_theta * eulerRatesDesired[2];
            BodyRates[1] =                          cos_phi * eulerRatesDesired[1]    + sin_phi * cos_theta * eulerRatesDesired[2];
            BodyRates[2] =                         -sin_phi * eulerRatesDesired[1]    + cos_phi * cos_theta * eulerRatesDesired[2];

            return BodyRates;
        }
        private float getBodyVerticalFromEarthVertical(Vector<float> eulerAngles, float earthVertical)
        {
            return (float)(earthVertical / (Cos(eulerAngles[0]) * Cos(eulerAngles[1])));
        }
    }

    public enum stable_flight_mode
    {
        STABILIZE = 0,
        STABILIZE_HEIGHT = 1,
        TRAJECTORY_TRACKING = 2,
        STABILIZE_POSITION = 3
    };
}
