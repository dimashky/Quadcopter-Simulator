using System;

namespace TripleM.Quadcopter.Physics
{
    class PID
    {
        // time between two calls of the filter (timp step)
        private float dT;

        // gains for P, I and D
        private float kp, ki, kd;

        // needed for I
        private float errorIntegral;
        private float iTermLimit;          // integral wind-up security

        // needed for D
        private float lastError;

        public PID()
        {
            // fixed during runtime
            dT = config.QS_SENSOR_INPUT_PERIOD / 1000000000.0f;    // 0.005

            setGains(0.0f, 0.0f, 0.0f);

            zeroErrorIntegral();
            setiTermLimit(0.0f);

            lastError = 0.0f;
        }
        public void setGains(float kp_arg, float ki_arg, float kd_arg)
        {
            kp = kp_arg;
            ki = ki_arg;
            kd = kd_arg;
        }
        public void setiTermLimit(float iTermLimit_arg)
        {
            iTermLimit = Math.Abs(iTermLimit_arg);
        }
        public void zeroErrorIntegral()
        {
            errorIntegral = 0.0f;
        }

        public float compute(float target, float current)
        {
            
            // error, integral of error and derivative of error
            float error = target - current;
            errorIntegral += error * dT;
            float errorDerivative = (error - lastError) / dT;


            // PID values
            float pTerm = kp * error;
            float iTerm = ki * errorIntegral;
            float dTerm = kd * errorDerivative;

            // limit iTerm
            iTerm = (iTerm > iTermLimit) ? iTermLimit : iTerm;
            iTerm = (iTerm < -iTermLimit) ? -iTermLimit : iTerm;

            // save error for next call
            lastError = error;

            if (config.CONTROLLER_TYPE == QS_CONTROLLER_TYPE.QS_CONTROLLER_TYPE_PD)
                iTerm = 0;

            if (config.CONTROLLER_TYPE == QS_CONTROLLER_TYPE.QS_CONTROLLER_TYPE_PI)
                dTerm = 0;

            if (config.CONTROLLER_TYPE == QS_CONTROLLER_TYPE.QS_CONTROLLER_TYPE_P)
                dTerm = iTerm = 0;


                return pTerm + iTerm + dTerm;
        }

        public float getErrorIntegral()
        {
            return errorIntegral;
        }
    }
}
