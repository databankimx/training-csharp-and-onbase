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
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch12.UsingEncryptionAndManagingAssemblies
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * This final chapter covers two genuinely separate topics that happen to share a
         *   chapter: cryptography (encryption, hashing, certificates), and assembly
         *   management (versioning, strong naming, the GAC). The common thread between
         *   them is really "things that matter for shipping and securing real software,"
         *   not a shared technical mechanism.
         *
         * A note before any of this: .NET's cryptography APIs are deliberately designed so
         *   the ALGORITHM classes (Aes, Rsa, Sha256, etc.) are hard to misuse in ways that
         *   weaken security, but it's still entirely possible to build something insecure
         *   ON TOP of them (reusing an IV, storing a key next to the data it protects,
         *   hashing a password without a salt). Every method below notes the specific
         *   pitfall it's avoiding, worth reading those notes as carefully as the code
         *   itself.
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                #region Using Encryption
                UsingSymmetricEncryption();
                GenericFunctions.Pause();

                UsingAsymmetricEncryption();
                GenericFunctions.Pause();

                UsingStreamEncryption();
                GenericFunctions.Pause();

                HashingData();
                GenericFunctions.Pause();

                CreatingAndInspectingACertificate();
                GenericFunctions.Pause();
                #endregion

                #region Managing Assemblies
                InspectingAssemblyVersions();
                GenericFunctions.Pause();

                UnderstandingStrongNaming();
                GenericFunctions.Pause();

                UnderstandingTheGac();
                GenericFunctions.Pause();
                #endregion
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

        #region Using Encryption
        // Symmetric encryption (AES): the SAME key encrypts and decrypts
        private static void UsingSymmetricEncryption()
        {
            const string plaintext = "The shipment arrives Tuesday at 3 PM.";

            using var aes = Aes.Create();
            // Aes.Create() already generates a cryptographically random Key and IV, worth
            //   knowing explicitly rather than assuming: never reuse the same Key+IV pair
            //   to encrypt more than one message, doing so can leak information about the
            //   plaintext even without ever recovering the key itself.

            byte[] encrypted;
            using (var encryptor = aes.CreateEncryptor())
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                using (var writer = new StreamWriter(cryptoStream))
                {
                    writer.Write(plaintext);
                }
                encrypted = memoryStream.ToArray();
            }

            string decrypted;
            using (var decryptor = aes.CreateDecryptor())
            using (var memoryStream = new MemoryStream(encrypted))
            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
            using (var reader = new StreamReader(cryptoStream))
            {
                decrypted = reader.ReadToEnd();
            }

            Console.WriteLine($"Original:  {plaintext}");
            Console.WriteLine($"Encrypted: {Convert.ToBase64String(encrypted)}");
            Console.WriteLine($"Decrypted: {decrypted}");
            Console.WriteLine($"{Environment.NewLine}Symmetric encryption is fast and well-suited to bulk data, but both parties need");
            Console.WriteLine("the SAME key, which creates the \"key distribution problem\": how do you get the");
            Console.WriteLine("key to the other party without an attacker intercepting it too?");
        }

        // Asymmetric encryption (RSA): a DIFFERENT key encrypts than decrypts
        private static void UsingAsymmetricEncryption()
        {
            const string plaintext = "Meet at the usual place.";
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

            using var rsa = RSA.Create();
            // rsa.ExportParameters(true) would export the PRIVATE key too, ExportParameters(false)
            //   exports only the PUBLIC key, exactly what a real sender would actually receive
            //   from the recipient, they should never possess the private key at all.
            RSAParameters publicKey = rsa.ExportParameters(includePrivateParameters: false);

            using var rsaPublicOnly = RSA.Create();
            rsaPublicOnly.ImportParameters(publicKey);

            // OaepSHA1, NOT OaepSHA256, on net48 specifically: RSA.Create() here returns an
            //   RSACryptoServiceProvider (the legacy, CAPI-based provider), which only
            //   supports OaepSHA1 (or plain Pkcs1) padding, requesting OaepSHA256 throws
            //   CryptographicException ("Specified padding mode is not valid for this
            //   algorithm"). Modern .NET's RSA.Create() returns a different provider that
            //   DOES support OaepSHA256 directly, a genuine platform difference worth
            //   knowing about if code like this ever moves between the two. Using SHA-1
            //   for OAEP's internal padding scheme specifically (not for hashing/signing
            //   data) is still considered acceptable, unlike using SHA-1 for a digital
            //   signature or a certificate, where it would be a real weakness.
            byte[] encrypted = rsaPublicOnly.Encrypt(plaintextBytes, RSAEncryptionPadding.OaepSHA1);

            // Only the ORIGINAL "rsa" instance still holds the private key, this is what
            //   actually makes asymmetric encryption solve the key distribution problem:
            //   the public key can be handed out freely, only the private key (which never
            //   left the recipient's possession) can decrypt anything encrypted with it.
            byte[] decrypted = rsa.Decrypt(encrypted, RSAEncryptionPadding.OaepSHA1);

            Console.WriteLine($"Original:  {plaintext}");
            Console.WriteLine($"Encrypted: {Convert.ToBase64String(encrypted)}");
            Console.WriteLine($"Decrypted: {Encoding.UTF8.GetString(decrypted)}");
            Console.WriteLine($"{Environment.NewLine}RSA solves the key distribution problem (the public key really can be public),");
            Console.WriteLine("but it's slow and has a hard size limit on what it can encrypt directly (roughly");
            Console.WriteLine("the key size in bytes, minus padding overhead), that's why real systems typically");
            Console.WriteLine("use RSA to encrypt a random AES key, then use that AES key for the actual data,");
            Console.WriteLine("known as \"hybrid encryption\", see CSharp.Ch12.Supplemental.01 for RSA's other");
            Console.WriteLine("major use: digital signatures.");
        }

        // Stream encryption: CryptoStream chained directly onto a FileStream, encrypting
        //   data as it flows through rather than requiring it all in memory at once
        private static void UsingStreamEncryption()
        {
            string plainPath = Path.Combine(Path.GetTempPath(), $"ch12-plain-{Guid.NewGuid():N}.txt");
            string encryptedPath = Path.Combine(Path.GetTempPath(), $"ch12-encrypted-{Guid.NewGuid():N}.bin");
            string decryptedPath = Path.Combine(Path.GetTempPath(), $"ch12-decrypted-{Guid.NewGuid():N}.txt");

            try
            {
                File.WriteAllText(plainPath, "This file's contents never exist in memory as one giant plaintext blob.");

                using var aes = Aes.Create();

                // Chaining CryptoStream directly onto a FileStream (rather than a
                //   MemoryStream, as UsingSymmetricEncryption() did above) means data is
                //   encrypted a chunk at a time as it's read from the source file and
                //   written to the destination file. For a multi-gigabyte file, this is
                //   the difference between using a small, constant amount of memory and
                //   trying to load the ENTIRE file into a byte array first.
                using (var sourceStream = File.OpenRead(plainPath))
                using (var destinationStream = File.Create(encryptedPath))
                using (var cryptoStream = new CryptoStream(destinationStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    sourceStream.CopyTo(cryptoStream);
                }

                using (var sourceStream = File.OpenRead(encryptedPath))
                using (var cryptoStream = new CryptoStream(sourceStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (var destinationStream = File.Create(decryptedPath))
                {
                    cryptoStream.CopyTo(destinationStream);
                }

                Console.WriteLine($"Encrypted file: {new FileInfo(encryptedPath).Length} bytes");
                Console.WriteLine($"Decrypted file contents: {File.ReadAllText(decryptedPath)}");
            }
            finally
            {
                foreach (string path in new[] { plainPath, encryptedPath, decryptedPath })
                {
                    if (File.Exists(path)) File.Delete(path);
                }
            }
        }

        // Hashing: a ONE-WAY fingerprint, not encryption at all, there's no key and no
        //   way to reverse it back to the original input
        private static void HashingData()
        {
            const string original = "password123";
            const string tampered = "password124";

            // SHA256.Create() (an instance-based provider), not the static SHA256.HashData()
            //   convenience method, which was only added in .NET 5+ and isn't available on
            //   net48 (this project's target).
            using var sha256 = SHA256.Create();
            byte[] originalHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(original));
            byte[] tamperedHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(tampered));

            // Convert.ToHexString() is also .NET 5+ only, BitConverter.ToString() (which
            //   HAS been available since .NET Framework 1.1) plus stripping its "-"
            //   separators is the net48-compatible equivalent.
            string originalHashHex = BitConverter.ToString(originalHash).Replace("-", "");
            string tamperedHashHex = BitConverter.ToString(tamperedHash).Replace("-", "");

            Console.WriteLine($"\"{original}\" hashes to: {originalHashHex}");
            Console.WriteLine($"\"{tampered}\" hashes to: {tamperedHashHex}");
            Console.WriteLine($"{Environment.NewLine}Changing a SINGLE character produced a completely different hash, this is called");
            Console.WriteLine("the \"avalanche effect\", and it's exactly what makes hashing useful for detecting");
            Console.WriteLine("tampering: comparing hashes is enough to know whether data changed at all, without");
            Console.WriteLine("ever needing to see the original data both times.");
            Console.WriteLine($"{Environment.NewLine}Worth being precise about the distinction from encryption: there is no \"unhash\"");
            Console.WriteLine("operation, and hashing a plain password directly (as shown here, for illustration)");
            Console.WriteLine("is NOT how real systems should store passwords, see");
            Console.WriteLine("CSharp.Ch12.Supplemental.02.PasswordHashingDoneRight for why, and what to do instead.");
        }

        // Certificates: creating a genuine, real, self-signed X509 certificate at runtime,
        //   then inspecting it the same way you'd inspect one loaded from a file or store
        private static void CreatingAndInspectingACertificate()
        {
            using var rsa = RSA.Create(2048);

            var request = new CertificateRequest(
                "CN=CSharp.Ch12.Demo, O=DataBank IMX Training",
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            // A self-signed certificate is its own issuer, worth contrasting against a
            //   real-world certificate, which is instead signed by a separate, trusted
            //   Certificate Authority (CA), the whole reason a browser trusts a website's
            //   certificate is that a CA it already trusts vouched for it, a self-signed
            //   certificate has no such third party backing it.
            using X509Certificate2 certificate = request.CreateSelfSigned(
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow.AddYears(1));

            Console.WriteLine($"Subject: {certificate.Subject}");
            Console.WriteLine($"Issuer: {certificate.Issuer} (self-signed, so this matches Subject)");
            Console.WriteLine($"Thumbprint: {certificate.Thumbprint}");
            Console.WriteLine($"Valid from {certificate.NotBefore:d} to {certificate.NotAfter:d}");
            Console.WriteLine($"Has a private key available: {certificate.HasPrivateKey}");
        }
        #endregion

        #region Managing Assemblies
        // Assembly versions: every .NET assembly carries a version number, worth reading
        //   directly off the currently-running assembly
        private static void InspectingAssemblyVersions()
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = currentAssembly.GetName();

            Console.WriteLine($"Assembly name: {assemblyName.Name}");
            Console.WriteLine($"Version: {assemblyName.Version}");
            Console.WriteLine($"Full name: {currentAssembly.FullName}");
            Console.WriteLine($"{Environment.NewLine}The four-part version number (Major.Minor.Build.Revision) is a .NET convention,");
            Console.WriteLine("not a language requirement, teams commonly follow semantic versioning ideas");
            Console.WriteLine("within it: Major for breaking changes, Minor for new-but-compatible features,");
            Console.WriteLine("Build/Revision for patches, though .NET itself doesn't enforce that meaning.");
        }

        // Strong naming: signing an assembly with a private key, giving it a unique,
        //   verifiable identity beyond just its file name
        private static void UnderstandingStrongNaming()
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            byte[] publicKeyToken = currentAssembly.GetName().GetPublicKeyToken();

            bool isStrongNamed = publicKeyToken != null && publicKeyToken.Length > 0;

            Console.WriteLine($"Is this assembly strong-named: {isStrongNamed}");
            Console.WriteLine($"{Environment.NewLine}This project isn't strong-named (most application projects aren't, only shared");
            Console.WriteLine("library assemblies that specifically need it usually are), so the answer above is");
            Console.WriteLine("expected to be \"False\". A strong-named assembly is signed with a private key");
            Console.WriteLine("(traditionally an .snk file), giving it a unique identity: its simple name, version,");
            Console.WriteLine("culture, and a public key token ALL together, rather than just a file name that");
            Console.WriteLine("could collide with something else entirely. This is what makes side-by-side");
            Console.WriteLine("versioning (two different versions of the SAME-NAMED assembly, installed and");
            Console.WriteLine("loadable at once) and safe placement in the GAC possible at all.");
        }

        // The Global Assembly Cache: a machine-wide store for shared assemblies
        private static void UnderstandingTheGac()
        {
            Console.WriteLine("The GAC (Global Assembly Cache) is a machine-wide store for assemblies meant to");
            Console.WriteLine("be shared across multiple applications on the same machine, rather than each");
            Console.WriteLine("application shipping and loading its own private copy.");
            Console.WriteLine();
            Console.WriteLine("Only STRONG-NAMED assemblies can go in the GAC (see UnderstandingStrongNaming()");
            Console.WriteLine("above), specifically because the GAC needs to be able to tell apart multiple");
            Console.WriteLine("DIFFERENT versions of an assembly with the SAME simple name, exactly what a");
            Console.WriteLine("strong name's full identity (name + version + culture + public key token)");
            Console.WriteLine("makes possible, this is \"side-by-side versioning\" in practice: version 1.0 and");
            Console.WriteLine("version 2.0 of the same-named library can both live in the GAC simultaneously,");
            Console.WriteLine("and each application loads whichever one it was actually built against.");
            Console.WriteLine();
            Console.WriteLine("Worth knowing this is much less common in modern .NET than it was in classic");
            Console.WriteLine(".NET Framework: NuGet-based, per-application dependency management (each app");
            Console.WriteLine("gets its own copy of exactly the packages/versions it needs, no machine-wide");
            Console.WriteLine("shared state to worry about) has largely superseded the GAC for new development,");
            Console.WriteLine("though it's still genuinely relevant for classic .NET Framework applications like");
            Console.WriteLine("the ones in this training set, and for certain framework-level assemblies.");
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
