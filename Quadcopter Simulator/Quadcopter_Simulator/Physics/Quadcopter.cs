using MathNet.Numerics.LinearAlgebra;
using Quadcopter_Simulator;
using System.Threading;
using static TripleM.Quadcopter.Physics.config;
using static TripleM.Quadcopter.Physics.diff_equation;

namespace TripleM.Quadcopter.Physics
{
    public class Quadcopter
    {
        // =================================================================
        // ========================== Attributes ===========================
        // =================================================================

        /* 
		 * These vectors describe the exact state of the quadcopter. They are advanced with every single timestep (dt).
		 * They should NOT be used by the controller directly because that would mean PERFECT knowledge of the quadcopter.
		 * Shared values are used as mutexed variables for sharing with other threads.
		 *
		 */
        // NOTE: HERE WE USED 'PHYSICS FRAME'
        // Quadcopter: x -> position, theta -> euler angle
        private Vector<float> x, x_shared;                   // position
        private Vector<float> xdot, xdot_shared;             // linear velocity  
        private Vector<float> xdotdot;                       // linear acceleration
        private Vector<float> theta, theta_shared;           // euler angles    
        private Vector<float> thetadot;                      // angular velocity of euler angles    
        // Motor
        private Vector<float> pwmDutyCycle;                  // current motor command 'PWM' signals
        private Vector<float> rpm, rpm_shared;               // current motor speed in 'RPM'
 

        /* 
		 * This variables define the quadcopter mechanics and electronics.
         * Frame mode configuration is (X) mode by default.
		 * 
		 */
        private QS_FRAME_MODE frame_mode;
        private Matrix<float> Inertia;
        private esc_motor escMotor0;
        private esc_motor escMotor1;
        private esc_motor escMotor2;
        private esc_motor escMotor3;


        /*
		 * These values are send by the USER. They describe the desired state of the quadcopter.
		 * For user input the keyboard is used.
		 * If there is no input, the quadcopter should HOLD position.
		 * 
		 */
        private receiver receiver;
        private Vector<float> theta_user;   // represented in pwm signals 
        private float throttle_user;        // represented in pwm signals     


        /*
		 * These values will be read from the sensors.
		 * The sensors take the ideal attitude and velocity values from above, transform them if needed and ADD noise and offsets.
		 * 
		 */
        private Accelerometer accel;
        private Gyroscope gyros;
        private Magnetometer magne;
        private Barometer barom;
        private Vector<float> xdotdot_bf_sensor;         // from accelerometer (accelerations along x, y and z axis in the body frame (bf))
        private Vector<float> thetadot_bf_sensor;        // from gyroscope (angular velocities in the body frame)
        private Vector<float> magneticField_bf_sensor;   // from magnetometer
        private float height_barom_sensor;               // from barometer (height is the same in the body and earth frame)


        /*
         * These values will be read from the sensor fusion.
         * Data refers To the earth frame (ef).
         * 
         */
        private sensorFusion sf;
        private Vector<float> theta_ef_sensor_fusion;
        private Vector<float> thetadot_ef_sensor_fusion;
        private float height_ef_sensor_fusion;
        private float heightdot_ef_sensor_fusion;
        private float heightdotdot_ef_sensor_fusion;

        /*
		 * The stabilizer outputs the pwm duty cycle (motor command).
		 * 
		 */
        private stabilizer stabilizer;



        // simulation thread parameters
        Thread simulation_thread;
        Mutex simulation_mutex;
        Mutex simulation_variables_mutex;
        bool thread_running;



        // Trajectory Tracking controller
        public trajectory_tracking trajectory_tracking_controller;







        // =================================================================
        // ========================== METHODS ==============================
        // =================================================================






