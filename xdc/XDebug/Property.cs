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
    public enum PropertyType {Scalar, Array, Object}

    public class Property
    {
        public string       Name;
        public string       Value;
        public PropertyType Type;
        public string       FullName;
        public bool         isComplete;

        public List<Property> ChildProperties;

        public Property() 
        {
            ChildProperties = new List<Property>();
        }

        public Property(string name, string val, PropertyType type, bool complete, string fullname)
        {
            this.Name = name;
            this.Value = val;
            this.Type = type;
            this.isComplete = complete;
            this.FullName = fullname;
            ChildProperties = new List<Property>();
        }

        static public Property Parse(XmlNode firstProperty)
        {                       
            Property rootProperty = new Property();

            rootProperty.Name = firstProperty.Attributes["name"].Value;
            rootProperty.FullName = firstProperty.Attributes["fullname"].Value;

            string propType = firstProperty.Attributes["type"].Value;

            if (propType != "array" && propType != "object")
            {               
                if (firstProperty.Attributes["encoding"] != null)
                {
                    if (firstProperty.Attributes["encoding"].Value == "base64")
                    {
                        byte[] todecode_byte = Convert.FromBase64String(firstProperty.InnerText);
                        System.Text.Decoder decoder = new System.Text.ASCIIEncoding().GetDecoder();
                                            
                        int charCount = decoder.GetCharCount(todecode_byte, 0, todecode_byte.Length);
                        char[] decoded_char = new char[charCount];
                        decoder.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
                        string result = new String(decoded_char);

                        rootProperty.Value = result;
                    }
                }
                else
                {
                    rootProperty.Value = firstProperty.InnerText;
                }

                rootProperty.isComplete = true;
                rootProperty.Type = PropertyType.Scalar;

                return rootProperty;
            }
            else
            {
                rootProperty.isComplete = false;
                rootProperty.Type = (propType == "array") ? PropertyType.Array : PropertyType.Object;

                if (propType == "array")
                {
                    rootProperty.Value = "Array (" + (firstProperty.Attributes["numchildren"] != null ? firstProperty.Attributes["numchildren"].Value : "") + ")";
                } else {
                    rootProperty.Value = "Instance of " + firstProperty.Attributes["classname"].Value;
                 }

                if (firstProperty.Attributes["children"].Value == "0")
                {
                    rootProperty.isComplete = true;
                }
                else
                {
                    int numChildren = firstProperty.Attributes["numchildren"] != null ? Convert.ToInt32(firstProperty.Attributes["numchildren"].Value) : 0;
                    rootProperty.isComplete = numChildren > 0 && numChildren == firstProperty.ChildNodes.Count;

                    foreach (XmlNode node in firstProperty.ChildNodes)
                    {
                        if (node.Attributes["name"].Value == "CLASSNAME") continue; // this is to handle http://bugs.xdebug.org/bug_view_page.php?bug_id=00000518
                        rootProperty.ChildProperties.Add(Property.Parse(node));                        
                    }
                }
            }

                    
            return rootProperty;
        }
    }
}
