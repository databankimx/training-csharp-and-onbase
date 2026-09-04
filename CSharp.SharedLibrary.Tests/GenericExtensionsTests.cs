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
using System.Collections.Generic;
using CSharp.SharedLibrary.HelperClasses;
using NUnit.Framework;
#endregion

namespace CSharp.SharedLibrary.Tests
{
    /// <summary>
    /// Unit tests for the extension methods in GenericExtensions
    /// </summary>
    [TestFixture]
    public class GenericExtensionsTests
    {
        #region Square
        /// <summary>
        /// Tests that the Square extension method correctly computes the square of a positive integer.
        /// </summary>
        [Test]
        public void Square_PositiveNumber_ReturnsSquare()
        {
            Assert.That(4.Square(), Is.EqualTo(16));
        }

        /// <summary>
        /// Verifies that squaring a negative number returns the positive square.
        /// </summary>
        [Test]
        public void Square_NegativeNumber_ReturnsPositiveSquare()
        {
            Assert.That((-4).Square(), Is.EqualTo(16));
        }

        /// <summary>
        /// Verifies that squaring zero returns zero.
        /// </summary>
        [Test]
        public void Square_Zero_ReturnsZero()
        {
            Assert.That(0.Square(), Is.EqualTo(0));
        }
        #endregion

        #region Replace (case-insensitive)
        /// <summary>
        /// Verifies that string replacement succeeds when the match is evaluated with case-insensitive current-culture
        /// comparison.
        /// </summary>
        /// <remarks>Asserts that replacing "world" with "C#" in "Hello World" produces "Hello C#" when
        /// using StringComparison.CurrentCultureIgnoreCase.</remarks>
        [Test]
        public void Replace_CaseInsensitiveMatch_ReplacesText()
        {
            const string source = "Hello World";
            string result = source.Replace("world", "C#", System.StringComparison.CurrentCultureIgnoreCase);
            Assert.That(result, Is.EqualTo("Hello C#"));
        }

        /// <summary>
        /// Verifies that replacing a non-existent substring returns the original string when using a case-insensitive
        /// current-culture comparison.
        /// </summary>
        [Test]
        public void Replace_NoMatch_ReturnsOriginalString()
        {
            const string source = "Hello World";
            string result = source.Replace("xyz", "C#", System.StringComparison.CurrentCultureIgnoreCase);
            Assert.That(result, Is.EqualTo(source));
        }

        /// <summary>
        /// Verifies that calling Replace on a null or empty string returns an empty string, regardless of the search and replacement values.
        /// </summary>
        [Test]
        public void Replace_NullOrEmptySource_ReturnsEmptyString()
        {
            Assert.That(((string)null).Replace("a", "b", System.StringComparison.Ordinal), Is.EqualTo(""));
        }
        #endregion

        #region Numeric Conversions
        /// <summary>
        /// Verifies that `ToInt` returns the expected integer for valid, invalid, and empty string inputs.
        /// </summary>
        /// <remarks>Covers representative conversion cases using NUnit `TestCase` data.</remarks>
        /// <param name="input">String value to convert to an integer.</param>
        /// <param name="expected">Expected conversion result.</param>
        [TestCase("42", 42)]
        [TestCase("not a number", 0)]
        [TestCase("", 0)]
        public void ToInt_VariousInputs_ReturnsExpectedValue(string input, int expected)
        {
            Assert.That(input.ToInt(), Is.EqualTo(expected));
        }

        /// <summary>
        /// Verifies that converting different string inputs to double yields the expected result.
        /// </summary>
        /// <remarks>Covers both a valid numeric string and a non-numeric string that is expected to
        /// convert to 0.0.</remarks>
        /// <param name="input">String value to convert.</param>
        /// <param name="expected">Expected double result of the conversion.</param>
        [TestCase("3.14", 3.14)]
        [TestCase("not a number", 0.0)]
        public void ToDouble_VariousInputs_ReturnsExpectedValue(string input, double expected)
        {
            Assert.That(input.ToDouble(), Is.EqualTo(expected));
        }
        #endregion

