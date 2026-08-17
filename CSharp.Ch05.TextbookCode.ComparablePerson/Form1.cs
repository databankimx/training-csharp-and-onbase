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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharp.Ch05.TextbookCode.ComparablePerson
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Make and display a list of names.
        private void Form1_Load(object sender, EventArgs e)
        {
            // Make an array of Person.
            Person[] people =
            {
                new Person(){ FirstName = "Fred", LastName = "Franklin"},
                new Person(){ FirstName = "Cindy", LastName = "Carter"},
                new Person(){ FirstName = "Dan", LastName = "Dent"},
                new Person(){ FirstName = "Ben", LastName = "Baker"},
                new Person(){ FirstName = "Eva", LastName = "Eager"},
                new Person(){ FirstName = "Ann", LastName = "Able"},
            };

            // Display the unsorted list.
            foreach (Person person in people) unsortedListBox.Items.Add(person);

            // Sort the array.
            Array.Sort(people);

            // Display the sorted list.
            foreach (Person person in people) sortedListBox.Items.Add(person);
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
