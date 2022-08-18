using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace AuroraWeb
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web-API-Konfiguration und -Dienste

            // Web-API-Routen
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(name: "DefaultApi", routeTemplate: "api/{controller}/{action}");
            config.Routes.MapHttpRoute(name: "ActionApi", routeTemplate: "api/{controller}/{action}/{id}");
            config.Routes.MapHttpRoute(name: "ActionApiValues",routeTemplate: "api/{controller}/{action}/{id}/{v}");
        }
    }
}
