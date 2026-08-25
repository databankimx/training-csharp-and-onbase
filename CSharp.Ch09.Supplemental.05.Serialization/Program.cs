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
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using CSharp.Ch09.Supplemental._05.Serialization.Models;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
using Newtonsoft.Json;
#endregion

namespace CSharp.Ch09.Supplemental._05.Serialization
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * "Understanding Serialization" is the final section of Chapter 9. Serialization is
         *   converting an object's state into a storable/transmittable form (bytes, XML text,
         *   JSON text), and deserialization is the reverse, reconstructing an object from that
         *   form. Everything here runs against a temporary working directory this project
         *   creates on startup and deletes on exit.
         *
         * !! WARNING !!
         * BinaryFormatter (used below for Binary and Custom Serialization) has well-documented
         *   security problems: deserializing binary data from an UNTRUSTED source with it can
         *   let an attacker execute arbitrary code, simply by handing your program a
         *   maliciously crafted byte stream to deserialize. Microsoft's own guidance is to
         *   avoid it in new code. It's covered here because it's part of this chapter's
         *   official curriculum and still works in classic .NET Framework, but a real
         *   application should prefer XML or JSON serialization (both covered below too),
         *   which don't carry this same risk, especially for anything crossing a trust
         *   boundary (a file from a user, data from a network request, etc.).
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            string workingDirectory = Path.Combine(Path.GetTempPath(), $"ch09-serialization-demo-{Guid.NewGuid():N}");

            try
            {
                Directory.CreateDirectory(workingDirectory);

                #region Chapter Lessons
                // Binary Serialization
                UsingBinarySerialization(workingDirectory);
                GenericFunctions.Pause();

                // XML Serialization
                UsingXmlSerialization(workingDirectory);
                GenericFunctions.Pause();

                // JSON Serialization
                UsingJsonSerialization(workingDirectory);
                GenericFunctions.Pause();

                // Custom Serialization
                UsingCustomSerialization(workingDirectory);
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
                if (Directory.Exists(workingDirectory)) Directory.Delete(workingDirectory, recursive: true);
                GenericFunctions.Pause(final: true);
            }
        }
        #endregion

        #region Lesson Methods
        // Demonstrate Binary Serialization
        private static void UsingBinarySerialization(string workingDirectory)
        {
            string filePath = Path.Combine(workingDirectory, "book.bin");
            var book = new Book("1984", "George Orwell", 1949);

            var formatter = new BinaryFormatter();

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                formatter.Serialize(stream, book);
            }
            Console.WriteLine($"BinaryFormatter wrote {new FileInfo(filePath).Length} bytes to {Path.GetFileName(filePath)}");

            Book restoredBook;
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                restoredBook = (Book)formatter.Deserialize(stream);
            }
            Console.WriteLine($"Deserialized: {restoredBook}");
        }

        // Demonstrate XML Serialization
        private static void UsingXmlSerialization(string workingDirectory)
        {
            string filePath = Path.Combine(workingDirectory, "book.xml");
            var book = new Book("Brave New World", "Aldous Huxley", 1932);

            // Note: XmlSerializer requires a PUBLIC parameterless constructor (Book has
            //   one) and only serializes PUBLIC read/write properties, it has no concept of
            //   [Serializable]/[NonSerialized] or ISerializable at all, those are specific
            //   to BinaryFormatter. XML customization uses a completely different mechanism
            //   (IXmlSerializable), not covered here for brevity.
            var serializer = new XmlSerializer(typeof(Book));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                serializer.Serialize(stream, book);
            }

            Console.WriteLine("Generated XML:");
            Console.WriteLine(File.ReadAllText(filePath));

            Book restoredBook;
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                restoredBook = (Book)serializer.Deserialize(stream);
            }
            Console.WriteLine($"Deserialized: {restoredBook}");
        }

        // Demonstrate JSON Serialization
        private static void UsingJsonSerialization(string workingDirectory)
        {
            string filePath = Path.Combine(workingDirectory, "book.json");
            var book = new Book("Fahrenheit 451", "Ray Bradbury", 1953);

            // Newtonsoft.Json (Json.NET), the long-established, widely-used JSON library for
            //   .NET, including classic .NET Framework. Serializes public read/write
            //   properties by default, similar in spirit to XmlSerializer.
            string json = JsonConvert.SerializeObject(book, Formatting.Indented);
            File.WriteAllText(filePath, json);

            Console.WriteLine("Generated JSON:");
            Console.WriteLine(json);

            var restoredBook = JsonConvert.DeserializeObject<Book>(json);
            Console.WriteLine($"Deserialized: {restoredBook}");
        }

        // Demonstrate Custom Serialization
        private static void UsingCustomSerialization(string workingDirectory)
        {
            string filePath = Path.Combine(workingDirectory, "custom-book.bin");
            var book = new Book("Dune", "Frank Herbert", 1965);

            // Reading Summary NOW, before serializing, populates and caches it on THIS
            //   instance, purely to make the point below visible: that cached value never
            //   gets written out at all.
            Console.WriteLine($"Original instance's Summary (already cached): {book.Summary}");

            var formatter = new BinaryFormatter();
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                formatter.Serialize(stream, book);
            }

            Book restoredBook;
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                restoredBook = (Book)formatter.Deserialize(stream);
            }

            // Book.GetObjectData() (see Models/Book.cs) deliberately never wrote cachedSummary
            //   out at all, only Title/Author/Year. Summary is recomputed fresh, lazily, the
            //   first time it's read on the newly-deserialized instance, not carried over as
            //   persisted state that could have gone stale.
            Console.WriteLine($"Restored instance's Title/Author/Year: {restoredBook.Title}, {restoredBook.Author}, {restoredBook.Year}");
            Console.WriteLine($"Restored instance's Summary (freshly recomputed, not restored): {restoredBook.Summary}");
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
