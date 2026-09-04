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
    /// The kind of document a <see cref="Objects.NewDocumentRequest"/> creates.
    /// </summary>
    public enum StorageType
    {
        /// <summary>
        /// Not yet set.
        /// </summary>
        Undefined = -1,

        /// <summary>
        /// A conventional document, created from one or more files.
        /// </summary>
        Document = 0,

        /// <summary>
        /// An OnBase electronic form (e-form).
        /// </summary>
        EForm = 1,

        /// <summary>
        /// A Unity Form.
        /// </summary>
        UnityForm = 2,

        /// <summary>
        /// An unindexed document. Not currently handled by <see cref="HelperClasses.OnBase.DocumentStorage"/>.
        /// </summary>
        UnindexedDocument = 3
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
