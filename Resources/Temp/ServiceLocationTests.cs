#region Using Directives
using Hyland.Unity;
using NUnit.Framework;
using Unity._00.CommonFunctionality.Models.Configuration;
using Unity._00.CommonFunctionality.Models.Enumerations;
#endregion

namespace Unity._00.CommonFunctionality.Tests.Models.Configuration
{
    [TestFixture]
    public class ServiceLocationTests
    {
        private ServiceLocation serviceLocation;

        [SetUp]
        public void SetUp() => serviceLocation = new ServiceLocation();

        [Test]
        public void ElementName_IsServiceLocation()
        {
            Assert.That(ServiceLocation.ElementName, Is.EqualTo("serviceLocation"));
        }

        [Test]
        public void AuthenticationMode_DefaultsToOnBaseCredentials()
        {
            Assert.That(serviceLocation.AuthenticationMode,
                Is.EqualTo(AuthenticationMode.OnBaseCredentials));
        }

        [Test]
        public void RequiredProperties_RoundTrip()
        {
            serviceLocation.ApplicationId = "app-guid";
            serviceLocation.ServicePath = "https://onbase.example.com/AppServer/Service.asmx";
            serviceLocation.DataSource = "OnBase";
            serviceLocation.LicenseType = LicenseType.EnterpriseCoreAPI;

            Assert.Multiple(() =>
            {
                Assert.That(serviceLocation.ApplicationId, Is.EqualTo("app-guid"));
                Assert.That(serviceLocation.ServicePath, Does.EndWith("Service.asmx"));
                Assert.That(serviceLocation.DataSource, Is.EqualTo("OnBase"));
                Assert.That(serviceLocation.LicenseType, Is.EqualTo(LicenseType.EnterpriseCoreAPI));
            });
        }

        [TestCase(AuthenticationMode.OnBaseCredentials)]
        [TestCase(AuthenticationMode.DomainCredentials)]
        [TestCase(AuthenticationMode.AccessToken)]
        [TestCase(AuthenticationMode.LicenseToken)]
        public void AuthenticationMode_RoundTripsEveryMode(AuthenticationMode mode)
        {
            serviceLocation.AuthenticationMode = mode;
            Assert.That(serviceLocation.AuthenticationMode, Is.EqualTo(mode));
        }

        [Test]
        public void SessionId_IsIndependentOfAuthenticationMode()
        {
            // Documents the design decision in AuthenticationMode's Training Notes:
            // SessionId is deliberately NOT a fifth enum member.
            serviceLocation.AuthenticationMode = AuthenticationMode.OnBaseCredentials;
            serviceLocation.SessionId = "session-123";

            Assert.Multiple(() =>
            {
                Assert.That(serviceLocation.SessionId, Is.EqualTo("session-123"));
                Assert.That(serviceLocation.AuthenticationMode,
                    Is.EqualTo(AuthenticationMode.OnBaseCredentials));
            });
        }
    }
}
