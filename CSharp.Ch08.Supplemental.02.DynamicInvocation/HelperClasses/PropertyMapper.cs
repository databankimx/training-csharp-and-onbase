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
using System.Linq;
#endregion

namespace CSharp.Ch08.Supplemental._02.DynamicInvocation.HelperClasses
{
    /// <summary>
    /// A small, reusable, reflection-based property mapper: copies every property that
    /// matches by name and type from a source object onto a destination object. This is
    /// a simplified version of what libraries like AutoMapper do under the hood, and is
    /// this project's answer to "why would I actually want reflection in real code".
    /// </summary>
    public static class PropertyMapper
    {
        #region Public Methods
        /// <summary>
        /// Copy every matching (same name, same type, writable) property from source to destination
        /// </summary>
        /// <param name="source">Object to copy property values from</param>
        /// <param name="destination">Object to copy property values onto</param>
        public static void CopyMatchingProperties(object source, object destination)
        {
            var sourceProperties = source.GetType().GetProperties();
            var destinationProperties = destination.GetType().GetProperties();

            foreach (var sourceProperty in sourceProperties)
            {
                // Only copy a property when the destination has a property with the SAME
                //   name, the SAME type, and a public setter. Anything else (ProductDto's
                //   Source property, for example, which Product has no counterpart for at
                //   all) is silently skipped, not an error.
                var destinationProperty = destinationProperties.FirstOrDefault(p =>
                    p.Name == sourceProperty.Name &&
                    p.PropertyType == sourceProperty.PropertyType &&
                    p.CanWrite);

                if (destinationProperty == null) continue;

                object value = sourceProperty.GetValue(source);
                destinationProperty.SetValue(destination, value);
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