        /*
         *
         *       Constructor
         *
         */
        public Quadcopter()
        {
            // init frame mode
            frame_mode = QS_FRAME_MODE_DEFAULT;

            // initialize thread variables
            thread_running = false;

            // instanciate thread variable
            simulation_mutex = new Mutex(true);
            simulation_variables_mutex = new Mutex(true);


            x = Vector<float>.Build.Dense(3, 0);
            x_shared = Vector<float>.Build.Dense(3, 0);
            xdot = Vector<float>.Build.Dense(3, 0);
            xdot_shared = Vector<float>.Build.Dense(3, 0);
            xdotdot = Vector<float>.Build.Dense(3, 0);
            theta = Vector<float>.Build.Dense(3, 0);
            theta_shared = Vector<float>.Build.Dense(3, 0);
            thetadot = Vector<float>.Build.Dense(3, 0);
            theta_user = Vector<float>.Build.Dense(3, 0);

            pwmDutyCycle = Vector<float>.Build.Dense(4, 0);
            rpm = Vector<float>.Build.Dense(4, 0);
            rpm_shared = Vector<float>.Build.Dense(4, 0);

            Inertia = Matrix<float>.Build.Dense(3, 3, 0);

            escMotor0 = new esc_motor();
            escMotor1 = new esc_motor();
            escMotor2 = new esc_motor();
            escMotor3 = new esc_motor();

            accel = new Accelerometer();
            gyros = new Gyroscope();
            magne = new Magnetometer();
            barom = new Barometer();
            xdotdot_bf_sensor = Vector<float>.Build.Dense(3, 0);
            thetadot_bf_sensor = Vector<float>.Build.Dense(3, 0);
            magneticField_bf_sensor = Vector<float>.Build.Dense(3, 0);


            sf = new sensorFusion();
            theta_ef_sensor_fusion = Vector<float>.Build.Dense(3, 0);
            thetadot_ef_sensor_fusion = Vector<float>.Build.Dense(3, 0); ;

            receiver = new receiver();

            stabilizer = new stabilizer();



            STAB_POSITION[2] = Z_START;
            STAB_POSITION[0] = X_START;
            STAB_POSITION[1] = Y_START;



            trajectory_tracking_controller = new trajectory_tracking(CURRENT_POINTS, STAB_POSITION);

        }










        /*
         *
         *      Simulation Methods
         *
         */
        public bool startSimulation()
        {
            if (simulationRunning())
                return true;

            calibrate_sensors();
            setInitState();
            attitute_height_estimation_before_takeoff();

            // init stabilizer
            stabilizer.setInitYawLock(theta_ef_sensor_fusion[2]);
            stabilizer.setInitHeightLock(height_ef_sensor_fusion);

            // now the simulation can be started in an EXTRA thread
            thread_running = true;
            simulation_thread = new Thread(new ThreadStart(simulationLoop));
            simulation_thread.Start();

            return true;
        }
        public bool stopSimulation()
        {
            lock (simulation_mutex)
            {
                thread_running = false;
            }

            simulation_thread.Abort();
            return true;
        }
        public bool simulationRunning()
        {
            bool ret_val_temp;
            lock (simulation_mutex)
            {
                ret_val_temp = thread_running;
            }

            return ret_val_temp;
        }
        public bool pauseSimulation()
        {
            simulation_thread.Abort();
            receiver.block_receiver(true);
            return true;
        }
        public bool resumeSimulation()
        {
            simulation_thread = new Thread(new ThreadStart(simulationLoop));
            simulation_thread.Start();

            receiver.block_receiver(false);
            return true;
        }
        public float get_thetadot(int x)
        {
            return thetadot[x];
        }







        /*
         *
         *      GET Methods to extract attitude and other data of quadcopter
         *
         */

        public Vector<float> get_position_vector()
        {
            return x;
        }
        public float get_position(int index)
        {
            float tmp;
            lock (simulation_variables_mutex)
            {
                tmp = x_shared[index];
            }

            return tmp;
        }
        public Vector<float> get_velocity_vector()
        {
            return xdot;
        }
        public float get_speed(int index)
        {
            float tmp;
            lock (simulation_variables_mutex)
            {
                tmp = xdot_shared[index];
            }
            return tmp;
        }
        public float get_attitude(int index)
        {
            float tmp;
            lock (simulation_variables_mutex)
            {
                tmp = theta_shared[index];
            }
            return tmp;
        }
        public Vector<float> get_attitude()
        {
            Vector<float> tmp;
            lock (simulation_variables_mutex)
            {
                tmp = theta_shared;
            }
            return tmp;
        }
        public Vector<float> get_motor_rpms()
        {
            Vector<float> tmp;
            lock (simulation_variables_mutex)
            {
                tmp = rpm;
            }
            return tmp;
        }
        public float get_motor_rpm(int index)
        {
            float tmp;
            lock (simulation_variables_mutex)
            {
                tmp = rpm_shared[index];
            }
            return tmp;
        }






        /*
         *
         *      Functions to manipulate quadcopter status
         *
         */

