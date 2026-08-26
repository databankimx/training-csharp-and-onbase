
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
using System.Security.Cryptography;
using System.Text;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch12.Supplemental._01.DigitalSignaturesDeepDive
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * The main lesson used RSA for ENCRYPTION, hiding a message's contents. This
         *   Supplemental covers RSA's other major job: SIGNING, proving a message really
         *   came from the holder of a specific private key, and that it wasn't altered
         *   after signing. The two use RSA's key pair in genuinely OPPOSITE roles, worth
         *   getting straight, see SigningKeysAreOppositeOfEncryptingKeys() below.
         *
         * All signature examples here use RSASignaturePadding.Pkcs1 specifically, the
         *   universally-supported padding scheme. RSASignaturePadding.Pss is the more
         *   modern choice where available, but support for it varies more across
         *   providers/platforms than Pkcs1 does, worth checking directly on whatever
         *   platform you're targeting before committing to it in real code.
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                SigningAndVerifyingData();
                GenericFunctions.Pause();

                SigningKeysAreOppositeOfEncryptingKeys();
                GenericFunctions.Pause();

                DetectingTamperedData();
                GenericFunctions.Pause();

                DetectingTamperedSignature();
                GenericFunctions.Pause();

                UsingHmacAsASymmetricAlternative();
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
        // A basic sign/verify round trip
        private static void SigningAndVerifyingData()
        {
#pragma warning disable S1192
            const string message = "Transfer $500 to account 12345.";
#pragma warning restore S1192
            byte[] data = Encoding.UTF8.GetBytes(message);

            // The SENDER signs with their OWN private key
            using var sender = RSA.Create();
            byte[] signature = sender.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            // Anyone holding the SENDER's PUBLIC key can verify it, they don't need
            //   (and should never have) the sender's private key to do this.
            RSAParameters senderPublicKey = sender.ExportParameters(includePrivateParameters: false);
            using var verifier = RSA.Create();
            verifier.ImportParameters(senderPublicKey);

            bool isValid = verifier.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            Console.WriteLine($"Message: \"{message}\"");
            Console.WriteLine($"Signature (Base64): {Convert.ToBase64String(signature)}");
            Console.WriteLine($"Signature verified: {isValid}");
        }

        // The genuinely important, easily-confused distinction: signing and encrypting
        //   use RSA's key pair in OPPOSITE roles
        private static void SigningKeysAreOppositeOfEncryptingKeys()
        {
            Console.WriteLine("ENCRYPTING (hiding a message's contents, see the main lesson):");
            Console.WriteLine("  1. Encrypt with the RECIPIENT's PUBLIC key");
            Console.WriteLine("  2. Only the RECIPIENT's PRIVATE key can decrypt it");
            Console.WriteLine("  -> Purpose: keep the message secret from everyone except the recipient");
            Console.WriteLine();
            Console.WriteLine("SIGNING (proving who sent a message, and that it wasn't altered):");
            Console.WriteLine("  1. Sign with the SENDER's PRIVATE key");
            Console.WriteLine("  2. Anyone with the SENDER's PUBLIC key can verify it");
            Console.WriteLine("  -> Purpose: prove authorship and integrity, NOT secrecy, a signed message");
            Console.WriteLine("     is still perfectly readable by anyone, signing doesn't hide anything");
            Console.WriteLine();
            Console.WriteLine("Worth internalizing directly: \"public key encrypts, private key decrypts\" is only");
            Console.WriteLine("true for ENCRYPTION. For SIGNING it's flipped: \"private key signs, public key");
            Console.WriteLine("verifies\". Same key PAIR, opposite roles depending on which operation you're doing.");
        }

        // Tampering with the DATA after signing invalidates the signature
        private static void DetectingTamperedData()
        {
            byte[] originalData = Encoding.UTF8.GetBytes("Transfer $500 to account 12345.");
            byte[] tamperedData = Encoding.UTF8.GetBytes("Transfer $500 to account 99999.");

            using var rsa = RSA.Create();
            byte[] signature = rsa.SignData(originalData, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            bool originalIsValid = rsa.VerifyData(originalData, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            bool tamperedIsValid = rsa.VerifyData(tamperedData, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            Console.WriteLine($"Original data + original signature: valid = {originalIsValid}");
            Console.WriteLine($"Tampered data + original signature: valid = {tamperedIsValid}");
            Console.WriteLine($"{Environment.NewLine}The signature was computed over the ORIGINAL data, changing the account number by");
            Console.WriteLine("even one digit produces completely different underlying hash data, the signature");
            Console.WriteLine("no longer matches, and verification correctly fails.");
        }

        // Tampering with the SIGNATURE itself (leaving the data untouched) also fails
        private static void DetectingTamperedSignature()
        {
            byte[] data = Encoding.UTF8.GetBytes("Transfer $500 to account 12345.");

            using var rsa = RSA.Create();
            byte[] signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            // Flip one bit in the signature itself, simulating a corrupted or forged signature
            byte[] tamperedSignature = (byte[])signature.Clone();
            tamperedSignature[0] ^= 0xFF;

            bool isValid = rsa.VerifyData(data, tamperedSignature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            Console.WriteLine($"Original data + tampered signature: valid = {isValid}");
            Console.WriteLine($"{Environment.NewLine}Either side of a sign/verify pair being altered, the DATA or the SIGNATURE itself,");
            Console.WriteLine("breaks verification. Both need to arrive exactly as they were when signed.");
        }

        // HMAC: a faster, symmetric alternative to RSA signatures, when both parties
        //   ALREADY share a secret key
        private static void UsingHmacAsASymmetricAlternative()
        {
            // RandomNumberGenerator.GetBytes(int) is a .NET 6+ static convenience method,
            //   not available on net48, using RandomNumberGenerator.Create() + GetBytes()
            //   is the net48-compatible way to generate cryptographically random bytes.
            byte[] sharedSecretKey = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(sharedSecretKey);
            }
            byte[] data = Encoding.UTF8.GetBytes("Transfer $500 to account 12345.");

            using var hmac = new HMACSHA256(sharedSecretKey);
            byte[] mac = hmac.ComputeHash(data);

            // The SAME shared key both computes and verifies, unlike RSA's asymmetric
            //   sign-with-private/verify-with-public split
            using var verifyingHmac = new HMACSHA256(sharedSecretKey);
            byte[] recomputedMac = verifyingHmac.ComputeHash(data);
            // byte[].SequenceEqual() (System.Linq), NOT Span<byte>.SequenceEqual(): Span<T>
            //   requires the System.Memory NuGet package on net48 (it's not built in the
            //   way it is on modern .NET), System.Linq's array comparison needs nothing extra.
            bool isValid = mac.SequenceEqual(recomputedMac);

            Console.WriteLine($"HMAC (Base64): {Convert.ToBase64String(mac)}");
            Console.WriteLine($"HMAC verified: {isValid}");
            Console.WriteLine($"{Environment.NewLine}HMAC is much faster than an RSA signature, and needs no key pair or certificate");
            Console.WriteLine("infrastructure at all. The tradeoff: it needs a shared secret key BOTH parties");
            Console.WriteLine("already possess, the same key distribution problem symmetric encryption has (see");
            Console.WriteLine("the main lesson). RSA signatures avoid that problem entirely, verifying only ever");
            Console.WriteLine("needs the sender's PUBLIC key, never a shared secret. Reach for HMAC when both");
            Console.WriteLine("sides already share a key (an internal service-to-service API, for instance),");
            Console.WriteLine("reach for RSA signatures when the verifier and signer are genuine strangers.");
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
