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
