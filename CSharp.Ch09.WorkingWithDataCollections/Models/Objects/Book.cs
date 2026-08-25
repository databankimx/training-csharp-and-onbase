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

namespace CSharp.Ch09.WorkingWithDataCollections.Models.Objects
{
    /// <summary>
    /// A simple book, used as a consistent, relatable dataset across this lesson's various
    /// collection demonstrations
    /// </summary>
    public class Book
    {
        #region Properties
        /// <summary>
        /// Book title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Author's name
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Year first published
        /// </summary>
        public int Year { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the Book class
        /// </summary>
        public Book() { }

        /// <summary>
        /// Create and initialize a new instance of the Book class
        /// </summary>
        public Book(string title, string author, int year)
        {
            Title = title;
            Author = author;
            Year = year;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Return a display-friendly representation of this book
        /// </summary>
        public override string ToString()
        {
            return $"{Title} by {Author} ({Year})";
        }
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
