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
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

#pragma warning disable S125 // Commented code permitted in lessons
namespace CSharp.Ch07.Supplemental._09.ConcurrentCollections
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * The collections in System.Collections and System.Collections.Generic (List<T>, Dictionary<TKey,TValue>,
         *   Queue<T>, Stack<T>, etc.) were built to be fast, not thread-safe. Reading and writing one of them
         *   from multiple threads at the same time, without your own locking, can corrupt the collection's
         *   internal state or throw ("Collection was modified" is a common symptom).
         *
         * .NET's System.Collections.Concurrent namespace provides thread-safe versions of the common
         *   collection types, safe to read and write from multiple threads at once with no locking of your own:
         *
         * - ConcurrentDictionary<TKey, TValue>  Thread-safe version of Dictionary<TKey, TValue>
         * - ConcurrentQueue<T>                  Thread-safe version of Queue<T> (first-in, first-out)
         * - ConcurrentStack<T>                  Thread-safe version of Stack<T> (last-in, first-out)
         * - ConcurrentBag<T>                    Thread-safe, unordered collection (fastest when order
         *                                          genuinely doesn't matter)
         * - BlockingCollection<T>               Wraps one of the above (ConcurrentQueue<T> by default) and
         *                                          adds blocking/bounding: a consumer can wait for an item
         *                                          to become available instead of polling
         *
         * Except for ConcurrentDictionary, these implement IProducerConsumerCollection<T>, which centers on
         *   two methods:
         * - TryAdd / TryTake     "try" versions that return false instead of throwing if the operation can't
         *                          complete right now (an empty collection has nothing to TryTake, for example)
         *
         * These collections don't eliminate the NEED to think about concurrency, they eliminate the need to
         *   write your OWN locking code around the collection itself. You can still get logically incorrect
         *   results if you write code that assumes something hasn't changed between two separate calls
         *   (checking Count, then adding, for example, is still not atomic even on a ConcurrentDictionary).
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                // Demonstrate ConcurrentDictionary
                UsingConcurrentDictionary();
                GenericFunctions.Pause();

                // Demonstrate ConcurrentQueue
                UsingConcurrentQueue();
                GenericFunctions.Pause();

                // Demonstrate ConcurrentStack
                UsingConcurrentStack();
                GenericFunctions.Pause();

                // Demonstrate ConcurrentBag
                UsingConcurrentBag();
                GenericFunctions.Pause();

                // Demonstrate BlockingCollection (producer/consumer)
                UsingBlockingCollection();
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
                GenericFunctions.Pause(final: true);
            }
        }
        #endregion

        #region Lesson Methods
        // Demonstrate ConcurrentDictionary: many threads updating the same keys at once
        private static void UsingConcurrentDictionary()
        {
            Console.WriteLine("Ten threads each incrementing a shared counter for five keys...");

            var wordCounts = new ConcurrentDictionary<string, int>();
            string[] words = ["apple", "banana", "cherry", "date", "elderberry"];

            Parallel.For(0, 10, i =>
            {
                foreach (string word in words)
                {
                    // AddOrUpdate: if the key doesn't exist yet, add it with the given value;
                    //   if it does exist, update it using the given function - all atomically,
                    //   no separate "check if it exists, then add or update" needed.
                    wordCounts.AddOrUpdate(word, 1, (key, existingValue) => existingValue + 1);
                }
            });

            foreach (var pair in wordCounts)
            {
                Console.WriteLine($"{pair.Key}: {pair.Value} (expected 10)");
            }
        }

        // Demonstrate ConcurrentQueue: multiple producers enqueue, multiple consumers dequeue
        private static void UsingConcurrentQueue()
        {
            Console.WriteLine("Five producer threads enqueue, five consumer threads dequeue...");

            var queue = new ConcurrentQueue<int>();
            int totalDequeued = 0;

            Parallel.Invoke(
                () => Parallel.For(0, 5, producer =>
                {
                    for (int i = 0; i < 20; i++) queue.Enqueue(producer * 100 + i);
                }),
                () => Parallel.For(0, 5, consumer =>
                {
                    // Consumers race the producers, so a consumer might briefly find the queue
                    //   empty even though more items are still coming. TryDequeue simply returns
                    //   false in that case rather than throwing or blocking.
                    for (int i = 0; i < 20; i++)
                    {
                        while (!queue.TryDequeue(out _))
                        {
                            Thread.Sleep(1);
                        }
                        Interlocked.Increment(ref totalDequeued);
                    }
                })
            );

            Console.WriteLine($"Total items dequeued: {totalDequeued} (expected 100)");
        }

        // Demonstrate ConcurrentStack: same idea as the queue, but last-in, first-out
        private static void UsingConcurrentStack()
        {
            Console.WriteLine("Pushing and popping from multiple threads at once...");

            var stack = new ConcurrentStack<int>();

            Parallel.For(0, 100, stack.Push);

            int poppedCount = 0;
            Parallel.For(0, 100, _ =>
            {
                if (stack.TryPop(out _)) Interlocked.Increment(ref poppedCount);
            });

            Console.WriteLine($"Items popped: {poppedCount} (expected 100), items remaining: {stack.Count} (expected 0)");
        }

        // Demonstrate ConcurrentBag: unordered, but still fully thread-safe
        private static void UsingConcurrentBag()
        {
            Console.WriteLine("Ten threads each adding ten items to a shared bag...");

            var bag = new ConcurrentBag<int>();

            Parallel.For(0, 10, i =>
            {
                for (int j = 0; j < 10; j++) bag.Add(i * 10 + j);
            });

            Console.WriteLine($"Bag contains {bag.Count} items (expected 100)");
        }

        // Demonstrate BlockingCollection: a real producer/consumer pattern, where the consumer
        //   blocks (waits) instead of polling when nothing is available yet
        private static void UsingBlockingCollection()
        {
            Console.WriteLine("A producer adds items on its own schedule; a consumer waits for each one...");

            // By default, BlockingCollection<T> wraps a ConcurrentQueue<T> (first-in, first-out)
            using var collection = new BlockingCollection<int>();

            var producer = Task.Run(() =>
            {
                for (int i = 1; i <= 5; i++)
                {
                    Console.WriteLine($"Producing item {i}...");
                    collection.Add(i);
                    Thread.Sleep(500);
                }

                // CompleteAdding() tells any consumer using GetConsumingEnumerable() that no more
                //   items are coming, once the buffered items are consumed, the enumerable ends
                //   instead of blocking forever waiting for one more that will never arrive.
                collection.CompleteAdding();
            });

            var consumer = Task.Run(() =>
            {
                // GetConsumingEnumerable() blocks between items until one is available, or until
                //   CompleteAdding() has been called and the collection is empty.
                foreach (int item in collection.GetConsumingEnumerable())
                {
                    Console.WriteLine($"Consumed item {item}...");
                }
            });

            Task.WaitAll(producer, consumer);
            Console.WriteLine("Producer and consumer both finished.");
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
