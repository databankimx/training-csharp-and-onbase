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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TPLApp
{
    class Program
    {

        const int NUMBER_OF_ITERATIONS = 32;

        static void Main(string[] args)
        {


            // We are using Stopwatch to time the code
            Stopwatch sw = Stopwatch.StartNew();

            // Run the method
            RunParallelForCorrected();

            // Print the time it took to run the application.
            Console.WriteLine("We're done in {0}ms!", sw.ElapsedMilliseconds);

            Utils.CommonFunctions.WaitForKeyWhehDebugging();
        }

        static void RunSequencial()
        {

            double result = 0d;

            // Here we call same method several times. 
            for (int i = 0; i < NUMBER_OF_ITERATIONS; i++)
            {
                result += Utils.CommonFunctions.DoIntensiveCalculations();
            }

            // Print the result
            Console.WriteLine("The result is {0}", result);

        }

        static void RunParallelFor()
        {

            double result = 0d;

            // Here we call same method several times in parallel. 
            Parallel.For(0, NUMBER_OF_ITERATIONS, i => {
                result += Utils.CommonFunctions.DoIntensiveCalculations();
            });

            // Print the result
            Console.WriteLine("The result is {0}", result);

        }


        static void RunTasks()
        {

            double result = 0d;

            Task[] tasks = new Task[NUMBER_OF_ITERATIONS];
            // We create one task per iteration. 
            for (int i = 0; i < NUMBER_OF_ITERATIONS; i++)
            {
                tasks[i] = Task.Run(() => result += Utils.CommonFunctions.DoIntensiveCalculations());
            }

            // We wait for the tasks to finish
            Task.WaitAll(tasks);

            //// We collect the results
            //foreach (var task in tasks) {
            //    result += task.Result;
            //}

            // Print the result
            Console.WriteLine("The result is {0}", result);
        }

        static void RunTasksCorrected()
        {

            double result = 0d;

            Task<double>[] tasks = new Task<double>[NUMBER_OF_ITERATIONS];

            // We create one task per iteration. 
            for (int i = 0; i < NUMBER_OF_ITERATIONS; i++)
            {
                tasks[i] = Task.Run(() => Utils.CommonFunctions.DoIntensiveCalculations());
            }

            // We wait for the tasks to finish
            Task.WaitAll(tasks);

            // We collect the results
            foreach (var task in tasks)
            {
                result += task.Result;
            }

            // Print the result
            Console.WriteLine("The result is {0}", result);
        }

        static void RunParallelForCorrected()
        {

            double result = 0d;

            // Here we call same method several times. 
            //for (int i = 0; i < NUMBER_OF_ITERATIONS; i++) 
            Parallel.For(0, NUMBER_OF_ITERATIONS,
                // Interim resul = 0d
                () => 0d,

                //    result += Utils.CommonFunctions.DoIntensiveCalculations();
                (i, state, interimResult) => interimResult + Utils.CommonFunctions.DoIntensiveCalculations(),

                // Final step after the calculations 
                // we add the result to the final result
                (lastInterimResult) => result += lastInterimResult
            );
            // Print the result
            Console.WriteLine("The result is {0}", result);
        }
    }
}
