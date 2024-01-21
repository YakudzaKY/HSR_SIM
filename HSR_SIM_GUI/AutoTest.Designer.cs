namespace HSR_SIM_GUI
{
    partial class AutoTest
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
            dgTest = new System.Windows.Forms.DataGridView();
            dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            GoBtn = new System.Windows.Forms.Button();
            lblRes = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)dgTest).BeginInit();
            SuspendLayout();
            // 
            // dgTest
            // 
            dgTest.AllowUserToAddRows = false;
            dgTest.AllowUserToDeleteRows = false;
            dgTest.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgTest.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { dataGridViewTextBoxColumn3, dataGridViewTextBoxColumn4 });
            dgTest.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            dgTest.Location = new System.Drawing.Point(12, 38);
            dgTest.Name = "dgTest";
            dgTest.RowHeadersVisible = false;
            dgTest.Size = new System.Drawing.Size(419, 251);
            dgTest.TabIndex = 22;
            // 
            // dataGridViewTextBoxColumn3
            // 
            dataGridViewTextBoxColumn3.HeaderText = "File";
            dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            dataGridViewTextBoxColumn3.ReadOnly = true;
            dataGridViewTextBoxColumn3.Width = 200;
            // 
            // dataGridViewTextBoxColumn4
            // 
            dataGridViewTextBoxColumn4.HeaderText = "Result";
            dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            // 
            // GoBtn
            // 
            GoBtn.Location = new System.Drawing.Point(12, 335);
            GoBtn.Name = "GoBtn";
            GoBtn.Size = new System.Drawing.Size(107, 32);
            GoBtn.TabIndex = 23;
            GoBtn.Text = "GO";
            GoBtn.UseVisualStyleBackColor = true;
            GoBtn.Click += GoBtn_Click;
            // 
            // lblRes
            // 
            lblRes.AutoSize = true;
            lblRes.Location = new System.Drawing.Point(12, 302);
            lblRes.Name = "lblRes";
            lblRes.Size = new System.Drawing.Size(0, 15);
            lblRes.TabIndex = 24;
            // 
            // AutoTest
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(459, 379);
            Controls.Add(lblRes);
            Controls.Add(GoBtn);
            Controls.Add(dgTest);
            Name = "AutoTest";
            Text = "AutoTest";
            Load += AutoTest_Load;
            ((System.ComponentModel.ISupportInitialize)dgTest).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.DataGridView dgTest;
        private System.Windows.Forms.Button GoBtn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.Label lblRes;
    }
}