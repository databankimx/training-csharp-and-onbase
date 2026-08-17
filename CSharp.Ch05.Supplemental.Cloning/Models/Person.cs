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

namespace CSharp.Ch05.Supplemental.Cloning.Models
{
    /// <summary>
    /// Demonstrates assignment, shallow cloning, and deep cloning.
    /// </summary>
    internal sealed class Person
    {
        #region Properties
        /// <summary>
        /// Gets or sets the name of the person.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the home address.
        /// </summary>
        public Address HomeAddress { get; set; }

        /// <summary>
        /// Gets or sets the collection of skills.
        /// </summary>
        #pragma warning disable IDE0028 // Not simplifying collection initialization in lessons
        public List<string> Skills { get; set; } = new List<string>();
        #pragma warning restore IDE0028
        #endregion

        #region Public Methods
        /// <summary>
        /// MemberwiseClone creates a new Person object, but reference-type
        /// fields still point to the same child objects as the source.
        /// </summary>
        public Person ShallowClone()
        {
            return (Person)MemberwiseClone();
        }

        /// <summary>
        /// Creates a completely independent copy of the Person and the
        /// mutable reference-type objects owned by it.
        /// </summary>
        public Person DeepClone()
        {
            #pragma warning disable IDE0028 // Not simplifying collection initialization in lessons
            #pragma warning disable IDE0306 // Not simplifying collection initialization in lessons
            return new Person
            {
                Name = Name,
                Age = Age,
                HomeAddress = HomeAddress?.DeepClone(),
                Skills = Skills == null
                    ? null
                    : new List<string>(Skills)
            };
            #pragma warning restore IDE0306
            #pragma warning restore IDE0028
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
