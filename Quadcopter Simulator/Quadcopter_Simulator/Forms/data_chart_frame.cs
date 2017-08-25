using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using rtChart;
using MathNet.Numerics.LinearAlgebra;
using static TripleM.Quadcopter.Physics.config;

namespace Quadcopter_Simulator.Forms
{
    public partial class data_chart_frame : MaterialForm
    {
        public data_chart_frame()
        {
            InitializeComponent();
            kayChart pitch = new kayChart(angles_chart, 10);
            pitch.serieName = "Pitch";
            kayChart roll = new kayChart(angles_chart, 10);
            roll.serieName = "Roll";

            kayChart rpm1 = new kayChart(rpm_chart, 10);
            rpm1.serieName = "rpm1";
            kayChart rpm2 = new kayChart(rpm_chart, 10);
            rpm2.serieName = "rpm2";
            kayChart rpm3 = new kayChart(rpm_chart, 10);
            rpm3.serieName = "rpm3";
            kayChart rpm4 = new kayChart(rpm_chart, 10);
            rpm4.serieName = "rpm4";


            Task.Factory.StartNew(() =>
            {
                pitch.updateChart(updatePitch, 600);
            });

            Task.Factory.StartNew(() =>
            {
                roll.updateChart(updateRoll, 600);
            });

            Task.Factory.StartNew(() =>
            {
                rpm1.updateChart(updateRPM1, 600);
            });
            Task.Factory.StartNew(() =>
            {
                rpm2.updateChart(updateRPM2, 600);
            });
            Task.Factory.StartNew(() =>
            {
                rpm3.updateChart(updateRPM3, 600);
            });
            Task.Factory.StartNew(() =>
            {
                rpm4.updateChart(updateRPM4, 600);
            });
        }

        private double updateRPM1()
        {
            return TripleM.Quadcopter.Graphics.Game1.input.rpm[0];
        }
        private double updateRPM2()
        {
            return TripleM.Quadcopter.Graphics.Game1.input.rpm[1];
        }
        private double updateRPM3()
        {
            return TripleM.Quadcopter.Graphics.Game1.input.rpm[2];
        }
        private double updateRPM4()
        {
            return TripleM.Quadcopter.Graphics.Game1.input.rpm[3];
        }
        private double updatePitch()
        {
            return RAD2DEG(TripleM.Quadcopter.Graphics.Game1.input.quadcopter.get_attitude(1));
        }
        private double updateRoll()
        {
            return RAD2DEG(TripleM.Quadcopter.Graphics.Game1.input.quadcopter.get_attitude(0));
        }

        private double updateX()
        {
            return TripleM.Quadcopter.Graphics.Game1.input.position.X;
        }

        private double updateY()
        {
            return TripleM.Quadcopter.Graphics.Game1.input.position.Y;
        }
        private double updateZ()
        {
            return TripleM.Quadcopter.Graphics.Game1.input.position.Z;
        }

        private void rpm_chart_Click(object sender, EventArgs e)
        {

        }
    }
}
