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
    public partial class SourceFileForm : DockContent
    {
        static Color BreakpointColor = Color.FromArgb(255, 213, 211);
        static Color CurrentLineColor = Color.FromName("Yellow");

        private string _filename;
        
        private xdc.XDebug.Client _xdebugClient;
        
        public SourceFileForm(xdc.XDebug.Client xdebugClient, string filename)
        {            
            this._xdebugClient = xdebugClient;
            this._filename = filename;

            InitializeComponent();

            this.textEditor.TextEditorProperties.UseCustomLine = true;
            this.textEditor.TextEditorProperties.ShowEOLMarker = false;
            this.textEditor.TextEditorProperties.ShowSpaces = false;
            this.textEditor.Document.ReadOnly = true;
            this.textEditor.TextEditorProperties.CreateBackupCopy = false;
            
            this.textEditor.ActiveTextAreaControl.TextArea.IconBarMargin.MouseDown += new ICSharpCode.TextEditor.MarginMouseEventHandler(OnIconBarMarginMouseDown);
            this.textEditor.ActiveTextAreaControl.TextArea.KeyDown += new System.Windows.Forms.KeyEventHandler(OnTextEditorKeyDown);
            this.textEditor.Document.BookmarkManager.Removed += new ICSharpCode.TextEditor.Document.BookmarkEventHandler(OnBookmarkRemoved);
            this.textEditor.Document.BookmarkManager.Added += new ICSharpCode.TextEditor.Document.BookmarkEventHandler(OnBookmarkAdded);
            this.textEditor.ActiveTextAreaControl.TextArea.ToolTipRequest += new ToolTipRequestEventHandler(OnToolTipRequest);
        }

        public void LoadFile(string filename)
        {           
            this.textEditor.LoadFile(filename);
        }

        public string getFilename()
        {
            return this._filename;
        }

        public void setFilename(string filename)
        {
            this._filename = filename;
        }
       
        public void ToggleMenuItems(bool started)
        {
            inspectToolStripMenuItem.Enabled = started;
        }

        #region Utility functions
        public ICSharpCode.TextEditor.Document.BookmarkManager getBookmarkManager()
        {
            return this.textEditor.Document.BookmarkManager;
        }

        private void setLineColor(int lineNumber, Color color)
        {
            IDocument d = this.textEditor.Document;

            d.CustomLineManager.RemoveCustomLine(lineNumber);

            d.CustomLineManager.AddCustomLine(lineNumber, color, false);

            d.RequestUpdate(
                new ICSharpCode.TextEditor.TextAreaUpdate(
                    ICSharpCode.TextEditor.TextAreaUpdateType.SingleLine,
                    lineNumber 
                )
            );

            d.CommitUpdate();
        }
        #endregion

        #region Eventhandlers
        void OnIconBarMarginMouseDown(ICSharpCode.TextEditor.AbstractMargin sender, Point mousepos, MouseButtons mouseButtons)
        {
            if (mouseButtons != MouseButtons.Left)
                return;

            ICSharpCode.TextEditor.IconBarMargin marginObj = (ICSharpCode.TextEditor.IconBarMargin)sender;

            Rectangle viewRect = marginObj.TextArea.TextView.DrawingPosition;
            Point logicPos     = marginObj.TextArea.TextView.GetLogicalPosition(0, mousepos.Y - viewRect.Top);

            if (logicPos.Y >= 0 && logicPos.Y < marginObj.TextArea.Document.TotalNumberOfLines)
            {
                LineSegment l = marginObj.Document.GetLineSegment(logicPos.Y);

                string s = marginObj.Document.GetText(l);

                if (s.Trim().Length == 0)
                    return;

                // FIXME:
                // Not quite happy with this. It's hidden. Also, removing
                // the breakpoint has extra code in OnBookmarkManager not
                // matching up with OnBookmarkAdded.
                this.textEditor.Document.BookmarkManager.AddMark
                (
                    new Breakpoint (
                        this._filename,
                        this.textEditor.Document,
                        logicPos.Y
                    )
                );

                this.RedrawBreakpoints();
                                  
                marginObj.Paint(marginObj.TextArea.CreateGraphics(), marginObj.TextArea.DisplayRectangle);
            }            
        }

        void OnTextEditorKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.F9)
                return;

            ICSharpCode.TextEditor.TextArea area_obj = (ICSharpCode.TextEditor.TextArea)sender;

            ICSharpCode.TextEditor.Document.LineSegment l = area_obj.Document.GetLineSegment(area_obj.Caret.Line);
            string s = area_obj.Document.GetText(l);
            if (s.Trim().Length == 0)
                return;

            ICSharpCode.TextEditor.Document.BookmarkManager bm = this.textEditor.Document.BookmarkManager;

            if (bm.IsMarked(area_obj.Caret.Line))
                bm.RemoveMark(bm.GetNextMark(area_obj.Caret.Line-1));
            else
                this.textEditor.Document.BookmarkManager.AddMark(new Breakpoint(this._filename,this.textEditor.Document,area_obj.Caret.Line));

            // this.textEditor.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, area_obj.Caret.Line));

            this.RedrawBreakpoints();

            // area_obj.IconBarMargin.Paint(area_obj.IconBarMargin.TextArea.CreateGraphics(), area_obj.IconBarMargin.TextArea.DisplayRectangle);
        }

        public void SetActiveMark(int Line)
        {      
            ActiveMark mark = new ActiveMark(this._filename, this.textEditor.Document, Line);

            this.textEditor.Document.BookmarkManager.AddMark(mark);

            this.setLineColor(Line, Color.FromName("Yellow"));

            this.textEditor.ActiveTextAreaControl.ScrollTo(Line - this.textEditor.ActiveTextAreaControl.TextArea.TextView.VisibleLineCount / 2);
            this.textEditor.ActiveTextAreaControl.ScrollTo(Line);
            this.textEditor.ActiveTextAreaControl.ScrollTo(Line + this.textEditor.ActiveTextAreaControl.TextArea.TextView.VisibleLineCount / 2);
        }


            
        public void RemoveActiveMark()
        {
            BookmarkManager mgr = this.textEditor.Document.BookmarkManager;

            foreach (Bookmark mark in mgr.Marks)
            {
                if (mark is ActiveMark)
                {
                    mgr.RemoveMark(mark);
                    this.textEditor.Document.CustomLineManager.RemoveCustomLine(mark.LineNumber);

                    break;
                }
            }

            this.RedrawBreakpoints();
        }

        private void RedrawBreakpoints()
        {
            foreach (Bookmark mark in this.textEditor.Document.BookmarkManager.Marks)
            {
                if (mark is Breakpoint)
                {
                    this.setLineColor(mark.LineNumber, SourceFileForm.BreakpointColor);
                }
            }
        }

        void OnBookmarkAdded(object sender, ICSharpCode.TextEditor.Document.BookmarkEventArgs e)
        {
            
        }

        void OnBookmarkRemoved(object sender, ICSharpCode.TextEditor.Document.BookmarkEventArgs e)
        {            
            this.textEditor.Document.CustomLineManager.RemoveCustomLine(e.Bookmark.LineNumber);
            
            this.textEditor.Document.RequestUpdate(
                new ICSharpCode.TextEditor.TextAreaUpdate(
                    ICSharpCode.TextEditor.TextAreaUpdateType.SingleLine, 
                    e.Bookmark.LineNumber
                )
            );

            this.textEditor.Document.CommitUpdate();            
        }

        void OnToolTipRequest(object sender, ToolTipRequestEventArgs e)
        {
            if (e.InDocument && !e.ToolTipShown && _xdebugClient.State == XdebugClientState.Break)
            {
                ICSharpCode.TextEditor.TextArea area_obj = (ICSharpCode.TextEditor.TextArea)sender;

                string s = ICSharpCode.TextEditor.Document.TextUtilities.GetWordAt(area_obj.Document, area_obj.Document.PositionToOffset(new Point(e.LogicalPosition.X, e.LogicalPosition.Y)));

                if (area_obj.Document.GetCharAt(MyFindWordStart(area_obj.Document, area_obj.Document.PositionToOffset(new Point(e.LogicalPosition.X, e.LogicalPosition.Y))) - 1).ToString() == "$")
                    s = "$" + s;

                if (s != "")
                {
                    Property p = _xdebugClient.GetPropertyValue(s, "0");

                    if (p != null)
                        e.ShowToolTip(p.Value);
                }
            }
        }

        // ICSharpCode.TextEditor.Document.TextUtilities.FindWordStart did not work in the
        // Assembly I used, so I copied this from source
        public static int MyFindWordStart(IDocument document, int offset)
        {
            LineSegment line = document.GetLineSegmentForOffset(offset);
            int lineOffset = line.Offset;
            while (offset > lineOffset && ICSharpCode.TextEditor.Document.TextUtilities.IsLetterDigitOrUnderscore(document.GetCharAt(offset - 1)))
            {
                --offset;
            }

            return offset;
        }

        #endregion

        private void inspectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_xdebugClient.State == XdebugClientState.Break)
            {
                string s;

                if (textEditor.ActiveTextAreaControl.SelectionManager.HasSomethingSelected)
                    s = textEditor.ActiveTextAreaControl.SelectionManager.SelectedText;
                else
                    s = ICSharpCode.TextEditor.Document.TextUtilities.GetWordAt(textEditor.ActiveTextAreaControl.Document, textEditor.ActiveTextAreaControl.Caret.Offset);

                Property p = _xdebugClient.GetPropertyValue(s, "0"); // TODO, what about global context

                if (p == null)
                {
                    MessageBox.Show("Property \"" + s + "\" is not available in this scope.", "Unavailable");
                    return;
                }

                PropertyForm propForm = new PropertyForm(_xdebugClient);

                propForm.LoadProperty(p);

                propForm.Show();
            }      
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            inspectToolStripMenuItem.Enabled = (_xdebugClient.State == XdebugClientState.Break);
            runToCursorToolStripMenuItem.Enabled = (_xdebugClient.State == XdebugClientState.Break);
        }

        public void LoadSourceAsFile(string sourceCode)
        {
            this.textEditor.BeginUpdate();

            this.textEditor.Document.HighlightingStrategy = ICSharpCode.TextEditor.Document.HighlightingStrategyFactory.CreateHighlightingStrategy("PHP");

            this.textEditor.Text = sourceCode;

            this.textEditor.EndUpdate();
            
        }

        private void runToCursorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_xdebugClient.State == XdebugClientState.Break)
            {
                _xdebugClient.RunTo(_filename, textEditor.ActiveTextAreaControl.Caret.Line);
            }
        }
    }
}