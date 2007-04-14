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
 *  along with Foobar; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

using WeifenLuo.WinFormsUI;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

using xdc.XDebug;
using xdc.GUI;

namespace xdc
{
    public partial class MainForm : Form
    {
        private xdc.Forms.StatusForm _statusFrm;
        private xdc.Forms.CallstackForm _callstackFrm;

        private xdc.XDebug.Client _client;
        
        private BreakpointManager _breakpointMgr;
        
        private FileManager _fileMgr;
        private Location _CurrentLocation;        

        delegate bool XdebugClientCallback(XDebugEventArgs e);

        public MainForm()
        {
            InitializeComponent();
            
            _client = new xdc.XDebug.Client("localhost", 9000);                         
            _client.EventCallback += new XDebugEventHandler(XDebugEventCallback);

            // Get the forms up. 
            _statusFrm    = new xdc.Forms.StatusForm();
            _callstackFrm = new xdc.Forms.CallstackForm();

            // Helper objects
            _breakpointMgr = new BreakpointManager();
            _fileMgr = new FileManager();
            
            _CurrentLocation = new Location();
            _CurrentLocation.line = -1;

            this.KeyPreview = true;
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(MainForm_KeyUp);

            this.ToggleMenuItems(false);            
        }

        #region Threading helpers 
        private bool ReinvokeInOwnThread(XdebugClientCallback xcc, object[] list)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(xcc, list);
                return true;
            }

