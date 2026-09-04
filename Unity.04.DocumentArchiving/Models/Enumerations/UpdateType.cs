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

namespace Unity._04.DocumentArchiving.Models.Enumerations
{
    /// <summary>
    /// The kind of update an <see cref="Objects.UpdateDocumentRequest"/> performs.
    /// </summary>
    public enum UpdateType
    {
        /// <summary>
        /// Not yet set.
        /// </summary>
        Undefined = -1,

        /// <summary>
        /// Update keywords, document type, and/or document date.
        /// </summary>
        Metadata = 0,

        /// <summary>
        /// Store a new revision.
        /// </summary>
        Revision = 1,

        /// <summary>
        /// Store a new rendition on the current revision.
        /// </summary>
        Rendition = 2
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
