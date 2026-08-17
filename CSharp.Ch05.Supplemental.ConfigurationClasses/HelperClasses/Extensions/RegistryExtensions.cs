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
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using CSharp.SharedLibrary.Models;
using Microsoft.Win32;
#endregion

namespace CSharp.Ch05.Supplemental.ConfigurationClasses.HelperClasses.Extensions
{
    // Adapted from the DPAPI Library by J.D. Meier, Alex Mackman, Michael Dunner,
    // and Srinath Vasireddy.
    //
    // This class provides a static method capable of reading an encrypted
    // string from the registry (using DPAPI), given a string in the format
    //     "registry:key,value"
    // The caller must have access to read the registry key.
    internal static class RegistryExtensions
    {
        #region Constants
        // Default timeout for RegEx operations. This is used to prevent a maliciously crafted string from
        // causing a denial of service by taking an excessive amount of time to evaluate.
        private static readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(10);
        #endregion

        #region DPAPI Methods
        private const int CryptprotectUiForbidden = 0x1;

        [DllImport("Crypt32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool CryptUnprotectData(
            ref DataBlob pDataIn,
            String szDataDescr,
            IntPtr pOptionalEntropy,
            IntPtr pvReserved,
            IntPtr pPromptStruct,
            int dwFlags,
            ref DataBlob pDataOut);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct DataBlob
        {
            public int cbData;
            public IntPtr pbData;
        }
        
        private static byte[] Decrypt(byte[] cipherText)
        {
            var plainTextBlob = new DataBlob();
            var cipherBlob = new DataBlob();

            try
            {
                try
                {
                    int cipherTextSize = cipherText.Length;
                    cipherBlob.pbData = Marshal.AllocHGlobal(cipherTextSize);
                    if (cipherBlob.pbData == IntPtr.Zero)
                    {
                        throw new DatabankException("Unable to allocate cipherText buffer.");
                    }
                    cipherBlob.cbData = cipherTextSize;
                    Marshal.Copy(cipherText, 0, cipherBlob.pbData, cipherBlob.cbData);
                }
                catch (Exception ex)
                {
                    throw new DatabankException("Exception marshalling data.", ex);
                }

                var retVal = CryptUnprotectData(ref cipherBlob, null,
                    IntPtr.Zero, IntPtr.Zero,
                    IntPtr.Zero, CryptprotectUiForbidden,
                    ref plainTextBlob);
                if (!retVal)
                {
                    throw new DatabankException("Decryption failed.");
                }

                if (cipherBlob.pbData != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(cipherBlob.pbData);
                }
            }
            catch (Exception ex)
            {
                throw new DatabankException("Exception decrypting.", ex);
            }


            byte[] plainText = new byte[plainTextBlob.cbData];
            Marshal.Copy(plainTextBlob.pbData, plainText, 0, plainTextBlob.cbData);
            return plainText;
        }
        #endregion

        #region Encrypted Registry Value Extension Methods
        /// <summary>
        /// Determines whether the string has been encrypted into the registry
        /// This will return true for any value beginning with "registry:" and containing a comma to separate the registry path from the registry key name
        /// Example value: registry:HKLM\SOFTWARE\DataBank\CustomServicePortal\ASPNET_SETREG,userName
        /// </summary>
        /// <param name="value">Value to test</param>
        /// <returns>True if value matches the encrypted identifier regex</returns>
        public static bool IsEncrypted(this string value)
        {
            return Regex.IsMatch(value, @"^registry:(.+),(.+)$", RegexOptions.Compiled, _defaultTimeout);
        }

        /// <summary>
        /// Opens the registry key designated by the value and returns the decrypted registry key value
        /// Example value: registry:HKLM\SOFTWARE\DataBank\DeveloperTraining\ASPNET_SETREG,userName
        /// </summary>
        /// <param name="value">Registry key path string</param>
        /// <returns>Decrypted registry key value</returns>
        public static string DecryptRegistryKey(this string value)
        {
            var registryKeyPath = Regex.Match(value, "\\\\(.+),", RegexOptions.Compiled, _defaultTimeout).Groups[1].Value;
            var registryKeyName = Regex.Match(value, ",(.+)$", RegexOptions.Compiled, _defaultTimeout).Groups[1].Value;
            var registryHiveName = Regex.Match(value, "^registry:([A-Za-z]+)\\\\", RegexOptions.Compiled, _defaultTimeout).Groups[1].Value;
            RegistryKey registryHive;

            #pragma warning disable IDE0066 // Not using switch expressions in lesson code to maintain compatibility with older C# versions
            switch (registryHiveName.ToUpper())
            {
                case "HKLM":
                    registryHive = Registry.LocalMachine;
                    break;
                case "HKCR":
                    registryHive = Registry.ClassesRoot;
                    break;
                case "HKCU":
                    registryHive = Registry.CurrentUser;
                    break;
                case "HKU":
                case "HKCC":
                    registryHive = Registry.Users;
                    break;
                default:
                    throw new DatabankException($"Unknown Registry hive name '{registryHiveName}'");
            }
            #pragma warning restore IDE0066

            byte[] bytes;

            using (var registryKey = registryHive.OpenSubKey(registryKeyPath, false))
            {
                if (registryKey == null)
                {
                    throw new DatabankException(
                        $"Error accessing Registry Key Path '{registryKeyPath}'. Ensure that you have run the aspnet_setreg.exe command to encrypt the credentials and also verify 'Read' permissions to this path have been granted to the account running the Application Pool");
                }

                bytes = (byte[])registryKey.GetValue(registryKeyName);
                #pragma warning disable IDE0270 // Coalesce expression shown in comment below, but not used in lesson code
                if (bytes == null)
                {
                    throw new DatabankException(
                        $"Error accessing Registry Key '{registryKeyPath}'. Ensure that you have run the aspnet_setreg.exe command to encrypt the credentials and also verify 'Read' permissions to this path have been granted to the account running the Application Pool");
                }
                #pragma warning restore IDE0270 // Use coalesce expression

                #pragma warning disable S125 // Allow commented out code for educational purposes
                // Alternative syntax can use nullcoalescing operator to simplify the null check and assignment
                //bytes = (byte[])registryKey.GetValue(registryKeyName) ?? throw new DatabankException(
                //        $"Error accessing Registry Key '{registryKeyPath}'. Ensure that you have run the aspnet_setreg.exe command to encrypt the credentials and also verify 'Read' permissions to this path have been granted to the account running the Application Pool");
                #pragma warning restore S125
            }

            return Encoding.Unicode.GetString(Decrypt(bytes));
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
