using MathNet.Numerics.LinearAlgebra;
using static TripleM.Quadcopter.Physics.config;
using static TripleM.Quadcopter.Physics.diff_equation;

namespace TripleM.Quadcopter.Physics
{
    class Gyroscope
    {

        private Vector<float> calibrated_offsets;
        private Vector<float> calibrated_offsets_sum;

        private Vector<float> chip_tilt;

        public Gyroscope()
        {
            float[] x = { DEG2RAD(CHIP_OFFSET_R), DEG2RAD(CHIP_OFFSET_P), DEG2RAD(CHIP_OFFSET_Y) };
            calibrated_offsets = Vector<float>.Build.Dense(3,0);
            calibrated_offsets_sum = Vector<float>.Build.Dense(3, 0);
            chip_tilt = Vector<float>.Build.DenseOfArray(x);
        }

        // calibration
        public void read_calibration_value(Vector<float> xdotdot_ideal, Vector<float> attitude_ideal)
        {
            Vector<float> thetadot_bf_new_cal_val = Vector<float>.Build.Dense(3,0);
            get_corrupted_angveloc(ref thetadot_bf_new_cal_val, xdotdot_ideal, attitude_ideal);
            calibrated_offsets_sum += thetadot_bf_new_cal_val;
        }
        void calibrate()
        {
            calibrated_offsets = calibrated_offsets_sum / GYROSCOPE_CAL_READS;
            calibrated_offsets_sum = Vector<float>.Build.Dense(3,0);
        }

        // chip tilt calibration
        public void take_chip_off_quadcopter()
        {
            chip_tilt = Vector<float>.Build.Dense(3, 0);
        }
        public void place_chip_on_quadcopter()
        {
            float[] x = { DEG2RAD(CHIP_OFFSET_R), DEG2RAD(CHIP_OFFSET_P), DEG2RAD(CHIP_OFFSET_Y) };
            chip_tilt = Vector<float>.Build.DenseOfArray(x);
        }

        public void get_corrupted_angveloc(ref Vector<float> thetadot_bf_corrupted, Vector<float> thetadot_ideal, Vector<float> attitude_ideal)
        {
            // The angular velocities in the earth frame are perfectly known from the differential equation.
            // But the gyroscope returns angular accelerations in the body frame, therefore a transformation is needed.
            // The easiest way is to use the functions for the differential equations.
            thetadot2omega(ref thetadot_bf_corrupted, thetadot_ideal, attitude_ideal);

            // add chip tilt
            thetadot2omega(ref thetadot_bf_corrupted, thetadot_bf_corrupted,chip_tilt);

            MathNet.Numerics.Distributions.Normal distribution = new MathNet.Numerics.Distributions.Normal(0.0, 1.0);

            // add noise and an offset
            thetadot_bf_corrupted[0] += (float)distribution.Sample() * DEG2RAD(GYROSCOPE_STANDEV_R) + DEG2RAD(GYROSCOPE_OFFSET_R);
            thetadot_bf_corrupted[1] += (float)distribution.Sample() * DEG2RAD(GYROSCOPE_STANDEV_P) + DEG2RAD(GYROSCOPE_OFFSET_P);
            thetadot_bf_corrupted[2] += (float)distribution.Sample() * DEG2RAD(GYROSCOPE_STANDEV_Y) + DEG2RAD(GYROSCOPE_OFFSET_Y);

            // remove the offset from the calibration
            thetadot_bf_corrupted -= calibrated_offsets;
        }
    }
}
