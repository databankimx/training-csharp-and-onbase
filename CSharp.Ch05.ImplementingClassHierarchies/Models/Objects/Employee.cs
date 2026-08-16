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
#endregion

namespace CSharp.Ch05.ImplementingClassHierarchies.Models.Objects
{
    /// <summary>
    /// Defines an Employee as a derived specific class of the Person superclass
    /// </summary>
    public class Employee : Person
    {
        /* DERIVED CLASS NOTES
         * A "derived" (or child) (or sub-) class inherits all of the code from the "base" (or parent) (or super-) class
         *
         * The book describes class hierarchies as a "family tree" where
         *  - Classes that inherit from a given class are its "descendants"
         *  - Classes from which a given class inherits are its "ancestors"
         *  - Classes with the same parent class are "siblings"
         *
         * Class inheritance is shown in this form:
         *      class ChildClass : ParentClass
         *
         * NOTE: A class can only inherit from a single base class.
         *       We'll see later, however, that a class can implement multiple interfaces.
         *
         * When implementing constructors, the parent class constructor can be inherited, provided
         *     all of the arguments required for it are passed into the child constructor.
         *     The syntax is in this form
         *      public ChildClass(parentClassArguments, childClassArguments) : base(parentClassArguments)
         */

        #region Properties
        /// <summary>
        /// Employee's Department
        /// </summary>
        public string Department { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create an instance of the Employee Class
        /// </summary>
        public Employee() { }

        /*
         * Using the "base" keyword, we can call a constructor from the base (parent) class
         *
         * The base constructor must have a signature matching the set of arguments passed
         * The base constructor executes before the code in the new constructor
         */

        /// <summary>
        /// Create and partially initialize a new instance of the Employee class
        /// </summary>
        /// <param name="firstName">Employee's First Name</param>
        public Employee(string firstName) : base(firstName)
        {
        }

        /// <summary>
        /// Create and partially initialize a new instance of the Employee class
        /// </summary>
        /// <param name="firstName">Employee's First Name</param>
        /// <param name="lastName">Employee's Last Name</param>
        public Employee(string firstName, string lastName) : base(firstName, lastName)
        {
        }

        /// <summary>
        /// Create and initialize a new instance of the Employee class
        /// </summary>
        /// <param name="firstName">Employee's First Name</param>
        /// <param name="lastName">Employee's Last Name</param>
        /// <param name="department">Employee's Department</param>
        public Employee(string firstName, string lastName, string department) : base(firstName, lastName)
        {
            // Validate the department
            if (string.IsNullOrEmpty(department))
                throw new ArgumentOutOfRangeException(nameof(department), department, "Department must not be null or blank!");

            // Store the Department
            Department = department;
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
