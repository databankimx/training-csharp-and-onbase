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

#region Directives
using System;
using System.IO;
using CSharp.SharedLibrary.Models;
using NUnit.Framework;
#endregion

namespace CSharp.SharedLibrary.Tests
{
    /// <summary>
    /// Unit tests for the DatabankException model
    /// </summary>
    [TestFixture]
    public class DatabankExceptionTests
    {
        #region Constructors
        /// <summary>
        /// Tests that the constructor with only a message parameter sets the message
        /// and defaults the ExceptionType to "DatabankException".
        /// </summary>
        [Test]
        public void Constructor_MessageOnly_SetsMessageAndDefaultExceptionType()
        {
            var ex = new DatabankException("Something went wrong!");

            Assert.Multiple(() =>
            {
                Assert.That(ex.Message, Is.EqualTo("Something went wrong!"));
                Assert.That(ex.ExceptionType, Is.EqualTo("DatabankException"));
                Assert.That(ex.InnerException, Is.Null);
            });
        }

        /// <summary>
        /// Verifies that constructing <c>DatabankException</c> with a message and an inner exception preserves both
        /// values.
        /// </summary>
        [Test]
        public void Constructor_MessageAndInnerException_SetsBoth()
        {
            var inner = new InvalidOperationException("The original problem");
            var ex = new DatabankException("Wrapped error", inner);

            Assert.Multiple(() =>
            {
                Assert.That(ex.Message, Is.EqualTo("Wrapped error"));
                Assert.That(ex.InnerException, Is.SameAs(inner));
            });
        }

        /// <summary>
        /// Verifies that constructing a DatabankException from another exception preserves the original exception type
        /// name and message.
        /// </summary>
        [Test]
        public void Constructor_FromOtherException_CapturesOriginalExceptionType()
        {
            #pragma warning disable S3928 // Undefined parameter OK in test (do not do this in production code)
            var original = new ArgumentNullException("someParameter");
            #pragma warning restore S3928
            var ex = new DatabankException(original);

            Assert.Multiple(() =>
            {
                Assert.That(ex.ExceptionType, Is.EqualTo(nameof(ArgumentNullException)));
                Assert.That(ex.Message, Is.EqualTo(original.Message));
            });
        }
        #endregion

        #region ExceptionType Property
        /// <summary>
        /// Verifies that the ExceptionType property can be overridden after a DatabankException instance is created.
        /// </summary>
        [Test]
        public void ExceptionType_CanBeOverriddenAfterConstruction()
        {
            var ex = new DatabankException("test") { ExceptionType = "CustomType" };

            Assert.That(ex.ExceptionType, Is.EqualTo("CustomType"));
        }
        #endregion

        #region Log
        /// <summary>
        /// Verifies that logging an exception writes its type name and message to the console output.
        /// </summary>
        /// <remarks>Temporarily redirects <see cref="System.Console.Out"/> to capture output and restores
        /// the original writer after execution.</remarks>
        [Test]
        public void Log_WritesExceptionTypeAndMessageToConsole()
        {
            var ex = new DatabankException("Outer failure");
            var originalOut = Console.Out;

            try
            {
                using var writer = new StringWriter();
                Console.SetOut(writer);

                ex.Log();

                string output = writer.ToString();
                Assert.That(output, Does.Contain("DatabankException"));
                Assert.That(output, Does.Contain("Outer failure"));
            }
            finally
            {
                // Always restore Console.Out, even if an assertion above fails,
                // otherwise every test that runs after this one in the same
                // session inherits a StringWriter nobody is reading from.
                Console.SetOut(originalOut);
            }
        }

        /// <summary>
        /// Verifies that logging an exception with an inner exception writes both the outer and inner exception
        /// messages to the console output.
        /// </summary>
        /// <remarks>Redirects <see cref="System.Console.Out"/> to a <see cref="System.IO.StringWriter"/>
        /// for assertion and restores the original output writer after execution.</remarks>
        [Test]
        public void Log_WithInnerException_WritesBothMessagesToConsole()
        {
            var inner = new InvalidOperationException("Root cause");
            var ex = new DatabankException("Outer failure", inner);
            var originalOut = Console.Out;

            try
            {
                using var writer = new StringWriter();
                Console.SetOut(writer);

                ex.Log();

                string output = writer.ToString();
                Assert.That(output, Does.Contain("Outer failure"));
                Assert.That(output, Does.Contain("Root cause"));
            }
            finally
            {
                Console.SetOut(originalOut);
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
