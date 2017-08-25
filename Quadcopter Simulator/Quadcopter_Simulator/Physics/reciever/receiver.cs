using MathNet.Numerics.LinearAlgebra;
using Quadcopter_Simulator;
using System.Runtime.InteropServices;
using static TripleM.Quadcopter.Physics.config;

namespace TripleM.Quadcopter.Physics
{
    class receiver
    {
        // pwm signals when a certain key is pressed
        private float roll_pwm;
        private float pitch_pwm;
        private float yaw_pwm;
        private float throttle_pwm;
        // always return zero
        private bool output_blocked;

        public receiver()
        {
            roll_pwm = RECEIVER_ROLL_KEY_PWM;
            pitch_pwm = RECEIVER_PITCH_KEY_PWM;
            yaw_pwm = RECEIVER_YAW_KEY_PWM;
            throttle_pwm = RECEIVER_THROTTLE_KEY_PWM;

            output_blocked = false;
        }
        

        // in radians
        public void get_desired_theta(ref Vector<float> theta_d)
        {
            // zero signals in case no input or blocked
            theta_d[0] = RECEIVER_PWM_ZERO_SIGNAL;
            theta_d[1] = RECEIVER_PWM_ZERO_SIGNAL;
            theta_d[2] = RECEIVER_PWM_ZERO_SIGNAL;

            // only roll and pitch
            if (!output_blocked)
            {
                // roll (A and D)
                if (keypressed(0x44))
                    theta_d[0] -= roll_pwm;
                else if (keypressed(0x41))
                    theta_d[0] += roll_pwm;

                // pitch (W and S)
                if (keypressed(0x53))
                    theta_d[1] += pitch_pwm;
                else if (keypressed(0x57))
                    theta_d[1] -= pitch_pwm;

                // yaw (Q and E)
                if (keypressed(0x51))
                    theta_d[2] -= yaw_pwm;
                else if (keypressed(0x45))
                    theta_d[2] += yaw_pwm;
            }

            theta_d[0] = constrainn(theta_d[0], RECEIVER_PWM_MIN, RECEIVER_PWM_MAX);
            theta_d[1] = constrainn(theta_d[1], RECEIVER_PWM_MIN, RECEIVER_PWM_MAX);
            theta_d[2] = constrainn(theta_d[2], RECEIVER_PWM_MIN, RECEIVER_PWM_MAX);
        }

        // in meter per second
        public void get_desired_throttle(ref float throttle)
        {
            // user has to put stick in hover position if he does not want to climb/sink
            throttle = getPWMinPointOfEquilibirum();

            // if block demand normal position
            if (!output_blocked)
            {
                if (keypressed(0x26))
                    throttle += throttle_pwm;

                else if (keypressed(0x28))
                    throttle -= throttle_pwm;

            }

            throttle = constrainn(throttle, RECEIVER_PWM_MIN, RECEIVER_PWM_MAX);
        }

        // always return zero
        public void block_receiver(bool blocked)
        {
            output_blocked = blocked;
        }

        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey); // Keys enumeration

        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(System.Int32 vKey);

        private bool keypressed(int keyvalue)
        {
            short tabKeyState = GetAsyncKeyState(keyvalue);

            // test high bit - if set, key was down when GetAsyncKeyState was called
            if (((1 << 16) & tabKeyState) != 0)
                return true;

            return false;
        }
    }
}
