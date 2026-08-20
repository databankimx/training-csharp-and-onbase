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
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

#pragma warning disable S125 // Allow commented out code in lessons
namespace CSharp.Ch07.Supplemental._06.Barriers
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * A refresher on the info from the previous chapter notes on barriers
         * 
         * - Barriers
         *
         *   - Barrier
         *     Provides a means of grouping threads to rejoin at specified conditions
         *     METHODS
         *     - AddParticipant()       Adds a process to the barrier
         *     - AddParticipants()      Adds multiple processes to the barrier
         *     - RemoveParticipant()    Removes a process from the barrier
         *     - RemoveParticipants()   Removes multiple processes from the barrier
         *     - SignalAndWait()        Indicates that a process has reached the barrier and will await the others
         *     PROPERTIES
         *     - CurrentPhaseNumber     Identifies the barrier's current phase
         *     - ParticipantCount       Number of processes participating in the barrier
         *     - ParticipantsRemaining  Number of participating processes that have not yet reached the barrier
         *
         */
        #endregion

        #region Private Globals
        // Number of participant processes
        private const int Participants = 5;
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                UseBarrier();
                GenericFunctions.Pause();

                //UseBarrierWithCancel();
                //GenericFunctions.Pause();
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

        #region Chapter Methods
        // Demonstrate the use of a Barrier
        private static void UseBarrier()
        {
            // When declaring a barrier, add one extra participant for the main thread
            var barrier = new Barrier(Participants + 1,
                b =>
                {
                    // Here, we count one less than the actual count, since we aren't concerned with the main thread
                    // I have added one to the phase ID to count from 1 instead of 0
                    Console.WriteLine($"{b.ParticipantCount - 1} participants are at rendezvous point {b.CurrentPhaseNumber + 1}");
                });

            BarrierProcess(barrier);
        }

        // Process to perform on participants in the barrier
        private static void BarrierProcess(Barrier barrier)
        {
            for (int i = 0; i < Participants; i++)
            {
                int localCopy = i;

                Task.Run(() =>
                {
                    Console.WriteLine($"Task {localCopy} left point A...");
                    Nap(localCopy + 1);
                    if (localCopy % 2 == 0)
                    {
                        Console.WriteLine($"Task {localCopy} arrived at point B...");
                        barrier.SignalAndWait();

                        // Only tasks that are still registered participants after point B may
                        //   continue on to signal again at point C. A task that called
                        //   RemoveParticipant() (the else branch below) has permanently left the
                        //   barrier, calling SignalAndWait() again afterward would exceed the
                        //   barrier's remaining ParticipantCount and throw
                        //   InvalidOperationException. Originally, both branches fell through to
                        //   a single shared SignalAndWait() call here, which hit exactly that
                        //   problem for every odd-numbered task, silently, since these are
                        //   fire-and-forget Task.Run() calls with nothing observing their
                        //   exceptions.
                        Nap(Participants - localCopy);
                        Console.WriteLine($"Task {localCopy} arrived at point C...");
                        barrier.SignalAndWait();
                    }
                    else
                    {
                        Console.WriteLine($"Task {localCopy} signaled but returned to point A...");
                        barrier.RemoveParticipant();
                    }
                });
            }

            Console.WriteLine($"Main thread is waiting for {barrier.ParticipantsRemaining - 1} participants...\n");

            barrier.SignalAndWait(); // Main thread waiting at the first phase
            Console.WriteLine("\nMain thread signaled phase B...\n");
            barrier.SignalAndWait(); // Main thread waiting at the second phase
            Console.WriteLine("\nMain thread signaled phase C...\n");

            // This pause is to allow the remaining threads that were blocked at B by the main thread to complete the journey
            Nap(Participants);
            Console.WriteLine("\nMain thread complete.\n");
        }

        // Demonstrate the use of a Barrier with cancellation
        #pragma warning disable S1144 // Although unused, this function is a lesson example and is not intended to be called in this demo
        private static void UseBarrierWithCancel()
        {
            var tokenSource = new CancellationTokenSource();

            // When declaring a barrier, add one extra participant for the main thread
            var barrier = new Barrier(Participants + 1,
                b =>
                {
                    // Here, we count one less than the actual count, since we aren't concerned with the main thread
                    // I have added one to the phase ID to count from 1 instead of 0
                    Console.WriteLine($"{b.ParticipantCount - 1} participants are at rendezvous point {b.CurrentPhaseNumber + 1}");
                });

            BarrierProcessWithCancel(barrier, tokenSource);
        }
        #pragma warning restore S1144

        // Process to perform on participants in the barrier
        private static void BarrierProcessWithCancel(Barrier barrier, CancellationTokenSource tokenSource)
        {
            for (int i = 0; i < Participants; i++)
            {
                int localCopy = i;

                Task.Run(() =>
                {
                    try
                    {
                        Console.WriteLine($"Task {localCopy} left point A...");
                        Nap(1);
                        if (localCopy % 2 == 0)
                        {
                            Console.WriteLine($"Task {localCopy} arrived at point B...");
                            barrier.SignalAndWait(tokenSource.Token);

                            // See the matching note in BarrierProcess(): a task that instead
                            //   takes the RemoveParticipant() branch below has permanently left
                            //   the barrier and must not call SignalAndWait() again.
                            Nap(1);
                            Console.WriteLine($"Task {localCopy} arrived at point C...");
                            barrier.SignalAndWait(tokenSource.Token);
                        }
                        else
                        {
                            Console.WriteLine($"Task {localCopy} signaled but returned to point A...");
                            barrier.RemoveParticipant();
                        }
                    }
                    // Suppressing exception that occurs if canceled while task is running
                    catch (OperationCanceledException)
                    {
                        // Do nothing
                    }
                });
            }

            Console.WriteLine($"Main thread is waiting for {barrier.ParticipantsRemaining - 1} participants...\n");
            Console.WriteLine("Press <ENTER> at any time to cancel...\n");
            Console.ReadLine();

            if (barrier.CurrentPhaseNumber < 1)
            {
                tokenSource.Cancel();
                Console.WriteLine("\nOperation canceled...\n");
            }
            else
            {
                Console.WriteLine("Too late to cancel...");
            }

            Nap(Participants);
            Console.WriteLine("\nMain thread complete\n");
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
