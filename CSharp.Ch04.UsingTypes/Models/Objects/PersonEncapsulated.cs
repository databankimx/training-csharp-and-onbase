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
    /// Defines a Person class using encapsulation
    /// </summary>
    public class PersonEncapsulated
    {
        #region LESSON NOTES
        /* This is not meant to be used in the inherited class hierarchy, but to illustrate encapsulation
         *
         * Encapsulation hides the variables being used, exposing only what the developer intends
         *
         * Here, we're using properties to encapsulate private members.
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
         */
        #endregion

        #region Private Members
        // Person first name
        private string firstName;

        // Person last name
        // Because I only included a "get" accessor, this is only modified by the constructor
        //    so it can be marked "readonly"
        // This is NOT a good real-world example
        private readonly string lastName;
        #endregion

        #region Properties
        /// <summary>
        /// Person first name (accessor for encapsulated firstName variable)
        /// </summary>
        public string FirstName
        {
            get
            {
                return firstName;
            }
            set
            {
                firstName = value;
            }
        }

        /// <summary>
        /// Person last name (accessor for encapsulated firstName variable)
        /// </summary>
        public string LastName
        {
            get
            {
                return lastName;
            }
            // NOTE: I have commented out the setter here to illustrate how this can limit the access
            //set
            //{
            //    lastName = value;
            //}
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Create and initialize a new instance of the Person class
        /// </summary>
        /// <param name="fName">Person first name</param>
        /// <param name="lName">Person last name</param>
        public PersonEncapsulated(string fName, string lName)
        {
            firstName = fName;
            lastName = lName;
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
