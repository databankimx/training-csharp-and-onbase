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
 * Three real bugs were found and fixed here during the 2026 migration, see
 * LectureNotes.md for the full detail on each:
 *   - TripleDESSample.Run() printed "Symmetric AesManaged" (copy-pasted from
 *     AesManagedSample.cs and never updated)
 *   - RSASample.Run() persisted an RSA key container to the Windows CSP key store and
 *     never cleaned it up, leaving real, permanent state on whoever's machine ran this
 *   - ProtectDataSample.Run() printed a raw byte[] with Console.WriteLine() directly,
 *     which just prints "System.Byte[]", not the actual encrypted bytes
 */

using EncryptionSamples.Asymmetric;
using EncryptionSamples.Hashing;
using EncryptionSamples.ProtectingData;
using EncryptionSamples.Symmetric;

namespace EncryptionSamples
{
    class Program
    {
        static void Main(string[] args)
        {
            TripleDESSample.Run();
            AesManagedSample.Run();
            RSASample.Run();
            SHASample.Run();
            ProtectDataSample.Run();

        }
    }
}
