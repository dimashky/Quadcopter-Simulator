using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using Quadcopter_Simulator.Forms;

namespace Quadcopter_Simulator
{
    public partial class QuadcopterSimulator : MaterialForm
    {
        public QuadcopterSimulator()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TripleM.Quadcopter.Graphics.Program.start = true;
            this.Close();
        }

        private void QuadcopterSimulator_Load(object sender, EventArgs e)
        {
            
        }

        private void materialRaisedButton1_Click(object sender, EventArgs e)
        {
            (new environment_select_form()).Show(this);

            this.Hide();

            
  //          Program.start = true;

//            this.Close();
        }

        private void materialRaisedButton2_Click(object sender, EventArgs e)
        {
            setting_form setting = new setting_form();
            setting.Visible = true;
        }

        private void materialFlatButton1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("He");
        }

        private void materialRaisedButton4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void materialRaisedButton3_Click(object sender, EventArgs e)
        {
            (new About_us_form()).Show();
        }

        private void materialRaisedButton5_Click(object sender, EventArgs e)
        {
            (new map_form()).Show();
        }

        private void materialRaisedButton6_Click(object sender, EventArgs e)
        {
            (new environment_select_form()).Show();
        }
    }
}
