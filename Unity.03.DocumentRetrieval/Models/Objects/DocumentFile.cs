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

namespace Unity._03.DocumentRetrieval.Models.Objects
{
    #region Training Notes
    /*
     * *Migration Note: the original version of this class had a comment noting a
     * base64-string content option "would" be added in a real-world example, rather than
     * actually being added. Base64Content below is that option, implemented as a
     * COMPUTED property derived from Content, not a separately-settable field. Deriving
     * it keeps the two representations from ever disagreeing with each other, there's
     * only ever one real source of truth (Content), Base64Content is just a different
     * encoding of the same bytes, computed on read.
     */
    #endregion

    /// <summary>
    /// Defines a document's file content for integration retrieval
    /// </summary>
    public class DocumentFile
    {
        #region Properties
        /// <summary>
        /// File contents as byte array
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        /// File contents as a base64-encoded string, computed from <see cref="Content"/>.
        /// <see langword="null"/> when <see cref="Content"/> is <see langword="null"/>.
        /// </summary>
        public string Base64Content => Content == null ? null : Convert.ToBase64String(Content);
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
