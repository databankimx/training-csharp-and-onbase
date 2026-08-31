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
    partial class ProjectInstaller
    {
        #region Fields
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Controls which Windows account the service process runs as.
        /// </summary>
        private ServiceProcessInstaller serviceProcessInstaller1;

        /// <summary>
        /// Controls the installed service's name, display name, and start type.
        /// </summary>
        private ServiceInstaller serviceInstaller1;
        #endregion

        #region Parent Overrides
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if managed resources should be disposed; otherwise, <see langword="false"/>.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support. Do not modify the contents of this
        /// method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            serviceProcessInstaller1 = new ServiceProcessInstaller();
            serviceInstaller1 = new ServiceInstaller();

            // serviceProcessInstaller1
            // *Migration Note: LocalSystem is the simplest choice for a sample, a real
            //   production service would very likely run under a dedicated, least-
            //   privilege service account instead.
            serviceProcessInstaller1.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller1.Password = null;
            serviceProcessInstaller1.Username = null;

            // serviceInstaller1
            // *Migration Note: ServiceName here MUST match DataHealthCheckService's own
            //   ServiceName (see DataHealthCheckService.Designer.cs), the Service Control
            //   Manager uses this name to associate an installed service registration
            //   with the actual ServiceBase implementation that handles it.
            serviceInstaller1.ServiceName = "Samples.WindowsService";
            serviceInstaller1.DisplayName = "Samples Windows Service (Data Health Check)";
            serviceInstaller1.Description = "Periodically checks the ZipCodes table for rows with missing data and logs what it finds. DataBank IMX training sample.";
            serviceInstaller1.StartType = ServiceStartMode.Automatic;

            // ProjectInstaller
            Installers.AddRange(new System.Configuration.Install.Installer[] {
                serviceProcessInstaller1,
                serviceInstaller1});
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
