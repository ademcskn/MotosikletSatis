using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCeTicaret.Models;

namespace MVCeTicaret.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            Context db = new Context();
            TempData["Yamaha"] = db.Products.Where(x => x.SubCategoryID == 2).OrderBy(x => Guid.NewGuid()).Take(3).ToList();
            TempData["Honda"] = db.Products.Where(x => x.SubCategoryID == 1).OrderBy(x => Guid.NewGuid()).Take(3).ToList();
            TempData["BMW"] = db.Products.Where(x => x.SubCategoryID == 3).OrderBy(x => Guid.NewGuid()).Take(3).ToList();


            return View();
        }

    }
}