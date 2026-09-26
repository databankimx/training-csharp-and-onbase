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
using YamlDotNet.Serialization;
#endregion

namespace CSharp.Supplemental.FactoryPattern._03.ImprovingPattern
{
    // Same factory method structure as 02.BasicFactory, but now a third format - YAML - has
    // been added. Compare this file to 02.BasicFactory\Program.cs: the Interface (Serialize)
    // didn't change at all. Adding YAML meant one new enum value, one new switch arm in the
    // Creator, and one new Product method - nothing that already worked had to be touched.
    // That's the improvement the pattern's name promises.
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
            Console.WriteLine();
            Console.WriteLine("YAML");
            Console.WriteLine(Serialize(song, DataFormat.Yaml));

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
            DataFormat.Yaml => SerializeToYaml,
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

        // The new product - nothing above it needed to change to make room for this
        private static string SerializeToYaml(Song song)
        {
            var serializer = new SerializerBuilder().Build();
            return serializer.Serialize(song);
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
        Xml = 2,
        Yaml = 3
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
