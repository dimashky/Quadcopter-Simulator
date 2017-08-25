using MathNet.Numerics.LinearAlgebra;
using static System.Math;
using static TripleM.Quadcopter.Physics.config;

namespace TripleM.Quadcopter.Physics
{
    class sensorFusion
    {
        //======================================================================
        //=========================== ATTRIBUTES =============================
        //======================================================================

        private Vector<float> initSensorTiltAccelSum, initSensorTiltMagneSum;
        private int initSensorTiltReadsCnt;

        private Vector<float> initAccelSum, initGyrosSum, initMagneSum, initBaromSum;
        private int initAccelReadsCnt, initGyrosReadsCnt, initMagneReadsCnt, initBaromReadsCnt;

        // memory on stack -> do not touch!
        // Arduino does no like heap memory, on windows it does no matter.
        // But it is kept for simplicity.
        private Vector<float>[] accelDataStack;
        private Vector<float>[] gyrosDataStack;
        private Vector<float>[] magneDataStack;
        private Vector<float>[] baromDataStack;


        // ring buffers will be connected to the stack memory
        ringBuffer accelData, gyrosData, magneData, baromData;

        // time between two calls
        float dT;

        // filters for roll, pitch, yaw and heightdot
        float rollPitchA_minimum;
        float rollPitchA_maximum;
        complementaryFilter[] cf;

        // accel tilt compensation in rad
        Vector<float> sensorTiltCalib;

        // gyro calibration in degrees/s
        Vector<float> gyrosCalib;

        // fused attitude and altitude
        Vector<float> RPY;
        Vector<float> RPYDot;
        float height;
        float heightDot;
        float heightDotDot;




        //======================================================================
        //=========================== METHODS =============================
        //======================================================================

        public sensorFusion()
        {
            accelDataStack = new Vector<float>[SFUS_ACCEL_BUF];
            gyrosDataStack = new Vector<float>[SFUS_GYROS_BUF];
            baromDataStack = new Vector<float>[SFUS_BAROM_BUF];
            magneDataStack = new Vector<float>[SFUS_MAGNE_BUF];
            for (int i = 0; i < 10; ++i)
            {
                accelDataStack[i] = Vector<float>.Build.Dense(3, 0);

                baromDataStack[i] = Vector<float>.Build.Dense(3, 0);

                magneDataStack[i] = Vector<float>.Build.Dense(3, 0);

                gyrosDataStack[i] = Vector<float>.Build.Dense(3, 0);
            }

            float[] tmp = { SFUS_SENSOR_TILT_R, SFUS_SENSOR_TILT_P, SFUS_SENSOR_TILT_Y };
            sensorTiltCalib = Vector<float>.Build.DenseOfArray(tmp);
            gyrosCalib = Vector<float>.Build.Dense(3, 0);


            initSensorTiltAccelSum = Vector<float>.Build.Dense(3, 0.0f);
            initSensorTiltMagneSum = Vector<float>.Build.Dense(3, 0.0f);
            initSensorTiltReadsCnt = 0;

            initAccelSum = Vector<float>.Build.Dense(3, 0.0f);
            initGyrosSum = Vector<float>.Build.Dense(3, 0.0f);
            initMagneSum = Vector<float>.Build.Dense(3, 0.0f);
            initBaromSum = Vector<float>.Build.Dense(3, 0.0f);
            initAccelReadsCnt = 0;
            initGyrosReadsCnt = 0;
            initMagneReadsCnt = 0;
            initBaromReadsCnt = 0;

            RPY = Vector<float>.Build.Dense(3, 0);
            RPYDot = Vector<float>.Build.Dense(3, 0);

            // couple ring buffers with stack memory
            accelData = new ringBuffer(ref accelDataStack, SFUS_ACCEL_BUF);
            gyrosData = new ringBuffer(ref gyrosDataStack, SFUS_GYROS_BUF);
            magneData = new ringBuffer(ref magneDataStack, SFUS_MAGNE_BUF);
            baromData = new ringBuffer(ref baromDataStack, SFUS_BAROM_BUF);

            dT = QS_SENSOR_INPUT_PERIOD / 1000000000.0f;

            // the factor tau is variable depending on quadcopter angle
            rollPitchA_minimum = SFUS_COMPF_RP_TAU_MIN / (SFUS_COMPF_RP_TAU_MIN + dT);
            rollPitchA_maximum = SFUS_COMPF_RP_TAU_MAX / (SFUS_COMPF_RP_TAU_MAX + dT);

            cf = new complementaryFilter[4];
            for (int i = 0; i < 4; ++i)
                cf[i] = new complementaryFilter();

            cf[ROLL].setTauViaA(rollPitchA_minimum);
            cf[PITCH].setTauViaA(rollPitchA_minimum);
            cf[YAW].setTauViaA(SFUS_COMPF_YAW_A);
            cf[HEIGHTDOT].setTauViaA(SFUS_COMPF_HEIGHTDOT_A);
        }

