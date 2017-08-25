using static TripleM.Quadcopter.Physics.config;

namespace TripleM.Quadcopter.Physics
{
    class complementaryFilter
    {
        // combined estimatation of both inputs
        private float combinedEstimation;

        // time between two samples
        private float dT;

        // relationship: a = tau/(tau+dT) respectively tau = a*dT/(1-a)
        private float tau;
        private float a;


        public complementaryFilter()
        {
            // starting point has to be set via 'setCombinedEstimation()'
            combinedEstimation = 0.0f;

            // the loop time is fixed during runtime
            dT = QS_SENSOR_INPUT_PERIOD / 1000000000.0f;

            // this setting means: trust the linear input, mistrust the derivative
            setTauViaA(1.0f);
        }

        // set starting point
        public void setCombinedEstimation(float combEstim)
        {
            combinedEstimation = combEstim;
        }

        // extract the most recent estimation
        public float getCombinedEstimation(float estimation, float estimation_derivative)
        {
            combinedEstimation = a * (combinedEstimation + estimation_derivative * dT) + (1.0f - a) * estimation;

            return combinedEstimation;
        }

        // Get and set parameter 'a' respectively 'tau'.
        // 0 <= tau < +infinity
        // 0 <= a <= 1
        public float getTau()
        {
            return tau;
        }
        public void setTau(float tau_arg)
        {
            tau = tau_arg;
            a = tau / (tau + dT);
        }
        public void setTauViaA(float a_arg)
        {
            a = a_arg;
            tau = (a != 1.0f) ? a * dT / (1.0f - a) : COMPLEMENTARY_FILTER_TAU_UNDEFINED;
        }
        public float getA()
        {
            return a;
        }
    }
}
