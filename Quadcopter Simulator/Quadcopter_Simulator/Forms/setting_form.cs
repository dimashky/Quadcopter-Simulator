using System;
using MaterialSkin;
using MaterialSkin.Controls;
using static TripleM.Quadcopter.Physics.config;
using TripleM.Quadcopter.Physics;


namespace Quadcopter_Simulator
{
    public partial class setting_form : MaterialForm
    {

        private int prev_p_roll;
        private int prev_p_pitch;
        private int prev_p_yaw;
        private int prev_p_height;

        public setting_form()
        {
            InitializeComponent();

            materialCheckBox_recoil_effect.Checked = RECOIL_EFFECT;

            trackBar_P_Roll.Value = (int)((P_ROLL - 4.5f) / 10f);
            trackBar_P_pitch.Value = (int)((P_PITCH - 4.5f) / 10f);
            trackBar_P_yaw.Value = (int)((P_YAW - 1.5f) / 10f);
            trackBar_P_h.Value = (int)((P_HEIGHT - 1.0f) / 10f);

            prev_p_roll = trackBar_P_Roll.Value;
            prev_p_pitch = trackBar_P_pitch.Value;
            prev_p_yaw = trackBar_P_yaw.Value;
            prev_p_height = trackBar_P_h.Value;



            trackBar_H_spin.Value = (int)((ROTATION_SPEED[0]*20f + 10f));
            trackBar_V_spin.Value = (int)((ROTATION_SPEED[1]*20f + 10f));


            int Value = trackBar_H_spin.Value - 10;
            if ( Value == 0)
            {
                materialLabel_H_spin.Text = "No spin";
            }
            else if(Value > 0)
            {
                materialLabel_H_spin.Text = "RightSpin";
            }
            else if(Value < 0)
            {
                materialLabel_H_spin.Text = "LeftSpin";
            }

            Value = trackBar_V_spin.Value - 10;

            if (Value == 0)
            {
                materialLabel_V_spin.Text = "No spin";
            }
            else if (Value > 0)
            {
                materialLabel_V_spin.Text = "Backspin";
            }
            else if (Value < 0)
            {
                materialLabel_V_spin.Text = "Topspin";
            }



            trackBar_simulation_speed.Value = (int)SIMULATION_SPEED;

            Label_Proj_diameter.Text = "(" + ( PROJECTILE_DIAMETER * 100) + " cm)";
            Label_Proj_dt.Text = "( " + (PROJECTILE_TIMESTEP) + " s)";
            Label_Proj_Mass.Text = "( " + ( PROJECTILE_MASS * 1000) + " g)";
            Label_Proj_Velocity.Text = "( " + PROJECTILE_VELOCITY + " m/s)";

            trackBar_Proj_diameter.Value = (int)((PROJECTILE_DIAMETER - 0.1f) * 100);
            trackBar_Proj_Mass.Value = (int)(((PROJECTILE_MASS - 0.05f)/50) * 1000);
            trackBar_Proj_dt.Value = (int)(( PROJECTILE_TIMESTEP) * 1000f);
            trackBar_Proj_Velocity.Value = (int)((PROJECTILE_VELOCITY));



            if (flight_mode == stable_flight_mode.TRAJECTORY_TRACKING)
            {
                materialFlatButton_stopTracking.Enabled = true;
                panel1.Enabled = false;
                materialFlatButton_startTrajectory.Enabled = false;
            }
            else
            {
                materialFlatButton_startTrajectory.Enabled = true;
                panel1.Enabled = true;
                materialFlatButton_stopTracking.Enabled = false;
            }
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);


            if (flight_mode == stable_flight_mode.STABILIZE)
            {
                materialRadioButton_withoutHolding.Checked = true;
            }
            else if (flight_mode == stable_flight_mode.STABILIZE_POSITION)
            {
                materialRadioButton_HoldPosition.Checked = true;

            }
            else if (flight_mode == stable_flight_mode.STABILIZE_HEIGHT)
            {
                materialRadioButton_holdHight.Checked = true;
            }
            trackBar_setting_sensor.Value = (int)(sensorsAccuracyfactor * 10);
            switch (ENVIROMENT)
            {
                case ENVIROMENT_DENSITY.VACUUM:
                    materialLabel_Medium_type.Text = "(VACUUM)";
                    trackBar_setting_drag.Value = 0;
                    break;
                case ENVIROMENT_DENSITY.AIR:
                    materialLabel_Medium_type.Text = "(AIR)";
                    trackBar_setting_drag.Value = 1;
                    break;
                case ENVIROMENT_DENSITY.LIQUID_HYDROGEN:
                    materialLabel_Medium_type.Text = "(HAYDROGEN)";
                    trackBar_setting_drag.Value = 2;
                    break;
                case ENVIROMENT_DENSITY.OIL:
                    materialLabel_Medium_type.Text = "(OIL)";
                    trackBar_setting_drag.Value = 3;
                    break;
                case ENVIROMENT_DENSITY.WATER:
                    materialLabel_Medium_type.Text = "(WATER)";
                    trackBar_setting_drag.Value = 4;
                    break;
            }
            trackBar_setting_weight.Value = (int)((MASS - 0.4f) * 10);
            trackBar_setting_tilt.Value = (int)MAX_TILT_ANGLE - 20;
            trackBar_setting_windSpeed.Value = (int)(WIND_SPEED / 0.5f);
            trackBar_setting_windDirection.Value = WIND_DEGREE;
        }

