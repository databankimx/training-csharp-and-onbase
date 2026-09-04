#region Using Directives
using NUnit.Framework;
using Unity._04.DocumentArchiving.Models.Enumerations;
using Unity._04.DocumentArchiving.Models.Objects;
#endregion

namespace Unity._04.DocumentArchiving.Tests.Models.Objects
{
    [TestFixture]
    public class NewDocumentRequestTests
    {
        [Test]
        public void DefaultConstructor_UsesDocumentStorageType()
        {
            Assert.That(new NewDocumentRequest().StorageType, Is.EqualTo(StorageType.Document));
        }

        [Test]
        public void Files_DefaultsToEmptyList_NotNull()
        {
            Assert.That(new NewDocumentRequest().Files, Is.Not.Null.And.Empty);
        }

        [TestCase(StorageType.Document)]
        [TestCase(StorageType.EForm)]
        [TestCase(StorageType.UnityForm)]
        public void PrimaryConstructor_SetsStorageType(StorageType storageType)
        {
            Assert.That(new NewDocumentRequest(storageType).StorageType, Is.EqualTo(storageType));
        }

        [Test]
        public void InheritsFromStorageRequest()
        {
            Assert.That(new NewDocumentRequest(), Is.InstanceOf<StorageRequest>());
        }
    }
}