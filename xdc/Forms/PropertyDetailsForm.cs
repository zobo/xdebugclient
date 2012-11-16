using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace xdc.Forms
{
    public partial class PropertyDetailsForm : Form
    {
        private string _content_value;

        public PropertyDetailsForm(string v)
        {
            InitializeComponent();

            _content_value = v;
        }

        private void PropertyDetailsForm_Load(object sender, EventArgs e)
        {
            textBox1.Text = _content_value;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
