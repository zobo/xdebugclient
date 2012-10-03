using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using xdc.XDebug;

namespace xdc.Forms
{
    public partial class ContextForm : DockContent
    {
        public ContextForm(xdc.XDebug.Client client, string text)
        {
            InitializeComponent();
            this.Text = this.TabText = text;

            properyControl1.Client = client;
        }

        protected override string GetPersistString()
        {
            return base.GetPersistString() + this.Text;
        }

        public void LoadPropertyList(List<Property> list)
        {
            this.properyControl1.LoadPropertyList(list);
        }
    }
}
