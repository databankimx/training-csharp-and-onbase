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

namespace CSharp.Ch04.UsingTypes.Models.Objects
{
    /// <summary>
    /// Defines a Person base class
    /// </summary>
    public class Person
    {
        #region Properties
        /* LESSON NOTES:
         * Here, we're using one of the most common implementations of properties.
         * Instead of using properties to encapsulate private members, we are using auto-properties.
         *
         * Consider the following syntax examples:
         *
         * // This is a public member variable
         * public string MyVar;
         *
         * // This is a private member variable
         * private string myVar;
         *
         * // Here is a property method encapsulating the private variable
         * public string MyVar
         * {
         *     get
         *     {
         *         return myVar;
         *     }
         *     set
         *     {
         *         myVar = value;
         *     }
         * }
         *
         * // This property is using expression bodies (we'll discuss  lambda expressions in a later chapter)
         * public string MyVar2
         * {
         *     get => myVar;
         *     set => myVar = value;
         * }
         *
         * // This is an expression-bodied property 
         * public string MyVar3 => myVar;
         *
         * // This is an auto-property (does not use a separate private variable)
         * public string MyVar { get; set; }
         */

        /// <summary>
        /// Person first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Person last name
        /// </summary>
        public string LastName { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the Person class
        /// </summary>
        public Person() { }

        /// <summary>
        /// Create and initialize a new instance of the Person class
        /// </summary>
        /// <param name="firstName">Person first name</param>
        /// <param name="lastName">Person last name</param>
        public Person(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
        #endregion

        #region Public Methods
        /*
         * The ToString() method (inherited from the System.Object data type), would return the class name
         *     in this case, CSharp.Ch04.UsingTypes.Models.Objects.Person
         *
         * That's not useful, so we can override that method to return a more meaningful string
         */

        // Here, we're overriding the default ToString method
        public override string ToString()
        { 
            return $"{FirstName} {LastName}";
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
