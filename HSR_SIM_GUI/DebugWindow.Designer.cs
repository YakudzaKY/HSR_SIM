namespace HSR_SIM_GUI
{
    partial class DebugWindow
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
            dbgText = new System.Windows.Forms.RichTextBox();
            SuspendLayout();
            // 
            // dbgText
            // 
            dbgText.Location = new System.Drawing.Point(12, 12);
            dbgText.Name = "dbgText";
            dbgText.Size = new System.Drawing.Size(775, 410);
            dbgText.TabIndex = 0;
            dbgText.Text = "";
            dbgText.TextChanged += dbgText_TextChanged;
            // 
            // DebugWindow
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(dbgText);
            Name = "DebugWindow";
            Text = "DebugWindow";
            ResumeLayout(false);
        }

        #endregion

        public System.Windows.Forms.RichTextBox dbgText;
    }
}