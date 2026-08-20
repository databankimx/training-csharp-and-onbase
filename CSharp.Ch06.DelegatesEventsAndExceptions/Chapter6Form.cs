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
using System.Threading;
using System.Windows.Forms;
#endregion

#pragma warning disable S125 // Allow commented code in lessons
namespace CSharp.Ch06.DelegatesEventsAndExceptions
{
    /// <summary>
    /// Example form for demonstrating simple delegation
    /// </summary>
    public partial class Chapter6Form : Form
    {
        #region Chapter Notes
        /*
         * Definition: Delegate
         * A delegate is a data type that defines the parameters and return type of a method rather than a value or class.
         *
         * This is useful when you want a named function to behave differently depending on the situation.
         * Think of this as a version of (if... else if... else...) that can be scoped to encompass events in addition to variables.
         *
         * In some ways, it's more similar to an interface, in that it creates a signature that must be implemented later.
         *
         * The MSDN provides some rules of thumb for selecting when to use a delegate or an interface
         *      https://msdn.microsoft.com/en-us/library/ms173173.aspx
         *
         * Both delegates and interfaces enable a class designer to separate type declarations and implementation.
         *  A given interface can be inherited and implemented by any class or struct. A delegate can be created for a
         *  method on any class, as long as the method fits the method signature for the delegate. An interface reference
         *  or a delegate can be used by an object that has no knowledge of the class that implements the interface or
         *  delegate method. Given these similarities, when should a class designer use a delegate and when should it use
         *  an interface?
         *
         * Use a delegate in the following circumstances:
         *      - An eventing design pattern is used.
         *      - It is desirable to encapsulate a static method.
         *      - The caller has no need to access other properties, methods, or interfaces on the object implementing the method.
         *      - Easy composition is desired.
         *      - A class may need more than one implementation of the method.
         *
         * Use an interface in the following circumstances:
         *      - There is a group of related methods that may be called.
         *      - A class only needs one implementation of the method.
         *      - The class using the interface will want to cast that interface to other interface or class types.
         *      - The method being implemented is linked to the type or identity of the class: for example, comparison methods.
         *
         * One good example of using a single-method interface instead of a delegate is IComparable or the generic version,
         *  IComparable<T>. IComparable declares the CompareTo method, which returns an integer that specifies a less than,
         *  equal to, or greater than relationship between two objects of the same type. IComparable can be used as the basis
         *  of a sort algorithm. Although using a delegate comparison method as the basis of a sort algorithm would be valid,
         *  it is not ideal. Because the ability to compare belongs to the class and the comparison algorithm does not change
         *  at run time, a single-method interface is ideal.
         *
         * Some Notes about Delegates:
         *  https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/
         *  - Delegates are like C++ function pointers but are type safe
         *  - Delegates allow methods to be passed as parameters (this is a big deal!)
         *  - Delegates can be used to define callback methods
         *  - Delegates can be chained together; for example, multiple methods can be called on a single event
         *  - Methods do not have to match the delegate type exactly
         *    - https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/covariance-contravariance/using-variance-in-delegates
         *  - Since C# 2.0, Delegates extend to the concept of Anonymous Methods (see below)
         *    - Prior to C# 2.0, *all* delegates had to be Named Methods
         *  - You can call a delegate asynchronously, using BeginInvoke and EndInvoke
         *
         * Syntax:
         * [accessor] delegate <returnType> DelegateName([parameters]);
         *
         * Code Convention:
         * Consider adding a suffix of "Delegate" to the name for a delegate, or "Callback" if it's used as a callback
         *
         * Some notes on Anonymous Methods
         *  - Allow code blocks to be passed as parameters in place of a separately defined method
         *  - Since C# 3.0, this includes the ability to use Lambda Expressions (or Anonymous Functions)
         *    - Anonymous Functions (Lambdas) have now superseded Anonymous Methods and are the preferred
         *      method for writing inline code.
         *    - The exception to this rule is:
         *      - When you will not be using any of the passed parameters (a good example is an event handler
         *        where you're not using the Event Args or the Sender object), you can omit the parameter list.
         *        This option is not available with Lambdas
         *  - Important!
         *    - Because the parameter scope in an anonymous method is the code block, the following statements
         *      (flow controls) would result in errors if called there:
         *      - goto (Separate Note: If I ever see goto on a code review, you'd better be prepared to defend it)
         *      - break
         *      - continue
         *
         * Predefined Delegates
         *   - Action : A predefined delegate returning void (0 to 18 arguments)
         *              Action<T> actionName = new Action<T>(Code);
         *              actionName([parameters]);
         *
         *   - Func   : A predefined delegate returning a value (0 to 18 arguments)
         *              Func<T> functionName = Func<T>(Code);
         *              variable = functionName([parameters]);
         *
         * Predefined delegates are especially useful when creating event handlers, as these create named
         *   delegates, so it is possible to unsubscribe them.
         *
         * Here's a good discussion of *why* you can't unsubscribe an anonymous function (lambda)
         *   https://stackoverflow.com/questions/25563518/why-cant-i-unsubscribe-from-an-event-using-a-lambda-expression/
         */
        #endregion

