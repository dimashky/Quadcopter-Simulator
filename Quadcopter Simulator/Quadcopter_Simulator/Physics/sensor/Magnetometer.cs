using static TripleM.Quadcopter.Physics.config;
using static TripleM.Quadcopter.Physics.diff_equation;
using MathNet.Numerics.LinearAlgebra;

namespace TripleM.Quadcopter.Physics
{
    class Magnetometer
    {
        private Vector<float> calibrated_offsets;
        private Vector<float> calibrated_offsets_sum;

        private Vector<float> chip_tilt;

        private Vector<float> magneticEarthFieldVector;

        public Magnetometer()
        {
            float[] m1 = { DEG2RAD(CHIP_OFFSET_R), DEG2RAD(CHIP_OFFSET_P), DEG2RAD(CHIP_OFFSET_Y) };
            float[] m2 = { MAGFIELD_EARTH_X, MAGFIELD_EARTH_Y, MAGFIELD_EARTH_Z };
            calibrated_offsets = Vector<float>.Build.Dense(3, 0);
            calibrated_offsets_sum = Vector<float>.Build.Dense(3, 0);
            chip_tilt = Vector<float>.Build.DenseOfArray(m1);
            magneticEarthFieldVector = Vector<float>.Build.DenseOfArray(m2);
        }

        // calibration
        public void read_calibration_value(Vector<float> attitude_ideal)
        {
            Vector<float> magField_new_cal_val = Vector<float>.Build.Dense(3, 0);
            get_corrupted_MagneticVectorBodyFrame(ref magField_new_cal_val, attitude_ideal);
            calibrated_offsets_sum += magField_new_cal_val;
        }
        public void calibrate()
        {
            calibrated_offsets = calibrated_offsets_sum / MAGNETOMETER_CAL_READS;
            calibrated_offsets -= magneticEarthFieldVector;
            calibrated_offsets_sum = Vector<float>.Build.Dense(3, 0);
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

        public void get_corrupted_MagneticVectorBodyFrame(ref Vector<float> magField_bf_corrupted, Vector<float> attitude_ideal)
        {
            Matrix<float> R = Matrix<float>.Build.Dense(3,3,0);
            rotation(ref R, attitude_ideal);

            // from earth to body frame
            magField_bf_corrupted = R.Inverse() * magneticEarthFieldVector;

            // add chip tilt
            rotation(ref R, chip_tilt);
            magField_bf_corrupted = R.Inverse() * (magField_bf_corrupted);

            MathNet.Numerics.Distributions.Normal distribution = new MathNet.Numerics.Distributions.Normal(0.0, 1.0);

            magField_bf_corrupted[0] += (float)distribution.Sample() * MAGNETOMETER_OFFSET_X + MAGNETOMETER_STANDEV_X;
            magField_bf_corrupted[1] += (float)distribution.Sample() * MAGNETOMETER_OFFSET_Y + MAGNETOMETER_STANDEV_Y;
            magField_bf_corrupted[2] += (float)distribution.Sample() * MAGNETOMETER_OFFSET_Z + MAGNETOMETER_STANDEV_Z;
        }
    }
}
