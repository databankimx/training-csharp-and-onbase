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
using System.Collections.Generic;
using CSharp.Ch05.Supplemental.Cloning.Models;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch05.Supplemental.Cloning
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
                Console.WriteLine("Chapter 5 Supplemental: Shallow and Deep Cloning");
                Console.WriteLine("=================================================\n");

                Person original = CreatePerson();

                DemonstrateReferenceAssignment(original);
                DemonstrateShallowClone(original);
                DemonstrateDeepClone(original);
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

        #region Demonstrations
        private static Person CreatePerson()
        {
            #pragma warning disable IDE0028 // Not simplifying collection initialization in lesson code
            return new Person
            {
                Name = "Ada Lovelace",
                Age = 36,
                HomeAddress = new Address
                {
                    Street = "123 Example Street",
                    City = "London",
                    State = "England"
                },
                Skills = new List<string>
                {
                    "Mathematics",
                    "Programming"
                }
            };
            #pragma warning restore IDE0028
        }

        private static void DemonstrateReferenceAssignment(Person original)
        {
            Console.WriteLine("1. REFERENCE ASSIGNMENT");
            Console.WriteLine("-----------------------");

            Person assigned = original;

            Console.WriteLine($"ReferenceEquals(original, assigned): {ReferenceEquals(original, assigned)}");
            Console.WriteLine("No new Person was created. Both variables refer to the exact same object.\n");
        }

        private static void DemonstrateShallowClone(Person original)
        {
            Console.WriteLine("2. SHALLOW CLONE");
            Console.WriteLine("----------------");

            Person shallow = original.ShallowClone();

            Console.WriteLine($"Same Person object:  {ReferenceEquals(original, shallow)}");
            Console.WriteLine($"Same Address object: {ReferenceEquals(original.HomeAddress, shallow.HomeAddress)}");
            Console.WriteLine($"Same Skills list:    {ReferenceEquals(original.Skills, shallow.Skills)}");
            Console.WriteLine();

            shallow.Name = "Shallow Copy";
            shallow.HomeAddress.City = "Chicago";
            shallow.Skills.Add("Shared-list surprise");

            Console.WriteLine("After changing the shallow clone:");
            Console.WriteLine($"Original.Name:             {original.Name}");
            Console.WriteLine($"Original.HomeAddress.City: {original.HomeAddress.City}");
            Console.WriteLine($"Original.Skills.Count:     {original.Skills.Count}");
            Console.WriteLine();
            Console.WriteLine("Name did not change because assigning a string replaces the clone's property value.");
            Console.WriteLine("City and Skills DID change because the nested Address and List are shared references.\n");

            // Restore the shared nested state before the next demonstration.
            original.HomeAddress.City = "London";
            original.Skills.Remove("Shared-list surprise");
        }

        private static void DemonstrateDeepClone(Person original)
        {
            Console.WriteLine("3. DEEP CLONE");
            Console.WriteLine("-------------");

            Person deep = original.DeepClone();

            Console.WriteLine($"Same Person object:  {ReferenceEquals(original, deep)}");
            Console.WriteLine($"Same Address object: {ReferenceEquals(original.HomeAddress, deep.HomeAddress)}");
            Console.WriteLine($"Same Skills list:    {ReferenceEquals(original.Skills, deep.Skills)}");
            Console.WriteLine();

            deep.Name = "Deep Copy";
            deep.HomeAddress.City = "Chicago";
            deep.Skills.Add("Independent list");

            Console.WriteLine("After changing the deep clone:");
            Console.WriteLine($"Original.Name:             {original.Name}");
            Console.WriteLine($"Original.HomeAddress.City: {original.HomeAddress.City}");
            Console.WriteLine($"Original.Skills.Count:     {original.Skills.Count}");
            Console.WriteLine($"Deep.HomeAddress.City:     {deep.HomeAddress.City}");
            Console.WriteLine($"Deep.Skills.Count:         {deep.Skills.Count}");
            Console.WriteLine();
            Console.WriteLine("The deep clone owns independent mutable child objects, so its changes do not affect the original.");
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
