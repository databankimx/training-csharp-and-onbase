/*
 * Warning!
 *
 * This is the unedited code downloaded from the Textbook publisher
 * This code does not follow coding standards or best practices and may have bugs/errors
 *
 * Downloaded From:
 * https://media.wiley.com/product_ancillary/94/11186120/DOWNLOAD/MCSD%20Certification%20Code%20and%20Test%20Questions.zip
 *
 * *Migration Note:
 *
 * A cleanup step was added at the end of Run(), see LectureNotes.md for the full
 * explanation: rsa.PersistKeyInCsp = true (below) writes a real, permanent key
 * container to the Windows CSP key store, one that survives this process exiting and
 * was never removed by the original download. Left as originally downloaded, running
 * this sample would leave real cryptographic key material sitting on whoever's machine
 * ran it, indefinitely, growing by one container every run. The added
 * CleanupPersistedKeyContainer() call removes it again before Run() returns.
 */

using System;
using System.Security.Cryptography;
using System.Text;

namespace EncryptionSamples.Asymmetric
{
    public class RSASample
    {
        public static void Run() {

            string keyContainerName = "MyKeyContainer";
            string clearText = "This is the data we want to encrypt!";
            var cspParams = new CspParameters();
            cspParams.KeyContainerName = keyContainerName;

            RSAParameters publicKey;
            RSAParameters privateKey;

            using (var rsa = new RSACryptoServiceProvider(cspParams)) {

                rsa.PersistKeyInCsp = true;
                publicKey = rsa.ExportParameters(false);
                privateKey = rsa.ExportParameters(true);

                rsa.Clear();
            }

            byte[] encrypted = EncryptUsingRSAParam(clearText, publicKey);
            string decrypted = DecryptUsingRSAParam(encrypted, privateKey);

            Console.WriteLine("Asymmetric RSA");
            Console.WriteLine("Asymmetric RSA - Using RSA Params");
            Console.WriteLine("Encrypted:{0}", Convert.ToBase64String(encrypted));
            Console.WriteLine("Decrypted:{0}", decrypted);
            Console.WriteLine();

            Console.WriteLine("Asymmetric RSA - Using Persistent Key Container");
            encrypted = EncryptUsingContainer(clearText, keyContainerName);
            decrypted = DecryptUsingContainer(encrypted, keyContainerName);
            Console.WriteLine("Encrypted:{0}", Convert.ToBase64String(encrypted));
            Console.WriteLine("Decrypted:{0}", decrypted);
            Console.WriteLine();

            // *Added*: remove the persisted key container this Run() created above,
            //   see the migration note at the top of this file for why this matters.
            CleanupPersistedKeyContainer(keyContainerName);
        }

        static byte[] EncryptUsingRSAParam(string value, RSAParameters rsaKeyInfo)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(rsaKeyInfo);
                byte[] encodedData = Encoding.Default.GetBytes(value);
                byte[] encryptedData = rsa.Encrypt(encodedData, true);

                rsa.Clear();
                return encryptedData;
            }
        }

        static string DecryptUsingRSAParam(byte[] encryptedData, RSAParameters rsaKeyInfo) {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(rsaKeyInfo);
                byte[] decryptedData = rsa.Decrypt(encryptedData, true);
                string decryptedValue = Encoding.Default.GetString(decryptedData);

                rsa.Clear();
                return decryptedValue;
            }
        }

        static byte[] EncryptUsingContainer(string value, string containerName)
        {
            var cspParams = new CspParameters();
            cspParams.KeyContainerName = containerName;
            using (var rsa = new RSACryptoServiceProvider(cspParams))
            {
                byte[] encodedData = System.Text.Encoding.Default.GetBytes(value);
                byte[] encryptedData = rsa.Encrypt(encodedData, true);

                rsa.Clear();
                return encryptedData;
            }
        }

        static string DecryptUsingContainer(byte[] encryptedData, string containerName)
        {
            var cspParams = new CspParameters();
            cspParams.KeyContainerName = containerName;
            using (var rsa = new RSACryptoServiceProvider(cspParams))
            {
                byte[] decryptedData = rsa.Decrypt(encryptedData, true);
                string decryptedValue = Encoding.Default.GetString(decryptedData);

                rsa.Clear();
                return decryptedValue;
            }
        }

        // Removes a persisted CSP key container: setting PersistKeyInCsp = false BEFORE
        //   Clear()/disposal tells the CSP to actually delete the container from the
        //   Windows key store, rather than leaving it behind indefinitely.
        static void CleanupPersistedKeyContainer(string containerName)
        {
            var cspParams = new CspParameters();
            cspParams.KeyContainerName = containerName;
            using (var rsa = new RSACryptoServiceProvider(cspParams))
            {
                rsa.PersistKeyInCsp = false;
                rsa.Clear();
            }
        }
    }
}
