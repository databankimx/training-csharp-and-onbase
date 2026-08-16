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
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch05.ImplementingClassHierarchies.Models.Objects
{
    /// <summary>
    /// Defines a Course class for Students
    /// </summary>
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
        public string LetterGrade
        {
            get
            {
                foreach (var criterion in gradeCriteria.Where(criterion => RawGrade / 100 >= criterion.Value))
                {
                    return criterion.Key;
                }

                return "I";
            }
        }
        #endregion

        #region Private Members
        // Raw grade criteria to evaluate for letter grades
        #pragma warning disable IDE0090 // In lesson, not simplifying (to `new()`) to avoid confusion for students
        private readonly Dictionary<string, double> gradeCriteria = new Dictionary<string, double>
        #pragma warning restore IDE0090
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
