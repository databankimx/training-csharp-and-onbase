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
using System.Threading;
using CSharp.Ch06.Supplemental._01.NamedVersusAnonymousDelegates.Models.Objects;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch06.Supplemental._01.NamedVersusAnonymousDelegates
{
    // Default class for console executable
    internal static class Program
    {
        #region Chapter Notes
        /*
         * Here is a site with some good information on anonymous methods
         * // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/anonymous-methods
         */
        #endregion

        #region Main Executable Method
        // Presence of Main() method renders the class runnable
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                CallDelegates();
                GenericFunctions.Pause();

                CombineDelegates();
                GenericFunctions.Pause();

                StaticAndInstanceDelegates();
                GenericFunctions.Pause();

                CovarianceAndContravariance();
                GenericFunctions.Pause();

                ThreadDelegate();
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
        #region Global for CallDelegates
        // Declare a delegate.
        private delegate void Printer(string data);
        #endregion

        // Illustrate calling a named versus an anonymous delegate
        private static void CallDelegates()
        {
            // An anonymous method is simply a block of code assigned to a delegate
            // Instantiate the delegate type using an anonymous method.
            Printer p = Console.WriteLine;

            // Results from the anonymous delegate call.
            p("The delegate using an anonymous method was called.");
            GenericFunctions.Pause();

            // A named method is declared separately and then assigned to the delegate
            // The delegate instantiation using a named method "DoWork".
            p = DoWork;

            // Results from the old style delegate call.
            p("The delegate using a named method was called.");
        }

        #region Helper Functions for CallDelegates
        // A method associated with a named delegate.
        private static void DoWork(string data)
        {
            Console.WriteLine(data);
        }
        #endregion

        #region Global for CombineDelegates
        // Declare a delegate.
        private delegate void Step(string data);
        #endregion

        // Demonstrate that a delegate is a data type and can be added/subtracted
        private static void CombineDelegates()
        {
            // Here is an example of adding two delegates together
            //   Note: These must be of the same type (e.g. Step)
            // We'll cover this more in the lesson on multicast delegates
            Step one = StepOne;
            Step two = StepTwo;
            Step combined = one + two;
            combined("Test");
            // You can also subtract
            Step truncated = combined - one;
            truncated("Test");
        }

        #region Helper Functions for CombineDelegates
        // Method for first combined delegate
        private static void StepOne(string one)
        {
            Console.Write(one + " ");
        }

        // Method for second combined delegate
        private static void StepTwo(string two)
        {
            Console.WriteLine(two);
        }
        #endregion

        // This method uses delegates defined in Person.cs
        // Illustrate difference between static and instance delegates
        private static void StaticAndInstanceDelegates()
        {
            var alice = new Person { Name = "Alice" };
            var bob = new Person { Name = "Bob" };

            // Make Alice's InstanceMethod variable refer to her own GetName method.
            alice.InstanceMethod = alice.GetName;
            alice.StaticMethod = Person.StaticName;

            // Make Bob's InstanceMethod variable refer to Alice's GetName method.
            bob.InstanceMethod = alice.GetName;
            bob.StaticMethod = Person.StaticName;

            // Demonstrate the methods.
            Console.WriteLine("Alice's InstanceMethod returns: " + alice.InstanceMethod());
            Console.WriteLine("Bob's InstanceMethod returns: " + bob.InstanceMethod());
            Console.WriteLine("Alice's StaticMethod returns: " + alice.StaticMethod());
            Console.WriteLine("Bob's StaticMethod returns: " + bob.StaticMethod());
        }

        #region Globals for CovarianceAndContravariance
        // A delegate that returns a Person.
        // Here, we are using a Func (predefined delegate), because the method will return a data type
        #pragma warning disable S1450 // Allow unused private fields
        #pragma warning disable S125 // Allow commented code
        private static Func<Person> returnPersonMethod;
        // This is equivalent to the following:

        // private static delegate Person returnPersonDelegate();
        // private static returnPersonDelegate returnPersonMethod;

        // A delegate that takes an Employee as a parameter.
        // Here, we are using an Action (predefined delegate), because the method will return null
        private static Action<Employee> employeeParameterMethod;
        // This is equivalent to the following:
        // private static delegate void employeeParameterDelegate(Employee employee);
        // private static employeeParameterDelegate employeeParameterMethod;
        #pragma warning restore S125
        #pragma warning restore S1450
        #endregion

        // Demonstrate co- and contravariance
        private static void CovarianceAndContravariance()
        {
            // COVARIANCE:
            //   Allows a method that returns a derived class to be assigned to a delegate that returns a base class

            // CONTRAVARIANCE:
            //   Allows a method with a parameter of a base class to be assigned to a delegate with a parameter of a derived class

            // Set ReturnPersonMethod = ReturnEmployee.
            // Covariance allows this because ReturnPersonDelegate
            // returns a Person and an Employee is a kind (subclass) of Person.
            returnPersonMethod = ReturnEmployee;

            // Set EmployeeParameterMethod = PersonParameter.
            // Contravariance allows this because EmployeeParameterDelegate
            // takes an Employee as a parameter and an Employee is a kind of Person.
            // In other words, when you invoke the delegate's method you will
            // pass it an Employee and an Employee is a kind of Person so
            // PersonParameter can handle it.
            employeeParameterMethod = PersonParameter;

            // I can use the delegated returnPersonMethod to create an object
            var person = returnPersonMethod();
            // Notice that even though it implements the ReturnEmployee method,
            //   it is a Person (defined by the delegate at compile time), so this line would be an error
            // `employeeParameterMethod(person);`
            PersonParameter(person);
            // However, when I check its type name, it returns Employee (defined by the method assigned to the delegate at runtime)
            Console.WriteLine($"'person' is a(n) [{person.GetType().Name}] named [{person.Name}]");

            // In order to use the employeeParameterMethod I need an object that is an employee at compile time
            var employee = new Employee();
            employeeParameterMethod(employee);
            Console.WriteLine($"'employee' is a(n) [{employee.GetType().Name}] named [{employee.Name}]");
        }

        #region Helper Functions for CovarianceAndContravariance
        // Create a new Employee instance
        private static Employee ReturnEmployee()
        {
            return new Employee();
        }

        // A method that takes a Person as a parameter.
        private static void PersonParameter(Person person)
        {
            person.Name = "John Smith";
        }
        #endregion

        // Illustrate passing a delegate to a thread
        private static void ThreadDelegate()
        {
            // A thread takes a delegate method and executes it in a new thread
            // Here, we are setting an anonymous method
            var thread = new Thread(delegate ()
            {
                Thread.Sleep(1000);
                Console.WriteLine("Step 1...");
            });
            thread.Start();
            Console.WriteLine("Step 2...");
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
