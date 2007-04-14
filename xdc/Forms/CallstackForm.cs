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

 *  Foobar is distributed in the hope that it will be useful,
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
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using xdc.XDebug;

namespace xdc.Forms
{
    public partial class CallstackForm : DockContent
    {
        public CallstackForm()
        {
            InitializeComponent();  
            
        }

        public void setCallstack(List<StackEntry> stack)
        {
            treeView1.Nodes.Clear();

            foreach (StackEntry entry in stack)
            {                
                treeView1.Nodes.Add(entry.location + "() at " + entry.fileName + ":" + entry.lineNumber);
            }
        }

    }
}