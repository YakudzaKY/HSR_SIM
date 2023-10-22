namespace HSR_SIM_GUI
{
    partial class WarGear
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
            ImportAPI = new System.Windows.Forms.Button();
            AvatarBox = new System.Windows.Forms.PictureBox();
            dgStats = new System.Windows.Forms.DataGridView();
            propClmn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            valClmn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            label1 = new System.Windows.Forms.Label();
            txtLvl = new System.Windows.Forms.NumericUpDown();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            txtRank = new System.Windows.Forms.NumericUpDown();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            dgSkills = new System.Windows.Forms.DataGridView();
            dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            SaveXML = new System.Windows.Forms.Button();
            chAutoSave = new System.Windows.Forms.CheckBox();
            label6 = new System.Windows.Forms.Label();
            button1 = new System.Windows.Forms.Button();
            LightCone = new System.Windows.Forms.Label();
            txtLC = new System.Windows.Forms.TextBox();
            label7 = new System.Windows.Forms.Label();
            dgSets = new System.Windows.Forms.DataGridView();
            dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            txtLCRank = new System.Windows.Forms.NumericUpDown();
            label8 = new System.Windows.Forms.Label();
            txtLcLevel = new System.Windows.Forms.NumericUpDown();
            label9 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)AvatarBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgStats).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtLvl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtRank).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgSkills).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgSets).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtLCRank).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtLcLevel).BeginInit();
            SuspendLayout();
            // 
            // ImportAPI
            // 
            ImportAPI.Location = new System.Drawing.Point(12, 406);
            ImportAPI.Name = "ImportAPI";
            ImportAPI.Size = new System.Drawing.Size(122, 32);
            ImportAPI.TabIndex = 0;
            ImportAPI.Text = "Import from API";
            ImportAPI.UseVisualStyleBackColor = true;
            ImportAPI.Click += Import_Click;
            // 
            // AvatarBox
            // 
            AvatarBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            AvatarBox.Location = new System.Drawing.Point(12, 12);
            AvatarBox.Name = "AvatarBox";
            AvatarBox.Size = new System.Drawing.Size(100, 100);
            AvatarBox.TabIndex = 0;
            AvatarBox.TabStop = false;
            // 
            // dgStats
            // 
            dgStats.AllowUserToAddRows = false;
            dgStats.AllowUserToDeleteRows = false;
            dgStats.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgStats.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { propClmn, valClmn });
            dgStats.Location = new System.Drawing.Point(12, 139);
            dgStats.Name = "dgStats";
            dgStats.RowHeadersVisible = false;
            dgStats.RowTemplate.Height = 25;
            dgStats.Size = new System.Drawing.Size(255, 251);
            dgStats.TabIndex = 3;
            dgStats.CellContentClick += DgStats_CellContentClick;
            dgStats.CellEndEdit += DgStats_CellEndEdit;
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
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(30, 19);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(0, 15);
            label1.TabIndex = 4;
            // 
            // txtLvl
            // 
            txtLvl.Location = new System.Drawing.Point(118, 37);
            txtLvl.Name = "txtLvl";
            txtLvl.Size = new System.Drawing.Size(78, 23);
            txtLvl.TabIndex = 5;
            txtLvl.ValueChanged += TxtLvl_ValueChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(115, 19);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(37, 15);
            label2.TabIndex = 6;
            label2.Text = "Level:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(12, 121);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(62, 15);
            label3.TabIndex = 7;
            label3.Text = "Base Stats:";
            // 
            // txtRank
            // 
            txtRank.Location = new System.Drawing.Point(118, 89);
            txtRank.Maximum = new decimal(new int[] { 6, 0, 0, 0 });
            txtRank.Name = "txtRank";
            txtRank.Size = new System.Drawing.Size(78, 23);
            txtRank.TabIndex = 8;
            txtRank.ValueChanged += TxtRank_ValueChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(118, 71);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(55, 15);
            label4.TabIndex = 9;
            label4.Text = "Eidolons:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(706, 121);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(33, 15);
            label5.TabIndex = 10;
            label5.Text = "Skills";
            // 
            // dgSkills
            // 
            dgSkills.AllowUserToAddRows = false;
            dgSkills.AllowUserToDeleteRows = false;
            dgSkills.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgSkills.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2, Column1 });
            dgSkills.Location = new System.Drawing.Point(706, 139);
            dgSkills.Name = "dgSkills";
            dgSkills.RowHeadersVisible = false;
            dgSkills.RowTemplate.Height = 25;
            dgSkills.Size = new System.Drawing.Size(353, 251);
            dgSkills.TabIndex = 11;
            dgSkills.CellEndEdit += DgSkills_CellEndEdit;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.HeaderText = "Skill";
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn2
            // 
            dataGridViewTextBoxColumn2.HeaderText = "Level";
            dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // Column1
            // 
            Column1.HeaderText = "MaxLevel";
            Column1.Name = "Column1";
            // 
            // SaveXML
            // 
            SaveXML.Location = new System.Drawing.Point(937, 406);
            SaveXML.Name = "SaveXML";
            SaveXML.Size = new System.Drawing.Size(122, 32);
            SaveXML.TabIndex = 12;
            SaveXML.Text = "Save XML";
            SaveXML.UseVisualStyleBackColor = true;
            SaveXML.Click += SaveXML_Click;
            // 
            // chAutoSave
            // 
            chAutoSave.AutoSize = true;
            chAutoSave.FlatStyle = System.Windows.Forms.FlatStyle.System;
            chAutoSave.Location = new System.Drawing.Point(150, 412);
            chAutoSave.Name = "chAutoSave";
            chAutoSave.Size = new System.Drawing.Size(25, 13);
            chAutoSave.TabIndex = 13;
            chAutoSave.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(171, 410);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(114, 15);
            label6.TabIndex = 14;
            label6.Text = "AutoSave full profile";
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(809, 406);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(122, 32);
            button1.TabIndex = 15;
            button1.Text = "Open XML";
            button1.UseVisualStyleBackColor = true;
            button1.Click += Button1_Click;
            // 
            // LightCone
            // 
            LightCone.AutoSize = true;
            LightCone.Location = new System.Drawing.Point(217, 19);
            LightCone.Name = "LightCone";
            LightCone.Size = new System.Drawing.Size(68, 15);
            LightCone.TabIndex = 18;
            LightCone.Text = "Light Cone:";
            // 
            // txtLC
            // 
            txtLC.Location = new System.Drawing.Point(217, 36);
            txtLC.Name = "txtLC";
            txtLC.ReadOnly = true;
            txtLC.Size = new System.Drawing.Size(105, 23);
            txtLC.TabIndex = 19;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(291, 121);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(31, 15);
            label7.TabIndex = 20;
            label7.Text = "Sets:";
            // 
            // dgSets
            // 
            dgSets.AllowUserToAddRows = false;
            dgSets.AllowUserToDeleteRows = false;
            dgSets.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgSets.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { dataGridViewTextBoxColumn3, dataGridViewTextBoxColumn4 });
            dgSets.Location = new System.Drawing.Point(297, 139);
            dgSets.Name = "dgSets";
            dgSets.RowHeadersVisible = false;
            dgSets.RowTemplate.Height = 25;
            dgSets.Size = new System.Drawing.Size(286, 251);
            dgSets.TabIndex = 21;
            dgSets.CellEndEdit += DgSets_CellEndEdit;
            // 
            // dataGridViewTextBoxColumn3
            // 
            dataGridViewTextBoxColumn3.HeaderText = "Set";
            dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            dataGridViewTextBoxColumn3.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn4
            // 
            dataGridViewTextBoxColumn4.HeaderText = "Num";
            dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            // 
            // txtLCRank
            // 
            txtLCRank.Location = new System.Drawing.Point(343, 36);
            txtLCRank.Name = "txtLCRank";
            txtLCRank.Size = new System.Drawing.Size(37, 23);
            txtLCRank.TabIndex = 22;
            txtLCRank.ValueChanged += TxtLCRank_ValueChanged;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(386, 39);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(37, 15);
            label8.TabIndex = 24;
            label8.Text = "Level:";
            // 
            // txtLcLevel
            // 
            txtLcLevel.Location = new System.Drawing.Point(429, 36);
            txtLcLevel.Name = "txtLcLevel";
            txtLcLevel.Size = new System.Drawing.Size(44, 23);
            txtLcLevel.TabIndex = 23;
            txtLcLevel.ValueChanged += TxtLcLevel_ValueChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(327, 39);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(15, 15);
            label9.TabIndex = 25;
            label9.Text = "C";
            // 
            // WarGear
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1087, 450);
            Controls.Add(label9);
            Controls.Add(label8);
            Controls.Add(txtLcLevel);
            Controls.Add(txtLCRank);
            Controls.Add(dgSets);
            Controls.Add(label7);
            Controls.Add(txtLC);
            Controls.Add(LightCone);
            Controls.Add(button1);
            Controls.Add(label6);
            Controls.Add(chAutoSave);
            Controls.Add(SaveXML);
            Controls.Add(dgSkills);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(txtRank);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(txtLvl);
            Controls.Add(label1);
            Controls.Add(AvatarBox);
            Controls.Add(dgStats);
            Controls.Add(ImportAPI);
            Name = "WarGear";
            Text = "WarGear import";
            Load += WarGear_Load;
            ((System.ComponentModel.ISupportInitialize)AvatarBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgStats).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtLvl).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtRank).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgSkills).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgSets).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtLCRank).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtLcLevel).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button ImportAPI;
        private System.Windows.Forms.PictureBox AvatarBox;
        private System.Windows.Forms.DataGridView dgStats;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridViewTextBoxColumn propClmn;
        private System.Windows.Forms.DataGridViewTextBoxColumn valClmn;
        private System.Windows.Forms.NumericUpDown txtLvl;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown txtRank;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DataGridView dgSkills;
        private System.Windows.Forms.Button SaveXML;
        private System.Windows.Forms.CheckBox chAutoSave;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label LightCone;
        private System.Windows.Forms.TextBox txtLC;
        private System.Windows.Forms.DataGridView dgSets;
        private System.Windows.Forms.NumericUpDown txtLCRank;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown txtLcLevel;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
    }
}