using System.Web.Mvc;
using Samples.MvcWebApi.Filters;

namespace Samples.MvcWebApi.Controllers
{
    public class HomeController : Controller
    {
        [LogFilter]
        [ExceptionFilter]
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
