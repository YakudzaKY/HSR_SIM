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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.DataPoint dataPoint1 = new System.Windows.Forms.DataVisualization.Charting.DataPoint(1D, "300,0");
            System.Windows.Forms.DataVisualization.Charting.DataPoint dataPoint2 = new System.Windows.Forms.DataVisualization.Charting.DataPoint(2D, "333,0");
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            label2 = new System.Windows.Forms.Label();
            cbScenario = new System.Windows.Forms.ComboBox();
            NmbIterations = new System.Windows.Forms.NumericUpDown();
            label3 = new System.Windows.Forms.Label();
            NmbThreadsCount = new System.Windows.Forms.NumericUpDown();
            label4 = new System.Windows.Forms.Label();
            PB1 = new System.Windows.Forms.ProgressBar();
            pnlCharts = new System.Windows.Forms.Panel();
            chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            dgStatUpgrades = new System.Windows.Forms.DataGridView();
            propClmn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            valClmn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            chkProfiles = new System.Windows.Forms.CheckedListBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label6 = new System.Windows.Forms.Label();
            nmbSteps = new System.Windows.Forms.NumericUpDown();
            label5 = new System.Windows.Forms.Label();
            nmbUpgradesPerStep = new System.Windows.Forms.NumericUpDown();
            cbCharacter = new System.Windows.Forms.ComboBox();
            label1 = new System.Windows.Forms.Label();
            chkStats = new System.Windows.Forms.CheckedListBox();
            BtnGo = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)NmbIterations).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NmbThreadsCount).BeginInit();
            pnlCharts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chart1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgStatUpgrades).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nmbSteps).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nmbUpgradesPerStep).BeginInit();
            SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(12, 9);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(52, 15);
            label2.TabIndex = 12;
            label2.Text = "Scenario";
            // 
            // cbScenario
            // 
            cbScenario.FormattingEnabled = true;
            cbScenario.Location = new System.Drawing.Point(12, 27);
            cbScenario.Name = "cbScenario";
            cbScenario.Size = new System.Drawing.Size(116, 23);
            cbScenario.TabIndex = 9;
            // 
            // NmbIterations
            // 
            NmbIterations.Location = new System.Drawing.Point(12, 71);
            NmbIterations.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            NmbIterations.Name = "NmbIterations";
            NmbIterations.Size = new System.Drawing.Size(116, 23);
            NmbIterations.TabIndex = 13;
            NmbIterations.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(12, 53);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(56, 15);
            label3.TabIndex = 14;
            label3.Text = "Iterations";
            // 
            // NmbThreadsCount
            // 
            NmbThreadsCount.Location = new System.Drawing.Point(12, 115);
            NmbThreadsCount.Name = "NmbThreadsCount";
            NmbThreadsCount.Size = new System.Drawing.Size(116, 23);
            NmbThreadsCount.TabIndex = 15;
            NmbThreadsCount.Value = new decimal(new int[] { 16, 0, 0, 0 });
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(12, 97);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(48, 15);
            label4.TabIndex = 16;
            label4.Text = "Threads";
            // 
            // PB1
            // 
            PB1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            PB1.Location = new System.Drawing.Point(12, 166);
            PB1.Name = "PB1";
            PB1.Size = new System.Drawing.Size(776, 29);
            PB1.TabIndex = 18;
            // 
            // pnlCharts
            // 
            pnlCharts.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pnlCharts.AutoScroll = true;
            pnlCharts.Controls.Add(chart1);
            pnlCharts.Location = new System.Drawing.Point(12, 201);
            pnlCharts.Name = "pnlCharts";
            pnlCharts.Size = new System.Drawing.Size(776, 607);
            pnlCharts.TabIndex = 20;
            // 
            // chart1
            // 
            chartArea1.AxisX.Title = "adsfsdf";
            chartArea1.Name = "ChartArea1";
            chartArea2.Name = "ChartArea2";
            chart1.ChartAreas.Add(chartArea1);
            chart1.ChartAreas.Add(chartArea2);
            legend1.Name = "Legend1";
            legend2.LegendStyle = System.Windows.Forms.DataVisualization.Charting.LegendStyle.Column;
            legend2.Name = "Legend2";
            chart1.Legends.Add(legend1);
            chart1.Legends.Add(legend2);
            chart1.Location = new System.Drawing.Point(33, 42);
            chart1.Name = "chart1";
            chart1.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.Chocolate;
            chart1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Bar;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            dataPoint1.Label = "ssd";
            dataPoint1.MarkerColor = System.Drawing.Color.White;
            dataPoint2.Label = "dssdsd";
            series1.Points.Add(dataPoint1);
            series1.Points.Add(dataPoint2);
            series1.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Double;
            series1.YValuesPerPoint = 2;
            series2.ChartArea = "ChartArea2";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series2.Legend = "Legend1";
            series2.Name = "Series2";
            chart1.Series.Add(series1);
            chart1.Series.Add(series2);
            chart1.Size = new System.Drawing.Size(658, 300);
            chart1.TabIndex = 22;
            chart1.Text = "chart1";
            title1.Name = "Title1";
            title1.Text = "dgfhdfghfgh";
            chart1.Titles.Add(title1);
            chart1.Visible = false;
            // 
            // dgStatUpgrades
            // 
            dgStatUpgrades.AllowUserToAddRows = false;
            dgStatUpgrades.AllowUserToDeleteRows = false;
            dgStatUpgrades.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgStatUpgrades.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { propClmn, valClmn });
            dgStatUpgrades.Location = new System.Drawing.Point(291, 15);
            dgStatUpgrades.Name = "dgStatUpgrades";
            dgStatUpgrades.RowHeadersVisible = false;
            dgStatUpgrades.RowTemplate.Height = 25;
            dgStatUpgrades.Size = new System.Drawing.Size(218, 127);
            dgStatUpgrades.TabIndex = 31;
            // 
            // propClmn
            // 
            propClmn.HeaderText = "Property";
            propClmn.Name = "propClmn";
            propClmn.ReadOnly = true;
            // 
            // valClmn
            // 
            valClmn.HeaderText = "Value";
            valClmn.Name = "valClmn";
            // 
            // chkProfiles
            // 
            chkProfiles.FormattingEnabled = true;
            chkProfiles.Location = new System.Drawing.Point(134, 9);
            chkProfiles.Name = "chkProfiles";
            chkProfiles.Size = new System.Drawing.Size(120, 148);
            chkProfiles.TabIndex = 21;
            chkProfiles.ItemCheck += chkProfiles_ItemCheck;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(dgStatUpgrades);
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(nmbSteps);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(nmbUpgradesPerStep);
            groupBox1.Controls.Add(cbCharacter);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(chkStats);
            groupBox1.Location = new System.Drawing.Point(260, 6);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(528, 151);
            groupBox1.TabIndex = 23;
            groupBox1.TabStop = false;
            groupBox1.Text = "Stat weight calc";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(6, 104);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(35, 15);
            label6.TabIndex = 29;
            label6.Text = "Steps";
            // 
            // nmbSteps
            // 
            nmbSteps.Location = new System.Drawing.Point(6, 122);
            nmbSteps.Name = "nmbSteps";
            nmbSteps.Size = new System.Drawing.Size(116, 23);
            nmbSteps.TabIndex = 28;
            nmbSteps.Value = new decimal(new int[] { 4, 0, 0, 0 });
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(6, 62);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(102, 15);
            label5.TabIndex = 27;
            label5.Text = "Upgrades per step";
            // 
            // nmbUpgradesPerStep
            // 
            nmbUpgradesPerStep.Location = new System.Drawing.Point(6, 80);
            nmbUpgradesPerStep.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            nmbUpgradesPerStep.Name = "nmbUpgradesPerStep";
            nmbUpgradesPerStep.Size = new System.Drawing.Size(116, 23);
            nmbUpgradesPerStep.TabIndex = 26;
            nmbUpgradesPerStep.Value = new decimal(new int[] { 4, 0, 0, 0 });
            // 
            // cbCharacter
            // 
            cbCharacter.FormattingEnabled = true;
            cbCharacter.Items.AddRange(new object[] { "character" });
            cbCharacter.Location = new System.Drawing.Point(6, 36);
            cbCharacter.Name = "cbCharacter";
            cbCharacter.Size = new System.Drawing.Size(128, 23);
            cbCharacter.TabIndex = 25;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 18);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(58, 15);
            label1.TabIndex = 24;
            label1.Text = "Character";
            // 
            // chkStats
            // 
            chkStats.FormattingEnabled = true;
            chkStats.Location = new System.Drawing.Point(153, 15);
            chkStats.Name = "chkStats";
            chkStats.Size = new System.Drawing.Size(122, 130);
            chkStats.TabIndex = 23;
            // 
            // BtnGo
            // 
            BtnGo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            BtnGo.Location = new System.Drawing.Point(351, 169);
            BtnGo.Name = "BtnGo";
            BtnGo.Size = new System.Drawing.Size(84, 23);
            BtnGo.TabIndex = 24;
            BtnGo.Text = "GO";
            BtnGo.UseVisualStyleBackColor = true;
            BtnGo.Click += BtnGo_Click;
            // 
            // StatCheck
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 816);
            Controls.Add(BtnGo);
            Controls.Add(groupBox1);
            Controls.Add(chkProfiles);
            Controls.Add(pnlCharts);
            Controls.Add(PB1);
            Controls.Add(label4);
            Controls.Add(NmbThreadsCount);
            Controls.Add(label3);
            Controls.Add(NmbIterations);
            Controls.Add(label2);
            Controls.Add(cbScenario);
            Name = "StatCheck";
            Text = "Stat check";
            Load += StatCheck_Load;
            ((System.ComponentModel.ISupportInitialize)NmbIterations).EndInit();
            ((System.ComponentModel.ISupportInitialize)NmbThreadsCount).EndInit();
            pnlCharts.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)chart1).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgStatUpgrades).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nmbSteps).EndInit();
            ((System.ComponentModel.ISupportInitialize)nmbUpgradesPerStep).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbScenario;
        private System.Windows.Forms.NumericUpDown NmbIterations;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown NmbThreadsCount;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ProgressBar PB1;
        private System.Windows.Forms.Panel pnlCharts;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.CheckedListBox chkProfiles;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown nmbSteps;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown nmbUpgradesPerStep;
        private System.Windows.Forms.ComboBox cbCharacter;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckedListBox chkStats;
        private System.Windows.Forms.Button BtnGo;
        private System.Windows.Forms.DataGridView dgStatUpgrades;
        private System.Windows.Forms.DataGridViewTextBoxColumn propClmn;
        private System.Windows.Forms.DataGridViewTextBoxColumn valClmn;
    }
}