        #region ToBoolean
        /// <summary>
        /// Verifies that <c>ToBoolean</c> returns the expected Boolean value for common truthy, falsy, and empty string
        /// inputs.
        /// </summary>
        /// <param name="input">The string value to convert to a Boolean result.</param>
        /// <param name="expected">The expected Boolean conversion result.</param>
        [TestCase("true", true)]
        [TestCase("yes", true)]
        [TestCase("y", true)]
        [TestCase("1", true)]
        [TestCase("false", false)]
        [TestCase("no", false)]
        [TestCase("0", false)]
        [TestCase("", false)]
        public void ToBoolean_VariousInputs_ReturnsExpectedValue(string input, bool expected)
        {
            Assert.That(input.ToBoolean(), Is.EqualTo(expected));
        }
        #endregion

        #region Parse / TryParse
        /// <summary>
        /// Verifies that parsing valid boolean text returns the expected Boolean value.
        /// </summary>
        /// <remarks>Covers the affirmative input "yes" and expects a result of <see
        /// langword="true"/>.</remarks>
        [Test]
        public void Parse_ValidBooleanText_ReturnsParsedValue()
        {
            Assert.That("yes".Parse(), Is.True);
        }

        /// <summary>
        /// Verifies that attempting to parse unparseable text throws a <see cref="System.FormatException"/>.
        /// </summary>
        [Test]
        public void Parse_UnparseableText_ThrowsFormatException()
        {
            Assert.Throws<System.FormatException>(() => "banana".Parse());
        }

        /// <summary>
        /// Verifies that parsing valid Boolean text returns `true` and outputs the parsed `true` value.
        /// </summary>
        [Test]
        public void TryParse_ValidBooleanText_ReturnsTrueAndOutputsValue()
        {
            bool success = "true".TryParse(out bool result);

            Assert.Multiple(() =>
            {
                Assert.That(success, Is.True);
                Assert.That(result, Is.True);
            });
        }
        
        /// <summary>
        /// Verifies that parsing unparseable Boolean text returns `false` and outputs the default `false` value.
        /// </summary>
        [Test]
        public void TryParse_UnparseableText_ReturnsFalse()
        {
            bool success = "banana".TryParse(out bool result);

            Assert.Multiple(() =>
            {
                Assert.That(success, Is.False);
                Assert.That(result, Is.False);
            });
        }
        #endregion

        #region ToArray
        /// <summary>
        /// Verifies that `ToArray` splits a comma-delimited string into the expected elements when a character
        /// delimiter is provided.
        /// </summary>
        [Test]
        public void ToArray_CharDelimiter_SplitsCorrectly()
        {
            string[] result = "a,b,c".ToArray(',');
            Assert.That(result, Is.EqualTo(new[] { "a", "b", "c" }));
        }

        /// <summary>
        /// Verifies that `ToArray` splits a string into elements using a string delimiter.
        /// </summary>
        /// <remarks>Confirms that splitting "a::b::c" with "::" produces ["a", "b", "c"].</remarks>
        [Test]
        public void ToArray_StringDelimiter_SplitsCorrectly()
        {
            string[] result = "a::b::c".ToArray("::");
            Assert.That(result, Is.EqualTo(new[] { "a", "b", "c" }));
        }

        /// <summary>
        /// Verifies that converting a string to an array with a delimiter that is not present returns a single-element
        /// array containing the original string.
        /// </summary>
        [Test]
        public void ToArray_StringDelimiter_NoDelimiterPresent_ReturnsSingleElementArray()
        {
            string[] result = "a".ToArray("::");
            Assert.That(result, Is.EqualTo(new[] { "a" }));
        }
        #endregion

        #region IsNumeric / IsPositive
        /// <summary>
        /// Verifies that numeric validation returns the expected result for different input strings and integer-only
        /// settings.
        /// </summary>
        /// <remarks>Covers valid integers, valid decimals, decimal rejection when integer-only validation
        /// is enabled, and non-numeric text.</remarks>
        /// <param name="input">Input string to evaluate as numeric.</param>
        /// <param name="integerOnly">Indicates whether only integer formats are considered valid.</param>
        /// <param name="expected">Expected validation result for the specified input and option.</param>
        [TestCase("42", false, true)]
        [TestCase("3.14", false, true)]
        [TestCase("3.14", true, false)]
        [TestCase("abc", false, false)]
        public void IsNumeric_VariousInputs_ReturnsExpectedValue(string input, bool integerOnly, bool expected)
        {
            Assert.That(input.IsNumeric(integerOnly), Is.EqualTo(expected));
        }

