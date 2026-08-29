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

namespace Samples.MvcWebApi.Core.Models
{
    /// <summary>
    /// EF Core entity, Code-First. Unlike Samples.MvcWebApi's EF6 Database-First model (an
    /// existing table reverse-engineered into an .edmx), this class IS the source of truth,
    /// EF Core generates the schema from it via a migration, see LectureNotes.md.
    /// </summary>
    public class ZipCode
    {
        #region Properties
        /// <summary>
        /// ID column, primary key, auto-incremented by the database. EF Core infers this from the property name "Id" and its type "int", no [Key] or [DatabaseGenerated] attributes needed.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// State column, required. EF Core infers this from the property type "string" and the fact that it's not nullable, no [Required] attribute needed.
        /// </summary>
        public required string State { get; set; }

        /// <summary>
        /// County column, required. EF Core infers this from the property type "string" and the fact that it's not nullable, no [Required] attribute needed.
        /// </summary>
        public required string County { get; set; }

        /// <summary>
        /// City column, required. EF Core infers this from the property type "string" and the fact that it's not nullable, no [Required] attribute needed.
        /// </summary>
        public required string City { get; set; }

        /// <summary>
        /// ZipCode column, required. EF Core infers this from the property type "string" and the fact that it's not nullable, no [Required] attribute needed.
        /// </summary>
        public required string ZipCode1 { get; set; }
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