            return false;
        }
        #endregion

        #region GUI helpers
        
        private void ToggleMenuItems(bool started)
        {
            if (started)
            {
                runToolStripMenuItem.Enabled = true;
                stepInToolStripMenuItem.Enabled = true;
                stepOutToolStripMenuItem.Enabled = true;
                stepOverToolStripMenuItem.Enabled = true;

                startListeningToolStripMenuItem.Enabled = false;
                stopDebuggingToolStripMenuItem.Enabled = true;
                openToolStripMenuItem.Enabled = true;
            }
            else
            {
                stepInToolStripMenuItem.Enabled = false;
                stepOutToolStripMenuItem.Enabled = false;
                stepOverToolStripMenuItem.Enabled = false;
                runToolStripMenuItem.Enabled = false;

                startListeningToolStripMenuItem.Enabled = true;
                stopDebuggingToolStripMenuItem.Enabled = false;
                openToolStripMenuItem.Enabled = false;
            }

        }

        private void StopDebuggingSession()
        {
            if (_CurrentLocation.line != -1)
            {
                xdc.Forms.SourceFileForm form = _fileMgr.getFormByRemoteFilename(_CurrentLocation.filename);
                form.RemoveActiveMark();
            }

            _client.Disconnect();

            foreach (OpenFileStruct file in _fileMgr.Forms.Values)
            {
                file.form.ToggleMenuItems(false);
            }

            this.ToggleMenuItems(false);
        }

        private void WriteDebugLine(string line)
        {
            System.Diagnostics.Debug.WriteLine(line);
        }
        #endregion

        #region File helpers
        private void PrepareFileForAccess(string filename)
        {
            if ( _fileMgr.getFormByRemoteFilename(filename) == null) 
            {            
                this.LoadFile(filename);               
            }
        }
             
        private void SetActiveFileAndLine(Location location)
        {
            if (_CurrentLocation.line != -1)
            {
                xdc.Forms.SourceFileForm previousFile = _fileMgr.getFormByRemoteFilename(_CurrentLocation.filename);
                previousFile.RemoveActiveMark();
            }

            _CurrentLocation.line = location.line;
            _CurrentLocation.filename = location.filename;

            xdc.Forms.SourceFileForm currentFile = _fileMgr.getFormByRemoteFilename(location.filename);

            currentFile.SetActiveMark(_CurrentLocation.line);
            currentFile.Focus();
            currentFile.BringToFront();

            
            
            WriteDebugLine("[DEBUG] Setting active line to " + location.filename + ": " + location.line);

        }
                  
        private bool LoadFile(string filename)
        {
            string baseFile = System.IO.Path.GetFileName(filename);
            
            string remoteFilename = filename;
            string localFilename = filename;

            xdc.Forms.SourceFileForm sff = _fileMgr.getFormByRemoteFilename(filename);

            if (sff != null)
            {
                sff.Focus();
                return true;
            }            

            if (!System.IO.File.Exists(filename) && !System.IO.File.Exists(_fileMgr.getLocalFilename(filename)))
            {


                string sourceFilename = System.IO.Path.GetFileName(filename);
                bool done = false;

                while (!done)
                {
                    DialogResult r = MessageBox.Show(
                        "We couldn't open the file:\r\n\r\n" + filename + "\r\n\r\nPerhaps it's on a different server. Do you want to search for it?",
                        "Local file not found.",
                        MessageBoxButtons.YesNo
                    );

                    if (r != DialogResult.Yes)
                    {
                        MessageBox.Show("Nothing I can do then. Ciao!");
                        this.StopDebuggingSession();
                        return false;
                    }

                    OpenFileDialog fileDialog = new OpenFileDialog();
                    fileDialog.Filter = "PHP file|" + sourceFilename;

                    DialogResult file = fileDialog.ShowDialog();

                    if (file == DialogResult.OK)
                    {
                        if (_fileMgr.DetermineRewritePath(fileDialog.FileName, filename))
                        {
                            done = true;
                            break;
                        }
                    }
                    else if (file == DialogResult.Cancel)
                    {                        
                        MessageBox.Show("Nothing I can do then. CIAO!");
                        this.StopDebuggingSession();
                        return false;
                    }
                }

                localFilename = _fileMgr.getLocalFilename(remoteFilename);

            }
            else
            {
                /* User could've opened a file before hand. Figure out if we can turn it into a
                 * remote path. */

                string tmpLocalFilename = _fileMgr.getLocalFilename(localFilename);
                remoteFilename = localFilename;
                localFilename = tmpLocalFilename;
            }         
           
            xdc.Forms.SourceFileForm f = new xdc.Forms.SourceFileForm(localFilename, remoteFilename, _client);
            
            f.FormClosed += new FormClosedEventHandler(SourceFileForm_FileClosed);
            f.Text    = baseFile;
            f.TabText = localFilename;                    

            f.Show(this.dockPanel, DockState.Document);
            

            _fileMgr.Add(remoteFilename, f);

            closeToolStripMenuItem.Enabled = true;

            // We have to collect all the bookmarks added and removed. This way
            // XdebugClient is able to restore bookmarks whenever a script is rerun.
            f.getBookmarkManager().Added   += new BookmarkEventHandler(BreakpointAdded);
            f.getBookmarkManager().Removed += new BookmarkEventHandler(BreakpointRemoved);

            return true;
        }
        #endregion
    
        #region XDebugClient events

        /* The methods in this region aren't neccessarily real events as
         * C# defines them. Some of them (those called from _client_EventCallback) 
         * as just regular methods. 
         * 
         * The callback XDebugEventCallback is used as a centralized
         * place to further instruct the GUI what to do. We use only one event as 
         * the client uses asynchronized methods (threading). By using 1 callback the
         * number of threading-related reinvoking (see MainForm.ReinvokeInOwnThread) 
         */

        /// <summary>
        /// The XDebugEventCallback is called whenever something changes within the Xdebug.Client
        /// implementation. It serves mostly as a dispatcher.
        /// </summary>                
        private bool XDebugEventCallback(XDebugEventArgs e)
        {
            if (!this.ReinvokeInOwnThread(new XdebugClientCallback(XDebugEventCallback), new object[] { e }))
            {
                switch (e.EventType)
                {                  
                    case XDebugEventType.DebuggerConnected:
                        this.OnXdebuggerConnected(e);
                        break;

                    case XDebugEventType.ConnectionInitialized:                                             
                        return this.OnXdebugConnectionInitialized(e);                                              

                    case XDebugEventType.MessageReceived:
                        this.OnXdebugMessageReceived(e);
                        break;

                    case XDebugEventType.CommandSent:
                        this.OnXdebugCommandSent(e);
                      
                        break;

                    case XDebugEventType.BreakpointHit:
                        this.OnXdebugBreakpointHit(e);
                        break;

                    case XDebugEventType.ErrorOccured:
                        this.OnXdebugErrorOccurred(e);                  
                        break;

                    case XDebugEventType.ScriptFinished:
                        this.OnXdebugScriptFinished(e);                       
                        break;

                    default:
                        WriteDebugLine("(!) Unknown event happened.");
                        break;
                }

               
            }

            return false;
        }

        private void OnXdebuggerConnected(XDebugEventArgs e)
        {
            _statusFrm.WriteStatusLine("(-) Debugger connected.");
            
            try {
                if (_client.Initialize())
                {
                    _statusFrm.WriteStatusLine("(-) XDebugClient initialized.");
                }
                else
                {                  
                    return;
                }
            } 
            catch (Exception ex)
            {
                _statusFrm.WriteStatusLine("(-) Cannot initialize XDebugClient: " + e.Message);
                
                MessageBox.Show(
                    "XDebugClient was unable to initialize. Debugging session terminated.\r\n\r\n" + ex.Message,
                    "System error",
                    MessageBoxButtons.OK
                );

                this.StopDebuggingSession();
            }
          
        }

        private void OnXdebugCommandSent(XDebugEventArgs e)
        {
            WriteDebugLine(" -> SENT: " + e.Message.RawMessage);
        }

        private void OnXdebugBreakpointHit(XDebugEventArgs e)
        {
            this.PrepareFileForAccess(e.CurrentLocation.filename);
            this.SetActiveFileAndLine(e.CurrentLocation);

            List<StackEntry> callstack = _client.GetCallStack(-1);
            _callstackFrm.setCallstack(callstack);
        }

        private void OnXdebugErrorOccurred(XDebugEventArgs e)
        {
            if (e.ErrorType == XDebugErrorType.Warning)
            {
                WriteDebugLine("(!) PHP Notice: " + e.ErrorMessage);
                this._client.Run();
            }
            else
            {
                WriteDebugLine("(!) PHP Fatal error: " + e.ErrorMessage);

                MessageBox.Show(
                    "A Fatal error occurred:\r\n\r\n" + e.ErrorMessage + "\r\n\r\nYour script has been terminated.",
                    "Fatal Error",
                    MessageBoxButtons.OK
                );
     
                this.StopDebuggingSession();
            }
        }

        private void OnXdebugScriptFinished(XDebugEventArgs e)
        {
            _statusFrm.WriteStatusLine("(!) Script finished.");

            this.StopDebuggingSession();
        }

        private void OnXdebugMessageReceived(XDebugEventArgs e)
        {
            // Nothing to do for now
        }

        private bool OnXdebugConnectionInitialized(XDebugEventArgs e)
        {
          
            this.ToggleMenuItems(true);
                                 
            return this.LoadFile(e.Filename);
        }

        #endregion

        #region TextEditor events (breakpoints)
        private void BreakpointAdded(object sender, BookmarkEventArgs e)
        {
            if (e.Bookmark is Breakpoint)
            {
                Breakpoint b = e.Bookmark as Breakpoint;
                WriteDebugLine(String.Format("Breakpoint added: {0}:{1}", b.filename, b.LineNumber));

                _breakpointMgr.Record(b, BreakpointState.Added);
            }
        }

        private void BreakpointRemoved(object sender, BookmarkEventArgs e)
        {
            if (e.Bookmark is Breakpoint)
            {
                Breakpoint b = e.Bookmark as Breakpoint;
                WriteDebugLine(String.Format("Breakpoint removed: {0}:{1}", b.filename, b.LineNumber));
                _breakpointMgr.Record(b, BreakpointState.Removed);
            }
        }

        #endregion

        #region Winforms events
        private void SourceFileForm_FileClosed(object sender, FormClosedEventArgs e)
        {
            xdc.Forms.SourceFileForm f = sender as xdc.Forms.SourceFileForm;

            _fileMgr.Remove(f.getFilename());
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult r = openFileDialog1.ShowDialog();

            if (r == DialogResult.OK)
            {
                this.LoadFile(openFileDialog1.FileName);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _callstackFrm.Show(this.dockPanel, DockState.DockBottom);
            _statusFrm.Show(_callstackFrm.Pane, _callstackFrm);
        }
       
        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SendContinuationCommand("run");
        }
      
        private void startListeningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _statusFrm.WriteStatusLine("(-) Waiting for xdebug to connect");

            try
            {
                _client.listenForConnection();

                startListeningToolStripMenuItem.Enabled = false;
                stopDebuggingToolStripMenuItem.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to create listening socket: " + ex.Message,
                    "Cannot open socket",
                    MessageBoxButtons.OK
                );
            }
        }

        private void stopDebuggingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.StopDebuggingSession();

            _statusFrm.WriteStatusLine("(-) Stopped and disconnected.");

            stopDebuggingToolStripMenuItem.Enabled = false;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            xdc.Forms.AboutForm aboutFrm = new xdc.Forms.AboutForm();
            aboutFrm.ShowDialog();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.dockPanel.ActiveDocument.DockHandler.Close();

            if (!_fileMgr.HasOpenFiles)
            {
                closeToolStripMenuItem.Enabled = false;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_client.State != XdebugClientState.Uninitialized)
            {
                _client.Disconnect();
            }

            this.Close();
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            /* Only allow F5 if we're not on a breakpoint. Step over/step in in 
             * the Initialized state is useless. */

     /*       if (_client.State != XdebugClientState.Break && e.KeyCode != Keys.F5)
            {
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.F5:
                    this.SendContinuationCommand("run");
                    e.Handled = true;
                    break;

                case Keys.F10:
                    this.SendContinuationCommand("step_over");
                    e.Handled = true;
                    break;

                case Keys.F11:
                    this.SendContinuationCommand("step_into");
                    e.Handled = true;
                    break;

                case Keys.F12:
                    this.SendContinuationCommand("step_out");
                    e.Handled = true;
                    break;
            } */
        } 
        #endregion

        #region General helpers
        private void SendContinuationCommand(string command)
        {
            if (_client.State == XdebugClientState.Initialized)
            {
                /* Keep all the breakpoints of the files that are still open.
                 * This should probably be replaced by code in the SourceFileView_closing
                 * form triggering removal of all it's breakpoints.
                 */
                foreach (Breakpoint p in _breakpointMgr.Breakpoints)
                {
                    if (_fileMgr.getFormByRemoteFilename(p.filename) != null)
                    {
                        if (!_breakpointMgr.AddedBreakpoints.Contains(p))
                            _breakpointMgr.AddedBreakpoints.Add(p);
                    }
                    else
                    {
                        _breakpointMgr.Breakpoints.Remove(p);
                    }
                }
            }

            foreach (Breakpoint b in _breakpointMgr.AddedBreakpoints)
            {
                _client.AddBreakpoint(b);
            }

            foreach (Breakpoint b in _breakpointMgr.RemovedBreakpoints)
            {
                _client.RemoveBreakpoint(b);
            }

            _breakpointMgr.FlushDelta();

            _client.Run(command);
        }


        #endregion

        private void stepOverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_client.State == XdebugClientState.Break)
                this.SendContinuationCommand("step_over");

        }

        private void stepInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_client.State == XdebugClientState.Break)
                this.SendContinuationCommand("step_into");
        }

        private void stepOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_client.State == XdebugClientState.Break)
                this.SendContinuationCommand("step_out");
        }

    }
}