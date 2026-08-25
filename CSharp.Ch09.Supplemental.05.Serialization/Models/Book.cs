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
using System.Runtime.Serialization;
#endregion

namespace CSharp.Ch09.Supplemental._05.Serialization.Models
{
    /// <summary>
    /// A book, serializable three different ways (binary, XML, JSON), and implementing
    /// ISerializable to demonstrate hand-controlled binary serialization specifically: the
    /// Summary field is deliberately excluded from what gets written out, and recomputed
    /// instead the first time it's read after deserialization, rather than persisted and
    /// potentially going stale.
    /// </summary>
    [Serializable]
    public class Book : ISerializable
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

        #region Private Members
        // Deliberately NOT written out by GetObjectData() below, recomputed on first access
        // instead, see the ISerializable region for why.
        [NonSerialized]
        private string cachedSummary;
        #endregion

        #region Public Properties (Computed)
        /// <summary>
        /// A display-friendly summary, computed on first access and cached, never itself
        /// persisted through binary serialization
        /// </summary>
        public string Summary => cachedSummary ??= $"{Title} by {Author} ({Year})";
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

        #region ISerializable
        // The special constructor BinaryFormatter (and any other ISerializable-aware
        //   formatter) calls during deserialization, reading exactly the values
        //   GetObjectData() below chose to write out, in this case, Title/Author/Year only.
        protected Book(SerializationInfo info, StreamingContext context)
        {
            Title = info.GetString(nameof(Title));
            Author = info.GetString(nameof(Author));
            Year = info.GetInt32(nameof(Year));
            // cachedSummary is intentionally left unset here, Summary recomputes it lazily
            //   the next time it's actually read, rather than trusting a persisted value
            //   that could have gone stale if Title/Author/Year were ever meant to change.
        }

        /// <summary>
        /// Called by BinaryFormatter during serialization: chooses exactly which values get
        /// written out, giving fine control beyond what [Serializable]/[NonSerialized] alone
        /// provide.
        /// </summary>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Title), Title);
            info.AddValue(nameof(Author), Author);
            info.AddValue(nameof(Year), Year);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Return a display-friendly representation of this book
        /// </summary>
        public override string ToString()
        {
            return Summary;
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
