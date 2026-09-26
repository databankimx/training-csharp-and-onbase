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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
#endregion

namespace CSharp.Supplemental.Algorithms.Shared
{
    /// <summary>
    /// Opens the shared browser-based algorithm visualization for a given algorithm name.
    /// Each consuming project copies the Visualizations folder into its own output directory
    /// (see that project's .csproj), so this resolves the player relative to
    /// AppDomain.CurrentDomain.BaseDirectory rather than a fragile relative path.
    /// </summary>
    public static class Visualization
    {
        /// <summary>
        /// Open the visualization player in the default browser for the given algorithm.
        /// </summary>
        /// <param name="algorithmName">
        /// Matches the algorithm script's file name (without extension) under
        /// Visualizations/algorithms/ - e.g. "linear-search" for linear-search.js.
        /// </param>
        /// <returns>
        /// True if a visualization exists and was opened; false if no script exists yet for
        /// this algorithm (not every algorithm has one built yet).
        /// </returns>
        public static bool Open(string algorithmName)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string visualizationsDir = Path.Combine(baseDir, "Visualizations");
            string scriptPath = Path.Combine(visualizationsDir, "algorithms", algorithmName + ".js");
            string playerPath = Path.Combine(visualizationsDir, "player.html");

            if (!File.Exists(scriptPath))
            {
                Console.WriteLine($"No visualization is available yet for '{algorithmName}'.");
                return false;
            }

            // Which algorithm to load is passed via a small generated script rather than a
            // "player.html?algo=x" query string - Process.Start with UseShellExecute routes a
            // local file:// URL through Windows' file-association handling, which appears to
            // treat it as "open this file" rather than "navigate to this exact URL", and drops
            // the query string somewhere in that translation. Writing the algorithm name into
            // its own script file, loaded the same way the algorithm scripts themselves already
            // are, sidesteps the question entirely.
            string currentAlgorithmPath = Path.Combine(visualizationsDir, "current-algorithm.js");
            File.WriteAllText(currentAlgorithmPath, $"window.CURRENT_ALGORITHM = {EscapeForJavaScriptString(algorithmName)};");

            try
            {
                Process.Start(new ProcessStartInfo(playerPath) { UseShellExecute = true });
                return true;
            }
            catch (Win32Exception)
            {
                Console.WriteLine("Could not open a browser for the visualization - no default browser is associated with this file type.");
                return false;
            }
        }

        // Wraps a value as a double-quoted JavaScript string literal, escaping backslashes and
        // quotes - algorithmName is always one of this project's own hyphenated file names, not
        // arbitrary input, but this costs nothing and removes any doubt.
        private static string EscapeForJavaScriptString(string value)
        {
            string escaped = value.Replace("\\", "\\\\").Replace("\"", "\\\"");
            return "\"" + escaped + "\"";
        }

        /// <summary>
        /// Open the Sieve of Eratosthenes grid visualization - self-contained, unlike Open(),
        /// since there's only the one grid-based algorithm rather than a family of
        /// interchangeable ones sharing a bar-chart renderer.
        /// </summary>
        public static bool OpenSieve()
        {
            string sievePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Visualizations", "sieve.html");

            if (!File.Exists(sievePath))
            {
                Console.WriteLine("The Sieve of Eratosthenes visualization is not available.");
                return false;
            }

            try
            {
                Process.Start(new ProcessStartInfo(sievePath) { UseShellExecute = true });
                return true;
            }
            catch (Win32Exception)
            {
                Console.WriteLine("Could not open a browser for the visualization - no default browser is associated with this file type.");
                return false;
            }
        }
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
