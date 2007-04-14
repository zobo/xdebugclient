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

    struct DirectoryRewrite
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
        private List<DirectoryRewrite> RewriteDirectories;
    
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
            RewriteDirectories = new List<DirectoryRewrite>();
        }

        public bool DetermineRewritePath(string localFilename, string remoteFilename)
        {
            List<string> filenameElements;
            List<string> tmpFilenameElements;

            string separator = Convert.ToString(System.IO.Path.DirectorySeparatorChar);
            if (remoteFilename.IndexOf("/") != -1)
            {
                separator = "/";

                filenameElements = new List<string>(remoteFilename.Split('/'));
            }
            else
            {
                filenameElements = new List<string>(
                    remoteFilename.Split(System.IO.Path.DirectorySeparatorChar)
                );
            }

            tmpFilenameElements = new List<string>(localFilename.Split(System.IO.Path.DirectorySeparatorChar));

            filenameElements.Reverse();
            tmpFilenameElements.Reverse();

            /* Bail if the filename (first element in the List instances after reverse) do 
             * not match. */
            if (filenameElements[0] != tmpFilenameElements[0])
            {
                return false;
            }

            int i = 0;
            foreach (String element in tmpFilenameElements)
            {
                if (filenameElements[i] != element)
                {
                    break;
                }

                i++;
            }

            string rewriteRemote = "";
            string rewriteLocal = "";

            for (int j = i; j < filenameElements.Count; j++)
            {
                if (filenameElements[j] != "")
                    rewriteRemote = filenameElements[j] + separator + rewriteRemote;
            }

            if (separator == "/")
                rewriteRemote = separator + rewriteRemote;

            for (int j = i; j < tmpFilenameElements.Count; j++)
            {
                rewriteLocal += tmpFilenameElements[i] + System.IO.Path.DirectorySeparatorChar;
            }

            DirectoryRewrite newRewrite = new DirectoryRewrite();
            newRewrite.remotePath = rewriteRemote;
            newRewrite.localPath = rewriteLocal;
         
            RewriteDirectories.Add(newRewrite);

            return true;
        }

        public string getLocalFilename(string filename)
        {
            foreach (DirectoryRewrite rewrite in RewriteDirectories)
            {
                if (filename.IndexOf(rewrite.remotePath) != -1)
                {
                   string tmpFilename = filename.Replace(rewrite.remotePath, rewrite.localPath);
                   tmpFilename = tmpFilename.Replace(
                       System.IO.Path.AltDirectorySeparatorChar,
                       System.IO.Path.DirectorySeparatorChar
                   );
                   return tmpFilename;
                }
            }

            return filename;
        } 
       
        public string getRemoteFilename(string filename)
        {
            foreach (DirectoryRewrite rewrite in RewriteDirectories)
            {
                if (filename.IndexOf(rewrite.localPath) != -1)
                {
                    return filename.Replace(rewrite.localPath, rewrite.remotePath);
                }
            }

            return "";
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

        public void Add(string filename, SourceFileForm form)
        {
            OpenFileStruct file = new OpenFileStruct();

            file.localFilename = this.getLocalFilename(filename);
            file.remoteFilename = filename;
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
