using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Project
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                "viewArticle",
                "Article/View/{id}/{title}",
                new {controller = "Article", action = "View", id= UrlParameter.Optional, title=UrlParameter.Optional}
                );
            routes.MapRoute(
                "viewBlog",
                "Blog/View/{id}/{title}",
                new {controller="Blog", action="View", id = UrlParameter.Optional, title = UrlParameter.Optional}
                );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
