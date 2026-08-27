/*
 * Warning!
 *
 * This is the unedited code downloaded from the Textbook publisher
 * This code does not follow coding standards or best practices and may have bugs/errors
 *
 * Downloaded From:
 * https://media.wiley.com/product_ancillary/94/11186120/DOWNLOAD/MCSD%20Certification%20Code%20and%20Test%20Questions.zip
 */

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EncryptionSamples.Symmetric
{
    public class TripleDESSample
    {
        private const int Iterations = 100;
            
        private static byte[] Encrypt(string value, string password, string salt)
        {
            DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt), Iterations);

            SymmetricAlgorithm algorithm = new TripleDESCryptoServiceProvider();

            byte[] rgbKey = rgb.GetBytes(algorithm.KeySize / 8);
            byte[] rgbIV = rgb.GetBytes(algorithm.BlockSize / 8);

            ICryptoTransform transform = algorithm.CreateEncryptor(rgbKey, rgbIV);

            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
                {
                    using (var writer = new StreamWriter(cryptoStream, Encoding.Unicode))
                    {
                        writer.Write(value);
                    }
                }

                return memoryStream.ToArray();
            }
        }


        private static string Decrypt(byte[] encrypted, string password, string salt)          
        {
            DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt), Iterations);

            SymmetricAlgorithm algorithm = new TripleDESCryptoServiceProvider();

            byte[] rgbKey = rgb.GetBytes(algorithm.KeySize / 8);
            byte[] rgbIV = rgb.GetBytes(algorithm.BlockSize / 8);

            ICryptoTransform transform = algorithm.CreateDecryptor(rgbKey, rgbIV);

            using (var memoryStream = new MemoryStream(encrypted))
            {
                using (var cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read))
                {
                    using (var reader = new StreamReader(cryptoStream, Encoding.Unicode))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        public static void Run() {
            string input = "Data to be TrippleDES Encrypted!";
            var encrypted = Encrypt(input, "pass", "salt");
            var decrypted = Decrypt(encrypted, "pass", "salt");

            // *Fixed*: this originally printed "Symmetric AesManaged", copy-pasted from
            //   AesManagedSample.cs and never updated to describe THIS sample.
            Console.WriteLine("Symmetric TripleDES");
            Console.WriteLine("Input:{0}", input);
            Console.WriteLine("Encrypted:{0}", Convert.ToBase64String(encrypted));
            Console.WriteLine("Decrypted:{0}", decrypted);
            Console.WriteLine();
        }

    }
}
