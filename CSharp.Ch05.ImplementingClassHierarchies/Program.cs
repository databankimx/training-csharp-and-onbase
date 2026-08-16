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
using System.Text;
using CSharp.Ch05.ImplementingClassHierarchies.Models.Enumerations;
using CSharp.Ch05.ImplementingClassHierarchies.Models.Interfaces;
using CSharp.Ch05.ImplementingClassHierarchies.Models.Objects;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

#pragma warning disable S125    // Code in comments is part of the lesson
#pragma warning disable IDE0028 // Don't simplify collections in lesson
#pragma warning disable IDE0090 // Don't simplify 'new(...)' in lesson
#pragma warning disable IDE0300 // Don't simplify collections in lesson
namespace CSharp.Ch05.ImplementingClassHierarchies
{
    // Default class for console executable
    internal static class Program
    {
        #region Constants
        // Location for code sample files
        private const string CodeSamples = @"Textbook Resources.zip\MCSD Certification Code and Test Questions\05\Chapter5\";

        // Location for study guide (cheat sheet)
        private const string CheatSheet = @"Textbook Resources.zip\MCSD Certification Toolkit Cheat Sheets & Key Terms\";

        // Chapter number
        private const int Chapter = 5;

        // Chapter topic
        private const string Topic = "implementing class hierarchies";
        #endregion

