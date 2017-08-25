namespace Quadcopter_Simulator.Forms
{
    partial class data_chart_frame
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series5 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series6 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(data_chart_frame));
            this.angles_chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.rpm_chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.angles_chart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rpm_chart)).BeginInit();
            this.SuspendLayout();
            // 
            // angles_chart
            // 
            chartArea1.Name = "ChartArea1";
            this.angles_chart.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.angles_chart.Legends.Add(legend1);
            this.angles_chart.Location = new System.Drawing.Point(-25, 68);
            this.angles_chart.Name = "angles_chart";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series1.Legend = "Legend1";
            series1.Name = "Pitch";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series2.Legend = "Legend1";
            series2.Name = "Roll";
            this.angles_chart.Series.Add(series1);
            this.angles_chart.Series.Add(series2);
            this.angles_chart.Size = new System.Drawing.Size(575, 313);
            this.angles_chart.TabIndex = 0;
            this.angles_chart.Text = "angles_chart";
            // 
            // rpm_chart
            // 
            chartArea2.Name = "ChartArea1";
            this.rpm_chart.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            this.rpm_chart.Legends.Add(legend2);
            this.rpm_chart.Location = new System.Drawing.Point(-25, 387);
            this.rpm_chart.Name = "rpm_chart";
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series3.Legend = "Legend1";
            series3.Name = "rpm1";
            series4.ChartArea = "ChartArea1";
            series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series4.Legend = "Legend1";
            series4.Name = "rpm2";
            series5.ChartArea = "ChartArea1";
            series5.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series5.Legend = "Legend1";
            series5.Name = "rpm3";
            series6.ChartArea = "ChartArea1";
            series6.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series6.Legend = "Legend1";
            series6.Name = "rpm4";
            this.rpm_chart.Series.Add(series3);
            this.rpm_chart.Series.Add(series4);
            this.rpm_chart.Series.Add(series5);
            this.rpm_chart.Series.Add(series6);
            this.rpm_chart.Size = new System.Drawing.Size(575, 335);
            this.rpm_chart.TabIndex = 1;
            this.rpm_chart.Text = "chart1";
            this.rpm_chart.Click += new System.EventHandler(this.rpm_chart_Click);
            // 
            // data_chart_frame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(523, 734);
            this.Controls.Add(this.rpm_chart);
            this.Controls.Add(this.angles_chart);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "data_chart_frame";
            this.Text = "Data Chart";
            ((System.ComponentModel.ISupportInitialize)(this.angles_chart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rpm_chart)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart angles_chart;
        private System.Windows.Forms.DataVisualization.Charting.Chart rpm_chart;
    }
}