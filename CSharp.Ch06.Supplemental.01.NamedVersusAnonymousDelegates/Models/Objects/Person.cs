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

namespace CSharp.Ch06.Supplemental._01.NamedVersusAnonymousDelegates.Models.Objects
{
    /// <summary>
    /// Defines a person
    /// </summary>
    public class Person
    {
        #region Properties
        /// <summary>
        /// Instance Name
        /// </summary>
        public string Name;

        /// <summary>
        /// A method that returns a string.
        /// </summary>
        /// <returns><see cref="string"/></returns>
        public delegate string GetStringDelegate();

        /// <summary>
        /// Delegate that will hold a static method
        /// </summary>
        public GetStringDelegate StaticMethod;

        /// <summary>
        /// Delegate that will hold an instance method
        /// </summary>
        public GetStringDelegate InstanceMethod;
        #endregion

        #region Public Methods
        /// <summary>
        /// A static method.
        /// </summary>
        /// <returns><see cref="string"/></returns>
        public static string StaticName()
        {
            return "Static";
        }

        /// <summary>
        /// Return this instance's Name.
        /// </summary>
        /// <returns>Instance name<see cref="string"/></returns>
        public string GetName()
        {
            return Name;
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
