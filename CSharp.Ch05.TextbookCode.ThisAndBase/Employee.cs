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
    class Employee : Person
    {
        public string DepartmentName { get; set; }

        // Constructor with first name.
        public Employee(string firstName)
            : base(firstName)
        {
            Form1.Results += "    Employee(" + firstName + ")" +
                             Environment.NewLine;
        }

        // Constructor with first and last name.
        public Employee(string firstName, string lastName)
            : base(firstName, lastName)
        {
            Form1.Results += "    Employee(" + firstName + ", " +
                             lastName + ")" + Environment.NewLine;
        }

        // Constructor with first name, last name, and department name.
        public Employee(string firstName, string lastName, string departmentName)
            : this(firstName, lastName)
        {
            Form1.Results += "    Employee(" + firstName + ", " +
                             lastName + ", " + departmentName + ")" + Environment.NewLine;
            DepartmentName = departmentName;
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
