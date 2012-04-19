using Bottles;
using FubuCore;
using FubuMVC.Core.Packaging;
using FubuTestingSupport;
using NUnit.Framework;

namespace FubuMVC.Tests.Packaging
{
    [TestFixture]
    public class ZipFilePackageLoaderTester
    {
        [Test]
        public void zip_packages_should_store_content_in_WebContent_subfolder()
        {
            const string packageFolder = @"c:\packageFolder";
            var webContentSubFolder = FileSystem.Combine(packageFolder, CommonBottleFiles.WebContentFolder);
            ZipFilePackageLoader.GetContentFolderForPackage(packageFolder).ShouldEqual(webContentSubFolder);
        }
    }
}