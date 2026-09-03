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
     * [SetUp] runs before EVERY [Test] method in this fixture, a fresh sampleLocation for
     * each test, not shared/reused across tests. This matters because NUnit test METHODS
     * within one fixture can run in any order (and, depending on configuration, even in
     * parallel), a test that accidentally depended on a previous test's mutation of shared
     * state would be a real, hard-to-diagnose bug. Since Location is an immutable record
     * here, this particular fixture doesn't strictly NEED fresh state per test, but the
     * pattern itself (fresh setup per test, no cross-test state) is worth establishing
     * regardless, it's what keeps a growing test suite reliable as it's added to over time.
     */
    #endregion

    /// <summary>
    /// Tests for <see cref="LocationFormatter"/>.
    /// </summary>
    [TestFixture]
    public class LocationFormatterTests
    {
        #region Fields
        private Location sampleLocation = null!;
        #endregion

        #region Setup
        [SetUp]
        public void SetUp()
        {
            sampleLocation = new Location("75067", "Lewisville", "Denton", "TX");
        }
        #endregion

        #region ToDisplayString(Location) Tests
        [Test]
        public void ToDisplayString_SingleLocation_FormatsAsExpected()
        {
            // Act
            var result = LocationFormatter.ToDisplayString(sampleLocation);

            // Assert
            Assert.That(result, Is.EqualTo("Lewisville, Denton County, TX 75067"));
        }

        [Test]
        public void ToDisplayString_NullLocation_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => LocationFormatter.ToDisplayString((Location)null!));
        }
        #endregion

        #region ToDisplayString(IEnumerable<Location>) Tests
        [Test]
        public void ToDisplayString_MultipleLocations_FormatsOneLinePerLocation()
        {
            // Arrange
            var locations = new List<Location>
            {
                sampleLocation,
                new("75068", "Lewisville", "Denton", "TX")
            };

            // Act
            var result = LocationFormatter.ToDisplayString(locations);

            // Assert
            Assert.That(result, Is.EqualTo(
                "Lewisville, Denton County, TX 75067" + Environment.NewLine +
                "Lewisville, Denton County, TX 75068"));
        }

        [Test]
        public void ToDisplayString_EmptyCollection_ReturnsNoResultsFoundMessage()
        {
            // Arrange
            var locations = new List<Location>();

            // Act
            var result = LocationFormatter.ToDisplayString(locations);

            // Assert
            Assert.That(result, Is.EqualTo("No results found."));
        }

        [Test]
        public void ToDisplayString_NullCollection_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => LocationFormatter.ToDisplayString((IEnumerable<Location>)null!));
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
