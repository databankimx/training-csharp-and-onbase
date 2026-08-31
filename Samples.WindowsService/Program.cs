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
using System.ServiceProcess;
#endregion

namespace Samples.WindowsService
{
    /// <summary>
    /// Contains the application's entry point.
    /// </summary>
    internal static class Program
    {
        #region Methods
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <remarks>
        /// *Migration Note: ServiceBase.Run() is what makes this executable behave as a
        /// Windows Service, unlike Samples.WindowsService.NetCore's UseWindowsService(),
        /// this does NOT auto-detect an interactive/console context, running this .exe
        /// directly (F5, dotnet run) throws an InvalidOperationException, "Cannot start
        /// service ... because the process is not running as a Windows service" the
        /// moment ServiceBase.Run() is reached. A classic Windows Service genuinely can
        /// only be started through the Service Control Manager (sc start, or the Services
        /// MMC snap-in) after being installed, see LectureNotes.md for how, and for how
        /// this differs from the .NetCore sibling's dual console/service behavior.
        /// </remarks>
        private static void Main()
        {
            ServiceBase.Run(new DataHealthCheckService());
        }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
