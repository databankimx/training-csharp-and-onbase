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
using System.Collections.Generic;
#endregion

namespace Unity._03.DocumentRetrieval.Models.Objects
{
    #region Training Notes
    /*
     * *Migration Note: added when auditing Unity.TestHarness for logic that belongs in
     * the library rather than the UI layer. DocumentDetailViewModel/ArchivingViewModel
     * both held live Hyland.Unity Revision objects directly as view model state, and
     * bound to their raw properties from XAML, which only works because a WPF app keeps
     * one connected Unity session open for its whole lifetime. A serializable DTO
     * (mirroring DocumentInfo's own existing pattern) is what a future web portal
     * actually needs instead, there's no live Unity connection held between page loads
     * for a given web user the way there is in this desktop app.
     */
    #endregion

    /// <summary>
    /// Defines revision metadata for an integration application, a serializable
    /// alternative to holding a live <see cref="Hyland.Unity.Revision"/> reference.
    /// </summary>
    public class RevisionInfo
    {
        #region Properties
        /// <summary>
        /// Revision ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Revision date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Revision comment
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Revision creator
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// This revision's renditions
        /// </summary>
        public List<RenditionInfo> Renditions { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the RevisionInfo class
        /// </summary>
        public RevisionInfo()
        {
            Renditions = [];
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
