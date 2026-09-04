#region Using Directives
using System;
using System.Linq;
using NUnit.Framework;
using Unity._00.CommonFunctionality.Models.Enumerations;
#endregion

namespace Unity._00.CommonFunctionality.Tests.Models.Enumerations
{
    [TestFixture]
    public class AuthenticationModeTests
    {
        [Test]
        public void HasExactlyFourModes()
        {
            // Guards the documented decision that SessionId is NOT a member here.
            Assert.That(Enum.GetValues(typeof(AuthenticationMode)), Has.Length.EqualTo(4));
        }

        [Test]
        public void DoesNotContainSessionId()
        {
            Assert.That(Enum.GetNames(typeof(AuthenticationMode)),
                Does.Not.Contain("SessionId"));
        }
    }
}