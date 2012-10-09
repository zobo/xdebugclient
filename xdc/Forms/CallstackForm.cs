/*
 * Copyright (C), 2007, Mathieu Kooiman < xdc@scriptorama.nl> 
 * $Id$
 * 
 * This file is part of XDebugClient.
 *
 *  XDebugClient is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation; either version 2.1 of the License, or
 *  (at your option) any later version.

 *  XDebugClient is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with XDebugClient; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using WeifenLuo.WinFormsUI;
using WeifenLuo.WinFormsUI.Docking;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using xdc.XDebug;

namespace xdc.Forms
{
    public partial class CallstackForm : DockContent
    {
        public event EventHandler<StackEventArgs> StackSelected;

        public CallstackForm()
        {
            InitializeComponent();  
            
        }

        public void setCallstack(List<StackEntry> stack)
        {
            treeView1.Nodes.Clear();

            foreach (StackEntry entry in stack)
            {                
                TreeNode tn = new TreeNode(entry.location + "() at " + entry.fileName + ":" + entry.lineNumber);
                tn.Tag = entry;
                treeView1.Nodes.Add(tn);
            }
        }

        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                StackEntry se = (StackEntry)treeView1.SelectedNode.Tag;
                if (se.Location.line >= 0)
                {
                    DoStackSelected(se);
                }
            }
        }

        protected void DoStackSelected(StackEntry stackEntry)
        {
            if (this.StackSelected != null)
            {
                this.StackSelected(this, new StackEventArgs(stackEntry));
            }
        }

    }

    public class StackEventArgs : EventArgs
    {
        public StackEntry StackEntry;

        public StackEventArgs(StackEntry stackEntry)
        {
            this.StackEntry = stackEntry;
        }
    }
}