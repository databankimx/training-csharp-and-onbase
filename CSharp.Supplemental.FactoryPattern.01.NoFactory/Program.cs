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
using System.Text.Json;
using System.Xml.Linq;
#endregion

namespace CSharp.Supplemental.FactoryPattern._01.NoFactory
{
    // A client has a library of songs and wants to convert them to a more convenient format
    // like XML or JSON. This version does NOT implement the factory method pattern - see
    // 02.BasicFactory and 03.ImprovingPattern for what changes once it does.
    internal static class Program
    {
        #region Main
        private static void Main()
        {
            var song = new Song("1", "Hello", "John Doe");

            Console.WriteLine("JSON");
            Console.WriteLine(Serialize(song, "JSON"));
            Console.WriteLine();
            Console.WriteLine("XML");
            Console.WriteLine(Serialize(song, "XML"));

            if (!System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
        #endregion

        #region Serialization
        // Every format the caller might ask for, and every step of producing it, all
        // tangled together in one method. Adding a third format means editing this same
        // method again - there's nowhere else for that logic to go.
        private static string Serialize(Song song, string dataFormat)
        {
            if (dataFormat == "JSON")
            {
                return JsonSerializer.Serialize(song, new JsonSerializerOptions { WriteIndented = true });
            }

            if (dataFormat == "XML")
            {
                var songElement = new XElement("song",
                    new XAttribute("id", song.SongId),
                    new XElement("title", song.Title),
                    new XElement("artist", song.Artist));
                return songElement.ToString();
            }

            throw new ArgumentException($"Unknown data format: {dataFormat}");
        }
        #endregion
    }

    // Represents a song
    internal class Song
    {
        public string SongId { get; }
        public string Title { get; }
        public string Artist { get; }

        public Song(string songId, string title, string artist)
        {
            SongId = songId;
            Title = title;
            Artist = artist;
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
