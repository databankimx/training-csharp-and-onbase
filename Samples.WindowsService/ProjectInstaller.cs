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
using System.ComponentModel;
using System.Configuration.Install;
#endregion

namespace Samples.WindowsService
{
    #region Training Notes
    /*
     * InstallUtil.exe (installutil.exe, shipped with the .NET Framework, not .NET Core/
     * modern .NET) is the classic way to install a ServiceBase-derived Windows Service:
     *
     *   installutil.exe Samples.WindowsService.exe
     *   installutil.exe /u Samples.WindowsService.exe   (uninstall)
     *
     * It works by finding a class in the target assembly decorated with
     * [RunInstaller(true)] (this one) that derives from System.Configuration.Install.
     * Installer, and running its Install()/Uninstall() logic, which in turn runs
     * whatever Installer-derived components are attached to it (see
     * ProjectInstaller.Designer.cs: a ServiceProcessInstaller, controlling the account
     * the service runs as, and a ServiceInstaller, controlling the service's name,
     * display name, and start type).
     *
     * Samples.WindowsService.NetCore has NO equivalent to this file at all,
     * AddWindowsService() plus a plain "sc create" command is the modern replacement,
     * no installer class, no InstallUtil.exe, no [RunInstaller] attribute needed
     * anywhere. See that project's own LectureNotes.md and this project's README.md for
     * both installation paths written out.
     */
    #endregion

    /// <summary>
    /// The installer for <see cref="DataHealthCheckService"/>, run by InstallUtil.exe.
    /// </summary>
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectInstaller"/> class.
        /// </summary>
        public ProjectInstaller()
        {
            InitializeComponent();
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
