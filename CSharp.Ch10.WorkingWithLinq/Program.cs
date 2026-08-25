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
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CSharp.Ch10.WorkingWithLinq.Models;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch10.WorkingWithLinq
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * LINQ (Language Integrated Query) lets you write queries directly in C#, against
         *   almost any source of data, an in-memory collection, a database, XML, using a
         *   consistent syntax regardless of where the data actually lives.
         *
         * LINQ comes in two equivalent syntaxes:
         * - Query syntax: SQL-like ("from x in source where ... select ..."), often more
         *     readable for queries with several clauses (joins, groupings especially)
         * - Method syntax: chained extension methods ("source.Where(...).Select(...)"),
         *     required for operations query syntax has no keyword for (Skip, Take, Distinct,
         *     Concat, and most aggregate functions)
         *
         * Every query syntax expression is actually compiled into the equivalent method
         *   syntax chain, they are not two different technologies, just two different ways
         *   of writing the same thing. This lesson demonstrates both side by side for the
         *   operations that support it, and method-only for the ones that don't.
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                var authors = BuildAuthors();
                var books = BuildBooks();

                #region Chapter Lessons
                #region Query Expressions
                // Filtering
                QueryFiltering(books);
                GenericFunctions.Pause();

                // Ordering
                QueryOrdering(books);
                GenericFunctions.Pause();

                // Projection
                QueryProjection(books);
                GenericFunctions.Pause();

                // Joining (inner join)
                QueryInnerJoin(books, authors);
                GenericFunctions.Pause();

                // Joining (outer join)
                QueryOuterJoin(books, authors);
                GenericFunctions.Pause();

                // Grouping
                QueryGrouping(books);
                GenericFunctions.Pause();
                #endregion

                #region Method-Based Queries
                // Filtering, Ordering, Projection (method syntax)
                MethodFilteringOrderingProjection(books);
                GenericFunctions.Pause();

                // Joining (method syntax)
                MethodJoining(books, authors);
                GenericFunctions.Pause();

                // Grouping (method syntax)
                MethodGrouping(books);
                GenericFunctions.Pause();

                // Aggregate Functions
                AggregateFunctions(books);
                GenericFunctions.Pause();

                // First and Last
                FirstAndLast(books);
                GenericFunctions.Pause();

                // Concatenation
                Concatenation(books);
                GenericFunctions.Pause();

                // Skip and Take
                SkipAndTake(books);
                GenericFunctions.Pause();

                // Distinct
                Distinct(books);
                GenericFunctions.Pause();
                #endregion

                #region LINQ to XML
                LinqToXml(books);
                GenericFunctions.Pause();
                #endregion
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

        #region Sample Data
        // Sample authors, including one (Margaret Atwood) with no books in BuildBooks(),
        //   used later for the "authors without any books" side of the outer join demo
        private static List<Author> BuildAuthors()
        {
            return
            [
                new Author { AuthorId = 1, Name = "George Orwell", Country = "UK" },
                new Author { AuthorId = 2, Name = "Aldous Huxley", Country = "UK" },
                new Author { AuthorId = 3, Name = "Ray Bradbury", Country = "USA" },
                new Author { AuthorId = 4, Name = "Isaac Asimov", Country = "USA" },
                new Author { AuthorId = 5, Name = "Margaret Atwood", Country = "Canada" }
            ];
        }

        // Sample books, including one (BookId 7) with AuthorId left null, used later for
        //   the "books without a matched author" side of the outer join demo
        private static List<Book> BuildBooks()
        {
            return
            [
                new Book { BookId = 1, Title = "1984", AuthorId = 1, Year = 1949, Genre = "Dystopian", Price = 12.99m },
                new Book { BookId = 2, Title = "Animal Farm", AuthorId = 1, Year = 1945, Genre = "Satire", Price = 9.99m },
                new Book { BookId = 3, Title = "Brave New World", AuthorId = 2, Year = 1932, Genre = "Dystopian", Price = 11.99m },
                new Book { BookId = 4, Title = "Fahrenheit 451", AuthorId = 3, Year = 1953, Genre = "Dystopian", Price = 10.99m },
                new Book { BookId = 5, Title = "The Martian Chronicles", AuthorId = 3, Year = 1950, Genre = "Science Fiction", Price = 13.99m },
                new Book { BookId = 6, Title = "Foundation", AuthorId = 4, Year = 1951, Genre = "Science Fiction", Price = 14.99m },
                new Book { BookId = 7, Title = "Unattributed Anthology", AuthorId = null, Year = 2000, Genre = "Anthology", Price = 8.99m }
            ];
        }
        #endregion

        #region Query Expressions
        // Filtering
        private static void QueryFiltering(List<Book> books)
        {
            var dystopianBooks = from b in books
                                  where b.Genre == "Dystopian"
                                  select b;

            Console.WriteLine("Dystopian books (single condition):");
            foreach (var book in dystopianBooks) Console.WriteLine($" - {book.Title} ({book.Year})");

            // Multiple where clauses (or a single "&&" condition) both filter to the
            //   intersection of both conditions
            var dystopianClassics = from b in books
                                      where b.Genre == "Dystopian"
                                      where b.Year < 1950
                                      select b;

            Console.WriteLine($"{Environment.NewLine}Dystopian books published before 1950 (chained where clauses):");
            foreach (var book in dystopianClassics) Console.WriteLine($" - {book.Title} ({book.Year})");
        }

        // Ordering
        private static void QueryOrdering(List<Book> books)
        {
            var byYearDescending = from b in books
                                     orderby b.Year descending
                                     select b;

            Console.WriteLine("Books, newest first:");
            foreach (var book in byYearDescending) Console.WriteLine($" - {book.Year}: {book.Title}");

            var byGenreThenYear = from b in books
                                    orderby b.Genre ascending, b.Year ascending
                                    select b;

            Console.WriteLine($"{Environment.NewLine}Books, by genre then year (multi-key ordering):");
            foreach (var book in byGenreThenYear) Console.WriteLine($" - {book.Genre}, {book.Year}: {book.Title}");
        }

        // Projection
        private static void QueryProjection(List<Book> books)
        {
            var titlesOnly = from b in books
                              select b.Title;

            Console.WriteLine("Titles only (projecting to a single field):");
            foreach (string title in titlesOnly) Console.WriteLine($" - {title}");

            var titleAndYear = from b in books
                                select new { b.Title, b.Year };

            Console.WriteLine($"{Environment.NewLine}Title and year (projecting to an anonymous type):");
            foreach (var book in titleAndYear) Console.WriteLine($" - {book.Title} ({book.Year})");
        }

        // Joining (inner join)
        private static void QueryInnerJoin(List<Book> books, List<Author> authors)
        {
            var booksWithAuthors = from b in books
                                     join a in authors on b.AuthorId equals a.AuthorId
                                     select new { b.Title, a.Name };

            // Note: "Unattributed Anthology" (AuthorId null) does not appear here, an inner
            //   join only returns rows with a match on BOTH sides.
            Console.WriteLine("Books with their authors (inner join):");
            foreach (var book in booksWithAuthors) Console.WriteLine($" - {book.Title} by {book.Name}");
        }

        // Joining (outer join, via "join ... into" plus DefaultIfEmpty())
        private static void QueryOuterJoin(List<Book> books, List<Author> authors)
        {
            var booksWithAuthors = from b in books
                                     join a in authors on b.AuthorId equals a.AuthorId into bookAuthors
                                     from author in bookAuthors.DefaultIfEmpty(new Author { Name = "(unknown)" })
                                     select new { b.Title, author.Name };

            // Unlike QueryInnerJoin(), "Unattributed Anthology" DOES appear here now, with
            //   Name falling back to "(unknown)" since it has no matching Author at all.
            Console.WriteLine("Books with their authors, including books with no matched author (outer join):");
            foreach (var book in booksWithAuthors) Console.WriteLine($" - {book.Title} by {book.Name}");
        }

        // Grouping
        private static void QueryGrouping(List<Book> books)
        {
            var byGenre = from b in books
                          group b by b.Genre;

            Console.WriteLine("Books grouped by genre:");
            foreach (var genreGroup in byGenre)
            {
                Console.WriteLine($" - {genreGroup.Key} ({genreGroup.Count()}):");
                foreach (var book in genreGroup) Console.WriteLine($"    - {book.Title}");
            }

            // "group ... into" continues the query against the grouped results, here
            //   projecting each group down to just its key and an aggregate over its members
            var genreSummary = from b in books
                                group b by b.Genre into g
                                select new { Genre = g.Key, AveragePrice = g.Average(b => b.Price) };

            Console.WriteLine($"{Environment.NewLine}Average price by genre (group ... into):");
            foreach (var summary in genreSummary) Console.WriteLine($" - {summary.Genre}: {summary.AveragePrice:C}");
        }
        #endregion

        #region Method-Based Queries
        // Filtering, Ordering, Projection (method syntax), directly mirroring the query
        //   syntax versions above
        private static void MethodFilteringOrderingProjection(List<Book> books)
        {
            var dystopianClassics = books
                .Where(b => b.Genre == "Dystopian")
                .Where(b => b.Year < 1950);

            Console.WriteLine("Dystopian books published before 1950 (method syntax):");
            foreach (var book in dystopianClassics) Console.WriteLine($" - {book.Title} ({book.Year})");

            var byGenreThenYear = books
                .OrderBy(b => b.Genre)
                .ThenBy(b => b.Year);

            Console.WriteLine($"{Environment.NewLine}Books, by genre then year (method syntax):");
            foreach (var book in byGenreThenYear) Console.WriteLine($" - {book.Genre}, {book.Year}: {book.Title}");

            var titleAndYear = books.Select(b => new { b.Title, b.Year });

            Console.WriteLine($"{Environment.NewLine}Title and year (method syntax):");
            foreach (var book in titleAndYear) Console.WriteLine($" - {book.Title} ({book.Year})");
        }

        // Joining (method syntax), both inner (Join) and outer (GroupJoin + SelectMany)
        private static void MethodJoining(List<Book> books, List<Author> authors)
        {
            var innerJoined = books.Join(authors,
                b => b.AuthorId,
                a => a.AuthorId,
                (b, a) => new { b.Title, a.Name });

            Console.WriteLine("Books with their authors (Join, method syntax):");
            foreach (var book in innerJoined) Console.WriteLine($" - {book.Title} by {book.Name}");

            // GroupJoin() alone produces one entry per book, each holding the (0 or 1)
            //   matching authors as a nested group. SelectMany() then flattens that back
            //   into one row per book, using DefaultIfEmpty() to keep books with no match.
            var outerJoined = books.GroupJoin(authors,
                    b => b.AuthorId,
                    a => a.AuthorId,
                    (b, matchedAuthors) => new { Book = b, MatchedAuthors = matchedAuthors })
                .SelectMany(
                    x => x.MatchedAuthors.DefaultIfEmpty(new Author { Name = "(unknown)" }),
                    (x, a) => new { x.Book.Title, a.Name });

            Console.WriteLine($"{Environment.NewLine}Books with their authors, including unmatched books (GroupJoin, method syntax):");
            foreach (var book in outerJoined) Console.WriteLine($" - {book.Title} by {book.Name}");
        }

        // Grouping (method syntax)
        private static void MethodGrouping(List<Book> books)
        {
            var byGenre = books.GroupBy(b => b.Genre);

            Console.WriteLine("Books grouped by genre (method syntax):");
            foreach (var genreGroup in byGenre)
            {
                Console.WriteLine($" - {genreGroup.Key} ({genreGroup.Count()}):");
                foreach (var book in genreGroup) Console.WriteLine($"    - {book.Title}");
            }
        }

        // Aggregate Functions
        private static void AggregateFunctions(List<Book> books)
        {
            var dystopianBooks = books.Where(b => b.Genre == "Dystopian");

            int count = dystopianBooks.Count();
            decimal average = dystopianBooks.Average(b => b.Price);
            decimal sum = dystopianBooks.Sum(b => b.Price);
            decimal min = dystopianBooks.Min(b => b.Price);
            decimal max = dystopianBooks.Max(b => b.Price);

            Console.WriteLine("Aggregate functions over Dystopian books:");
            Console.WriteLine($" - Count: {count}");
            Console.WriteLine($" - Average price: {average:C}");
            Console.WriteLine($" - Total price: {sum:C}");
            Console.WriteLine($" - Cheapest: {min:C}");
            Console.WriteLine($" - Most expensive: {max:C}");
        }

        // First and Last
        private static void FirstAndLast(List<Book> books)
        {
            var byYear = books.OrderBy(b => b.Year);

            var earliest = byYear.First();
            var latest = byYear.Last();

            Console.WriteLine($"Earliest book: {earliest.Title} ({earliest.Year})");
            Console.WriteLine($"Latest book: {latest.Title} ({latest.Year})");
        }

        // Concatenation
        private static void Concatenation(List<Book> books)
        {
            var recentReleases = new List<Book>
            {
                new Book { BookId = 8, Title = "A New Release", Year = 2025, Genre = "Science Fiction", Price = 19.99m }
            };

            var allBooks = books.Concat(recentReleases);

            Console.WriteLine($"All books, existing catalog concatenated with new releases ({allBooks.Count()} total):");
            foreach (var book in allBooks) Console.WriteLine($" - {book.Title}");
        }

        // Skip and Take
        private static void SkipAndTake(List<Book> books)
        {
            var byYear = books.OrderBy(b => b.Year).ToList();

            var secondPage = byYear.Skip(2).Take(2);

            Console.WriteLine("Books 3-4, ordered by year (Skip(2).Take(2), a simple pagination pattern):");
            foreach (var book in secondPage) Console.WriteLine($" - {book.Year}: {book.Title}");
        }

        // Distinct
        private static void Distinct(List<Book> books)
        {
            var genres = books.Select(b => b.Genre).Distinct();

            Console.WriteLine("Distinct genres:");
            foreach (string genre in genres) Console.WriteLine($" - {genre}");
        }
        #endregion

        #region LINQ to XML
        // Build an XML document directly from a LINQ query's results
        private static void LinqToXml(List<Book> books)
        {
            var xmlCatalog = new XElement("Catalog",
                from b in books
                select new XElement("Book",
                    new XAttribute("id", b.BookId),
                    new XElement("Title", b.Title),
                    new XElement("Year", b.Year),
                    new XElement("Genre", b.Genre)));

            Console.WriteLine("Books, rendered as XML:");
            Console.WriteLine(xmlCatalog);

            // Querying back OUT of an XElement uses the exact same LINQ syntax as querying
            //   any other collection, XElement/XDocument implement IEnumerable<T> too.
            var dystopianTitles = from book in xmlCatalog.Elements("Book")
                                   where book.Element("Genre")?.Value == "Dystopian"
                                   select book.Element("Title")?.Value;

            Console.WriteLine($"{Environment.NewLine}Dystopian titles, queried back out of the XML:");
            foreach (string title in dystopianTitles) Console.WriteLine($" - {title}");
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
