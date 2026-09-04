#region Using Directives
using Hyland.Unity;
using NUnit.Framework;
using Unity._00.CommonFunctionality.HelperClasses.Extensions;
using Unity._00.CommonFunctionality.Models.Enumerations;
using Unity._00.CommonFunctionality.Models.Objects;
#endregion

namespace Unity._00.CommonFunctionality.Tests.HelperClasses.Extensions
{
    [TestFixture]
    public class TypeConversionExtensionsTests
    {
        #region ToLicenseType
        [TestCase("Query", LicenseType.QueryMetering)]
        [TestCase("query", LicenseType.QueryMetering)]
        [TestCase("Metering", LicenseType.QueryMetering)]
        [TestCase("Enterprise", LicenseType.EnterpriseCoreAPI)]
        [TestCase("e", LicenseType.EnterpriseCoreAPI)]
        [TestCase("Default", LicenseType.Default)]
        [TestCase("something-unrecognized", LicenseType.Default)]
        public void ToLicenseType_MapsFirstCharacterCaseInsensitively(string value, LicenseType expected)
        {
            Assert.That(value.ToLicenseType(), Is.EqualTo(expected));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void ToLicenseType_ReturnsDefault_ForNullOrWhitespace(string value)
        {
            Assert.That(value.ToLicenseType(), Is.EqualTo(LicenseType.Default));
        }
        #endregion

        #region ToFileTypeId - Forms
        [TestCase("UnityForm", FileFormat.UnityForm)]
        [TestCase("unityform", FileFormat.UnityForm)]
        [TestCase("VirtualForm", FileFormat.VirtualForm)]
        [TestCase("EForm", FileFormat.EForm)]
        [TestCase("Form", FileFormat.EForm)]
        public void ToFileTypeId_MapsFormsWithoutExtensions(string value, FileFormat expected)
        {
            Assert.That(value.ToFileTypeId(), Is.EqualTo((long)expected));
        }
        #endregion

        #region ToFileTypeId - Extensions
        [TestCase("txt", FileFormat.Text)]
        [TestCase("ctx", FileFormat.Text)]
        [TestCase("tif", FileFormat.Image)]
        [TestCase("jpg", FileFormat.Image)]
        [TestCase("png", FileFormat.Image)]
        [TestCase("bmp", FileFormat.Image)]
        [TestCase("pcl", FileFormat.Pcl)]
        [TestCase("doc", FileFormat.Word)]
        [TestCase("docx", FileFormat.Word)]
        [TestCase("xls", FileFormat.Excel)]
        [TestCase("xlsx", FileFormat.Excel)]
        [TestCase("ppt", FileFormat.PowerPoint)]
        [TestCase("rtf", FileFormat.RichText)]
        [TestCase("pdf", FileFormat.Pdf)]
        [TestCase("htm", FileFormat.Html)]
        [TestCase("html", FileFormat.Html)]
        [TestCase("avi", FileFormat.Avi)]
        [TestCase("mov", FileFormat.QuickTime)]
        [TestCase("wav", FileFormat.Wav)]
        [TestCase("xml", FileFormat.Xml)]
        [TestCase("msg", FileFormat.Outlook)]
        [TestCase("hl7", FileFormat.Hl7)]
        [TestCase("eml", FileFormat.Email)]
        [TestCase("zip", FileFormat.Zip)]
        [TestCase("rar", FileFormat.Zip)]
        public void ToFileTypeId_MapsKnownExtensions(string extension, FileFormat expected)
        {
            Assert.That(extension.ToFileTypeId(), Is.EqualTo((long)expected));
        }

        [TestCase("TXT")]
        [TestCase("Pdf")]
        public void ToFileTypeId_IsCaseInsensitive(string extension)
        {
            Assert.That(extension.ToFileTypeId(), Is.Not.EqualTo((long)FileFormat.Undefined));
        }

        [Test]
        public void ToFileTypeId_PadsShortExtensions()
        {
            // "7z" is padded to "7z-" before matching, mapping to Zip
            Assert.That("7z".ToFileTypeId(), Is.EqualTo((long)FileFormat.Zip));
        }

        [TestCase("xyz")]
        [TestCase("q")]
        public void ToFileTypeId_ReturnsUndefined_ForUnknownExtensions(string extension)
        {
            Assert.That(extension.ToFileTypeId(), Is.EqualTo((long)FileFormat.Undefined));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void ToFileTypeId_Throws_ForNullOrWhitespace(string value)
        {
            var ex = Assert.Throws<DatabankException>(() => value.ToFileTypeId());
            Assert.That(ex.Message, Does.Contain("without an extension"));
        }
        #endregion
    }
}