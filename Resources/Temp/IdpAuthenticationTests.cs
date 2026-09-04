#region Using Directives
using NUnit.Framework;
#endregion

namespace Unity._01.ConnectingToOnBase.Tests.HelperClasses.OnBase
{
    [TestFixture]
    [Category("Integration")]
    [Explicit("Requires a reachable Hyland Identity Provider.")]
    public class IdpAuthenticationTests
    {
        [Test]
        public void GetAccessToken_ReturnsToken_ForValidCredentials()
        {
            Assert.Ignore("Populate IdP settings for your environment before enabling.");
        }
    }
}