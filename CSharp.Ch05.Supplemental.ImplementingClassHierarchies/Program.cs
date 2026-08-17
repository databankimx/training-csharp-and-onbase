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
using CSharp.Ch05.Supplemental.ImplementingClassHierarchies.HelperClasses;
using CSharp.Ch05.Supplemental.ImplementingClassHierarchies.Models.Objects;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch05.Supplemental.ImplementingClassHierarchies
{
    // Default class for console executable
    internal static class Program
    {
        #region Main Executable Method
        // Presence of Main() method renders the class runnable
        private static void Main()
        {
            try
            {
                var someone = new Contact
                {
                    FirstName = "Jordan",
                    MiddleName = "A",
                    LastName = "Rivera",
                    HomeAddress = new Address
                    {
                        StreetAddress = "123 Main St",
                        City = "Lewisville",
                        State = "TX",
                        ZipCode = "75067"
                    },
                    HomePhone = new Telephone
                    {
                        Number = "2145550234"
                    },
                    WorkPhone = new Telephone
                    {
                        Number = "2145550199"
                    },
                    MobilePhone = new Telephone
                    {
                        Number = "2145550172"
                    },
                    Email = "jrivera@databankimx.com",
                    WorkAddress = new BusinessAddress
                    {
                        CompanyName = "DataBank IMX",
                        StreetAddress = "456 Corporate Dr",
                        City = "Lewisville",
                        State = "TX",
                        ZipCode = "75067"
                    }
                };

                Console.WriteLine("Name");
                Console.WriteLine("----");
                Console.WriteLine($"FullName(): {someone.FullName()}");
                Console.WriteLine($"FullName(reverse: true): {someone.FullName(reverse: true)}");
                Console.WriteLine($"FullName(includeMiddle: true): {someone.FullName(includeMiddle: true)}");
                Console.WriteLine($"Initials: {someone.Initials()}");
                Console.WriteLine();

                Console.WriteLine("Contact Information");
                Console.WriteLine("-------------------");
                Console.WriteLine($"Email: {someone.Email}");
                Console.WriteLine($"Home Phone: {someone.HomePhone.Number}");
                Console.WriteLine($"Work Phone: {someone.WorkPhone.Number}");
                Console.WriteLine($"Mobile Phone: {someone.MobilePhone.Number}");
                Console.WriteLine();

                Console.WriteLine("Home Address");
                Console.WriteLine("------------");
                Console.WriteLine(someone.HomeAddress.StreetAddress);
                Console.WriteLine($"{someone.HomeAddress.City}, {someone.HomeAddress.State} {someone.HomeAddress.ZipCode}");
                Console.WriteLine();

                Console.WriteLine("Work Address");
                Console.WriteLine("------------");
                Console.WriteLine(someone.WorkAddress.CompanyName);
                Console.WriteLine(someone.WorkAddress.StreetAddress);
                Console.WriteLine($"{someone.WorkAddress.City}, {someone.WorkAddress.State} {someone.WorkAddress.ZipCode}");

                GenericFunctions.Pause();
            }
            catch (Exception ex)
            {
                new DatabankException("Error Caught!", ex).Log();
                GenericFunctions.Pause();
            }
            finally
            {
                GenericFunctions.Pause(final: true);
            }
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
