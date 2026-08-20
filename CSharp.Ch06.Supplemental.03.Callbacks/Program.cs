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
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

#region Compiler Warning Suppression
#pragma warning disable 1998
#endregion

namespace CSharp.Ch06.Supplemental._03.Callbacks
{
    // Default class for console executable
    internal static class Program
    {
        #region Chapter Notes
        /*
         * A callback is simply a method that is passed as an argument
         * Based on your previous topics in this chapter, that limits callback methods to delegates,
         *   since a delegate is a pointer to a method, and that pointer can be passed to a function.
         *
         * The callback will be executed within the receiving function, typically (but not necessarily)
         *   at the end of other processing in the receiving method's code block.
         *
         * The most common use of a callback is in asynchronous processing to indicate when the
         *   processing is completed. For example, you might raise an even that reloads a status indicator
         *   after an asynchronous process completes.
         *
         * Code Considerations:
         *   https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/events-and-callbacks
         *   ✓ CONSIDER using callbacks to allow users to provide custom code to be executed by the framework
         *   ✓ CONSIDER using events to allow users to customize the behavior of a framework without the need for
         *      understanding object-oriented design
         *   ✓ DO prefer events over plain callbacks, because they are more familiar to a broader range of
         *      developers and are integrated with Visual Studio statement completion
         *   X  AVOID using callbacks in performance-sensitive APIs
         *   ✓ DO use the new Func<...>, Action<...>, or Expression<...> types instead of custom delegates,
         *      when defining APIs with callbacks
         *      - Func<...> and Action<...> represent generic delegates. Expression<...> represents function
         *        definitions that can be compiled and subsequently invoked at runtime but can also be serialized
         *        and passed to remote processes.
         *   ✓ DO measure and understand performance implications of using Expression<...>, instead of using
         *      Func<...> and Action<...> delegates.
         *      - Expression<...> types are in most cases logically equivalent to Func<...> and Action<...> delegates.
         *        The main difference between them is that the delegates are intended to be used in local process
         *        scenarios; expressions are intended for cases where it’s beneficial and possible to evaluate the
         *        expression in a remote process or machine.
         *   ✓ DO understand that by calling a delegate, you are executing arbitrary code, and that could have security,
         *      correctness, and compatibility repercussions.
         */
        #endregion

        #region Constants
        // Name of the solution file, used to locate a stable, portable search target
        // (the CSharp.Ch06.DelegatesEventsAndExceptions project folder) regardless of
        // which machine or directory this solution happens to be checked out to.
        private const string SolutionFileName = "DataBank.DeveloperTraining.sln";
        #endregion

        #region Private Members
        // list of files discovered by search function
        private static readonly List<string> Files = new List<string>();
        #endregion

        #region Main Executable Method
        // Presence of Main() method renders the class runnable
        private static void Main()
        {
            try
            {
                #region Chapter Lesson
                StartSearch();
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

        #region Callback Methods
        // Triggers the Search method with function callbacks
        private static async void StartSearch()
        {
            string searchPath = Path.Combine(FindSolutionRoot(), "CSharp.Ch06.DelegatesEventsAndExceptions");
            await Search(".cs", searchPath, Callback, Callback2);
        }

        // Performs the actual search process
        private static async Task Search(string searchTerm, string directory, Action<int> callback, Action callback2)
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException($"Failed to locate directory [{directory}]!");

            int matchedFiles = 0;

            foreach (string path in Directory.GetFiles(directory))
            {
                if (!path.Contains(searchTerm)) continue;

                matchedFiles++;
                callback(matchedFiles);
                Files.Add(Path.GetFileName(path));
            }

            callback2();
        }

        // First callback function
        private static void Callback(int count)
        {
            Console.Clear();
            Console.WriteLine($"Found {count} files...");
            Thread.Sleep(1000);
        }

        // Second callback function
        private static void Callback2()
        {
            if (Files == null || Files.Count == 0)
            {
                Console.WriteLine("No files found!");
                return;
            }

            Console.WriteLine($"{Environment.NewLine}Files:");

            foreach (string name in Files)
            {
                Console.WriteLine($"{Environment.NewLine}{name}");
            }
        }
        #endregion

        #region Solution Discovery
        // Walks up from wherever this executable actually is, looking for the .sln
        // file, so the search target resolves correctly no matter which machine or
        // directory this solution is checked out to. Originally this project used a
        // hardcoded absolute path (D:\FileStore\...) tied to one specific development
        // machine, which broke on any other machine, this replaces that with the same
        // solution-root-discovery technique LessonRunner uses.
        private static string FindSolutionRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);

            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, SolutionFileName)))
                {
                    return directory.FullName;
                }
                directory = directory.Parent;
            }

            throw new DatabankException($"Could not locate {SolutionFileName} above {AppContext.BaseDirectory}");
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
