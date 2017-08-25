using MathNet.Numerics.LinearAlgebra;
using static TripleM.Quadcopter.Physics.config;
using static TripleM.Quadcopter.Physics.diff_equation;

namespace TripleM.Quadcopter.Physics
{
    class Accelerometer
    {
        private Vector<float> calibrated_offsets;       
        private Vector<float> calibrated_offsets_sum;   // to take the average  

        private Vector<float> chip_tilt;                

        public Accelerometer()
        {
            // init the vectors
            calibrated_offsets = Vector<float>.Build.Dense(3,0);
            calibrated_offsets_sum = Vector<float>.Build.Dense(3, 0);

            float[] tmp = { (CHIP_OFFSET_R), DEG2RAD(CHIP_OFFSET_P), DEG2RAD(CHIP_OFFSET_Y) };
            chip_tilt = Vector<float>.Build.DenseOfArray(tmp);
        }

        /* sensor calibration
        Sensor calibration: is a method of improving sensor performance by removing structural errors in the sensor outputs.  
        Structural errors: are differences between a sensors expected output and its measured output, which show up consistently every time a new measurement is taken.  
        Any of these errors that are repeatable can be calculated during calibration, so that during actual end-use the measurements made by the sensor can be compensated in real-time to digitally remove any errors.  
        Calibration provides a means of providing enhanced performance by improving the overall accuracy of the underlying sensors.
        */
        public void read_calibration_value(Vector<float> xdotdot_ideal, Vector<float> attitude_ideal)
        {
            Vector<float> xdotdot_bf_new_cal_val = Vector<float>.Build.Dense(3,0);
            get_corrupted_accelerations(ref xdotdot_bf_new_cal_val, xdotdot_ideal, attitude_ideal);
            calibrated_offsets_sum += xdotdot_bf_new_cal_val;
        }
        public void calibrate()
        {
            calibrated_offsets = calibrated_offsets_sum / ACCELEROMETER_CAL_READS;
            calibrated_offsets_sum = Vector<float>.Build.Dense(3,0);

            // remove gravity from axis z
            calibrated_offsets[2] -= GRAVITY;
        }

        // chip tilt calibration
        public void take_chip_off_quadcopter()
        {
            chip_tilt.Clear();
        }
        public void place_chip_on_quadcopter()
        {
            float[] x = { DEG2RAD(CHIP_OFFSET_R), DEG2RAD(CHIP_OFFSET_P), DEG2RAD(CHIP_OFFSET_Y) };
            chip_tilt = Vector<float>.Build.DenseOfArray(x);
        }

        // sensor reading
        public void get_corrupted_accelerations(ref Vector<float> xdotdot_bf_corrupted, Vector<float> xdotdot_ideal, Vector<float> attitude_ideal)
        {
            // The accelerations in the earth frame are perfectly known from the differential equation.
            // But the accelerometer returns the accelerations in the body frame, therefore a transformation is needed.
            // R is a transformation matrix from the body to the earth frame, so R.inverse() transforms from earth to body frame.
            Matrix<float> R = Matrix<float>.Build.Dense(3,3,0);
            rotation(ref R, attitude_ideal);
            xdotdot_bf_corrupted = R.Inverse() * xdotdot_ideal;


            // If there is no acceleration in the earth frame (xdotdot_ideal = 0), the accelerometer would still measure the gravity acceleration.
            // Therefore the gravity vector has to be added.
            float[] gravity = { 0, 0, GRAVITY };
            xdotdot_bf_corrupted = xdotdot_bf_corrupted + R.Inverse() * Vector<float>.Build.DenseOfArray(gravity);

            // add chip tilt
            rotation(ref R, chip_tilt);
            xdotdot_bf_corrupted = R.Inverse() * xdotdot_bf_corrupted;

            MathNet.Numerics.Distributions.Normal distribution = new MathNet.Numerics.Distributions.Normal(0.0,1.0); //using standard normal distribution

            // add noise and an offset
            xdotdot_bf_corrupted[0] += (float)distribution.Sample() * ACCELEROMETER_STANDEV_X + ACCELEROMETER_OFFSET_X;
            xdotdot_bf_corrupted[1] += (float)distribution.Sample() * ACCELEROMETER_STANDEV_Y + ACCELEROMETER_OFFSET_Y;
            xdotdot_bf_corrupted[2] += (float)distribution.Sample() * ACCELEROMETER_STANDEV_Z + ACCELEROMETER_OFFSET_Z;
            
            // remove the offset from the calibration
            xdotdot_bf_corrupted -= calibrated_offsets;
        }
    }
}
