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
using System.Configuration;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Unity._00.CommonFunctionality.HelperClasses.Extensions;
using Unity._00.CommonFunctionality.Models.Objects;
#endregion

namespace Unity._06.UnityFormDefaultValues
{
    #region Training Notes
    /*
     * *Migration Note: BaseUrl and Token were originally hardcoded constants, Token in
     * particular is a genuine secret, an HMAC signing key that authenticates the
     * pre-populated field values in the generated URL, whoever holds it can construct
     * valid signed URLs for this Unity Form integration. Both moved to App.config's
     * <appSettings>, and Token is read through RegistryExtensions.IsEncrypted()/
     * DecryptRegistryKey() (reused directly from Unity.00.CommonFunctionality, changed
     * from internal to public specifically to make this reuse possible), the exact same
     * DPAPI/registry-encryption pattern ServiceLocation/IdpSettings use for their own
     * secrets. Every ApplicationException here was also changed to DatabankException,
     * matching this training set's standard, and previously undocumented here.
     */
    #endregion

    internal static class Program
    {
        #region Constants
        // List of form fields to pre-populate (key = field ID - value = field value)
        private static readonly List<KeyValuePair<string, string>> FormFields = [new("lawId", "2")];
        #endregion

        #region Main executable method
        private static void Main()
        {
            try
            {
                string baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
                if (string.IsNullOrEmpty(baseUrl))
                    throw new DatabankException("BaseUrl is not configured in App.config!");

                string rawToken = ConfigurationManager.AppSettings["Token"];
                if (string.IsNullOrEmpty(rawToken))
                    throw new DatabankException("Token is not configured in App.config!");
                string token = rawToken.IsEncrypted() ? rawToken.DecryptRegistryKey() : rawToken;

                // Create the parameter string for the URL
                string parameterString = CreateParameterString(FormFields);
                Console.WriteLine($"Parameter String:{Environment.NewLine}{parameterString}");

                byte[] parameterBytes = StringToBytes(parameterString);

                // Create the hash token for the parameter query string
                string hash = GenerateHash(token, parameterBytes);
                Console.WriteLine($"Hash:{Environment.NewLine}{hash}");

                // Generate the final URL
                string url = GenerateUrl(baseUrl, parameterString, hash);
                Console.WriteLine($"URL:{Environment.NewLine}{url}");

                // Test the URL in the default browser
                Process.Start(url);
            }
            catch (Exception ex)
            {
                while (ex != null)
                {
                    Console.WriteLine(ex);
                    ex = ex.InnerException;
                }
            }
            finally
            {
                Console.WriteLine("Done! Press <ENTER> to exit...");
                Console.ReadLine();
            }
        }
        #endregion

        #region Helper Functions
        // Create a string of parameters representing the default-value fields
        private static string CreateParameterString(List<KeyValuePair<string, string>> fields)
        {
            try
            {
                var builder = new StringBuilder();
                foreach (var field in fields)
                {
                    builder.Append($"&ufpre{Uri.EscapeDataString(field.Key)}={Uri.EscapeDataString(field.Value)}");
                }
                return builder.ToString();
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error creating parameter string!", ex);
            }
        }

        // Get the UTF-8 encoded byte array from the string
        private static byte[] StringToBytes(string value)
        {
            try
            {
                return Encoding.UTF8.GetBytes(value);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error converting string to byte array!", ex);
            }
        }

        // Generate the hash string
        private static string GenerateHash(string token, byte[] parameterBytes)
        {
            try
            {
                byte[] tokenBytes = Convert.FromBase64String(token);
                using (var hmac = new HMACSHA256(tokenBytes))
                {
                    var hashBytes = hmac.ComputeHash(parameterBytes);
                    return Uri.EscapeDataString(Convert.ToBase64String(hashBytes));
                }
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error generating hash from byte array!", ex);
            }
        }

        // Create the shared form URL
        private static string GenerateUrl(string baseUrl, string parameterString, string hash)
        {
            try
            {
                return $"{baseUrl}{parameterString}&ufprehash={hash}";
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error generating shared form URL!", ex);
            }
        }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
