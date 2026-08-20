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

using System;

namespace CSharp.Ch05.TextbookCode.ThisAndBase
{
    class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // Constructor with first name.
        public Person(string firstName)
        {
            Form1.Results += "  Person(" + firstName + ")" +
                             Environment.NewLine;
            FirstName = firstName;
        }

        // Constructor with first and last name.
        public Person(string firstName, string lastName)
            : this(firstName)
        {
            Form1.Results += "  Person(" + firstName + ", " +
                             lastName + ")" + Environment.NewLine;
            LastName = lastName;
        }
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
