namespace xdc.Forms
{
    partial class SourceFileForm
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
            this.components = new System.ComponentModel.Container();
            this.textEditor = new ICSharpCode.TextEditor.TextEditorControl();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.inspectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runToCursorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textEditor
            // 
            this.textEditor.ContextMenuStrip = this.contextMenuStrip1;
            this.textEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textEditor.Location = new System.Drawing.Point(0, 0);
            this.textEditor.Name = "textEditor";
            this.textEditor.ShowEOLMarkers = true;
            this.textEditor.ShowSpaces = true;
            this.textEditor.ShowTabs = true;
            this.textEditor.ShowVRuler = true;
            this.textEditor.Size = new System.Drawing.Size(292, 266);
            this.textEditor.TabIndex = 0;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.inspectToolStripMenuItem,
            this.runToCursorToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(153, 70);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // inspectToolStripMenuItem
            // 
            this.inspectToolStripMenuItem.Name = "inspectToolStripMenuItem";
            this.inspectToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.inspectToolStripMenuItem.Text = "Inspect";
            this.inspectToolStripMenuItem.Click += new System.EventHandler(this.inspectToolStripMenuItem_Click);
            // 
            // runToCursorToolStripMenuItem
            // 
            this.runToCursorToolStripMenuItem.Name = "runToCursorToolStripMenuItem";
            this.runToCursorToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.runToCursorToolStripMenuItem.Text = "Run To Cursor";
            this.runToCursorToolStripMenuItem.Click += new System.EventHandler(this.runToCursorToolStripMenuItem_Click);
            // 
            // SourceFileForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.textEditor);
            this.Name = "SourceFileForm";
            this.TabText = "SourceFileForm";
            this.Text = "SourceFileForm";
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ICSharpCode.TextEditor.TextEditorControl textEditor;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem inspectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runToCursorToolStripMenuItem;
    }
}