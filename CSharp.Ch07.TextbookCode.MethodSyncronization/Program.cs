/*
 * Warning!
 *
 * This is the unedited code downloaded from the Textbook publisher
 * This code does not follow coding standards or best practices and may have bugs/errors
 *
 * Downloaded From:
 * https://media.wiley.com/product_ancillary/94/11186120/DOWNLOAD/MCSD%20Certification%20Code%20and%20Test%20Questions.zip
 */

/*
 * *Migration Note:
 *
 * A real, program-breaking bug was found and fixed here during the 2026 migration: a
 * "WaitHandle.WaitAny(new WaitHandle[] { ce.WaitHandle });" call originally sat right
 * after the CountdownEvent was created, but BEFORE any of the four threads that
 * eventually signal it were spawned. Since ce.WaitHandle only becomes signaled once
 * all four Signal() calls happen (which never could, since the threads that call them
 * hadn't been created yet), this blocked Main() forever right there, the four threads
 * were never even started and the whole program hung on launch. Removed, since the
 * intended wait already happens correctly via ce.Wait() at the end of Main(), this
 * line was almost certainly a stray leftover, not something meant to run before the
 * threads exist. See LectureNotes.md for further discussion, including why all four
 * spawned threads actually end up contending for the exact same lock.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading;

namespace MethodSyncronization
{
    class Program
    {
        static void Main(string[] args)
        {

            var handle = new EventWaitHandle(true, EventResetMode.AutoReset);
            var handle2 = new EventWaitHandle(true, EventResetMode.ManualReset);
            var handle3 = new AutoResetEvent(true);

            CountdownEvent ce = new CountdownEvent(4);

            var instance = new SingleThreaded();
            new Thread(() => {

                instance.OneCallInstance1();
                ce.Signal();
            }).Start();

            new Thread(() => {

                lock (instance)
                {
                    Console.WriteLine("Main");
                    Console.ReadLine();
                    Console.WriteLine("Main");

                }
                ce.Signal();
            }).Start();

            new Thread(() => {

                instance.OneCallInstance2();
                ce.Signal();
            }).Start();

            new Thread(() => {

                instance.OneCallLockThis();
                ce.Signal();
            }).Start();

            //new Thread(() => {

            //    SingleThreaded.OneCallStatic1();
            //}).Start();

            //new Thread(() => {

            //    SingleThreaded.OneCallStatic2();
            //}).Start();

            ce.Wait();
            //Console.ReadLine();
        }

    }

    class SingleThreaded
    {

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void OneCallInstance1()
        {

            Console.WriteLine("OneCallInstance1");
            Console.ReadLine();
            Console.WriteLine("OneCallInstance1");
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void OneCallInstance2()
        {
            Console.WriteLine("OneCallInstance2");
            Console.ReadLine();
            Console.WriteLine("OneCallInstance2");
        }

        public void OneCallLockThis()
        {

            lock (this)
            {
                Console.WriteLine("OneCallLockThis");
                Console.ReadLine();
                Console.WriteLine("OneCallLockThis");
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void OneCallStatic1()
        {
            Console.WriteLine("OneCallStatic1");
            Console.ReadLine();
            Console.WriteLine("OneCallStatic1");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void OneCallStatic2()
        {
            Console.WriteLine("OneCallStatic2");
            Console.ReadLine();
            Console.WriteLine("OneCallStatic2");
        }
    }
}
