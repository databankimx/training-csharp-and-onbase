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
using System.Diagnostics;
#endregion

namespace CSharp.Ch06.Supplemental._09.Closures
{
    internal static class Program
    {
        #region Main
        // Demonstrates the concept of closures in C#. A closure is a function that
        // "closes over" its surrounding state, meaning it can remember and access
        // variables from its enclosing scope.
        private static void Main()
        {
            try
            {
                Console.WriteLine("=== Example 1: First-Class Functions ===");
                // Three spellings, same idea: a function stored in a variable.
                // No borrowed state, no memory, nothing up its sleeve.
                static string GreetLocal(string name) => $"Hello, {name}!";

                Func<string, string> greetDelegate = delegate (string name)
                {
                    return $"Hello, {name}!";
                };

                Func<string, string> greetLambda = name => $"Hello, {name}!";

                Console.WriteLine(GreetLocal("Ada"));
                Console.WriteLine(greetDelegate("Ada"));
                Console.WriteLine(greetLambda("Ada"));
                Console.WriteLine();

                Console.WriteLine("=== Example 2: Free Variable ===");
                // Same shape as Example 1, but "salutation" is declared outside
                // the function and borrowed by it. Watch what happens when we
                // change it after the closure already exists.
                string salutation = "Hello";
                Func<string, string> greetWithSalutation = delegate (string name)
                {
                    return $"{salutation}, {name}!";
                };

                Console.WriteLine(greetWithSalutation("Ada"));
                salutation = "Howdy";
                Console.WriteLine(greetWithSalutation("Alan")); // salutation changed, and the closure noticed
                Console.WriteLine();

                Console.WriteLine("=== Example 3: The Closure ===");
                // MakeGreeter returns a function that closes over both
                // "salutation" and "greetingCount". Watch the counter survive
                // even though MakeGreeter itself finished running ages ago.
                var politeGreeter = MakeGreeter("Hello");

                Console.WriteLine(politeGreeter("Ada"));
                _ = MakeGreeter("Hi"); // This call is ignored, but it proves the closure is not a singleton.
                Console.WriteLine(politeGreeter("Alan"));
                politeGreeter = MakeGreeter("Greetings"); // This call replaces the old closure with a new one
                Console.WriteLine(politeGreeter("Grace"));
                Console.WriteLine();

                Console.WriteLine("=== Example 3b: Two Closures, Two Memories ===");
                // A second call to MakeGreeter creates a brand new
                // "greetingCount", proving the closure owns its own copy of
                // the free variables rather than sharing one global stash.
                var curtGreeter = MakeGreeter("Yo");

                Console.WriteLine(curtGreeter("Ada"));
                Console.WriteLine(politeGreeter("Grace")); // politeGreeter keeps its own count
                Console.WriteLine(curtGreeter("Alan"));
                Console.WriteLine();

                Console.WriteLine("=== Example 4: The Modified Closure Gotcha ===");
                // Unlike MakeGreeter, these two closures are NOT built by a
                // factory. They share the same outer "exp" variable directly,
                // so they are not two independent memories, they are two
                // windows onto the same memory.
                int exp = 2;
                Func<int, int> square = x => (int)Math.Pow(x, exp);

                Console.WriteLine($"square(2) = {square(2)}"); // 4, as expected

                exp = 3;
                Func<int, int> cube = x => (int)Math.Pow(x, exp);

                Console.WriteLine($"cube(2) = {cube(2)}"); // 8, also as expected

                Console.WriteLine($"square(2) = {square(2)}  <- WRONG, but deserved"); // 8, not 4
                Console.WriteLine();

                Console.WriteLine("=== Example 5: Access to Modified Closure ===");
                // Same disease as Example 4, wearing a loop for a costume.
                // Every lambda below closes over the same "i", not a copy
                // of whatever "i" happened to be when it was created.
                var actions = new List<Action>();

                for (int i = 0; i < 3; i++)
                {
                    actions.Add(() => Console.WriteLine($"Broken: {i}"));
                }

                foreach (var action in actions)
                {
                    action(); // Prints 3, 3, 3. The loop already finished by the time these run.
                }
                Console.WriteLine();

                // The fix: give each iteration its own private variable
                // instead of letting every closure share the loop's.
                var fixedActions = new List<Action>();

                for (int i = 0; i < 3; i++)
                {
                    int local = i;
                    fixedActions.Add(() => Console.WriteLine($"Fixed: {local}"));
                }

                foreach (var action in fixedActions)
                {
                    action(); // Prints 0, 1, 2, like a reasonable person would expect.
                }
            }
            catch (Exception ex)
            {
                while (ex != null)
                {
                    Console.WriteLine($"\n{ex.GetType().Name}: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
                    ex = ex.InnerException;
                }
            }
            finally
            {
                if (!Debugger.IsAttached)
                {
                    Console.WriteLine("\nDone!\nPress any key to exit...");
                    Console.ReadKey();
                }
            }
        }
        #endregion

        #region Closure Factory
        // Returns a function that remembers "salutation" and "greetingCount"
        // long after this method has returned. That's the whole trick.
        //
        // Each call to MakeGreeter gets its own private "greetingCount",
        // which is exactly what Examples 4 and 5 are missing, and exactly
        // why Examples 4 and 5 go wrong.
        private static Func<string, string> MakeGreeter(string salutation)
        {
            var greetingCount = 0;

            Func<string, string> greet = delegate (string name)
            {
                greetingCount++;
                return $"{salutation}, {name}! (greeting #{greetingCount})";
            };

            return greet;
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
