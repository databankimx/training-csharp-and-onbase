#region Using Directives
using NUnit.Framework;
using Unity._00.CommonFunctionality.Models.Objects;
using Unity._03.DocumentRetrieval.HelperClasses.OnBase;
#endregion

namespace Unity._03.DocumentRetrieval.Tests.HelperClasses.OnBase
{
    [TestFixture]
    public class DocumentExtensionsTests
    {
        [Test]
        public void KeywordLookup_OnNullDocument_Throws()
        {
            // Extension methods on Document are invocable with a null receiver;
            // verify they fail with the training track's own exception type
            // rather than a bare NullReferenceException.
            Assert.Throws<DatabankException>(() => DocumentExtensionsInvoker.LookupOnNull());
        }
    }
}