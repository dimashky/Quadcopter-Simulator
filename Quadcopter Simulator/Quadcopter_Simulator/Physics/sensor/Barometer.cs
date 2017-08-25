using static TripleM.Quadcopter.Physics.config;

namespace TripleM.Quadcopter.Physics
{
    class Barometer
    {
        private float calibrated_offset;
        private float calibrated_offset_sum;
        
        public Barometer()
        {
            calibrated_offset = 0.0f;
            calibrated_offset_sum = 0.0f;
        }

        // calibration
        public void read_calibration_value(float height_ideal)
        {
            float height_new_cal_val = 0;
            get_corrupted_height(ref height_new_cal_val, height_ideal);
            calibrated_offset_sum += height_new_cal_val;
        }
        public void calibrate()
        {
            calibrated_offset = calibrated_offset_sum / BAROMETER_CAL_READS;
            calibrated_offset_sum = 0.0f;
        }


        public void get_corrupted_height(ref float height_corrupted, float height_ideal)
        {
            MathNet.Numerics.Distributions.Normal distribution = new MathNet.Numerics.Distributions.Normal(0.0, 1.0);

            // add noise and an offset
            height_corrupted = height_ideal + (float)distribution.Sample() * BAROMETER_STANDEV + BAROMETER_OFFSET;

            // remove the offset from the calibration
            height_corrupted -= calibrated_offset;
        }
    }
}
