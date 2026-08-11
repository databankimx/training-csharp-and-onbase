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
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch02.BasicProgramStructure
{
    // Default class for console executable
    internal static class Program
    {
        #region Constants
        // Location for code sample files
        private const string CodeSamples = @"Textbook Resources.zip\MCSD Certification Code and Test Questions\02\Chapter2\";

        // Location for study guide (cheat sheet)
        private const string CheatSheet = @"Textbook Resources.zip\MCSD Certification Toolkit Cheat Sheets & Key Terms\";

        // Chapter number
        private const int Chapter = 2;

        // Chapter topic
        private const string Topic = "program flow";
        #endregion

        #region Main Executable Method
        // Note: We have removed the "args" array since we are not passing command-line arguments
        private static void Main()
        {
            #region Pre-Lesson: Understanding Comments
            // A single-line comment is preceded by two forward-slashes (//)

            /*
             * A multi-line comment is preceded by a forward-slash and an asterisk
             * Note: A common convention is to precede internal lines with an asterisk,
             *       but this is not required.
             * The multi-line comment is closed when it is followed by an asterisk and a forward-slash
             */
            #endregion

            // Note: I am using a try/catch/finally structure here, because this is our standard pattern
            //       However, we will save a discussion of this until the appropriate chapter
            try
            {
                #region Chapter Lessons
                // Code Standards Hint: Method-names should be self-commenting. That is, the method name should explain
                // what the method does. Because of this, each method should only perform one main task.

                // Lesson 1: Understanding Simple Statements
                SimpleStatements();
                GenericFunctions.Pause();

                // Lesson 2: Understanding Complex Statements
                ComplexStatements();
                GenericFunctions.Pause();

                // Lesson 3: Conditional Operators
                ConditionalOperators();
                GenericFunctions.Pause();
                
                // Code Lab: Use of Bool
                CodeLabUseOfBool();
                GenericFunctions.Pause();

                // Lesson 4: If Then Else
                IfThenElse();
                GenericFunctions.Pause();

                // Code Lab: Using if Statements
                CodeLabUsingIfStatements();
                GenericFunctions.Pause();

                // Lesson 5: Switch Statements
                SwitchStatements();
                GenericFunctions.Pause();

                // Lesson 6: Using Loops
                UsingLoops();
                GenericFunctions.Pause();

                // Code Lab: Working with 'for' Loops
                CodeLabForLoops();
                GenericFunctions.Pause();
                #endregion

                #region Bonus Lessons
                // Illustrates the use of arithmetic operators
                BonusArithmeticOperators();
                GenericFunctions.Pause();

                // Illustrates operator precedence
                BonusPrecedence();
                GenericFunctions.Pause();

                // Illustrates the use of arithmetic operators
                BonusIncrementAndDecrement();
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
        // Lesson 1: Understanding Simple Statements
        private static void SimpleStatements()
        {
            /*
             * Statement (Definition)
             * A statement is a code construct that instructs the computer to do something.
             */

            /*
             * Simple Statement (Definition)
             * A simple statement ends with a semicolon and typically instructs the computer to perform a single action
             */

            // Simple Statements (Examples)
            // Variable Declaration Statements (Declare variable names)
            int counter;
            float distance;
            string firstName;
            // Note: In a real-world program, we would declare these with the initialization

            // Assignment Statements (Assign values to variables)
            counter = 0;
            distance = 4.5f;
            firstName = "Bill";

            // You can combine declaration and assignment in a single simple statement
            const string instructorName = "Scott McLean";

            // Jump Statements (Used to direct code flow)
            // Note: I have commented these out, as they cannot be used in their current location
            #pragma warning disable S125 // Commented code is intensional for lesson
            //break;
            //continue;
            //return;
            #pragma warning restore S125

            // Empty Statement (A stand-alone semicolon on a line by itself is legal in code but does nothing)
            #pragma warning disable S1116 // Allow meaningless semicolons in this example
            ;
            #pragma warning restore S1116

            // Write out the declared variables to the Console window
            Console.Clear();
            Console.WriteLine("Chapter 2 : Lesson 1");
            Console.WriteLine();
            Console.WriteLine("Simple Statements");
            Console.WriteLine("-----------------");
            Console.WriteLine($"counter = {counter}");
            Console.WriteLine($"distance = {distance}");
            Console.WriteLine($"firstName = {firstName}");
            Console.WriteLine($"instructorName = {instructorName}");
        }

        // Lesson 2: Understanding Complex Statements
        private static void ComplexStatements()
        {
            /*
             * Block (Definition)
             * A block is a section of code contained within a pair of curly braces {}      NOSONAR
             */

            /*
             * Complex Statement (Definition)
             * A complex statement will enclose multiple simple statements within a block
             * Note: Complex statements may end with a semicolon (e.g. do {} while (); block),
             *       but this is not a requirement for most.
             */

            // Let's use a couple of simple statements to declare an array and a variable first
            #pragma warning disable IDE0300 // Keeping legacy style for lesson
            // Could also be expressed as: `int[] numbers = [5, 24, 36, 19, 45, 60, 78];`
            int[] numbers = { 5, 24, 36, 19, 45, 60, 78 };
            #pragma warning restore IDE0300
            int evenNums = 0;

            // Complex Statements (Examples)

            Console.Clear();
            Console.WriteLine("Chapter 2 : Lesson 2");
            Console.WriteLine();
            Console.WriteLine("Complex Statements");
            Console.WriteLine("------------------");

            // Loop example (foreach)
            // Don't worry about the operators for now. Just note the blocks that make this a complex statement
            foreach (int num in numbers)
            {
                Console.WriteLine(num);
                if (num % 2 == 0)
                {
                    evenNums++;
                }
            }

            Console.WriteLine($"Found {evenNums} even number{(evenNums == 1 ? "" : "s")}");
        }

        // Lesson 3: Conditional Operators
        private static void ConditionalOperators()
        {
            /*
             * Conditional operations allow the programmer to determine what action to take based on a value (or condition).
             * Conditionals rely on the C# concepts below...
             */

            /*
             * Boolean (true/false) variables and values are used in logical (comparison) operations
             * The result of a conditional is always a Boolean value
             */

            Console.Clear();
            Console.WriteLine("Chapter 2 : Lesson 3");
            Console.WriteLine();
            Console.WriteLine("Conditional Operators");
            Console.WriteLine("---------------------");

            const bool myConditionResult = false;
            Console.WriteLine($"myConditionResult = {myConditionResult}");

            /*
             * Relational Operators
             * These operators provide a means of comparing two expressions (values or variables)
             * These operators always return a Boolean value
             * 
             * Operator |        Meaning        |    Example     |        Returns true When
             *    <     | Less Than             | expr1 < expr2  | expr1 is less than expr2
             *    >     | Greater Than          | expr1 > expr2  | expr1 is greater than expr2
             *    <=    | Less than or Equal    | expr1 <= expr2 | expr1 is less than or equal to expr2
             *    >=    | Greater than or Equal | expr1 >= expr2 | expr1 is greater than or equal to expr2
             *    ==    | Equality              | expr1 == expr2 | expr1 is equal to expr2
             *    !=    | Inequality            | expr1 != expr2 | expr1 is not equal to expr2
             */

            byte expr1 = 1;
            Console.WriteLine($"expr1 = {expr1}");
            byte expr2 = 2;
            Console.WriteLine($"expr2 = {expr2}");
            Console.WriteLine($"expr1 < expr2 ? {expr1 < expr2}");
            Console.WriteLine($"expr1 > expr2 ? {expr1 > expr2}");
            Console.WriteLine($"expr1 <= expr2 ? {expr1 <= expr2}");
            Console.WriteLine($"expr1 >= expr2 ? {expr1 >= expr2}");
            Console.WriteLine($"expr1 == expr2 ? {expr1 == expr2}");
            Console.WriteLine($"expr1 != expr2 ? {expr1 != expr2}");

            /*
             * GOTCHA WARNING!
             * 
             * Be careful not to use the assignment operator when you mean to use the equality operator.
             * The assignment operator will always be evaluated as true if it returns a non-zero, non-null,
             *     and you'll change the value in your variable.
             * 
             * This is a very common logic error and can be difficult to find in a complex program,
             *     so be careful!
             */
            Console.WriteLine($"expr1 == expr2 ? {expr1 == expr2}");
            #pragma warning disable S1121 // Invalid expression intentional for lesson
            Console.WriteLine($"expr1 = expr2 ? {expr1 = expr2}");
            #pragma warning restore S1121
            Console.WriteLine($"expr1 = {expr1}");
            Console.WriteLine($"expr2 = {expr2}");

            #pragma warning disable S125
            /*
             * Boolean (and Bitwise) Logical Operators
             * Because it is sometimes necessary to evaluate more than one condition,
             *     logical (Boolean) and bitwise (binary) operators are provided.
             * 
             * Operator | Meaning
             *    &     | Bitwise AND                |
             *    |     | Bitwise OR                 |
             *    ^     | Bitwise Exclusive OR (XOR) |
             *    !     | Logical Negation (NOT)     |
             *    ~     | Bitwise Complement         |
             *    &&    | Logical AND                |
             *    ||    | Logical OR                 |
             *   true   | Assignable Boolean true    |
             *   false  | Assignable Boolean false   |
             *
             *   Note: Logical AND (&&) can short-circuit
             *         Returns false without evaluating the right-hand operand if the left-hand operand is false
             *
             *   Note: Logical OR (||) can short-circuit
             *         Returns true without evaluating the right-hand operand if the left-hand operand is true
             * 
             *   Note: There is no logical XOR operator, but XOR can be approximated using && and || thus...
             *         (expr1 || expr2) && !(expr1 && expr2)
             *           or
             *         (expr1 || expr2) && (!expr1 || !expr2)
             *           or
             *         (expr1 && !expr2) || (!expr1 && expr2)
             *           note: in the last example, the parentheses are not necessary
             *                 due to && having precedence over ||
             *
             *   Note: Unary (&) returns memory address of its operand
             *         Using this requires specifying "unsafe" code
             *
             * Truth Tables for Logical Operators
             * 
             * Negation (NOT)
             *   !   x
             *  -------
             *   F | t
             *   T | f
             * 
             * Conjunction (AND)
             *   x  &&   y
             *  -----------
             *   t | T | t
             *   f | F | t
             *   t | F | f
             *   f | F | f
             *
             * Disjunction (OR)
             *   x  ||   y
             *  -----------
             *   t | T | t
             *   f | T | t
             *   t | T | f
             *   f | F | f
             * 
             * Bitwise operators do not return boolean values. Instead, they compare each bit in the values,
             *     returning 1 or 0 bit values for the result
             * 
             * Truth Tables for Bitwise Operators
             * 
             * Complement
             *   ~   b
             *  -------
             *   0 | 1
             *   1 | 0
             * 
             * Conjunction (AND)
             *   b1  &  b2
             *  -----------
             *   1 | 1 | 1
             *   0 | 0 | 1
             *   1 | 0 | 0
             *   0 | 0 | 0
             * 
             * Disjunction (OR)
             *   b1  |  b2
             *  -----------
             *   1 | 1 | 1
             *   0 | 1 | 1
             *   1 | 1 | 0
             *   0 | 0 | 0
             * 
             * Exclusive Disjunction (XOR)
             *   b1  ^  b2
             *  -----------
             *   1 | 0 | 1
             *   0 | 1 | 1
             *   1 | 1 | 0
             *   0 | 0 | 0
             */
            #pragma warning restore S125

            // Bitwise Operator Examples
            expr1 = 15; // Binary 00001111
            expr2 = 10; // Binary 00001010
            Console.WriteLine();
            Console.WriteLine("Bitwise Examples");
            Console.WriteLine("----------------");
            Console.WriteLine($"expr1 = {Convert.ToString(expr1, 2).PadLeft(8, '0')} = {expr1}");
            Console.WriteLine($"expr2 = {Convert.ToString(expr2, 2).PadLeft(8, '0')} = {expr2}");
            Console.WriteLine($"expr1 & expr2 = {Convert.ToString(expr1 & expr2, 2).PadLeft(8, '0')} = {expr1 & expr2}");
            // expr1 & expr2 = Binary 00001010 = 10
            Console.WriteLine($"expr1 | expr2 = {Convert.ToString(expr1 | expr2, 2).PadLeft(8, '0')} = {expr1 | expr2}");
            // expr1 | expr2 = Binary 00001111 = 15
            Console.WriteLine($"expr1 ^ expr2 = {Convert.ToString(expr1 ^ expr2, 2).PadLeft(8, '0')} = {expr1 ^ expr2}");
            // expr1 ^ expr2 = Binary 00000101 =  5

            /*
             * GOTCHA WARNING!
             * The bitwise ~ operator returns a signed 32-bit integer by default, regardless of the data type being
             * complemented, so be sure to cast the result where necessary to get the expected results.
             */

            // Note: Bitwise complements will include the leading zero bits complemented to 1's
            Console.WriteLine($"~expr1 = {Convert.ToString((byte)~expr1, 2).PadLeft(8, '0')} = {(byte)~expr1}");
            // ~expr1 = Binary 11110000 = 240
            Console.WriteLine($"~expr2 = {Convert.ToString((byte)~expr2, 2).PadLeft(8, '0')} = {(byte)~expr2}");
            // ~expr1 = Binary 11110101 = 245

            /*
             * The Ternary Conditional Operator
             * C# offers a number of abbreviated methods for expressing complex but common operations.
             * The conditional operator allows you to evaluate a condition and return different values when it is true or false.
             * 
             * The syntax is condition ? valueIfTrue : valueIfFalse
             */

            // Conditional Operator Example
            string result = expr1 > expr2 ? "" : "not ";
            // This is equivalent to the commented out code below:
            #pragma warning disable S125 // Code in comment intentional
            //if (expr1 > expr2)
            //{
            //    result = "";
            //}
            //else
            //{
            //    result = "not ";
            //}
            #pragma warning restore S125

            Console.WriteLine();
            Console.WriteLine("Conditional Operator Example");
            Console.WriteLine("----------------------------");
            Console.WriteLine($"{expr1} is {result}greater than {expr2}");
        }

        // Lesson 4: If Then Else
        private static void IfThenElse()
        {
            #pragma warning disable S125 // Commented code intentional for lesson
            /*
             * The most basic control structure, 'if' directs the computer to perform some action(s)
             *     only if the condition being evaluated is true
             *     
             * Syntax:
             *   if (condition)
             *   {
             *       statement_executed_when_true;
             *   }
             * 
             * When there is a need to have some alternative action(s) when the condition is false,
             *     use the 'else' control structure
             *     
             * Syntax:
             *   if (condition)
             *   {
             *       statement_executed_when_true;
             *   }
             *   else
             *   {
             *       statement_executed_when_false;
             *   }
             * 
             * To evaluate multiple conditions, use the 'else if' control structure
             *     
             * Syntax:
             *   if (condition1)
             *   {
             *       statement_executed_when_condition1_true;
             *   }
             *   else if (condition2)
             *   {
             *       statement_executed_when_condition2_true;
             *   }  
             *   ...
             *   else if (condition_n)
             *   {
             *       statement_executed_when_condition_n_true;
             *   }
             *   else
             *   {
             *       statement_executed_when_all_conditions_false;
             *   }
             */
            #pragma warning restore S125

            int x = 1;
            int y = 2;

            Console.Clear();
            Console.WriteLine("Chapter 2 : Lesson 4");
            Console.WriteLine();
            Console.WriteLine("If Then Else");
            Console.WriteLine("------------");

            // NOTE: The book recommends always surrounding the statement governed by an "if" with curly braces
            //       However, if there is only one simple statement to execute, these are not strictly required
            if (true) Console.WriteLine("This statement still executes.");

            // Through the remainder of this lesson, we'll stick to the book standards.

            if (x < y)
            {
                Console.WriteLine($"{x} is less than {y}");
            }

            x = 3;

            if (x > y)
            {
                Console.WriteLine($"{x} is greater than {y}");
            }
            else
            {
                Console.WriteLine($"{x} is not greater than {y}");
            }

            x = 2;

            if (x < y)
            {
                Console.WriteLine($"{x} is less than {y}");
            }
            else if (x > y)
            {
                Console.WriteLine($"{x} is greater than {y}");
            }
            else
            {
                Console.WriteLine($"{x} is equal to {y}");
            }
        }

        // Lesson 5: SwitchStatements
        private static void SwitchStatements()
        {
            #pragma warning disable S125
            /*
             * When comparing possible values of a single variable, the if, else if ... else construct
             *     can become unwieldy. A better control structure for this scenario is the 'switch'
             *
             * Switch can compare values for any simple data type (string, int, double, etc.)
             *
             * Don't use this structure if your decision branching is based on multiple variables
             * Don't use this structure to compare complex data types
             * 
             * Syntax:
             *   switch (variable)
             *       case value1:
             *           statements when variable == value1;
             *           break;
             *       case value2:
             *           statements when variable == value2;
             *           break;
             *       ...
             *       case value_n:
             *           statements when variable == value_n;
             *           break;
             *       default:
             *           statements when variable is any value not specified above;
             *           break;
             * 
             * This is equivalent to the following more cumbersome code:
             *   if (variable == value1)
             *   {
             *       statements when variable == value1;
             *   }
             *   else if (variable == value2)
             *   {
             *       statements when variable == value2;
             *   }
             *   ...
             *   else if (variable == value_n)
             *   {
             *       statements when variable == value_n;
             *   }
             *   else
             *   {
             *       statements when variable is any value not specified above;
             *   }
             * 
             * The 'switch' structure can be especially useful when using the 'return' jump instead of 'break'
             *   switch (variable)
             *       case value1:
             *           return value when variable == value1;
             *       case value2:
             *           return value when variable == value2;
             *       ...
             *       case value_n:
             *           return value when variable == value_n;
             *       default:
             *           return value when variable is any value not specified above;
             *
             * The 'switch' statement also supports stacking conditions to execute a single result, like this
             *   switch (variable)
             *       case value1:
             *       case value2:
             *           return value when variable == value1 or value2;
             *       ...
             *       case value_n:
             *           return value when variable == value_n;
             *       default:
             *           return value when variable is any value not specified above;
             *
             */
            #pragma warning restore S125

            Console.Clear();
            Console.WriteLine("Chapter 2 : Lesson 5");
            Console.WriteLine();
            Console.WriteLine("Switch Statements");
            Console.WriteLine("-----------------");
            Console.WriteLine();

            // Sample switch statement using a string comparison
            string condition = "Hello";
            Console.WriteLine(condition);

            switch (condition)
            {
                case "Good Morning":
                    Console.WriteLine("Good morning to you!");
                    break;
                case "Hello":
                    Console.WriteLine("Hello to you too.");
                    break;
                case "Good Evening":
                    Console.WriteLine("Have a wonderful evening!");
                    break;
                default:
                    Console.WriteLine("Good bye...");
                    break;
            }

            /*
             * When several values have the same result, they can be stacked within the 'switch'
             */

            var r = new Random();
            int number = r.Next(0, 9);
            Console.WriteLine();
            switch (number)
            {
                case 0:
                case 1:
                    Console.WriteLine($"Number [{number}] could be binary, octal, or decimal.");
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    Console.WriteLine($"Number [{number}] could be octal or decimal.");
                    break;
                default:
                    Console.WriteLine($"Number [{number}] must be decimal.");
                    break;
            }
        }

        // Lesson 6: Using Loops
        private static void UsingLoops()
        {
            /*
             * Loops allow you to repeat a series of instructions, avoiding repetition of the code
             */

            Console.Clear();
            Console.WriteLine("Chapter 2 : Lesson 6");
            Console.WriteLine();
            Console.WriteLine("Using Loops");
            Console.WriteLine("-----------");
            Console.WriteLine();

            #pragma warning disable S125 // Commented code intentional for lesson
            /*
             * 'for' Loop
             * 
             * When you want to execute instructions a specified number of times, use a 'for' loop
             * 
             * Syntax:
             *   for (initial_state, condition, iterator)
             *   {
             *       instructions_to_repeat;
             *   }
             */
            #pragma warning restore S125

            Console.WriteLine("'for' Loop Example:");
            Console.WriteLine("-------------------");
            // This loop continues to run as long as the condition (i <= 10) remains true
            for (int i = 1; i <= 10; i++)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine();

            #pragma warning disable S125 // Commented code intentional for lesson
            /*
             * GOTCHA WARNING!
             * 
             * In all loops, make sure you have properly coded an exit point. That is, you need to have a condition
             *     that, when met, ends the loop, and there needs to be some function that will eventually cause
             *     that condition to occur. Otherwise, you'll have what's called an "infinite loop."
             * 
             * Although this error is much more common when using a 'while' loop. here's an example of an infinite
             *     'for' loop:
             * 
             *   // Because this iterates down (-1, -2, etc.), i will *always* be less than 10, and the loop never ends
             *   for (int i = 0; i <= 10; i--)
             *   {
             *       Console.WriteLine(i);
             *   }
             */
            #pragma warning restore S125

            Console.WriteLine("Press any key to run the lottery code lab...");
            Console.ReadKey();
            CodeLabLotteryProgram();
            GenericFunctions.Pause();

            Console.Clear();
            Console.WriteLine("Chapter 2 : Lesson 6");
            Console.WriteLine();
            Console.WriteLine("Using Loops");
            Console.WriteLine("-----------");
            Console.WriteLine();

            #pragma warning disable S125 // Commented code intentional for lesson
            /*
             * 'foreach' Loop
             * 
             * When you have a collection of items, and you want to perform a series of instructions on every item
             *     in the collection, use a 'foreach' loop.
             *     
             * Syntax:
             *   foreach(variable in collection)
             *   {
             *       instructions_to_repeat;
             *   }
             */
            #pragma warning restore S125

            Console.WriteLine("'foreach' Loop Example:");
            Console.WriteLine("-----------------------");
            int[] numbers = [5, 10, 15, 20];
            foreach (int number in numbers)
            {
                Console.WriteLine(number / 5);
            }

            Console.WriteLine("Press any key to run the grades code lab...");
            Console.ReadKey();
            CodeLabAverageGrades();
            GenericFunctions.Pause();

            Console.Clear();
            Console.WriteLine("Chapter 2 : Lesson 6");
            Console.WriteLine();
            Console.WriteLine("Using Loops");
            Console.WriteLine("-----------");
            Console.WriteLine();

            #pragma warning disable S125 // Commented code intentional for lesson
            /*
             * 'while' and 'do while' Loops
             * 
             * When you need to loop until a condition occurs, but you are not controlling the condition outside the loop,
             *     a 'while' or 'do while' loop is your best choice.
             *     
             * 'while' Loop Syntax:
             *   while (condition)
             *   {
             *       instructions_to_repeat;
             *   }
             *      
             * 'do while' Loop Syntax:
             *   do
             *   {
             *       instructions_to_repeat;
             *   } while (condition)
             * 
             * These two types of loops are very similar, but there is one main difference:
             * - A 'while' loop compares the condition before it executes, but a 'do while' loop compares
             *       the condition after executing.
             * - This means that even if the condition is already false, a 'do while' loop will execute at least once
             */
            #pragma warning restore S125

            Console.WriteLine("'while' Loop Example:");
            Console.WriteLine("---------------------");
            int num = 0;
            var r = new Random();
            while (num != 10)
            {
                num = r.Next(0, 11);
                Console.WriteLine(num);
            }
            Console.WriteLine();

            Console.WriteLine("'do while' Loop Example:");
            Console.WriteLine("------------------------");
            do
            {
                Console.WriteLine("Note: Even though I made the condition false, this loop ran once.");
            } while (false);
            Console.WriteLine();
        }
        #endregion

        #region Code Labs
        // See \Textbook Resources\MCSD Certification Code and Test Questions\02\Chapter2\Use of Bool Code Lab.txt
        private static void CodeLabUseOfBool()
        {
            Console.Clear();
            Console.WriteLine("Chapter 2 : Code Lab - Use of Booleans");
            Console.WriteLine();

            // create a variable of type bool called result
            // Note: Unlike fields, local variables have no default value in C# - the compiler
            //       requires "result" to be definitely assigned before it's read, which is why
            //       it must be set below before Console.WriteLine can use it
            bool result;

            // check a simple comparison and assign the value to variable result
            // in this case, we check if the literal 2 is equal to the literal 2
            // the result of this comparison is true and the variable result will
            // now contain the bool value true
            #pragma warning disable S1764 // Intentionally using a comparison for demonstration purposes
            result = 2 == 2;
            #pragma warning restore S1764

            Console.WriteLine(result);  // will output the value true
        }

        // See \Textbook Resources\MCSD Certification Code and Test Questions\02\Chapter2\using_if_statements
        private static void CodeLabUsingIfStatements()
        {
            Console.Clear();
            Console.WriteLine("Chapter 2 : Code Lab - Using If Statements");
            Console.WriteLine();

            // declare some variables for use in the code and assign initial values
            int first = 2;
            int second = 0;

            // use a single if statement to evaluate a condition and output some text
            // indicating the results

            Console.WriteLine("Single if statement");

            if (first == 2)
            {
                Console.WriteLine("The if statement evaluated to true");
            }
            Console.WriteLine("This line outputs regardless of the if condition");

            Console.WriteLine();

            // create an if statement that evaluates two conditions and executes
            // statements only if both are true
            Console.WriteLine("An if statement using && operator.");

            if (first == 2 && second == 0)
            {
                Console.WriteLine("The if statement evaluated to true");
            }
            Console.WriteLine("This line outputs regardless of the if condition");

            Console.WriteLine();

            // create nested if statements

            Console.WriteLine("Nested if statements.");

            if (first == 2)
            {
                if (second == 0)
                {
                    Console.WriteLine("Both outer and inner conditions are true.");
                }
                Console.WriteLine("Outer condition is true, inner may be true.");
            }
            Console.WriteLine("This line outputs regardless of the if condition");

            Console.WriteLine();
        }

        // See \Textbook Resources\MCSD Certification Code and Test Questions\02\Chapter2\lottery_program
        private static void CodeLabLotteryProgram()
        {
            Console.Clear();
            Console.WriteLine("Chapter 2 : Code Lab - Lottery Program");
            Console.WriteLine();

            // used to set up a range of values to choose from
            int[] range = new int[49];

            // used to simulate lottery numbers chosen
            int[] picked = new int[6];

            // set up a random number generator
            Random rnd = new();

            // populate the range with values from 1 to 49
            for (int i = 0; i < 49; i++)
            {
                range[i] = i + 1;
            }

            // pick 6 random numbers
            for (int select = 0; select < 6; select++)
            {
                picked[select] = range[rnd.Next(49)];
            }

            Console.WriteLine("Your lotto numbers are:");
            for (int j = 0; j < 6; j++)
            {
                Console.Write(" " + picked[j] + " ");
            }
            Console.WriteLine();
        }

        // See \Textbook Resources\MCSD Certification Code and Test Questions\02\Chapter2\average_grades
        private static void CodeLabAverageGrades()
        {
            Console.Clear();
            Console.WriteLine("Chapter 2 : Code Lab - Average Grades");
            Console.WriteLine();

            // foreach loop to average grades in an array
            // set up an integer array and assign some values
            int[] arrGrades = [78, 89, 90, 76, 98, 65];

            // create three variables to hold the sum, number of grades, and the average
            int total = 0;
            int gradeCount = 0;
            double average;

            // loop to iterate over each integer value in the array
            // foreach does not need to know the size initially as it is determined
            // at the time the array is accessed. 
            foreach (int grade in arrGrades)
            {
                // Equivalent to total = total + grade;         NOSONAR
                total += grade;   // add each grade value to total
                gradeCount++;     // increment counter for use in average
            }

            if (gradeCount == 0) total = gradeCount = 1;

            average = (double)total / gradeCount;   // calculate average of grades
            Console.WriteLine(average);
        }

        // See \Textbook Resources\MCSD Certification Code and Test Questions\02\Chapter2\working_with_for_loops
        private static void CodeLabForLoops()
        {
            Console.Clear();
            Console.WriteLine("Chapter 2 : Code Lab - For Loops");
            Console.WriteLine();

            // using a for loop to count up by one
            Console.WriteLine("Count up by one");

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine();

            // using a for loop to count down by one
            Console.WriteLine("Count down by one");

            for (int i = 10; i > 0; i--)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine();

            // using a for loop to count up by 2
            Console.WriteLine("Count up by two");

            for (int i = 0; i < 10; i += 2)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine();

            // using a for loop to increment by multiples of 5
            Console.WriteLine("Count up by multiples of 5");

            for (int i = 5; i < 1000; i *= 5)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine();

            // using a foreach loop with integers
            Console.WriteLine("foreach over an array of integers");

            int[] arrInts = [1, 2, 3, 4, 5];
            foreach (int number in arrInts)
            {
                Console.WriteLine(number);
            }
            Console.WriteLine();

            // using a for each loop with strings
            Console.WriteLine("foreach over an array of strings");

            string[] arrStrings = ["First", "Second", "Third", "Fourth", "Fifth"];
            foreach (string text in arrStrings)
            {
                Console.WriteLine(text);
            }
            Console.WriteLine();

            // using a while loop
            int whileCounter = 0;

            Console.WriteLine("Counting up by one using a while loop");
            while (whileCounter < 10)
            {
                Console.WriteLine(whileCounter);
                whileCounter++;
            }
            Console.WriteLine();

            // using a do-while loop
            int doCounter = 0;

            Console.WriteLine("Counting up using a do-while loop");
            do
            {
                Console.WriteLine(doCounter);
                doCounter++;
            } while (doCounter < 10);
            Console.WriteLine();
        }
        #endregion

        #region Bonus Methods
        // Illustrates the use of arithmetic operators
        private static void BonusArithmeticOperators()
        {
            // The most critical operator is the assignment operator
            // This assigns the value of the statement to its right to the variable to its left

            // Assignment (=)
            int a = 4;
            int b = 2;
            Console.WriteLine($"a = {a} and b = {b}");

            // The unary plus and minus operators set the sign of the value being assigned

            // Unary Plus (+) : Positive
            int c = +1;
            Console.WriteLine($"c = {c}");

            // Unary Minus (-) : Negative
            int d = -1;
            Console.WriteLine($"d = {d}");

            // The binary computing operators are the ones we're all familiar with from grade-school arithmetic

            // Addition (+)
            c = a + b;
            Console.WriteLine($"{a} + {b} = {c}");

            // Subtraction (-)
            c = a - b;
            Console.WriteLine($"{a} - {b} = {c}");

            // Multiplication (*)
            c = a * b;
            Console.WriteLine($"{a} * {b} = {c}");

            // Division (/)
            c = a / b;
            Console.WriteLine($"{a} / {b} = {c}");

            c = 5;

            // Here are examples of compound assignment operators that perform a
            //   computation and then assign to the same variable used in the computation

            // Addition/Assignment (+=)
            Console.Write($"{c} += 5 yields ");
            c += 5;
            Console.WriteLine(c);

            // Subtraction/Assignment (-=)
            Console.Write($"{c} -= 5 yields ");
            c -= 5;
            Console.WriteLine(c);

            // Multiplication/Assignment (*=)
            Console.Write($"{c} *= 2 yields ");
            c *= 2;
            Console.WriteLine(c);

            // Division/Assignment (/=)
            Console.Write($"{c} /= 2 yields ");
            c /= 2;
            Console.WriteLine(c);

            // Some operators exist that you might not expect, like the modulus

            // Modulus (%) - Returns the remainder when dividing the values to either side
            d = c % b;
            Console.WriteLine($"{c} % {b} = {d}");
        }

        // Illustrates operator precedence
        private static void BonusPrecedence()
        {
            // Aside from the assignment operator, other arithmetic operators process left-to-right
            // However, they also obey an order of precedence

            // First, multiplication and division are processed (still left-to-right)
            //   and then addition and subtraction are processed

            // Here the multiplication processes first, so this is equivalent to 2 + 4 = 6
            Console.WriteLine($"2 + 2 * 2 = {2 + 2 * 2}");

            // You can override this by implementing parentheses for grouping
            // (which are processed from inner to outer and then from left to right)

            // Here, because it's in parentheses, the addition is processed first,
            //   so this is equivalent to 4 * 2 = 8
            Console.WriteLine($"(2 + 2) * 2 = {(2 + 2) * 2}");
        }

        #pragma warning disable S125 // Commented code intentional for lesson
        // Illustrates the increment (++) and decrement operators (--)
        //   and the concept of pre- and post-fix
        #pragma warning restore S125
        private static void BonusIncrementAndDecrement()
        {
            #pragma warning disable S125 // Commented code intentional for lesson
            // The special operators for increment (++) and decrement (--)
            //   have different behaviors depending on where they are placed relative to the variable
            #pragma warning restore S125

            int a = 0;
            Console.WriteLine($"a = {a}");

            // I can increment like this
            #pragma warning disable IDE0054 // Demonstrating the long form of incrementing a variable
            a = a + 1;
            Console.WriteLine($"a = {a}");
            #pragma warning restore IDE0054

            // Or like this
            a += 1;
            Console.WriteLine($"a = {a}");

            // Or like this
            a++;
            Console.WriteLine($"a = {a}");

            // Or like this
            ++a;
            Console.WriteLine($"a = {a}");

            // I can decrement like this
            #pragma warning disable IDE0054 // Demonstrating the long form of decrementing a variable
            a = a - 1;
            Console.WriteLine($"a = {a}");
            #pragma warning restore IDE0054

            // Or like this
            a -= 1;
            Console.WriteLine($"a = {a}");

            // Or like this
            a--;
            Console.WriteLine($"a = {a}");

            // Or like this
            --a;
            Console.WriteLine($"a = {a}");

            // You will have noticed that when I used the unary increment and decrement,
            //   I could put them either before (prefix) or after (postfix) the variable

            // When in the prefix position, the operation takes place before the variable is used
            Console.WriteLine("Prefix");
            Console.WriteLine($"a = {++a}");
            Console.WriteLine($"a = {a}");

            // But in the postfix position, the operation takes place after the variable is used
            Console.WriteLine("Postfix");
            Console.WriteLine($"a = {a++}");
            Console.WriteLine($"a = {a}");

            // The same rule holds true when decrementing
            Console.WriteLine("Prefix");
            Console.WriteLine($"a = {--a}");
            Console.WriteLine($"a = {a}");
            Console.WriteLine("Postfix");
            Console.WriteLine($"a = {a--}");
            Console.WriteLine($"a = {a}");

            // NOTE: pre- and post-fix do not impact the usable value in a for loop,
            //       because the iterator step always takes place after the statement executes.
            //       So these two loops will display identical results
            Console.WriteLine($"{Environment.NewLine}Using postfix in a for loop iterator...");
            for (int i = 0; i < 5; i++)
            {
                Console.Write($"{(i > 0 ? ", " : "")}{i}");
            }

            Console.WriteLine($"{Environment.NewLine}Using prefix in a for loop iterator...");
            for (int i = 0; i < 5; ++i)
            {
                Console.Write($"{(i > 0 ? ", " : "")}{i}");
            }
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
