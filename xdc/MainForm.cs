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
using System.Diagnostics;
using System.IO;

using WeifenLuo.WinFormsUI.Docking;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

using xdc.XDebug;
using xdc.GUI;
using xdc.GUI.FileLoader;

namespace xdc
{
    public partial class MainForm : Form
    {
        private xdc.Forms.StatusForm _statusFrm;
        private xdc.Forms.CallstackForm _callstackFrm;
        private xdc.Forms.ContextForm _localContextFrm;
        private xdc.Forms.ContextForm _globalContextFrm;

        private xdc.XDebug.Client _client;
        
        private BreakpointManager _breakpointMgr;
        private xdc.GUI.FileLoader.FileLoader _fileLoader;

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
            _localContextFrm = new xdc.Forms.ContextForm(_client, "Locals"); // todo, name etc
            _globalContextFrm = new xdc.Forms.ContextForm(_client, "Globals"); // todo, name etc

            // Helper objects
            _breakpointMgr = new BreakpointManager();
            _fileMgr = new FileManager();
           

            _CurrentLocation = new Location();
            _CurrentLocation.line = -1;

            this.KeyPreview = true;            

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
                if (_fileLoader != null && _fileLoader.AllowsOpenFileDialog)
                {
                    openToolStripMenuItem.Enabled = true;
                }

                runToolStripMenuItem.Enabled = true;
                stepInToolStripMenuItem.Enabled = true;
                stepOutToolStripMenuItem.Enabled = true;
                stepOverToolStripMenuItem.Enabled = true;

