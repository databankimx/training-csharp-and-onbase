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
using System.Configuration;
using System.IO;
using OnBase.Preprocessor.HelperClasses.Extensions;
using OnBase.Preprocessor.HelperClasses.Processes;
using OnBase.Preprocessor.Models.Configuration;
using OnBase.Preprocessor.Models.Enumerations;
using CSharp.SharedLibrary.Models;
#endregion

namespace OnBase.Preprocessor
{
    #region Training Notes
    /* ******************************************************************** *
     *                           TRAINING NOTES                             *
     *                                                                      *
     *               SAMPLE PREPROCESSOR FOR ONBASE COLD/DIP                *
     *                                                                      *
     * A preprocessor is used to manipulate the text file being imported    *
     *     via COLD or DIP processing.                                      *
     *                                                                      *
     * Common uses of preprocessors include...                              *
     *  - Removing unwanted or illegal characters                           *
     *    * OnBase processes ASCII but will bog down on control characters. *
     *  - Deleting header lines from index file                             *
     *  - Conversion from other character sets (e.g. EBCDIC)                *
     *  - Reformatting keyword information (e.g. Dates)                     *
     *                                                                      *
     * The following rules must be obeyed when creating preprocessors:      *
     *  - The program should run without any user interaction               *
     *    - For this reason, a console application is preferred.            *
     *                                                                      *
     *  - The executable must accept at least two command-line arguments... *
     *    - Input file (%I in OnBase configuration)                         *
     *    - Output file (%O in OnBase configuration)                        *
     *                                                                      *
     *  - At end the program must return an integer value indicating        *
     *      success or failure (also configured in OnBase)                  *
     *                                                                      *
     * The following additional guidelines can be helpful:                  *
     *  - In a complex preprocessor, consider multiple error return codes   *
     *    For example:                                                      *
     *    - Success:                      0                                 *
     *    - Input file not found:        -1                                 *
     *    - Unable to write output file: -2                                 *
     *    - Error processing text:       -3                                 *
     *      etc.                                                            *
     *    This can help a lot in troubleshooting issues.                    *
     *    NOTE: This simple training example does not implement this        *
     *                                                                      *
     *  - Write (at least errors) to a log file!                            *
     *    Because users are not interacting, this may be the only way to    *
     *      see exception information.                                      *
     *                                                                      *
     * ******************************************************************** */
    #endregion

    /// <summary>
    /// Preprocessor Executable
    /// </summary>
    internal static class Program
    {
        #region Properties
        /// <summary>
        /// Status when exiting the program (provides OnBase integer return)
        /// </summary>
        public static ExitStatus Status { get; set; }
        #endregion

        #region Globals
        // Configuration file settings for the preprocessor
        private static PreprocessorSettings settings;

        // Input file to process (%I from OnBase)
        private static string inputFile;

        // Output file to write (%O from OnBase)
        private static string outputFile;

        // Helper class to perform actual file processing
        private static FileCleaner processor;
        #endregion

        #region Main Executable Method
        // Main method to run when the program is executed
        private static int Main(string[] args)
        {
            try
            {
                // Initialize global variables
                Initialize(args);

                LogWriter.Log("Preprocessor started...", forceLog: true);

                // Perform file processing
                processor.CleanFile(inputFile, outputFile);
            }
            catch (Exception ex)
            {
                // Make sure all exceptions result in an error state being returned to OnBase
                if (Status == ExitStatus.Success) Status = ExitStatus.GeneralError;
                ex.Log();
            }
            finally
            {
                if (settings != null && settings.Interactive)
                {
                    LogWriter.Log($"Preprocessor completed with exit status [{Status}]", forceLog: true);
                    Console.WriteLine("Done! Press <ENTER> to exit...");
                    Console.ReadLine();
                }
            }

            return (int)Status;
        }
        #endregion

        #region Helper Functions
        // Initialize global variables
        private static void Initialize(string[] args)
        {
            try
            {
                Status = ExitStatus.Success;
                settings = (PreprocessorSettings)ConfigurationManager.GetSection(PreprocessorSettings.SectionName);
                LogWriter.DebugMode = settings.DebugMode;
                LogWriter.Interactive = settings.Interactive;
                LogWriter.Initialize(settings.LogFilePath);

                if (args.Length < 2)
                {
                    Status = ExitStatus.MissingArgument;
                    throw new ArgumentNullException(nameof(args), "Command line must include input and output file paths!");
                }

                inputFile = args[0];
                if (!File.Exists(inputFile))
                {
                    Status = ExitStatus.InputFileNotFound;
                    throw new FileNotFoundException($"Cannot find input file [{inputFile}]!");
                }
                LogWriter.Log($"Processing input file [{inputFile}]...");

                outputFile = args[1];
                string outputDirectory = Path.GetDirectoryName(outputFile);
                // An empty outputDirectory just means "no directory given" - i.e. the current
                // directory, which always exists. Only a non-empty directory that doesn't
                // exist is actually an error. (The original always treated empty as an error
                // too, which meant a perfectly normal bare output filename - no path at all -
                // failed here every time.)
                if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
                {
                    Status = ExitStatus.OutputDirectoryNotFound;
                    throw new DirectoryNotFoundException($"Cannot find output directory [{outputDirectory}]!");
                }
                LogWriter.Log($"Processing output file [{outputFile}]...");

                processor = new FileCleaner(settings);
                LogWriter.Log("Initialized file processor...");
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error initializing preprocessor!", ex);
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
