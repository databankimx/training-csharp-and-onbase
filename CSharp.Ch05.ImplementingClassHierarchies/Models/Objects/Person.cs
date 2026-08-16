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
    /// Defines a Person (Base Class)
    /// </summary>
    #pragma warning disable S4035 // In the real world, IEqualityComparer<T> would be implemented as well, but for this lesson, we will not implement it
    public class Person : IEquatable<Person>, ICloneable
    #pragma warning restore S4035
    {
        #region Properties
        // Note the use of auto-properties instead of encapsulation
        /// <summary>
        /// Person's First Name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Person's Last Name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Person's Manager
        /// </summary>
        public Person Manager { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the Person class
        /// </summary>
        public Person() { }

        /// <summary>
        /// Create and partially initialize a new instance of the Person class
        /// </summary>
        /// <param name="firstName">Person's First Name</param>
        public Person(string firstName)
        {
            // Validate the first name
            if (string.IsNullOrEmpty(firstName))
                throw new ArgumentOutOfRangeException(nameof(firstName), firstName, @"FirstName must not be null or blank!");

            // Store the first name
            FirstName = firstName;
        }

        /*
         * Using the "this" keyword, you can inherit a constructor from within the same class in this form
         *      public ClassName(argumentsInExistingConstructor, additionalArguments) : this(argumentsInExistingConstructor)
         *
         * The signature of the additional constructor must be different from the existing one
         * The invoked constructor called by "this" executes before entering the code block of the new constructor
         */

        /// <summary>
        /// Create and initialize a new instance of the Person class
        /// </summary>
        /// <param name="firstName">Person's First Name</param>
        /// <param name="lastName">Person's Last Name</param>
        public Person(string firstName, string lastName) : this(firstName)
        {
            // Validate the last name
            if (string.IsNullOrEmpty(lastName))
                throw new ArgumentOutOfRangeException(nameof(lastName), lastName, @"LastName must not be null or blank!");

            // Store the last name
            LastName = lastName;
        }
        #endregion

        /* Best Practices Note:
         * Because List<T>, Dictionary<T,T>, Stack<T>, and Queue<T> expose methods that compare equality, it is a best practice
         * to implement IEquatable on any classes that will be placed in Generic Collections
         */

        #region IEquatable
        /// <summary>
        /// Validates two Person objects equivalent
        /// </summary>
        /// <param name="other">Other person object to compare</param>
        /// <returns>True if this and other match</returns>
        public bool Equals(Person other)
        {
            // First, we evaluate if the compared item is null, which we treat as not equal
            if (other is null) return false;

            // Next, we compare to see if these are the same item in memory (reference), which would certainly be equal
            if (ReferenceEquals(this, other)) return true;

            // Finally, we'll compare based on attribute value(s)
            return string.Equals(FirstName, other.FirstName, StringComparison.CurrentCultureIgnoreCase) &&
                   string.Equals(LastName, other.LastName, StringComparison.CurrentCultureIgnoreCase);
        }
        #endregion

        #region ICloneable
        /// <summary>
        /// Produces a clone of the current object (necessary with reference types to re-instantiate)
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            // This is a shallow clone (objects are recreated)
            #pragma warning disable S125 // Commented code is part of the lesson and is not to be removed
            //return new Person
            //{
            //    FirstName = FirstName,
            //    LastName = LastName,
            //    Manager = Manager
            //};
            #pragma warning restore S125

            // A simpler expression of a shallow clone simply auto-creates a duplicate of the member values
            //return MemberwiseClone();

            // A deep clone must create a new instance of any objects in the class instance
            return new Person
            {
                FirstName = FirstName,
                LastName = LastName,
                Manager = (Person)Manager?.Clone()
            };
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get the person's full name
        /// </summary>
        /// <returns>Full name</returns>
        public string FullName(bool lastFirst = false)
        {
            return lastFirst
                ? $"{LastName}, {FirstName}"
                : $"{FirstName} {LastName}";
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
