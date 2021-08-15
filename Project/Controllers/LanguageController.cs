using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Project.Controllers
{
    public class LanguageController : Controller
    {
        // GET: Language
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Change(string language)
        {
            if(language != null)
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(language);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
                HttpCookie cookie = new HttpCookie("Language");
                cookie.Value = language;
                Response.Cookies.Add(cookie);
            }
            return RedirectToAction("Index", "Home", null);
        }
    }
}