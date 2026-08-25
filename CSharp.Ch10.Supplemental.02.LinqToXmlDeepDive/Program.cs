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
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch10.Supplemental._02.LinqToXmlDeepDive
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * The main lesson's BooksToXml() showed the basics: building an XElement tree
         *   straight out of a LINQ query. This Supplemental covers the rest of what
         *   System.Xml.Linq offers: parsing existing XML, querying it with LINQ, navigating
         *   with Elements()/Descendants()/Attribute(), modifying a loaded document in place,
         *   handling namespaces, and saving/loading to a real file.
         *
         * LINQ to XML (XElement/XAttribute/XDocument, all in System.Xml.Linq) is worth
         *   knowing specifically as the modern, easier-to-work-with alternative to the
         *   older XmlDocument/XmlNode DOM API. Everything here reads and writes noticeably
         *   more naturally than the equivalent XmlDocument code would.
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), $"ch10-linqtoxml-demo-{Guid.NewGuid():N}.xml");

            try
            {
                #region Chapter Lessons
                ParsingExistingXml();
                GenericFunctions.Pause();

                QueryingXmlWithLinq();
                GenericFunctions.Pause();

                TransformingXmlShape();
                GenericFunctions.Pause();

                ModifyingXmlInPlace();
                GenericFunctions.Pause();

                WorkingWithNamespaces();
                GenericFunctions.Pause();

                SavingAndLoadingXml(tempFilePath);
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
                if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
                GenericFunctions.Pause(final: true);
            }
        }
        #endregion

        #region Shared Sample Data
        private static XElement GetLibraryXml()
        {
            return XElement.Parse(@"
                <Library>
                    <Book year=""1949"">
                        <Title>1984</Title>
                        <Author>George Orwell</Author>
                        <Genre>Dystopian</Genre>
                    </Book>
                    <Book year=""1953"">
                        <Title>Fahrenheit 451</Title>
                        <Author>Ray Bradbury</Author>
                        <Genre>Dystopian</Genre>
                    </Book>
                    <Book year=""1932"">
                        <Title>Brave New World</Title>
                        <Author>Aldous Huxley</Author>
                        <Genre>Dystopian</Genre>
                    </Book>
                </Library>");
        }
        #endregion

        #region Lesson Methods
        // Parsing existing XML text and navigating it
        private static void ParsingExistingXml()
        {
            var library = GetLibraryXml();

            Console.WriteLine($"Root element name: {library.Name}");

            // .Elements("Book") gets the immediate child <Book> elements only
            Console.WriteLine($"{Environment.NewLine}All books (Elements(\"Book\")):");
            foreach (var book in library.Elements("Book"))
            {
                // .Element("Title") gets the FIRST matching child, .Value is its text content,
                //   .Attribute("year") reads an attribute directly off the element
#pragma warning disable S1192
                string title = book.Element("Title")?.Value;
#pragma warning restore S1192
                string year = book.Attribute("year")?.Value;
                Console.WriteLine($" - {title} ({year})");
            }
        }

        // Running an ordinary LINQ query over parsed XML
        private static void QueryingXmlWithLinq()
        {
            var library = GetLibraryXml();

            // Descendants() digs through every level, not just immediate children, worth
            //   using over Elements() whenever the XML's nesting depth isn't fixed/known
            var titlesAfter1940 = from book in library.Descendants("Book")
                                   where int.Parse(book.Attribute("year").Value) > 1940
                                   orderby book.Attribute("year").Value
                                   select book.Element("Title").Value;

            Console.WriteLine("Books published after 1940, oldest first:");
            foreach (string title in titlesAfter1940)
            {
                Console.WriteLine($" - {title}");
            }
        }

        // Reshaping XML into a different structure via a LINQ query
        private static void TransformingXmlShape()
        {
            var library = GetLibraryXml();

            // Flattening <Book><Title>/<Author>/<Genre></Book> into <Entry title="..."
            //   author="..." /> elements, attributes instead of nested child elements
#pragma warning disable S1192
            var flattened = new XElement("FlatLibrary",
                from book in library.Elements("Book")
                select new XElement("Entry",
                    new XAttribute("title", book.Element("Title")?.Value ?? ""),
                    new XAttribute("author", book.Element("Author")?.Value ?? ""),
                    new XAttribute("year", book.Attribute("year")?.Value ?? "")));
#pragma warning restore S1192

            Console.WriteLine("Reshaped XML (nested elements flattened into attributes):");
            Console.WriteLine(flattened);
        }

        // Modifying a loaded XML tree in place
        private static void ModifyingXmlInPlace()
        {
            var library = GetLibraryXml();

            // Add a new <Book>
            library.Add(new XElement("Book",
                new XAttribute("year", "1965"),
                new XElement("Title", "Dune"),
                new XElement("Author", "Frank Herbert"),
                new XElement("Genre", "Science Fiction")));

            // Update an existing element's text via SetElementValue()
            var orwellBook = library.Elements("Book").First(b => b.Element("Author")?.Value == "George Orwell");
            orwellBook.SetElementValue("Genre", "Dystopian Classic");

            // Remove elements matching a condition
            library.Elements("Book").Where(b => b.Element("Author")?.Value == "Aldous Huxley").Remove();

            Console.WriteLine("Library after adding Dune, updating 1984's Genre, and removing Brave New World:");
            Console.WriteLine(library);
        }

        // XML namespaces
        private static void WorkingWithNamespaces()
        {
            // XNamespace represents an XML namespace URI, combined with a name (via the +
            //   operator) to build a fully-qualified element/attribute name
            XNamespace ns = "http://example.com/library";

            var library = new XElement(ns + "Library",
                new XElement(ns + "Book", new XAttribute("year", "1949"),
                    new XElement(ns + "Title", "1984")));

            Console.WriteLine("XML with an explicit namespace:");
            Console.WriteLine(library);

            // Querying namespaced XML requires using the SAME XNamespace when naming
            //   elements to search for, a plain "Book" (no namespace) would match nothing
            var titles = library.Descendants(ns + "Title").Select(t => t.Value);

            Console.WriteLine($"{Environment.NewLine}Titles found (searching with the correct namespace):");
            foreach (string title in titles)
            {
                Console.WriteLine($" - {title}");
            }
        }

        // Saving to and loading from a real file
        private static void SavingAndLoadingXml(string filePath)
        {
            var library = GetLibraryXml();

            library.Save(filePath);
            Console.WriteLine($"Saved to {filePath}");

            var reloaded = XElement.Load(filePath);
            Console.WriteLine($"{Environment.NewLine}Reloaded from disk, book count: {reloaded.Elements("Book").Count()}");
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
