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
using System.Collections;
using System.Collections.Generic;
using CSharp.Ch09.WorkingWithDataCollections.Models.Collections;
using CSharp.Ch09.WorkingWithDataCollections.Models.Objects;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch09.WorkingWithDataCollections
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * Chapter 9 is "Working with Data". This lesson covers the first, foundational half of
         *     that: arrays and collections, the in-memory structures almost every other data
         *     technique in this chapter (ADO.NET, serialization, file I/O) ultimately reads into
         *     or writes out of. The remaining sections of the chapter (ADO.NET/Entity Framework,
         *     File I/O, and Serialization) are each covered in their own Supplemental project.
         *
         * Arrays vs. Collections vs. Custom Collections:
         * - Arrays: fixed-size, contiguous, fastest option, but the size can't change after creation.
         * - System.Collections: the ORIGINAL, non-generic collection types (ArrayList, Hashtable,
         *     Queue, Stack). They store everything as plain "object", meaning boxing/unboxing for
         *     value types and no compile-time type safety at all. Largely superseded.
         * - System.Collections.Generic: the modern, type-safe versions (List<T>, Dictionary<TKey,
         *     TValue>, Queue<T>, Stack<T>, HashSet<T>, SortedList<TKey,TValue>, LinkedList<T>).
         *     This is what you should reach for by default today.
         * - Custom Collections: when a built-in collection's storage is fine but you need to
         *     enforce a rule the built-in type doesn't (a maximum size, a required sort order, a
         *     validation check on every add), you wrap or implement a collection interface
         *     yourself. See BoundedCollection<T> in this project for a concrete example.
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                #region Arrays
                // Demonstrate Array class utility methods (Sort, Reverse, BinarySearch, etc.)
                ArrayUtilityMethods();
                GenericFunctions.Pause();
                #endregion

                #region System.Collections
                // Demonstrate ArrayList
                UsingArrayList();
                GenericFunctions.Pause();

                // Demonstrate Hashtable
                UsingHashtable();
                GenericFunctions.Pause();

                // Demonstrate the non-generic Queue and Stack
                UsingLegacyQueueAndStack();
                GenericFunctions.Pause();
                #endregion

                #region System.Collections.Generic
                // Demonstrate List<T>
                UsingListT();
                GenericFunctions.Pause();

                // Demonstrate Dictionary<TKey, TValue>
                UsingDictionary();
                GenericFunctions.Pause();

                // Demonstrate the generic Queue<T> and Stack<T>
                UsingGenericQueueAndStack();
                GenericFunctions.Pause();

                // Demonstrate HashSet<T> and set operations
                UsingHashSet();
                GenericFunctions.Pause();

                // Demonstrate SortedList<TKey, TValue>
                UsingSortedList();
                GenericFunctions.Pause();

                // Demonstrate LinkedList<T>
                UsingLinkedList();
                GenericFunctions.Pause();
                #endregion

                #region Custom Collections
                // Demonstrate a custom collection enforcing a business rule (a maximum capacity)
                UsingCustomBoundedCollection();
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

        #region Lesson Methods
        #region Arrays
        // Demonstrate Array class utility methods
        private static void ArrayUtilityMethods()
        {
            int[] numbers = [5, 2, 8, 1, 9, 3];
            Console.WriteLine($"Original: {string.Join(", ", numbers)}");

            Array.Sort(numbers);
            Console.WriteLine($"Array.Sort(numbers): {string.Join(", ", numbers)}");

            Array.Reverse(numbers);
            Console.WriteLine($"Array.Reverse(numbers): {string.Join(", ", numbers)}");

            // Array.BinarySearch requires the array to already be sorted ASCENDING to work correctly
            Array.Sort(numbers);
            int foundIndex = Array.BinarySearch(numbers, 8);
            Console.WriteLine($"{Environment.NewLine}Array.BinarySearch(numbers, 8): found at index {foundIndex}");

            int indexOfThree = Array.IndexOf(numbers, 3);
            Console.WriteLine($"Array.IndexOf(numbers, 3): index {indexOfThree}");

            var copy = new int[numbers.Length];
            Array.Copy(numbers, copy, numbers.Length);
            Console.WriteLine($"{Environment.NewLine}Array.Copy(numbers, copy, numbers.Length): {string.Join(", ", copy)}");

            Array.Resize(ref copy, 3);
            Console.WriteLine($"Array.Resize(ref copy, 3): {string.Join(", ", copy)}");

            bool hasLargeNumber = Array.Exists(numbers, n => n > 8);
            Console.WriteLine($"{Environment.NewLine}Array.Exists(numbers, n => n > 8): {hasLargeNumber}");

            int[] evenNumbers = Array.FindAll(numbers, n => n % 2 == 0);
            Console.WriteLine($"Array.FindAll(numbers, n => n % 2 == 0): {string.Join(", ", evenNumbers)}");

            Console.Write($"{Environment.NewLine}Array.ForEach(numbers, ...): ");
            Array.ForEach(numbers, n => Console.Write($"{n} "));
            Console.WriteLine();

            Array.Clear(numbers, 0, numbers.Length);
            Console.WriteLine($"{Environment.NewLine}Array.Clear(numbers, 0, numbers.Length): [{string.Join(", ", numbers)}]");
        }
        #endregion

        #region System.Collections
        #pragma warning disable S1192 // "String literals should not be duplicated" - this is a demo, so duplication is intentional
        // Demonstrate ArrayList: the original, non-generic dynamic array
        private static void UsingArrayList()
        {
            var mixedList = new ArrayList
            {
                "A string",
                42,
                new Book("1984", "George Orwell", 1949)
            };

            // Note: ArrayList stores everything as plain "object", so mixing wildly different
            //   types like this is perfectly legal, and perfectly easy to do by accident.
            Console.WriteLine("ArrayList contents (mixed types, no compile-time safety):");
            foreach (object item in mixedList)
            {
                Console.WriteLine($" - {item} (actual type: {item.GetType().Name})");
            }
        }

        // Demonstrate Hashtable: the original, non-generic key/value store
        private static void UsingHashtable()
        {
            var byAuthor = new Hashtable
            {
                ["Orwell"] = new Book("1984", "George Orwell", 1949),
                ["Bradbury"] = new Book("Fahrenheit 451", "Ray Bradbury", 1953)
            };

            Console.WriteLine("Hashtable contents:");
            foreach (DictionaryEntry entry in byAuthor)
            {
                Console.WriteLine($" - {entry.Key}: {entry.Value}");
            }
        }

        // Demonstrate the non-generic Queue and Stack
        private static void UsingLegacyQueueAndStack()
        {
            var queue = new Queue();
            queue.Enqueue("First in line");
            queue.Enqueue("Second in line");
            queue.Enqueue("Third in line");
            Console.WriteLine($"Queue.Dequeue(): {queue.Dequeue()}");

            var stack = new Stack();
            stack.Push("Pushed first");
            stack.Push("Pushed second");
            stack.Push("Pushed third");
            Console.WriteLine($"Stack.Pop(): {stack.Pop()}");
        }
        #endregion

        #region System.Collections.Generic
        // Demonstrate List<T>
        private static void UsingListT()
        {
            var books = new List<Book>
            {
                new("1984", "George Orwell", 1949),
                new("Brave New World", "Aldous Huxley", 1932),
                new("Fahrenheit 451", "Ray Bradbury", 1953)
            };

            books.Sort((a, b) => a.Year.CompareTo(b.Year));
            Console.WriteLine("List<Book>, sorted by year:");
            foreach (var book in books) Console.WriteLine($" - {book}");

            var found = books.Find(b => b.Author == "Ray Bradbury");
            Console.WriteLine($"{Environment.NewLine}Find(b => b.Author == \"Ray Bradbury\"): {found}");

            bool anyPre1940 = books.Exists(b => b.Year < 1940);
            Console.WriteLine($"Exists(b => b.Year < 1940): {anyPre1940}");
        }

        // Demonstrate Dictionary<TKey, TValue>
        private static void UsingDictionary()
        {
            var byTitle = new Dictionary<string, Book>
            {
                ["1984"] = new Book("1984", "George Orwell", 1949),
                ["Fahrenheit 451"] = new Book("Fahrenheit 451", "Ray Bradbury", 1953)
            };

            if (byTitle.TryGetValue("1984", out var book))
            {
                Console.WriteLine($"TryGetValue(\"1984\"): {book}");
            }

            Console.WriteLine($"ContainsKey(\"Dune\"): {byTitle.ContainsKey("Dune")}");

            Console.WriteLine($"{Environment.NewLine}All entries:");
            foreach (KeyValuePair<string, Book> pair in byTitle)
            {
                Console.WriteLine($" - {pair.Key} => {pair.Value}");
            }
        }

        // Demonstrate the generic Queue<T> and Stack<T>
        private static void UsingGenericQueueAndStack()
        {
            var queue = new Queue<Book>();
            queue.Enqueue(new Book("1984", "George Orwell", 1949));
            queue.Enqueue(new Book("Brave New World", "Aldous Huxley", 1932));
            Console.WriteLine($"Queue<Book>.Dequeue(): {queue.Dequeue()}");

            var stack = new Stack<Book>();
            stack.Push(new Book("1984", "George Orwell", 1949));
            stack.Push(new Book("Brave New World", "Aldous Huxley", 1932));
            Console.WriteLine($"Stack<Book>.Pop(): {stack.Pop()}");
        }

        // Demonstrate HashSet<T> and set operations
        private static void UsingHashSet()
        {
            var scienceFiction = new HashSet<string> { "1984", "Brave New World", "Fahrenheit 451", "Dune" };
            var frequentlyBannedBooks = new HashSet<string> { "Fahrenheit 451", "Brave New World", "Beloved" };

            var bannedSciFi = new HashSet<string>(scienceFiction);
            bannedSciFi.IntersectWith(frequentlyBannedBooks);
            Console.WriteLine($"IntersectWith (in both sets): {string.Join(", ", bannedSciFi)}");

            var everyTitle = new HashSet<string>(scienceFiction);
            everyTitle.UnionWith(frequentlyBannedBooks);
            Console.WriteLine($"UnionWith (in either set): {string.Join(", ", everyTitle)}");

            var sciFiOnly = new HashSet<string>(scienceFiction);
            sciFiOnly.ExceptWith(frequentlyBannedBooks);
            Console.WriteLine($"ExceptWith (in scienceFiction but not frequentlyBannedBooks): {string.Join(", ", sciFiOnly)}");
        }

        // Demonstrate SortedList<TKey, TValue>
        private static void UsingSortedList()
        {
            var byYear = new SortedList<int, string>
            {
                { 1953, "Fahrenheit 451" },
                { 1932, "Brave New World" },
                { 1949, "1984" }
            };

            // Even though the entries above were added out of chronological order,
            //   SortedList<TKey, TValue> always enumerates in key order automatically.
            Console.WriteLine("SortedList<int, string>, entries added out of order but enumerated by year:");
            foreach (var pair in byYear)
            {
                Console.WriteLine($" - {pair.Key}: {pair.Value}");
            }
        }

        // Demonstrate LinkedList<T>
        private static void UsingLinkedList()
        {
            var timeline = new LinkedList<string>();
            var middleNode = timeline.AddFirst("Brave New World (1932)");
            timeline.AddAfter(middleNode, "1984 (1949)");
            timeline.AddLast("Fahrenheit 451 (1953)");
            timeline.AddFirst("The Time Machine (1895)");

            Console.WriteLine("LinkedList<string> timeline:");
            foreach (var entry in timeline)
            {
                Console.WriteLine($" - {entry}");
            }
        }
        #endregion

        #region Custom Collections
        // Demonstrate a custom collection enforcing a rule no built-in collection enforces
        private static void UsingCustomBoundedCollection()
        {
            var recentReads = new BoundedCollection<Book>(maxCapacity: 3)
            {
                new("1984", "George Orwell", 1949),
                new("Brave New World", "Aldous Huxley", 1932),
                new("Fahrenheit 451", "Ray Bradbury", 1953)
            };

            Console.WriteLine($"Count: {recentReads.Count} / MaxCapacity: {recentReads.MaxCapacity}");
            foreach (var book in recentReads) Console.WriteLine($" - {book}");

            Console.WriteLine($"{Environment.NewLine}Attempting to add a fourth book...");
            try
            {
                recentReads.Add(new Book("Dune", "Frank Herbert", 1965));
            }
            catch (DatabankException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        #pragma warning restore S1192
        #endregion
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
