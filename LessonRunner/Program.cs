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
using System.Diagnostics;
using System.IO;
using System.Linq;
using LessonRunner.Models;
using CSharp.SharedLibrary.Models;
#endregion

namespace LessonRunner
{
    // Menu-driven launcher for the developer training solution. Chapter -> Lesson menu,
    // runs the chosen lesson as its own process, and returns to the lesson menu when
    // it exits so the next lesson in the chapter is one keypress away.
    internal static class Program
    {
        #region Constants
        // Name of the solution file, used to locate the solution root regardless of
        // whether this was launched via "dotnet run", F5 in Visual Studio, or by
        // double-clicking the built .exe directly.
        private const string SolutionFileName = "DataBank.DeveloperTraining.sln";
        #endregion

        #region Main Executable Method
        private static void Main()
        {
            try
            {
                string solutionRoot = FindSolutionRoot();
                var chapters = BuildCatalog();

                RunChapterMenu(chapters, solutionRoot);
            }
            catch (Exception ex)
            {
                new DatabankException("Error Caught!", ex).Log();
            }
            finally
            {
                if (!Debugger.IsAttached)
                {
                    Console.WriteLine("\nDone!\n\nPress any key to exit!");
                    Console.ReadKey();
                }
            }
        }
        #endregion

        #region Catalog
        // Chapters and lessons in logical teaching order. Add new chapters/lessons here
        // as the training solution grows, this is the only place that needs updating.
        private static List<Chapter> BuildCatalog()
        {
            return
            [
                new Chapter("Chapter 1 - Hello World",
                [
                    new Lesson("Hello World", "CSharp.Ch01.HelloWorld")
                ]),

                new Chapter("Chapter 2 - Basic Program Structure",
                [
                    new Lesson("Basic Program Structure (Full Lesson)", "CSharp.Ch02.BasicProgramStructure"),
                    new Lesson("Textbook Lab: Using If Statements", "CSharp.Ch02.TextbookCode.UsingIfStatements"),
                    new Lesson("Textbook Lab: Lottery Program", "CSharp.Ch02.TextbookCode.LotteryProgram"),
                    new Lesson("Textbook Lab: Average Grades", "CSharp.Ch02.TextbookCode.AverageGrades"),
                    new Lesson("Textbook Lab: Working with For Loops", "CSharp.Ch02.TextbookCode.WorkingWithForLoops")
                ]),

                new Chapter("Chapter 3 - Working with the Type System",
                [
                    new Lesson("Working with the Type System (Full Lesson)", "CSharp.Ch03.WorkingWithTheTypeSystem"),
                    new Lesson("Textbook Lab: Value Type Alias", "CSharp.Ch03.TextbookCode.ValueTypeAlias"),
                    new Lesson("Textbook Lab: Using Value Types", "CSharp.Ch03.TextbookCode.UsingValueTypes"),
                    new Lesson("Textbook Lab: Student Class", "CSharp.Ch03.TextbookCode.StudentClass"),
                    new Lesson("Textbook Lab: Student Class with Methods", "CSharp.Ch03.TextbookCode.StudentClassWithMethods"),
                    new Lesson("Textbook Lab: Value Type Passing", "CSharp.Ch03.TextbookCode.ValueTypePassing"),
                    new Lesson("Textbook Lab: Using Enums", "CSharp.Ch03.TextbookCode.UsingEnums"),
                    new Lesson("Textbook Lab: Using Properties", "CSharp.Ch03.TextbookCode.UsingProperties"),
                    new Lesson("Textbook Lab: Accessing Properties", "CSharp.Ch03.TextbookCode.AccessingProperties"),
                    new Lesson("Textbook Lab: Overloading Constructors", "CSharp.Ch03.TextbookCode.OverloadingConstructors")
                ]),

                new Chapter("Chapter 4 - Using and Converting Data Types",
                [
                    new Lesson("Using Types (Full Lesson)", "CSharp.Ch04.UsingTypes"),
                    new Lesson("Textbook Lab: Casting Arrays (WinForms, best run in the debugger)", "CSharp.Ch04.TextbookCode.CastingArrays"),
                    new Lesson("Textbook Lab: Order Entry Forms (WinForms, interactive)", "CSharp.Ch04.TextbookCode.Ch04RealWorldScenario01"),
                    new Lesson("Textbook Lab: Order Entry Forms with % Tax Rate (WinForms, interactive)", "CSharp.Ch04.TextbookCode.Ch04RealWorldScenario02"),
                    new Lesson("Textbook Lab: StringBuilder Staircase (WinForms)", "CSharp.Ch04.TextbookCode.Ch04RealWorldScenario03"),
                    new Lesson("Textbook Lab: Order Entry Forms with Currency Display (WinForms, interactive)", "CSharp.Ch04.TextbookCode.Ch04RealWorldScenario04"),
                    new Lesson("Textbook Lab: Clone Array (WinForms, best run in the debugger)", "CSharp.Ch04.TextbookCode.CloneArray"),
                    new Lesson("Textbook Lab: Excel Interop (WinForms, requires Excel installed)", "CSharp.Ch04.TextbookCode.ExcelInterop", requiresFullFrameworkMsBuild: true),
                    new Lesson("Textbook Lab: Permutations (WinForms, interactive)", "CSharp.Ch04.TextbookCode.Permutations"),
                    new Lesson("Textbook Lab: Short Path Names (WinForms, interactive)", "CSharp.Ch04.TextbookCode.ShortPathNames")
                ]),

                new Chapter("Chapter 5 - Implementing Class Hierarchies",
                [
                    new Lesson("Implementing Class Hierarchies (Full Lesson)", "CSharp.Ch05.ImplementingClassHierarchies"),
                    new Lesson("Supplemental: Configuration Classes", "CSharp.Ch05.Supplemental.ConfigurationClasses"),
                    new Lesson("Supplemental: Implementing Class Hierarchies", "CSharp.Ch05.Supplemental.ImplementingClassHierarchies"),
                    new Lesson("Supplemental: Shallow and Deep Cloning", "CSharp.Ch05.Supplemental.Cloning"),
                    new Lesson("Textbook Lab: Shape Resources (WinForms, best run in the debugger)", "CSharp.Ch05.TextbookCode.Ch05RealWorldScenario01"),
                    new Lesson("Textbook Lab: Shape Resources, Part 2 (WinForms, reference only)", "CSharp.Ch05.TextbookCode.Ch05RealWorldScenario02"),
                    new Lesson("Textbook Lab: Comparable Person (WinForms, interactive)", "CSharp.Ch05.TextbookCode.ComparablePerson"),
                    new Lesson("Textbook Lab: Ellipses and Circles (WinForms, best run in the debugger)", "CSharp.Ch05.TextbookCode.EllipsesAndCircles"),
                    new Lesson("Textbook Lab: ICloneable Person (WinForms, interactive)", "CSharp.Ch05.TextbookCode.ICloneablePerson"),
                    new Lesson("Textbook Lab: IComparable Cars (WinForms, interactive)", "CSharp.Ch05.TextbookCode.IComparableCars"),
                    new Lesson("Textbook Lab: IComparer Cars (WinForms, interactive)", "CSharp.Ch05.TextbookCode.IComparerCars"),
                    new Lesson("Textbook Lab: IDisposable Class (WinForms, interactive)", "CSharp.Ch05.TextbookCode.IDisposableClass"),
                    new Lesson("Textbook Lab: IEnumerable Tree (WinForms)", "CSharp.Ch05.TextbookCode.IEnumerableTree"),
                    new Lesson("Textbook Lab: IEquatable Person (WinForms, interactive)", "CSharp.Ch05.TextbookCode.IEquatablePerson"),
                    new Lesson("Textbook Lab: Person Hierarchy (WinForms, reference only)", "CSharp.Ch05.TextbookCode.PersonHierarchy"),
                    new Lesson("Textbook Lab: This and Base (WinForms, interactive)", "CSharp.Ch05.TextbookCode.ThisAndBase"),
                    new Lesson("Textbook Lab: Tree Enumerator (WinForms)", "CSharp.Ch05.TextbookCode.TreeEnumerator"),
                    new Lesson("Textbook Lab: University Classes (WinForms, reference only)", "CSharp.Ch05.TextbookCode.UniversityClasses")
                ]),

                new Chapter("Chapter 6 - Working with Delegates, Events, and Exceptions",
                [
                    new Lesson("Delegates, Events, and Exceptions (Full Lesson, WinForms)", "CSharp.Ch06.DelegatesEventsAndExceptions"),
                    new Lesson("Supplemental 01: Named Versus Anonymous Delegates", "CSharp.Ch06.Supplemental.01.NamedVersusAnonymousDelegates"),
                    new Lesson("Supplemental 02: Lambda Expressions", "CSharp.Ch06.Supplemental.02.LambdaExpressions"),
                    new Lesson("Supplemental 03: Callbacks", "CSharp.Ch06.Supplemental.03.Callbacks"),
                    new Lesson("Supplemental 04: Multicast Delegates", "CSharp.Ch06.Supplemental.04.MulticastDelegates"),
                    new Lesson("Supplemental 05: Exception Handling", "CSharp.Ch06.Supplemental.05.ExceptionHandling"),
                    new Lesson("Supplemental 06: Parameterized Thread Start", "CSharp.Ch06.Supplemental.06.ParameterizedThreadStart"),
                    new Lesson("Supplemental 07: Events", "CSharp.Ch06.Supplemental.07.Events"),
                    new Lesson("Supplemental 08: Assertions", "CSharp.Ch06.Supplemental.08.Assertions"),
                    new Lesson("Textbook Lab: Anonymous Graph (WinForms, interactive)", "CSharp.Ch06.TextbookCode.AnonymousGraph"),
                    new Lesson("Textbook Lab: Arithmetic Exceptions (WinForms, interactive)", "CSharp.Ch06.TextbookCode.ArithmeticExceptions"),
                    new Lesson("Textbook Lab: Async Lambdas (WinForms, interactive)", "CSharp.Ch06.TextbookCode.AsyncLambdas"),
                    new Lesson("Textbook Lab: Bank Account (WinForms, interactive)", "CSharp.Ch06.TextbookCode.BankAccount"),
                    new Lesson("Textbook Lab: Overdraft Account (WinForms, interactive)", "CSharp.Ch06.TextbookCode.Ch06RealWorldScenario01"),
                    new Lesson("Textbook Lab: Factorials (WinForms, interactive)", "CSharp.Ch06.TextbookCode.Ch06RealWorldScenario02"),
                    new Lesson("Textbook Lab: Covariance and Contravariance (WinForms, reference only)", "CSharp.Ch06.TextbookCode.CovarianceAndContravariance"),
                    new Lesson("Textbook Lab: Events (WinForms, interactive)", "CSharp.Ch06.TextbookCode.Events"),
                    new Lesson("Textbook Lab: Exception Handling / Finally (WinForms, interactive)", "CSharp.Ch06.TextbookCode.ExceptionHandling"),
                    new Lesson("Textbook Lab: Graph Function (WinForms, interactive)", "CSharp.Ch06.TextbookCode.GraphFunction"),
                    new Lesson("Textbook Lab: Money Market Account (WinForms, interactive)", "CSharp.Ch06.TextbookCode.MoneyMarketAccount"),
                    new Lesson("Textbook Lab: Static and Instance Delegates (WinForms, interactive)", "CSharp.Ch06.TextbookCode.StaticAndInstanceDelegates")
                ]),

                new Chapter("Chapter 7 - Multithreading and Asynchronous Processing",
                [
                    new Lesson("Multithreading and Asynchronous Processing (Full Lesson)", "CSharp.Ch07.MultithreadingAndAsynchronousProcessing"),
                    new Lesson("Supplemental 01: Thread Pool Example", "CSharp.Ch07.Supplemental.01.ThreadPoolExample"),
                    new Lesson("Supplemental 02: Unblocking the UI (WinForms, interactive)", "CSharp.Ch07.Supplemental.02.UnblockingTheUI"),
                    new Lesson("Supplemental 03: Task Parallel Library", "CSharp.Ch07.Supplemental.03.TaskParallelLibrary"),
                    new Lesson("Supplemental 04: Asynchronicity", "CSharp.Ch07.Supplemental.04.Asynchronicity"),
                    new Lesson("Supplemental 05: Race Conditions", "CSharp.Ch07.Supplemental.05.RaceConditions"),
                    new Lesson("Supplemental 06: Barriers", "CSharp.Ch07.Supplemental.06.Barriers"),
                    new Lesson("Supplemental 07: Locking", "CSharp.Ch07.Supplemental.07.Locking"),
                    new Lesson("Supplemental 08: Lock-Free Alternatives", "CSharp.Ch07.Supplemental.08.LockFreeAlternatives"),
                    new Lesson("Supplemental 09: Concurrent Collections", "CSharp.Ch07.Supplemental.09.ConcurrentCollections"),
                    new Lesson("Textbook Lab: Barrier Sample", "CSharp.Ch07.TextbookCode.BarrierSample"),
                    new Lesson("Textbook Lab: Barrier With Cancellation Sample", "CSharp.Ch07.TextbookCode.BarrierWithCancellationSample"),
                    new Lesson("Textbook Lab: Barrier With Tasks", "CSharp.Ch07.TextbookCode.BarrierWithTasks"),
                    new Lesson("Textbook Lab: Continuations App", "CSharp.Ch07.TextbookCode.ContinuationsApp"),
                    new Lesson("Textbook Lab: Locking", "CSharp.Ch07.TextbookCode.Locking"),
                    new Lesson("Textbook Lab: Method Syncronization", "CSharp.Ch07.TextbookCode.MethodSyncronization"),
                    new Lesson("Textbook Lab: Simple App", "CSharp.Ch07.TextbookCode.SimpleApp"),
                    new Lesson("Textbook Lab: TPL App", "CSharp.Ch07.TextbookCode.TPLApp"),
                    new Lesson("Textbook Lab: WinForm App (interactive)", "CSharp.Ch07.TextbookCode.WinFormApp"),
                    new Lesson("Textbook Lab: WPF App (interactive)", "CSharp.Ch07.TextbookCode.WpfApp"),
                    new Lesson("Textbook Lab: WPF Async App (interactive)", "CSharp.Ch07.TextbookCode.WPFAsyncApp")
                ]),

                new Chapter("Chapter 8 - Creating and Using Types with Reflection, Custom Attributes, the CodeDOM, and Lambda Expressions",
                [
                    new Lesson("Reflection, Custom Attributes, and the CodeDOM (Full Lesson)", "CSharp.Ch08.Reflection"),
                    new Lesson("Supplemental 01: Custom Attributes Deep Dive", "CSharp.Ch08.Supplemental.01.CustomAttributes"),
                    new Lesson("Supplemental 02: Dynamic Object Creation and Invocation", "CSharp.Ch08.Supplemental.02.DynamicInvocation"),
                    new Lesson("Supplemental 03: CodeDOM Compile and Run", "CSharp.Ch08.Supplemental.03.CodeDomCompileAndRun"),
                    new Lesson("Supplemental 04: Reflection Performance", "CSharp.Ch08.Supplemental.04.ReflectionPerformance"),
                    new Lesson("Textbook Lab: Chapter 8 (reference only, sixteen individual code blocks)", "CSharp.Ch08.TextbookCode.Chapter8")
                ]),

                new Chapter("Chapter 9 - Working with Data",
                [
                    new Lesson("Working with Data Collections (Full Lesson)", "CSharp.Ch09.WorkingWithDataCollections"),
                    new Lesson("Supplemental 01: ADO.NET and Entity Framework (requires SQL Server, see README.md)", "CSharp.Ch09.Supplemental.01.AdoNetAndEntityFramework"),
                    new Lesson("Supplemental 02: SQL Injection and Parameterized Queries (interactive, requires SQL Server)", "CSharp.Ch09.Supplemental.02.SqlInjection"),
                    new Lesson("Supplemental 03: Connecting to Other Databases (SQLite runs live, others reference only)", "CSharp.Ch09.Supplemental.03.ConnectingToOtherDatabases"),
                    new Lesson("Supplemental 04: File I/O", "CSharp.Ch09.Supplemental.04.FileIO"),
                    new Lesson("Supplemental 05: Serialization", "CSharp.Ch09.Supplemental.05.Serialization"),
                    new Lesson("Textbook Lab: Chapter 9 (reference only, per-file code samples)", "CSharp.Ch09.TextbookCode.Chapter9"),
                    new Lesson("Textbook Lab: File I/O Async (WinForms, interactive, needs C:\\Chapter9Samples and C:\\Test)", "CSharp.Ch09.TextbookCode.FileIOAsync"),
                    new Lesson("Textbook Lab: Serialization", "CSharp.Ch09.TextbookCode.Serialization"),
                    new Lesson("Textbook Lab: Northwinds Console (requires Northwinds DB, see README.md)", "CSharp.Ch09.TextbookCode.NorthwindsConsole"),
                    new Lesson("Textbook Lab: Northwinds WCF Data Service (Visual Studio only)", "CSharp.Ch09.TextbookCode.NorthwindsWCFDataService",
                        requiresVisualStudio: true,
                        visualStudioInstructions:
                            "This is an IIS-hosted WCF Data Service (a .svc file, no standalone .exe), it\n" +
                            "needs Visual Studio's own IIS Express integration to run, there's no \"dotnet\n" +
                            "run\" equivalent for that.\n\n" +
                            "1. Open CSharp.Ch09.TextbookCode.NorthwindsWCFDataService\\CSharp.Ch09.TextbookCode.NorthwindsWCFDataService.csproj in Visual Studio\n" +
                            "2. Make sure the \"Northwinds\" database is set up (see NorthwindsConsole's README.md)\n" +
                            "3. Press F5, or right-click NorthwindsService.svc and choose \"View in Browser\"\n\n" +
                            "See this project's own LectureNotes.md for further detail."),
                    new Lesson("Textbook Lab: Northwinds Client (requires the WCF Data Service running)", "CSharp.Ch09.TextbookCode.NorthwindsClient")
                ]),

                new Chapter("Chapter 10 - Working with Language Integrated Query (LINQ)",
                [
                    new Lesson("Working with LINQ (Full Lesson)", "CSharp.Ch10.WorkingWithLinq"),
                    new Lesson("Supplemental 01: Deferred Execution", "CSharp.Ch10.Supplemental.01.DeferredExecution"),
                    new Lesson("Supplemental 02: LINQ to XML Deep Dive", "CSharp.Ch10.Supplemental.02.LinqToXmlDeepDive"),
                    new Lesson("Supplemental 03: Custom LINQ Extension Methods", "CSharp.Ch10.Supplemental.03.CustomLinqExtensionMethods"),
                    new Lesson("Supplemental 04: IQueryable vs IEnumerable (requires ExternalData DB, see Ch09 README.md)", "CSharp.Ch10.Supplemental.04.IQueryableVsIEnumerable"),
                    new Lesson("Textbook Lab: LINQ Samples (reference only, per-method code samples)", "CSharp.Ch10.TextbookCode.LINQSamples")
                ]),

                new Chapter("Chapter 11 - Input Validation, Debugging, and Instrumentation",
                [
                    new Lesson("Input Validation, Debugging, and Instrumentation (Full Lesson)", "CSharp.Ch11.InputValidationDebuggingAndInstrumentation"),
                    new Lesson("Supplemental 01: Regular Expressions Deep Dive", "CSharp.Ch11.Supplemental.01.RegularExpressionsDeepDive"),
                    new Lesson("Supplemental 02: Preprocessor Directives Deep Dive", "CSharp.Ch11.Supplemental.02.PreprocessorDirectivesDeepDive"),
                    new Lesson("Supplemental 03: Trace Listeners", "CSharp.Ch11.Supplemental.03.TraceListeners"),
                    new Lesson("Supplemental 04: Performance Counters and Profiling", "CSharp.Ch11.Supplemental.04.PerformanceCountersAndProfiling"),
                    new Lesson("Textbook Lab: Order Entry Form (WinForms, interactive)", "CSharp.Ch11.TextbookCode.Ch11RealWorldScenario01"),
                    new Lesson("Textbook Lab: Write to Event Log (WinForms, interactive)", "CSharp.Ch11.TextbookCode.WriteToEventLog")
                ])
            ];
        }
        #endregion

