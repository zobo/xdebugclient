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

using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

using xdc.XDebug;
using xdc.GUI;

using Aga.Controls.Tree;


namespace xdc.Forms
{
    public partial class PropertyForm : Form
    {
        private TreeModel _Model;
        private xdc.XDebug.Client _client; 
        public PropertyForm(xdc.XDebug.Client client)
        {
            InitializeComponent();

            _client = client;

            _Model = new TreeModel();
            treeViewAdv1.Model = _Model;
            
            treeViewAdv1.Expanding += new EventHandler<TreeViewAdvEventArgs>(treeViewAdv1_Expanding);
            
        }

        void treeViewAdv1_Expanding(object sender, TreeViewAdvEventArgs e)
        {
            if (_client.State != XdebugClientState.Break)
            {
                MessageBox.Show(
                    "This property is no longer available. Close the Property window and try running the script again.",
                    "Property invalidated",
                    MessageBoxButtons.OK
                );

                return;
            }

            DebugNode node = e.Node.Tag as DebugNode;
            if ( node != null && !node.Property.isComplete)
            {
                Property p = _client.GetPropertyValue(node.Property.FullName, 0);

                /* We don't want 'p' itself. It will be a copy of the node that
                 * was marked as inComplete. */
                foreach (Property child in p.ChildProperties)
                {
                    DebugNode newNode = this.BuildDebugNode(child, node);

                    node.Nodes.Add(newNode);
                }

                node.Property.isComplete = true;
            }          
        }

        public DebugNode BuildDebugNode(Property p, DebugNode Parent)
        {
            DebugNode newNode = new DebugNode(p);

            if (Parent != null && Parent is DebugNode)
            {
                newNode.Parent = Parent;
            }

            foreach (Property Child in p.ChildProperties)
            {
                DebugNode tmpNode = this.BuildDebugNode(Child, newNode);                
            }

            return newNode;

        }

        public void LoadProperty(Property p, DebugNode Parent)
        {
            DebugNode newNode = this.BuildDebugNode(p, Parent);

            _Model.Nodes.Add(newNode);            
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}