        public void setZeroState()
        {
            x.Clear();
            xdot.Clear();
            xdotdot.Clear();
            theta.Clear();
            thetadot.Clear();

            pwmDutyCycle.Clear();

            rpm.Clear();

            calc_inertia_matrix(ref Inertia, CENTRAL_MASS, CENTRAL_MASS_RADIUS, MOTOR_MASS, LENGTH_ARM);
        }
        public void setInitState()
        {
            // set the initial state of the quadcopter as defined in "config.cs"
            x[0] = x_shared[0] = X_START;
            x[1] = x_shared[1] = Y_START;
            x[2] = x_shared[2] = Z_START;

            xdot.Clear();
            xdotdot.Clear();

            theta[0] = theta_shared[0] = DEG2RAD(ROLL_START);
            theta[1] = theta_shared[1] = DEG2RAD(PITCH_START);
            theta[2] = theta_shared[2] = wrap_Pi(DEG2RAD(YAW_START));

            thetadot.Clear();

            calc_inertia_matrix(ref Inertia, CENTRAL_MASS, CENTRAL_MASS_RADIUS, MOTOR_MASS, LENGTH_ARM);
        }









        /*
         *
         *      Startup functions
         *
         */
        public void calibrate_sensors()
        {
            // bring the quadcopter into the default position
            setZeroState();

            // Sensor Calibration:
            // Accelerometer chip: horizontal position, calibrate so that only gravitation vector (g) is shown
            // Magnetometer chip: horizontal position and directed to north, calibrate so yaw becomes zero (psi = 0).
            // Gyroscope chip: is calibrated at each start up by sensor fusion.
            // Barometer chip: does no need to be calibrated since absolute height is irrelevant.
            magne.take_chip_off_quadcopter();
            accel.take_chip_off_quadcopter();
            gyros.take_chip_off_quadcopter();

            for (int i = 0; i < MAGNETOMETER_CAL_READS; i++)
                magne.read_calibration_value(theta);

            magne.calibrate();

            for (int i = 0; i < ACCELEROMETER_CAL_READS; i++)
                accel.read_calibration_value(xdotdot, theta);

            accel.calibrate();

            // Chip on Quadcopter offset calibration:
            // quadcopter: motors in horizontal position and quadcopter directed to north, calibrate the offset between sensor chips and quadcopter frame
            magne.place_chip_on_quadcopter();
            accel.place_chip_on_quadcopter();
            gyros.place_chip_on_quadcopter();
            do
            {
                read_sensors();
            } while (!sf.writeSensorTiltCalibData(xdotdot_bf_sensor, magneticField_bf_sensor));
        }
        public void attitute_height_estimation_before_takeoff()
        {
            // As in reality, the quadcopter starts from the standstill, so no velocity and no acceleration (linear and angular).
            // From this position it can calculate it's initial attitude.
            // Since the accelerometer is calibrated, this estimation should be very good.
            // Often, the calibration attitude and the start position are almost the same.

            // initialize init_corrupted vectors for (Accelerometer, Gyroscope and Magnetometer)>
            Vector<float> init_accel_corrupted = Vector<float>.Build.Dense(3, 0);
            Vector<float> init_gyros_corrupted = Vector<float>.Build.Dense(3, 0);
            Vector<float> init_magne_corrupted = Vector<float>.Build.Dense(3, 0);
            Vector<float> condition = Vector<float>.Build.Dense(3, 0);
            float init_height_corrupted = 0;

            // read sensors until sensor fusion says it's done!
            do
            {
                accel.get_corrupted_accelerations(ref init_accel_corrupted, xdotdot, theta);
                gyros.get_corrupted_angveloc(ref init_gyros_corrupted, thetadot, theta);
                magne.get_corrupted_MagneticVectorBodyFrame(ref init_magne_corrupted, theta);
                barom.get_corrupted_height(ref init_height_corrupted, x[2]);

                condition[2] = init_height_corrupted;

            } while (sf.writeInitData(init_accel_corrupted, init_gyros_corrupted, init_magne_corrupted, condition) != true);

            // extract data in earth frame and save it!
            theta_ef_sensor_fusion = sf.getRPY();
            thetadot_ef_sensor_fusion = sf.getRPYDot();
            height_ef_sensor_fusion = sf.getHeight();
            heightdot_ef_sensor_fusion = sf.getHeightDot();
            heightdotdot_ef_sensor_fusion = sf.getHeightDotDot();
        }












