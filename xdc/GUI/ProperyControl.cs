/*
 * Copyright (C), 2007, Mathieu Kooiman < xdc@scriptorama.nl> 
 * Copyright (C), 2012, Damjan Cvetko <damjan.cvetko@gmail.com> 
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
using Aga.Controls.Tree;
using xdc.XDebug;
using System.Collections.ObjectModel;

namespace xdc.GUI
{
	public partial class ProperyControl : UserControl
	{
		private TreeModel _Model;
		private xdc.XDebug.Client _client;

		public ProperyControl()
		{
			InitializeComponent();

			_Model = new TreeModel();
			treeViewAdv1.Model = _Model;

			treeViewAdv1.Expanding += new EventHandler<TreeViewAdvEventArgs>(treeViewAdv1_Expanding);
			nodeTextBox1.DrawText += new EventHandler<Aga.Controls.Tree.NodeControls.DrawEventArgs>(nodeTextBox1_DrawText);
			nodeTextBox2.DrawText += new EventHandler<Aga.Controls.Tree.NodeControls.DrawEventArgs>(nodeTextBox1_DrawText);
		}

		void nodeTextBox1_DrawText(object sender, Aga.Controls.Tree.NodeControls.DrawEventArgs e)
		{
			DebugNode node = e.Node.Tag as DebugNode;
			if (node.Changed)
			{
				e.TextColor = Color.Red;
			}
		}

		public Client Client
		{
			get { return _client; }
			set { _client = value; }
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
			if (node != null && !node.Property.isComplete)
			{
				this.treeViewAdv1.BeginUpdate();
				Property p = _client.GetPropertyValue(node.Property.FullName);

				/* We don't want 'p' itself. It will be a copy of the node that
				 * was marked as inComplete. */
				foreach (Property child in p.ChildProperties)
				{
					DebugNode newNode = this.BuildDebugNode(child, node);

					node.Nodes.Add(newNode);
				}

				node.Property.isComplete = true;
				this.treeViewAdv1.EndUpdate();
			}
		}

		public DebugNode BuildDebugNode(Property p, DebugNode Parent)
		{
			DebugNode newNode = new DebugNode(p);
			if (dataMap.ContainsKey(p.FullName) && dataMap[p.FullName] != p.Type + p.Value) newNode.Changed = true;

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

		public void LoadProperty(Property p)
		{
			List<Property> list = new List<Property>();
			list.Add(p);
			this.LoadPropertyList(list);
		}

		public void LoadPropertyList(List<Property> list)
		{
			this.treeViewAdv1.BeginUpdate();
			// todo, save old expand states, scroll?, changed
			fillDataMap(_Model.Nodes);
			_Model.Nodes.Clear();
			foreach (Property p in list)
			{
				DebugNode node = this.BuildDebugNode(p, null);
				_Model.Nodes.Add(node);
			}
			this.treeViewAdv1.EndUpdate();
		}

		#region Changed
		private Dictionary<string, string> dataMap = new Dictionary<string, string>();
		private void fillDataMap(Collection<Node> nodes)
		{
			foreach (Node node in nodes)
			{
				DebugNode dnode = node as DebugNode;
				if (dnode != null && dnode.Property != null)
				{
					dataMap[dnode.Property.FullName] = dnode.Property.Type + dnode.Property.Value;
				}
				fillDataMap(node.Nodes);
			}
		}
		#endregion
	}
}
