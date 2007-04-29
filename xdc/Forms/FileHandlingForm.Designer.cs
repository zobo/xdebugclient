namespace xdc.Forms
{
    partial class FileHandlingForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.radioXdebugSource = new System.Windows.Forms.RadioButton();
            this.radioXdebugSamba = new System.Windows.Forms.RadioButton();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.filenameLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(380, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "XDebugClient cannot find a file that xdebug wants to inspect. How to proceed?";
            // 
            // radioXdebugSource
            // 
            this.radioXdebugSource.AutoSize = true;
            this.radioXdebugSource.Checked = true;
            this.radioXdebugSource.Location = new System.Drawing.Point(16, 56);
            this.radioXdebugSource.Name = "radioXdebugSource";
            this.radioXdebugSource.Size = new System.Drawing.Size(210, 17);
            this.radioXdebugSource.TabIndex = 1;
            this.radioXdebugSource.TabStop = true;
            this.radioXdebugSource.Text = "Ask the debugger to retrieve source file";
            this.radioXdebugSource.UseVisualStyleBackColor = true;
            // 
            // radioXdebugSamba
            // 
            this.radioXdebugSamba.AutoSize = true;
            this.radioXdebugSamba.Location = new System.Drawing.Point(16, 79);
            this.radioXdebugSamba.Name = "radioXdebugSamba";
            this.radioXdebugSamba.Size = new System.Drawing.Size(268, 17);
            this.radioXdebugSamba.TabIndex = 2;
            this.radioXdebugSamba.Text = "Select the file and let XDC handle filename rewriting";
            this.radioXdebugSamba.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(233, 115);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "&Ok";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(314, 115);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "&Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // filenameLabel
            // 
            this.filenameLabel.AutoSize = true;
            this.filenameLabel.Location = new System.Drawing.Point(50, 30);
            this.filenameLabel.Name = "filenameLabel";
            this.filenameLabel.Size = new System.Drawing.Size(65, 13);
            this.filenameLabel.TabIndex = 5;
            this.filenameLabel.Text = "/path/to/file";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(13, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "File:";
            // 
            // FileHandlingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(407, 148);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.filenameLabel);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.radioXdebugSamba);
            this.Controls.Add(this.radioXdebugSource);
            this.Controls.Add(this.label1);
            this.Name = "FileHandlingForm";
            this.Text = "How to load files?";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton radioXdebugSource;
        private System.Windows.Forms.RadioButton radioXdebugSamba;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label filenameLabel;
        private System.Windows.Forms.Label label3;
    }
}