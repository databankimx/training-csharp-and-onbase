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
using NUnit.Framework;
using Samples.NuGetLibrary;
#endregion

namespace Samples.NUnitTests
{
    #region Training Notes
    /*
     * [TestFixture] marks this class as containing NUnit tests. Unlike some other test
     * frameworks, NUnit doesn't strictly require it (a class with [Test] methods is
     * usually discovered anyway), but it's conventional and makes intent explicit,
     * especially useful once a fixture also has [SetUp]/[TearDown] or fixture-level
     * attributes.
     *
     * [TestCase] is NUnit's parameterized test attribute, ONE test METHOD, MULTIPLE test
     * CASES, each shown as a separate result in the test explorer/runner output. This is
     * a genuinely idiomatic NUnit pattern for testing the same logic against a range of
     * inputs without writing a separate method (and a separate, easily-forgotten
     * assertion) for each one.
     *
     * Assert.That(...) is NUnit's modern "constraint model" syntax (Is.True, Is.EqualTo,
     * Is.Null, etc.), the current recommended style, superseding the older
     * Assert.IsTrue()/Assert.AreEqual() classic-model methods, which still exist and
     * still work, but read less fluently and are being phased toward deprecation in
     * NUnit's own documentation.
     */
    #endregion

    /// <summary>
    /// Tests for <see cref="ZipCodeValidator"/>.
    /// </summary>
    [TestFixture]
    public class ZipCodeValidatorTests
    {
        #region IsValid Tests
        [TestCase("75067", ExpectedResult = true, TestName = "IsValid_ValidFiveDigitZipCode_ReturnsTrue")]
        [TestCase(" 75067 ", ExpectedResult = true, TestName = "IsValid_ValidZipCodeWithWhitespace_ReturnsTrue")]
        [TestCase("7506", ExpectedResult = false, TestName = "IsValid_TooShort_ReturnsFalse")]
        [TestCase("750671", ExpectedResult = false, TestName = "IsValid_TooLong_ReturnsFalse")]
        [TestCase("7506A", ExpectedResult = false, TestName = "IsValid_ContainsLetter_ReturnsFalse")]
        [TestCase("", ExpectedResult = false, TestName = "IsValid_EmptyString_ReturnsFalse")]
        [TestCase(null, ExpectedResult = false, TestName = "IsValid_Null_ReturnsFalse")]
        [TestCase("     ", ExpectedResult = false, TestName = "IsValid_WhitespaceOnly_ReturnsFalse")]
        public bool IsValid_VariousInputs_ReturnsExpectedResult(string? zipCode)
        {
            return ZipCodeValidator.IsValid(zipCode);
        }
        #endregion

        #region Normalize Tests
        [Test]
        public void Normalize_ValidZipCodeWithWhitespace_ReturnsTrimmedValue()
        {
            // Arrange
            const string input = " 75067 ";

            // Act
            var result = ZipCodeValidator.Normalize(input);

            // Assert
            Assert.That(result, Is.EqualTo("75067"));
        }

        [Test]
        public void Normalize_InvalidZipCode_ReturnsNull()
        {
            // Arrange
            const string input = "not-a-zip";

            // Act
            var result = ZipCodeValidator.Normalize(input);

            // Assert
            Assert.That(result, Is.Null);
        }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
