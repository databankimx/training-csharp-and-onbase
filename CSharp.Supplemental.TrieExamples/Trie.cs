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
using System.IO;
#endregion

namespace CSharp.Supplemental.TrieExamples;

internal class Trie
{
    #region Properties
    /// <summary>
    /// Number of children per node
    /// </summary>
    internal int Size { get; set; }

    /// <summary>
    /// Root node
    /// </summary>
    internal TrieNode Root { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Create and initialize a new instance of a Trie
    /// </summary>
    /// <param name="size">Number of children per node</param>
    internal Trie(int size = 26)
    {
        Size = size;
        Root = new TrieNode(size);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Load the word file into the trie
    /// </summary>
    /// <param name="fileName">Name of file to load words from</param>
    /// <param name="folder">Folder path to file</param>
    internal void LoadWords(string fileName, string folder = "")
    {
        string path = string.IsNullOrEmpty(folder)
            ? fileName
            : Path.Combine(folder, fileName);
        using var reader = new StreamReader(path);
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            InsertKey(line);
        }
    }

    /// <summary>
    /// Add a new string to the trie
    /// </summary>
    /// <param name="key">String to add</param>
    /// <returns>False in the event of an error - Otherwise true</returns>
    internal bool InsertKey(string key)
    {
        var currentNode = Root;
        foreach (char c in key.ToLower())
        {
            int loc = c - 'a';
            if (loc < 0) return false;
            if (currentNode.Children[loc] == null)
                currentNode.Children[loc] = new TrieNode(Size);

            currentNode = currentNode.Children[loc];
        }
        currentNode.WordCount++;
        return true;
    }

    /// <summary>
    /// Check if a given prefix string exists in the trie
    /// </summary>
    /// <param name="key">Prefix string</param>
    /// <returns>True if prefix was found without error - Otherwise false</returns>
    internal bool PrefixExists(string key)
    {
        var currentNode = Root;
        foreach (char c in key.ToLower())
        {
            int loc = c - 'a';
            if (loc < 0) return false;
            if (currentNode.Children[loc] == null) return false;
            currentNode = currentNode.Children[loc];
        }
        return true;
    }

    /// <summary>
    /// Find a given string in the trie
    /// </summary>
    /// <param name="key">String to find</param>
    /// <returns>True if string was found without error - Otherwise false</returns>
    internal bool Search(string key)
    {
        var currentNode = Root;
        foreach (char c in key.ToLower())
        {
            int loc = c - 'a';
            if (loc < 0) return false;
            if (currentNode.Children[loc] == null) return false;
            currentNode = currentNode.Children[loc];
        }
        return currentNode.WordCount > 0;
    }

    /// <summary>
    /// Remove a given string from the trie
    /// </summary>
    /// <param name="key">String to remove</param>
    /// <returns>True if string was removed without error - Otherwise false</returns>
    #pragma warning disable S3776 // Not overly complex
    internal bool Delete(string key)
    #pragma warning restore S3776
    {
        var currentNode = Root;
        TrieNode lastBranchNode = null;
        char lastBranchChar = 'a';
        int loc;
        foreach (char c in key.ToLower())
        {
            loc = c - 'a';
            if (loc < 0) return false;
            if (currentNode.Children[loc] == null) return false;
            int count = 0;
            for (int i = 0; i < Size; i++)
            {
                if (currentNode.Children[i] != null) count++;
            }
            if (count > 1)
            {
                lastBranchNode = currentNode;
                lastBranchChar = c;
            }
            currentNode = currentNode.Children[loc];
        }
        int counter = 0;
        for (int i = 0; i < Size; i++)
        {
            if (currentNode.Children[i] != null) counter++;
        }
        if (counter > 0)
        {
            currentNode.WordCount--;
            return true;
        }
        loc = lastBranchChar - 'a';
        if (lastBranchNode?.Children[loc] == null) return true;
        loc = key[0] - 'a';
        #pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Root.Children[loc] = null;
        #pragma warning restore CS8625
        return true;
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