        #region Chapter Menu
        private static void RunChapterMenu(List<Chapter> chapters, string solutionRoot)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("DataBank IMX Developer Training");
                Console.WriteLine("================================");
                Console.WriteLine();
                Console.WriteLine("Select a chapter:");
                Console.WriteLine();

                for (int i = 0; i < chapters.Count; i++)
                {
                    Console.WriteLine($"  {i + 1}. {chapters[i].Title}");
                }
                Console.WriteLine();
                Console.WriteLine("  X. Exit");
                Console.WriteLine();
                Console.Write("Choice: ");

                string choice = Console.ReadLine()?.Trim() ?? "";

                if (string.Equals(choice, "X", StringComparison.OrdinalIgnoreCase)) return;

                if (int.TryParse(choice, out int selection) && selection >= 1 && selection <= chapters.Count)
                {
                    bool exitProgram = RunLessonMenu(chapters[selection - 1], solutionRoot);
                    if (exitProgram) return;
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("That's not a valid choice. Press any key to try again...");
                    Console.ReadKey();
                }
            }
        }
        #endregion

        #region Lesson Menu
        // Returns true if the person chose to exit the whole program from here,
        // false if they chose to go back to the chapter menu instead.
        private static bool RunLessonMenu(Chapter chapter, string solutionRoot)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine(chapter.Title);
                Console.WriteLine(new string('=', chapter.Title.Length));
                Console.WriteLine();
                Console.WriteLine("Select a lesson:");
                Console.WriteLine();

                for (int i = 0; i < chapter.Lessons.Count; i++)
                {
                    Console.WriteLine($"  {i + 1}. {chapter.Lessons[i].DisplayName}");
                }
                Console.WriteLine();
                Console.WriteLine("  B. Back to chapter menu");
                Console.WriteLine("  X. Exit program");
                Console.WriteLine();
                Console.Write("Choice: ");

                string choice = Console.ReadLine()?.Trim() ?? "";

                if (string.Equals(choice, "B", StringComparison.OrdinalIgnoreCase)) return false;
                if (string.Equals(choice, "X", StringComparison.OrdinalIgnoreCase)) return true;

                if (int.TryParse(choice, out int selection) && selection >= 1 && selection <= chapter.Lessons.Count)
                {
                    RunLesson(chapter.Lessons[selection - 1], solutionRoot);

                    Console.WriteLine();
                    Console.WriteLine("Press any key to return to the lesson menu...");
                    Console.ReadKey();
                    // Falls through and redisplays this same lesson menu once acknowledged
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("That's not a valid choice. Press any key to try again...");
                    Console.ReadKey();
                }
            }
        }
        #endregion

        #region Running a Lesson
        // Runs a lesson as its own process, sharing this console window rather than
        // spawning a new one, and waits for it to finish before returning. Most lessons
        // run via "dotnet run", which builds automatically if out of date and never
        // needs a new project reference (and rebuild) here every time a lesson is added.
        // Lessons using a <COMReference> (RequiresFullFrameworkMsBuild) can't go through
        // "dotnet run" at all, see RunLessonWithFullFrameworkMsBuild for why, and are
        // built and launched separately instead.
        private static void RunLesson(Lesson lesson, string solutionRoot)
        {
            if (lesson.RequiresVisualStudio)
            {
                Console.Clear();
                Console.WriteLine($"{lesson.DisplayName} can't be launched from LessonRunner.");
                Console.WriteLine();
                Console.WriteLine(lesson.VisualStudioInstructions ?? "Open this project's .csproj in Visual Studio and run it from there.");
                return;
            }

            string projectDirectory = Path.Combine(solutionRoot, lesson.ProjectName);
            string projectFile = Path.Combine(projectDirectory, $"{lesson.ProjectName}.csproj");

            if (!File.Exists(projectFile))
            {
                Console.WriteLine();
                Console.WriteLine($"Could not find {projectFile}");
                Console.WriteLine("Press any key to return to the lesson menu...");
                Console.ReadKey();
                return;
            }

            Console.Clear();

            if (lesson.RequiresFullFrameworkMsBuild)
            {
                RunLessonWithFullFrameworkMsBuild(lesson, projectDirectory, projectFile);
            }
            else
            {
                RunLessonWithDotNetRun(lesson, projectDirectory, projectFile);
            }

            // A lesson that runs without a debugger attached and shares this console
            // (rather than getting its own window) can leave stray keystrokes sitting in
            // the console input buffer, someone pressing keys while waiting on a pause
            // that wasn't actually happening, for example. Left undrained, that leftover
            // input gets silently prepended to the next ReadLine() call here (the
            // "press any key to return" prompt or the lesson menu choice after it),
            // which can corrupt an otherwise valid choice like "X" for Exit. Drain it now
            // that the lesson's own process has fully exited.
            DrainInputBuffer();
        }

        // Discards any keystrokes sitting in the console input buffer without blocking.
        // Safe to call even when input has been redirected (e.g. non-interactive contexts).
        private static void DrainInputBuffer()
        {
            try
            {
                while (Console.KeyAvailable)
                {
                    Console.ReadKey(intercept: true);
                }
            }
            catch (InvalidOperationException)
            {
                // Console.KeyAvailable/ReadKey throw when input is redirected, nothing to drain there.
            }
        }

        // The standard path: "dotnet run" builds the project if it's out of date and
        // launches it, sharing this console.
        private static void RunLessonWithDotNetRun(Lesson lesson, string projectDirectory, string projectFile)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{projectFile}\" --nologo",
                WorkingDirectory = projectDirectory,
                UseShellExecute = false
            };

            try
            {
                using var process = Process.Start(startInfo);
                process?.WaitForExit();

                if (process != null && process.ExitCode != 0)
                {
                    Console.WriteLine();
                    Console.WriteLine($"[{lesson.DisplayName}] exited with a non-zero code ({process.ExitCode}), scroll up to see what it printed.");
                }
            }
            catch (Exception ex)
            {
                new DatabankException($"Error running lesson [{lesson.DisplayName}]!", ex).Log();
                Console.WriteLine();
                Console.WriteLine("Press any key to return to the lesson menu...");
                Console.ReadKey();
            }
        }

        // For lessons using a <COMReference> (tlbimp-based COM interop, e.g. Excel).
        // The "dotnet" SDK CLI's bundled MSBuild cannot process the ResolveComReference
        // task at all (MSB4803: "The task 'ResolveComReference' is not supported on the
        // .NET Core version of MSBuild"), so "dotnet run"/"dotnet build" fail outright
        // regardless of machine setup. The full .NET Framework MSBuild.exe that ships
        // with Visual Studio can process it, so we locate and invoke that instead, then
        // launch the resulting .exe directly.
        private static void RunLessonWithFullFrameworkMsBuild(Lesson lesson, string projectDirectory, string projectFile)
        {
            string msBuildPath = FindFullFrameworkMsBuild();

            if (msBuildPath == null)
            {
                Console.WriteLine("Could not locate a full .NET Framework MSBuild.exe (via vswhere).");
                Console.WriteLine($"[{lesson.DisplayName}] uses a <COMReference>, which only Visual Studio's");
                Console.WriteLine("MSBuild can build, not \"dotnet build\". Make sure Visual Studio (with the");
                Console.WriteLine(".NET desktop development workload) is installed.");
                return;
            }

            var buildInfo = new ProcessStartInfo
            {
                FileName = msBuildPath,
                Arguments = $"\"{projectFile}\" /p:Configuration=Debug /nologo /verbosity:minimal",
                WorkingDirectory = projectDirectory,
                UseShellExecute = false
            };

            using (var buildProcess = Process.Start(buildInfo))
            {
                buildProcess?.WaitForExit();

                if (buildProcess == null || buildProcess.ExitCode != 0)
                {
                    Console.WriteLine();
                    Console.WriteLine($"[{lesson.DisplayName}] failed to build with MSBuild.exe, scroll up to see what it printed.");
                    return;
                }
            }

            // Directory.Build.props pins every project in this solution to net48
            string exePath = Path.Combine(projectDirectory, "bin", "Debug", "net48", $"{lesson.ProjectName}.exe");

            if (!File.Exists(exePath))
            {
                Console.WriteLine();
                Console.WriteLine($"Build succeeded but could not find {exePath}");
                return;
            }

            var runInfo = new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = projectDirectory,
                UseShellExecute = false
            };

            using var runProcess = Process.Start(runInfo);
            runProcess?.WaitForExit();
        }

        // Locates the full .NET Framework MSBuild.exe bundled with Visual Studio via
        // vswhere.exe (itself installed alongside any VS 2017+ install), since the
        // "dotnet" SDK CLI ships its own, different MSBuild that can't build COM references.
        private static string FindFullFrameworkMsBuild()
        {
            string vswherePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Microsoft Visual Studio", "Installer", "vswhere.exe");

            if (!File.Exists(vswherePath)) return null;

            var startInfo = new ProcessStartInfo
            {
                FileName = vswherePath,
                Arguments = "-latest -requires Microsoft.Component.MSBuild -find MSBuild\\**\\Bin\\MSBuild.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            using var process = Process.Start(startInfo);
            string output = process?.StandardOutput.ReadToEnd() ?? "";
            process?.WaitForExit();

            string path = output
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            return !string.IsNullOrWhiteSpace(path) && File.Exists(path) ? path : null;
        }
        #endregion

        #region Solution Discovery
        // Walks up from wherever this executable actually is, looking for the .sln
        // file, so lesson paths resolve correctly no matter how LessonRunner itself
        // was launched (dotnet run, F5 in Visual Studio, or the built .exe directly).
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
