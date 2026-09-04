#region Using Directives
using System;
using NUnit.Framework;
#endregion

namespace Unity._04.DocumentArchiving.Tests.HelperClasses.OnBase
{
    /// <summary>
    /// Pins the two areas README.md documents as intentionally incomplete: repeater-row
    /// support and e-form/Unity Form revision and rendition updates. These tests should
    /// FAIL (and be rewritten) the day either area is actually implemented.
    /// </summary>
    [TestFixture]
    public class DocumentStorageNotImplementedTests
    {
        [Test]
        [Ignore("Fill in once the exact NotImplementedException call sites are confirmed.")]
        public void EFormRevisionUpdate_ThrowsNotImplemented()
        {
            Assert.Throws<NotImplementedException>(() => { });
        }
    }
}