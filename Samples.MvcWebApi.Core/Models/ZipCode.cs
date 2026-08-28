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
        public int Id { get; set; }
        public required string State { get; set; }
        public required string County { get; set; }
        public required string City { get; set; }
        public required string ZipCode1 { get; set; }
    }
}