        // chip on quadcopter tilt compensation
        public bool writeSensorTiltCalibData(Vector<float> accel, Vector<float> magne)
        {
            initSensorTiltReadsCnt++;
            if (initSensorTiltReadsCnt > SFUS_SENSOR_TILT_DROPS && initSensorTiltReadsCnt <= (SFUS_SENSOR_TILT_DROPS + SFUS_SENSOR_TILT_READS))
            {
                initSensorTiltAccelSum += accel;
                initSensorTiltMagneSum += magne;
            }

            // data collection finished
            if (initSensorTiltReadsCnt >= (SFUS_SENSOR_TILT_DROPS + SFUS_SENSOR_TILT_READS))
            {
                initSensorTiltAccelSum /= SFUS_SENSOR_TILT_READS;
                initSensorTiltMagneSum /= SFUS_SENSOR_TILT_READS;

                // reset calibration to zero
                sensorTiltCalib = Vector<float>.Build.Dense(3, 0.0f);

                // quadcopter stands flat on the ground and is directed to magnetic north
                RPY = Vector<float>.Build.Dense(3, 0.0f);

                // calculate roll and pitch compensation from accelerometer
                sensorTiltCalib = getEulerAnglesFromAccel(initSensorTiltAccelSum);
                sensorTiltCalib[2] = 0.0f;

                // compensate magnetometer roll and pitch tilt on quadcopter
                Vector<float> initSensorTiltMagneSumRPCompensated = getVectorFromBody2EarthFrame(initSensorTiltMagneSum, sensorTiltCalib);

                // calculate yaw tilt of magnetometer relative to quadcopter
                sensorTiltCalib[2] = getEulerYawFromMagne(initSensorTiltMagneSumRPCompensated);

                return true;
            }

            return false;
        }