        #region Main Executable Method
        // Presence of Main() method renders the class runnable
        private static void Main()
        {
            try
            {
                #region Chapter Notes
                /*
                 * TERMINOLOGY:
                 *
                 * Base Class (also Parent Class or Superclass):
                 *      This is a class from which other classes are derived through inheritance
                 *      It is possible for this also to be a derived or child class of another parent
                 *
                 * Derived Class (also Child Class or Subclass):
                 *      This is a class that inherits from a parent or base class
                 *      When implemented, the class inherits all code from the parent class
                 *
                 * Descendant:
                 *      This can refer to immediate child classes of a parent class
                 *      or it can refer to additional classes derived from those children, etc.
                 *
                 * Ancestor:
                 *      This can refer to a class's immediate parent class or that parent's parent class, etc.
                 *
                 * Sibling:
                 *      Refers to any class with which a class shares a common immediate parent
                 */
                #endregion

                #region Implementing Class Hierarchies
                // NOTE: Look at the hierarchy of the Person -> Employee classes first

                // Examples of creating Person class instances using constructors
                CodeLabInvokingConstructors();
                GenericFunctions.Pause();

                // NOTE: Look at the IStudent interface and Student/Faculty/TeachingAssistant classes

                // Demonstrate implementing an interface
                CodeLabImplementingAnInterface();
                GenericFunctions.Pause();
                #endregion

                #region Implemementing Common C# Interfaces
                // NOTE: Look at the Car and CarComparer classes

                // Using IComparable to implement sorting
                CodeLabComparingCars();
                GenericFunctions.Pause();

                // NOTE: Look at use of IEquatable in the Person class

                // Using IEquatable to compare persons
                CodeLabEquatingPersons();
                GenericFunctions.Pause();

                // NOTE: Look at the use of ICloneable in the Person class

                // Using ICloneable to close Persons
                CodeLabCloningPersons();
                GenericFunctions.Pause();

                // NOTE: Look at TreeNode and TreeEnumerator classes

                // Using IEnumerable to generate a tree
                CodeLabOrgChart();
                GenericFunctions.Pause();
                #endregion

                #region Implementing IDisposable

                /*
                                 * We've separated out IDisposable from the other common interfaces, because it is more general-purpose
                                 * We should look at IDisposable as a means of releasing memory we've used explicitly, before the
                                 *   garbage collector has a chance to identify it as releasable after its scope unravels.
                                 *
                                 *
                                 * First, it helps to understand automatic garbage collection
                                 *
                                 * - An object becomes eligible for garbage collection when it is unreachable (when all references to it
                                 *   are null or have gone out of scope).
                                 *
                                 * - Garbage collection takes place periodically though, so eligible objects may remain in memory for a while
                                 *
                                 * - When it runs, the garbage collector:
                                 *   1. Starts by marking all in-use objects as unreachable
                                 *   2. Traverses the running code. Wherever it finds a reference to an object, that object is marked reachable
                                 *   3. Checks items marked as unreachable for finalizers
                                 *      a. Whenever one exists, it is called (to perform cleanup). This is important, since children of the object
                                 *         are still reachable (from it) and otherwise will not be cleaned up until the next pass of the GC
                                 *         * non-deterministic finalization (cannot predict when a given object will be finalized)
                                 *   4. Releases the memory used by the unreachable objects
                                 *
                                 *
                                 * IDisposable allow us to set up explicit methods to release an object (and unlock any resources it is using)
                                 * The Dispose() methods generated when implementing IDisposable are called destructors
                                 *   Dispose() can be called explicitly in code or implicitly at the end of a using block
                                 *
                                 * The default syntax for a Dispose() method is:
                                 *
                                 * public void Dispose()
                                 * {
                                 *     // Release any non-managed items (and, optionally, managed ones as well)
                                 * }
                                 *
                                 * A better implementation looks like this:
                                 *
                                 * // The finalizer can explicitly call our Dispose() method
                                 * ~ClassName()
                                 * {
                                 *     Dispose(false);
                                 * }
                                 *
                                 * // This one is called explicitly in code where the class is instantiated
                                 * public void Dispose()
                                 * {
                                 *     Dispose(true);
                                 *     // Since we will have freed managed objects, we don't have to let the GC finalize this
                                 *     GC.SuppressFinalize(this);
                                 * }
                                 *
                                 * // This one is called internally within the class
                                 * protected void Dispose(bool releaseManagedObjects)
                                 * {
                                 *     // You can set a boolean member to check if Dispose has already been called
                                 *     // This makes the Dispose method safe to execute more than once
                                 *     if (disposed) return;
                                 *
                                 *     // Code here should explicitly free up unmanaged items
                                 *
                                 *     if (!releaseManagedObjects) return;
                                 *
                                 *     // Code here should explicitly free up managed items
                                 * }
                                 *
                                 * Since we included a finalizer in that design, let's explore those for a moment
                                 *
                                 *   A finalizer is indicated by a tilde (~) followed by the class name
                                 *     ~ClassName()
                                 *     {
                                 *         // Your code to clean up unmanaged resources
                                 *     }
                                 *
                                 *   You cannot explicitly call a finalizer. It is called automatically when an object's scope ends
                                 *   Unless you are performing specific code, a finalizer is not required
                                 *
                                 *   The finalizer implicitly calls the Finalize() method (inherited from System.Object),
                                 *     and acts as an implicit override of the Finalize() method like this:
                                 *
                                 * protected override void Finalize()
                                 * {
                                 *     try
                                 *     {
                                 *         // Your code to clean up unmanaged resources
                                 *     }
                                 *     finally
                                 *     {
                                 *         base.Finalize();
                                 *     }
                                 * }
                                 */

                CodeLabDispose();
                GenericFunctions.Pause();

                // Because of the elevated risk, you should not do this in production software,
                // but the following will clean up Betty
                #pragma warning disable S1215 // Allowing this as part of lesson (do not use in real-world projects)
                GC.Collect();
                #pragma warning restore S1215
                GenericFunctions.Pause();
                #endregion

                /* Real-World Example
                 * At this point, the book reviews a real-world example called "Shape Resources"
                 * I've included some of the classes for reference here
                 * The full code is available in the chapter downloads
                 * This will not be covered in lecture
                 */

                #region Program Completion
                GenericFunctions.FinishChapter(CodeSamples, CheatSheet, Chapter, Topic);
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

        #region Code Labs
        // Code Lab: Examples of creating Person class instances using constructors
        private static void CodeLabInvokingConstructors()
        {
            Console.WriteLine("Code Lab : Invoking Constructors");
            Console.WriteLine("----------------------");

            string results = "";

            // Make some Persons
            results += $"Making Person(Bea){Environment.NewLine}{Environment.NewLine}";
            #pragma warning disable S1481   // Allow unnecessary assignment of a value for lesson
            #pragma warning disable IDE0059 // Allow unnecessary assignment of a value for lesson
            var bea = new Person("Bea");

            results += $"Making Person(Al, Able){Environment.NewLine}{Environment.NewLine}";
            var al = new Person("Al", "Able");

            // Make some Employees - note the "base" keyword in the constructor
            results += $"Making Employee(Carl){Environment.NewLine}{Environment.NewLine}";
            var carl = new Employee("Carl");

            results += $"Making Employee(Deb, Dart){Environment.NewLine}{Environment.NewLine}";
            var deb = new Employee("Deb", "Dart");

            results += $"Making Employee(Ed, Eager, IT){Environment.NewLine}{Environment.NewLine}";
            var ed = new Employee("Ed", "Eager", "IT");
            #pragma warning restore IDE0059
            #pragma warning restore S1481

            Console.WriteLine(results);
        }

        // Code Lab: Demonstrate implementing an interface
        private static void CodeLabImplementingAnInterface()
        {
            // Note: Our Student class inherits from Person but also implements IStudent
            var student = new Student
            {
                FirstName = "Jim",
                LastName = "Learner",
                Courses = new List<Course>
                {
                    new Course
                    {
                        Name = "C# Development",
                        RawGrade = 85.0
                    }
                }
            };

            // Because it inherits from Person, the Student class includes the person methods as well as properties
            Console.WriteLine(student.FullName());

            // But it also implements all properties and methods from the IStudent interface
            student.PrintGrades();

            // Faculty is another descendant of person
            var faculty = new Faculty
            {
                FirstName = "Jane",
                LastName = "Teacher",
                Department = "Computer Science",
                Degree = Degree.Doctorate
            };

            // Because it inherits from Person, the Faculty class includes the person attributes
            Console.WriteLine(faculty.FullName());

            // Here's where an interface really comes in handy
            // What if we need to categorize a TA. This kind of person is both a student and a faculty member.
            // Because we can't inherit from multiple classes, we'll inherit from Faculty but implement IStudent
            var ta = new TeachingAssistant
            {
                FirstName = "Jo",
                LastName = "Midway",
                Department = "Computer Science",
                Degree = Degree.Bachelors,
                Courses = new List<Course>
                {
                    new Course
                    {
                        Name = "C# Development",
                        RawGrade = 95.0
                    }
                }
            };

            // So, via inheritance we have the attributes of a person and faculty member
            Console.WriteLine($"{ta.FullName()}");

            // As well as any methods implemented in TeachingAssistant
            Console.WriteLine(ta.Credentials());

            // And by implementing IStudent, we have the attributes of a student as well
            ta.PrintGrades();

            // Although you cannot instantiate an interface, you can declare a variable as one
            IStudent secondTa = new TeachingAssistant
            {
                FirstName = "Jake",
                LastName = "Middleton",
                Degree = Degree.Masters,
                Department = "Computer Science",
                Courses = new List<Course>
                {
                    new Course
                    {
                        Name = "C# Development",
                        RawGrade = 99.0
                    }
                }
            };

            // And you can still access the IStudent members
            secondTa.PrintGrades();

            // But even though we constructed this as a TeachingAssistant, you cannot access the members defined in TeachingAssistant
            // So this will not work
            // Console.WriteLine(secondTa.Credentials());

            // To get at that, we have to cast as the TeachingAssistant class
            Console.WriteLine(((TeachingAssistant)secondTa).Credentials());
        }

        // Code Lab: Comparing Cars
        private static void CodeLabComparingCars()
        {
            Console.WriteLine("Code Lab : Comparing Cars");
            Console.WriteLine("----------------------");

            // Let's compare a couple of class instances with identical properties
            var car1 = new Car { Year = 2014, Make = "SSC Ultimate", Model = "Aero", MaxMph = 257, Horsepower = 1183, Price = 654400m };
            var car2 = new Car { Year = 2014, Make = "SSC Ultimate", Model = "Aero", MaxMph = 257, Horsepower = 1183, Price = 654400m };

            // By default, a plain class only supports reference equality with ==, and < / >
            //     wouldn't even compile, neither is meaningful for an arbitrary reference type
            //     until you define what it means. Car overloads ==, !=, <, <=, >, and >= on top
            //     of its IComparable implementation (see the Bonus Methods region in Car.cs), so
            //     all of these now reflect value comparison instead of reference comparison.

            // This returns TRUE, car1 and car2 have equivalent Name values, and our ==
            //     operator is built on the same CompareTo() logic used just below
            Console.WriteLine($"car1 == car2:         {car1 == car2}");

            // This also returns TRUE, since our CompareTo() method will return 0, indicating that the properties are equivalent
            Console.WriteLine($"car1.CompareTo(car2): {car1.CompareTo(car2) == 0}\n\n");

            // Now, we'll use CompareTo() as intended, to sort a list of objects

            // Make some data.
            // Source: http://www.thesupercars.org/fastest-cars/fastest-cars-in-the-world-top-10-list.
            Car[] cars =
            {
                new Car { Year=2014, Make="SSC Ultimate", Model="Aero", MaxMph=257, Horsepower=1183, Price=654400m},
                new Car { Year=2014, Make="Bugatti", Model="Veyron", MaxMph=253, Horsepower=1001, Price=1700000m},
                new Car { Year=2014, Make="Saleen", Model="S7 Twin-Turbo", MaxMph=248, Horsepower=750, Price=555000m},
                new Car { Year=2014, Make="Koenigsegg", Model="CCX", MaxMph=245, Horsepower=806, Price=545568m},
                new Car { Year=2014, Make="McLaren", Model="F1", MaxMph=240, Horsepower=637, Price=970000m},
                new Car { Year=2014, Make="Ferrari", Model="Enzo", MaxMph=217, Horsepower=660, Price=670000m},
                new Car { Year=2014, Make="Jaguar", Model="XJ220", MaxMph=217, Horsepower=542, Price=650000m},
                new Car { Year=2014, Make="Pagani Zonda", Model="F", MaxMph=215, Horsepower=650, Price=667321m},
                new Car { Year=2014, Make="Lamborghini", Model="Murcielago LP640", MaxMph=211, Horsepower=640, Price=430000m},
                new Car { Year=2014, Make="Porsche", Model="Carrera GT", MaxMph=205, Horsepower=612, Price=440000m},
            };

            // Let's compare using the IComparable CompareTo method
            string position = GetPosition(cars[0].CompareTo(cars[1]));
            Console.WriteLine($"{cars[0].Name} comes {position} {cars[1].Name}");

            // Now let's use the IComparer generic compare method
            var comparer = new CarComparer();
            position = GetPosition(comparer.Compare(cars[0], cars[1]));
            Console.WriteLine($"{cars[0].Name} comes {position} {cars[1].Name}");
            comparer.SortBy = CarComparer.CompareField.MaxMph;
            Console.WriteLine();

            Console.WriteLine(@"Original Order");
            foreach (var car in cars) Console.WriteLine($@"{car.Name} - {car.MaxMph} MPH - {car.Horsepower} HP - ${car.Price}");

            Array.Sort(cars);

            Console.WriteLine($@"{Environment.NewLine}Sorted Alphabetically");
            foreach (var car in cars) Console.WriteLine($@"{car.Name} - {car.MaxMph} MPH - {car.Horsepower} HP - ${car.Price}");
        }

        // Code Lab: Equating Persons
        private static void CodeLabEquatingPersons()
        {
            var abeLincoln = new Person("Abe", "Lincoln");

            var abrahamLincoln = new Person("Abe", "Lincoln");

            // Let's see what happens when we try to compare by equivalency
            Console.WriteLine($"abeLincoln == abrahamLincoln ? {abeLincoln == abrahamLincoln}");

            // Here we are explicitly using our Equals method from the IEquatable interface
            Console.WriteLine($"abeLincoln.Equals(abrahamLincoln) ? {abeLincoln.Equals(abrahamLincoln)}");

            var people = new List<Person> {abeLincoln};

            // Here, we are leveraging our IEquatable Equals method to enable the List<T>.Contains method
            // It is a best practice to implement IEquatable on any class that you will store in a List, Dictionary, Stack, or Queue
            Console.WriteLine(people.Contains(abrahamLincoln)
                ? $"List already contains {abrahamLincoln.FirstName} {abrahamLincoln.LastName}"
                : $"Added {abrahamLincoln.FirstName} {abrahamLincoln.LastName} to list...");
        }

        // Code Lab: Cloning Persons
        private static void CodeLabCloningPersons()
        {
            // Sometimes you need to pass an object (reference type), but you do not want the original object to be modified
            // For instances like this, ICloneable creates a new instance with the same values as the original

            var ann = new Person("Ann", "Archer");
            Console.WriteLine(ann.FullName());

            var anne = ann;
            anne.FirstName = "Anne";
            // Because "anne" points to the same reference as "ann", "ann" is modified
            Console.WriteLine(ann.FullName());

            var bob = new Person("Bob", "Baker");
            Console.WriteLine(bob.FullName());

            var robert = (Person)bob.Clone();
            robert.FirstName = "Robert";
            // Because "robert" is a clone of "bob", "bob" is not modified
            Console.WriteLine(bob.FullName());
        }

        // Code Lab: Creating an enumerable tree
        private static void CodeLabOrgChart()
        {
            #pragma warning disable S1481   // Allow unnecessary assignment of a value for lesson
            #pragma warning disable IDE0059 // Allow unnecessary assignment of a value for lesson
            var president = new TreeNode("President");
            var sales = president.AddChild("VP Sales");
            var domestic = sales.AddChild("Domestic Sales");
            var domRep1 = domestic.AddChild("Dom. Rep 1");
            var domRep2 = domestic.AddChild("Dom. Rep 2");
            var international = sales.AddChild("International Sales");
            var intlRep1 = international.AddChild("Intl. Rep 1");
            var intlRep2 = international.AddChild("Intl. Rep 2");
            var ops = president.AddChild("VP Operations");
            var dev = ops.AddChild("Development");
            var dev1 = dev.AddChild("Developer 1");
            var dev2 = dev.AddChild("Developer 2");
            var ps = ops.AddChild("Professional Services");
            var eng1 = ps.AddChild("Engineer 1");
            var eng2 = ps.AddChild("Engineer 2");
            #pragma warning restore IDE0059
            #pragma warning restore S1481

            var text = new StringBuilder();
            #pragma warning disable IDE0063 // Don't simplify 'using' statement in lesson
            using (var enumerator = president.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current == null) continue;
                    string spacer = new string(' ', 4 * enumerator.Current.Depth);
                    text.Append($"{spacer}{enumerator.Current.Text}{Environment.NewLine}");
                }
                Console.WriteLine(text);
            }
            #pragma warning restore IDE0063
        }

