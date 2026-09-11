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

namespace RestApi._04.DocumentArchiving.Models.Enumerations
{
    #region Training Notes
    /*
     * *Migration Note: no StorageType here, unlike Unity.04's own (Document/EForm/
     * UnityForm/Undefined): this project is scoped to conventional documents only, see
     * this project's own .csproj Training Notes and LectureNotes.md for why. UpdateType
     * is kept as-is, all three members (Metadata/Revision/Rendition) are fully
     * implemented for conventional documents.
     */
    #endregion

    /// <summary>Which kind of update <see cref="Objects.UpdateDocumentRequest"/> represents.</summary>
    public enum UpdateType
    {
        /// <summary>Update keywords, document type, and/or document date.</summary>
        Metadata,

        /// <summary>Store a new revision.</summary>
        Revision,

        /// <summary>Store a new rendition on the latest revision.</summary>
        Rendition
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
