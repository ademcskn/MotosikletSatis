using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCeTicaret.Models;

namespace MVCeTicaret.Controllers
{
    public class ProductController : Controller
    {
        Context db;
        public ProductController()
        {
            db = new Context();
        }

        public ActionResult Product(int? id)
        {
            if (id != null)
            {
                return View(db.Products.Where(x => x.SubCategoryID == id).ToList());
            }
            else
            {
                return View(db.Products.ToList());
            }
        }

        public ActionResult ProductDetail(int id)
        {
            ViewData["Reviews"] = db.Reviews.Where(x => x.ProductID == id).ToList();
            return View(db.Products.Find(id));
        }

        public ActionResult AddReview(int id, FormCollection frm)
        {
            Review review = new Review()
            {
                Comment = frm["review"],
                CustomerID = TemporaryUserData.UserID,
                DateTime = DateTime.Now,
                IsDeleted = false,
                Name = frm["name"] == "" ? "Anonymous" : frm["name"],
                ProductID = id,
                Rate = int.Parse(frm["rate"])
            };
            db.Reviews.Add(review);
            db.SaveChanges();


            return RedirectToAction("ProductDetail", new { id = id });
        }
    }
}