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
using System.Text;
using System.Threading.Tasks;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch09.Supplemental._04.FileIO
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * "Performing I/O Operations" is the third section of Chapter 9. Everything here works
         *   against real files in a temporary working directory this project creates on startup
         *   and deletes on exit, safe to run repeatedly with no leftover state.
         *
         * Layers, from lowest-level to highest:
         * - Files and Directories   File/Directory (static utility methods) and FileInfo/
         *                             DirectoryInfo (instance-based, useful when you need
         *                             several pieces of information about the same file/
         *                             directory without repeated lookups)
         * - Streams                 A Stream is a sequence of bytes, FileStream reads/writes a
         *                             file's raw bytes directly, MemoryStream does the same
         *                             against an in-memory byte buffer instead of a file
         * - Readers and Writers     Wrap a Stream to work with something more useful than raw
         *                             bytes: StreamReader/StreamWriter for text,
         *                             BinaryReader/BinaryWriter for typed binary data
         * - Asynchronous I/O        Every one of the above has an async counterpart
         *                             (ReadAsync/WriteAsync/ReadToEndAsync, etc.), since disk
         *                             I/O is exactly the kind of "waiting on something slow"
         *                             scenario async/await exists for (see Chapter 7's
         *                             coverage of async/await for the underlying mechanics)
         */
        #endregion

        #region Main Method
        private static async Task Main()
        {
            string workingDirectory = Path.Combine(Path.GetTempPath(), $"ch09-fileio-demo-{Guid.NewGuid():N}");

            try
            {
                Directory.CreateDirectory(workingDirectory);
                Console.WriteLine($"Working directory for this lesson: {workingDirectory}");
                GenericFunctions.Pause();

                #region Chapter Lessons
                // Files and Directories
                UsingFilesAndDirectories(workingDirectory);
                GenericFunctions.Pause();

                // Streams
                UsingStreams(workingDirectory);
                GenericFunctions.Pause();

                // Readers and Writers
                UsingReadersAndWriters(workingDirectory);
                GenericFunctions.Pause();

                // Asynchronous I/O Operations
                await UsingAsyncIo(workingDirectory);
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
        // Demonstrate Files and Directories
        private static void UsingFilesAndDirectories(string workingDirectory)
        {
            string filePath = Path.Combine(workingDirectory, "laws.txt");

            // File.WriteAllText() is the simplest possible way to create/overwrite a file
            //   with text content, one call, no manual stream management needed.
            File.WriteAllText(filePath, "Murphy's Law: Anything that can go wrong will go wrong.");
            Console.WriteLine($"File.Exists(\"{Path.GetFileName(filePath)}\"): {File.Exists(filePath)}");

            // FileInfo gives you an object you can query repeatedly (Length, LastWriteTime,
            //   etc.) without File's static methods re-touching the filesystem each time.
            var fileInfo = new FileInfo(filePath);
            Console.WriteLine($"FileInfo.Length: {fileInfo.Length} bytes");
            Console.WriteLine($"FileInfo.Extension: {fileInfo.Extension}");

            // Path's utility methods work purely on the string itself, no filesystem access at all
            Console.WriteLine($"{Environment.NewLine}Path.GetFileName: {Path.GetFileName(filePath)}");
            Console.WriteLine($"Path.GetDirectoryName: {Path.GetDirectoryName(filePath)}");
            Console.WriteLine($"Path.GetExtension: {Path.GetExtension(filePath)}");
            Console.WriteLine($"Path.ChangeExtension(..., \".md\"): {Path.ChangeExtension(filePath, ".md")}");

            string copyPath = Path.Combine(workingDirectory, "laws-copy.txt");
            File.Copy(filePath, copyPath);
            Console.WriteLine($"{Environment.NewLine}File.Copy() created: {Path.GetFileName(copyPath)}");

            string subDirectory = Path.Combine(workingDirectory, "archive");
            Directory.CreateDirectory(subDirectory);
            string movedPath = Path.Combine(subDirectory, "laws-copy.txt");
            File.Move(copyPath, movedPath);
            Console.WriteLine($"File.Move() relocated it into: {Path.GetFileName(subDirectory)}/");

            Console.WriteLine($"{Environment.NewLine}Directory.GetFiles(workingDirectory):");
            foreach (string file in Directory.GetFiles(workingDirectory))
            {
                Console.WriteLine($" - {Path.GetFileName(file)}");
            }

            Console.WriteLine($"{Environment.NewLine}Directory.GetDirectories(workingDirectory):");
            foreach (string dir in Directory.GetDirectories(workingDirectory))
            {
                Console.WriteLine($" - {Path.GetFileName(dir)}/");
            }
        }

        // Demonstrate Streams
        private static void UsingStreams(string workingDirectory)
        {
            string filePath = Path.Combine(workingDirectory, "stream-demo.bin");
            byte[] data = Encoding.UTF8.GetBytes("Streamed as raw bytes.");

            // FileStream reads/writes a file's raw bytes directly, no text encoding or typed
            //   values involved, just a sequence of bytes.
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                fileStream.Write(data, 0, data.Length);
            }
            Console.WriteLine($"FileStream wrote {data.Length} raw bytes to {Path.GetFileName(filePath)}");

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[fileStream.Length];
                int byteCount = fileStream.Read(buffer, 0, buffer.Length);
                Console.WriteLine($"FileStream read {byteCount} bytes back: \"{Encoding.UTF8.GetString(buffer)}\"");
            }

            // MemoryStream is the same idea, but the bytes live in memory instead of a file,
            //   useful when you need Stream-based APIs but don't actually want a file at all.
            using var memoryStream = new MemoryStream();
            memoryStream.Write(data, 0, data.Length);
            Console.WriteLine($"{Environment.NewLine}MemoryStream now holds {memoryStream.Length} bytes in memory, no file involved.");
        }

        // Demonstrate Readers and Writers
        private static void UsingReadersAndWriters(string workingDirectory)
        {
            string textPath = Path.Combine(workingDirectory, "reader-writer-demo.txt");

            // StreamWriter/StreamReader work with TEXT, handling character encoding for you,
            //   the same underlying idea as FileStream, but at the text level instead of raw bytes.
            using (var writer = new StreamWriter(textPath))
            {
                writer.WriteLine("Line one.");
                writer.WriteLine("Line two.");
            }

            using (var reader = new StreamReader(textPath))
            {
                Console.WriteLine("StreamReader, line by line:");
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Console.WriteLine($" - {line}");
                }
            }

            string binaryPath = Path.Combine(workingDirectory, "reader-writer-demo.bin");

            // BinaryWriter/BinaryReader work with TYPED values (int, double, bool, string...),
            //   writing them in their native binary representation, not as text.
            using (var writer = new BinaryWriter(File.Open(binaryPath, FileMode.Create)))
            {
                writer.Write(42);
                writer.Write(3.14);
                writer.Write("Murphy's Law");
            }

            using (var reader = new BinaryReader(File.Open(binaryPath, FileMode.Open)))
            {
                int intValue = reader.ReadInt32();
                double doubleValue = reader.ReadDouble();
                string stringValue = reader.ReadString();
                Console.WriteLine($"{Environment.NewLine}BinaryReader read back: {intValue}, {doubleValue}, \"{stringValue}\"");

                // Note: BinaryReader/Writer must read values back in the EXACT same order and
                //   type they were written in, there's no self-describing structure the way
                //   text formats (JSON, XML) provide, see Supplemental 05 for those.
            }
        }

        // Demonstrate Asynchronous I/O Operations
        private static async Task UsingAsyncIo(string workingDirectory)
        {
            string filePath = Path.Combine(workingDirectory, "async-demo.txt");

            // .NET Framework has no File.WriteAllTextAsync()/ReadAllTextAsync() at all, those
            //   were only added starting with .NET Core, so the equivalent here is a
            //   StreamWriter/StreamReader using their own WriteAsync()/ReadToEndAsync()
            //   methods, which HAVE been available since .NET 4.5.
            using (var writer = new StreamWriter(filePath))
            {
                await writer.WriteAsync("Written asynchronously.");
            }
            Console.WriteLine($"StreamWriter.WriteAsync() wrote to {Path.GetFileName(filePath)}");

            string content;
            using (var reader = new StreamReader(filePath))
            {
                content = await reader.ReadToEndAsync();
            }
            Console.WriteLine($"StreamReader.ReadToEndAsync() read back: \"{content}\"");

            // FileStream's own ReadAsync() for lower-level async access. Note this is a
            //   plain "using", not "await using": FileStream in .NET Framework only
            //   implements IDisposable, not IAsyncDisposable (also a .NET Core addition),
            //   so there's no async form of disposing it to await here.
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            {
                var buffer = new byte[fileStream.Length];
                int bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);
                string rawContent = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"{Environment.NewLine}FileStream.ReadAsync(): \"{rawContent}\"");
            }
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
