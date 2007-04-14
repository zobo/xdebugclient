/*
 * Copyright (C), 2007, Mathieu Kooiman < xdc@scriptorama.nl> 
 * $LastRevision$
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
using System.Text;
using Aga.Controls.Tree;

using xdc.XDebug;

namespace xdc.GUI
{
    public class DebugNode : Aga.Controls.Tree.Node
    {
        private string _Name;
        private string _Value;
        
        public Property Property;

        public string Name { get { return _Name; } }
        public string Value { get { return _Value; } }

        public DebugNode(Property p)
        {
            _Name = p.Name;
            _Value = p.Value;

            this.Property = p;
            this.Tag = p;
        }

        public override bool IsLeaf
        {
            get
            {
                bool result;
                if (this.Property.ChildProperties.Count != 0)
                {
                    result= false;
                }
                else if (!this.Property.isComplete)
                {
                    result = false;
                }
                else
                {
                    result = true;
                }

                return result;
            }
        }
    }
}
