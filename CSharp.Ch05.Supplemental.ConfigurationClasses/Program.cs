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
using System.Configuration;
using CSharp.Ch05.Supplemental.ConfigurationClasses.Models.Configuration;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch05.Supplemental.ConfigurationClasses
{
    // Default class for console executable
    internal static class Program
    {
        // Presence of Main() method renders the class runnable
        private static void Main()
        {
            try
            {
                // You can use the default appSettings section
                string appSetting = ConfigurationManager.AppSettings["subject"];
                Console.WriteLine($"Today, we are learning about [{appSetting}]{Environment.NewLine}");

                // But it is much better to create custom configuration sections

                // Read the defined config file information
                var onBaseSettings = (OnBaseSettings)ConfigurationManager.GetSection(OnBaseSettings.SectionName);

                // Iterate across the configuration information
                foreach (DocumentTypeElement documentType in onBaseSettings.DocumentTypes)
                {
                    Console.WriteLine("Document Type:{0}  Name: [{1}]{0}  ID: [{2}]{0}  Keyword Types:",
                        Environment.NewLine, documentType.Name, documentType.Id);
                    foreach (KeywordTypeElement keywordType in documentType.KeywordTypes)
                    {
                        Console.WriteLine("    Keyword Type:{0}      Name: [{1}]{0}      ID: [{2}]{0}      Data Type: [{3}]{4}",
                            Environment.NewLine,
                            keywordType.Name,
                            keywordType.Id,
                            keywordType.DataType,
                            keywordType.DataType.ToLower().StartsWith("alpha")
                                ? $"{Environment.NewLine}      Length: [{keywordType.DataLength}]"
                                : "");
                    }
                }
                Console.WriteLine();

                Console.WriteLine("OnBase Connection Settings:");
                Console.WriteLine("--------------------------");
                Console.WriteLine($"App Server URL: {onBaseSettings.ServiceLocation.ServicePath}");
                Console.WriteLine($"Data Source:    {onBaseSettings.ServiceLocation.DataSource}");
                Console.WriteLine($"Username:       {onBaseSettings.ServiceLocation.DecryptedUsername}");
                Console.WriteLine($"Password:       {onBaseSettings.ServiceLocation.DecryptedPassword}");
                Console.WriteLine();
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
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
