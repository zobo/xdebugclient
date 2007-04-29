using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace xdc.Forms
{
    public partial class FileHandlingForm : Form
    {
        public FileHandlingForm(string filename)
        {
            InitializeComponent();
            this.filenameLabel.Text = filename;
        }

        public string SelectedFileLoader
        {
            get
            {
                if (radioXdebugSamba.Checked)
                {
                    return "rewritefileloader";
                }
                else if (radioXdebugSource.Checked)
                {
                    return "sourcefileloader";
                }
                else
                {
                    return "unknown";
                }
            }
        }
    }
}