        /*
         *
         *      Functions to use the subelements (Reciever, Controller, Sensors and Sensor Fusion) 
         *
         */
        public void read_receiver()
        {
            receiver.get_desired_theta(ref theta_user);
            receiver.get_desired_throttle(ref throttle_user);
        }
        public void read_controller()
        {
            bool stable_position = true;
            if (flight_mode == stable_flight_mode.TRAJECTORY_TRACKING)
            {
                float r = 0, p = 0;
                bool check = trajectory_tracking_controller.claculation(ref p, ref r, theta[2], x);
                if (check)
                {
                    System.Windows.Forms.MessageBox.Show("Trajectory Finished! Free mode activated!");
                    STAB_POSITION[0] = x[0];
                    STAB_POSITION[1] = x[1];
                    STAB_POSITION[2] = x[2];
                    flight_mode = stable_flight_mode.STABILIZE_HEIGHT;
                }
                theta_user[0] = r;
                theta_user[1] = p;
            }
            else if (flight_mode == stable_flight_mode.STABILIZE_POSITION)
            {
                if ((theta_user[0] == RECEIVER_PWM_ZERO_SIGNAL || theta_user[0] - 1 < 0) && (theta_user[1] == RECEIVER_PWM_ZERO_SIGNAL || theta_user[1] - 1 < 0) && (throttle_user == getPWMinPointOfEquilibirum() || throttle_user - 1 < 0))
                {
                    float r = 0, p = 0;
                    trajectory_tracking.position_control(ref p, ref r, theta[2], x, STAB_POSITION);
                    theta_user[0] = r;
                    theta_user[1] = p;
                }
                else
                {
                    stable_position = false;
                    STAB_POSITION[0] = x[0];
                    STAB_POSITION[1] = x[1];
                    STAB_POSITION[2] = x[2];

                }
            }
            stabilizer.compute_pwmDutyCycle(ref (pwmDutyCycle),
                                                theta_ef_sensor_fusion,           // attitude
                                                thetadot_ef_sensor_fusion,        // angular velocity
                                                height_ef_sensor_fusion,          // height
                                                heightdot_ef_sensor_fusion,       // vertical speed
                                                heightdotdot_ef_sensor_fusion,    // vertical acceleration
                                                theta_user[0],                    // user roll
                                                theta_user[1],                    // user pitch
                                                theta_user[2],                    // user yaw
                                                throttle_user,                    // user thrust
                                                stable_position);
        }

        public void read_sensors()
        {
            // give the sensors the ideal values, they return a REALISTIC value with an offset and noise.
            accel.get_corrupted_accelerations(ref (xdotdot_bf_sensor), xdotdot, theta);
            gyros.get_corrupted_angveloc(ref (thetadot_bf_sensor), thetadot, theta);
            magne.get_corrupted_MagneticVectorBodyFrame(ref (magneticField_bf_sensor), theta);
            barom.get_corrupted_height(ref height_barom_sensor, x[2]);
        }

        public void read_sensorFusion()
        {
            // After reading sensors we will write new sensor data to sensor fusion
            Vector<float> t = Vector<float>.Build.Dense(3, 0); t[2] = height_barom_sensor;
            sf.writeData(xdotdot_bf_sensor, thetadot_bf_sensor, magneticField_bf_sensor, t);

            // extract data in earth frame and save it!
            theta_ef_sensor_fusion = sf.getRPY();
            thetadot_ef_sensor_fusion = sf.getRPYDot();
            height_ef_sensor_fusion = sf.getHeight();
            heightdot_ef_sensor_fusion = sf.getHeightDot();
            heightdotdot_ef_sensor_fusion = sf.getHeightDotDot();
        }












        /*
         *
         *      Function to advance simulation and update states
         *
         */

