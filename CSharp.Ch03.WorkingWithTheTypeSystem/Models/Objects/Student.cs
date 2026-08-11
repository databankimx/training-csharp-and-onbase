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

namespace CSharp.Ch03.WorkingWithTheTypeSystem.Models.Objects
{
    /// <summary>
    /// Defines a standard class for a student object
    /// </summary>
    public class Student
    {
        #region Public Members
        /// <summary>
        /// Student number
        /// </summary>
        public static int StudentCount;

        /// <summary>
        /// Student first name
        /// </summary>
        public string FirstName;

        /// <summary>
        /// Student last name
        /// </summary>
        public string LastName;

        /// <summary>
        /// Student grade
        /// </summary>
        public string Grade;

        /// <summary>
        /// School name (added in the Overloading Constructors lab)
        /// </summary>
        public string SchoolName;
        #endregion

        #region Lesson Notes
        /*
         * CODE NOTE: In the first lab for this class, only the fields above are included.
         *            In the second lab using this class, the methods below are added.
         *            The constructors are modified the book code and are added as an example.
         *            In the overloading lab, Grade becomes an int. For consistency I've left it a string.
         */

        /*
         * Overloading Methods:
         * 
         * A method "signature" is the name of the method followed by the arguments it receives.
         *     The signature includes the arguments' data types and order.
         * 
         * The same method name (in this example, the constructor) can be overloaded with multiple
         *     implementations provided each one has a different signature.
         *     e.g. (This would be valid):
         *       public void MyFunction() { //instructions; }
         *       public void MyFunction(string myArgument) { //instructions; }
         * 
         * You cannot have multiple instances of the same signature for the same method. Even if the
         *     argument name(s) are different, it is the data type(s) that matter.
         *     e.g. (This would be invalid):
         *       public void MyFunction(string myArgument) { //instructions; }
         *       public void MyFunction(string myParameter) { //instructions; }
         * 
         * Note: Overloading is not related to overriding, where the new method replaces the existing
         *       one. Overloading results in a method that can be called multiple different ways.
         */

        /*
         * We're overloading the constructor with four signatures:
         * Notice how each one has either a different number of arguments or at least one argument
         *     with a different data type.
         *       public Student() {}
         *       public Student(int, string) {}
         *       public Student(int, string, string, string) {}
         *       public Student(string, string, string, string) {}
         */
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the Student class
        /// </summary>
        public Student() { }

        /// <summary>
        /// Create and initialize a new instance of the Student class
        /// </summary>
        /// <param name="studentCount">Student number</param>
        /// <param name="firstName">Student first name</param>
        public Student(int studentCount, string firstName)
        {
            StudentCount = studentCount;
            FirstName = firstName;
        }

        /// <summary>
        /// Create and initialize a new instance of the Student class
        /// </summary>
        /// <param name="studentCount">Student number</param>
        /// <param name="firstName">Student first name</param>
        /// <param name="lastName">Student last name</param>
        /// <param name="grade">Student grade</param>
        public Student(int studentCount, string firstName, string lastName, string grade)
        {
            StudentCount = studentCount;
            FirstName = firstName;
            LastName = lastName;
            Grade = grade;
        }

        /// <summary>
        /// Create and initialize a new instance of the Student class
        /// </summary>
        /// <param name="firstName">Student first name</param>
        /// <param name="lastName">Student last name</param>
        /// <param name="grade">Student grade</param>
        /// <param name="school">School name</param>
        public Student(string firstName, string lastName, string grade, string school)
        {
            FirstName = firstName;
            LastName = lastName;
            Grade = grade;
            SchoolName = school;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get the student's full name
        /// </summary>
        /// <returns>Full name</returns>
        public string ConcatenateName()
        {
            string fullName = FirstName + " " + LastName;
            return fullName;
        }

        /// <summary>
        /// Write out the student name to the console
        /// </summary>
        public void DisplayName()
        {
            string name = ConcatenateName();
            Console.WriteLine(name);
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
