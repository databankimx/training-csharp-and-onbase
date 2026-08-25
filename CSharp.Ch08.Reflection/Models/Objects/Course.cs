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
using System.Linq;
using CSharp.Ch08.Reflection.Models.Attributes;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch08.Reflection.Models.Objects
{
    /// <summary>
    /// Defines a Course class for Students
    /// </summary>
    [CourseCatalog("Computer Science", 3)]
    public class Course
    {
        #region Properties
        /// <summary>
        /// Course name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Course raw (numeric) grade
        /// </summary>
        public double RawGrade { get; set; }

        /// <summary>
        /// Course letter grade
        /// </summary>
        public string LetterGrade =>
            gradeCriteria
                .Where(criterion => RawGrade / 100 >= criterion.Value)
                .Select(criterion => criterion.Key)
                .FirstOrDefault() ?? "I";

        #pragma warning disable S125 // Allow commented code for educational purposes
        // The above property uses LINQ to determine the letter grade based on the raw grade and the defined criteria.
        // It returns the first matching letter grade or "I" if no match is found. Here is a more traditional implementation
        // of the LetterGrade property using a foreach loop, which is commented out below:
        //public string LetterGrade
        //{
        //    get
        //    {
        //        foreach (var criterion in gradeCriteria.Where(criterion => RawGrade / 100 >= criterion.Value))
        //        {
        //            #pragma warning disable S1751 // A student will only have one letter grade, so we can return the first match
        //            return criterion.Key;
        //            #pragma warning restore S1751
        //        }

        //        return "I";
        //    }
        //}
        #endregion

        #region Private Members
        // Raw grade criteria to evaluate for letter grades
        private readonly Dictionary<string, double> gradeCriteria = new()
        {
            {"A", 0.9},
            {"B", 0.8},
            {"C", 0.7},
            {"D", 0.6},
            {"F", 0.0}
        };
        #endregion

        #region Public Methods
        /// <summary>
        /// Assigns a different minimum number for a letter grade
        /// </summary>
        /// <param name="letter"></param>
        /// <param name="criterion"></param>
        public void SetGradeCriterion(string letter, double criterion)
        {
            if (criterion < 0) throw new DatabankException("Cannot set grade criterion to a negative number!");

            if (!gradeCriteria.ContainsKey(letter.ToUpper()))
                throw new DatabankException($"Letter grade [{letter}] does not exist!");

            gradeCriteria[letter.ToUpper()] = criterion;
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