                startListeningToolStripMenuItem.Enabled = false;
                stopDebuggingToolStripMenuItem.Enabled = true;                
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
                closeToolStripMenuItem.Enabled = false;
            }

        }

        private void StopDebuggingSession()
        {
            _fileLoader = null;

            if (_CurrentLocation.line != -1)
            {
                xdc.Forms.SourceFileForm form = _fileMgr.getFormByRemoteFilename(_CurrentLocation.filename);
                
                if (form != null)
                    form.RemoveActiveMark();                
            }

            _client.Disconnect();

            foreach (OpenFileStruct file in _fileMgr.Forms.Values)
            {
                file.form.ToggleMenuItems(false);
            }

            this.ToggleMenuItems(false);

            _statusFrm.WriteStatusLine("(!) Debugging session terminated.");
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
        }
                  
        private bool LoadFile(string filename)
        {
            /**
             * Loading a file:
             *  1. Local file which can be used 1-on-1
             *  2. "Local" file which needs rewriting (FTP Location Service)
             *  3. Remote file, not accessible via Samba.
             *  4. Remote file, accessible via Samba.
             * 
             * Situations 1, 2 and 4 can be resolved using a simple filename rewriter. In case (1)
             * no rewriting has to be done, in case (2) complete rewriting has be done and in case (3)
             * partial rewriting has to be done.
             * 
             * Situation 3 can be resolved by using the Source DBGP command, but that leaves us in the dark
             * as to what other files are available which makes debugging rather annoying when dealing
             * with large systems. It can also take a very long time with loading large files, at least 
             * with the current implementation.
             */             
            
            string baseFile = System.IO.Path.GetFileName(filename);
            
            string remoteFilename = filename;
            string localFilename = filename;

            xdc.Forms.SourceFileForm sff = _fileMgr.getFormByRemoteFilename(filename);
            
            if (sff != null)
            {
                sff.Focus();
                return true;
            }

            xdc.Forms.SourceFileForm f = new xdc.Forms.SourceFileForm(_client, filename);
            

            f.FormClosed += new FormClosedEventHandler(SourceFileForm_FileClosed);
            f.Text = baseFile;
            f.TabText = localFilename;                    

            /* When we can't find the file specified, offer a way to rewrite the path */
            if (!System.IO.File.Exists(filename))
            {

                if (_fileLoader == null)
                {
                    xdc.Forms.FileHandlingForm filehandlingForm = new xdc.Forms.FileHandlingForm(filename);
                    DialogResult handlerResult = filehandlingForm.ShowDialog();

                    if (handlerResult != DialogResult.OK)
                    {
                        MessageBox.Show("Can't debug without a source file. Terminating debug session.", "No file loaded.");
                        this.StopDebuggingSession();
                        return false;
                    }

                    _fileLoader = FileLoaderFactory.Create(filehandlingForm.SelectedFileLoader);
                    _fileLoader.setClient(_client);

                }

                bool fileLoaded = false;

                if (_fileLoader.DetermineLocalFilename(filename, ref localFilename))
                {
                    if (_fileLoader.OpenFile(f, localFilename))
                    {
                        fileLoaded = true;
                    }
                }

                if (!fileLoaded)
                {
                    MessageBox.Show("Can't debug without a source file. Terminating debug session.", "No file loaded");
                    this.StopDebuggingSession();
                    return false;
                }
            }
            else
            {
                /* The user might've opened a file that appears to be local (opened via network, for instance)
                 * that should be mapped to a remote path. If we have a fileLoader instance, see if it 
                 * can rewrite the file for us. */

                if (_fileLoader != null)
                {
                    string tmpRemoteFilename = "";
                    if (_fileLoader.DetermineRemoteFilename(localFilename, ref tmpRemoteFilename))
                    {
                        f.TabText = tmpRemoteFilename;

                        /* The filename in the SourceFileForm is always considered
                         * to be the filename expected by xdebug, so update it to
                         * the remote version */
                        f.setFilename(tmpRemoteFilename);
                        filename = tmpRemoteFilename;
                    }
                }

                f.LoadFile(localFilename);                
            }

            _fileMgr.Add(filename, localFilename, f);
            f.Show(this.dockPanel, DockState.Document);
                       
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

                    if (!xdc.Properties.Settings.Default.break_on_script_start)
                        this.SendContinuationCommand("run");
                    else
                        this.SendContinuationCommand("step_into");
                }
                else
                {                  
                    return;
                }
            } 
            catch (Exception ex)
            {
                _statusFrm.WriteStatusLine("(-) Cannot initialize XDebugClient: " + ex.Message);
                
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
            // local and global context
            List<Property> ctx = _client.getContext("0", 0);
            _localContextFrm.LoadPropertyList(ctx);
            ctx = _client.getContext("1", 0);
            _globalContextFrm.LoadPropertyList(ctx);
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

            if (xdc.Properties.Settings.Default.auto_restart)
            {
                _statusFrm.WriteStatusLine("(-) Automatically restarting debugging.");

                try
                {
                    _client.listenForConnection();

                    startListeningToolStripMenuItem.Enabled = false;
                    stopDebuggingToolStripMenuItem.Enabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Unable to re-create listening socket: " + ex.Message,
                        "Cannot open socket",
                        MessageBoxButtons.OK
                    );
                }
            }
        }

        private void OnXdebugMessageReceived(XDebugEventArgs e)
        {
            // Nothing to do for now
        }

        private bool OnXdebugConnectionInitialized(XDebugEventArgs e)
        {                     
            if (this.LoadFile(e.Filename))
            {
                this.ToggleMenuItems(true);
                return true;
            }

            return false;
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
            string localFilename = _fileMgr.GetLocalFilename(f.getFilename());

            _fileMgr.Remove(localFilename);

            closeToolStripMenuItem.Enabled = _fileMgr.HasOpenFiles;
            
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
            _localContextFrm.Show(this.dockPanel, DockState.DockBottom);
            _globalContextFrm.Show(this.dockPanel, DockState.DockBottom);
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
                int port;

                try
                {
                    port = Convert.ToInt32(xdc.Properties.Settings.Default.listening_port);
                }
                catch
                {
                    MessageBox.Show("An invalid port number has been set in options, using 9000.", "Invalid port number");
                    port = 9000;
                }


                _client.setListeningPort(port);
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
            IDockContent content = this.dockPanel.ActiveDocument;

            if (content is xdc.Forms.SourceFileForm)
            {
                xdc.Forms.SourceFileForm form = (content as xdc.Forms.SourceFileForm);
                string localFilename = _fileMgr.GetLocalFilename(form.getFilename());

                if (localFilename != "")
                    _fileMgr.Remove(localFilename);
            }
            
            if (!_fileMgr.HasOpenFiles)
            {
                closeToolStripMenuItem.Enabled = false;
            }
            content.DockHandler.Close();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_client.State != XdebugClientState.Uninitialized)
            {
                _client.Disconnect();
            }

            this.Close();
        }
       
        #endregion

        #region General helpers
        private void SendContinuationCommand(string command)
        {
            if (_client.State == XdebugClientState.Initialized)
            {
                /* Keep all the breakpoints of the files that are still open. */                                 
                foreach (Breakpoint p in _breakpointMgr.Breakpoints)
                {
                    if (_fileMgr.getFormByRemoteFilename(p.filename) != null)
                    {
                        if (!_breakpointMgr.AddedBreakpoints.Contains(p))
                            _breakpointMgr.AddedBreakpoints.Add(p);
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

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            xdc.Forms.ConfigForm cf = new xdc.Forms.ConfigForm();
            cf.ShowDialog();
        }
    }
}