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

namespace CSharp.Ch10.WorkingWithLinq.Models
{
    /// <summary>
    /// A book, used throughout this chapter's LINQ examples
    /// </summary>
    public class Book
    {
        #region Properties
        /// <summary>
        /// Book title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Foreign key to the writing Author
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Year first published
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Genre
        /// </summary>
        public string Genre { get; set; }

        /// <summary>
        /// Price
        /// </summary>
        public decimal Price { get; set; }
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
