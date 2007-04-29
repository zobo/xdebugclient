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
using System.Windows;
using System.Windows.Forms;
using System.IO;

using xdc.XDebug;
using xdc.Forms;

namespace xdc.GUI.FileLoader
{
    class RewriteFileLoader : FileLoader
    {
        private List<DirectoryRewrite> RewriteDirectories;

        public RewriteFileLoader()
        {
            RewriteDirectories = new List<DirectoryRewrite>();
        }

        #region FileLoader implementation

        public override bool AllowsOpenFileDialog
        {
            get { return true;  }
        }

        public override bool OpenFile(xdc.Forms.SourceFileForm targetForm, string filename)
        {
            try
            {
                targetForm.LoadFile(filename);
            } catch (Exception e) 
            {
                MessageBox.Show("Unable to open file: " + filename + "(" + e.Message + ")" );
                return false;
            }
           
            return true;
        }
        #endregion

        public override bool DetermineLocalFilename(string filename, ref string localFilename)
        {
            /* First try to find any existing rewrite rules. */

            foreach (DirectoryRewrite r in RewriteDirectories)
            {
                if (filename.IndexOf(r.remotePath) != -1)
                {                    
                    localFilename = filename.Replace(r.remotePath, r.localPath);

                    return true;
                }
            }

            DirectoryRewrite rule = this.DetermineRewriteRule(filename);

            if (rule != null)
            {
                RewriteDirectories.Add(rule);

                foreach (DirectoryRewrite r in RewriteDirectories)
                {
                    if (filename.IndexOf(r.remotePath) != -1)
                    {

                        localFilename = filename.Replace(r.remotePath, r.localPath);
                        
                        return true;
                    }
                }
            }
            
            return false;
            
        }

        public DirectoryRewrite DetermineRewriteRule(string filename)
        {
            DirectoryRewrite result = null;

            string sourceFilename = Path.GetFileName(filename);
            bool done = false;

            while (!done)
            {               
                OpenFileDialog fileDialog = new OpenFileDialog();

                /* This should have a way to also detect filename[1].php, filename[2].php for
                 * the FTP service. */
                fileDialog.Filter = "PHP file|" + sourceFilename;
               
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    result = this.GenerateRewritePath(fileDialog.FileName, filename);

                    if (result != null)
                    {                       
                        done = true;
                        break;
                    }
                }
                else
                {
                    return null;                    
                }
            }

            return (result != null) ? result : null;            
        }

        public DirectoryRewrite GenerateRewritePath(string localFilename, string remoteFilename)
        {
            List<string> filenameElements;
            List<string> tmpFilenameElements;

            bool isUNC = localFilename.IndexOf("\\\\") == 0;
            string remoteFile = System.IO.Path.GetFileName(remoteFilename);
            string localFile = System.IO.Path.GetFileName(localFilename);

            string separator = Convert.ToString(System.IO.Path.DirectorySeparatorChar);
            if (remoteFilename.IndexOf("/") != -1)
            {
                separator = "/";

                filenameElements = new List<string>(remoteFilename.Split('/'));
            }
            else
            {
                filenameElements = new List<string>(
                    remoteFilename.Split(
                        new char[] { System.IO.Path.DirectorySeparatorChar }, 
                        StringSplitOptions.RemoveEmptyEntries
                    )
                );
            }

            tmpFilenameElements = new List<string>(
                localFilename.Split(
                    new char[] { System.IO.Path.DirectorySeparatorChar },
                    StringSplitOptions.RemoveEmptyEntries
                )
            );

            filenameElements.Reverse();
            tmpFilenameElements.Reverse();

            /* Bail if the filename (first element in the List instances after reverse) do 
             * not match. This currently breaks FTP access: blaa.php != blaa[1].php
            if (filenameElements[0] != tmpFilenameElements[0])
            {
                return null;
            } */

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
                {
                    if (remoteFile == filenameElements[j])
                        rewriteRemote = filenameElements[j];
                    else 
                        rewriteRemote = filenameElements[j] + separator + rewriteRemote;
                }

            }

            if (separator == "/")
                rewriteRemote = separator + rewriteRemote;

            for (int j = i; j < tmpFilenameElements.Count; j++)
            {
                if (localFile == tmpFilenameElements[j])
                    rewriteLocal = tmpFilenameElements[j];
                else 
                    rewriteLocal = tmpFilenameElements[j] + System.IO.Path.DirectorySeparatorChar + rewriteLocal;
            }

            if (isUNC)
                rewriteLocal = "\\\\" + rewriteLocal;
        
            DirectoryRewrite newRewrite = new DirectoryRewrite();
            newRewrite.remotePath = rewriteRemote;
            newRewrite.localPath = rewriteLocal;

            return newRewrite;
        }
    }
}

