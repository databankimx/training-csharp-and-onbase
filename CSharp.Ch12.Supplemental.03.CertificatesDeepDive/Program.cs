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
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch12.Supplemental._03.CertificatesDeepDive
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * The main lesson's CreatingAndInspectingACertificate() built the simplest
         *   possible self-signed certificate and read a handful of its properties. This
         *   Supplemental goes deeper: adding real extensions (the fields that actually
         *   tell a relying party what a certificate is FOR), exporting/importing in both
         *   the private-key-included format (PFX) and public-only format (CER), using the
         *   Windows certificate store, and validating a certificate chain, including
         *   seeing exactly what "untrusted" looks like for a self-signed certificate.
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                AddingCertificateExtensions();
                GenericFunctions.Pause();

                ExportingAndImportingCertificates();
                GenericFunctions.Pause();

                UsingTheCertificateStore();
                GenericFunctions.Pause();

                ValidatingACertificateChain();
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
        // Real certificates declare what they're actually FOR, via extensions
        private static void AddingCertificateExtensions()
        {
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest(
                "CN=CSharp.Ch12.ExtensionsDemo",
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            // Basic Constraints: is this certificate allowed to ISSUE other certificates
            //   (a Certificate Authority) or is it an end-entity certificate only? A real
            //   CA's root/intermediate certificates set this to true, a website's or
            //   person's certificate sets it to false, critically, a browser refuses to
            //   trust a certificate chain where a NON-CA certificate tries to sign another
            //   certificate, this extension is what makes that check possible at all.
            request.CertificateExtensions.Add(
                new X509BasicConstraintsExtension(certificateAuthority: false, hasPathLengthConstraint: false, pathLengthConstraint: 0, critical: true));

            // Key Usage: which cryptographic OPERATIONS is this certificate's key actually
            //   allowed to be used for. A certificate meant only for encryption shouldn't
            //   also be trusted to produce digital signatures, and vice versa, keeping
            //   these separate limits the damage if a key is ever compromised.
            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, critical: true));

            using X509Certificate2 certificate = request.CreateSelfSigned(
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow.AddYears(1));

            Console.WriteLine($"Subject: {certificate.Subject}");
            Console.WriteLine($"{Environment.NewLine}Extensions on this certificate:");
            foreach (var extension in certificate.Extensions)
            {
                Console.WriteLine($" - {extension.Oid.FriendlyName}: {extension.Format(multiLine: false)}");
            }
        }

        // Exporting: PFX (includes the PRIVATE key, password-protected) vs. CER (PUBLIC
        //   key only, no password needed, safe to hand out freely)
        private static void ExportingAndImportingCertificates()
        {
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest("CN=CSharp.Ch12.ExportDemo", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            using X509Certificate2 original = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

            // PFX (PKCS#12): the format for MOVING a certificate somewhere it still needs
            //   to be able to prove ownership of the private key, password-protected
            //   specifically because it contains that private key.
            byte[] pfxBytes = original.Export(X509ContentType.Pfx, "P@ssw0rd123!");

            // CER: PUBLIC key and metadata only, no password, no private key, this is
            //   what you'd actually hand out to someone who just needs to VERIFY things
            //   signed by this certificate, or encrypt something TO it.
            byte[] cerBytes = original.Export(X509ContentType.Cert);

            Console.WriteLine($"PFX export: {pfxBytes.Length} bytes (includes the private key, password-protected)");
            Console.WriteLine($"CER export: {cerBytes.Length} bytes (public key and metadata only, no password)");

            using var reimportedFromPfx = new X509Certificate2(pfxBytes, "P@ssw0rd123!");
            using var reimportedFromCer = new X509Certificate2(cerBytes);

            Console.WriteLine($"{Environment.NewLine}Re-imported from PFX, has private key: {reimportedFromPfx.HasPrivateKey}");
            Console.WriteLine($"Re-imported from CER, has private key: {reimportedFromCer.HasPrivateKey}");
            Console.WriteLine($"{Environment.NewLine}Worth internalizing which format to hand to whom: give the CER (or the raw public");
            Console.WriteLine("key) to anyone who needs to verify your signatures or encrypt data TO you. Give");
            Console.WriteLine("the PFX to nobody but the system that will actually USE this identity itself,");
            Console.WriteLine("anyone holding the PFX can impersonate this certificate's owner entirely.");
        }

        // The Windows certificate store: a system-managed place to keep certificates,
        //   rather than juggling loose files
        private static void UsingTheCertificateStore()
        {
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest("CN=CSharp.Ch12.StoreDemo", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            using X509Certificate2 certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

            // CurrentUser\My ("Personal") is the standard store for a user's own
            //   certificates, doesn't require administrator privileges to write to,
            //   unlike LocalMachine stores, which generally do.
            using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);

            try
            {
                store.Add(certificate);
                Console.WriteLine($"Added certificate with thumbprint {certificate.Thumbprint} to CurrentUser\\My.");

                // Look it back up by thumbprint, exactly how a real application would
                //   locate a specific, already-installed certificate at runtime, rather
                //   than needing a file path to it at all.
                var found = store.Certificates.Find(X509FindType.FindByThumbprint, certificate.Thumbprint, validOnly: false);
                Console.WriteLine($"Found {found.Count} matching certificate(s) back in the store by thumbprint.");
            }
            finally
            {
                // Clean up: this is a throwaway demo certificate, remove it so this
                //   lesson doesn't leave junk certificates sitting in a real user's
                //   certificate store after each run.
                store.Remove(certificate);
                Console.WriteLine($"{Environment.NewLine}Removed the demo certificate again, cleaning up after itself.");
            }
        }

        // Chain validation: what actually happens when something checks whether a
        //   certificate should be TRUSTED, not just whether it's well-formed
        private static void ValidatingACertificateChain()
        {
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest("CN=CSharp.Ch12.ChainDemo", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            using X509Certificate2 certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

            using var chain = new X509Chain();
            bool isChainValid = chain.Build(certificate);

            Console.WriteLine($"Chain built successfully: {isChainValid}");
            Console.WriteLine($"{Environment.NewLine}Chain status flags:");
            foreach (var status in chain.ChainStatus)
            {
                Console.WriteLine($" - {status.Status}: {status.StatusInformation.Trim()}");
            }

            Console.WriteLine($"{Environment.NewLine}A self-signed certificate's chain reports UntrustedRoot, exactly as expected, its");
            Console.WriteLine("own issuer (itself) isn't in the machine's trusted root store. This is the same");
            Console.WriteLine("check a browser performs on every HTTPS connection, and exactly why self-signed");
            Console.WriteLine("certificates trigger a warning page rather than being silently accepted, the chain");
            Console.WriteLine("genuinely doesn't terminate at anything the system already trusts.");
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
