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
            this.LogWindow = new System.Windows.Forms.RichTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.combatOut = new System.Windows.Forms.PictureBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.combatOut)).BeginInit();
            this.SuspendLayout();
            // 
            // LogWindow
            // 
            this.LogWindow.Cursor = System.Windows.Forms.Cursors.Default;
            this.LogWindow.ForeColor = System.Drawing.SystemColors.WindowText;
            this.LogWindow.Location = new System.Drawing.Point(12, 418);
            this.LogWindow.Name = "LogWindow";
            this.LogWindow.Size = new System.Drawing.Size(795, 218);
            this.LogWindow.TabIndex = 0;
            this.LogWindow.Text = "";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 642);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(103, 30);
            this.button1.TabIndex = 1;
            this.button1.Text = "Open Scenario";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // combatOut
            // 
            this.combatOut.Location = new System.Drawing.Point(7, 12);
            this.combatOut.Name = "combatOut";
            this.combatOut.Size = new System.Drawing.Size(800, 400);
            this.combatOut.TabIndex = 2;
            this.combatOut.TabStop = false;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(217, 642);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(103, 30);
            this.button2.TabIndex = 3;
            this.button2.Text = "Prev Step";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(326, 642);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(103, 30);
            this.button3.TabIndex = 4;
            this.button3.Text = "Next step";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(820, 690);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.combatOut);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.LogWindow);
            this.Name = "Main";
            this.Text = "SIM GUI";
            this.Load += new System.EventHandler(this.Main_Load);
            ((System.ComponentModel.ISupportInitialize)(this.combatOut)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox LogWindow;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.PictureBox combatOut;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
    }
}

