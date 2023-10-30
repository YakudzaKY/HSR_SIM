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
            label1 = new System.Windows.Forms.Label();
            cbProfile = new System.Windows.Forms.ComboBox();
            cbScenario = new System.Windows.Forms.ComboBox();
            NmbIterations = new System.Windows.Forms.NumericUpDown();
            label3 = new System.Windows.Forms.Label();
            NmbThreadsCount = new System.Windows.Forms.NumericUpDown();
            label4 = new System.Windows.Forms.Label();
            BtnGo = new System.Windows.Forms.Button();
            PB1 = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)NmbIterations).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NmbThreadsCount).BeginInit();
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
            NmbIterations.Name = "NmbIterations";
            NmbIterations.Size = new System.Drawing.Size(76, 23);
            NmbIterations.TabIndex = 13;
            NmbIterations.Value = new decimal(new int[] { 10, 0, 0, 0 });
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
            NmbThreadsCount.Value = new decimal(new int[] { 8, 0, 0, 0 });
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
            // StatCheck
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
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
    }
}