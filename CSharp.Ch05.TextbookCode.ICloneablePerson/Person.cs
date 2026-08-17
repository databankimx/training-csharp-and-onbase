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

namespace CSharp.Ch05.TextbookCode.ICloneablePerson
{
    class Person : ICloneable
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Person Manager { get; set; }

        // Return a clone of this person.
        public object Clone()
        {
            Person person = new Person();
            person.FirstName = FirstName;
            person.LastName = LastName;
            person.Manager = Manager;
            // Uncomment the following for deep clones.
            //if (Manager != null)
            //    person.Manager = (Person)Manager.Clone();
            return person;
        }

        // Return a textual representation of the Person.
        public override string ToString()
        {
            string text = FirstName + " " + LastName;
            if (Manager != null)
                text += " (Manager: " + Manager.ToString() + ")";
            return text;
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
