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
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using CSharp.Ch04.UsingTypes.HelperClasses.Extensions;
using CSharp.Ch04.UsingTypes.Models.Objects;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
using Excel = Microsoft.Office.Interop.Excel;
using Newtonsoft.Json.Linq;
#endregion

// NOTE!  In real-world programming, do not use pragmas to disable warnings across the entire file.
//        Instead, use them only for the specific lines of code that are causing the warning.
#pragma warning disable S125    // Commented code intentionally left in for demonstration purposes
#pragma warning disable S1854   // Unused assignments intentionally left in for demonstration purposes
#pragma warning disable IDE0059 // Unused assignments intentionally left in for demonstration purposes
#pragma warning disable IDE0071 // Non-simplified interpolated strings intentionally left in for demonstration purposes
#pragma warning disable IDE0018 // Not using inline variable declaration (clearer for demonstration purposes)
#pragma warning disable IDE0090 // Not simplifying to 'new(...)' (clearer for demonstration purposes)
namespace CSharp.Ch04.UsingTypes
{
    // Default class for console executable
    internal static class Program
    {
        #region Constants
        // Location for code sample files
        private const string CodeSamples = @"Textbook Resources.zip\MCSD Certification Code and Test Questions\04\Chapter4\";

        // Location for study guide (cheat sheet)
        private const string CheatSheet = @"Textbook Resources.zip\MCSD Certification Toolkit Cheat Sheets & Key Terms\";

        // Chapter number
        private const int Chapter = 4;

        // Chapter topic
        private const string Topic = "using and converting data types";
        #endregion

        #region Private Globals
        // This calls an external COM (non-managed) DLL
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        // There is a reference for such P/Invoke calls here:   http://www.pinvoke.net/

        // Here is the book example of the same thing
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetShortPathName(string lpszLongPath, char[] lpszShortPath, int cchBuffer);

        // When you need to explicitly manage the memory used, you can use the Marshal class
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetShortPathNameExplicit([MarshalAs(UnmanagedType.LPTStr)] string lpszLongPath, 
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszShortPath, uint cchBuffer);
        #endregion

        #region Main Executable Method
        // Presence of Main() method renders the class runnable
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                // Examples of data type conversion
                ConvertingBetweenTypes();
                GenericFunctions.Pause();

                // Examples of casting with arrays
                CastingArrays();
                GenericFunctions.Pause();

                // Examples using parsing methods
                Parsing();
                GenericFunctions.Pause();

                // Examples using System.Convert
                SystemConvert();
                GenericFunctions.Pause();

                // Examples using System.BitConverter
                SystemBitConvert();
                GenericFunctions.Pause();

                // Explain boxing and unboxing
                BoxingAndUnboxing();
                GenericFunctions.Pause();

                // Illustrate building custom string conversion extension methods
                CustomConversions();
                GenericFunctions.Pause();

                // Demonstrate the use of DllImport
                ImportedComDll();
                GenericFunctions.Pause();

                // Demonstrate use of COM DLLs via interop
                ExcelInterop();
                GenericFunctions.Pause();

                // Demonstrate the use of the "dynamic" type
                BonusLessonDynamics();
                GenericFunctions.Pause();

                // Demonstrate methods for cloning arrays
                CloningArrays();
                GenericFunctions.Pause();

                /*
                 * At this time the book goes into a "real-world example" called "Order Entry Forms"
                 * You will find the complete project in the downloaded text book source code
                 * I will not cover this in lecture
                 */

                // Demonstrate string manipulations
                ManipulatingStrings();
                GenericFunctions.Pause();

                // Demonstrate the static methods on the string class
                StaticStringMethods();
                GenericFunctions.Pause();

                // Demonstrate the instance methods on the string class
                InstanceStringMethods();
                GenericFunctions.Pause();

                /*
                 * At this time the book goes into a "real-world example" called "Handling Percentage Values"
                 * You will find the complete project in the downloaded text book source code
                 * I will not cover this in lecture
                 */

                // Demonstrate StringBuilder StringWriter and StringReader
                UsingStringBuilder();
                GenericFunctions.Pause();

                // Illustrate the use, overloads, and overrides of ToString()
                UsingToString();
                GenericFunctions.Pause();

                // Demonstrate some of the options for using string.Format
                StringFormat();
                GenericFunctions.Pause();

                /*
                 * At this time the book goes into a "real-world example" called "Displaying Currency Values"
                 * You will find the complete project in the downloaded text book source code
                 * I will not cover that example directly, but the bonus lesson below covers the actual
                 *     reason "decimal" exists as a type in the first place, which is exactly what that
                 *     real-world example would have needed
                 */

                // Bonus: Illustrate why decimal, not double, is the correct type for money
                BonusDecimalVsDouble();
                GenericFunctions.Pause();
                #endregion

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

