/*
 * Warning!
 *
 * This is the unedited code downloaded from the Textbook publisher
 * This code does not follow coding standards or best practices and may have bugs/errors
 *
 * Downloaded From:
 * https://media.wiley.com/product_ancillary/94/11186120/DOWNLOAD/MCSD%20Certification%20Code%20and%20Test%20Questions.zip
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContinuationsApp
{
    class Program
    {
        static void Main(string[] args)
        {

            Task step1Task = Task.Run(() => Step1());
            Task step2Task = Task.Run(() => Step2());
            Task step3Task = Task.Factory.ContinueWhenAny(
                new Task[] { step1Task, step2Task },
                (previousTask) => Step3());

            step3Task.Wait();
        }

        static void Step1()
        {
            Console.WriteLine("Step1");
        }
        static void Step2()
        {
            Console.WriteLine("Step2");
        }
        static void Step3()
        {
            Console.WriteLine("Step3");
        }
    }
}

//Task step3Task = Task.Factory.ContinueWhenAll(
//        new Task[] { step1Task, step2Task },
//        (previousTask) => Step3());
////Task step3Task = Task.WhenAll(step1Task, step2Task).ContinueWith((previousTask) => Step3());
//Task step3Task = Task.WhenAny(step1Task, step2Task).ContinueWith((previousTask) => Step3());
