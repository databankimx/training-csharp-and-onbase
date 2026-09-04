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
using System.Collections.Generic;
using Unity._04.DocumentArchiving.Models.Enumerations;
#endregion

namespace Unity._04.DocumentArchiving.Models.Objects
{
    /// <summary>
    /// Parameters for storing a new document, e-form, or Unity Form.
    /// </summary>
    /// <remarks>
    /// Create a new instance of the NewDocumentRequest class
    /// </remarks>
    /// <param name="storageType">The kind of document to create.</param>
    public class NewDocumentRequest(StorageType storageType = StorageType.Document) : StorageRequest
    {
        #region Properties
        /// <summary>
        /// File(s) to store. Required when <see cref="StorageType"/> is
        /// <see cref="Enumerations.StorageType.Document"/>; ignored for e-forms and Unity Forms.
        /// </summary>
        public List<string> Files { get; set; } = [];

        /// <summary>
        /// The kind of document to create.
        /// </summary>
        public StorageType StorageType { get; set; } = storageType;
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