        public void solve_diff_equation(long time_delta_simulation, long period)
        {
            // here we solve the equation for the quadcopter as a rigid body and the equations for the motors

            
            // Add wind effect, simply we add some additional value to linear velocity  
            float[] tmp = { WIND_OFFSET_X, WIND_OFFSET_Y, WIND_OFFSET_Z };
            Vector<float> Wind_offset = Vector<float>.Build.DenseOfArray(tmp) * 10f;
           

            if ((Graphics.Game1.input.InTerrain(x[1], x[0]) == 0) && (!Graphics.Game1.input.collisionDetection(x[1], x[2], x[0])) && FIRING && RECOIL_EFFECT)
            {
                FIRING = false;
                xdot[1] += RECOIL_VELOCITY[0];
                xdot[2] += RECOIL_VELOCITY[2];
                xdot[0] -= RECOIL_VELOCITY[1];
            }
        
            // refresh input signals for motors
            escMotor0.esc_set_inputPWMDutyCycle(pwmDutyCycle[0]);
            escMotor1.esc_set_inputPWMDutyCycle(pwmDutyCycle[1]);
            escMotor2.esc_set_inputPWMDutyCycle(pwmDutyCycle[2]);
            escMotor3.esc_set_inputPWMDutyCycle(pwmDutyCycle[3]);

            /* ADVANCE EACH MOTOR */
            rpm[0] = escMotor0.solve_diff_equation_step();
            rpm[1] = escMotor1.solve_diff_equation_step();
            rpm[2] = escMotor2.solve_diff_equation_step();
            rpm[3] = escMotor3.solve_diff_equation_step();

            if (POWER_OFF)
            {
                rpm.Clear();
            }

            // convert from nanoseconds in integer to seconds in floats to calculate timestep
            float d_dt = (time_delta_simulation) / ((float)1e9);

            if(ENVIROMENT_DENSITY.AIR == config.ENVIROMENT)
                calc_drag_constant(x[2]);

            for (int i = 0; i < period / time_delta_simulation; i++)
            {

                /** ADVANCE RIGID BODY **/
                /* Linear movement */

                //1- compute linear accelerations and get xdotdot
                acceleration(ref xdotdot, rpm, theta, xdot, MASS, GRAVITY, MOTOR_CONSTANT, DRAG_CONSTANT , x[2], xdot);
                // advance system state
                //2- compute linear velocity (first derivation of velocity is acceleration) and get xdot
                xdot = xdot + d_dt * xdotdot;
                //3- Check collision detection.
                if ((Graphics.Game1.input.InTerrain(x[1] + SIMULATION_SPEED * d_dt * (xdot[1] + Wind_offset[1] + d_dt * xdotdot[1]), x[0] + SIMULATION_SPEED * d_dt * (xdot[0]+ Wind_offset[0] + SIMULATION_SPEED * d_dt * xdotdot[0])) == 0) && (!Graphics.Game1.input.collisionDetection(x[1] + SIMULATION_SPEED * d_dt * (xdot[1] + Wind_offset[1] + d_dt * xdotdot[1]), x[2] + SIMULATION_SPEED * d_dt * (xdot[2] + Wind_offset[2] + d_dt * xdotdot[2]), x[0] + SIMULATION_SPEED * d_dt * (xdot[0] + Wind_offset[0] + d_dt * xdotdot[0]))))
                {
                        //4- No collision, so update x (first derivation of position is velocity)
                        x = x + SIMULATION_SPEED * d_dt * (xdot + Wind_offset);
                }
                else
                {
                    xdot *= -0.1f;
                }


                /* Angular movement */
                Vector<float> omega = Vector<float>.Build.Dense(3, 0);
                Vector<float> omegadot = Vector<float>.Build.Dense(3, 0);
                //1- get angular velocity in bf from euler angles velocity
                thetadot2omega(ref omega, thetadot, theta);
                //2- compute angular accelerations
                angular_acceleration(ref omegadot, rpm, omega, Inertia, LENGTH_ARM, TORQUE_YAW_CONSTANT, MOTOR_CONSTANT, QS_FRAME_MODE_DEFAULT);

                // advance system state
                omega = omega + d_dt * omegadot;
                omega2thetadot(ref thetadot, omega, theta);
                theta = theta + d_dt * thetadot;
            }



            // write to shared variables
            x_shared = x;
            theta_shared = theta;
            xdot_shared = xdot;
            for (int i = 0; i < 4; i++)
                rpm_shared[i] = rpm[i];

        }








        /*
         *
         *      Actual loop for simulation
         *
         */
        public void simulationLoop()
        {
            // timer used to align real time and simulation time
            TimeClassWrapper.Timer Timer = new TimeClassWrapper.Timer();

            while (simulationRunning() && Timer.get_simulationTimeElapsed() < (ulong)QS_SIMULATION_END)
            {
                Timer.set_nextState();
                switch (Timer.get_currState())
                {
                    case 0:
                        read_receiver();
                        break;

                    case 1:
                        read_sensors();
                        read_sensorFusion();
                        read_controller();
                        break;

                    case 2:
                        solve_diff_equation(QS_TIME_DELTA, (long)Timer.get_simulationTime2compute());
                        break;
                }
            }
            stopSimulation();
        }
    }
}
