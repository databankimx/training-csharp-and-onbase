#region Using Directives
using NUnit.Framework;
using Unity._00.CommonFunctionality.Models.Configuration;
using Unity._00.CommonFunctionality.Models.Enumerations;
using Unity._00.CommonFunctionality.Models.Objects;
using Unity._01.ConnectingToOnBase.HelperClasses.OnBase;
#endregion

namespace Unity._01.ConnectingToOnBase.Tests.HelperClasses.OnBase
{
    [TestFixture]
    public class SessionManagementTests
    {
        #region Configuration Binding
        [Test]
        public void StaticConstructor_BindsServiceLocationFromConfig()
        {
            Assert.That(SessionManagement.ServiceLocation, Is.Not.Null);
            Assert.That(SessionManagement.ServiceLocation.DataSource, Is.EqualTo("TestDataSource"));
        }

        [Test]
        public void StaticConstructor_BindsAuthenticationMode()
        {
            Assert.That(SessionManagement.ServiceLocation.AuthenticationMode,
                Is.EqualTo(AuthenticationMode.OnBaseCredentials));
        }
        #endregion

        #region Connect - Failure Contract
        [Test]
        public void Connect_AssignsServiceLocationBeforeAttemptingConnection()
        {
            var location = BuildLocation();

            // The connection itself will fail (no server), but the property assignment
            // on the first line of Connect() happens before that.
            Assert.Throws<DatabankException>(() => location.Connect());
            Assert.That(SessionManagement.ServiceLocation, Is.SameAs(location));
        }

        [Test]
        public void Connect_WrapsUnderlyingFailuresInDatabankException()
        {
            var location = BuildLocation();

            var ex = Assert.Throws<DatabankException>(() => location.Connect());
            Assert.Multiple(() =>
            {
                Assert.That(ex.Message, Does.Contain("Error connecting to OnBase"));
                Assert.That(ex.InnerException, Is.Not.Null,
                    "The original Unity API failure must be preserved as the inner exception.");
            });
        }

        [Test]
        public void Connect_WithSessionId_WrapsReconnectFailures()
        {
            var location = BuildLocation();

            Assert.Throws<DatabankException>(() => location.Connect("bogus-session-id"));
        }
        #endregion

        #region Helpers
        private static ServiceLocation BuildLocation() => new ServiceLocation
        {
            ApplicationId = "test-app-guid",
            ServicePath = "https://onbase.invalid/AppServer/Service.asmx",
            DataSource = "TestDataSource",
            AuthenticationMode = AuthenticationMode.OnBaseCredentials
        };
        #endregion
    }
}