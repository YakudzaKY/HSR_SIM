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
            button1 = new System.Windows.Forms.Button();
            combatOut = new System.Windows.Forms.PictureBox();
            button2 = new System.Windows.Forms.Button();
            button3 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)combatOut).BeginInit();
            SuspendLayout();
            // 
            // LogWindow
            // 
            LogWindow.ForeColor = System.Drawing.SystemColors.WindowText;
            LogWindow.Location = new System.Drawing.Point(8, 430);
            LogWindow.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            LogWindow.Name = "LogWindow";
            LogWindow.Size = new System.Drawing.Size(800, 305);
            LogWindow.TabIndex = 0;
            LogWindow.Text = "";
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(14, 741);
            button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(120, 35);
            button1.TabIndex = 1;
            button1.Text = "Open Scenario";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // combatOut
            // 
            combatOut.Location = new System.Drawing.Point(8, 14);
            combatOut.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            combatOut.Name = "combatOut";
            combatOut.Size = new System.Drawing.Size(800, 400);
            combatOut.TabIndex = 2;
            combatOut.TabStop = false;
            // 
            // button2
            // 
            button2.Location = new System.Drawing.Point(253, 741);
            button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(120, 35);
            button2.TabIndex = 3;
            button2.Text = "Prev Step";
            button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.Location = new System.Drawing.Point(380, 741);
            button3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(120, 35);
            button3.TabIndex = 4;
            button3.Text = "Next step";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // Main
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            ClientSize = new System.Drawing.Size(826, 796);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(combatOut);
            Controls.Add(button1);
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
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.PictureBox combatOut;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
    }
}