        private void trackBar_setting_weight_ValueChanged(object sender, EventArgs e)
        {
            double weight = trackBar_setting_weight.Value;
            weight = weight / 10 + 0.4;
            Label_setting_weightValue.Text = "(" + weight + " Kg)";
        }

        private void trackBar_setting_pitch_ValueChanged(object sender, EventArgs e)
        {
            int pitch = trackBar_setting_tilt.Value;
            pitch = pitch + 20;
            Label_setting_maxTiltValue.Text = "(" + pitch + ")";
        }

        private void trackBar_setting_roll_ValueChanged(object sender, EventArgs e)
        {
            int tilt = trackBar_setting_tilt.Value;
            tilt = tilt + 20;
            Label_setting_maxTiltValue.Text = "(" + tilt + ")";
        }

        private void Button_setting_cancel_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void trackBar_setting_windDirection_ValueChanged(object sender, EventArgs e)
        {
            int direction = 0 + trackBar_setting_windDirection.Value;
            Label_setting_windDirectionValue.Text = "(" + direction + " deg)";
        }

        private void trackBar_setting_windSpeed_ValueChanged(object sender, EventArgs e)
        {
            float speed = 0 + trackBar_setting_windSpeed.Value*0.5f;
            Label_setting_windSpeedValue.Text = "(" + speed + " mps)";
        }

        private void Button_setting_done_Click(object sender, EventArgs e)
        {


            P_ROLL = 4.5f + trackBar_P_Roll.Value * 10f;
            P_PITCH = 4.5f + trackBar_P_pitch.Value * 10f;
            P_YAW = 1.5f + trackBar_P_yaw.Value * 10f;
            P_HEIGHT = 1.0f + trackBar_P_h.Value * 10f;

            if(trackBar_P_Roll.Value != prev_p_roll || trackBar_P_pitch.Value != prev_p_pitch || trackBar_P_yaw.Value != prev_p_yaw || trackBar_P_h.Value != prev_p_height)
            {
                RESTART = true;
            }

            ROTATION_SPEED[0] = (trackBar_H_spin.Value - 10f) / 20f;
            ROTATION_SPEED[1] = (trackBar_V_spin.Value - 10f) / 20f;




            RECOIL_EFFECT = materialCheckBox_recoil_effect.Checked;

            
            SIMULATION_SPEED = trackBar_simulation_speed.Value;

            PROJECTILE_MASS = 0.05f + (((float)trackBar_Proj_Mass.Value)*50) / 1000f;
            PROJECTILE_VELOCITY = trackBar_Proj_Velocity.Value;
            PROJECTILE_DIAMETER = 0.1f + (float)trackBar_Proj_diameter.Value / 100f;
            PROJECTILE_TIMESTEP = 0.001f + (float)trackBar_Proj_dt.Value / 1000f;
            
            if (flight_mode == stable_flight_mode.TRAJECTORY_TRACKING)
            {
                // nothing todo
            }
            else if (materialRadioButton_withoutHolding.Checked)
            {
                flight_mode = stable_flight_mode.STABILIZE;
            }
            else if (materialRadioButton_HoldPosition.Checked)
            {
                flight_mode = stable_flight_mode.STABILIZE_POSITION;
            }
            else if (materialRadioButton_holdHight.Checked)
            {
                flight_mode = stable_flight_mode.STABILIZE_HEIGHT;
            }

            
            setDragConst(trackBar_setting_drag.Value);

            SET_MASS(0.4f + ((float)trackBar_setting_weight.Value) / 10);

            set_max_tilt_angle(trackBar_setting_tilt.Value + 20);


            SET_WIND_OFFSET(trackBar_setting_windDirection.Value, trackBar_setting_windSpeed.Value * 0.5f);

            if (trackBar_setting_sensor.Value / 10f != sensorsAccuracyfactor) RESTART = true;
            setSensorsAccuracy(trackBar_setting_sensor.Value / 10f);


            this.Dispose();
        }

