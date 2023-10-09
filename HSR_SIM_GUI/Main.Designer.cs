﻿namespace HSR_SIM_GUI
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
            ini.IniWriteValue("form", "Scenario", cbScenario.Text);
            ini.IniWriteValue("form", "Profile", cbProfile.Text);
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
            cbScenario = new System.Windows.Forms.ComboBox();
            cbProfile = new System.Windows.Forms.ComboBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            button1 = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            button3 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)combatOut).BeginInit();
            SuspendLayout();
            // 
            // LogWindow
            // 
            LogWindow.ForeColor = System.Drawing.SystemColors.WindowText;
            LogWindow.Location = new System.Drawing.Point(517, 609);
            LogWindow.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            LogWindow.Name = "LogWindow";
            LogWindow.Size = new System.Drawing.Size(696, 215);
            LogWindow.TabIndex = 0;
            LogWindow.Text = "";
            // 
            // BtnOpen
            // 
            BtnOpen.Location = new System.Drawing.Point(269, 634);
            BtnOpen.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            BtnOpen.Name = "BtnOpen";
            BtnOpen.Size = new System.Drawing.Size(120, 30);
            BtnOpen.TabIndex = 1;
            BtnOpen.Text = "Open Scenario";
            BtnOpen.UseVisualStyleBackColor = true;
            BtnOpen.Click += button1_Click;
            // 
            // combatOut
            // 
            combatOut.Location = new System.Drawing.Point(13, 1);
            combatOut.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            combatOut.Name = "combatOut";
            combatOut.Size = new System.Drawing.Size(1200, 600);
            combatOut.TabIndex = 2;
            combatOut.TabStop = false;
            // 
            // BtnBack
            // 
            BtnBack.Location = new System.Drawing.Point(8, 678);
            BtnBack.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            BtnBack.Name = "BtnBack";
            BtnBack.Size = new System.Drawing.Size(120, 35);
            BtnBack.TabIndex = 3;
            BtnBack.Text = "Prev step";
            BtnBack.UseVisualStyleBackColor = true;
            BtnBack.Click += button2_Click;
            // 
            // BtnNext
            // 
            BtnNext.Location = new System.Drawing.Point(134, 678);
            BtnNext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            BtnNext.Name = "BtnNext";
            BtnNext.Size = new System.Drawing.Size(120, 35);
            BtnNext.TabIndex = 4;
            BtnNext.Text = "Next step";
            BtnNext.UseVisualStyleBackColor = true;
            BtnNext.Click += button3_Click;
            // 
            // cbScenario
            // 
            cbScenario.FormattingEnabled = true;
            cbScenario.Location = new System.Drawing.Point(8, 639);
            cbScenario.Name = "cbScenario";
            cbScenario.Size = new System.Drawing.Size(116, 23);
            cbScenario.TabIndex = 5;
            // 
            // cbProfile
            // 
            cbProfile.FormattingEnabled = true;
            cbProfile.Location = new System.Drawing.Point(134, 639);
            cbProfile.Name = "cbProfile";
            cbProfile.Size = new System.Drawing.Size(116, 23);
            cbProfile.TabIndex = 6;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(134, 612);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(41, 15);
            label1.TabIndex = 7;
            label1.Text = "Profile";
            label1.Click += label1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(13, 612);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(52, 15);
            label2.TabIndex = 8;
            label2.Text = "Scenario";
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(269, 607);
            button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(120, 24);
            button1.TabIndex = 9;
            button1.Text = "refresh";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click_1;
            // 
            // button2
            // 
            button2.Location = new System.Drawing.Point(134, 719);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(120, 21);
            button2.TabIndex = 10;
            button2.Text = "To finish";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click_1;
            // 
            // button3
            // 
            button3.Location = new System.Drawing.Point(8, 719);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(120, 21);
            button3.TabIndex = 11;
            button3.Text = "To start";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click_1;
            // 
            // Main
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            ClientSize = new System.Drawing.Size(1225, 834);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(cbProfile);
            Controls.Add(cbScenario);
            Controls.Add(BtnNext);
            Controls.Add(BtnBack);
            Controls.Add(combatOut);
            Controls.Add(BtnOpen);
            Controls.Add(LogWindow);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "Main";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "SIM GUI";
            Load += Main_Load;
            ((System.ComponentModel.ISupportInitialize)combatOut).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.RichTextBox LogWindow;
        private System.Windows.Forms.Button BtnOpen;
        private System.Windows.Forms.PictureBox combatOut;
        private System.Windows.Forms.Button BtnBack;
        private System.Windows.Forms.Button BtnNext;
        private System.Windows.Forms.ComboBox cbScenario;
        private System.Windows.Forms.ComboBox cbProfile;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
    }
}
