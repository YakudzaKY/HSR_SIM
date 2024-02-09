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
            label2 = new System.Windows.Forms.Label();
            cbScenario = new System.Windows.Forms.ComboBox();
            NmbIterations = new System.Windows.Forms.NumericUpDown();
            label3 = new System.Windows.Forms.Label();
            NmbThreadsCount = new System.Windows.Forms.NumericUpDown();
            label4 = new System.Windows.Forms.Label();
            PB1 = new System.Windows.Forms.ProgressBar();
            pnlCharts = new System.Windows.Forms.Panel();
            chkProfiles = new System.Windows.Forms.CheckedListBox();
            BtnGo = new System.Windows.Forms.Button();
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            gbStatImpcat = new System.Windows.Forms.GroupBox();
            label8 = new System.Windows.Forms.Label();
            cbStatToReplace = new System.Windows.Forms.ComboBox();
            label7 = new System.Windows.Forms.Label();
            btnMainStats = new System.Windows.Forms.Button();
            btnLoadSubstats = new System.Windows.Forms.Button();
            dgStatUpgrades = new System.Windows.Forms.DataGridView();
            propClmn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            valClmn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            label6 = new System.Windows.Forms.Label();
            nmbSteps = new System.Windows.Forms.NumericUpDown();
            label5 = new System.Windows.Forms.Label();
            nmbUpgradesPerStep = new System.Windows.Forms.NumericUpDown();
            cbCharacter = new System.Windows.Forms.ComboBox();
            label1 = new System.Windows.Forms.Label();
            chkStats = new System.Windows.Forms.CheckedListBox();
            gbGearReplace = new System.Windows.Forms.GroupBox();
            button1 = new System.Windows.Forms.Button();
            btnImport = new System.Windows.Forms.Button();
            gbPlus = new System.Windows.Forms.GroupBox();
            gbMinus = new System.Windows.Forms.GroupBox();
            cbCharacterGrp = new System.Windows.Forms.ComboBox();
            label9 = new System.Windows.Forms.Label();
            panel1 = new System.Windows.Forms.Panel();
            rbGearReplace = new System.Windows.Forms.RadioButton();
            rbStatImpcat = new System.Windows.Forms.RadioButton();
            lblProgressCnt = new System.Windows.Forms.Label();
            btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)NmbIterations).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NmbThreadsCount).BeginInit();
            flowLayoutPanel1.SuspendLayout();
            gbStatImpcat.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgStatUpgrades).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nmbSteps).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nmbUpgradesPerStep).BeginInit();
            gbGearReplace.SuspendLayout();
            panel1.SuspendLayout();
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
            NmbIterations.Value = new decimal(new int[] { 1000, 0, 0, 0 });
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
            PB1.Size = new System.Drawing.Size(1141, 29);
            PB1.TabIndex = 18;
            // 
            // pnlCharts
            // 
            pnlCharts.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pnlCharts.AutoScroll = true;
            pnlCharts.Location = new System.Drawing.Point(12, 201);
            pnlCharts.Name = "pnlCharts";
            pnlCharts.Size = new System.Drawing.Size(1141, 607);
            pnlCharts.TabIndex = 20;
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
            // BtnGo
            // 
            BtnGo.Location = new System.Drawing.Point(12, 137);
            BtnGo.Name = "BtnGo";
            BtnGo.Size = new System.Drawing.Size(116, 23);
            BtnGo.TabIndex = 24;
            BtnGo.Text = "GO";
            BtnGo.UseVisualStyleBackColor = true;
            BtnGo.Click += BtnGo_Click;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(gbStatImpcat);
            flowLayoutPanel1.Controls.Add(gbGearReplace);
            flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flowLayoutPanel1.Location = new System.Drawing.Point(433, 0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(729, 314);
            flowLayoutPanel1.TabIndex = 25;
            // 
            // gbStatImpcat
            // 
            gbStatImpcat.Controls.Add(label8);
            gbStatImpcat.Controls.Add(cbStatToReplace);
            gbStatImpcat.Controls.Add(label7);
            gbStatImpcat.Controls.Add(btnMainStats);
            gbStatImpcat.Controls.Add(btnLoadSubstats);
            gbStatImpcat.Controls.Add(dgStatUpgrades);
            gbStatImpcat.Controls.Add(label6);
            gbStatImpcat.Controls.Add(nmbSteps);
            gbStatImpcat.Controls.Add(label5);
            gbStatImpcat.Controls.Add(nmbUpgradesPerStep);
            gbStatImpcat.Controls.Add(cbCharacter);
            gbStatImpcat.Controls.Add(label1);
            gbStatImpcat.Controls.Add(chkStats);
            gbStatImpcat.Location = new System.Drawing.Point(3, 3);
            gbStatImpcat.Name = "gbStatImpcat";
            gbStatImpcat.Size = new System.Drawing.Size(723, 151);
            gbStatImpcat.TabIndex = 24;
            gbStatImpcat.TabStop = false;
            gbStatImpcat.Text = "Stat impact calc";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(242, 15);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(87, 15);
            label8.TabIndex = 36;
            label8.Text = "Load stat table:";
            // 
            // cbStatToReplace
            // 
            cbStatToReplace.FormattingEnabled = true;
            cbStatToReplace.Items.AddRange(new object[] { "character" });
            cbStatToReplace.Location = new System.Drawing.Point(128, 80);
            cbStatToReplace.Name = "cbStatToReplace";
            cbStatToReplace.Size = new System.Drawing.Size(111, 23);
            cbStatToReplace.TabIndex = 35;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(128, 62);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(82, 15);
            label7.TabIndex = 34;
            label7.Text = "Stat to replace";
            // 
            // btnMainStats
            // 
            btnMainStats.Location = new System.Drawing.Point(242, 63);
            btnMainStats.Name = "btnMainStats";
            btnMainStats.Size = new System.Drawing.Size(111, 23);
            btnMainStats.TabIndex = 33;
            btnMainStats.Text = "mainstats";
            btnMainStats.UseVisualStyleBackColor = true;
            btnMainStats.Click += btnMainStats_Click;
            // 
            // btnLoadSubstats
            // 
            btnLoadSubstats.Location = new System.Drawing.Point(242, 34);
            btnLoadSubstats.Name = "btnLoadSubstats";
            btnLoadSubstats.Size = new System.Drawing.Size(111, 23);
            btnLoadSubstats.TabIndex = 32;
            btnLoadSubstats.Text = "substats";
            btnLoadSubstats.UseVisualStyleBackColor = true;
            btnLoadSubstats.Click += btnLoadSubstats_Click;
            // 
            // dgStatUpgrades
            // 
            dgStatUpgrades.AllowUserToAddRows = false;
            dgStatUpgrades.AllowUserToDeleteRows = false;
            dgStatUpgrades.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgStatUpgrades.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { propClmn, valClmn });
            dgStatUpgrades.Location = new System.Drawing.Point(487, 15);
            dgStatUpgrades.Name = "dgStatUpgrades";
            dgStatUpgrades.RowHeadersVisible = false;
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
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(6, 104);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(104, 15);
            label6.TabIndex = 29;
            label6.Text = "Upgrade iterations";
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
            label5.Size = new System.Drawing.Size(120, 15);
            label5.TabIndex = 27;
            label5.Text = "Upgrades per iteraion";
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
            cbCharacter.Size = new System.Drawing.Size(230, 23);
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
            chkStats.Location = new System.Drawing.Point(359, 15);
            chkStats.Name = "chkStats";
            chkStats.Size = new System.Drawing.Size(122, 130);
            chkStats.TabIndex = 23;
            // 
            // gbGearReplace
            // 
            gbGearReplace.Controls.Add(button1);
            gbGearReplace.Controls.Add(btnImport);
            gbGearReplace.Controls.Add(gbPlus);
            gbGearReplace.Controls.Add(gbMinus);
            gbGearReplace.Controls.Add(cbCharacterGrp);
            gbGearReplace.Controls.Add(label9);
            gbGearReplace.Location = new System.Drawing.Point(3, 160);
            gbGearReplace.Name = "gbGearReplace";
            gbGearReplace.Size = new System.Drawing.Size(723, 151);
            gbGearReplace.TabIndex = 25;
            gbGearReplace.TabStop = false;
            gbGearReplace.Text = "Gear repleace calc";
            gbGearReplace.Visible = false;
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(41, 119);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(151, 26);
            button1.TabIndex = 32;
            button1.Text = "refresh rect";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // btnImport
            // 
            btnImport.Location = new System.Drawing.Point(41, 83);
            btnImport.Name = "btnImport";
            btnImport.Size = new System.Drawing.Size(151, 35);
            btnImport.TabIndex = 31;
            btnImport.Text = "Import from screen>>";
            btnImport.UseVisualStyleBackColor = true;
            btnImport.Click += btnImport_Click;
            // 
            // gbPlus
            // 
            gbPlus.Location = new System.Drawing.Point(434, 6);
            gbPlus.Name = "gbPlus";
            gbPlus.Size = new System.Drawing.Size(200, 139);
            gbPlus.TabIndex = 29;
            gbPlus.TabStop = false;
            gbPlus.Text = "STATS EQUIP";
            // 
            // gbMinus
            // 
            gbMinus.Location = new System.Drawing.Point(198, 6);
            gbMinus.Name = "gbMinus";
            gbMinus.Size = new System.Drawing.Size(200, 139);
            gbMinus.TabIndex = 28;
            gbMinus.TabStop = false;
            gbMinus.Text = "STATS UNEQUIP";
            // 
            // cbCharacterGrp
            // 
            cbCharacterGrp.FormattingEnabled = true;
            cbCharacterGrp.Items.AddRange(new object[] { "character" });
            cbCharacterGrp.Location = new System.Drawing.Point(9, 38);
            cbCharacterGrp.Name = "cbCharacterGrp";
            cbCharacterGrp.Size = new System.Drawing.Size(165, 23);
            cbCharacterGrp.TabIndex = 27;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(9, 20);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(58, 15);
            label9.TabIndex = 26;
            label9.Text = "Character";
            // 
            // panel1
            // 
            panel1.Controls.Add(rbGearReplace);
            panel1.Controls.Add(rbStatImpcat);
            panel1.Location = new System.Drawing.Point(264, 6);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(163, 154);
            panel1.TabIndex = 26;
            // 
            // rbGearReplace
            // 
            rbGearReplace.AutoSize = true;
            rbGearReplace.Location = new System.Drawing.Point(20, 47);
            rbGearReplace.Name = "rbGearReplace";
            rbGearReplace.Size = new System.Drawing.Size(120, 19);
            rbGearReplace.TabIndex = 1;
            rbGearReplace.Text = "Gear repleace calc";
            rbGearReplace.UseVisualStyleBackColor = true;
            rbGearReplace.CheckedChanged += radioButton2_CheckedChanged;
            // 
            // rbStatImpcat
            // 
            rbStatImpcat.AutoSize = true;
            rbStatImpcat.Checked = true;
            rbStatImpcat.Location = new System.Drawing.Point(20, 19);
            rbStatImpcat.Name = "rbStatImpcat";
            rbStatImpcat.Size = new System.Drawing.Size(109, 19);
            rbStatImpcat.TabIndex = 0;
            rbStatImpcat.TabStop = true;
            rbStatImpcat.Text = "Stat impact calc";
            rbStatImpcat.UseVisualStyleBackColor = true;
            rbStatImpcat.CheckedChanged += radioButton1_CheckedChanged;
            // 
            // lblProgressCnt
            // 
            lblProgressCnt.AutoSize = true;
            lblProgressCnt.Location = new System.Drawing.Point(23, 172);
            lblProgressCnt.Name = "lblProgressCnt";
            lblProgressCnt.Size = new System.Drawing.Size(0, 15);
            lblProgressCnt.TabIndex = 27;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(134, 137);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(116, 23);
            btnCancel.TabIndex = 28;
            btnCancel.Text = "CANCEL!";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Visible = false;
            btnCancel.Click += btnCancel_Click;
            // 
            // StatCheck
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1165, 816);
            Controls.Add(btnCancel);
            Controls.Add(lblProgressCnt);
            Controls.Add(panel1);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(BtnGo);
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
            FormClosing += StatCheck_FormClosing;
            Load += StatCheck_Load;
            ((System.ComponentModel.ISupportInitialize)NmbIterations).EndInit();
            ((System.ComponentModel.ISupportInitialize)NmbThreadsCount).EndInit();
            flowLayoutPanel1.ResumeLayout(false);
            gbStatImpcat.ResumeLayout(false);
            gbStatImpcat.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgStatUpgrades).EndInit();
            ((System.ComponentModel.ISupportInitialize)nmbSteps).EndInit();
            ((System.ComponentModel.ISupportInitialize)nmbUpgradesPerStep).EndInit();
            gbGearReplace.ResumeLayout(false);
            gbGearReplace.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
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
        private System.Windows.Forms.CheckedListBox chkProfiles;
        private System.Windows.Forms.Button BtnGo;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.GroupBox gbStatImpcat;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cbStatToReplace;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnMainStats;
        private System.Windows.Forms.Button btnLoadSubstats;
        private System.Windows.Forms.DataGridView dgStatUpgrades;
        private System.Windows.Forms.DataGridViewTextBoxColumn propClmn;
        private System.Windows.Forms.DataGridViewTextBoxColumn valClmn;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown nmbSteps;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown nmbUpgradesPerStep;
        private System.Windows.Forms.ComboBox cbCharacter;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckedListBox chkStats;
        private System.Windows.Forms.GroupBox gbGearReplace;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton rbGearReplace;
        private System.Windows.Forms.RadioButton rbStatImpcat;
        private System.Windows.Forms.ComboBox cbCharacterGrp;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox gbPlus;
        private System.Windows.Forms.GroupBox gbMinus;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lblProgressCnt;
        private System.Windows.Forms.Button btnCancel;
    }
}