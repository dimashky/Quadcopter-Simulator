namespace Quadcopter_Simulator.Forms
{
    partial class pausing_form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(pausing_form));
            this.Button_pause_resume = new MaterialSkin.Controls.MaterialRaisedButton();
            this.Button_pause_setting = new MaterialSkin.Controls.MaterialRaisedButton();
            this.Button_pause_restart = new MaterialSkin.Controls.MaterialRaisedButton();
            this.materialRaisedButton2 = new MaterialSkin.Controls.MaterialRaisedButton();
            this.materialRaisedButton1 = new MaterialSkin.Controls.MaterialRaisedButton();
            this.SuspendLayout();
            // 
            // Button_pause_resume
            // 
            this.Button_pause_resume.Depth = 0;
            this.Button_pause_resume.Location = new System.Drawing.Point(137, 92);
            this.Button_pause_resume.MouseState = MaterialSkin.MouseState.HOVER;
            this.Button_pause_resume.Name = "Button_pause_resume";
            this.Button_pause_resume.Primary = true;
            this.Button_pause_resume.Size = new System.Drawing.Size(100, 60);
            this.Button_pause_resume.TabIndex = 0;
            this.Button_pause_resume.Text = "Resume";
            this.Button_pause_resume.UseVisualStyleBackColor = true;
            this.Button_pause_resume.Click += new System.EventHandler(this.Button_pause_resume_Click);
            // 
            // Button_pause_setting
            // 
            this.Button_pause_setting.Depth = 0;
            this.Button_pause_setting.Location = new System.Drawing.Point(137, 158);
            this.Button_pause_setting.MouseState = MaterialSkin.MouseState.HOVER;
            this.Button_pause_setting.Name = "Button_pause_setting";
            this.Button_pause_setting.Primary = true;
            this.Button_pause_setting.Size = new System.Drawing.Size(100, 43);
            this.Button_pause_setting.TabIndex = 1;
            this.Button_pause_setting.Text = "Setting";
            this.Button_pause_setting.UseVisualStyleBackColor = true;
            this.Button_pause_setting.Click += new System.EventHandler(this.Button_pause_setting_Click);
            // 
            // Button_pause_restart
            // 
            this.Button_pause_restart.Depth = 0;
            this.Button_pause_restart.Location = new System.Drawing.Point(243, 158);
            this.Button_pause_restart.MouseState = MaterialSkin.MouseState.HOVER;
            this.Button_pause_restart.Name = "Button_pause_restart";
            this.Button_pause_restart.Primary = true;
            this.Button_pause_restart.Size = new System.Drawing.Size(100, 43);
            this.Button_pause_restart.TabIndex = 3;
            this.Button_pause_restart.Text = "Restart";
            this.Button_pause_restart.UseVisualStyleBackColor = true;
            this.Button_pause_restart.Click += new System.EventHandler(this.Button_pause_restart_Click);
            // 
            // materialRaisedButton2
            // 
            this.materialRaisedButton2.Depth = 0;
            this.materialRaisedButton2.Location = new System.Drawing.Point(31, 158);
            this.materialRaisedButton2.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialRaisedButton2.Name = "materialRaisedButton2";
            this.materialRaisedButton2.Primary = true;
            this.materialRaisedButton2.Size = new System.Drawing.Size(100, 43);
            this.materialRaisedButton2.TabIndex = 5;
            this.materialRaisedButton2.Text = "Charts";
            this.materialRaisedButton2.UseVisualStyleBackColor = true;
            this.materialRaisedButton2.Click += new System.EventHandler(this.materialRaisedButton2_Click);
            // 
            // materialRaisedButton1
            // 
            this.materialRaisedButton1.Depth = 0;
            this.materialRaisedButton1.Location = new System.Drawing.Point(137, 207);
            this.materialRaisedButton1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialRaisedButton1.Name = "materialRaisedButton1";
            this.materialRaisedButton1.Primary = true;
            this.materialRaisedButton1.Size = new System.Drawing.Size(100, 60);
            this.materialRaisedButton1.TabIndex = 6;
            this.materialRaisedButton1.Text = "Exit";
            this.materialRaisedButton1.UseVisualStyleBackColor = true;
            this.materialRaisedButton1.Click += new System.EventHandler(this.materialRaisedButton1_Click_1);
            // 
            // pausing_form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(373, 312);
            this.ControlBox = false;
            this.Controls.Add(this.materialRaisedButton1);
            this.Controls.Add(this.materialRaisedButton2);
            this.Controls.Add(this.Button_pause_restart);
            this.Controls.Add(this.Button_pause_setting);
            this.Controls.Add(this.Button_pause_resume);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "pausing_form";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pause";
            this.ResumeLayout(false);

        }

        #endregion

        private MaterialSkin.Controls.MaterialRaisedButton Button_pause_resume;
        private MaterialSkin.Controls.MaterialRaisedButton Button_pause_setting;
        private MaterialSkin.Controls.MaterialRaisedButton Button_pause_restart;
        private MaterialSkin.Controls.MaterialRaisedButton materialRaisedButton2;
        private MaterialSkin.Controls.MaterialRaisedButton materialRaisedButton1;
    }
}