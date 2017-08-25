using static TripleM.Quadcopter.Physics.config;

namespace TripleM.Quadcopter.Physics
{
    class esc_motor
    {
        private float input_pwmDutyCycle;
        public void esc_set_inputPWMDutyCycle(float pwm_dutyCycle)
        {
            input_pwmDutyCycle = pwm_dutyCycle;
        }
        public float solve_diff_equation_step()
        {
            // At this point the relation between pwm and rpm comes into play.
            if (QS_ESC_DUTY_CYCLE_THRUST_RELATION_DEFAULT == QS_ESC_DUTY_CYCLE_THRUST_RELATION.QS_ESC_DUTY_CYCLE_2_RPM_LINEAR)
            {
                return mapp(input_pwmDutyCycle, MOTOR_ESC_PWM_MIN, MOTOR_ESC_PWM_MAX, MOTOR_RPM_MIN, MOTOR_RPM_MAX);
            }

            if (QS_ESC_DUTY_CYCLE_THRUST_RELATION_DEFAULT == QS_ESC_DUTY_CYCLE_THRUST_RELATION.QS_ESC_DUTY_CYCLE_2_THRUST_LINEAR)
            {
                float thrust_wanted = mapp(input_pwmDutyCycle, MOTOR_ESC_PWM_MIN, MOTOR_ESC_PWM_MAX, MOTOR_THRUST_MIN, MOTOR_THRUST_MAX);
                return (float)System.Math.Pow(thrust_wanted / MOTOR_CONSTANT, 1.0 / MOTOR_EXPONENT_Q);
            }
            
            return MOTOR_RPM_EQU;
        }
    }
}
