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

namespace CSharp.Supplemental.FactoryPattern._02.BasicFactory
{
    // Same task as 01.NoFactory - serialize a Song to JSON or XML - but this version
    // introduces the factory method pattern: a Creator (GetSerializer) that decides which
    // Product (SerializeToJson/SerializeToXml) to hand back, keeping the public Interface
    // (Serialize) itself completely unaware of how many formats exist or how each one works.
    internal static class Program
    {
        #region Main
        private static void Main()
        {
            var song = new Song("1", "Hello", "John Doe");

            Console.WriteLine("JSON");
            Console.WriteLine(Serialize(song, DataFormat.Json));
            Console.WriteLine();
            Console.WriteLine("XML");
            Console.WriteLine(Serialize(song, DataFormat.Xml));

            if (!System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
        #endregion

        #region Interface
        private static string Serialize(Song song, DataFormat dataFormat)
        {
            var serializer = GetSerializer(dataFormat);
            return serializer(song);
        }
        #endregion

        #region Creator
        private static Func<Song, string> GetSerializer(DataFormat dataFormat) => dataFormat switch
        {
            DataFormat.Json => SerializeToJson,
            DataFormat.Xml => SerializeToXml,
            _ => throw new ArgumentException($"Unknown data format: {dataFormat}")
        };
        #endregion

        #region Products
        private static string SerializeToJson(Song song)
            => JsonSerializer.Serialize(song, new JsonSerializerOptions { WriteIndented = true });

        private static string SerializeToXml(Song song)
        {
            var songElement = new XElement("song",
                new XAttribute("id", song.SongId),
                new XElement("title", song.Title),
                new XElement("artist", song.Artist));
            return songElement.ToString();
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

    // Supported data formats
    internal enum DataFormat
    {
        Undefined = 0,
        Json = 1,
        Xml = 2
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