        #region Lesson Methods
        // Examples of data type conversion
        private static void ConvertingBetweenTypes()
        {
            Console.Clear();
            Console.WriteLine(@"Chapter 4 : Lesson 1");
            Console.WriteLine();
            Console.WriteLine(@"Converting Between Types");
            Console.WriteLine(@"------------------------");

            try
            {

                /*
                 * The simplest type of conversion is casting, which can be done in this syntactic form:
                 *     type varName = (type)varOfOtherType;
                 *
                 * Casting requires that there be an implicit mechanism for converting between the types.
                 * Later, we'll address creating explicit conversion methods.
                 */

                // A widening conversion (smaller to larger of similar basic type) will always work
                byte b = 127;
                int i = (int)b; // Note that the explicit cast can be omitted in the code here
                Console.WriteLine($@"byte to int: {i}");
                Console.WriteLine();

                // A narrowing conversion (larger to smaller of similar types) works properly only if the
                //    value of the larger type is able to be contained in the smaller type
                i = 64;
                b = (byte)i;
                Console.WriteLine($@"int to byte: {b}");
                Console.WriteLine();

                // A narrowing conversion (larger to smaller of similar types) fails if the
                //    value of the larger type is unable to be contained in the smaller type
                i = 264;
                b = (byte)i;
                Console.WriteLine($@"int to byte with invalid value ({i}): {b}");
                Console.WriteLine($@"byte max value: {byte.MaxValue}");
                Console.WriteLine($@"The ending value [{b}] results from any bits higher than the max value being lost.");
                Console.WriteLine($@"{i} - 256 (the unsupported bit in the byte type for value {i}) = {i - 256}");
                Console.WriteLine();
                // Notice that this doesn't throw an exception

                // This time, by surrounding the operation with a "checked" block, though, the same
                //    conversion results in an exception
                checked
                {
                    i = 264;
                    b = (byte)i;
                    Console.WriteLine($@"int to byte with invalid value ({i}): {b}");
                }
                // Just a note: The "checked" block is exclusively locally scoped. It will not apply the
                //    checked rules to any methods called from within the block.
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            GenericFunctions.Pause();

            // The same failure to complain when a narrowing cast is applied to floating-point
            //    types can be addressed by checking for infinite values
            double big = -1E40;
            float small = (float)big;
            Console.WriteLine();
            Console.WriteLine(float.IsInfinity(small)
                ? "Whoops! Must have overflowed the type..."
                : small.ToString(CultureInfo.InvariantCulture));
            Console.WriteLine();

            // Before reviewing this example, look at the concept of encapsulation (PersonEncapsulated class)
            // When converting from a child class to its parent, implicit (widening) conversion is supported
            var employee = new Employee("Joe", "Programmer", "Development", "Software Engineer");
            Person person = employee;

            Console.WriteLine($@"Variable ""employee"" [{employee.FirstName}] is of type [{employee.GetType().Name}]");
            Console.WriteLine($@"Variable ""person"" [{person.FirstName}] is of type [{person.GetType().Name}]");
            // Notice that its underlying type has not changed, but its behaviors have.

            // You can access the .Department and .JobTitle on "employee" but not "person" even though they're still there
            // Remember, this is a reference type!

            // You can check the type by using the "is" keyword...
            Console.WriteLine($@"""person"" is {(person is Employee ? "" : "not")} an Employee");

            // ...and cast using the "as keyword
            // In this case, this allows us to again access the properties that were hidden by the case to Person
            Console.WriteLine($@"{person.FirstName} is a {(person as Employee).JobTitle}");
        }

        // Examples of casting with arrays
        private static void CastingArrays()
        {
            // Important note: Casting does not generate a new array; it references the existing one
            // This is also true of array assignment
            //   if you have int[] numbers = new int[10];   ...
            //     int[] moreNumbers = numbers          does NOT create a second array
            //     Array.Copy(numbers, moreNumbers)     is an example of a way to create a second array

            // Declare and initialize an array of Employees.
            Employee[] employees = new Employee[10];
            for (int id = 0; id < employees.Length; id ++) employees[id] = new Employee(id);

            // Implicit cast to an array of Persons.
            // (An Employee is a type of Person.)
            Person[] persons = employees;
            
            // Explicit cast back to an array of Employees.
            // (The Persons in the array happen to be Employees.)
            persons = (Employee[]) persons;
            
            // Use the is operator.
            if (persons is Employee[])
            {
                Console.WriteLine("Array \"persons\" is now an array of \"Employee\" type");
            }

            // Use the as operator.
            employees = persons as Employee[];

            // After this as statement, managers is null.
            Manager[] managers = persons as Manager[];

            // Use the is operator again, this time to see if persons is compatible with Manager[]
            Console.WriteLine($"persons is {(persons is Manager[] ? "" : "not ")} convertible to Manager[]");

            // This cast fails at run time because the array // holds Employees not Managers.
            try
            {
                managers = (Manager[])persons;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        // Examples using parsing methods
        private static void Parsing()
        {
            // All of the C# primitive types expose a Parse() function
            //   that will attempt to format a string as the specified type.

            // The following data types are supported:
            // bool
            // byte
            // char
            // DateTime
            // decimal
            // double
            // float
            // int
            // long
            // sbyte
            // short
            // TimeSpan
            // uint
            // ulong
            // ushort

            string numString = "10";
            int number = int.Parse(numString);
            Console.WriteLine($"String [{numString}] parses to int [{number}]...");

            numString = "ten";
            // Parsing that value throws an exception, so we'll try/catch this
            try
            {
                number = int.Parse(numString);
                Console.WriteLine($"{Environment.NewLine}String [{numString}] parses to int [{number}]...");
            }
            catch (Exception ex)
            {
                number = 0;
                Console.WriteLine();
                Console.WriteLine(ex);
            } 

            // But C# offers a better way to handle this. TryParse()
            if (int.TryParse(numString, out number))
            {
                // Only gets here if the value parses without error
                Console.WriteLine($"{Environment.NewLine}String [{numString}] parses to int [{number}]...");
            }
            else
            {
                number = 0;
                Console.WriteLine($"{Environment.NewLine}String [{numString}] cannot be parsed to int...");
            }

            // NOTE: As a best practice, you should avoid using the type.Parse() method whenever possible
            //       type.TryParse() is safer, but even it is not necessarily reliable

            // Of course, the Parse and TryParse methods are limited in the string formats they can process
            // As you'll see in a couple of lessons, we can improve this by creating our own parsing/conversion methods

            // One special example of parsing is using the decimal.Parse() to handle currency
            // By default, decimal.Parse() can handle grouping symbols
            string money = "1,000.00";
            Console.WriteLine();
            Console.WriteLine($"decimal.Parse(\"{money}\") = {decimal.Parse(money)}");

            // But it cannot handle currency symbols
            money = "$1,000.00";
            try
            {
                Console.WriteLine();
                Console.WriteLine($"decimal.Parse(\"{money}\") = {decimal.Parse(money)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Environment.NewLine}Failed to parse [{money}] as a decimal!");
                Console.WriteLine(ex);
            }

            // You can overcome this limitation (sometimes) by passing the optional argument for NumberStyle
            Console.WriteLine();
            Console.WriteLine($"decimal.Parse(\"{money}\", NumberStyles.Currency) = {decimal.Parse(money, NumberStyles.Currency)}");

            // Because the NumberStyles enum is a set of bit-flag values, you can use the
            //   bitwise OR to stack specific options instead of using the aggregate "Currency" value
            Console.WriteLine();
            Console.WriteLine($"decimal.Parse with stacked NumberStyles = {decimal.Parse(money, 
                NumberStyles.AllowCurrencySymbol | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands)}");
        }

        // Examples using System.Convert
        private static void SystemConvert()
        {
            // System.Convert exposes many methods for converting data types,
            //   but we need to be careful to properly handle the resulting value changes

            // A common example would be to convert a decimal value to an integer
            // Many municipal tax systems round income to the nearest dollar, for example
            double income = 9.50;
            int rounded = Convert.ToInt32(income);
            // This yields 10, as we expect from normal 5/4 rounding
            Console.WriteLine($"[{income.ToString("#.##")}] rounds to [{rounded}]");

            income = 10.50;
            rounded = Convert.ToInt32(income);
            // But this also yields 10 where we would have expected 11 from normal rounding
            Console.WriteLine($"[{income.ToString("#.##")}] rounds to [{rounded}]");

            // This happens because integer conversions implement banker's rounding
            // Banker's rounding is just like normal 5/4 rounding unless the decimal is exactly .5,
            //   in which case, rounding is to the nearest even integer, not always up as we'd expect

            // We can overcome this by manually rounding
            rounded = (int)Math.Round(income, MidpointRounding.AwayFromZero);
            Console.WriteLine($"[{income.ToString("#.##")}] rounds to [{rounded}]");
            // This seems trivial, but it's a very important lesson
            // As programmers, we rely on exceptions a lot of the time to reveal bugs in our code,
            //   but this is an instance where there would be no error, just bad values,
            //   and if we don't watch out for it and code to handle the scenario, it's our fault

            // One important difference between casting and using System.Convert is that the Convert
            //   methods throw exceptions when a value is out of range instead of rolling over into 
            //   unexpected values
            try
            {
                income = 300.00;
                byte tooSmall = Convert.ToByte(income);
                Console.WriteLine($"[{income.ToString("#.##")}] converts to [{tooSmall}]");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            // System.Convert also provides a ChangeType method
            // Its advantage is that you can use the C# type aliases instead of the System .NET types
            // Note that since this is a generic method, we have to cast the result from object to our data type
            rounded = (int)Convert.ChangeType(income, typeof(int));
            Console.WriteLine($"[{income.ToString("#.##")}] changes to [{rounded}]");
        }

        // Examples using System.BitConverter
        private static void SystemBitConvert()
        {
            // It's often useful to be able to stage an item as a byte array
            // Some examples would be to load a value into a stream or to serialize a binary file
            // The System.BitConverter class exposes methods to convert to and from byte arrays
            // https://learn.microsoft.com/en-us/dotnet/api/system.bitconverter?view=netframework-4.8
            int packedValue = PackTwoIntegers(15, 25);
            Console.WriteLine($"{Environment.NewLine}The packed int value is [{packedValue}]");

            // Convert the packed value into a byte array
            byte[] packedBytes = BitConverter.GetBytes(packedValue);
            
            // Unpack the resulting values
            short left = BitConverter.ToInt16(packedBytes, 0);
            short right = BitConverter.ToInt16(packedBytes, 2);
            Console.WriteLine($"{Environment.NewLine}The unpacked values are [{left}] and [{right}]");

            // For strings, we use System.Text.Encoding to get to our byte arrays,
            //   because the text encoding affects the byte values
            string maeterlinck = "At every crossway on the road that leads to the future stand a thousand men dedicated to guard the past";
            byte[] maeterlinckBytes = Encoding.UTF8.GetBytes(maeterlinck);
            Console.WriteLine();
            foreach (byte item in maeterlinckBytes)
            {
                Console.Write($"{item:x2} ");
            }
            Console.WriteLine();
            maeterlinck = Encoding.UTF8.GetString(maeterlinckBytes);
            Console.WriteLine($"{Environment.NewLine}{maeterlinck}");
            // We could use the BitConverter on this, but the resulting string would show the untranslated bytes
            Console.WriteLine($"{Environment.NewLine}{BitConverter.ToString(maeterlinckBytes)}");
        }

        // Explain boxing and unboxing
        private static void BoxingAndUnboxing()
        {
            // Boxing is the process of converting a value type such as an int or bool into an object or an interface
            //   that is supported by the value’s type.
            // Unboxing is the processing of converting a boxed value back into its original value.

            // Here's an illustration of explicitly boxing and unboxing a variable
            int num = 10;

            // Here, we're "boxing" the int as an object
            object boxedNum = num;

            // Here, we're "unboxing" the object as an int
            int unboxedNum = (int)boxedNum;
            Console.WriteLine($"boxedNum = {boxedNum}, unboxedNum = {unboxedNum}");

            // Frequently, boxing takes place implicitly
            // For example, the signature of this call is string.Format(string, object)
            // We are passing an int (which is compatible with object), so it is implicitly boxed when we make the call
            Console.WriteLine();
            Console.WriteLine(string.Format("num is {0}", num));
        }

        // Illustrate building custom string conversion methods
        private static void CustomConversions()
        {
            // Here are some values we might assume would come back as TRUE
            string t = "true";
            string y = "yes";
            string o = "1";

            // Here are some values we might assume would come back as FALSE
            string f = "false";
            string n = "no";
            string z = "0";

            // bool.Parse can't handle most of them
            Console.WriteLine("Using bool.Parse() method...");
            Console.WriteLine($"Value [{t}] yields Boolean [{bool.Parse(t)}]");
            Console.WriteLine($"Value [{f}] yields Boolean [{bool.Parse(f)}]");
            // Values other than "true" or "false" result in a FormatException
            foreach (string val in new[] {y, n, o, z})
            {
                try
                {
                    Console.WriteLine($"Value [{val}] yields Boolean [{bool.Parse(val)}]");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Value [{val}] caused an exception!");
                    Console.WriteLine(ex);
                }
            }

            // bool.TryParse protects against exceptions but still doesn't handle most of our values
            Console.WriteLine($"{Environment.NewLine}Using bool.TryParse() method...");
            bool parsed;
            if (bool.TryParse(t, out parsed)) Console.WriteLine($"Value [{t}] yields Boolean [{parsed}]");
            if (bool.TryParse(f, out parsed)) Console.WriteLine($"Value [{f}] yields Boolean [{parsed}]");
            // The values that threw exceptions before, now simply return false from bool.TryParse()
            // So these four lines won't print out
            if (bool.TryParse(y, out parsed)) Console.WriteLine($"Value [{t}] yields Boolean [{parsed}]");
            if (bool.TryParse(n, out parsed)) Console.WriteLine($"Value [{t}] yields Boolean [{parsed}]");
            if (bool.TryParse(o, out parsed)) Console.WriteLine($"Value [{t}] yields Boolean [{parsed}]");
            if (bool.TryParse(z, out parsed)) Console.WriteLine($"Value [{t}] yields Boolean [{parsed}]");

            // However, we can create our own conversion methods to overcome that limitation

            // Look at the GenericExtensions class in the SharedLibrary project for the definition of ToBoolean()
            Console.WriteLine($"{Environment.NewLine}Using custom ToBoolean() method...");
            Console.WriteLine($"Value [{t}] yields Boolean [{t.ToBoolean()}]");
            Console.WriteLine($"Value [{f}] yields Boolean [{f.ToBoolean()}]");
            Console.WriteLine($"Value [{y}] yields Boolean [{y.ToBoolean()}]");
            Console.WriteLine($"Value [{n}] yields Boolean [{n.ToBoolean()}]");
            Console.WriteLine($"Value [{o}] yields Boolean [{o.ToBoolean()}]");
            Console.WriteLine($"Value [{z}] yields Boolean [{z.ToBoolean()}]");

            // We can also implement improved Parse and TryParse methods
            // Look at the GenericExtensions class in the SharedLibrary project for the definitions
            Console.WriteLine($"{Environment.NewLine}Using custom Parse() method...");
            Console.WriteLine($"Value [{t}] yields Boolean [{t.Parse()}]");
            Console.WriteLine($"Value [{f}] yields Boolean [{f.Parse()}]");
            Console.WriteLine($"Value [{y}] yields Boolean [{y.Parse()}]");
            Console.WriteLine($"Value [{n}] yields Boolean [{n.Parse()}]");
            Console.WriteLine($"Value [{o}] yields Boolean [{o.Parse()}]");
            Console.WriteLine($"Value [{z}] yields Boolean [{z.Parse()}]");
            Console.WriteLine($"{Environment.NewLine}Using custom TryParse() method...");
            if (t.TryParse(out parsed)) Console.WriteLine($"Value [{t}] yields Boolean [{parsed}]");
            if (f.TryParse(out parsed)) Console.WriteLine($"Value [{f}] yields Boolean [{parsed}]");
            if (y.TryParse(out parsed)) Console.WriteLine($"Value [{y}] yields Boolean [{parsed}]");
            if (n.TryParse(out parsed)) Console.WriteLine($"Value [{n}] yields Boolean [{parsed}]");
            if (o.TryParse(out parsed)) Console.WriteLine($"Value [{o}] yields Boolean [{parsed}]");
            if (z.TryParse(out parsed)) Console.WriteLine($"Value [{z}] yields Boolean [{parsed}]");
        }

        // Demonstrate the use of DllImport
        private static void ImportedComDll()
        {
            // Call the imported user32.dll to display a message box
            MessageBox(new IntPtr(0), "Hello World!", "Hello Dialog", 0);
            Console.WriteLine($"A message box pops up as a separate window.{Environment.NewLine}");
            // We could, of course, do the same thing with a managed assembly by using System.Windows.Forms
            
            // Now for the book's example (taken from the ShortPathNames solution in the sample code
            // This process gets the old-style 8-character max path components
            string longName = Assembly.GetExecutingAssembly().Location;
            Console.WriteLine($"Long Name:{Environment.NewLine}{longName}{Environment.NewLine}");
            char[] buffer = new char[1024];
            long length = GetShortPathName(longName, buffer, buffer.Length);
            // Get the short name.
            string shortName = new string(buffer).Substring(0, (int)length);
            Console.WriteLine($"Short Name:{Environment.NewLine}{shortName}");

            // I have not included the code to use the Marshal example
            // If interested, give writing that code a try
        }

        // Demonstrate use of COM DLLs via interop
        private static void ExcelInterop()
        {
            // Don't forget to reference the interop DLL

            // Adapted from the ExcelInterop solution in the sample code
            // Open the Excel application.
            Excel._Application excelApp = new Excel.Application();

            // Add a workbook.
            Excel.Workbook workbook = excelApp.Workbooks.Add();

            // The Worksheets indexer (and Range members like Cells and Columns) are typed
            //     as plain "object" in this interop assembly rather than their specific COM
            //     types, since the NuGet package doesn't get the "embed interop types"
            //     treatment an old-style raw COM reference did. Rather than casting every
            //     single member access below, "dynamic" defers all of that to runtime, this
            //     is exactly the scenario the "dynamic" keyword exists for.
            dynamic sheet = workbook.Worksheets[1];

            // Display Excel.
            excelApp.Visible = true;

            // Display some column headers.
            sheet.Cells[1, 1].Value = "Value";
            sheet.Cells[1, 2].Value = "Value Squared";

            // Display the first 10 squares.
            for (int i = 1; i <= 10; i++)
            {
                sheet.Cells[i + 1, 1].Value = i;
                sheet.Cells[i + 1, 2].Value = (i * i).ToString();
            }

            // Auto-fit the columns.
            sheet.Columns[1].AutoFit();
            sheet.Columns[2].AutoFit();

            Console.WriteLine("Excel opened in another window.");
        }

        // Demonstrate the use of the "dynamic" type
        private static void BonusLessonDynamics()
        {
            // A dynamic is an object from which you can access named properties
            // But it bypasses type-checking at compile time
            // This means you can assign any structure to it
            
            // For this example, I am using it to unpack a JSON string for which we have not defined a class
            // NOTE: I added Newtonsoft.Json via NuGet instead of referencing a local DLL
            const string json = "{\"Id\":\"1234-5678\",\"Data\":{\"FirstName\":\"Scott\",\"LastName\":\"McLean\"}}";
            dynamic result = JObject.Parse(json);
            Console.WriteLine($"{result.Id}: {result.Data.FirstName} {result.Data.LastName}");
        }

        // Demonstrate methods for cloning arrays
        private static void CloningArrays()
        {
            // Make an array of numbers
            int[] array1 = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

            // This doesn't work, because Clone() returns an object
            // int[] array2 = array1.Clone();

            // This works (because we're casting)
            int[] array3 = (int[])array1.Clone();
            Console.WriteLine($"array3 = {string.Join(", ", array3)}");
            Console.WriteLine();

            // This also works
            dynamic array4 = array1.Clone();
            Console.WriteLine($"array4[9] = {array4[9]}");
            Console.WriteLine();

            // But even though the dynamic isn't type-checked, this will not work
            try
            {
                // No compiler-time error, but the type mismatch will produce a runtime error
                array4[0] = "one";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        // Demonstrate string manipulations
        private static void ManipulatingStrings()
        {
            // A string is (essentially) a series of 16-bit Unicode characters
            // Notes:
            // - Unicode values are commonly represented in hexadecimal 0000 to FFFF
            // - For the majority of English-language usage, characters only require 8 bits
            //   so, for or ASCII and UTF-8, the upper byte is always 00
            // - You can view the included Unicode chart for character values

            // A string is NOT a character array
            // - char is both a primitive and a value type, so both char and char[] are stored on the stack
            // - string is a class implementation (reference type) and thus is stored on the heap

            // A string, once it has been stored in memory, is immutable (cannot be altered)
            // This means that if you change the value of a string, you are recreating that string
            //   in another allocated space in memory
            // Because of this, string manipulating functions generally just return the modified value
            //   as opposed to changing the value of the variable itself.
            // This is mitigated a bit by the use of the intern pool, where every unique string used
            //   by the program is stored, so that multiple string variables sharing the same value
            //   can point to a single instance of the memory used to hold that value.

            // The simplest means of populating a string is just to set it to a literal
            string marquee = "************************";

            // Or to another existing value
            string nl = Environment.NewLine;

            // String constructor examples
            // From a char array
            char[] fNameParts = [ 'S', 'c', 'o', 't', 't' ];
            string fName = new string(fNameParts);
                               // From a range in a character array
            char[] lNameParts =
            [
                'T', 'h', 'e', ' ', 'M', 'c', 'L', 'e', 'a', 'n', 's', 'a', 'r', 'e', ' ', 'f', 'r', 'o', 'm', ' ', 'D',
                'u', 'a', 'r', 't', ' ', 'i', 'n', ' ', 'S', 'c', 'o', 't', 'l', 'a', 'n', 'd'
            ];
            string lName = new string(lNameParts, 4, 6);
            // From a character and a repetition length
            string padding = new string('*', 5);

            Console.WriteLine($"{marquee}{nl}{padding} {fName} {lName} {padding}{nl}{marquee}{nl}");

            // From the string class you can access three properties

            // - Empty (can be used to represent an empty string (no values in its character list)
            string value = string.Empty;

            // - Length (number of characters in the string's character list)
            Console.WriteLine($"value [{value}] is {value.Length} character{(value.Length == 1 ? "" : "s")} long{nl}");
            value = "12345";
            Console.WriteLine($"value [{value}] is {value.Length} character{(value.Length == 1 ? "" : "s")} long{nl}");

            // - Indexer (allows you to access a character by its position in the string's character list)
            //   Don't let this confuse you about the string being an array. It isn't.
            Console.WriteLine($"The 4th character of value [{value}] is [{value[3]}]{nl}");
            // Note: You can't do this, because the indexer is read-only
            // value[3] = '5';

            // However, if you need a character array containing the string's contents, you can do this:
            char[] valueChars = value.ToCharArray();
            for (int i = 0; i < valueChars.Length; i++)
            {
                Console.WriteLine($"[{value[i]}] = [{valueChars[i]}]");
            }
            Console.WriteLine();
        }

        // Demonstrate the static methods on the string class
        private static void StaticStringMethods()
        {
            string nl = Environment.NewLine;

            // There are tons of string manipulation methods in C#. The book illustrates these:
            // string.Compare is useful for sorting and returns
            Console.WriteLine($"string.Compare(\"A\", \"A\") = {string.Compare("A", "A")}"); // Returns  0 (A equals A)
            Console.WriteLine($"string.Compare(\"A\", \"B\") = {string.Compare("A", "B")}"); // Returns -1 (A before B)
            Console.WriteLine($"string.Compare(\"B\", \"A\") = {string.Compare("B", "A")}"); // Returns  1 (B after  A)
            Console.WriteLine($"string.Compare(\"A\", \"a\") = {string.Compare("A", "a")}"); // Returns  1 (A after  a) * remember, we're in a case-sensitive world
            Console.WriteLine($"string.Compare(\"A\", \"a\", CurrentCultureIgnoreCase) = {string.Compare("A", "a", StringComparison.CurrentCultureIgnoreCase)}"); // Returns  0 (A equals a) * case-insensitive
            Console.WriteLine();

            // string.Concat can be used to concatenate multiple strings or an array of strings
            string[] words = ["Development ", "is ", "fun!"];
            Console.WriteLine($"string.Concat(words) = {string.Concat(words)}");
            Console.WriteLine(string.Concat("It ", "really ", "is!"));
            // Of course it's sometimes easier to just use the concatenation operator [+]
            Console.WriteLine("See..." + nl);

            // string.Copy creates a copy of the string
            string original = "12345";
            string copied = string.Copy(original);
            string other = "54321";

            // Here is one place where strings are unlike most other reference types
            // Although original and copied are different items, the == operator still returns TRUE
            //   because they both point to the same unique value in the intern pool
            Console.WriteLine($"original == copied ? [{original == copied}]");
            Console.WriteLine($"string.Equals(original, copied) ? [{string.Equals(original, copied)}]");
            // This different behavior is intentional, since we'll often work with strings as though they were value types
            Console.WriteLine($"string.Equals(original, other) ? [{string.Equals(original, other)}]{nl}");

            // Because of its complexity, we'll hold string.Format and handle it separately

            // string.IsNullOrEmpty (along with its cousin string.IsNullOrWhiteSpace) is very useful
            //   Often it's insufficient to null-check a string. It may have been initialized without a value.
            string nullString = null;
            string emptyString = string.Empty; // Could also use string emptyString = "";
            string spaceString = "   ";
            string tabString = "\t";
            foreach (string val in new[] { nullString, emptyString, spaceString, tabString })
            {
                Console.WriteLine($"[{val}]{nl} - null ? {val == null}{nl} - empty ? {string.IsNullOrEmpty(val)}{nl} - whitespace ? {string.IsNullOrWhiteSpace(val)}{nl}");
            }

            // string.Join links an array of values using a separator
            int[] nums = [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 ];
            Console.WriteLine($"string.Join(\"-\", nums) = {string.Join("-", nums)}");
            Console.WriteLine();
        }
        
        // Demonstrate the instance methods on the string class
        private static void InstanceStringMethods()
        {
            // Of course, once you have created a string instance, you can execute methods directly from the instance
            // Many of these expose the same functionality as static methods

            // Clone does the same thing as string.Copy (remember, you have to cast the object returned by Clone)
            string original = "one two three four five";
            string clone = (string)original.Clone();
            Console.WriteLine($"clone = {clone}");
            Console.WriteLine();

            // CompareTo works like string.Compare, treating the instance as the left operand
            string value = "B";
            Console.WriteLine($"value.CompareTo(\"A\") = {value.CompareTo("A")}");
            Console.WriteLine($"value.CompareTo(\"B\") = {value.CompareTo("B")}");
            Console.WriteLine($"value.CompareTo(\"C\") = {value.CompareTo("C")}");
            Console.WriteLine($"value.CompareTo(\"b\") = {value.CompareTo("b")}");

            // instance.CompareTo doesn't expose a way to handle case-insensitive comparison like the static string.Compare
            // Consider how you might easily create an extension method for this functionality on the instance
            // Then take a look at a simple implementation in the StringExtensions class
            Console.WriteLine($"value.CompareTo(\"b\", CurrentCultureIgnoreCase) = {value.CompareTo("b", StringComparison.CurrentCultureIgnoreCase)}");
            Console.WriteLine();

            // Contains checks to see if a specified string appears in the instance value
            Console.WriteLine($"original.Contains(\"one\") = {original.Contains("one")}");
            Console.WriteLine($"original.Contains(\"ONE\") = {original.Contains("ONE")}");
            Console.WriteLine($"original.Contains(\"six\") = {original.Contains("six")}");
            Console.WriteLine();
            // This is another candidate for an extension method to accept a StringComparison argument

            // CopyTo duplicates a portion of the string into a char array
            char[] array = new char[3];
            original.CopyTo(4, array, 0, 3);
            Console.WriteLine($"original.CopyTo(...) yields = {new string(array)}");
            Console.WriteLine();

            // EndsWith checks if the tail end of the instance matches a specified string
            Console.WriteLine($"original.EndsWith(\"four\") = {original.EndsWith("four")}");
            Console.WriteLine($"original.EndsWith(\"five\") = {original.EndsWith("five")}");
            Console.WriteLine($"original.EndsWith(\"FIVE\") = {original.EndsWith("FIVE")}");
            Console.WriteLine($"original.EndsWith(\"FIVE\", CurrentCultureIgnoreCase) = {original.EndsWith("FIVE", StringComparison.CurrentCultureIgnoreCase)}");
            Console.WriteLine();

            // Equals works just like string.Equals with the instance as the left operand
            Console.WriteLine($"value.Equals(\"A\") = {value.Equals("A")}");
            Console.WriteLine($"value.Equals(\"B\") = {value.Equals("B")}");
            Console.WriteLine($"value.Equals(\"b\") = {value.Equals("b")}");
            Console.WriteLine($"value.Equals(\"b\", CurrentCultureIgnoreCase) = {value.Equals("b", StringComparison.CurrentCultureIgnoreCase)}");
            Console.WriteLine();

            // IndexOf gets the first position in a string where a specified character or string occurs
            Console.WriteLine($"original.IndexOf(\"two\") = {original.IndexOf("two", StringComparison.CurrentCultureIgnoreCase)}");
            Console.WriteLine();

            // IndexOfAny gets the first position in a string where any of an array of characters occurs
            Console.WriteLine($"original.IndexOfAny(['t','f']) = {original.IndexOfAny([ 't', 'f' ])}");
            Console.WriteLine();

            // Insert returns a string containing another inserted string
            Console.WriteLine($"original.Insert(4, \"half \") = {original.Insert(4, "half ")}");
            // But it doesn't change the original string
            Console.WriteLine($"original = {original}");
            Console.WriteLine();

            // LastIndexOf gets the last position in a string where a specified character or string occurs
            #pragma warning disable S1075 // Not a real path - Nothing exposed
            string filePath = @"C:\Temp\Something.Something\DarkSide.txt";
            #pragma warning restore S1075
            // Note: This is not the best way to get the file extension, just an illustration for LastIndexOf
            string extension = filePath.Substring(filePath.LastIndexOf(".", StringComparison.Ordinal) + 1);
            Console.WriteLine($"extension = {extension}");
            Console.WriteLine();

            // LastIndexOfAny gets the last position in a string where any of an array of characters occurs
            Console.WriteLine($"original.LastIndexOfAny(['t','f']) = {original.LastIndexOfAny([ 't', 'f' ])}");
            Console.WriteLine();

            // PadLeft allows you to pad a string (on the left end) to a specified length with a desired character
            Console.WriteLine($"\"1\".PadLeft(5) = [{"1".PadLeft(5, ' ')}]");
            Console.WriteLine($"\"10\".PadLeft(5) = [{"10".PadLeft(5, ' ')}]");
            Console.WriteLine($"\"100\".PadLeft(5) = [{"100".PadLeft(5, ' ')}]");
            Console.WriteLine($"\"1000\".PadLeft(5) = [{"1000".PadLeft(5, ' ')}]");
            Console.WriteLine();

            // PadRight allows you to pad a string (on the right end) to a specified length with a desired character
            var p1 = new Employee("John", "Doe", "Development", "Software Engineer");
            var p2 = new Employee("Mary", "Smith", "IT", "Network Engineer");
            Console.WriteLine("First Name     Last Name      Department     Title");
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine($"{p1.FirstName.PadRight(15, ' ')}{p1.LastName.PadRight(15, ' ')}{p1.Department.PadRight(15, ' ')}{p1.JobTitle}");
            // We can also
            Console.WriteLine($"{p2.FirstName.PadRight(15, ' ')}{p2.LastName.PadRight(15, ' ')}{p2.Department.PadRight(15, ' ')}{p2.JobTitle}");
            Console.WriteLine();

            // Remove removes a specified range from the string
            Console.WriteLine($"original.Remove(7, 6) = {original.Remove(7, 6)}");
            // Like the other functions, it does not modify the value stored in the variable
            Console.WriteLine($"original = {original}");
            Console.WriteLine();

            // Replace replaces each instance of a specific string with a desired replacement string
            Console.WriteLine($"original.Replace(\"two\", \"222\") = {original.Replace("two", "222")}");
            // Like the other functions, it does not modify the value stored in the variable
            Console.WriteLine($"original = {original}");
            Console.WriteLine();

            // Split returns an array of strings by splitting the original string on a specified character
            // If you don't specify a delimiting character, it will split on spaces by default
            string[] pieces = original.Split();
            foreach (string piece in pieces) Console.WriteLine($"piece = {piece}");
            Console.WriteLine();

            // StartsWith checks if the tail end of the instance matches a specified string
            Console.WriteLine($"original.StartsWith(\"one\") = {original.StartsWith("one")}");
            Console.WriteLine($"original.StartsWith(\"ONE\") = {original.StartsWith("ONE")}");
            Console.WriteLine($"original.StartsWith(\"ONE\", CurrentCultureIgnoreCase) = {original.StartsWith("ONE", StringComparison.CurrentCultureIgnoreCase)}");
            Console.WriteLine();

            // Substring returns a new string specifying a portion of the original value
            Console.WriteLine($"original.Substring(4) = {original.Substring(4)}");
            Console.WriteLine($"original.Substring(4, 3) = {original.Substring(4, 3)}");
            Console.WriteLine();

            // ToCharArray returns a character array containing the characters in the string
            char[] contents = original.ToCharArray();
            Console.WriteLine($"original.ToCharArray() = {string.Join(".", contents)}");
            contents = original.ToCharArray(4, 3);
            Console.WriteLine($"original.ToCharArray(4, 3) = {string.Join(".", contents)}");
            Console.WriteLine();

            // ToLower returns the string with all letter characters converted to lower-case
            string databank = "DataBank";
            Console.WriteLine($"databank = {databank}");
            Console.WriteLine($"databank.ToLower() = {databank.ToLower()}");
            Console.WriteLine();

            // ToString returns a string containing the content of another data type and sometimes accepts formatting
            DateTime rightNow = DateTime.Now;
            Console.WriteLine($"rightNow.ToString(\"MM-dd-yyyy h:mm:ss\") = {rightNow.ToString("MM-dd-yyyy h:mm:ss")}");
            Console.WriteLine($"rightNow.ToString(\"R\") = {rightNow.ToString("R")}");
            Console.WriteLine();

            // ToUpper returns the string with all letter characters converted to upper-case
            Console.WriteLine($"databank = {databank}");
            Console.WriteLine($"databank.ToUpper() = {databank.ToUpper()}");
            Console.WriteLine();

            // Trim returns a string with all leading and trailing whitespace removed
            string formatted = "          information          ";
            Console.WriteLine($"formatted = [{formatted}]");
            Console.WriteLine($"formatted.Trim() = [{formatted.Trim()}]");
            Console.WriteLine();

            // TrimEnd returns a string with all trailing whitespace removed
            Console.WriteLine($"formatted = [{formatted}]");
            Console.WriteLine($"formatted.TrimEnd() = [{formatted.TrimEnd()}]");
            Console.WriteLine();

            // TrimStart returns a string with all leading whitespace removed
            Console.WriteLine($"formatted = [{formatted}]");
            Console.WriteLine($"formatted.TrimStart() = [{formatted.TrimStart()}]");
            Console.WriteLine();
        }

        // Demonstrate StringBuilder StringWriter and StringReader
        private static void UsingStringBuilder()
        {
            // Adapted from book solution "Permutations"
            // Helper functions below

            int numLetters = 8;

            // Make a String holding the letters to use.
            string letters = "";
            for (int i = 0; i < numLetters; i++)
            {
                #pragma warning disable S1643 // StringBuilder isn't covered until a later lesson
                letters += (char)('A' + i);
                #pragma warning restore S1643
            }

            // Generate permutations.
            DateTime startTime, stopTime;

            Console.WriteLine($"Using 8 Characters; [{letters}]");
            Console.WriteLine("Counting permutations using string");
            Console.WriteLine("-----------------------------------------");
            string permutations = "";
            startTime = DateTime.Now;
            ConcatenatePermutations(ref permutations, letters, "");
            Console.WriteLine(permutations);
            stopTime = DateTime.Now;
            TimeSpan elapsed = stopTime - startTime;
            Console.WriteLine($"Permutations: {Factorial(numLetters)}");
            Console.WriteLine($"Elapsed Time: {elapsed.TotalSeconds.ToString("0.00")} sec{Environment.NewLine}");

            GenericFunctions.Pause();

            Console.WriteLine($"Using 8 Characters; [{letters}]");
            Console.WriteLine("Counting permutations using StringBuilder");
            Console.WriteLine("-----------------------------------------");
            StringBuilder permutationsBuilder = new StringBuilder();
            startTime = DateTime.Now;
            StringBuilderPermutations(permutationsBuilder, letters, "");
            Console.WriteLine($"Permutations: {permutationsBuilder}");
            stopTime = DateTime.Now;
            elapsed = stopTime - startTime;
            Console.WriteLine($"Permutations: {Factorial(numLetters)}");
            Console.WriteLine($"Elapsed Time: {elapsed.TotalSeconds.ToString("0.00")} sec{Environment.NewLine}");
            System.Diagnostics.Debug.Assert(permutations == permutationsBuilder.ToString());
        }
        
        // Illustrate the use, overloads, and overrides of ToString()
        private static void UsingToString()
        {
            /* LESSON NOTES:
             * Up to now, we have used some examples of ToString() where needed to print out results
             * Now, we'll look at some of the features of this method across data types
             */

            // In its most basic form, ToString() simply formats the results as a string
            int i = 1234567890;
            Console.WriteLine($"i.ToString() = {i.ToString()}");
            double d = 12345.67890;
            Console.WriteLine($"d.ToString() = {d.ToString()}");

            // It is recommended for floating-point numbers to specify the (optional) culture
            Console.WriteLine($"d.ToString(InvariantCulture) = {d.ToString(CultureInfo.InvariantCulture)}");

            // C# has built-in format specifiers
            // You can read about them here: https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
            // For example: this one will present the value as currency based on local machine settings
            Console.WriteLine($"d.ToString(\"c\") = {d.ToString("c")}");
            // We can also specify US currency
            Console.WriteLine($"d.ToString(\"c\", en-US) = {d.ToString("c", CultureInfo.CreateSpecificCulture("en-US"))}");
            // Or British, etc.
            Console.WriteLine($"d.ToString(\"c\", en-GB) = {d.ToString("c", CultureInfo.CreateSpecificCulture("en-GB"))}");

            // We can also construct specific formats to fit specific needs
            // For example, I might want to use thousands separators
            Console.WriteLine($"i.ToString(\"0,0\") = {i.ToString("0,0")}");
            // Or show a specific number of decimal places
            Console.WriteLine($"d.ToString(\"0,0.00\") = {d.ToString("0,0.00")}");

            // Further reading on ToString() formats:
            //   https://learn.microsoft.com/en-us/dotnet/api/system.int32.tostring?view=netframework-4.8
        }

        // Demonstrate some of the options for using string.Format
        private static void StringFormat()
        {
            int i = 163;
            // Without a formatted string, you would have to use concatenation to display formatted output
            Console.WriteLine((char)i + " = " + i.ToString().Substring(0, 3) + " or 0x" + i.ToString("X"));

            // Note: The book's decision to cast as char means the first instance of 'i' will be interpreted as the
            //       ASCII character with value 163 (£)

            // The string.Format method provides a means of including one or more
            //   literal or non-literal values in a resulting string without clumsy concatenation

            // The syntax for each embedded value is
            //   {index[,length][:format]}

            // Here is the same example embedding the same integer with and without the optional arguments
            Console.WriteLine(string.Format("{0} = {1,4} or 0x{2:X}", (char)i, i, i));

            // NOTE: Our team prefers string interpolation
            // String interpolation provides a more readable means of formatting a string

            // The syntax for string interpolation is to place a dollar-sign ($) preceding the opening quotation mark

            // The syntax for each embedded value is
            //   {name_or_literal[,length][:format]}

            // Here is the same example embedding the same integer with and without the optional arguments
            Console.WriteLine($"{(char)i} = {i,4} or 0x{i:X}");

            // Argument indices can be used in any order (they simply match the order the arguments are provided),
            //   and they do not need to be used
            #pragma warning disable S3457 // Unused `{0}` intentional as part of the lesson
            string text = string.Format("{1} {4} {2} {1} {3}", "who", "I", "therefore", "am", "think");
            #pragma warning restore S3457
            Console.WriteLine(text);

            // A good example of a frequently formatted string is a DateTime
            DateTime now = DateTime.Now;

            // There are a number of ready-made default formats
            //   https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings
            Console.WriteLine($"now.ToString(\"d\") = {now.ToString("d")}");
            Console.WriteLine($"string.Format(\"{{0:d}}\", now) = {string.Format("{0:d}", now)}");
            Console.WriteLine($"$\"{{now:d}}\" = {now:d}");

            // Or you can create custom formats made up of provided abbreviations
            //   https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
            Console.WriteLine($"now.ToString(\"MM/dd/yyyy\") = {now.ToString("MM/dd/yyyy")}");
            Console.WriteLine($"string.Format(\"{{0:MM/dd/yyyy}}\", now) = {string.Format("{0:MM/dd/yyyy}", now)}");
            Console.WriteLine($"$\"{{now:MM/dd/yyyy}}\" = {now:MM/dd/yyyy}");

            // DateTime also has a special, extended library of ToString functions
            Console.WriteLine($"now.ToShortDateString() = {now.ToShortDateString()}");
            Console.WriteLine($"$\"{{now:d}}\" = {now:d}");
            Console.WriteLine($"now.ToShortTimeString() = {now.ToShortTimeString()}");
            Console.WriteLine($"$\"{{now:t}}\" = {now:t}");
            Console.WriteLine($"now.ToLongDateString() = {now.ToLongDateString()}");
            Console.WriteLine($"$\"{{now:D}}\" = {now:D}");
            Console.WriteLine($"now.ToLongTimeString() = {now.ToLongTimeString()}");
            Console.WriteLine($"$\"{{now:T}}\" = {now:T}");
        }

        // Bonus: Illustrate why decimal, not double, is the correct type for money
        private static void BonusDecimalVsDouble()
        {
            /*
             * double doesn't actually store the number you typed. It stores the closest binary
             *     approximation it can manage, because double is built out of powers of two
             *     (halves, quarters, eighths, sixteenths), and most of the decimal fractions we use
             *     every day, like 0.1 or 0.2, simply don't have an exact binary equivalent. It's the
             *     same problem you'd run into trying to write 1/3 as a decimal: you can get close,
             *     but you can never quite land on it.
             *
             * decimal, on the other hand, is base-10 under the hood (128-bit, storing a scaled
             *     integer plus a power-of-10 exponent), so it can represent 0.1 or 0.2 exactly.
             *     It speaks the same language money does.
             */

            Console.WriteLine("Watch what happens when 0.1 and 0.2 meet each other");
            double d = 0.1 + 0.2;
            Console.WriteLine($"d = {d}"); // 0.30000000000000004, not 0.3
            Console.WriteLine();

            Console.WriteLine("Watch the drift happen: add 0.1 together ten times");
            double price = 0.1;
            double total = 0;
            for (int i = 0; i < 10; i++)
            {
                total += price;
            }
            Console.WriteLine($"total = {total}"); // 0.9999999999999999, not 1.0
            Console.WriteLine();

            // Errors don't stay small. Currency math rarely happens in isolation, it's thousands or
            //     millions of additions, tax calculations, and interest calculations chained together.
            //     Tiny errors compound, and eventually somebody's total doesn't match the invoice.

            Console.WriteLine("Watch a comparison quietly fail");
            Console.WriteLine(total == 1.0 ? "Equal" : "Not Equal"); // Not Equal, even though it "should" be
            Console.WriteLine();

            // Auditors don't care that it's "basically" right. Financial systems are usually expected
            //     to round to the cent in specific, predictable ways (banker's rounding, round-half-up,
            //     etc.). decimal plays nicely with Math.Round, whereas double can throw the rounding
            //     off before you even get to the rounding step, because the imprecision was baked in
            //     from the start.

            Console.WriteLine("Same math, done with decimal instead");
            decimal dm = 0.1m + 0.2m;
            Console.WriteLine($"dm = {dm}"); // 0.3, exactly
            Console.WriteLine();

            decimal priceM = 0.1m;
            decimal totalM = 0;
            for (int i = 0; i < 10; i++)
            {
                totalM += priceM;
            }
            Console.WriteLine($"totalM = {totalM}"); // 1.0, exactly
            Console.WriteLine(totalM == 1.0m ? "Equal" : "Not Equal"); // Equal, every time
            Console.WriteLine();

            Console.WriteLine("When you do need to round, decimal is predictable and explicit");
            decimal roundPrice = 19.995m;
            decimal rounded = Math.Round(roundPrice, 2, MidpointRounding.ToEven);
            Console.WriteLine($"rounded = {rounded}"); // 20.00
        }
        #endregion

        #region Standard Formatting Strings
        /*
         * NUMERIC
         *  C/c     currency
         *  D/d     decimal
         *  E/e     scientific notation
         *  F/f     fixed point
         *  G/g     general (chooses fixed-point or scientific notation, whichever is shorter - like a calculator)
         *  N/n     number (includes decimal and thousands separators
         *  P/p     percent
         *  X/x     hexadecimal
         *
         * DATETIME
         *  d       short date (M/d/yyyy)
         *  D       long date (dddd, MMMM d, yyyy)
         *  f       "full" with short time (dddd, MMMM d, yyyy h:mm tt)
         *  F       "full" with long time (dddd, MMMM d, yyyy h:mm:ss tt)
         *  g       "general" with short time (M/d/yyyy h:mm tt)
         *  G       "general" with long time (M/d/yyyy h:mm:ss tt)
         *  m/M     month and day (MMMM d)
         *  t       short time (h:mm tt)
         *  T       long tim (h:mm:ss tt)
         *  y/Y     month and year (MMMM, yyyy)
         */
        #endregion

        #region Decimal vs Double Quick Reference
        /*
         * Aspect            | double                        | decimal
         * ------------------|-------------------------------|--------------------------------
         * Base               Binary (base 2)                  Base 10, scaled integer
         * Size               8 bytes                          16 bytes
         * Precision          ~15-17 significant digits        28-29 significant digits
         * Range              Very large (±5.0 x 10^308)       Smaller (±7.9 x 10^28)
         * Hardware support   Native FPU support, fast         Software-implemented, slower
         * Exact decimal      No (e.g. 0.1 is approximate)     Yes (e.g. 0.1m is exact)
         * fractions
         * Best for           Scientific / engineering /       Currency, pricing, financial
         *                    graphics math                    calculations
         *
         * Use decimal for money, prices, tax calculations, financial reporting, basically anything
         *     where a person would be upset if the math didn't match what's printed on paper.
         * Use double/float for scientific computation, graphics, physics simulations, statistics,
         *     places that need a huge dynamic range and can tolerate a tiny relative error, where the
         *     values are measurements rather than currency.
         */
        #endregion

        #region Helper Methods
        // Packs two unsigned 16-bit integers in a single 32-bit integer
        private static int PackTwoIntegers(ushort left, ushort right)
        {
            return left << 16 | right;
        }

        // Use concatenation to make permutations separated by new lines.
        private static void ConcatenatePermutations(ref string permutations, string letters, string word)
        {
            // See if we're out of letters.
            if (letters.Length == 0)
            {
                // Add word to the result.
                permutations += word + Environment.NewLine;
            }
            else
            {
                // Add another letter to word and continue recursion.
                for (int i = 0; i < letters.Length; i++)
                {
                    char ch = letters[i];
                    string newWord = word + ch;
                    string newLetters = letters.Remove(i, 1);
                    // Recursion is costly
                    ConcatenatePermutations(ref permutations, newLetters, newWord);
                }
            }
        }

        // Use a StringBuilder to make permutations separated by new lines.
        private static void StringBuilderPermutations(StringBuilder permutations, string letters, string word)
        {
            // See if we're out of letters.
            if (letters.Length == 0)
            {
                // Add word to the result.
                permutations.AppendLine(word);
            }
            else
            {
                // Add another letter to word and continue recursion.
                for (int i = 0; i < letters.Length; i++)
                {
                    char ch = letters[i];
                    string newWord = word + ch;
                    string newLetters = letters.Remove(i, 1);
                    StringBuilderPermutations(permutations, newLetters, newWord);
                }
            }
        }

        // Return number!
        private static long Factorial(long number)
        {
            long result = 1;
            for (int i = 2; i <= number; i++) result *= i;
            return result;
        }
        #endregion
    }
}
#pragma warning restore IDE0090
#pragma warning restore IDE0018
#pragma warning restore IDE0071
#pragma warning restore IDE0059
#pragma warning restore S1854
#pragma warning restore S125

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
