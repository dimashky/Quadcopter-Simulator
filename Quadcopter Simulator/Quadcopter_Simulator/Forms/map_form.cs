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
using MathNet.Numerics.LinearAlgebra;
using TripleM.Quadcopter.Physics;
using static TripleM.Quadcopter.Physics.config;
using System.Reflection;
using System.IO;

namespace Quadcopter_Simulator.Forms
{
    public partial class map_form : MaterialForm
    {
        List<int> yCoordinates;
        public map_form()
        {
            yCoordinates = new List<int>();
            InitializeComponent();
            panel1.BackColor = Color.AliceBlue;
            if(environment_select_form.selectedEnv)
                panel1.BackgroundImage = Image.FromFile(@"MountainsUp.png");
            else panel1.BackgroundImage = Image.FromFile(@"WaterUp.PNG");
        }
        private void map_form_Load(object sender, EventArgs e)
        {

        }
        private void trackBar1_Scroll(object sender, EventArgs e)
        {

        }

        private void map_form_MouseDown(object sender, MouseEventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DataGridViewRow[] delete = new DataGridViewRow[dataGridView1.SelectedRows.Count];
            System.Drawing.Graphics g = panel1.CreateGraphics();
            SolidBrush s = new SolidBrush(Color.Red);
            Point loc = new Point();
            dataGridView1.SelectedRows.CopyTo(delete, 0);
            for (int i = 0; i < dataGridView1.SelectedRows.Count; i++)
            {
                yCoordinates.RemoveAt(delete[i].Index);
                dataGridView1.Rows.Remove(delete[i]);
            }
            panel1.Refresh();
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                loc.X = (int)(Convert.ToDouble(dataGridView1.Rows[i].Cells[0].Value.ToString()) * 0.368);
                loc.Y = yCoordinates.ElementAt(i);
                g.FillEllipse(s, new Rectangle(loc, new Size(10, 10)));
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button.Equals(MouseButtons.Left))
            {
                System.Drawing.Graphics g = panel1.CreateGraphics();
                SolidBrush s = new SolidBrush(Color.Red);
                g.FillEllipse(s, new Rectangle(e.Location, new Size(10, 10)));
                dataGridView1.Rows.Add(e.Location.X * 2.7, 0, (374 - e.Location.Y) * 2.7);
                yCoordinates.Add(e.Location.Y);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            

        }

        private void materialRaisedButton2_Click(object sender, EventArgs e)
        {
            this.Close();

            if(main_form != null)
                (new select_mode_form()).Show(main_form);
        }

        private void materialRaisedButton1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count < 1)
            {
                MessageBox.Show("Select at least one point to start!");
                return;
            }
            MOVMENT_SPEED = trackBar_speed.Value;
            CURRENT_POINTS.Clear();
            for (int i = 0  ; i < dataGridView1.Rows.Count; ++i)
            {
                float[] points = new float[3];
                points[0] =  -1 * (float)Convert.ToDouble(dataGridView1.Rows[i].Cells[2].Value);
                points[1] =  (float)Convert.ToDouble(dataGridView1.Rows[i].Cells[0].Value);
                points[2] = 0;
                Vector<float> vec_points = Vector<float>.Build.DenseOfArray(points);
                CURRENT_POINTS.Enqueue(vec_points);
            }
            flight_mode = stable_flight_mode.TRAJECTORY_TRACKING;

            try {
                TripleM.Quadcopter.Graphics.Game1.input.quadcopter.trajectory_tracking_controller = new trajectory_tracking(CURRENT_POINTS, STAB_POSITION);
            }
            catch
            {

            }
            if (main_form != null)
            {
                TripleM.Quadcopter.Graphics.Program.start = true;
                
                main_form.Close();
            }
            else
            {
                this.Close();
            }
        }

        private QuadcopterSimulator main_form;
        public void Show(QuadcopterSimulator main_form)
        {
            base.Show();
            this.main_form = main_form;
        }
    }
}
