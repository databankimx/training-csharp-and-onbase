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
using System.Linq;
using System.Text.RegularExpressions;
#endregion

namespace CSharp.Supplemental.TrieExamples;

/// <summary>
/// Provides the application entry point for loading a dictionary into a trie and running an interactive console-based
/// word lookup.
/// </summary>
/// <remarks>Parses non-flag command-line arguments for optional dictionary file and data folder overrides,
/// initializes the trie, and prompts for alphabetic input until the user exits.</remarks>
internal static class Program
{
    #region Main
    // Main Executable Method
    private static void Main(string[] args)
    {
        // Defensive against anything flag-like ending up in args (e.g. a launcher forwarding
        // its own command-line switches through by mistake) - only treat an argument as a real
        // file/folder override if it doesn't start with a dash.
        string[] realArgs = [.. args.Where(a => !a.StartsWith("-"))];
        string file = realArgs.Length > 0 ? realArgs[0] : "words.txt";
        string folder = realArgs.Length > 1 ? realArgs[1] : "data";

        var t = InitializeTrie(file, folder);
        Pause();

        while (true)
        {
            Console.Clear();
            Console.Write("\nEnter a word to search (letters only) or press <ENTER> to quit:\n> ");
            string word = Console.ReadLine();
            if (string.IsNullOrEmpty(word))
            {
                Console.Clear();
                Console.WriteLine("\nGoodbye...\n");
                break;
            }
            if (!Regex.IsMatch(word, "^[a-zA-Z]+$"))
            {
                Console.WriteLine("\nPlease enter letters only!\n");
                Pause();
                continue;
            }
            word = word.ToLower();
            string result = t.Search(word) ? "" : "not ";
            Console.WriteLine($"'{word}' is {result}in the dictionary...\n");
            Pause();
        }
    }
    #endregion

    #region Helper Functions
    // Load the trie to test
    private static Trie InitializeTrie(string fileName, string folder = "")
    {
        Console.Clear();
        Console.WriteLine("Initializing...");
        var t = new Trie();
        t.LoadWords(fileName, folder);
        Console.WriteLine("Ready!\n");
        return t;
    }

    // Wait for user interaction before continuing
    private static void Pause()
    {
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
        Console.Clear();
    }
    #endregion
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
