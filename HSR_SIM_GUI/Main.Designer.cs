namespace HSR_SIM_GUI
{
    partial class Main
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            LogWindow = new System.Windows.Forms.RichTextBox();
            BtnOpen = new System.Windows.Forms.Button();
            combatOut = new System.Windows.Forms.PictureBox();
            BtnBack = new System.Windows.Forms.Button();
            BtnNext = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)combatOut).BeginInit();
            SuspendLayout();
            // 
            // LogWindow
            // 
            LogWindow.ForeColor = System.Drawing.SystemColors.WindowText;
            LogWindow.Location = new System.Drawing.Point(8, 620);
            LogWindow.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            LogWindow.Name = "LogWindow";
            LogWindow.Size = new System.Drawing.Size(1200, 215);
            LogWindow.TabIndex = 0;
            LogWindow.Text = "";
            // 
            // BtnOpen
            // 
            BtnOpen.Location = new System.Drawing.Point(13, 841);
            BtnOpen.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            BtnOpen.Name = "BtnOpen";
            BtnOpen.Size = new System.Drawing.Size(120, 35);
            BtnOpen.TabIndex = 1;
            BtnOpen.Text = "Open Scenario";
            BtnOpen.UseVisualStyleBackColor = true;
            BtnOpen.Click += button1_Click;
            // 
            // combatOut
            // 
            combatOut.Location = new System.Drawing.Point(8, 14);
            combatOut.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            combatOut.Name = "combatOut";
            combatOut.Size = new System.Drawing.Size(1200, 600);
            combatOut.TabIndex = 2;
            combatOut.TabStop = false;
            // 
            // BtnBack
            // 
            BtnBack.Location = new System.Drawing.Point(171, 841);
            BtnBack.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            BtnBack.Name = "BtnBack";
            BtnBack.Size = new System.Drawing.Size(120, 35);
            BtnBack.TabIndex = 3;
            BtnBack.Text = "Prev Step";
            BtnBack.UseVisualStyleBackColor = true;
            BtnBack.Click += button2_Click;
            // 
            // BtnNext
            // 
            BtnNext.Location = new System.Drawing.Point(329, 841);
            BtnNext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            BtnNext.Name = "BtnNext";
            BtnNext.Size = new System.Drawing.Size(120, 35);
            BtnNext.TabIndex = 4;
            BtnNext.Text = "Next step";
            BtnNext.UseVisualStyleBackColor = true;
            BtnNext.Click += button3_Click;
            // 
            // Main
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            ClientSize = new System.Drawing.Size(1215, 888);
            Controls.Add(BtnNext);
            Controls.Add(BtnBack);
            Controls.Add(combatOut);
            Controls.Add(BtnOpen);
            Controls.Add(LogWindow);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "Main";
            Text = "SIM GUI";
            Load += Main_Load;
            ((System.ComponentModel.ISupportInitialize)combatOut).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.RichTextBox LogWindow;
        private System.Windows.Forms.Button BtnOpen;
        private System.Windows.Forms.PictureBox combatOut;
        private System.Windows.Forms.Button BtnBack;
        private System.Windows.Forms.Button BtnNext;
    }
}