        // initial attitude estimation
        public bool writeInitData(Vector<float> accel, Vector<float> gyros, Vector<float> magne, Vector<float> barom)
        {
            initAccelReadsCnt++;
            if (initAccelReadsCnt > SFUS_INIT_ACCEL_DROPS && initAccelReadsCnt <= (SFUS_INIT_ACCEL_DROPS + SFUS_INIT_ACCEL_READS))
                initAccelSum += accel;

            initGyrosReadsCnt++;
            if (initGyrosReadsCnt > SFUS_INIT_GYROS_DROPS && initGyrosReadsCnt <= (SFUS_INIT_GYROS_DROPS + SFUS_INIT_GYROS_READS))
                initGyrosSum += gyros;

            initMagneReadsCnt++;
            if (initMagneReadsCnt > SFUS_INIT_MAGNE_DROPS && initMagneReadsCnt <= (SFUS_INIT_MAGNE_DROPS + SFUS_INIT_MAGNE_READS))
                initMagneSum += magne;

            initBaromReadsCnt++;
            if (initBaromReadsCnt > SFUS_INIT_BAROM_DROPS && initBaromReadsCnt <= (SFUS_INIT_BAROM_DROPS + SFUS_INIT_BAROM_READS))
                initBaromSum += barom;

            // data collection finished
            if (initAccelReadsCnt >= (SFUS_INIT_ACCEL_DROPS + SFUS_INIT_ACCEL_READS)
                && initGyrosReadsCnt >= (SFUS_INIT_GYROS_DROPS + SFUS_INIT_GYROS_READS)
                    && initMagneReadsCnt >= (SFUS_INIT_MAGNE_DROPS + SFUS_INIT_MAGNE_READS)
                        && initBaromReadsCnt >= (SFUS_INIT_BAROM_DROPS + SFUS_INIT_BAROM_READS))
            {
                initAccelSum /= SFUS_INIT_ACCEL_READS;
                initGyrosSum /= SFUS_INIT_GYROS_READS;
                initMagneSum /= SFUS_INIT_MAGNE_READS;
                initBaromSum /= SFUS_INIT_BAROM_READS;

                // compensate chip tilt
                initAccelSum = getVectorFromBody2EarthFrame(initAccelSum, sensorTiltCalib);
                initMagneSum = getVectorFromBody2EarthFrame(initMagneSum, sensorTiltCalib);

                // determine initial values
                RPY = getEulerAnglesFromAccel(initAccelSum);
                RPY[2] = getEulerYawFromMagne(initMagneSum);
                RPYDot = Vector<float>.Build.Dense(3, 0);
                height = initBaromSum[2];
                heightDot = 0.0f;
                heightDotDot = 0.0f;



                // the gyroscope is recalibrated at each start up
                gyrosCalib = initGyrosSum;




                // push values into buffers
                for (int i = 0; i < SFUS_ACCEL_BUF; i++) accelData.pushNewElem(initAccelSum);
                for (int i = 0; i < SFUS_GYROS_BUF; i++) gyrosData.pushNewElem(initGyrosSum);
                for (int i = 0; i < SFUS_MAGNE_BUF; i++) magneData.pushNewElem(initMagneSum);
                for (int i = 0; i < SFUS_BAROM_BUF; i++) baromData.pushNewElem(initBaromSum);

                // init complementary filter A/Tau factor
                float currentA = (float)getTiltProportionalA();
                cf[ROLL].setTauViaA(currentA);
                cf[PITCH].setTauViaA(currentA);

                // init complementary filter angles
                cf[ROLL].setCombinedEstimation(RPY[0]);
                cf[PITCH].setCombinedEstimation(RPY[1]);
                cf[YAW].setCombinedEstimation(RPY[2]);
                cf[HEIGHTDOT].setCombinedEstimation(heightDot);

                return true;
            }

            return false;
        }

