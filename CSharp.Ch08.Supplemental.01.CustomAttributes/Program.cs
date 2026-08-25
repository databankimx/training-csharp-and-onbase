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
using System.Linq;
using System.Reflection;
using CSharp.Ch08.Supplemental._01.CustomAttributes.Models.Attributes;
using CSharp.Ch08.Supplemental._01.CustomAttributes.Models.Objects;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch08.Supplemental._01.CustomAttributes
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * The main Chapter 8 lesson (CSharp.Ch08.Reflection) covers the basics of custom
         *   attributes: defining one, applying it once, reading it back with
         *   GetCustomAttribute<T>(). This project goes deeper into four things that come up
         *   quickly once you actually start using attributes for real:
         *
         * - AllowMultiple: can the SAME attribute type be applied more than once to the same
         *     target? Off by default; DataMappingAttribute turns it on to let one class carry
         *     its entire external-to-internal column mapping as a stack of attributes.
         *
         * - Named initializer syntax: [SomeAttribute(Property1 = value1, Property2 = value2)]
         *     sets properties directly, rather than requiring every value to flow through the
         *     constructor. AuditableAttribute uses this, along with an enum-typed property.
         *
         * - IsDefined() vs GetCustomAttribute<T>(): IsDefined() only checks presence (a bool),
         *     it never actually allocates an attribute instance, worth knowing when all you
         *     need is a yes/no answer.
         *
         * - Inherited: does a subclass of an attributed class get reported as having that
         *     attribute too, via reflection, even without its own copy of the attribute?
         *     Governed by [AttributeUsage(..., Inherited = true/false)] on the attribute
         *     itself, demonstrated with two attributes on the same base class that disagree.
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                // Demonstrate AllowMultiple: reading a stack of the same attribute type
                UsingAllowMultiple();
                GenericFunctions.Pause();

                // Demonstrate named initializer syntax and an enum-typed attribute property
                UsingNamedInitializersAndEnums();
                GenericFunctions.Pause();

                // Demonstrate IsDefined() vs GetCustomAttribute<T>()
                UsingIsDefined();
                GenericFunctions.Pause();

                // Demonstrate Inherited = true vs Inherited = false
                UsingAttributeInheritance();
                GenericFunctions.Pause();
                #endregion
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

        #region Lesson Methods
        // Demonstrate reading every instance of an AllowMultiple attribute off one class
        private static void UsingAllowMultiple()
        {
            var recordType = typeof(CustomerRecord);

            // GetCustomAttribute<T>() (singular) would throw here, since there's more than
            //   one DataMappingAttribute on this type. GetCustomAttributes<T>() (plural)
            //   returns all of them.
            var mappings = recordType.GetCustomAttributes<DataMappingAttribute>().ToList();

            Console.WriteLine($"{recordType.Name} carries {mappings.Count} DataMappingAttribute instance(s):");
            foreach (var mapping in mappings)
            {
                Console.WriteLine($" - Column '{mapping.ColumnName}' maps to property '{mapping.PropertyName}'");
            }
        }

        // Demonstrate named initializer syntax and an enum-typed attribute property
        private static void UsingNamedInitializersAndEnums()
        {
            var recordType = typeof(BaseRecord);
            var auditable = recordType.GetCustomAttribute<AuditableAttribute>();

            if (auditable != null)
            {
                Console.WriteLine($"{recordType.Name} is auditable: {auditable.Enabled}, at level: {auditable.Level}");
            }
        }

        // Demonstrate IsDefined() (a yes/no check, no allocation) vs GetCustomAttribute<T>()
        //   (allocates and returns the actual attribute instance)
        private static void UsingIsDefined()
        {
            var recordType = typeof(BaseRecord);

            bool hasAuditable = Attribute.IsDefined(recordType, typeof(AuditableAttribute));
            Console.WriteLine($"IsDefined<AuditableAttribute>() on {recordType.Name}: {hasAuditable}");

            // IsDefined() answers "is it there at all", without ever constructing an
            //   AuditableAttribute instance behind the scenes, cheaper when you don't
            //   actually need the attribute's data, just whether it's present.
        }

        // Demonstrate Inherited = true vs Inherited = false
        private static void UsingAttributeInheritance()
        {
            var derivedType = typeof(DerivedRecord);

            // AuditableAttribute is marked Inherited = true: DerivedRecord declares no
            //   [Auditable] attribute of its own, but reflection still finds BaseRecord's.
            var inheritedAuditable = derivedType.GetCustomAttribute<AuditableAttribute>();
            Console.WriteLine($"{derivedType.Name}.GetCustomAttribute<AuditableAttribute>(): " +
                               (inheritedAuditable != null ? $"found (Level = {inheritedAuditable.Level})" : "not found"));

            // ClassSpecificAttribute is marked Inherited = false: even though BaseRecord has
            //   one, DerivedRecord does NOT report having it.
            var notInherited = derivedType.GetCustomAttribute<ClassSpecificAttribute>();
            Console.WriteLine($"{derivedType.Name}.GetCustomAttribute<ClassSpecificAttribute>(): " +
                               (notInherited != null ? "found" : "not found"));
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
