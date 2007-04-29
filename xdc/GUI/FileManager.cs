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

using xdc.XDebug;
using xdc.Forms;
using System.Windows.Forms;

namespace xdc.GUI
{
    struct OpenFileStruct
    {
        public string localFilename;
        public string remoteFilename;
        public SourceFileForm form;
    }

    class DirectoryRewrite
    {
        public string remotePath;
        public string localPath;
    }

    /// <summary>
    /// Keeps a list of Forms and their remote plus local filenames.
    /// </summary>
    class FileManager
    {
        private Dictionary<string, OpenFileStruct> _openFiles;        
        
    
        public bool HasOpenFiles
        {
            get
            {
                return _openFiles.Count > 0;
            }
        }

        public Dictionary<string, OpenFileStruct> Forms
        {
            get
            {
                return _openFiles;
            }
        }

        public FileManager()
        {
            _openFiles = new Dictionary<string, OpenFileStruct>();
        }
         
        public SourceFileForm getFirstForm()
        {
            if (_openFiles.Keys.Count > 0)
            {
                System.Collections.Generic.Dictionary<string, OpenFileStruct>.Enumerator al = _openFiles.GetEnumerator();
                if (al.MoveNext())
                {
                    return al.Current.Value.form;
                }                               
            }

            return
                null;

        }

        public void Add(string actualFilename, string localFilename, SourceFileForm form)
        {
            OpenFileStruct file = new OpenFileStruct();

            file.localFilename = localFilename;
            file.remoteFilename = actualFilename;
            file.form = form;

            _openFiles.Add(file.localFilename, file);           
        }

        public void Remove(string filename)
        {
            if (_openFiles.ContainsKey(filename))
            {
                _openFiles.Remove(filename);
            }
        }

        public SourceFileForm getFormByLocalFilename(string filename)
        {
            if (_openFiles.ContainsKey(filename))
                return _openFiles[filename].form;

            return null;
        }

        public SourceFileForm getFormByRemoteFilename(string filename)
        {
            foreach (OpenFileStruct file in _openFiles.Values)
            {
                if (file.remoteFilename == filename)
                {
                    return file.form;
                }
            }

            return null;
        }


    }

}
