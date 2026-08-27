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
using System.Reflection;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch12.Supplemental._04.StrongNamingAndTheGacDeepDive
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * The main lesson's UnderstandingStrongNaming() found that THIS project's own
         *   assembly isn't strong-named, expected, since application projects rarely need
         *   to be. Rather than trying to strong-name a throwaway demo assembly (a real
         *   .snk-based signing setup is a build-time/tooling concern, not something
         *   meaningfully demonstrated by generating one on the fly at runtime), this
         *   Supplemental instead inspects REAL, already strong-named assemblies that are
         *   guaranteed to be present on any .NET Framework machine: the framework's own
         *   core assemblies. Every comparison below is against genuine, verifiable data,
         *   not a synthetic example built just for this lesson.
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                ComparingStrongNamedVsNotStrongNamed();
                GenericFunctions.Pause();

                CheckingIfLoadedFromTheGac();
                GenericFunctions.Pause();

                UnderstandingVersionRedirects();
                GenericFunctions.Pause();

                WhySideBySideVersioningMatters();
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
        // A real, concrete side-by-side: THIS project's own assembly (not strong-named)
        //   against mscorlib (definitely strong-named, present on every .NET Framework
        //   machine, no special setup needed)
        private static void ComparingStrongNamedVsNotStrongNamed()
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            Assembly mscorlib = typeof(object).Assembly;

            Console.WriteLine("THIS project's assembly:");
            Console.WriteLine($"  Full name: {thisAssembly.FullName}");
            PrintPublicKeyToken(thisAssembly);

            Console.WriteLine($"{Environment.NewLine}mscorlib (the core .NET Framework assembly, holds System.Object, System.String,");
            Console.WriteLine("etc.):");
            Console.WriteLine($"  Full name: {mscorlib.FullName}");
            PrintPublicKeyToken(mscorlib);

            Console.WriteLine($"{Environment.NewLine}Notice mscorlib's full name has FOUR parts (Name, Version, Culture,");
            Console.WriteLine("PublicKeyToken) all populated, this project's has an empty PublicKeyToken. That");
            Console.WriteLine("difference IS what \"strong-named\" means in practice: a full, verifiable identity");
            Console.WriteLine("versus just a simple name.");
        }

        private static void PrintPublicKeyToken(Assembly assembly)
        {
            byte[] token = assembly.GetName().GetPublicKeyToken();
            string display = (token == null || token.Length == 0)
                ? "(none, not strong-named)"
                : BitConverter.ToString(token).Replace("-", "").ToLowerInvariant();

            Console.WriteLine($"  Public key token: {display}");
        }

        // Assembly.GlobalAssemblyCache: whether an assembly was actually loaded FROM the GAC
        private static void CheckingIfLoadedFromTheGac()
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            Assembly mscorlib = typeof(object).Assembly;

            Console.WriteLine($"THIS project's assembly loaded from the GAC: {thisAssembly.GlobalAssemblyCache}");
            Console.WriteLine($"mscorlib loaded from the GAC: {mscorlib.GlobalAssemblyCache}");
            Console.WriteLine($"{Environment.NewLine}This project's assembly sits right next to its own .exe, in this project's own");
            Console.WriteLine("output folder, exactly what most application assemblies do. mscorlib, and the");
            Console.WriteLine("rest of the .NET Framework's own core assemblies, are installed machine-wide,");
            Console.WriteLine("every .NET Framework application on this machine shares the exact same physical");
            Console.WriteLine("copy, rather than each one bundling its own.");
        }

        // Version redirects: a real, concrete example already present elsewhere in this
        //   training set, worth connecting back to directly
        private static void UnderstandingVersionRedirects()
        {
            Console.WriteLine("A <bindingRedirect> in App.config/Web.config tells the .NET Framework runtime");
            Console.WriteLine("\"when something asks for version X of this strong-named assembly, actually load");
            Console.WriteLine("version Y instead\", without recompiling anything that made the original request.");
            Console.WriteLine();
            Console.WriteLine("This training set already has a genuine, real example of exactly this, worth going");
            Console.WriteLine("back and reading directly: CSharp.Ch09.TextbookCode.NorthwindsWCFDataService's own");
            Console.WriteLine("Web.config. That project hit a real FileLoadException (\"assembly manifest definition");
            Console.WriteLine("does not match the assembly reference\") because EntityFramework's ASSEMBLY VERSION");
            Console.WriteLine("stays frozen at 6.0.0.0 for every 6.x NuGet package release, while");
            Console.WriteLine("Microsoft.Data.Services (a different, related package) genuinely DOES version its");
            Console.WriteLine("assembly to match its package version. The fix was a <bindingRedirect> mapping the");
            Console.WriteLine("OLD version range to the actual version now installed.");
            Console.WriteLine();
            Console.WriteLine("Worth connecting the dots: this ENTIRE mechanism, redirecting a specific old version");
            Console.WriteLine("to a specific new one WITHOUT breaking anything else, only works because the");
            Console.WriteLine("assembly is strong-named in the first place, a plain \"MyLibrary.dll\" has no version");
            Console.WriteLine("number built into its identity for a binding redirect to even target.");
        }

        // Tying it together: WHY the full strong-name identity (not just simple name)
        //   matters for side-by-side versioning specifically
        private static void WhySideBySideVersioningMatters()
        {
            Console.WriteLine("Side-by-side versioning means two DIFFERENT versions of the SAME-NAMED assembly");
            Console.WriteLine("can both be installed and loaded on one machine at once, each application gets");
            Console.WriteLine("whichever specific version it actually references.");
            Console.WriteLine();
            Console.WriteLine("Worth reconnecting to CSharp.Ch12.UsingEncryptionAndManagingAssemblies' own");
            Console.WriteLine("discussion: a strong name's FULL identity is simple name + version + culture +");
            Console.WriteLine("public key token, ALL together. Two assemblies with the same simple name but");
            Console.WriteLine("different versions have genuinely DIFFERENT full identities, which is exactly what");
            Console.WriteLine("lets the runtime tell them apart and load the correct one for each caller, rather");
            Console.WriteLine("than being forced to pick just one version, machine-wide, for every application.");
            Console.WriteLine();
            Console.WriteLine("This is precisely why EntityFramework's assembly version staying frozen at 6.0.0.0");
            Console.WriteLine("(covered in UnderstandingVersionRedirects() above) is a deliberate design choice,");
            Console.WriteLine("not an oversight: it means every EF6.x NuGet package release, regardless of its own");
            Console.WriteLine("package version number, shares ONE assembly identity, avoiding a genuinely painful");
            Console.WriteLine("side-by-side proliferation of near-identical assembly versions for what's really");
            Console.WriteLine("the same underlying binary contract.");
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
