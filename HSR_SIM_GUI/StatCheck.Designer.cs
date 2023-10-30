using System;
using System.Linq;
using System.Threading;

namespace HSR_SIM_GUI
{
    partial class StatCheck
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
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            cbProfile = new System.Windows.Forms.ComboBox();
            cbScenario = new System.Windows.Forms.ComboBox();
            NmbIterations = new System.Windows.Forms.NumericUpDown();
            label3 = new System.Windows.Forms.Label();
            NmbThreadsCount = new System.Windows.Forms.NumericUpDown();
            label4 = new System.Windows.Forms.Label();
            BtnGo = new System.Windows.Forms.Button();
            PB1 = new System.Windows.Forms.ProgressBar();
            chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            lblCap1 = new System.Windows.Forms.Label();
            lblCap2 = new System.Windows.Forms.Label();
            lblWinRate = new System.Windows.Forms.Label();
            lblAv = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)NmbIterations).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NmbThreadsCount).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chart1).BeginInit();
            SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(22, 23);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(52, 15);
            label2.TabIndex = 12;
            label2.Text = "Scenario";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(143, 23);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(41, 15);
            label1.TabIndex = 11;
            label1.Text = "Profile";
            // 
            // cbProfile
            // 
            cbProfile.FormattingEnabled = true;
            cbProfile.Location = new System.Drawing.Point(143, 50);
            cbProfile.Name = "cbProfile";
            cbProfile.Size = new System.Drawing.Size(116, 23);
            cbProfile.TabIndex = 10;
            // 
            // cbScenario
            // 
            cbScenario.FormattingEnabled = true;
            cbScenario.Location = new System.Drawing.Point(17, 50);
            cbScenario.Name = "cbScenario";
            cbScenario.Size = new System.Drawing.Size(116, 23);
            cbScenario.TabIndex = 9;
            // 
            // NmbIterations
            // 
            NmbIterations.Location = new System.Drawing.Point(288, 50);
            NmbIterations.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            NmbIterations.Name = "NmbIterations";
            NmbIterations.Size = new System.Drawing.Size(76, 23);
            NmbIterations.TabIndex = 13;
            NmbIterations.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(288, 23);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(56, 15);
            label3.TabIndex = 14;
            label3.Text = "Iterations";
            // 
            // NmbThreadsCount
            // 
            NmbThreadsCount.Location = new System.Drawing.Point(390, 50);
            NmbThreadsCount.Name = "NmbThreadsCount";
            NmbThreadsCount.Size = new System.Drawing.Size(76, 23);
            NmbThreadsCount.TabIndex = 15;
            NmbThreadsCount.Value = new decimal(new int[] { 16, 0, 0, 0 });
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(390, 23);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(48, 15);
            label4.TabIndex = 16;
            label4.Text = "Threads";
            // 
            // BtnGo
            // 
            BtnGo.Location = new System.Drawing.Point(486, 46);
            BtnGo.Name = "BtnGo";
            BtnGo.Size = new System.Drawing.Size(84, 29);
            BtnGo.TabIndex = 17;
            BtnGo.Text = "GO";
            BtnGo.UseVisualStyleBackColor = true;
            BtnGo.Click += BtnGo_Click;
            // 
            // PB1
            // 
            PB1.Location = new System.Drawing.Point(12, 79);
            PB1.Name = "PB1";
            PB1.Size = new System.Drawing.Size(760, 29);
            PB1.TabIndex = 18;
            // 
            // chart1
            // 
            chartArea1.Name = "ChartArea1";
            chart1.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            chart1.Legends.Add(legend1);
            chart1.Location = new System.Drawing.Point(12, 147);
            chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            chart1.Series.Add(series1);
            chart1.Size = new System.Drawing.Size(401, 271);
            chart1.TabIndex = 19;
            chart1.Text = "chart1";
            // 
            // lblCap1
            // 
            lblCap1.AutoSize = true;
            lblCap1.Location = new System.Drawing.Point(17, 111);
            lblCap1.Name = "lblCap1";
            lblCap1.Size = new System.Drawing.Size(28, 15);
            lblCap1.TabIndex = 20;
            lblCap1.Text = "WR:";
            lblCap1.Click += lblWinRate_Click;
            // 
            // lblCap2
            // 
            lblCap2.AutoSize = true;
            lblCap2.Location = new System.Drawing.Point(117, 111);
            lblCap2.Name = "lblCap2";
            lblCap2.Size = new System.Drawing.Size(52, 15);
            lblCap2.TabIndex = 21;
            lblCap2.Text = "Total AV:";
            // 
            // lblWinRate
            // 
            lblWinRate.AutoSize = true;
            lblWinRate.Location = new System.Drawing.Point(51, 111);
            lblWinRate.Name = "lblWinRate";
            lblWinRate.Size = new System.Drawing.Size(0, 15);
            lblWinRate.TabIndex = 22;
            // 
            // lblAv
            // 
            lblAv.AutoSize = true;
            lblAv.Location = new System.Drawing.Point(175, 111);
            lblAv.Name = "lblAv";
            lblAv.Size = new System.Drawing.Size(0, 15);
            lblAv.TabIndex = 23;
            // 
            // StatCheck
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 547);
            Controls.Add(lblAv);
            Controls.Add(lblWinRate);
            Controls.Add(lblCap2);
            Controls.Add(lblCap1);
            Controls.Add(chart1);
            Controls.Add(PB1);
            Controls.Add(BtnGo);
            Controls.Add(label4);
            Controls.Add(NmbThreadsCount);
            Controls.Add(label3);
            Controls.Add(NmbIterations);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(cbProfile);
            Controls.Add(cbScenario);
            Name = "StatCheck";
            Text = "Stat check";
            Load += StatCheck_Load;
            ((System.ComponentModel.ISupportInitialize)NmbIterations).EndInit();
            ((System.ComponentModel.ISupportInitialize)NmbThreadsCount).EndInit();
            ((System.ComponentModel.ISupportInitialize)chart1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbProfile;
        private System.Windows.Forms.ComboBox cbScenario;
        private System.Windows.Forms.NumericUpDown NmbIterations;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown NmbThreadsCount;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button BtnGo;
        private System.Windows.Forms.ProgressBar PB1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.Label lblCap1;
        private System.Windows.Forms.Label lblCap2;
        private System.Windows.Forms.Label lblWinRate;
        private System.Windows.Forms.Label lblAv;
    }
}