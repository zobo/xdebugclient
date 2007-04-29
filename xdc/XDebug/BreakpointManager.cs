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

namespace xdc.XDebug
{
    enum BreakpointState { Added=0, Removed }

    /// <summary>
    /// The BreakpointCollector is responsible to get the Delta of added / removed
    /// breakpoints between runstates. 
    /// </summary>
    class BreakpointManager
    {

        private List<xdc.XDebug.Breakpoint> _Breakpoints;
        public List<xdc.XDebug.Breakpoint> Breakpoints
        {
            get {
                return _Breakpoints;
            }
        }

        private List<xdc.XDebug.Breakpoint> _Added;
        public List<xdc.XDebug.Breakpoint> AddedBreakpoints { get { return _Added; } } 
        
        private List<xdc.XDebug.Breakpoint> _Removed;
        public List<xdc.XDebug.Breakpoint> RemovedBreakpoints { get { return _Removed; } } 

        public BreakpointManager()
        {            
            _Breakpoints = new List<Breakpoint>();
            _Added = new List<Breakpoint>();
            _Removed = new List<Breakpoint>();
        }

        /// <summary>
        /// Record the given breakpoint under the given state. If the breakpoint already
        /// exists in our list simply remove it. It's state during this runstate is unchanged.
        /// </summary>        
        public void Record(Breakpoint brk, BreakpointState brkState)
        {
            if (brkState == BreakpointState.Added)
            {
                // User removed it, but changed his mind before hitting Run.
                if (_Removed.Contains(brk))
                {
                    _Removed.Remove(brk);
                }
                else
                {
                    _Added.Add(brk);
                }

                _Breakpoints.Add(brk);
                
            }
            else
            {
                if (_Added.Contains(brk))
                {
                    _Added.Remove(brk);
                }
                else
                {
                    _Removed.Add(brk);
                }

                _Breakpoints.Remove(brk);
            }
        }

              
        public void FlushDelta()
        {
            _Added.Clear();
            _Removed.Clear();
        }
    }
}
