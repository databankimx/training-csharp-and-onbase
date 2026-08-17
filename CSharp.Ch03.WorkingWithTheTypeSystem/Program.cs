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
using CSharp.Ch03.WorkingWithTheTypeSystem.Models.Objects;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

#pragma warning disable S125 // Code in comments is intentionally left in to illustrate points in the textbook
namespace CSharp.Ch03.WorkingWithTheTypeSystem
{
    // Default class for console executable
    internal static class Program
    {
        #region Constants
        // Location for code sample files
        private const string CodeSamples = @"Textbook Resources.zip\MCSD Certification Code and Test Questions\03\Chapter3\";

        // Location for study guide (cheat sheet)
        private const string CheatSheet = @"Textbook Resources.zip\MCSD Certification Toolkit Cheat Sheets & Key Terms\";

        // Chapter number
        private const int Chapter = 3;

        // Chapter topic
        private const string Topic = "data types";

        // Message to display when pausing for user input
        private const string ContinueMessage = "Press any key when you're ready to continue...";
        #endregion

        #region Main Executable Method
        // Presence of Main() method renders the class runnable
        private static void Main()
        {
            try
            {
                #region Value Type Notes
                /*
                 * Gaining an understanding of data types is critical to becoming a C# developer. This is in part because
                 *     C# is a strongly-typed language. That means that when comparing values, it's important to ensure
                 *     that you are also comparing the same data type.
                 * 
                 * Exam Note:
                 * One exam objective is "Create Types"
                 * Within this objective, a primary focus is on "Value Types"
                 *     (we'll contrast this later with "Reference Types")
                 * 
                 * - "Value Types" can be broken down into two categories, "Structs" and "Enumerations"
                 *   - "Structs" can be further broken down into additional categories:
                 *     - Numeric Types
                 *       - Integral Types
                 *       - Floating-Point Types
                 *       - Decimals
                 *     - Boolean Types
                 *       - Bool (bool)
                 *     - User-Defined Structs
                 */

                /*
                 * Predefined value data types in C#
                 * 
                 *   Alias   | Values                          | Size                     | .NET Type      | Default Value
                 *   --------|---------------------------------|--------------------------|----------------|---------------
                 *   bool    | true, false                     | 1 byte                   | System.Boolean | false
                 *   byte    | 0 to 255                        | Unsigned 8-bit           | System.Byte    | 0
                 *   char    | 0000 to FFFF (Unicode)          | 16-bit                   | System.Char    | '\0'
                 *   decimal | ±1.0 * 10^-28 to ±7.9 * 10^28   | 28-29 significant digits | System.Decimal | 0.0m
                 *   double  | ±5.0 * 10^-324 to ±1.7 * 10^308 | 15-16 digits             | System.Double  | 0.0d
                 *   enum    | User-Defined                    |                          |                | (E)0
                 *   float   | ±1.5 * 10^-45 to ±3.4 * 10^38   | 7 digits                 | System.Single  | 0.0f
                 *   int     | -2,147,483,648 to 2,147,483,647 | Signed 32-bit            | System.Int32   | 0
                 *   long    | -9,223,372,036,854,775,808 to   | Signed 64-bit            | System.Int64   | 0
                 *           |  9,223,372,036,854,775,807      |                          |                |
                 *   sbyte   | -128 to 127                     | Signed 8-bit             | System.SByte   | 0
                 *   short   | -32,768 to 32,767               | Signed 16-bit            | System.Int16   | 0
                 *   struct  | User-Defined                    |                          |                | null
                 *   uint    | 0 to 4,294,967,295              | Unsigned 32-bit          | System.Uint32  | 0
                 *   ulong   | 0 to 18,446,744,073,709,551,615 | Unsigned 64-bit          | System.Unity64 | 0
                 *   ushort  | 0 to 65,535                     | Unsigned 16-bit          | System.Uint16  | 0
                 * 
                 * Note: "Signed" types use one bit for the ± sign value, so for example, a signed 32-bit type
                 *         has a maximum value of ±2^31 (minus one on the positive side) - 1 as opposed to ±2^32
                 *       It's actually a little more complicated than this. If you're interested in why this really
                 *         behaves this way, check out the comment section called "Handling Binary Negatives"
                 * 
                 * Note: If no value is assigned, a declared variable contains its type's default value
                 *         e.g.: int i = new int(); // i contains value 0
                 * 
                 * Concepts:
                 * 
                 * - Memory Storage
                 *   All value types are stored on the stack, whereas reference types are stored on the heap.
                 *   The result of this is that while value types are released from memory when the stack unwinds
                 *     (at the end of their scoped existence), assigning a value type variable to another variable
                 *     results in a second instance of the value being stored in memory.
                 *     
                 *     e.g.: int i = 1;
                 *           int j = i;
                 *           // At this point there are two System.Int32 variables stored in memory
                 *           
                 *           var w1 = new StreamWriter();
                 *           var w2 = w1;
                 *           // At this point there is only one System.IO.StreamWriter stored in memory
                 *           // Both variables reference the same object and actually store only the memory address
                 *           //    (or reference).
                 *           // Being less structured than the stack, the heap must be referenced by memory address
                 * 
                 * - Memory Efficiency
                 *   Even though most machines currently have ample power and memory, it is a best practice to use no
                 *       more memory than necessary. To achieve this:
                 *     - Use the smallest data type that can accommodate your values. For example, if you are iterating
                 *           across values 1 to 100, you don't need a 32-bit int in memory. An 8-bit byte will do.
                 *     - Avoid duplicating variables. If you have an int containing a value, declaring another one with
                 *           the same value simply for naming convenience is wasteful.
                 *     - Declare your variables within the smallest scope that is practical so that they are released from
                 *           memory in a timely fashion
                 */
                #endregion

                #region Handling Binary Negatives (Two's Complement)
                /*
                 * NOTE: This section is only to satisfy curiosity. It won't affect how you write code.
                 *
                 * Why do data types support one more value in the negative than the positive?
                 *
                 * First we need to consider how binary addition works.
                 *   Note: The is no binary subtraction. In binary you add a negative as opposed to subtracting.
                 *
                 * A binary adder (at its core) is a circuit. Optimally, all that circuit should do is:
                 *   1. Take in two bits A & B (plus a carry bit C)
                 *   2. Add them together where
                 *         A   B   Sum Carry
                 *      a. 0 + 0 =  0    0
                 *      b. 0 + 1 =  1    0
                 *      c. 1 + 0 =  1    0
                 *      d. 1 + 1 =  0    1
                 *   3. Return the output and pass the carry bit to the next bit adder
                 *
                 * Such a circuit look sort of like this:
                 *       A & B are the input bits, and C is the carry
                 *      ┌──────────────────────────────────────────────────────────────────────────────────┐
                 *      │                          Full Adder                                              │
                 *      │    ┌──────────────────┐              ┌──────────────────┐                        │
                 *  A───│───>│             SUM  │───A ^ B──────│             SUM  │───(A ^ B) ^ C──────────│───> Sum
                 *      │    │ Half Adder       │              │ Half Adder       │                        │
                 *  B───│───>│            CARRY │───A & B──┐ ┌─│            CARRY │───C & (A ^ B)─┐        │
                 *      │    └──────────────────┘          │ │ └──────────────────┘               │ ┌────┐ │
                 *  Cin─│──────────────────────────────────│─┘                                    └─│    │ │
                 *      │                                  │                                        │ ≥1 │─│───> Cout
                 *      │                                  └────────────────────────────────────────│    │ │
                 *      │                                                                           └────┘ │
                 *      └──────────────────────────────────────────────────────────────────────────────────┘
                 *
                 * The problem with implementing a circuit like this is the question of how you handle negative numbers
                 *
                 * For the examples below, imagine a 4-bit data type. We know that we need an indicator for sign,
                 *   so we will steal the leftmost bit and use it such that 0 = positive and 1 = negative.
                 *
                 * Positive Values
                 *    0000 = 0
                 *    0001 = 1
                 *    0010 = 2
                 *    0011 = 3
                 *    0100 = 4
                 *    0101 = 5
                 *    0110 = 6
                 *    0111 = 7
                 *
                 * If we try to work intuitively with our sign bit (the way human beings think),
                 *   we wind up with a couple of problems with our negative values;
                 *
                 * Negative Values
                 *    1000 = -0
                 *    1001 = -1
                 *    1010 = -2
                 *    1011 = -3
                 *    1100 = -4
                 *    1101 = -5
                 *    1110 = -6
                 *    1111 = -7
                 *
                 * First of all, we have a second value for zero (negative zero)
                 * And secondly, our adder circuit (as designed above) can't be as simple
                 *
                 * Imagine 7 + -3
                 * In this system, that is this
                 *      0111
                 *    + 1011
                 *    ------
                 * 1 <- 0010   (Using binary addition, we end up with 2 and we lose a carry bit)
                 *
                 * The next option that was considered was called one's-complement, where the negative numbers
                 *   are the binary complement to the positives, so
                 * 
                 * Negative Values
                 *    1111 = -0
                 *    1110 = -1
                 *    1101 = -2
                 *    1100 = -3
                 *    1011 = -4
                 *    1010 = -5
                 *    1001 = -6
                 *    1000 = -7
                 *
                 * We still have the problem of negative zero, but addition is improved
                 *
                 * Imagine 7 + -3
                 * In this system, that is this
                 *      0111
                 *    + 1100
                 *    ------
                 * 1 <- 0011 (Now it's 3, but if we wrap the carry bit around to the beginning and re-add)
                 *      0001
                 *    ------
                 *      0100 (We end up with a correct answer of 4)
                 *
                 * However, this business of wrapping the lost carry bit doesn't make sense without extra hardware
                 *   and therefore wasted memory (since we don't have a fifth bit to store it in)
                 *
                 * Which brings is to the concept of two's-complement, which is just one's-complement plus 1
                 *
                 * This resolves the negative zero problem, because adding one to the complement of zero...
                 *      1111
                 *    + 0001
                 *    ------
                 * 1 <- 0000    ... resolves to the same value as positive zero (with a throwaway carry bit)
                 *
                 * So, in two's-complement, we have 
                 * 
                 * Negative Values
                 *    0000 =  0
                 *    1111 = -1
                 *    1110 = -2
                 *    1101 = -3
                 *    1100 = -4
                 *    1011 = -5
                 *    1010 = -6
                 *    1001 = -7
                 *    ... which leaves a wasted bit, so
                 *    1000 = -8 (and this is why we have an extra value on the negative in numeric data types)
                 *
                 * And when we add:
                 *
                 * Imagine 7 + -3
                 * In this system, that is this
                 *      0111
                 *    + 1101
                 *    ------
                 * 1 <- 0100   (Finally, we end up with 4 and we lose a carry bit, but we don't need it, so it can be ignored)
                 */
                #endregion

                #region Chapter Lessons : Part 1
                // Code Lab: Value Type Aliases
                CodeLabTypeAliases();
                GenericFunctions.Pause();

                // Lesson 1: Assigning Values to Data Types
                AssigningValues();
                GenericFunctions.Pause();

                // Code Lab: Using Value Types
                CodeLabUsingValueTypes();
                GenericFunctions.Pause();

                // Lesson 2: Working with Structs
                Structs();
                GenericFunctions.Pause();

                // Code Lab: Real World Scenario - Books
                CodeLabBooks();
                GenericFunctions.Pause();

                // Lesson 3: Working with Enums
                Enums();
                GenericFunctions.Pause();
                #endregion

                #region Reference Type Notes
                /*
                 * With the introduction of Object-Oriented Programming (OOP), programmers are able to model
                 *     objects with meaningful correlation to the real-world problems they are working to solve.
                 *     These objects take the form of "classes," which is the general term for "reference" data
                 *     types.
                 * 
                 * Interfaces and delegates are also reference data types, and will be discussed later.
                 * 
                 * A class differs from a struct in very specific ways:
                 * - First, as a reference type, the variable only holds a memory address (reference) to the location
                 *       in memory where the data for that instance of the model is stored.
                 * - Second, reference types are stored on the 'heap' as opposed to the 'stack.' As a result, an
                 *       instance of a reference type must be released from memory (either explicitly of by means
                 *       of the .NET garbage collector (GC)). This can be contrasted with the stack, which unwinds
                 *       (or is released) when its scope expires.
                 * - Third, when copied to another variable, a reference type only copies the memory address (reference)
                 *       to the object model, not the object itself. The result is that there are two variables pointing
                 *       to the same object in memory. By contrast, when a value type is copied, the new variable contains
                 *       a copy of the data therein.
                 *       Note: Because of this behavior, you should always be careful when designing 'struct' data types.
                 *             Consider whether memory use becomes high enough to warrant a 'class' instead.
                 * 
                 * Clarifying the .NET Memory Constructs:
                 *   STACK:
                 *     The stack is a memory area reserved for the running application and is not shared with other
                 *         executing programs. Items are added to and removed from the stack throughout execution.
                 *         This is the storage location for value data types, and it has limited available space.
                 *   
                 *   HEAP:
                 *     The heap is a larger area of memory and is used to store the class-based reference data types.
                 *         The stored object for a class type contains both its member variables and any methods in the
                 *         object, so it can be relatively memory-intensive.
                 * 
                 * Class Structural Syntax:
                 *   class MyClass
                 *   {
                 *       // Fields (typically private global variables)
                 *       // Properties (public members - can be variables or property methods pointing to fields)
                 *       // Methods (public or private functions that model behaviors for the class)
                 *       // Events (statements that occur when conditions change during execution)
                 *       // Delegates (types that refer to or act on behalf of methods)
                 *       // Nested Classes (Other classes constructed within the parent class)
                 *   }
                 */
                #endregion

                #region Struct vs Class
                /* NOTES:
                 *  - Prior to OOP (C++), the struct was the only mechanism for grouping related properties
                 *  - Built-in value types are technically structs
                 *
                 * https://www.c-sharpcorner.com/blogs/difference-between-struct-and-class-in-c-sharp
                 * Compared to a class, a struct has the following limitations:
                 * - Struct cannot have a default constructor (a constructor without parameters) or a destructor.
                 * - Structs are value types and are copied on assignment.
                 * - Structs are value types while classes are reference types.
                 * - Structs can be instantiated without using a new operator.
                 * - A struct cannot inherit from another struct or class, and it cannot be the base of a class.
                 *   All structs inherit directly from System.ValueType, which inherits from System.Object.
                 * - Struct cannot be a base class. So, Struct types cannot abstract and are always implicitly sealed.
                 * - Abstract and sealed modifiers are not allowed and struct member cannot be protected or protected internals.
                 * - Function members in a struct cannot be abstract or virtual, and the override modifier is allowed only to the
                 *   override methods inherited from System.ValueType.
                 * - Struct does not allow the instance field declarations to include variable initializers.
                 *   But, static fields of a struct are allowed to include variable initializers.
                 * - A struct can implement interfaces.
                 * - A struct can be used as a nullable type and can be assigned a null value.
                 *
                 * 1	 Structs are value types, allocated either on the stack or inline in containing types
                 *       Classes are reference types, allocated on the heap and garbage-collected.
                 * 2 	 Allocations and de-allocations of value types are in general cheaper than allocations and de-allocations
                 *       of reference types.
                 *       Assignments of large reference types are cheaper than assignments of large value types.
                 * 3	 In structs, each variable contains its own copy of the data (except in the case of the ref and out
                 *       parameter variables), and an operation on one variable does not affect another variable.
                 *       In classes, two variables can contain the reference of the same object and any operation
                 *       on one variable can affect another variable.
                 *
                 * In this way, struct should be used only when you are sure that,
                 * - It logically represents a single value, like primitive types (int, double, etc.).
                 * - It is immutable.
                 * - It should not be boxed and un-boxed frequently.
                 */
                #endregion

                #region Notes on Modifiers
                /*
                 * When creating complex programs with multiple classes, it becomes necessary to be able to limit
                 *     accessibility to classes and/or their members. In C#, accessibility of any given element is
                 *     determined by a modifier applied to it.
                 *     BEST PRACTICE: In general, every class and its member fields, properties, methods, etc. should
                 *                    have an accessibility modifier explicitly included in the code.
                 * 
                 * Syntax:
                 *   // The modifier immediately precedes the declaration of the element, e.g.:
                 *   private int myField;
                 *   public string MyProperty;
                 *   internal class MyClass;
                 * 
                 * Modifiers can also describe the behavior of an element to which they are prepended, indicating things
                 *     like asynchronous functionality (async) or the ability to replace another like-named element (override).
                 * 
                 * C# includes a broad array of modifiers with which you should be familiar:
                 * 
                 *     Modifier  |     Type      | Description
                 *   ------------|---------------|-----------------------------------------------------------------------------
                 *    public     | accessibility | The most permissive level, a 'public' element can be accessed from outside
                 *               |               |   the object. Can be applied to value or reference types and methods.
                 *   ------------|---------------|-----------------------------------------------------------------------------
                 *    private    | accessibility | The least permissive level, a 'private' element can only be accessed from
                 *               |               |   within the object. Can be applied to value or reference types and methods.
                 *   ------------|---------------|-----------------------------------------------------------------------------
                 *    internal   | accessibility | An 'internal' element can be accessed from outside the object, but only by
                 *               |               |   other objects within the same assembly. For example, 'internal' elements
                 *               |               |   in a referenced DLL cannot be accessed by the program that references it.
                 *               |               |   Can be applied to value or reference types and methods.
                 *   ------------|---------------|-----------------------------------------------------------------------------
                 *    protected  | accessibility | A 'protected' element can be accessed only from within the class or derived
                 *               |               |   classes. Other classes, even within the same assembly cannot access it.
                 *               |               |   Can be applied to value or reference types and methods.
                 *   ------------|---------------|-----------------------------------------------------------------------------
                 *    abstract   | behavior      | An 'abstract' class is used as a base model for other classes that inherit
                 *               |               |   its members. An 'abstract' class cannot be instantiated. Can be applied
                 *               |               |   only to classes.
                 *   ------------|---------------|-----------------------------------------------------------------------------
                 *    async      | behavior      | An 'async' method (or lambda expression) executes asynchronously. Once a
                 *               |               |   method of this type begins, other code statements continue to execute
                 *               |               |   while it runs in the background. Can be applied only to methods and
                 *               |               |   lambda expressions.
                 *   ------------|---------------|-----------------------------------------------------------------------------
                 *    const      | behavior      | A 'const' (or constant) element contains a value that cannot be modified.
                 *               |               |   The value must be initialized when the member is declared.
                 *   ------------|---------------|-----------------------------------------------------------------------------
                 *    event      | behavior      | The 'event' modifier declares that the associated element is an event for
                 *               |               |   which an event handler method contains the code to execute when the event
                 *               |               |   is raised.
                 *   ------------|---------------|-----------------------------------------------------------------------------
                 *    extern     | behavior      | The 'extern' modifier indicates that a method (for which you will often only
                 *               |               |   provide the signature) is defined and implemented externally. This is most
                 *               |               |   commonly used when using methods from imported DLLs (those for which you
                 *               |               |   must use the DllImport attribute.
                 *   ------------|---------------|-----------------------------------------------------------------------------
                 *    new        | behavior      | The 'new' modifier (as opposed to the use of the 'new' keyword to invoke a
                 *               |               |   constructor) is used in a class that is inheriting from another class to
                 *               |               |   hide an inherited member of the base class with the same name. Can be 
                 *               |               |   applied to value or reference types.
                 *   ------------|---------------|-----------------------------------------------------------------------------
                 *    override   | behavior      | The 'override' modifier is used when implementing a method that should
                 *               |               |   execute instead of an inherited method with the same signature.
                 *   ------------|---------------|-----------------------------------------------------------------------------
                 *    partial    | behavior      | The 'partial' modifier indicates that the class exists (at least in part) in
                 *               |               |   another file in the assembly.
                 *   ------------|---------------|-----------------------------------------------------------------------------
                 *    readonly   | behavior      | The 'readonly' modifier indicates that the member can only be assigned
                 *               |               |   when it is declared or within the class constructor. This is similar to
                 *               |               |   the behavior of the 'const' modifier, except that this allows creating a
                 *               |               |   non-modifiable reference type variable and permits programmatic setting
                 *               |               |   of the value when the class instance is constructed.
                 *   ------------|---------------|-----------------------------------------------------------------------------
                 *    sealed     | accessibility | When applied to a class, the 'sealed' modifier indicates that the class
                 *               |               |   cannot be inherited.
                 *   ------------|---------------|-----------------------------------------------------------------------------
                 *    static     | accessibility | When applied to a class, the 'static' modifier indicates that the class
                 *               |               |   cannot be instantiated, but that members may be called by invoking the
                 *               |               |   class type name. When applied to members of the class, the 'static'
                 *               |               |   modifier indicates that the member belongs to the class type, not to the
                 *               |               |   instance of the class, so only one instance of the static member exists
                 *               |               |   across all instances of the class.
                 *   ------------|---------------|-----------------------------------------------------------------------------
                 *    unsafe     | behavior      | The 'unsafe' modifier indicates that the affected area of code exists
                 *               |               |   outside of normal .NET memory management. This modifier allows the coder
                 *               |               |   to access a variable's pointer (memory address) among other things.
                 *               |               |   However, when used without extreme caution, this can impact the
                 *               |               |   functionality of objects and garbage collection. As the name suggests,
                 *               |               |   this is unsafe, and should be avoided whenever possible.
                 *   ------------|---------------|-----------------------------------------------------------------------------
                 *    virtual    | behavior      | When applied to a class member method, the 'virtual' modifier explicitly
                 *               |               |   permits that method to be overriden in derived classes using the 'override'
                 *               |               |   modifier. This can also be applied to a property (which is really a method)
                 *               |               |   or an event.
                 *   ------------|---------------|-----------------------------------------------------------------------------
                 *    volatile   | behavior      | When applied to a member field, the 'volatile' modifier indicates that the
                 *               |               |   member can be modified externally to the code (e.g. from the OS or
                 *               |               |   another thread). This functions as a compiler hint and is typically used
                 *               |               |   to enhance performance when multiple threads or sources are modifying the
                 *               |               |   same member. When using the 'lock' statement to serialize access to a
                 *               |               |   field in a multi-threaded application, the 'volatile' modifier is not
                 *               |               |   necessary.
                 *   ------------|---------------|-----------------------------------------------------------------------------
                 */
                #endregion

                #region Chapter Lessons : Part 2
                // Code Lab: Accessing Member Fields of a Class
                CodeLabStudent();
                GenericFunctions.Pause();

                // Code Lab: Accessing Member Methods of a Class
                CodeLabStudentWithMethods();
                GenericFunctions.Pause();

                // Code Lab: Passing Value Types to a Member Method
                CodeLabValuesToMethods();
                GenericFunctions.Pause();

                // Lesson 4: Calling generic types
                CallingGenericTypes();
                GenericFunctions.Pause();

                // Lesson 5: Using bit-shifts
                BitShifts();
                GenericFunctions.Pause();

                // Lesson 6: Using bit-flags
                CheckingBitFlags();
                GenericFunctions.Pause();

                // Lesson 7: Indexers
                CodeLabIndexer();
                GenericFunctions.Pause();
                #endregion

                #region Further Reading
                /*
                 * Built-in data types:
                 * https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/built-in-types
                 *
                 * Struct vs. Class:
                 * https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/choosing-between-class-and-struct
                 */
                #endregion

                #region Code Standards - Variable Naming
                /*
                 * Notes on variable naming:
                 *
                 * All variable names should be meaningful (which results in self-commenting code)
                 *  e.g.:   double accountBalance is better than double amount
                 *          Never use something like double myDouble or double num
                 *
                 * https://betterprogramming.pub/string-case-styles-camel-pascal-snake-and-kebab-case-981407998841
                 *
                 * Public members and properties, should use PascalCase
                 *
                 * Public or private, classes, constants, and method names should use PascalCase
                 *
                 * Private and locally scoped variables should use camelCase
                 *
                 * Do not use snake_case or kebab-case
                 *
                 * Never use ALL_CAPS
                 *
                 * Do not use Hungarian notation like strName or arr10Numbers
                 *  https://en.wikipedia.org/wiki/Hungarian_notation
                 */
                #endregion

                #region Bonus Lessons
                // Illustrate the difference between C# aliases and .NET System data types
                BonusAliasVersusSystemType();
                GenericFunctions.Pause();

                // Illustrate risks with value wrap-around
                BonusWrapAroundAndOverflow();
                GenericFunctions.Pause();

                // Illustrate the difference between value and reference types
                BonusValueVersusReference();
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
        // Lesson 1: Assigning Values to Data Types
        private static void AssigningValues()
        {
            Console.Clear();
            Console.WriteLine("Chapter 3 : Lesson 1");
            Console.WriteLine();
            Console.WriteLine("Assigning Values");
            Console.WriteLine("----------------");

            // Assigning one value type to another
            int myInt;
            int secondInt;

            // myInt will be assigned the value 2
            myInt = 2;

            // secondInt will also contain value 2 after this statement executes
            secondInt = myInt;

            Console.WriteLine($"myInt = {myInt}");
            Console.WriteLine($"secondInt = {secondInt}");
        }

        // Lesson 2: Working with Structs
        private static void Structs()
        {
            Console.Clear();
            Console.WriteLine("Chapter 3 : Lesson 2");
            Console.WriteLine();
            Console.WriteLine("Working with Structs");
            Console.WriteLine("--------------------");

            /*
             * A data structure (or 'struct') is a user-defined value type that allows you to meaningfully
             *     collect and use related data.
             * 
             * The struct is a 'complex' data type, since it can contain fields of multiple simple data types.
             * This differentiates it from an array or collection, which can only be of a single data type.
             * 
             * A struct can contain both variables (properties) and methods that act on those variables.
             * 
             * Syntax:
             *   struct Name
             *   {
             *       member_field1;
             *       member_field1;
             *       ...
             *       member_field_n;
             *       
             *       constructor(value1, value2, ..., value_n)
             *       {
             *           // Constructor must set values for all fields
             *           field1 = value1;
             *           field2 = value2;
             *           ...
             *           field_n = value_n;
             *       }
             *       
             *       member_method1(){}
             *       member_method2(){}
             *       ...
             *       member_method_n(){}
             *   }
             */

            /*
             * Further down the code, you will find the declaration of a struct for a Person
             */

            #pragma warning disable S6562 // DateTimeKind omitted for lesson
            var birth = new DateTime(1985, 6, 15);
            #pragma warning restore S6562
            int age = DateTime.Today.Year - birth.Year;
            if (DateTime.Today.DayOfYear < birth.DayOfYear) age--;

            var me = new Person("Alex", "Turner", (byte)age);
            Console.WriteLine(me.Greet());
        }

        // Lesson 3: Working with Enums
        private static void Enums()
        {
            Console.Clear();
            Console.WriteLine("Chapter 3 : Lesson 3");
            Console.WriteLine();
            Console.WriteLine("Working with Enums");
            Console.WriteLine("------------------");

            /*
             * An enum (or enumeration) allows you to create a list of names that refer to constant values
             * of a single data type
             * 
             * Syntax:
             *   enum Name : (optional data type)
             *   {
             *       Name1 = (optional value1),
             *       Name2 = (optional value2),
             *       ...
             *       Name_n = (optional value_n)
             *   }
             * 
             * By default, an enum is of data type System.Int32.
             * The default values for the enumerated items start with 0 and ascend by 1
             */

            if (Enum.TryParse("Jul", out Months selected)) Console.WriteLine($"Jul is month {(byte)selected}");
            Console.WriteLine($"The 8th month is {Enum.GetName(typeof(Months), 8)}");

            Console.WriteLine();
            Console.WriteLine("Press any key to run the Using Enums code lab...");
            GenericFunctions.Pause();
            CodeLabEnums();
        }

        // Lesson 4: Calling generic types
        private static void CallingGenericTypes()
        {
            Console.Clear();
            Console.WriteLine("Chapter 3 : Extra Lesson 1");
            Console.WriteLine();
            Console.WriteLine("Calling Generic Types");
            Console.WriteLine("---------------------");
            var queue = new GenericQueue<string>();

            queue.Add("Alex");
            queue.Add("Andy");
            queue.Add("Alan");

            Console.WriteLine("Queue:");

            while (queue.Waiting())
            {
                Console.WriteLine($"Now serving {queue.Next()}");
            }

            Console.WriteLine();
            Console.WriteLine("Stack:");

            var stack = new GenericStack<string>();

            stack.Add("Alex");
            stack.Add("Andy");
            stack.Add("Alan");

            while (stack.Waiting())
            {
                Console.WriteLine($"Now serving {stack.Next()}");
            }
        }

        // Lesson 5: Using bit-shifts
        private static void BitShifts()
        {
            Console.Clear();
            Console.WriteLine("Chapter 3 : Extra Lesson 2");
            Console.WriteLine();
            Console.WriteLine("Using BitShift");
            Console.WriteLine("--------------");

            int ig = 1;
            long lg = 1;
            // Shift i one bit to the left. The result is 2.
            Console.WriteLine("ig << 1 = 0x{0:x}", ig << 1);
            // In binary, 33 is 100001. Because the value of the five low-order
            // bits is 1, the result of the shift is again 2. 
            Console.WriteLine("ig << 33 = 0x{0:x}", ig << 33);
            // Because the type of lg is long, the shift is the value of the six
            // low-order bits. In this example, the shift is 33, and the value of
            // lg is shifted 33 bits to the left.
            //     In binary:     10 0000 0000 0000 0000 0000 0000 0000 0000 
            //     In hexadecimal: 2    0    0    0    0    0    0    0    0
            Console.WriteLine("lg << 33 = 0x{0:x}", lg << 33);

            // This program takes a random integer and then shifts it right.
            // ... Then it shifts it left.
            // ... It displays the bits and also the decimal representation.
            int value1 = new Random().Next();
            Console.WriteLine();
            Console.WriteLine("Shifting right:");
            for (int i = 0; i < 32; i++)
            {
                int shift = value1 >> i;
                Console.WriteLine("{0} = {1}", GetIntBinaryString(shift), shift);
            }
            Console.WriteLine();
            Console.WriteLine("Shifting left:");
            for (int i = 0; i < 32; i++)
            {
                int shift = value1 << i;
                Console.WriteLine("{0} = {1}", GetIntBinaryString(shift), shift);
            }
        }

        // Lesson 6: Using bit-flags
        private static void CheckingBitFlags()
        {
            Console.Clear();
            Console.WriteLine("Chapter 3 : Extra Lesson 3");
            Console.WriteLine();
            Console.WriteLine("Checking Bit Flags");
            Console.WriteLine("------------------");

            byte b = 73; // 01001001
            string s = GetIntBinaryString(b);
            s = s.Substring(s.Length - 8);
            Console.WriteLine($"{b} = {s}");

            for (int i = 0; i < 8; i++)
            {
                Console.WriteLine($"Bit {i} is {(b.IsBitSet(i) ? "" : "not ")}set");
            }
        }

        #region Lesson Methods Helper Functions
        // Returns the sum of two integers, demonstrating the use of named parameters
        private static int Sum(int value1, int value2)
        {
            Console.WriteLine("In method sum()");
            return value1 + value2;
        }

        // Demonstrates that value types are passed by value, so changes made to the values
        // in the method do not affect the original values
        private static void ChangeValues(int value1, int value2)
        {
            Console.WriteLine("In changeValues()");
            Console.WriteLine("value1 is " + value1);  // outputs 2
            Console.WriteLine("value2 is " + value2);  // outputs 3
            Console.WriteLine();
            Console.WriteLine("Changing values");

            value1--;
            value2 += 5;

            Console.WriteLine();
            Console.WriteLine("value1 is now " + value1);  // outputs 1
            Console.WriteLine("value2 is now " + value2);  // outputs 8
        }

        // Demonstrates that reference types are passed by reference, so changes made to the values
        // do affect the original values
        private static void ChangeName(Student refValue)
        {
            Console.WriteLine();
            Console.WriteLine("In changeName()");
            refValue.FirstName = "George";
        }

        // Helper function to return a string representation of an integer in binary format
        private static string GetIntBinaryString(int n)
        {
            char[] b = new char[32];
            int pos = 31;
            int i = 0;

            while (i < 32)
            {
                if ((n & (1 << i)) != 0)
                {
                    b[pos] = '1';
                }
                else
                {
                    b[pos] = '0';
                }
                pos--;
                i++;
            }
            return new string(b);
        }
        #endregion
        #endregion

        #region Code Labs
        // See \Textbook Resources\MCSD Certification Code and Test Questions\03\Chapter3\value_type_alias
        private static void CodeLabTypeAliases()
        {
            // create a variable to hold a value type using the alias form
            // but don't assign a variable
            int myInt = 0;
            int myNewInt = new();

            // create a variable to hold a .NET value type
            // this type is the .NET version of the alias form int
            // note the use of the keyword new, we are creating an object from 
            // the System.Int32 class
            System.Int32 myInt32 = new();

            // myInt is assigned above, so this reads back cleanly. Try changing
            // the declaration to just "int myInt;" (no assignment) and this line
            // will fail to compile, C# won't let you read a local variable that
            // was only declared, never given a value
            Console.WriteLine($"myInt = {myInt}");

            // print out the default value assigned to an int variable
            // that had no value assigned previously
            Console.WriteLine($"myNewInt = {myNewInt}");

            // this statement will work fine and will print out the default value for
            // this type, which in this case is 0
            Console.WriteLine($"myInt32 = {myInt32}");
        }

        // See \Textbook Resources\MCSD Certification Code and Test Questions\03\Chapter3\using_value_types
        private static void CodeLabUsingValueTypes()
        {
            // declare some numeric data types
            int myInt;
            double myDouble;
            byte myByte;
            char myChar;
            decimal myDecimal;
            float myFloat;
            long myLong;
            short myShort;
            bool myBool;

            // assign values to these types and then
            // print them out to the console window
            // also use the sizeOf operator to determine
            // the number of bytes taken up be each type

            myInt = 5000;
            Console.WriteLine("Integer");
            Console.WriteLine($"Value: {myInt}");
            Console.WriteLine($"Type: {myInt.GetType()}");
            Console.WriteLine($"Size (bytes): {sizeof(int)}");
            Console.WriteLine();

            myDouble = 5000.0;
            Console.WriteLine("Double");
            Console.WriteLine($"Value: {myDouble}");
            Console.WriteLine($"Type: {myDouble.GetType()}");
            Console.WriteLine($"Size (bytes): {sizeof(double)}");
            Console.WriteLine();

            myByte = 254;
            Console.WriteLine("Byte");
            Console.WriteLine($"Value: {myByte}");
            Console.WriteLine($"Type: {myByte.GetType()}");
            Console.WriteLine($"Size (bytes): {sizeof(byte)}");
            Console.WriteLine();

            myChar = 'r';
            Console.WriteLine("Char");
            Console.WriteLine($"Value: {myChar}");
            Console.WriteLine($"Type: {myChar.GetType()}");
            Console.WriteLine($"Size (bytes): {sizeof(char)}");
            Console.WriteLine();

            myDecimal = 20987.89756M;
            Console.WriteLine("Decimal");
            Console.WriteLine($"Value: {myDecimal}");
            Console.WriteLine($"Type: {myDecimal.GetType()}");
            Console.WriteLine($"Size (bytes): {sizeof(decimal)}");
            Console.WriteLine();

            myFloat = 254.09F;
            Console.WriteLine("Float");
            Console.WriteLine($"Value: {myFloat}");
            Console.WriteLine($"Type: {myFloat.GetType()}");
            Console.WriteLine($"Size (bytes): {sizeof(float)}");
            Console.WriteLine();

            myLong = 2544567538754;
            Console.WriteLine("Long");
            Console.WriteLine($"Value: {myLong}");
            Console.WriteLine($"Type: {myLong.GetType()}");
            Console.WriteLine($"Size (bytes): {sizeof(long)}");
            Console.WriteLine();

            myShort = 3276;
            Console.WriteLine("Short");
            Console.WriteLine($"Value: {myShort}");
            Console.WriteLine($"Type: {myShort.GetType()}");
            Console.WriteLine($"Size (bytes): {sizeof(short)}");
            Console.WriteLine();

            myBool = true;
            Console.WriteLine("Boolean");
            Console.WriteLine($"Value: {myBool}");
            Console.WriteLine($"Type: {myBool.GetType()}");
            Console.WriteLine($"Size (bytes): {sizeof(bool)}");
            Console.WriteLine();
        }

        // Relate to P. 70 - Real World Coding Scenario - Creating Structs
        private static void CodeLabBooks()
        {
            var myBook = new Book("MCSD Certification Toolkit (Exam 70-483)", "Certification", "Covaci, Tiberiu", 648, 1, 81118612095, "Softcover");

            Console.WriteLine($"Title: {myBook.Title}");
            Console.WriteLine($"Category: {myBook.Category}");
            Console.WriteLine($"Author: {myBook.Author}");
            Console.WriteLine($"NumPages: {myBook.NumPages}");
            Console.WriteLine($"CurrentPage: {myBook.CurrentPage}");
            Console.WriteLine($"ISBN: {myBook.ISBN}");
            Console.WriteLine($"CoverStyle: {myBook.CoverStyle}");
            Console.WriteLine();

            myBook.NextPage();
            myBook.PrevPage();
        }

        // Code Lab: Working with Enums
        private static void CodeLabEnums()
        {
            string name = Enum.GetName(typeof(Months), 8);
            Console.WriteLine("The 8th month in the enum is " + name);

            Console.WriteLine("The underlying values of the Months enum:");
            foreach (byte values in Enum.GetValues(typeof(Months)))
            {
                Console.WriteLine($"value = {values}");
            }
        }

        // Code Lab: Working with Class Fields
        private static void CodeLabStudent()
        {
            Student firstStudent = new();
            Student.StudentCount++;
            Student secondStudent = new();
            Student.StudentCount++;

            firstStudent.FirstName = "John";
            firstStudent.LastName = "Smith";
            firstStudent.Grade = "six";

            secondStudent.FirstName = "Tom";
            secondStudent.LastName = "Thumb";
            secondStudent.Grade = "two";

            Console.WriteLine($"firstStudent.FirstName = {firstStudent.FirstName}");
            Console.WriteLine($"secondStudent.FirstName = {secondStudent.FirstName}");
            Console.WriteLine($"Student.StudentCount = {Student.StudentCount}");
        }

        // Code Lab: Working with Class Methods
        private static void CodeLabStudentWithMethods()
        {
            Student firstStudent = new();
            Student.StudentCount++;
            Student secondStudent = new();
            Student.StudentCount++;

            firstStudent.FirstName = "John";
            firstStudent.LastName = "Smith";
            firstStudent.Grade = "six";

            secondStudent.FirstName = "Tom";
            secondStudent.LastName = "Thumb";
            secondStudent.Grade = "two";

            firstStudent.DisplayName();
        }

        // Code Lab: Passing Value Types to a Member Method
        private static void CodeLabValuesToMethods()
        {
            int num1 = 2;
            int num2 = 3;
            int result;

            // NOTE: Our coding standard here would be to use an initializer
            #pragma warning disable IDE0017 // Intentionally not using an initializer to illustrate the point of the lesson
            var firstStudent = new Student();

            firstStudent.FirstName = "John";
            firstStudent.LastName = "Smith";
            firstStudent.Grade = "six";
            #pragma warning restore IDE0017

            // Here's a good place to point out that we can call methods using named parameters
            // This would be our standard call (passing the values in signature order)
            // result = Sum(num1, num2);
            // Here, the values are passed in reverse order, but using named parameters,
            //   they end up in the same variables in the function
            result = Sum(value2: num2, value1: num1);
            Console.Write("Sum is: ");
            Console.WriteLine(result);  // outputs 5
            Console.WriteLine();

            ChangeValues(num1, num2);
            Console.WriteLine();
            Console.WriteLine("Back from changeValues()");
            Console.WriteLine($"num1 = {num1}");  // outputs 2
            Console.WriteLine($"num2 = {num2}");  // outputs 3

            Console.WriteLine();
            Console.WriteLine("First name for firstStudent is " + firstStudent.FirstName);
            ChangeName(firstStudent);
            Console.WriteLine();
            Console.WriteLine("First name for firstStudent is " + firstStudent.FirstName);
        }

        // Illustrate use of an indexer
        private static void CodeLabIndexer()
        {
            var myIp = new IpAddress();

            // Initialize to all zeroes
            Console.WriteLine("IP address bits:");
            for (int i = 0; i < 32; i++)
            {
                myIp[i] = 0;
                Console.Write($"{myIp[i]} ");
            }

            Console.WriteLine();
        }
        #endregion

        #region Bonus Lesson Methods
        // Illustrate the difference between C# aliases and .NET System data types
        private static void BonusAliasVersusSystemType()
        {
            // Uncommenting the lines below will show that the code won't compile with the variable unassigned
            //int myInt;
            //Console.WriteLine($"My string is [{myInt}]");

            System.Int32 mySystemInt = new();
            Console.WriteLine($"My System int is [{mySystemInt}]"); // Print zero
        }

        // Illustrate risks with value wrap-around
        private static void BonusWrapAroundAndOverflow()
        {
            short num = 0;
            do
            {
                // When your code does not throw an exception but generates an anomalous value,
                //

                // If the value is equal to System.Int16.MaxValue (32767),
                //   incrementing it wraps around and returns System.Int16.MainValue (-32768)
                num++;

                // I'll just write out the last two numbers produced
                if (num > 32766 || num < 0) Console.WriteLine($"num = {num}");

                // Because of the wrap-around, I have added this jump to escape an otherwise infinite loop
                if (num < 0) break;
            } while (num <= 32767);

            Console.WriteLine(ContinueMessage);
            Console.ReadKey();
            Console.Clear();

            /*
             * Another wrap-around case is more subtle.
             * This gets into how numbers are stored as binary digits.
             *
             * In C#, the leftmost bit in signed numeric values is used for the +- sign
             *   where 0 means positive and 1 means negative
             */

            // For example, if we use a 32-bit integer (System.Int32 = int),
            //   that means that we only have 31 bits for the numeric value

            int x = 1;
            Console.WriteLine($"Shift  0:{x,13} = {Convert.ToString(x, 2).PadLeft(32, '0')}");

            // Each time we shift the bit one place to the left, we double the value of the integer
            for (int i = 1; i < 32; i++)
            {
                x <<= 1;
                if (i == 31)
                {
                    Console.WriteLine();
                    Console.WriteLine("The current value in binary is 0100 0000 0000 0000 0000 0000 0000 0000");
                    Console.WriteLine("That's 1,073,741,824 in decimal");
                    Console.WriteLine("What do you think the value will be at the next bit-shift?");
                    Console.WriteLine(ContinueMessage);
                    Console.ReadKey();
                }
                Console.WriteLine($"Shift {i,2}:{x,13} = {Convert.ToString(x, 2).PadLeft(32, '0')}");
            }
            Console.WriteLine();
            Console.WriteLine("We have overflowed into the sign bit!");
            Console.WriteLine();

            Console.WriteLine();
            Console.WriteLine("Here's some more weirdness.");
            Console.WriteLine("Binary negatives count backwards!");
            Console.WriteLine(ContinueMessage);
            Console.ReadKey();
            Console.WriteLine();
            x++;
            Console.WriteLine($"adding 1 = {x,9} = {Convert.ToString(x, 2).PadLeft(32, '0')}");
            x++;
            Console.WriteLine($"adding 2 = {x,9} = {Convert.ToString(x, 2).PadLeft(32, '0')}");
            x = -1;
            Console.WriteLine($"{x,22} = {Convert.ToString(x, 2).PadLeft(32, '0')}");

            Console.WriteLine();
            Console.WriteLine(ContinueMessage);
            Console.ReadKey();
            Console.Clear();

            // So, what happens if we use a regular arithmetic operator to perform the same process?

            x = 1;
            Console.WriteLine($"      {x,13} = {Convert.ToString(x, 2).PadLeft(32, '0')}");

            // Each time we shift the bit one place to the left, we double the value of the integer
            for (int i = 1; i < 32; i++)
            {
                x *= 2;
                if (i == 31)
                {
                    Console.WriteLine();
                    Console.WriteLine("The current value in binary is 0100 0000 0000 0000 0000 0000 0000 0000");
                    Console.WriteLine("That's 1,073,741,824 in decimal");
                    Console.WriteLine("What do you think will happen when we double from here?");
                    Console.WriteLine("Will we still overflow, or will C# throw an exception?");
                    Console.WriteLine(ContinueMessage);
                    Console.ReadKey();
                }
                Console.WriteLine($"* 2 = {x,13} = {Convert.ToString(x, 2).PadLeft(32, '0')}");
            }
            Console.WriteLine();
            Console.WriteLine("We still overflowed into the sign bit!");
        }

        // Illustrate the difference between value and reference types
        private static void BonusValueVersusReference()
        {
            var valueCoords = new ValueCoordinates(0, 0);
            Console.WriteLine($"Value Coordinates: {valueCoords.X},{valueCoords.Y}"); // 0,0

            // Passing a value type by reference creates a copy - the original variable is not modified
            MoveXAxis(valueCoords);
            Console.WriteLine();
            Console.WriteLine("After calling by value");
            Console.WriteLine($"Value Coordinates: {valueCoords.X},{valueCoords.Y}"); // 0,0

            // Passing a value type explicitly by reference passes the memory address - the original variable is modified
            MoveXAxis(ref valueCoords);
            Console.WriteLine();
            Console.WriteLine("After calling by reference");
            Console.WriteLine($"Value Coordinates: {valueCoords.X},{valueCoords.Y}"); // 1,0

            var refCoords = new ReferenceCoordinates(0, 0);
            Console.WriteLine();
            Console.WriteLine($"Reference Coordinates: {refCoords.X},{refCoords.Y}"); // 0,0

            // Passing a reference type passes the memory address - the original variable is modified
            MoveXAxis(refCoords);
            Console.WriteLine();
            Console.WriteLine("Reference type always calls by reference");
            Console.WriteLine($"Reference Coordinates: {refCoords.X},{refCoords.Y}"); // 1,0
        }

        // Show effect of changing a value on a struct (value type) passed to the method
        private static void MoveXAxis(ValueCoordinates coords, int distance = 1)
        {
            coords.X += distance;
        }

        // Show effect of changing a value on a struct (value type) passed to the method by reference
        private static void MoveXAxis(ref ValueCoordinates coords, int distance = 1)
        {
            coords.X += distance;
        }

        // Show effect of changing a value on a class (reference type) passed to the method
        private static void MoveXAxis(ReferenceCoordinates coords, int distance = 1)
        {
            coords.X += distance;
        }
        #endregion

        #region Data Types
        /// <summary>
        /// Relate to Lesson 2: Structs
        /// Defines a Person
        /// </summary>
        public struct Person
        {
            #region Fields/Properties
            /// <summary>
            /// Person's First Name
            /// </summary>
            public string FirstName;

            /// <summary>
            /// Person's Last Name
            /// </summary>
            public string LastName;

            /// <summary>
            /// Person's Age
            /// </summary>
            public byte Age;
            #endregion

            #region Constructor (Must set every field/property in the struct)
            /// <summary>
            /// Create a new instance of the Person struct
            /// </summary>
            /// <param name="firstName">Person's First Name</param>
            /// <param name="lastName">Person's Last Name</param>
            /// <param name="age">Person's Age</param>
            #pragma warning disable IDE0290 // Using standard constructor instead of primary constructor to illustrate the lesson point
            public Person(string firstName, string lastName, byte age)
            #pragma warning restore IDE0290
            {
                FirstName = firstName;
                LastName = lastName;
                Age = age;
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// Provide Personal Greeting
            /// </summary>
            /// <returns>Greeting String</returns>
            #pragma warning disable IDE0251 // Not adding 'readonly' to the method to illustrate the lesson point
            public string Greet()
            #pragma warning restore IDE0251
            {
                return $"Hello. My name is {FirstName} {LastName}. I am {Age} years old.";
            }
            #endregion
        }

        /// <summary>
        /// Relate to P. 70 - Real World Coding Scenario - Creating Structs
        /// Defines a Book
        /// </summary>
        public struct Book
        {
            #region Public Fields
            /// <summary>
            /// Book Title
            /// </summary>
            public string Title;

            /// <summary>
            /// Category name.
            /// </summary>
            public string Category;

            /// <summary>
            /// Gets or sets the author name.
            /// </summary>
            public string Author;

            /// <summary>
            /// Number of pages.
            /// </summary>
            public int NumPages;

            /// <summary>
            /// Represents the current page number.
            /// </summary>
            public int CurrentPage;

            /// <summary>
            /// International Standard Book Number (ISBN) associated with the publication.
            /// </summary>
            public double ISBN;

            /// <summary>
            /// Represents the style of the cover.
            /// </summary>
            public string CoverStyle;
            #endregion

            #region Constructor
            /// <summary>
            /// Initializes a new instance of the <c>Book</c> class with title, category, author, page information,
            /// ISBN, and cover style.
            /// </summary>
            /// <remarks>If <paramref name="currentPage"/> is less than 1, it is set to 1. If it is
            /// greater than <paramref name="numPages"/>, it is set to <paramref name="numPages"/>.</remarks>
            /// <param name="title">The book title.</param>
            /// <param name="category">The book category or genre.</param>
            /// <param name="author">The author name.</param>
            /// <param name="numPages">The total number of pages.</param>
            /// <param name="currentPage">The current page number.</param>
            /// <param name="isbn">The ISBN number.</param>
            /// <param name="coverStyle">The cover style.</param>
            public Book(string title, string category, string author, int numPages, int currentPage, double isbn, string coverStyle)
            {
                Title = title;
                Category = category;
                Author = author;
                NumPages = numPages;
                CurrentPage = currentPage;
                if (CurrentPage < 1) CurrentPage = 1;
                if (CurrentPage > NumPages) CurrentPage = NumPages;
                ISBN = isbn;
                CoverStyle = coverStyle;
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// Advances to the next page when the current page is before the last page.
            /// </summary>
            /// <remarks>Writes the updated current page number to the console, or indicates that the
            /// end of the book has been reached.</remarks>
            public void NextPage()
            {
                if (CurrentPage < NumPages)
                {
                    CurrentPage++;
                    Console.WriteLine("Current page is now " + CurrentPage);
                }
                else
                {
                    Console.WriteLine("At end of book!");
                }
            }

            /// <summary>
            /// Moves to the previous page when the current page is greater than 1.
            /// </summary>
            /// <remarks>Writes the updated page number to the console after moving back one page. If
            /// already on the first page, writes a message indicating the beginning of the book.</remarks>
            public void PrevPage()
            {
                if (CurrentPage > 1)
                {
                    CurrentPage--;
                    Console.WriteLine("Current page is now " + CurrentPage);
                }
                else
                {
                    Console.WriteLine("At beginning of book!");
                }
            }
            #endregion
        }

        /// <summary>
        /// Relate to p. 73 - enum with non-default type and initializer
        /// </summary>
        public enum Months : byte
        {
            Jan = 1,
            Feb,
            Mar,
            Apr,
            May,
            Jun,
            Jul,
            Aug,
            Sep,
            Oct,
            Nov,
            Dec
        }

        /// <summary>
        /// Defines an X/Y coordinate location as a value type
        /// </summary>
        public struct ValueCoordinates
        {
            #region Fields/Properties
            /// <summary>
            /// Cartesian X coordinate
            /// </summary>
            public int X;

            /// <summary>
            /// Cartesian Y coordinate
            /// </summary>
            public int Y;
            #endregion

            #region Constructor (Must set every field/property in the struct)
            /// <summary>
            /// Create a new instance of the ValueCoordinates struct
            /// </summary>
            /// <param name="x">Cartesian X coordinate</param>
            /// <param name="y">Cartesian Y coordinate</param>
            #pragma warning disable IDE0290 // Not using primary constructor to illustrate the lesson point
            public ValueCoordinates(int x, int y)
            #pragma warning restore IDE0290
            {
                X = x;
                Y = y;
            }
            #endregion
        }

        /// <summary>
        /// Defines an X/Y coordinate location as a reference type
        /// </summary>
        public class ReferenceCoordinates
        {
            #region Properties
            /// <summary>
            /// Cartesian X coordinate
            /// </summary>
            public int X { get; set; }

            /// <summary>
            /// Cartesian Y coordinate
            /// </summary>
            public int Y { get; set; }
            #endregion

            #region Constructor (Must set every field/property in the struct)
            /// <summary>
            /// Create a new instance of the ReferenceCoordinates class
            /// </summary>
            /// <param name="x">Cartesian X coordinate</param>
            /// <param name="y">Cartesian Y coordinate</param>
            #pragma warning disable IDE0290 // Not using primary constructor to illustrate the lesson point
            public ReferenceCoordinates(int x, int y)
            #pragma warning restore IDE0290
            {
                X = x;
                Y = y;
            }
            #endregion
        }
        #endregion
    }
}
#pragma warning restore S125

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
