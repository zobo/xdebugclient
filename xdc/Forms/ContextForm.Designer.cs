namespace xdc.Forms
{
    partial class ContextForm
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
            this.properyControl1 = new xdc.GUI.ProperyControl();
            this.SuspendLayout();
            // 
            // properyControl1
            // 
            this.properyControl1.Client = null;
            this.properyControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.properyControl1.Location = new System.Drawing.Point(0, 0);
            this.properyControl1.Name = "properyControl1";
            this.properyControl1.Size = new System.Drawing.Size(284, 262);
            this.properyControl1.TabIndex = 0;
            // 
            // ContextForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.properyControl1);
            this.Name = "ContextForm";
            this.TabText = "ContextForm";
            this.Text = "ContextForm";
            this.ResumeLayout(false);

        }

        #endregion

        private xdc.GUI.ProperyControl properyControl1;
    }
}