        /// <summary>
        /// Verifies that `IsPositive` returns the expected result for positive, negative, and zero numeric string
        /// inputs.
        /// </summary>
        /// <remarks>Runs as a parameterized test across multiple input and expected-value
        /// pairs.</remarks>
        /// <param name="input">Numeric string to evaluate.</param>
        /// <param name="expected">Expected result indicating whether the input represents a value greater than zero.</param>
        [TestCase("5", true)]
        [TestCase("-5", false)]
        [TestCase("0", false)]
        public void IsPositive_VariousInputs_ReturnsExpectedValue(string input, bool expected)
        {
            Assert.That(input.IsPositive(), Is.EqualTo(expected));
        }
        #endregion

        #region IsList / IsDictionary
        /// <summary>
        /// Verifies that <c>IsList()</c> returns <see langword="true"/> for a <see cref="List{T}"/> instance.
        /// </summary>
        [Test]
        public void IsList_GivenAList_ReturnsTrue()
        {
            var list = new List<int> { 1, 2, 3 };
            Assert.That(list.IsList(), Is.True);
        }

        /// <summary>
        /// Verifies that <c>IsList()</c> returns <see langword="false"/> when the source object is a <see
        /// cref="Dictionary{TKey, TValue}"/>.
        /// </summary>
        [Test]
        public void IsList_GivenADictionary_ReturnsFalse()
        {
            var dictionary = new Dictionary<string, int>();
            Assert.That(dictionary.IsList(), Is.False);
        }

        /// <summary>
        /// Verifies that <c>IsList</c> returns <see langword="false"/> when the input object is <see langword="null"/>.
        /// </summary>
        [Test]
        public void IsList_GivenNull_ReturnsFalse()
        {
            Assert.That(((object)null).IsList(), Is.False);
        }

        /// <summary>
        /// Verifies that <c>IsDictionary</c> returns <see langword="true"/> for a <see cref="Dictionary{TKey,
        /// TValue}"/> instance.
        /// </summary>
        [Test]
        public void IsDictionary_GivenADictionary_ReturnsTrue()
        {
            var dictionary = new Dictionary<string, int>();
            Assert.That(dictionary.IsDictionary(), Is.True);
        }

        /// <summary>
        /// Verifies that <c>IsDictionary</c> returns <see langword="false"/> when the source object is a <see
        /// cref="List{T}"/>.
        /// </summary>
        [Test]
        public void IsDictionary_GivenAList_ReturnsFalse()
        {
            var list = new List<int>();
            Assert.That(list.IsDictionary(), Is.False);
        }
        #endregion

        #region IsBitSet
        /// <summary>
        /// Verifies that <c>IsBitSet</c> correctly identifies whether a specific bit position is set in an integer value.
        /// </summary>
        /// <param name="value">The integer value to evaluate.</param>
        /// <param name="position">The zero-based position of the bit to check.</param>
        /// <param name="expected">The expected result indicating whether the specified bit is set.</param>
        [TestCase(0b0001, 0, true)]
        [TestCase(0b0010, 0, false)]
        [TestCase(0b0010, 1, true)]
        [TestCase(0b1000, 3, true)]
        public void IsBitSet_VariousPositions_ReturnsExpectedValue(int value, int position, bool expected)
        {
            Assert.That(value.IsBitSet(position), Is.EqualTo(expected));
        }
        #endregion

        #region Swap
        /// <summary>
        /// Verifies that swapping two integer variables exchanges their values.
        /// </summary>
        /// <remarks>Uses <c>GenericExtensions.Swap(ref a, ref b)</c> and asserts both results in a single
        /// grouped assertion.</remarks>
        [Test]
        public void Swap_TwoIntegers_ExchangesValues()
        {
            int a = 1;
            int b = 2;

            GenericExtensions.Swap(ref a, ref b);

            Assert.Multiple(() =>
            {
                Assert.That(a, Is.EqualTo(2));
                Assert.That(b, Is.EqualTo(1));
            });
        }

        /// <summary>
        /// Verifies that swapping two string variables exchanges their values.
        /// </summary>
        /// <remarks>Uses `GenericExtensions.Swap` with `ref` arguments and asserts both variables contain
        /// each other's original values.</remarks>
        [Test]
        public void Swap_TwoStrings_ExchangesValues()
        {
            string a = "first";
            string b = "second";

            GenericExtensions.Swap(ref a, ref b);

            Assert.Multiple(() =>
            {
                Assert.That(a, Is.EqualTo("second"));
                Assert.That(b, Is.EqualTo("first"));
            });
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
