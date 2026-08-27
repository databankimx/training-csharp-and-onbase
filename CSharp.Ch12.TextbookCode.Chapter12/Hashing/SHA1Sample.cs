/*
 * Warning!
 *
 * This is the unedited code downloaded from the Textbook publisher
 * This code does not follow coding standards or best practices and may have bugs/errors
 *
 * Downloaded From:
 * https://media.wiley.com/product_ancillary/94/11186120/DOWNLOAD/MCSD%20Certification%20Code%20and%20Test%20Questions.zip
 *
 * *Migration Note: the FILE is named "SHA1Sample.cs" and the class is named generically
 * "SHASample", but the code actually calls SHA256.Create(), not SHA1. See
 * LectureNotes.md, not a functional bug (SHA-256 is the stronger, more correct choice
 * over SHA-1, which is cryptographically broken), just a naming inconsistency left
 * exactly as downloaded.
 */

using System;
using System.Security.Cryptography;
using System.Text;

namespace EncryptionSamples.Hashing {
    public class SHASample {
        static string ComputeHash(string input) {
            HashAlgorithm sha = SHA256.Create();
            byte[] hashData = sha.ComputeHash(Encoding.Default.GetBytes(input));
            return Convert.ToBase64String(hashData);
        }
        static bool VerifyHash(string input, string hashValue) {

            HashAlgorithm sha = SHA256.Create();
            byte[] hashData = sha.ComputeHash(Encoding.Default.GetBytes(input));
            return Convert.ToBase64String(hashData) == hashValue;
        }


        public static void Run() {

            string input = "Data to be hashed!";
            string hash = ComputeHash(input);
            bool sameHash = VerifyHash(input, hash);

            Console.WriteLine("Hashing");
            Console.WriteLine("Input:{0}", input);
            Console.WriteLine("Hash: {0}", hash);
            Console.WriteLine("Same hash: {0}", sameHash);
            Console.WriteLine();
        }

    }
}
