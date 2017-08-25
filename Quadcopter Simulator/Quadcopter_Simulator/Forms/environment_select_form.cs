using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin.Controls;
using MaterialSkin;
using Quadcopter_Simulator;

namespace Quadcopter_Simulator.Forms
{
    public partial class environment_select_form : MaterialForm
    {

        bool viewedEnv = false;
        public static bool selectedEnv = false;
        public environment_select_form()
        {
            InitializeComponent();
            materialLabel1.Text = "Water";
            panel1.BackColor = Color.AliceBlue;
            panel1.BackgroundImage = Image.FromFile(@"Water.PNG");
        }

        private void environment_select_form_Load(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void materialFlatButton2_Click(object sender, EventArgs e)
        {
            if (viewedEnv)
            {
                materialLabel1.Text = "Water";
                viewedEnv = false;
                panel1.BackColor = Color.AliceBlue;
                panel1.BackgroundImage = Image.FromFile(@"Water.PNG");
            }
            else
            {
                materialLabel1.Text = "Mountains";
                viewedEnv = true;
                panel1.BackColor = Color.AliceBlue;
                panel1.BackgroundImage = Image.FromFile(@"Mountains.PNG");
            }
        }

        private void materialLabel1_Click(object sender, EventArgs e)
        {

        }

        private void materialFlatButton1_Click(object sender, EventArgs e)
        {
            if (!viewedEnv)
            {
                materialLabel1.Text = "Mountains";
                viewedEnv = true;
                panel1.BackColor = Color.AliceBlue;
                panel1.BackgroundImage = Image.FromFile(@"Mountains.PNG");
            }
            else
            {
                materialLabel1.Text = "Water";
                viewedEnv = false;
                panel1.BackColor = Color.AliceBlue;
                panel1.BackgroundImage = Image.FromFile(@"Water.PNG");
            }
        }

        private void materialRaisedButton1_Click(object sender, EventArgs e)
        {
            selectedEnv = viewedEnv;
            if (materialLabel1.Text.Equals("Water"))
            {
                (new select_mode_form()).Show(main_form);
                this.Close();
            }
            else MessageBox.Show("this Environment isn't Available.");
        }

        private QuadcopterSimulator main_form;
        public void Show(QuadcopterSimulator main_form)
        {
            base.Show();
            this.main_form = main_form;
        }
    }
}