        #region Private Members
        // Defining a delegate
        private delegate float FunctionDelegate(float x);

        // Storing an instance of the delegate in a variable
        private FunctionDelegate theFunction;

        // Declaring an event and its handler
        public delegate void MyEventHandler();
        public event MyEventHandler MyEvent;

        // Counter for form clicks
        private int clicks;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the Chapter6Form class
        /// </summary>
        public Chapter6Form()
        {
            InitializeComponent();

            // Assign the form-load event handler to the Load event
            Load += Chapter6Form_Load;

            // Assign the form-closing event handler to the FormClosing event
            FormClosing += Chapter6Form_FormClosing;

            // Bind an anonymous method (delegate) to an event
            // Note: Because this must accept an event handler, the
            //       Signature is fixed. You can remove the signature
            //       entirely (see comment below), but you can't change the 
            //       number or type of arguments
            BtnAnon.Click += delegate (object o, EventArgs e)
            {
                clicks++;
                if (clicks > 3)
                {
                    // Raise an event
                    MyEvent?.Invoke();
                }
                else
                {
                    MessageBox.Show($@"I'm anonymous! - Clicked [{clicks}/3] times");
                }
            };

            // Assign the state-change event handler to the CheckedChanged event
            CbTracked.CheckedChanged += CbTracked_CheckChanged;

            // Trigger this delegate one time
            StartThread();
        }
        #endregion

        #region Event Handlers
        // Method executes when form loads
        private void Chapter6Form_Load(object sender, EventArgs e)
        {
            // Assign the function to the delegate
            theFunction = DelegatedFunctionForLoad;
            MessageBox.Show(theFunction(1).ToString(CultureInfo.CurrentCulture));

            // Assigning an anonymous *function* (lambda expression) to the event
            // However, I am *NOT!* raising the event
            MyEvent = () => MessageBox.Show(@"Too many clicks!");
        }

        // Method executes when form closes
        private void Chapter6Form_FormClosing(object sender, EventArgs e)
        {
            // Assign a different function to the delegate
            theFunction = DelegatedFunctionForUnload;
            MessageBox.Show(theFunction(1).ToString(CultureInfo.CurrentCulture));
        }

        // Method executes when checkbox is checked or unchecked
        private void CbTracked_CheckChanged(object sender, EventArgs e)
        {
            MessageBox.Show($@"Checkbox is {(CbTracked.Checked ? "" : "not ")}checked.", @"Checkbox Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Launch the graph form when the button is clicked
        private void BtnGraphForm_Click(object sender, EventArgs e)
        {
            new GraphForm().Show();
            BtnGraphForm.Visible = false;
        }
        #endregion

        #region Delegate Functions
        // Create a function used by the delegate
        private static float DelegatedFunctionForLoad(float x)
        {
            return (float)(12 * Math.Sin(3 * x) / (1 + Math.Abs(x)));
        }

        // Create another function used by the delegate
        private static float DelegatedFunctionForUnload(float x)
        {
            return (float)(12 * Math.Sin(2 * x) / (1 + Math.Abs(x)));
        }

        // An excellent example of a place to use an anonymous method is when code is used in only one place
        // For example, when starting a new thread, you may have simple code that is used every time, but is
        // never called from anywhere else in your code. In an instance like this, using a delegate in the
        // form of an anonymous method saves the overhead of making a method that is not called from anywhere else.
        private static void StartThread()
        {
            // This overload of the Thread constructor takes a plain ThreadStart delegate (no parameters).
            // Compare this to CSharp.Ch06.Supplemental.06.ParameterizedThreadStart, which uses the
            //     ParameterizedThreadStart overload instead, letting you pass a single object argument
            //     into the thread's entry point.
            var t1 = new Thread(delegate ()
            {
                MessageBox.Show(@"Hello World", @"Delegate Greeting", MessageBoxButtons.OK);
            });
            t1.Start();
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
