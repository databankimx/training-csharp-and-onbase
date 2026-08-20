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

#region Directives
using System;
using System.Threading;
using System.Threading.Tasks;
using CSharp.Ch07.Supplemental._07.Locking.Models;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch07.Supplemental._07.Locking
{
    internal static class Program
    {
        #region Chapter Notes (Locking)
        /*
         * Locking is implementing a mutual exclusion to ensure only one process at a time can access a resource.
         *
         * Some examples of Locking:
         *
         * - Monitor:
         *   Synchronize object access to reference types only
         *   METHODS        Note: All methods are static
         *   - Enter()      Acquires exclusive lock on specified object - enter ready queue and await if already locked
         *   - Exit()       Release lock on specified object
         *   - IsEntered()  True if the current thread holds the lock
         *   - TryEnter()   Attempts to acquire exclusive lock on specified object
         *     Note: Methods below can only be called when holding the lock
         *   - Pulse()      Notifies thread in waiting queue that the state has changed - move from waiting to ready queue
         *   - PulseAll()   Notifies all threads in waiting queue that the state has changed - move from waiting to ready queue
         *   - Wait()       Release lock and enter waiting queue until another thread pulses the monitor
         *
         *   * Alert!       It's important to always provide an exit in the event of an exception to avoid a deadlock
         *
         * - Mutex:
         *   Short for "mutual exclusion"
         *   Synchronizes access (including inter-process) to a resource, blocking threads until they own the mutes
         *   METHODS                Note: Only including some of the pertinent methods here
         *   - Close()              Releases all resources held by the WaitHandle
         *   - WaitOne()            Wait for the mutex to be available and take control
         *   - ReleaseMutex()       Release control and signal available
         *
         * - Semaphore:
         *   Limits the number of threads that can simultaneously access a resource
         *   METHODS                Note: Only including some of the pertinent methods here
         *   - WaitOne()            Wait for the semaphore to have at least one available slot and take control
         *   - Release()            Release control of slot and signal available
         *   - Release(n)           Release control of n slots and signal available
         *
         * Other types of locking (not demonstrated in the code examples below) include:
         *
         * - Interlock
         * - ReaderWriterLock
         * - ReaderWriterLockSlim
         * - and others
         *
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                // Demonstrate use of a Monitor for locking
                UsingMonitor();
                Nap(5); // Allow tasks to complete
                GenericFunctions.Pause();

                // Demonstrate use of a Mutex for locking
                UsingMutex();
                Nap(7); // Allow tasks to complete
                GenericFunctions.Pause();

                // Demonstrate use of a Semaphore for locking
                UsingSemaphore();
                Nap(5); // Allow tasks to complete
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

        #region Globals for Mutex/Semaphore Examples
        // Shared object with Mutex and/or Semaphore
        private static Thing excludedObject;
        #endregion

        #region Lesson Methods
        // Demonstrate use of a Monitor for locking
        private static void UsingMonitor()
        {
            var syncObject = new Thing();

            for (int i = 0; i < 2; i++)
            {
                int iCopy = i;
                Task.Run(() =>
                {
                    Console.WriteLine($"Start task {iCopy}...");
                    Monitor.Enter(syncObject);
                    try
                    {
                        Console.WriteLine($"Object locked by task {iCopy}...");
                        syncObject.Id = iCopy + 1;
                        Console.WriteLine($"Object's ID is now {syncObject.Id}...");
                        Nap(2);
                    }
                    finally
                    {
                        Monitor.Exit(syncObject);
                        Console.WriteLine($"Object released by task {iCopy}...");
                    }
                });
            }
        }

        // Demonstrate use of a Mutex for locking
        private static void UsingMutex()
        {
            excludedObject = new Thing
            {
                Name = null,
                Id = 0,
                // The mutex is a member of the protected object
                Mutex = new Mutex()
            };

            int numThreads = 3;

            for (int i = 0; i < numThreads; i++)
            {
                var newThread = new Thread(UseResourceWithMutex) { Name = $"Thread {i + 1}" };
                newThread.Start();
            }
        }

        // Consume a resource using a mutex to protect access
        private static void UseResourceWithMutex()
        {
            int numIterations = 1;

            for (int i = 0; i < numIterations; i++)
            {
                Console.WriteLine($"{Thread.CurrentThread.Name} is requesting the mutex...");
                if (excludedObject.Mutex.WaitOne(5000))
                {
                    try
                    {
                        Console.WriteLine($"{Thread.CurrentThread.Name} has taken control of the mutex...");
                        // Simulate work
                        excludedObject.Name = Thread.CurrentThread.Name;
                        Nap(2);
                        Console.WriteLine($"{Thread.CurrentThread.Name} has completed work on the shared resource...");
                    }
                    finally
                    {
                        excludedObject.Mutex.ReleaseMutex();
                        Console.WriteLine($"{Thread.CurrentThread.Name} has relinquished control of the mutex...");
                    }
                }
                else
                {
                    Console.WriteLine($"{Thread.CurrentThread.Name} has failed to acquire control of the mutex...");
                }
            }
        }

        // Demonstrate use of a Semaphore for locking
        private static void UsingSemaphore()
        {
            Console.WriteLine("Main thread generates Thing with semaphore allowing three thread to access...");

            excludedObject = new Thing
            {
                Name = null,
                Id = 0,
                // The semaphore is a member of the protected object
                SemaphorePool = new Semaphore(0, 3)
            };

            UseResourceWithSemaphore();
        }

        // Threads to consume a resource using a semaphore to protect access
        private static void UseResourceWithSemaphore()
        {
            for (int i = 0; i < 5; i++)
            {
                var thread = new Thread(ResourceWorkWithSemaphore);
                thread.Start(i + 1);
            }

            Nap(1);

            Console.WriteLine("Main thread releases 3 semaphore positions...");
            excludedObject.SemaphorePool.Release(3);

            Console.WriteLine("Main thread exits...");
        }

        // Consume a resource using a semaphore to protect access
        private static void ResourceWorkWithSemaphore(object num)
        {
            Console.WriteLine($"Thread {num} requesting semaphore access...");
            excludedObject.SemaphorePool.WaitOne();

            Console.WriteLine($"Thread {num} enters the semaphore...");
            // Simulate work
            Nap(1);

            Console.WriteLine($"Thread {num} releases the semaphore...");
            Console.WriteLine($"Thread {num} previous semaphore count {excludedObject.SemaphorePool.Release()}...");
        }
        #endregion

        #region Helper Functions
        // Pause for specified number of seconds to simulate work
        private static void Nap(int seconds)
        {
            Thread.Sleep(seconds * 1000);
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
