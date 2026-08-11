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
#endregion

namespace LessonRunner.Models
{
    /// <summary>
    /// A chapter: a title for the menu, and its lessons in logical (not alphabetical)
    /// teaching order.
    /// </summary>
    public class Chapter
    {
        #region Properties
        /// <summary>
        /// Text shown in the chapter menu
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Lessons belonging to this chapter, in the order they should be taught
        /// </summary>
        public List<Lesson> Lessons { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the Chapter class
        /// </summary>
        /// <param name="title">Text shown in the chapter menu</param>
        /// <param name="lessons">Lessons belonging to this chapter, in teaching order</param>
        public Chapter(string title, List<Lesson> lessons)
        {
            Title = title;
            Lessons = lessons;
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
