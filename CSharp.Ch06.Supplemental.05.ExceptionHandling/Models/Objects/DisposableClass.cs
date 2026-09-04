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
#endregion

namespace CSharp.Ch06.Supplemental.s05.ExceptionHandling.Models.Objects
{
    /// <summary>
    /// Example of implementing IDisposable
    /// </summary>
    public class DisposableClass : IDisposable
    {
        #region Properties
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; } = "";
        #endregion

        #region Private Members
        // Check if Dispose has already been called
        private bool resourcesAreFreed;
        #endregion

        #region IDisposable
        /// <summary>
        /// Safely clean up managed and unmanaged resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Safely clean up managed and unmanaged resources
        /// </summary>
        /// <param name="disposing">Indicates whether the method is called from Dispose (true) or from the finalizer (false)</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                FreeResources(true);
            }
        }

        /// <summary>
        /// Finalizer to clean up unmanaged resources only
        /// </summary>
        ~DisposableClass()
        {
            Dispose(false);
        }
        #endregion

        #region Helper Functions
        // Free resources when called by Dispose or finalizer
        private void FreeResources(bool freeManagedResources)
        {
            if (resourcesAreFreed) return;

            Console.WriteLine($"{Name}: FreeResources");
            resourcesAreFreed = true;

            Console.WriteLine($"{Name}: Dispose of unmanaged resources");
            // If there were unmanaged resources, we'd free them here

            if (!freeManagedResources) return;

            Console.WriteLine($"{Name}: Dispose of managed resources");
            // If there were managed resources, we'd free them here
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
