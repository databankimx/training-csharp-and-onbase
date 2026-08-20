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
using System.Linq;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch06.Supplemental._02.LambdaExpressions
{
    // Default class for console executable
    #pragma warning disable S125    // Allow commented code for lessons
    #pragma warning disable IDE0039 // Not using local functions, since this is a lesson on delegates and lambdas
    internal static class Program
    {
        #region Chapter Notes
        /*
         * Lambda Expressions (or Anonymous Functions) are the latest version of passable code blocks.
         * These supersede the older Anonymous Methods and are the most-preferred method for passing inline code.
         * Understanding these will enhance your ability to utilize LINQ for performance-optimized queries.
         *
         * New Concept (Lambda Operator: =>)
         * - https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-operator
         * - Has the same precedence as the assignment operator (=) and is right-associative.
         * - As the lambda operator in a lambda expression, it separates the input variables from the lambda body.
         * - In an expression body definition, it separates a member name from the member implementation.
         *
         * While we're on the topic, here's an example of using an expression body:
         *    public override string ToString() => $"{fname} {lname}".Trim();
         *
         * That's the same as this method definition
         *    public override string ToString() { return $"{fname} {lname}".Trim(); }
         *
         * Look at configuration sections to see great examples of using this in place of {return base["<name>"]}
         *    for properties.
         *
         * Syntax:
         * ([parameters]) => {<code block>}
         *
         * Types of Lambdas:
         * - Expression Lambda
         *   Syntax:    () => expression;
         *      The "expression" is a single C# statement to be executed by the delegate
         *
         * - Statement Lambda
         *   Syntax:    () => { series of expressions }
         *      The "series of expressions" denotes multiple C# statements enclosed in curly-braces
         *
         * - Async Lambda
         *   Although we won't touch asynchronous functionality until next chapter, we should note that the async/await keywords
         *     can apply to a lambda. We will hold off on an example until we get to chapter 7
         *   Syntax:    async () => { series of expressions }
         *
         * Follow-Up:
         *   https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/lambda-expressions
         */
        #endregion

        #region Private Methods
        private delegate void TestDelegate(string s);
#pragma warning restore S125
        #endregion

        #region Main Executable Method
        // Presence of Main() method renders the class runnable
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                ExpressionAndStatementLambdas();
                GenericFunctions.Pause();

                LambdaExamples();
                GenericFunctions.Pause();

                DelegateEvolution();
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
        // Example expression lambdas
        private static void ExpressionAndStatementLambdas()
        {
            // An expression lambda executes a single C# statement
            Action note = () => Console.WriteLine("1: Executed a parameterless expression lambda...");
            note();

            // It can accept a parameter
            Action<string> noteWithParameter = message => Console.WriteLine(message);
            noteWithParameter("2: Executed an expression lambda with a parameter...");

            // In fact, it can accept any number of parameters
            Action<string, int> noteWithMultipleParameters = (message, number) => Console.WriteLine($"{number}: {message}");
            noteWithMultipleParameters("Executed an expression lambda with multiple parameters...", 3);

            // An expression lambda can also return a value
            Func<float, float> square = x => x * x;
            float num = 2;
            Console.WriteLine($"{num} squared is {square(num)}");

            // A statement lambda differs from an expression lambda only in having multiple statements in a block
            Func<float, int, float> xToTheY = (x, y) =>
            {
                float z = x;
                for (int i = 1; i < y; i++) z *= x;
                return z;
            };
            int exp = 3;
            Console.WriteLine($"{num} to the power {exp} = {xToTheY(num, exp)}");
        }

        // Simple example of the use of a lambda operator
        private static void LambdaExamples()
        {
            string[] words = ["cherry", "apple", "blueberry"];

            // Use method syntax to apply a lambda expression to each element  
            // of the words array.   
            int shortestWordLength = words.Min(w => w.Length);
            Console.WriteLine(shortestWordLength);

            // Compare the following code that uses query syntax.  
            // Get the lengths of each word in the words array.  
            var query = from w in words select w.Length;

            // Apply the Min method to execute the query and get the shortest length.  
            int shortestWordLength2 = query.Min();
            Console.WriteLine(shortestWordLength2);

            GenericFunctions.Pause();

            string[] digits = ["zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine"];

            Console.WriteLine("Example that uses a lambda expression:");

            // Here, we're using a lambda with multiple arguments
            var shortDigits = digits.Where((digit, index) => digit.Length < index);
            foreach (var sD in shortDigits)
            {
                Console.WriteLine(sD);
            }
        }

        // Delegate method for DelegateEvolution example
        private static void M(string s)
        {
            Console.WriteLine(s);
        }

        // Illustrates the evolution od delegate calls over C#'s history
        private static void DelegateEvolution()
        {
            // Original delegate syntax required 
            // initialization with a named method.
            var testDelA = new TestDelegate(M);

            // C# 2.0: A delegate can be initialized with
            // inline code, called an "anonymous method." This
            // method takes a string as an input parameter.
            TestDelegate testDelB = delegate (string s) { Console.WriteLine(s); };

            // C# 3.0. A delegate can be initialized with
            // a lambda expression (anonymous function). The lambda also takes a string
            // as an input parameter (x). The type of x is inferred by the compiler.
            TestDelegate testDelC = (x) => { Console.WriteLine(x); };

            // In C# 6+, we can just declare this as a method group
            TestDelegate testDelD = Console.WriteLine;

            // Invoke the delegates.
            testDelA("Hello. My name is M and I write lines.");
            testDelB("That's nothing. I'm anonymous and ");
            testDelC("I'm a famous author.");
            testDelD("And now I write more sleekly");
        }
        #endregion
    }
    #pragma warning restore IDE0039
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
