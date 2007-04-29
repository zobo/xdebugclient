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
using System.Xml;

namespace xdc.XDebug
{
    /// <summary>
    /// The XDebugMessage parser knows of three types of messages:
    /// 
    ///     - Init message
    ///     - Error message
    ///     - Response message
    /// 
    /// For the Response message the parser will fetch the Transaction ID and
    /// the command name for future reference.
    /// </summary>
    public class Response
    {
        public const int MSG_INIT = 0;
        public const int MSG_ERROR = 1;
        public const int MSG_RESPONSE = 2;

        public int MessageType;
        public string TransactionID;

        public Dictionary<string, string> Attributes;
        public String RawMessage;
        public XmlDocument XmlMessage;

        public Response()
        {
            Attributes = new Dictionary<string, string>();
        }

        public string TypeToName()
        {
            switch (MessageType)
            {
                case MSG_INIT:
                    return "INIT";

                case MSG_RESPONSE:
                    return "RESPONSE";

                case MSG_ERROR:
                    return "ERROR";

                default:
                    return "UNKNOWN";
            }
        }

        public static XDebug.Response Parse(String xml)
        {
            System.Xml.XmlDocument d = new XmlDocument();
            d.LoadXml(xml);
            
            XDebug.Response m = new XDebug.Response();
            m.RawMessage = xml;
            m.XmlMessage = d;

            string messageType = d.DocumentElement.Name.ToLower();

            switch (messageType)
            {
                case "init":
                    m.MessageType = XDebug.Response.MSG_INIT;

                    string fileUri =
                        d.DocumentElement.Attributes["fileuri"].Value;

                    m.Attributes.Add("file", fileUri);

                    break;

                case "response":
                    bool isError = false;
                    //d.DocumentElement.FirstChild.Name.ToLower() == "error";

                    if (isError)
                    {
                        m.MessageType = XDebug.Response.MSG_ERROR;
                        m.Attributes.Add(
                            "ErrorMessage",
                            d.DocumentElement.FirstChild.InnerText
                        );

                        return m;                        
                    }

                    m.MessageType = XDebug.Response.MSG_RESPONSE;

                    //  m.TransactionID = d.DocumentElement.Attributes["transaction_id"].Value;
                    if (d.DocumentElement.Attributes["status"] != null)
                    {
                        m.Attributes.Add(
                            "status",
                            d.DocumentElement.Attributes["status"].Value
                        );
                    }


                    break;

                default:
                    throw new Exception("Unknown message type: " + messageType);
            }

            return m;
        }
    }
}
