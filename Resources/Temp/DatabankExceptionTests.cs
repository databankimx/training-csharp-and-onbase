#region Using Directives
using System;
using NUnit.Framework;
using Unity._00.CommonFunctionality.Models.Objects;
#endregion

namespace Unity._00.CommonFunctionality.Tests.Models.Objects
{
    [TestFixture]
    public class DatabankExceptionTests
    {
        [Test]
        public void MessageConstructor_SetsMessage_AndDefaultExceptionType()
        {
            var ex = new DatabankException("Something failed!");

            Assert.Multiple(() =>
            {
                Assert.That(ex.Message, Is.EqualTo("Something failed!"));
                Assert.That(ex.ExceptionType, Is.EqualTo("DatabankException"));
                Assert.That(ex.InnerException, Is.Null);
            });
        }

        [Test]
        public void MessageConstructor_PreservesInnerException()
        {
            var inner = new InvalidOperationException("inner");
            var ex = new DatabankException("outer", inner);

            Assert.That(ex.InnerException, Is.SameAs(inner));
        }

        [Test]
        public void WrappingConstructor_CopiesMessageAndRecordsOriginalTypeName()
        {
            var original = new ArgumentNullException("paramName", "was null");
            var ex = new DatabankException(original);

            Assert.Multiple(() =>
            {
                Assert.That(ex.ExceptionType, Is.EqualTo(nameof(ArgumentNullException)));
                Assert.That(ex.Message, Is.EqualTo(original.Message));
            });
        }

        [Test]
        public void WrappingConstructor_CarriesForwardTheOriginalsInnerException()
        {
            var root = new FormatException("root cause");
            var original = new InvalidOperationException("middle", root);

            var ex = new DatabankException(original);

            Assert.That(ex.InnerException, Is.SameAs(root));
        }

        [Test]
        public void IsCatchableAsException()
        {
            Assert.That(new DatabankException("x"), Is.InstanceOf<Exception>());
        }
    }
}