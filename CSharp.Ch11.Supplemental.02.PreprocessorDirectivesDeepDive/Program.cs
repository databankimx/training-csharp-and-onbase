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

// #define must appear before any real code token in the file, comments and other
//   preprocessor directives are fine before it, but a using directive or a namespace
//   declaration would not be. This is why it sits here, immediately after the
//   copyright comment block, rather than down with the rest of the code it affects.
//   This symbol is FILE-scoped, only THIS file sees it defined, contrast with
//   TRAINING_BUILD (see the .csproj's <DefineConstants>), which every file in this
//   project sees.
#define FILE_SCOPED_DEMO

#region Using Directives
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch11.Supplemental._02.PreprocessorDirectivesDeepDive
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * The main lesson showed #if DEBUG/#else/#endif deciding what gets compiled based
         *   on build configuration. This Supplemental covers the rest: defining your OWN
         *   symbols (both file-scoped, via #define, and project-scoped, via the .csproj),
         *   #pragma warning for silencing a specific warning around code that genuinely
         *   needs it, and a few directives worth knowing about but not safely
         *   demonstrable live here (#warning/#error would break this build on purpose,
         *   which isn't something a shared training solution should do every time it
         *   compiles). Also covers a closely-related, genuinely useful, ACTUALLY
         *   runtime-observable feature: caller info attributes, compiler-injected values
         *   that work a lot like "predefined compiler constants."
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                UsingDefineAndConditionalCompilation();
                GenericFunctions.Pause();

                ExplainingRegions();
                GenericFunctions.Pause();

                UsingPragmaWarningDisable();
                GenericFunctions.Pause();

                UsingCallerInfoAttributes();
                GenericFunctions.Pause();

                WorthKnowingButNotDemonstratedHere();
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

        #region Lesson Methods
        // #define (file-scoped) vs a project-wide symbol (via .csproj's DefineConstants)
        private static void UsingDefineAndConditionalCompilation()
        {
            #if FILE_SCOPED_DEMO
            Console.WriteLine("FILE_SCOPED_DEMO is defined (via #define at the top of THIS file only).");
            #else
            Console.WriteLine("FILE_SCOPED_DEMO is NOT defined.");
            #endif

            #if TRAINING_BUILD
            Console.WriteLine("TRAINING_BUILD is defined (via this PROJECT's .csproj, every file in this project sees it).");
            #else
            Console.WriteLine("TRAINING_BUILD is NOT defined.");
            #endif

            Console.WriteLine($"{Environment.NewLine}Both branches above compiled IN, both symbols are genuinely defined here, worth");
            Console.WriteLine("comparing this project's .csproj (<DefineConstants>) against this file's own");
            Console.WriteLine("#define line to see exactly where each symbol actually comes from.");
        }

        // #region/#endregion: pure editor organization, zero effect on compiled output
        private static void ExplainingRegions()
        {
            #region An Example Nested Region
            Console.WriteLine("This line lives inside a #region, purely for code-folding/organization in an editor.");
            #endregion

            Console.WriteLine("#region/#endregion have NO effect on the compiled program whatsoever, unlike every");
            Console.WriteLine("other directive in this file, they don't decide what compiles, they don't affect");
            Console.WriteLine("warnings, nothing. Purely a readability/navigation aid for whoever's editing the file.");
        }

        // #pragma warning: silencing a SPECIFIC warning for a SPECIFIC section of code
        private static void UsingPragmaWarningDisable()
        {
            // Without the #pragma below, this line would generate CS0219 ("The variable
            //   'intentionallyUnused' is assigned but its value is never used").
            #pragma warning disable CS0219
            #pragma warning disable S1481
            #pragma warning disable IDE0059 // Unnecessary assignment of a value
            int intentionallyUnused = 42;
            #pragma warning restore IDE0059 // Unnecessary assignment of a value
            #pragma warning restore S1481
            #pragma warning restore CS0219

            Console.WriteLine("Declared (and intentionally never read) a variable inside a #pragma warning disable");
            Console.WriteLine("CS0219 / #pragma warning restore CS0219 block, this compiles cleanly, without the");
            Console.WriteLine("\"assigned but never used\" warning that line would otherwise generate.");
            Console.WriteLine($"{Environment.NewLine}Worth using narrowly and restoring immediately after, exactly as done here, rather");
            Console.WriteLine("than disabling a warning number for an entire file (or, worse, an entire project),");
            Console.WriteLine("which risks silently hiding a genuine future mistake the warning would have caught.");
        }

        // Caller info attributes: compiler-injected values, closely related in spirit to
        //   "predefined compiler constants", and genuinely runtime-observable
        private static void UsingCallerInfoAttributes()
        {
            Log("This message shows exactly where it was logged from, automatically.");
        }

        // The compiler fills in filePath/lineNumber/memberName automatically at every call
        //   site, the caller never has to (and, using these attributes, CAN'T) pass them
        //   explicitly, __FILE__/__LINE__-style macros in other languages do something
        //   similar, but as a genuine C# language feature rather than textual substitution.
        private static void Log(string message,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
        {
            Console.WriteLine($"[{System.IO.Path.GetFileName(filePath)}:{lineNumber} in {memberName}()] {message}");
        }

        // Worth knowing about, not demonstrated live: directives that would either break
        //   this build on purpose, or have no runtime-observable effect to show
        private static void WorthKnowingButNotDemonstratedHere()
        {
            Console.WriteLine("#warning \"message\"   forces a compiler WARNING at that exact line, every build.");
            Console.WriteLine("#error \"message\"     forces a compiler ERROR at that exact line, stops the build entirely.");
            Console.WriteLine("#line 200 \"Other.cs\"  makes the compiler report SUBSEQUENT lines as if they came from");
            Console.WriteLine("                      line 200 of \"Other.cs\", used by code generators so errors in");
            Console.WriteLine("                      GENERATED code point back to the ORIGINAL source that produced it.");
            Console.WriteLine("#pragma checksum      embeds a checksum for a source file, used by debuggers to verify");
            Console.WriteLine("                      the source matches what was actually compiled.");
            Console.WriteLine($"{Environment.NewLine}None of these are demonstrated live here, #warning/#error would break this shared");
            Console.WriteLine("training solution's build on purpose, and #line/#pragma checksum have no runtime-");
            Console.WriteLine("observable effect to actually show, their entire effect is on tooling (the compiler's");
            Console.WriteLine("own error reporting, a debugger), not on program behavior.");
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
