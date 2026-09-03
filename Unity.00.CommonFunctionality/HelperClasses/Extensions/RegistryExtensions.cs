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
using Microsoft.Win32;
using Unity._00.CommonFunctionality.Models.Objects;
#endregion

namespace Unity._00.CommonFunctionality.HelperClasses.Extensions
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
        // Regex Timeout
        private readonly static TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);
        #endregion

        #region DPAPI Methods
        // Flag to indicate that no UI should be displayed during decryption
        private const int CryptprotectUiForbidden = 0x1;

        // Import the CryptUnprotectData function from Crypt32.dll
        [DllImport("Crypt32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool CryptUnprotectData(
            ref DataBlob pDataIn,
            String szDataDescr,
            IntPtr pOptionalEntropy,
            IntPtr pvReserved,
            IntPtr pPromptStruct,
            int dwFlags,
            ref DataBlob pDataOut);

        // DataBlob structure used by DPAPI
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct DataBlob
        {
            public int cbData;
            public IntPtr pbData;
        }

        // Decrypts a byte array using DPAPI
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
                    throw new DatabankException("Exception marshalling data. " + ex.Message);
                }

                var retVal = CryptUnprotectData(ref cipherBlob, null,
                    IntPtr.Zero, IntPtr.Zero,
                    IntPtr.Zero, CryptprotectUiForbidden,
                    ref plainTextBlob);
                if (!retVal)
                {
                    throw new DatabankException("Decryption failed. ");
                }

                if (cipherBlob.pbData != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(cipherBlob.pbData);
                }
            }
            catch (Exception ex)
            {
                throw new DatabankException("Exception decrypting. " + ex.Message);
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
            return Regex.IsMatch(value, @"^registry:(.+),(.+)$", RegexOptions.Compiled, RegexTimeout);
        }

        /// <summary>
        /// Opens the registry key designated by the value and returns the decrypted registry key value
        /// Example value: registry:HKLM\SOFTWARE\DataBank\DeveloperTraining\ASPNET_SETREG,userName
        /// </summary>
        /// <param name="value">Registry key path string</param>
        /// <returns>Decrypted registry key value</returns>
        public static string DecryptRegistryKey(this string value)
        {
            var registryKeyPath = Regex.Match(value, "\\\\(.+),", RegexOptions.Compiled, RegexTimeout).Groups[1].Value;
            var registryKeyName = Regex.Match(value, ",(.+)$", RegexOptions.Compiled, RegexTimeout).Groups[1].Value;
            var registryHiveName = Regex.Match(value, "^registry:([A-Za-z]+)\\\\", RegexOptions.Compiled, RegexTimeout).Groups[1].Value;

            RegistryKey registryHive = registryHiveName.ToUpper() switch
            {
                "HKLM" => Registry.LocalMachine,
                "HKCR" => Registry.ClassesRoot,
                "HKCU" => Registry.CurrentUser,
                "HKU" or "HKCC" => Registry.Users,
                _ => throw new DatabankException($"Unknown Registry hive name '{registryHiveName}'"),
            };

            var registryKey = registryHive.OpenSubKey(registryKeyPath, false) ?? throw new DatabankException(
                    $"Error accessing Registry Key Path '{registryKeyPath}'. Ensure that you have run the aspnet_setreg.exe command to encrypt the credentials and also verify 'Read' permissions to this path have been granted to the account running the Application Pool");

            byte[] bytes = (byte[])registryKey.GetValue(registryKeyName);

            return bytes == null
                ? throw new DatabankException(
                    $"Error accessing Registry Key '{registryKeyPath}'. Ensure that you have run the aspnet_setreg.exe command to encrypt the credentials and also verify 'Read' permissions to this path have been granted to the account running the Application Pool")
                : Encoding.Unicode.GetString(Decrypt(bytes));
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
