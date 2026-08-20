/*
 * Warning!
 *
 * This is the unedited* code downloaded from the Textbook publisher
 * This code does not follow coding standards or best practices and may have bugs/errors
 *
 * Downloaded From:
 * https://media.wiley.com/product_ancillary/94/11186120/DOWNLOAD/MCSD%20Certification%20Code%20and%20Test%20Questions.zip
 */

/*
 * *Instructor Note:
 *
 * Because the book code was badly broken, I have split this project into two parts
 * The "BarrierSample" class is now a separate project
 *                                                                      -SWM
 */

/*
 * *Migration Note:
 *
 * A second, real bug was found and fixed here during the 2026 migration: the odd-numbered
 * participants' first continuation called barrier.RemoveParticipant() then "return;", intending
 * to skip the second continuation's barrier.SignalAndWait(tokenSource.Token) call. That worked
 * correctly in the sibling BarrierSample project (a single un-chained lambda), but does NOT
 * work here: "return;" only ends the FIRST ContinueWith's own delegate, completing it normally
 * (RanToCompletion), it doesn't cancel the antecedent task or prevent the SECOND, separately-
 * chained ContinueWith from running. TaskContinuationOptions.NotOnCanceled doesn't help either,
 * since the first continuation never enters the Canceled state, it completes normally either way.
 * The result was that every odd-numbered task would still reach the second continuation and call
 * SignalAndWait() again after already calling RemoveParticipant(), throwing
 * InvalidOperationException silently (unobserved, since nothing awaits these continuations).
 * Fixed with a per-iteration "stillParticipating" flag the first continuation can set to tell the
 * second continuation not to proceed, see the flag and the added check in the second
 * ContinueWith below.
 */

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BarrierWithTasks
{
    class Program
    {
        static void Main(string[] args)
        {
            var participants = 5;

            // We create a CancellationTokenSource to be able to initiate the cancellation
            var tokenSource = new CancellationTokenSource();
            // We create a barrier object to use it for the rendez-vous points
            var barrier = new Barrier(participants,
                b =>
                {
                    Console.WriteLine("{0} paricipants are at rendez-vous point {1}.",
                                    b.ParticipantCount,
                                    b.CurrentPhaseNumber);
                });

            for (int i = 0; i < participants; i++)
            {
                var localCopy = i;
                var stillParticipating = true;
                Task.Delay(1000 * localCopy + 1, tokenSource.Token)
                    .ContinueWith(_ =>
                    {
                        Console.WriteLine("Task {0} left point A!", localCopy);
                        Thread.Sleep(1000 * localCopy + 1); // Do some "work"
                        if (localCopy % 2 == 0)
                        {
                            Console.WriteLine("Task {0} arrived at point B!", localCopy);
                            barrier.SignalAndWait(tokenSource.Token);
                        }
                        else
                        {
                            Console.WriteLine("Task {0} changed its mind and went back!", localCopy);
                            barrier.RemoveParticipant();
                            stillParticipating = false;
                        }
                    }, TaskContinuationOptions.NotOnCanceled)
                    .ContinueWith(_ =>
                    {
                        // A task that removed itself in the previous continuation must not reach
                        // the barrier again, see the Migration Note above for why "return;" alone
                        // in the previous continuation wasn't enough to guarantee that.
                        if (!stillParticipating) return;

                        Thread.Sleep(1000 * localCopy + 1);
                        Console.WriteLine("Task {0} arrived at point C!", localCopy);
                        barrier.SignalAndWait(tokenSource.Token);
                    }, TaskContinuationOptions.NotOnCanceled);
            }

            Console.WriteLine("Main thread is waiting for {0} tasks!", barrier.ParticipantsRemaining - 1);
            Console.WriteLine("Press enter to cancel!");
            Console.ReadLine();
            if (barrier.CurrentPhaseNumber < 2)
            {
                tokenSource.Cancel();
                Console.WriteLine("We canceled the operation!");
            }
            else
            {
                Console.WriteLine("Too late to cancel!");
            }
            Console.WriteLine("Main thread is done!");
            Console.ReadLine();
        }
    }
}
