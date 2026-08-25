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
         * LINQ (Language Integrated Query) is really TWO different syntaxes compiling down
         *   to the exact same thing. Query syntax (from x in collection where ... select ...)
         *   reads like SQL and is what most people reach for first; method syntax
         *   (collection.Where(...).Select(...)) uses ordinary extension methods and chains
         *   more naturally with everything else in C#. Every query-syntax example below has
         *   a matching method-syntax example doing the identical thing, worth comparing
         *   directly, since the compiler translates the former into the latter anyway.
         *
         * All examples run against a small, shared in-memory Authors/Books dataset (see
         *   GetAuthors()/GetBooks() below), deliberately including one author (Aldous
         *   Huxley... plus a fourth, unpublished author) with different numbers of books,
         *   specifically to make the join/grouping examples show something interesting.
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                #region Query Expression Syntax
                FilterWithQuerySyntax();
                GenericFunctions.Pause();

                OrderWithQuerySyntax();
                GenericFunctions.Pause();

                ProjectWithQuerySyntax();
                GenericFunctions.Pause();

                JoinWithQuerySyntax();
                GenericFunctions.Pause();

                OuterJoinWithQuerySyntax();
                GenericFunctions.Pause();

                GroupWithQuerySyntax();
                GenericFunctions.Pause();
                #endregion

                #region Method-Based Syntax
                FilterWithMethodSyntax();
                GenericFunctions.Pause();

                OrderWithMethodSyntax();
                GenericFunctions.Pause();

                ProjectWithMethodSyntax();
                GenericFunctions.Pause();

                JoinWithMethodSyntax();
                GenericFunctions.Pause();

                GroupWithMethodSyntax();
                GenericFunctions.Pause();

                AggregateFunctions();
                GenericFunctions.Pause();

                FirstAndLast();
                GenericFunctions.Pause();

                Concatenation();
                GenericFunctions.Pause();

                SkipAndTake();
                GenericFunctions.Pause();

                Distinct();
                GenericFunctions.Pause();
                #endregion

                #region LINQ to XML
                BooksToXml();
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

        #region Shared Sample Data
        // A fourth, unpublished author is included deliberately, so
        //   OuterJoinWithQuerySyntax() has an author with zero books to show
        private static List<Author> GetAuthors()
        {
            return
            [
                new Author { AuthorId = 1, Name = "George Orwell", Country = "United Kingdom" },
                new Author { AuthorId = 2, Name = "Ray Bradbury", Country = "United States" },
                new Author { AuthorId = 3, Name = "Aldous Huxley", Country = "United Kingdom" },
                new Author { AuthorId = 4, Name = "Unpublished Author", Country = "Canada" }
            ];
        }

        private static List<Book> GetBooks()
        {
            #pragma warning disable S1192 // Keeping literal in lesson
            return
            [
                new Book { Title = "1984", AuthorId = 1, Year = 1949, Genre = "Dystopian", Price = 9.99m },
                new Book { Title = "Animal Farm", AuthorId = 1, Year = 1945, Genre = "Satire", Price = 7.99m },
                new Book { Title = "Fahrenheit 451", AuthorId = 2, Year = 1953, Genre = "Dystopian", Price = 8.99m },
                new Book { Title = "The Martian Chronicles", AuthorId = 2, Year = 1950, Genre = "Science Fiction", Price = 10.99m },
                new Book { Title = "Brave New World", AuthorId = 3, Year = 1932, Genre = "Dystopian", Price = 9.49m }
            ];
            #pragma warning restore S1192
        }
        #endregion

        #region Query Expression Syntax
        // Filtering: where
        private static void FilterWithQuerySyntax()
        {
            var books = GetBooks();

            var dystopianBooks = from b in books
                                  where b.Genre == "Dystopian"
                                  select b;

            Console.WriteLine("Dystopian books (query syntax):");
            foreach (var book in dystopianBooks)
            {
                Console.WriteLine($" - {book.Title} ({book.Year})");
            }
        }

        // Ordering: orderby, multiple keys
        private static void OrderWithQuerySyntax()
        {
            var books = GetBooks();

            var orderedBooks = from b in books
                                orderby b.Genre, b.Year descending
                                select b;

            Console.WriteLine("Books ordered by Genre, then Year descending (query syntax):");
            foreach (var book in orderedBooks)
            {
                Console.WriteLine($" - {book.Genre}: {book.Title} ({book.Year})");
            }
        }

        // Projection: select into an anonymous type
        private static void ProjectWithQuerySyntax()
        {
            var books = GetBooks();

            var titleAndYear = from b in books
                                select new { b.Title, b.Year };

            Console.WriteLine("Title/Year projection (query syntax):");
            foreach (var book in titleAndYear)
            {
                Console.WriteLine($" - {book.Title}, {book.Year}");
            }
        }

        // Joining: inner join
        private static void JoinWithQuerySyntax()
        {
            var books = GetBooks();
            var authors = GetAuthors();

            var booksWithAuthors = from b in books
                                    join a in authors on b.AuthorId equals a.AuthorId
                                    select new { b.Title, AuthorName = a.Name };

            Console.WriteLine("Books joined to their Author (query syntax):");
            foreach (var book in booksWithAuthors)
            {
                Console.WriteLine($" - {book.Title} by {book.AuthorName}");
            }
        }

        // Joining: outer join (every Author, even ones with zero Books)
        private static void OuterJoinWithQuerySyntax()
        {
            var books = GetBooks();
            var authors = GetAuthors();

            // "join ... into" creates a group join, DefaultIfEmpty() is what turns it into an
            //   OUTER join specifically, without it, an Author with no matching Books would
            //   simply be dropped from the results entirely (same as JoinWithQuerySyntax()'s
            //   inner join above).
            var authorsWithBookCount = from a in authors
                                        join b in books on a.AuthorId equals b.AuthorId into authorBooks
                                        select new { a.Name, BookCount = authorBooks.Count() };

            Console.WriteLine("Every Author, with their book count, including zero (query syntax):");
            foreach (var author in authorsWithBookCount)
            {
                Console.WriteLine($" - {author.Name}: {author.BookCount} book(s)");
            }
        }

        // Grouping
        private static void GroupWithQuerySyntax()
        {
            var books = GetBooks();

            var booksByGenre = from b in books
                                group b by b.Genre;

            Console.WriteLine("Books grouped by Genre (query syntax):");
            foreach (var genreGroup in booksByGenre)
            {
                Console.WriteLine($" - {genreGroup.Key} ({genreGroup.Count()}):");
                foreach (var book in genreGroup)
                {
                    Console.WriteLine($"     {book.Title}");
                }
            }
        }
        #endregion

        #region Method-Based Syntax
        // Filtering: Where()
        private static void FilterWithMethodSyntax()
        {
            var books = GetBooks();

            var dystopianBooks = books.Where(b => b.Genre == "Dystopian");

            Console.WriteLine("Dystopian books (method syntax):");
            foreach (var book in dystopianBooks)
            {
                Console.WriteLine($" - {book.Title} ({book.Year})");
            }
        }

        // Ordering: OrderBy().ThenByDescending()
        private static void OrderWithMethodSyntax()
        {
            var books = GetBooks();

            var orderedBooks = books.OrderBy(b => b.Genre).ThenByDescending(b => b.Year);

            Console.WriteLine("Books ordered by Genre, then Year descending (method syntax):");
            foreach (var book in orderedBooks)
            {
                Console.WriteLine($" - {book.Genre}: {book.Title} ({book.Year})");
            }
        }

        // Projection: Select()
        private static void ProjectWithMethodSyntax()
        {
            var books = GetBooks();

            var titleAndYear = books.Select(b => new { b.Title, b.Year });

            Console.WriteLine("Title/Year projection (method syntax):");
            foreach (var book in titleAndYear)
            {
                Console.WriteLine($" - {book.Title}, {book.Year}");
            }
        }

        // Joining: Join()
        private static void JoinWithMethodSyntax()
        {
            var books = GetBooks();
            var authors = GetAuthors();

            var booksWithAuthors = books.Join(authors,
                b => b.AuthorId,
                a => a.AuthorId,
                (b, a) => new { b.Title, AuthorName = a.Name });

            Console.WriteLine("Books joined to their Author (method syntax):");
            foreach (var book in booksWithAuthors)
            {
                Console.WriteLine($" - {book.Title} by {book.AuthorName}");
            }
        }

        // Grouping: GroupBy()
        private static void GroupWithMethodSyntax()
        {
            var books = GetBooks();

            var booksByGenre = books.GroupBy(b => b.Genre);

            Console.WriteLine("Books grouped by Genre (method syntax):");
            foreach (var genreGroup in booksByGenre)
            {
                Console.WriteLine($" - {genreGroup.Key} ({genreGroup.Count()}):");
                foreach (var book in genreGroup)
                {
                    Console.WriteLine($"     {book.Title}");
                }
            }
        }

        // Aggregate Functions: Count/Sum/Average/Min/Max
        private static void AggregateFunctions()
        {
            var books = GetBooks();

            int count = books.Count(b => b.Genre == "Dystopian");
            decimal sum = books.Sum(b => b.Price);
            decimal average = books.Average(b => b.Price);
            decimal min = books.Min(b => b.Price);
            decimal max = books.Max(b => b.Price);

            Console.WriteLine($"Dystopian book count: {count}");
            Console.WriteLine($"Total price of all books: {sum:C}");
            Console.WriteLine($"Average book price: {average:C}");
            Console.WriteLine($"Cheapest book: {min:C}");
            Console.WriteLine($"Most expensive book: {max:C}");
        }

        // first and last
        private static void FirstAndLast()
        {
            var books = GetBooks();

            var firstDystopian = books.First(b => b.Genre == "Dystopian");
            var lastDystopian = books.Last(b => b.Genre == "Dystopian");

            // FirstOrDefault()/LastOrDefault() return null (for a reference type) instead of
            //   throwing InvalidOperationException when nothing matches, worth reaching for
            //   these by default unless a missing match genuinely IS exceptional here.
            var firstFantasy = books.FirstOrDefault(b => b.Genre == "Fantasy");

            Console.WriteLine($"First Dystopian book: {firstDystopian.Title}");
            Console.WriteLine($"Last Dystopian book: {lastDystopian.Title}");
            Console.WriteLine($"First Fantasy book: {firstFantasy?.Title ?? "(none found)"}");
        }

        // Concatenation
        private static void Concatenation()
        {
            var books = GetBooks();

            var recentReleases = new List<Book>
            {
                new() { Title = "Klara and the Sun", AuthorId = 5, Year = 2021, Genre = "Science Fiction", Price = 14.99m }
            };

            var allBooks = books.Concat(recentReleases);

            Console.WriteLine("Original books concatenated with recent releases:");
            foreach (var book in allBooks)
            {
                Console.WriteLine($" - {book.Title} ({book.Year})");
            }
        }

        // Skip and Take
        private static void SkipAndTake()
        {
            var books = GetBooks().OrderBy(b => b.Title).ToList();

            // A common pagination pattern: Skip() past earlier pages, Take() the page size
            var secondPage = books.Skip(2).Take(2);

            Console.WriteLine("Books, alphabetically, \"page 2\" (skip 2, take 2):");
            foreach (var book in secondPage)
            {
                Console.WriteLine($" - {book.Title}");
            }
        }

        // Distinct
        private static void Distinct()
        {
            var books = GetBooks();

            var genres = books.Select(b => b.Genre).Distinct();

            Console.WriteLine("Distinct genres:");
            foreach (var genre in genres)
            {
                Console.WriteLine($" - {genre}");
            }
        }
        #endregion

        #region LINQ to XML
        // Building an XML document from a LINQ query's results
        private static void BooksToXml()
        {
            var books = GetBooks();

            var xmlBooks = new XElement("Books",
                from b in books
                select new XElement("Book",
                    new XAttribute("year", b.Year),
                    new XElement("Title", b.Title),
                    new XElement("Genre", b.Genre)));

            Console.WriteLine("Books as XML:");
            Console.WriteLine(xmlBooks);
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