        private void tabPage_setting_controller_Click(object sender, EventArgs e)
        {

        }

        private void tabPage_setting_controller_Enter(object sender, EventArgs e)
        {

        }

        private void materialFlatButton_stopTracking_Click(object sender, EventArgs e)
        {
            panel1.Enabled = true;
            flight_mode = stable_flight_mode.STABILIZE_POSITION;
            materialFlatButton_stopTracking.Enabled = false;
        }

        private void materialFlatButton1_Click(object sender, EventArgs e)
        {
            (new Quadcopter_Simulator.Forms.map_form()).Show();
        }

        private void tabPage_setting_controller_MouseHover(object sender, EventArgs e)
        {

        }

        private void tabPage_setting_controller_MouseEnter(object sender, EventArgs e)
        {
            if (flight_mode == stable_flight_mode.TRAJECTORY_TRACKING)
            {
                materialFlatButton_stopTracking.Enabled = true;
                panel1.Enabled = false;
                materialFlatButton_startTrajectory.Enabled = false;
            }
            else
            {
                materialFlatButton_startTrajectory.Enabled = true;
                panel1.Enabled = true;
                materialFlatButton_stopTracking.Enabled = false;
            }
        }

        private void trackBar_Proj_Mass_Scroll(object sender, EventArgs e)
        {
            int value = 50 + (trackBar_Proj_Mass.Value)*50;
            Label_Proj_Mass.Text = "(" + value + " g)";
        }

        private void trackBar_Proj_Velocity_Scroll(object sender, EventArgs e)
        {
            int value = trackBar_Proj_Velocity.Value;
            Label_Proj_Velocity.Text = "(" + value + " m/s)";
        }

        private void trackBar_Proj_diameter_Scroll(object sender, EventArgs e)
        {
            int value = 10 + trackBar_Proj_diameter.Value;
            Label_Proj_diameter.Text = "(" + value + " cm)";
        }

        private void trackBar_Proj_dt_Scroll(object sender, EventArgs e)
        {
            float value = 0.001f + (float)trackBar_Proj_dt.Value / 1000f;
            Label_Proj_dt.Text = "(" + value + " s)";
        }

        private void materialFlatButton1_Click_1(object sender, EventArgs e)
        {
            panel_rotation.Visible = true;
        }

        private void materialFlatButton2_Click(object sender, EventArgs e)
        {
            panel_rotation.Visible = false;
        }

        private void trackBar_V_spin_Scroll(object sender, EventArgs e)
        {
            int Value = trackBar_V_spin.Value - 10;

            if (Value == 0)
            {
                materialLabel_V_spin.Text = "No spin";
            }
            else if (Value > 0)
            {
                materialLabel_V_spin.Text = "Backspin";
            }
            else if (Value < 0)
            {
                materialLabel_V_spin.Text = "Topspin";
            }
        }

        private void trackBar_H_spin_Scroll(object sender, EventArgs e)
        {
            int Value = trackBar_H_spin.Value - 10;

            if (Value == 0)
            {
                materialLabel_H_spin.Text = "No spin";
            }
            else if (Value > 0)
            {
                materialLabel_H_spin.Text = "RightSpin";
            }
            else if (Value < 0)
            {
                materialLabel_H_spin.Text = "LeftSpin";
            }
        }

        private void materialFlatButton3_Click(object sender, EventArgs e)
        {
            panel_PID_gains.Visible = true;
        }

        private void materialFlatButton4_Click(object sender, EventArgs e)
        {
            panel_PID_gains.Visible = false;
        }

        private void trackBar_setting_drag_Scroll(object sender, EventArgs e)
        {
            int type = trackBar_setting_drag.Value;
            switch (type)
            {
                case 0:
                    materialLabel_Medium_type.Text = "(VACUUM)";
                    break;
                case 1:
                    materialLabel_Medium_type.Text = "(AIR)";
                    break;
                case 2:
                    materialLabel_Medium_type.Text = "(HAYDROGEN)";
                    break;
                case 3:
                    materialLabel_Medium_type.Text = "(OIL)";
                    break;
                case 4:
                    materialLabel_Medium_type.Text = "(WATER)";
                    break;
            }
        }
    }
}
