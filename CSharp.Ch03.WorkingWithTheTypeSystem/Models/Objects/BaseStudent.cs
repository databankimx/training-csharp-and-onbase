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

namespace CSharp.Ch03.WorkingWithTheTypeSystem.Models.Objects
{
    /* ABSTRACT CLASSES
     * An abstract class cannot be instantiated. It can only be used for inheritance
     * Methods declared in an abstract class must be implemented in derived classes (similar to implementing an interface)
     */

    /// <summary>
    /// Defines an abstract base class for derived student classes
    /// </summary>
    public abstract class BaseStudent
    {
        #region Method Definitions
        /// <summary>
        /// Display the details for the student object
        /// </summary>
        public abstract void OutputDetails();
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
