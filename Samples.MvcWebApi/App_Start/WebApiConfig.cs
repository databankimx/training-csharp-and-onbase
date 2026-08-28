using System.Web.Http;
using Samples.MvcWebApi.HelperClasses;

namespace Samples.MvcWebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            // Modify the route to accept our arguments in REST format
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}/{data}",
                defaults: new
                {
                    id = RouteParameter.Optional,
                    data = RouteParameter.Optional
                }
            );

            config.Formatters.Add(new BrowserJsonFormatter());
        }
    }
}
