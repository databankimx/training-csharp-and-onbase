#region Copyright
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * All rights reserved                                                  *
 *                                                                      *
 * For further information consult:                                     *
 *  - The DataBank IMX End User License Agreement (EULA)                *
 *    or                                                                *
 *  - DataBank IMX Intellectual Property Statement                      *
 *                                                                      *
 * Above referenced documents available upon request from:              *
 *     development@databankimx.com                                      *
 *                                                                      *
 * ******************************************************************** */
#endregion

#region Using Directives
using System;
using System.Diagnostics;
#endregion

namespace CSharp.Ch11.Supplemental._03.TraceListeners
{
    /// <summary>
    /// A hand-written TraceListener, prefixing every line with a timestamp before handing
    /// it off to the console. TraceListener is the base class every built-in listener
    /// (ConsoleTraceListener, TextWriterTraceListener, EventLogTraceListener) derives from
    /// too, only two methods are strictly required to build your own: Write() and
    /// WriteLine().
    /// </summary>
    public class TimestampedTraceListener : TraceListener
    {
        #region Private Members
        // Tracks whether the next Write() call is starting a brand-new line, so the
        //   timestamp prefix is added once per line, not once per Write() call (Trace's
        //   own machinery sometimes calls Write() more than once before a final
        //   WriteLine() for the same logical line).
        private bool atLineStart = true;
        #endregion

        #region TraceListener Overrides
        /// <summary>
        /// Write text without a trailing newline
        /// </summary>
        public override void Write(string message)
        {
            if (atLineStart)
            {
                Console.Write($"[{DateTime.Now:HH:mm:ss.fff}] ");
                atLineStart = false;
            }
            Console.Write(message);
        }

        /// <summary>
        /// Write text WITH a trailing newline
        /// </summary>
        public override void WriteLine(string message)
        {
            Write(message);
            Console.WriteLine();
            atLineStart = true;
        }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
