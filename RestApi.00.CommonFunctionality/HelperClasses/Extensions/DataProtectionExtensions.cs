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
using System.IO;
using Microsoft.AspNetCore.DataProtection;
#endregion

namespace RestApi._00.CommonFunctionality.HelperClasses.Extensions
{
    #region Training Notes
    /*
     * *Migration Note: replaces what would otherwise be a straight port of
     * Unity.00.CommonFunctionality's own RegistryExtensions (DPAPI via raw P/Invoke, with
     * secrets stored in the registry via aspnet_setreg.exe and referenced from config as
     * "registry:key,value"). Once this track was no longer tied to net48, there was no
     * reason to keep that mechanism: Microsoft.AspNetCore.DataProtection's IDataProtector
     * is the actively-maintained, officially-recommended modern equivalent (despite the
     * ASP.NET Core-sounding package name, it works fine in any .NET app, via
     * DataProtectionProvider.Create(...), not just ASP.NET Core hosts).
     *
     * Two real improvements over the old mechanism, not just a rename:
     *  1. No more registry indirection. The encrypted value IS the config value (a
     *     "protected:" prefix followed by the protected Base64 payload), there's no
     *     separate registry key to manage, no aspnet_setreg.exe to run, no read
     *     permissions to grant to an application pool identity.
     *  2. Encryption can happen from WITHIN the app now (see Protect below), not just
     *     decryption. aspnet_setreg.exe was a separate, manually-run external tool; an
     *     admin had no way to encrypt a NEW secret from inside Unity.TestHarness itself.
     *     RestApi.TestHarness's Settings page can now genuinely encrypt secret fields
     *     before writing them to config, on Save.
     *
     * Key storage: %LOCALAPPDATA%\DataBank\RestApi.TestHarness\DataProtection-Keys, a
     * persistent, per-Windows-user location (DataProtection's own key ring, not the
     * protected values themselves, which still live in config). This mirrors DPAPI's own
     * user-profile-scoped nature: a protected value from this machine, for this Windows
     * user, only decrypts on that same machine, for that same user, same as before.
     *
     * The purpose string ("RestApi.00.CommonFunctionality.SecretProtection") is what
     * IDataProtector uses to derive a use-case-specific key from the underlying key ring,
     * preventing a value protected for one purpose from being unprotected via a
     * differently-purposed protector, even with the same key ring. Kept as one shared
     * purpose across every secret field in this project (Password, IdpClientSecret,
     * etc.), rather than one purpose per field. That's a deliberate simplification: nothing
     * in this training set's threat model calls for separating them, and one shared
     * purpose keeps ServiceLocation/IdpSettings from needing to know the field name at
     * protect/unprotect time.
     */
    #endregion

    /// <summary>
    /// Provides extension methods capable of protecting/unprotecting a secret string using
    /// Microsoft.AspNetCore.DataProtection's IDataProtector, storing the result as a
    /// "protected:" prefixed value directly in config, no registry involved.
    /// </summary>
    public static class DataProtectionExtensions
    {
        #region Constants
        // The prefix marking a config value as DataProtection-protected, rather than
        // plain text
        private const string ProtectedPrefix = "protected:";

        // The purpose string this project's IDataProtector is derived with; see this
        // class's own Training Notes for why one shared purpose is used for every secret
        // field
        private const string Purpose = "RestApi.00.CommonFunctionality.SecretProtection";
        #endregion

        #region Private Members
        // Lazily-created, reused for the lifetime of the process: DataProtectionProvider.Create
        // reads/creates the key ring on first use, no need to repeat that per call
        private static readonly Lazy<IDataProtector> Protector = new(CreateProtector);
        #endregion

        #region Protected Value Extension Methods
        /// <summary>
        /// Determines whether the string is a DataProtection-protected value (begins with
        /// the "protected:" prefix), as opposed to plain text.
        /// </summary>
        /// <param name="value">Value to test</param>
        /// <returns>True if value begins with the protected-value prefix</returns>
        public static bool IsProtected(this string value)
        {
            return !string.IsNullOrEmpty(value) && value.StartsWith(ProtectedPrefix, StringComparison.Ordinal);
        }

        /// <summary>
        /// Unprotects a "protected:"-prefixed value back to its original plain text.
        /// </summary>
        /// <param name="value">The protected value, including its "protected:" prefix</param>
        /// <returns>The original plain text</returns>
        public static string Unprotect(this string value)
        {
            var payload = value[ProtectedPrefix.Length..];
            return Protector.Value.Unprotect(payload);
        }

        /// <summary>
        /// Protects a plain text secret, returning a "protected:"-prefixed value suitable
        /// for storing directly in config. Only decryptable on this same machine, by this
        /// same Windows user (DataProtection's key ring is stored per-user, see this
        /// class's own Training Notes).
        /// </summary>
        /// <param name="value">The plain text secret to protect</param>
        /// <returns>A "protected:"-prefixed value</returns>
        public static string Protect(this string value)
        {
            return ProtectedPrefix + Protector.Value.Protect(value);
        }
        #endregion

        #region Private Methods
        // Build the IDataProtector this project's secrets are protected/unprotected with
        private static IDataProtector CreateProtector()
        {
            var keyPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DataBank", "RestApi.TestHarness", "DataProtection-Keys");

            Directory.CreateDirectory(keyPath);

            var provider = DataProtectionProvider.Create(new DirectoryInfo(keyPath));
            return provider.CreateProtector(Purpose);
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
