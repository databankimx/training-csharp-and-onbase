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

#region Directives
using System;
using System.IO;
using CSharp.SharedLibrary.Models;
using OnBase.Preprocessor.Models.Configuration;
using OnBase.Preprocessor.Models.Enumerations;
#endregion

namespace OnBase.Preprocessor.HelperClasses.Processes
{
    /// <summary>
    /// Preprocessor to remove/replace invalid characters in file
    /// </summary>
    public class FileCleaner
    {
        #region Properties
        /// <summary>
        /// Configuration file settings object
        /// </summary>
        public PreprocessorSettings Settings { get; set; }
        #endregion

        #region Constants
        // List of integer values of all non-printable characters in the ASCII character set
        private readonly int[] invalidCharacters =
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 11, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 127, 129, 141, 143, 144
        };

        // Integer value of form-feed character
        private const int FormFeed = 255;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the FileCleaner class
        /// </summary>
        /// <param name="settings">Configuration file settings object</param>
        public FileCleaner(PreprocessorSettings settings)
        {
            Settings = settings;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Process the file and remove/replace invalid characters
        /// </summary>
        /// <param name="inputFile">Input file path to read/process</param>
        /// <param name="outputFile">Output file path to write</param>
        public void CleanFile(string inputFile, string outputFile)
        {
            try
            {
                // Create read/write objects for the files
                using (var reader = new StreamReader(inputFile))
                using (var writer = new StreamWriter(outputFile))
                {
                    // Get the first character to initialize the variable
                    int i = reader.Read();

                    // When no characters remain, the Read() method returns -1
                    while (i > -1)
                    {
                        // Default state is to write the same character we read
                        string c = ((char)i).ToString();

                        // If the character is outside the permitted range of the assigned character set, or invalid replace it
                        if (i > (int)Settings.CharSet || Array.IndexOf(invalidCharacters, i) > -1)
                            c = Settings.InvalidCharacterSubstitute;

                        // If the character is in the explicit list of replacements, replace it
                        if (Settings.CharacterReplacements[(char)i] != null)
                            c = Settings.CharacterReplacements[(char)i].Replacement.ToString();

                        // If the character is a form-feed, and we are configured to replace them, replace it
                        if (i == FormFeed && Settings.ReplaceFormFeeds)
                            c = Environment.NewLine;

                        // Write the resulting character to the output file
                        if (!string.IsNullOrEmpty(c)) writer.Write(c);

                        // Get the next character in the file
                        i = reader.Read();
                    }
                }
            }
            catch (Exception ex)
            {
                Program.Status = ExitStatus.ProcessingError;
                throw new DatabankException($"Error processing file [{inputFile}]!", ex);
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
