using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaterialSkin.Controls;
using System.Windows.Forms;

namespace Quadcopter_Simulator.Forms
{
    public partial class select_mode_form : MaterialForm
    {
        public select_mode_form()
        {
            InitializeComponent();
        }

        private void select_mode_form_Load(object sender, EventArgs e)
        {

        }

        private void materialRaisedButton1_Click(object sender, EventArgs e)
        {
            this.Close();
            main_form.Close();

            TripleM.Quadcopter.Graphics.Program.start = true;
        }

        private void materialRaisedButton2_Click(object sender, EventArgs e)
        {
            (new map_form()).Show(main_form);

            this.Close();
        }

        private QuadcopterSimulator main_form;
        public void Show(QuadcopterSimulator main_form)
        {
            base.Show();
            this.main_form = main_form;
        }
    }
}
