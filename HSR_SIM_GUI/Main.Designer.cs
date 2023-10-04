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
            this.SuspendLayout();
            // 
            // LogWindow
            // 
            this.LogWindow.Cursor = System.Windows.Forms.Cursors.Default;
            this.LogWindow.ForeColor = System.Drawing.SystemColors.WindowText;
            this.LogWindow.Location = new System.Drawing.Point(12, 230);
            this.LogWindow.Name = "LogWindow";
            this.LogWindow.Size = new System.Drawing.Size(1114, 366);
            this.LogWindow.TabIndex = 0;
            this.LogWindow.Text = "";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 602);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(103, 30);
            this.button1.TabIndex = 1;
            this.button1.Text = "Open Scenario";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1138, 644);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.LogWindow);
            this.Name = "Main";
            this.Text = "SIM GUI";
            this.Load += new System.EventHandler(this.Main_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox LogWindow;
        private System.Windows.Forms.Button button1;
    }
}