        // input of raw sensor data
        public void writeData(Vector<float> accel, Vector<float> gyros, Vector<float> magne, Vector<float> barom)
        {
            // compensate chip tilt for all sensors
            accel = getVectorFromBody2EarthFrame(accel, sensorTiltCalib);
            gyros = getRatesFromBody2EarthFrame(gyros - gyrosCalib, sensorTiltCalib);
            magne = getVectorFromBody2EarthFrame(magne, sensorTiltCalib);

            // push tilt compensated values to container
            accelData.pushNewElem(accel);
            gyrosData.pushNewElem(gyros);
            magneData.pushNewElem(magne);
            baromData.pushNewElem(barom);

            // transform input from sensors into earth frame
            Vector<float> accelEulertmp = getEulerAnglesFromAccel(accel);
            Vector<float> gyrosEulerRatestmp = getRatesFromBody2EarthFrame(gyros, RPY);
            Vector<float> accelAccelerationtmp = getVectorFromBody2EarthFrame(accel, RPY);

            // adjust complementary filter
            float currentA = getTiltProportionalA();
            cf[ROLL].setTauViaA(currentA);
            cf[PITCH].setTauViaA(currentA);

            // filtering for roll and pitch
            RPY[0] = cf[ROLL].getCombinedEstimation(accelEulertmp[0], gyrosEulerRatestmp[0]);
            RPY[1] = cf[PITCH].getCombinedEstimation(accelEulertmp[1], gyrosEulerRatestmp[1]);
            RPY[2] = cf[YAW].getCombinedEstimation(getEulerYawFromMagne(magne), gyrosEulerRatestmp[2]);

            // yaw is +/- Pi
            RPY[2] = wrap_Pi(RPY[2]);
            cf[YAW].setCombinedEstimation(RPY[2]);

            // rotation rates from calibrated gyroscope
            RPYDot = gyrosEulerRatestmp;

            // apply moving average filter for height
            height = 0.0f;
            float height_old = 0.0f;
            for (int i = 0; i < SFUS_MA_HEIGHT; i++)
            {
                Vector<float> tmp = Vector<float>.Build.Dense(10, 0);

                baromData.getNthElem(ref tmp, i);
                height += tmp[2];

                baromData.getNthElem(ref tmp, i + 1);
                height_old += tmp[2];
            }
            height /= SFUS_MA_HEIGHT;
            height_old /= SFUS_MA_HEIGHT;

            // vertical speed
            heightDot = cf[HEIGHTDOT].getCombinedEstimation((height - height_old) / dT, accelAccelerationtmp[2] - GRAVITY);

            // vertical acceleration
            heightDotDot = accelAccelerationtmp[2] - GRAVITY;
        }

        public Vector<float> getRPY()
        {
            return RPY;
        }
        public Vector<float> getRPYDot()
        {
            return RPYDot;
        }
        public float getHeight()
        {
            return height;
        }
        public float getHeightDot()
        {
            return heightDot;
        }
        public float getHeightDotDot()
        {
            return heightDotDot;
        }