        // Demonstrate use of IDisposable
        private static void CodeLabDispose()
        {
            // We'll dispose of Alan explicitly
            var alan = new DisposableClass {Name = "Alan"};
            alan.Dispose();

            // We'll let the GC dispose of Betty using the finalizer
            // Because this occurs at a later (non-deterministic) time, we won't see this in the console during execution
            #pragma warning disable S1481   // Allow unnecessary assignment of a value for lesson
            #pragma warning disable IDE0059  // Allow unnecessary assignment of a value for lesson
            var betty = new DisposableClass { Name = "Betty" };
            #pragma warning restore IDE0059 // Unnecessary assignment of a value
            #pragma warning restore S1481

            // A using block will call Dispose implicitly when the block ends
            #pragma warning disable IDE0063 // Don't simplify 'using' statement in lesson
            using (var charles = new DisposableClass {Name = "Charles"})
            {
                charles.Name = "Chuck";
            }
            #pragma warning restore IDE0063
        }
        #endregion

        #region Helper Functions
        // Convert the IComparable or IComparer result to a string
        private static string GetPosition(int i)
        {
            switch (i)
            {

                case 0:
                    return "at the same time as";
                case 1:
                    return "after";
                case -1:
                    return "before";
                default:
                    return "unknown";
            }
        }
        #endregion
    }
}
#pragma warning restore IDE0300
#pragma warning restore IDE0090
#pragma warning restore IDE0028
#pragma warning restore S125

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
