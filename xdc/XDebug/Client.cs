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
using System.Web;
using System.Net;
using System.Net.Sockets;
using System.Xml;

namespace xdc.XDebug
{
    public delegate bool XDebugEventHandler(XDebugEventArgs e);
    
    public enum XdebugClientState { Uninitialized = 0, Initialized, Break, Stopped }
    public enum XDebugEventType { DebuggerConnected, ConnectionInitialized, CommandSent, MessageReceived, BreakpointHit, ScriptFinished, ErrorOccured, SystemError };
    public enum XDebugErrorType { FatalError, Warning }

    public class Client
    {
        const string supportedProtocolVersion = "1.0";
        const string supportedLanguage = "php";

        const string FatalErrorExceptionName = "Fatal error";
        const string NoticeExceptionName = "Notice";

        private int       _cmdCounter = 0;
        private int       _port;
        private string    _host;
        private Socket    _listener;
        private Socket    _client;

        private XdebugClientState _State = XdebugClientState.Uninitialized;        
        public XdebugClientState State { get { return _State; } }

        public event XDebugEventHandler EventCallback;    

        public Client(string host, int port)
        {           
            _port = port;
            _host = host;
        }

        #region Public interface

        /// <summary>
        /// Tells XDebug our preferences.
        /// </summary>        
        public bool Initialize()
        {
           /* As soon as we've been connected we'll receive the DBGP init 
            * message. Parse it into a XDebugMessage and pass it to the GUI so
            * it can load the initial file. */

            XDebug.Response initMessage = this.ReceiveMessage();

            /* This might throw a NullReference exception. */
            try
            {
                if (this.handleInitMessage(initMessage))
                {
                    /* We'd like to know when Fatal errors occur */
                    XDebug.Command c = new Command("breakpoint_set", "-t exception -x \"Fatal error\"");
                    XDebug.Response r = this.SendCommand(c);

                    /* We'd like to know when Notices occur */
                    XDebug.Command c2 = new Command("breakpoint_set", "-t exception -x \"Notice\"");
                    XDebug.Response r2 = this.SendCommand(c2);
                  
                    return true;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Initialization failed: " + e.Message);
            }

            return false;
        }

        /// <summary>
        /// Retrieve the source code to the specified file.
        /// </summary>        
        public string Source(string URI)
        {
            string remoteURI = "file://";

            if (URI[0] != '/')
                remoteURI += "/";

            remoteURI += System.Web.HttpUtility.UrlPathEncode(URI);

            XDebug.Command c = new Command("source", "-f " + remoteURI);
            XDebug.Response r = this.SendCommand(c);

            if (r == null)
            {
                throw new Exception("Parse error");
            }

            return base64ToASCII(r.XmlMessage.InnerText);
        }
       
        /// <summary>
        /// Send a basic 'run' command.
        /// </summary>
        public void Run()
        {
            this.Run("run");
        }

        /// <summary>
        /// Send a run, step_over or step_in command.
        /// </summary>        
        public void Run(string command)
        {
            XDebug.Command c = new Command(command, "");

            XDebug.Response resp = this.SendCommand(c);
            string status = resp.XmlMessage.DocumentElement.Attributes["status"].Value;
            string reason = resp.XmlMessage.DocumentElement.Attributes["reason"].Value;

            XDebugEventArgs e;

            if (reason == "exception")
            {
                XmlNode ErrorNode = resp.XmlMessage.DocumentElement.FirstChild;

                this._State = XdebugClientState.Stopped;

                e              = new XDebugEventArgs();
                e.EventType    = XDebugEventType.ErrorOccured;
                e.ErrorMessage = ErrorNode.InnerText;

                switch ( ErrorNode.Attributes["exception"].Value )
                {
                    case Client.FatalErrorExceptionName:
                        e.ErrorType = XDebugErrorType.FatalError;
                        break;

                    case Client.NoticeExceptionName:
                        e.ErrorType = XDebugErrorType.Warning;
                        break;

                    default:
                        throw new Exception("Unknown exception type");
                }
                
                this.EventCallback(e);

                return;
            }
            else
            {

                switch (status)
                {
                    case "break":
                        /* execution stopped: breakpoint, step_over/step_in result, etc. */

                        this._State = XdebugClientState.Break;

                        List<StackEntry> CallStack = this.GetCallStack(0);

                        e = new XDebugEventArgs();

                        e.CurrentLocation = new Location();

                        /* Linenumbers have to be zero based. Xdebug uses 1-based. */
                        e.CurrentLocation.filename = CallStack[0].fileName;
                        e.CurrentLocation.line = CallStack[0].lineNumber - 1;

                        e.EventType = XDebugEventType.BreakpointHit;

                        this.EventCallback(e);

                        break;

                    case "stopped":
                        /* Script's done. */

                        this.Disconnect();

                        e = new XDebugEventArgs();
                        e.EventType = XDebugEventType.ScriptFinished;

                        this.EventCallback(e);

                        break;

                    default:
                        throw new Exception("Unknown status: " + status);
                }

            }
        }

        /// <summary>
        /// Stop the active connection, if available. Terminate the listening socket.
        /// </summary>
        public void Disconnect()
        {
            if (this._client != null)
                this._client.Disconnect(false);

            if (this._listener != null)
            {
                this._listener.Close();
            }


            this._State = XdebugClientState.Uninitialized;
        }

        /// <summary>
        /// Tell xdebug to set a line-based breakpoint. Other types
        /// not yet supported.
        /// </summary>        
        public void AddBreakpoint(Breakpoint brk)
        {
            string filename = "file://";

            if (brk.filename[0] != '/')
                filename += "/";

            filename += System.Web.HttpUtility.UrlPathEncode(brk.filename);
            XDebug.Command c = new Command(
                "breakpoint_set",
                String.Format(
                    "-t line -f {0} -n {1}",  
                    filename, brk.LineNumber_XDebug )
            );

            XDebug.Response resp = this.SendCommand(c);

            brk.xdebugId = resp.XmlMessage.DocumentElement.Attributes["id"].Value;

        }

        /// <summary>
        /// Tell xdebug to remove a line-based breakpoint. Other types 
        /// not yet supported.
        /// </summary>        
        public void RemoveBreakpoint(Breakpoint brk)
        {
            XDebug.Command c = new Command(
                "breakpoint_remove",
                String.Format("-d {0}", brk.xdebugId)
            );

            XDebug.Response resp = this.SendCommand(c);
        }

        /// <summary>
        /// Get the callstack up to this point.
        /// </summary>        
        public List<StackEntry> GetCallStack(int Depth)
        {
            string options = "";

            if (Depth != -1)
                options = "-d " + Depth;
        
            XDebug.Command c = new Command("stack_get", options);
            XDebug.Response m = this.SendCommand(c);

            List<StackEntry> CallStack = new List<StackEntry>();

            foreach (XmlElement n in m.XmlMessage.DocumentElement.ChildNodes)
            {
                if (n.Name.ToLower() == "stack")
                {
                    StackEntry se = new StackEntry();

                    se.fileName = this.getLocalFilename(n.Attributes["filename"].Value);
                    se.lineNumber = System.Convert.ToInt32(n.Attributes["lineno"].Value);
                    se.level = System.Convert.ToInt32(n.Attributes["level"].Value);
                    se.location = n.Attributes["where"].Value;
                    
                    CallStack.Add(se);
                }
            }

            return CallStack;

        }

        /// <summary>
        /// Get the value of property. If the result is paged, xdebug returns at most
        /// 32 entries by default, all pages are fetched. Returns a tree of Property instances.      
        /// </summary>        
        public Property GetPropertyValue(string name, int depth)
        {            
            XDebug.Command c;
            XDebug.Response resp;
            Property theProperty;
            XmlNode propertyNode;

            c = new Command(
                "property_get",
                String.Format("-d {0} -n {1}", depth, name)
            );

            resp = this.SendCommand(c);

            if (resp == null)
            {
                return null;
            }

            propertyNode = resp.XmlMessage.DocumentElement.FirstChild;

            if (propertyNode.Name == "property")
            {                   
                theProperty = Property.Parse(propertyNode);

                /* Try to fetch all pages for a property if the 'pages'
                 * attribute is set in the result. */                 
                if (propertyNode.Attributes["page"] != null)
                {

                    int currentPage = Convert.ToInt32(propertyNode.Attributes["page"].Value);
                    int pageSize = Convert.ToInt32(propertyNode.Attributes["pagesize"].Value);
                    int numChildren = Convert.ToInt32(propertyNode.Attributes["numchildren"].Value);

                    /* The DBGP spec suggests in section 7.13 that the IDE should implement 
                     * a UI for paging. I'm not sure what kind of UI I would present
                     * to a user that's trying to inspect a simple but large (>32 element) array. So, I 
                     * just fetch all the pages if there are any. */

                    while ((currentPage++ * pageSize) < numChildren)
                    {                     
                        c = new Command(
                            "property_get",
                            String.Format("-d {0} -n {1} -p {2}", depth, name, currentPage)
                        );

                        resp = this.SendCommand(c);

                        if (resp != null)
                        {
                            propertyNode = resp.XmlMessage.DocumentElement.FirstChild;

                            if (propertyNode.Name == "property")
                            {
                                Property tmpProperty = Property.Parse(propertyNode);

                                foreach (Property tmpChildProperty in tmpProperty.ChildProperties)
                                {
                                    theProperty.ChildProperties.Add(tmpChildProperty);
                                }
                            }

                            if (!theProperty.isComplete)
                                theProperty.isComplete = true;
                        }
                        else
                        {
                            /* Error fetching a property page */
                            return null;
                        }
                    }
                }

                return theProperty;
            }

            return null;
        }
        #endregion

        #region Networking

        /// <summary>
        /// Start the listening socket. Might throw an Exception when the 
        /// port is currently in use.
        /// </summary>
        public void listenForConnection()
        {
            _listener = new Socket(AddressFamily.InterNetwork,
                              SocketType.Stream, ProtocolType.Tcp);

            _listener.Bind(new IPEndPoint(IPAddress.Any, 9000));
            _listener.Listen(1);

            AsyncCallback c = new AsyncCallback(OnConnectRequest);
            
            _listener.BeginAccept(new AsyncCallback(OnConnectRequest), _listener);
        }

        /// <summary>
        /// Deal with an incoming request. Also called when listening socket
        /// is terminated.
        /// </summary>        
        private void OnConnectRequest(IAsyncResult ar)
        {
            try
            {
                Socket listener = (Socket)ar.AsyncState;
                _client = listener.EndAccept(ar);

                _listener.Close();

                XDebugEventArgs e = new XDebugEventArgs();
                e.EventType = XDebugEventType.DebuggerConnected;

                this.EventCallback(e);
            }
            catch
            {
                /* This can probably dealt with in a less hacky way:
                 * 
                 * After closing the connection OnConnectRequest will be called. Then it dies
                 * with a ObjectDisposedException. Trap the exception. Not really anything else
                 * to do here. */
            }
        }                

        /// <summary>
        /// Parse a response by XDebug. Xdebug sends messages in 2 parts terminated 
        /// by \0. The first part of the message is the length of the second part 
        /// of the message.
        /// </summary>        
        private XDebug.Response ReceiveMessage()
        {
            List<byte> MessageLengthList = new List<byte>();
            Byte[] c = new Byte[1];

            /* Determine the length of the message byte-by-byte */
            do
            {
                _client.Receive(c, 1, SocketFlags.None);

                if (c[0] != (byte)0x00)
                {
                    MessageLengthList.Add(c[0]);
                }
            } while (c[0] != (byte)0x00);

            /* Turn the MessageLengthList into a number by merging it
             * into a byte array, casting it to a string and then parsing
             * the integer from it. I wonder if there's a better way to do this.*/

            byte[] lengthBytes = MessageLengthList.ToArray();
            string lengthStr = System.Text.Encoding.ASCII.GetString(lengthBytes);
            int length = Convert.ToInt32(lengthStr);
        
            /* The message length doesn't include the trailing NULL byte. Add it here */
            length++;
                                   
            byte[] messageBytes = new byte[length];
            int bytesRead = 0, totalBytesRead = 0, currentByte = 0;

            do
            {
                byte[] xmlMessageBytes = new byte[length];
                bytesRead = _client.Receive(xmlMessageBytes, length, SocketFlags.None);

                if (bytesRead == 0 || bytesRead < 0)
                    throw new Exception("Socket read error");

                totalBytesRead += bytesRead;

                for (int i = 0; i < bytesRead; i++)
                {
                    messageBytes[currentByte++] = xmlMessageBytes[i];
                }

            } while (totalBytesRead < length);

            string xmlMessage    = System.Text.Encoding.ASCII.GetString(messageBytes);
                      
            XDebug.Response resp = XDebug.Response.Parse(xmlMessage);

            if (resp != null)
            {
                if (resp.MessageType == XDebug.Response.MSG_ERROR)
                {
                    throw new Exception(resp.Attributes["ErrorMessage"]);
                }

                XDebugEventArgs xev = new XDebugEventArgs();
                xev.Message = resp;
                xev.EventType = XDebugEventType.MessageReceived;

                this.EventCallback(xev);
            }     
          

            return resp;

        }

        /// <summary>
        /// Send a command to XDebug. Returns a instance of the Response object or null
        /// upon failure.
        /// </summary>        
        public XDebug.Response SendCommand(XDebug.Command c)
        {
            string transactionId = "xdc" + _cmdCounter.ToString();
            string Message = "";

            _cmdCounter++;
            
            Message = String.Format(
                "{0} -i {1}",
                c.CommandText,
                transactionId                
            );

            if (c.OptionsText.Length != 0)
            {
                Message += " " + c.OptionsText;
            }
          
            XDebugEventArgs e = new XDebugEventArgs();
            e.Message            = new XDebug.Response();
            e.Message.RawMessage = Message;
            e.EventType          = XDebugEventType.CommandSent;

            this.EventCallback(e);            

            byte[] msg = System.Text.Encoding.ASCII.GetBytes(Message + "\0");
            this._client.Send(msg);

            return this.ReceiveMessage();            
        }
        #endregion

        #region Parsing / Handling
        /// <summary>
        /// Deal with the init-message xdebug sends us:
        ///     - See if we seem to be compatible language wise
        ///     - See if we're compatible protocol wise
        ///     - Find the initial file and fire off a ConnectionInitialized "event"
        /// 
        /// Returns true/false        
        /// </summary>        
        private bool handleInitMessage(XDebug.Response initMessage)
        {
            if (initMessage == null)
            {
                throw new Exception("Init message was empty.");
            }

            /* parse out the filename and check wether the version is 
             * compatible with XdebugClient */

            XmlElement d = initMessage.XmlMessage.DocumentElement;

            if (d.Attributes["protocol_version"] != null)
            {
                string remoteVersion = d.Attributes["protocol_version"].Value;

                if (remoteVersion != supportedProtocolVersion)
                {
                    throw new Exception(
                        String.Format(
                            "Expected version '{0}' but got version '{1}' which is not supported.'",
                            supportedProtocolVersion,
                            remoteVersion
                        )
                    );
                    
                }
            }

            if (d.Attributes["language"] != null)
            {
                string remoteLanguage = d.Attributes["language"].Value;

                if (remoteLanguage.ToLower() != supportedLanguage)
                {
                    throw new Exception(
                        String.Format(
                            "Expected language '{0}' but got '{1}' which is not supported.",
                            supportedLanguage,
                            remoteLanguage
                        )
                    );                 
                }
            }

            if (d.Attributes["fileuri"] != null)
            {
                string absoluteFilename = this.getLocalFilename(d.Attributes["fileuri"].Value);

                XDebugEventArgs xea = new XDebugEventArgs();
                xea.Filename = absoluteFilename;
                xea.EventType = XDebugEventType.ConnectionInitialized;

                if (this.EventCallback(xea))
                {
                    _State = XdebugClientState.Initialized;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                throw new Exception("Missing 'fileuri' attribute.");
            }           

            return true;
        }
        #endregion

        #region Helpers

        /// <summary>
        /// Return a url-decoded version of the given filename.
        /// </summary>        
        private string getLocalFilename(String rawFilename)
        {
            string filename = System.Web.HttpUtility.UrlDecode(rawFilename);
            Uri fileUri = new Uri(filename);

            return fileUri.LocalPath;
        }

        /// <summary>
        /// Turn Base64 encoded text into ASCII.
        /// </summary>        
        private string base64ToASCII(string text)
        {
            byte[] todecode_byte = Convert.FromBase64String(text);
            System.Text.Decoder decoder = new System.Text.ASCIIEncoding().GetDecoder();

            int charCount = decoder.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] decoded_char = new char[charCount];
            decoder.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);

            return new String(decoded_char);
        }
        #endregion

    }
}