        // transformation functions
        private float getTiltProportionalA()
        {
            float T_total = RAD2DEG((float)(Acos(Cos(RPY[0]) * Cos(RPY[1]))));
            T_total = constrainn(T_total, 0.0f, SFUS_COMPF_RP_LIM_TILT);

            return mapp(T_total, 0.0f, SFUS_COMPF_RP_LIM_TILT, rollPitchA_minimum, rollPitchA_maximum);
        }
        private Vector<float> getEulerAnglesFromAccel(Vector<float> accelBodyFrame)
        {
            // Source: https://cache.freescale.com/files/sensors/doc/app_note/AN3461.pdf
            float roll_accel = (float)((accelBodyFrame[2] != 0.0f) ? Atan(accelBodyFrame[1] / accelBodyFrame[2]) : 0.0f);
            float tmp = (float)Sqrt(accelBodyFrame[1] * accelBodyFrame[1] + accelBodyFrame[2] * accelBodyFrame[2]);
            float pitch_accel = (float)((tmp != 0.0) ? Atan(-accelBodyFrame[0] / tmp) : 0.0);

            float[] t = { roll_accel, pitch_accel, 0.0f };
            return Vector<float>.Build.DenseOfArray(t);
        }
        private float getEulerYawFromMagne(Vector<float> magneBodyFrame)
        {
            // tilt compensation without yaw
            // Source: http://www.chrobotics.com/library/understanding-euler-angles
            // Source: http://cache.freescale.com/files/sensors/doc/app_note/AN4248.pdf

            float[] t = { RPY[0], RPY[1], 0.0f };
            Vector<float> magneEarthFrame = getVectorFromBody2EarthFrame(magneBodyFrame, Vector<float>.Build.DenseOfArray(t));

            // Source: https://www.adafruit.com/datasheets/AN203_Compass_Heading_Using_Magnetometers.pdf
            float orientation_tmp = 0;
            if (magneEarthFrame[1] == 0.0)
            {
                if (magneEarthFrame[0] > 0.0)
                    orientation_tmp = 0.0f;
                else if (magneEarthFrame[0] < 0.0)
                    orientation_tmp = (float)PI;
            }
            else if (magneEarthFrame[1] > 0.0) // pitch are not equal zero so we have errors and we should compensate it
            {
                orientation_tmp = (float)(PI / 2.0 - Atan(magneEarthFrame[0] / magneEarthFrame[1]));
            }
            else if (magneEarthFrame[2] < 0.0) // pitch are not equal zero so we have errors and we should compensate it
            {
                orientation_tmp = (float)(1.5 * PI - Atan(magneEarthFrame[0] / magneEarthFrame[1]));
            }

            // transform from 0-2*Pi to +/-Pi
            return wrap_Pi(-1 * orientation_tmp);
        }
        private Vector<float> getVectorFromBody2EarthFrame(Vector<float> vecInBodyFrame, Vector<float> attitude)
        {
            // The angles are negative, but the minuses are itegrated into the matrix.
            // Source: http://www.chrobotics.com/library/understanding-euler-angles
            float cos_roll = (float)Cos(attitude[0]);
            float sin_roll = (float)Sin(attitude[0]);
            float cos_pitch = (float)Cos(attitude[1]);
            float sin_pitch = (float)Sin(attitude[1]);
            float cos_yaw = (float)Cos(attitude[2]);
            float sin_yaw = (float)Sin(attitude[2]);

            Vector<float> vecInEarthFrame = Vector<float>.Build.Dense(3, 0.0f);

            // vecInEarthFrame = R * vecInBodyFrame

            vecInEarthFrame[0] = cos_yaw * cos_pitch * vecInBodyFrame[0]    +     (cos_yaw * sin_pitch * sin_roll - sin_yaw * cos_roll) * vecInBodyFrame[1]   +  (cos_yaw * sin_pitch * cos_roll + sin_yaw * sin_roll) * vecInBodyFrame[2];
            vecInEarthFrame[1] = sin_yaw * cos_pitch * vecInBodyFrame[0]    +     (sin_yaw * sin_pitch * sin_roll + cos_yaw * cos_roll) * vecInBodyFrame[1]   +  (sin_yaw * sin_pitch * cos_roll - cos_yaw * sin_roll) * vecInBodyFrame[2];
            vecInEarthFrame[2] = -sin_pitch * vecInBodyFrame[0]             +      cos_pitch * sin_roll * vecInBodyFrame[1]                                   +  cos_pitch * cos_roll * vecInBodyFrame[2];

            return vecInEarthFrame;
        }
        private Vector<float> getRatesFromBody2EarthFrame(Vector<float> ratesInBodyFrame, Vector<float> attitude)
        {
            /* Transform the rates from the body frame into the earth frame.
	         * To do so, you need to know the current roll and pitch.
	         * The angles are negative, but the minuses are itegrated in the matrix.
	         * Can be ignored in a small angle approximation.
	         * Source: http://www.chrobotics.com/library/understanding-euler-angles
	         * Source: http://sal.aalto.fi/publications/pdf-files/eluu11_public.pdf
	         */

            float sin_roll = (float)Sin(attitude[0]);
            float cos_roll = (float)Cos(attitude[0]);
            float tan_pitch = (float)Tan(attitude[1]);
            float cos_pitch = (float)Cos(attitude[1]);

            Vector<float> ratesInEarthFrame = Vector<float>.Build.Dense(3,0.0f);

            // ratesInEarthFrame = Wn^-1 * ratesInBodyFrame

            ratesInEarthFrame[0] = ratesInBodyFrame[0]                         +     sin_roll * tan_pitch * ratesInBodyFrame[1]     +      cos_roll * tan_pitch * ratesInBodyFrame[2];
            ratesInEarthFrame[1] = 0                                           +     cos_roll * ratesInBodyFrame[1]                 -      sin_roll * ratesInBodyFrame[2];
            ratesInEarthFrame[2] = 0                                           +     sin_roll / cos_pitch * ratesInBodyFrame[2]     +      cos_roll / cos_pitch * ratesInBodyFrame[2];

            return ratesInEarthFrame;
        }
    }
}
