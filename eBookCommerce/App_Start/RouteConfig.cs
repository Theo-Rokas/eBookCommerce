using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace eBookCommerce
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Account",
                url: "Account/{action}",
                defaults: new { controller = "Account", action = "Index" }
            );            

            routes.MapRoute(
                name: "Book",
                url: "Book/{action}/{id}",
                defaults: new { controller = "Book", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Basket",
                url: "Basket/{action}",
                defaults: new { controller = "Basket", action = "Index" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}
