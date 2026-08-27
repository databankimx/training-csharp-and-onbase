/*
 * Warning!
 *
 * This is the unedited code downloaded from the Textbook publisher
 * This code does not follow coding standards or best practices and may have bugs/errors
 *
 * Downloaded From:
 * https://media.wiley.com/product_ancillary/94/11186120/DOWNLOAD/MCSD%20Certification%20Code%20and%20Test%20Questions.zip
 *
 * *Migration Note: the original Run() passed the raw "encrypted" byte[] directly to
 * Console.WriteLine("Protected: {0}", encrypted), which just prints the array's
 * ToString() representation ("System.Byte[]"), not anything about the actual encrypted
 * bytes, unlike every OTHER sample in this project, which correctly Base64-encodes its
 * byte[] output first. See LectureNotes.md.
 */

using System;
using System.Security.Cryptography;
using System.Text;

namespace EncryptionSamples.ProtectingData {
    class ProtectDataSample {

        static byte[] ProtectString(string data) {

            byte[] userData = Encoding.Default.GetBytes(data);
            byte[] encryptedData = ProtectedData.Protect(userData, null,
                                            DataProtectionScope.LocalMachine);
            return encryptedData;
        }

        static string UnprotectString(byte[] encryptedData) {

            byte[] userData = ProtectedData.Unprotect(encryptedData, null,
                                            DataProtectionScope.LocalMachine);
            string data = Encoding.Default.GetString(userData);
            return data;
        }

        public static void Run() {
            string input = "Data to be Protected!";
            
            var encrypted = ProtectString(input);
            var unprotected = UnprotectString(encrypted);

            Console.WriteLine("Using ProtectedData");
            Console.WriteLine("Input:{0}", input);
            // *Fixed*: originally passed the raw byte[] directly, printing "System.Byte[]"
            //   instead of anything meaningful. Base64-encoded, matching every other
            //   sample in this project.
            Console.WriteLine("Protected: {0}", Convert.ToBase64String(encrypted));
            Console.WriteLine("Unprotected: {0}", unprotected);
            Console.WriteLine();
        }

    }
}
