using System;
using MaterialSkin.Controls;
using TripleM.Quadcopter.Physics;
using TripleM.Quadcopter.Graphics;

namespace Quadcopter_Simulator.Forms
{
    public partial class pausing_form : MaterialForm
    {
        public pausing_form()
        {
            InitializeComponent();
        }

        private void Button_pause_resume_Click(object sender, EventArgs e)
        {
            this.Dispose();
            TripleM.Quadcopter.Graphics.Game1.input.resume();

        }
        
        private void Button_pause_setting_Click(object sender, EventArgs e)
        {
            setting_form setting = new setting_form();
            setting.Visible = true;

        }

        private void Button_pause_restart_Click(object sender, EventArgs e)
        {
            TripleM.Quadcopter.Graphics.Game1.input.restart();
            this.Dispose();
        }

        private void Button_pause_exit_Click(object sender, EventArgs e)
        {
            this.Dispose();
            Program.game.Dispose();
        }

        private void materialRaisedButton1_Click(object sender, EventArgs e)
        {


        }

        private void materialRaisedButton2_Click(object sender, EventArgs e)
        {
            Forms.data_chart_frame data_chart = new Forms.data_chart_frame();
            data_chart.Visible = true;
        }

        private void materialRaisedButton1_Click_1(object sender, EventArgs e)
        {
            this.Dispose();
            Program.game.Exit();
            config.setDragConst(1);
            config.setSensorsAccuracy(0);
            config.SET_MASS(0.4f);
            config.set_max_tilt_angle(30.0f);
            config.CONTROLLER_TYPE = QS_CONTROLLER_TYPE.QS_CONTROLLER_TYPE_PID;
            config.flight_mode = stable_flight_mode.STABILIZE_POSITION;
            config.SET_WIND_OFFSET(0, 0);
            (new QuadcopterSimulator()).Show();
        }
    }
}
