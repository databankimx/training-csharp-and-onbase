#region Using Directives
using System;
using NUnit.Framework;
using Unity._00.CommonFunctionality.Models.Objects;
using Unity._02.AccessingTaxonomy.HelperClasses.OnBase;
#endregion

namespace Unity._02.AccessingTaxonomy.Tests.HelperClasses.OnBase
{
    [TestFixture]
    public class OnBaseTaxonomyTests
    {
        private OnBaseTaxonomy taxonomy;

        [SetUp]
        public void SetUp() => taxonomy = new OnBaseTaxonomy();

        #region Constructor
        [Test]
        public void Constructor_WithNoApplication_LeavesAppNull()
        {
            Assert.That(taxonomy.App, Is.Null);
        }

        [Test]
        public void Constructor_WithNullApplication_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new OnBaseTaxonomy(null));
        }
        #endregion

        #region Initialize Guard
        // Every public lookup calls Initialize(app) first, so a null App must fail
        // the same way regardless of which method is invoked.
        private static readonly TestCaseData[] LookupInvocations =
        {
            new TestCaseData((Action<OnBaseTaxonomy>)(t => t.GetDocumentTypeGroups()))
                .SetName("GetDocumentTypeGroups"),
            new TestCaseData((Action<OnBaseTaxonomy>)(t => t.GetDocumentTypeGroup("Invoices")))
                .SetName("GetDocumentTypeGroup"),
            new TestCaseData((Action<OnBaseTaxonomy>)(t => t.GetDocumentTypes(null)))
                .SetName("GetDocumentTypes"),
            new TestCaseData((Action<OnBaseTaxonomy>)(t => t.GetUnityForm("SomeForm")))
                .SetName("GetUnityForm")
        };

        [TestCaseSource(nameof(LookupInvocations))]
        public void Lookup_WithNullApplication_ThrowsDatabankException(Action<OnBaseTaxonomy> invoke)
        {
            Assert.Throws<DatabankException>(() => invoke(taxonomy));
        }

        [Test]
        public void Lookup_WithNullApplication_PreservesInitializeFailureAsInnerException()
        {
            var ex = Assert.Throws<DatabankException>(() => taxonomy.GetDocumentTypeGroup("Invoices"));

            Assert.Multiple(() =>
            {
                Assert.That(ex.Message, Does.Contain("Error getting document type group"));
                Assert.That(ex.InnerException, Is.InstanceOf<DatabankException>());
                Assert.That(ex.InnerException.Message, Does.Contain("Error initializing Application object"));
                Assert.That(ex.InnerException.InnerException, Is.InstanceOf<DatabankException>());
                Assert.That(ex.InnerException.InnerException.Message,
                    Does.Contain("Application cannot be null"));
            });
        }

        [Test]
        public void Lookup_IncludesTheRequestedNameInTheErrorMessage()
        {
            var ex = Assert.Throws<DatabankException>(() => taxonomy.GetUnityForm("PatientIntake"));
            Assert.That(ex.Message, Does.Contain("PatientIntake"));
        }
        #endregion
    }
}