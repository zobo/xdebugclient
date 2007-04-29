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
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using ICSharpCode.TextEditor;

namespace xdc.XDebug
{
    public class Breakpoint : ICSharpCode.TextEditor.Document.Bookmark
    {
        public string filename;
        public string xdebugId;

        public int LineNumber_XDebug
        {
            get
            {
                return this.LineNumber + 1;
            }
        }

        /// <summary>
        /// Line Number Must Be 0-based!
        /// </summary>        
        public Breakpoint(string filename, ICSharpCode.TextEditor.Document.IDocument d, int l)
            : base(d, l)
        {
            this.filename = filename;
        }

        public override void Draw(ICSharpCode.TextEditor.IconBarMargin margin, Graphics g, Point p)
        {
            margin.DrawBreakpoint(g, p.Y, IsEnabled, true);
        }
    }
}