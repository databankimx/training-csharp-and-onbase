using System.Web.Optimization;

namespace Samples.MvcWebApi
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        //
        // *Migration Note: the original also bundled jquery-{version}.js, modernizr-*, and
        //   bootstrap.js from local Scripts/ files installed by NuGet's legacy packages.config
        //   content-file mechanism, which PackageReference does not replicate (it only restores
        //   assembly references, not physically-copied content files). Rather than hand-vendoring
        //   those libraries into the project, jQuery and Bootstrap are now loaded via CDN
        //   directly in Views/Shared/_Layout.cshtml, and Modernizr was dropped entirely, it has
        //   little purpose in any browser still receiving updates. Only this project's own
        //   Content/Site.css is still bundled here, System.Web.Optimization genuinely still
        //   does something useful for that: minification and a single combined request. See
        //   LectureNotes.md.
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/site.css"));
        }
    }
}
