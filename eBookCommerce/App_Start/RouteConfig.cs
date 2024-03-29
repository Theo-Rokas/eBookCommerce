﻿using System;
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
                name: "Books",
                url: "Books/{action}/{id}",
                defaults: new { controller = "Books", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Basket",
                url: "Basket/{action}",
                defaults: new { controller = "Basket", action = "Index" }
            );

            routes.MapRoute(
                name: "Api",
                url: "Api/{action}",
                defaults: new { controller = "Api", action = "Index" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}
